using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Application.DTOs.AI;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace AI
{
    /// <summary>
    /// AI Analysis Service client.
    /// Supports two modes:
    ///   1. Synchronous: POST /analyze (backward compatible, blocks until done)
    ///   2. Async Job:   POST /jobs → poll GET /jobs/{id} (non-blocking, high throughput)
    ///
    /// The async mode is used by default. Falls back to sync if async submission fails.
    /// </summary>
    public class AIAnalysisService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIAnalysisService> _logger;
        private readonly IStorageService _storageService;
        private readonly string _aiServiceUrl;
        private readonly bool _useAsyncMode;
        private readonly int _pollIntervalMs;
        private readonly int _maxPollAttempts;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public AIAnalysisService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AIAnalysisService> logger,
            IStorageService storageService)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(10); // Très long timeout pour Ollama sur CPU/Matériel lent
            _logger = logger;
            _storageService = storageService;
            _aiServiceUrl = configuration["AIService:Url"] ?? "http://localhost:8000";
            _useAsyncMode = configuration.GetValue("AIService:AsyncMode", true);
            _pollIntervalMs = configuration.GetValue("AIService:PollIntervalMs", 2000);
            _maxPollAttempts = configuration.GetValue("AIService:MaxPollAttempts", 150);
        }

        public async Task<AIAnalysisResponseDto> AnalyzeCVAsync(
            string filePath,
            string jobTitle,
            string jobDescription,
            int weightExp,
            int weightEdu,
            int weightSkills,
            string language = "fr")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Le chemin du CV est manquant ou invalide.");
            }

            byte[] fileBytes;
            string fileName;

            if (filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var signedUrl = _storageService.GetDownloadUrl(filePath);
                var downloadUrls = new List<string>();
                if (!string.IsNullOrWhiteSpace(signedUrl))
                    downloadUrls.Add(signedUrl);
                if (!string.Equals(filePath, signedUrl, StringComparison.OrdinalIgnoreCase))
                    downloadUrls.Add(filePath);

                Exception? lastDownloadError = null;
                fileBytes = Array.Empty<byte>();
                fileName = "resume.pdf";

                foreach (var downloadUrl in downloadUrls)
                {
                    Console.WriteLine($"[AIAnalysisService] Downloading CV from: {downloadUrl}");
                    try
                    {
                        // Use a fresh HttpClient to avoid any base-address / auth-header conflicts
                        using var downloadClient = new HttpClient();
                        downloadClient.Timeout = TimeSpan.FromSeconds(60);

                        var downloadResponse = await downloadClient.GetAsync(downloadUrl);

                        if (!downloadResponse.IsSuccessStatusCode)
                        {
                            var errorBody = await downloadResponse.Content.ReadAsStringAsync();
                            throw new Exception(
                                $"HTTP {(int)downloadResponse.StatusCode} lors du téléchargement du CV depuis Cloudinary. Body: {errorBody}");
                        }

                        fileBytes = await downloadResponse.Content.ReadAsByteArrayAsync();

                        // Extract filename from URL; fall back to "resume.pdf"
                        try
                        {
                            var rawPath = new Uri(downloadUrl).AbsolutePath;
                            fileName = Path.GetFileName(rawPath);
                        }
                        catch
                        {
                            fileName = "resume.pdf";
                        }

                        // Ensure a valid extension (.pdf or .docx)
                        var ext = Path.GetExtension(fileName).ToLowerInvariant();
                        if (string.IsNullOrEmpty(ext) || !new[] { ".pdf", ".docx", ".doc" }.Contains(ext))
                            fileName = Path.GetFileNameWithoutExtension(fileName) + ".pdf";

                        Console.WriteLine($"[AIAnalysisService] Downloaded {fileBytes.Length} bytes. Filename used: {fileName}");
                        lastDownloadError = null;
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastDownloadError = ex;
                        Console.WriteLine($"[AIAnalysisService] ERROR downloading CV from URL: {ex.Message}");
                    }
                }

                if (lastDownloadError != null || fileBytes.Length == 0)
                {
                    var errorMessage = lastDownloadError?.Message ?? "fichier vide";
                    throw new Exception($"Impossible de récupérer le CV: {errorMessage}");
                }
            }
            else
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[AIAnalysisService] ERROR: Local file not found: {filePath}");
                    throw new FileNotFoundException("CV file not found", filePath);
                }

                fileBytes = await File.ReadAllBytesAsync(filePath);
                fileName = Path.GetFileName(filePath);
                var ext2 = Path.GetExtension(fileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext2) || !new[] { ".pdf", ".docx", ".doc" }.Contains(ext2))
                    fileName = Path.GetFileNameWithoutExtension(fileName) + ".pdf";
            }

            try
            {
                return await AnalyzeInTwoStagesAsync(
                    fileBytes, fileName, jobTitle, jobDescription,
                    weightExp, weightEdu, weightSkills, language);
            }
            catch (BrokenCircuitException)
            {
                _logger.LogWarning("AI Service circuit is broken. Returning degraded response.");
                return new AIAnalysisResponseDto
                {
                    OverallScore = 0,
                    AutoGeneratedSummary = "L'analyse IA est temporairement indisponible (Circuit ouvert).",
                    AIRecommendation = "Veuillez r\u00e9essayer dans quelques minutes.",
                    ExtractedData = new ExtractedDataDto()
                };
            }
        }

        private async Task<AIAnalysisResponseDto> AnalyzeInTwoStagesAsync(
            byte[] fileBytes, string fileName, string jobTitle, string jobDescription,
            int weightExp, int weightEdu, int weightSkills, string language)
        {
            // Stage 1: extract text/data from PDF only
            using var extractForm = BuildFormContent(
                fileBytes, fileName, jobTitle, jobDescription,
                weightExp, weightEdu, weightSkills, "extract_only", language);

            var extractResponse = await _httpClient.PostAsync($"{_aiServiceUrl}/analyze", extractForm);
            if (!extractResponse.IsSuccessStatusCode)
            {
                var error = await extractResponse.Content.ReadAsStringAsync();
                throw new Exception($"Extraction IA échouée: {extractResponse.StatusCode} - {error}");
            }

            var extractJson = await extractResponse.Content.ReadAsStringAsync();
            var extractResult = JsonSerializer.Deserialize<AIAnalysisResponseDto>(extractJson, JsonOptions)
                ?? throw new Exception("Réponse d'extraction IA invalide.");

            if (string.IsNullOrWhiteSpace(extractResult.RawCvText))
                throw new Exception("Le texte CV extrait est vide.");

            // Stage 2: score + summary from extracted text
            var scorePayload = new
            {
                cvText = extractResult.RawCvText,
                jobTitle = jobTitle,
                jobDescription = jobDescription,
                weightExperience = weightExp,
                weightEducation = weightEdu,
                weightSkills = weightSkills,
                language = language
            };

            var scoreBody = new StringContent(
                JsonSerializer.Serialize(scorePayload),
                Encoding.UTF8,
                "application/json");

            var scoreResponse = await _httpClient.PostAsync($"{_aiServiceUrl}/score-from-text", scoreBody);
            if (!scoreResponse.IsSuccessStatusCode)
            {
                var error = await scoreResponse.Content.ReadAsStringAsync();
                throw new Exception($"Scoring IA échoué: {scoreResponse.StatusCode} - {error}");
            }

            var scoreJson = await scoreResponse.Content.ReadAsStringAsync();
            var scoreResult = JsonSerializer.Deserialize<AIAnalysisResponseDto>(scoreJson, JsonOptions)
                ?? throw new Exception("Réponse de scoring IA invalide.");

            // Keep extraction details if score response omitted any extracted fields.
            if (scoreResult.ExtractedData == null ||
                (scoreResult.ExtractedData.WorkExperiences.Count == 0 &&
                 scoreResult.ExtractedData.Educations.Count == 0 &&
                 scoreResult.ExtractedData.Skills.Count == 0 &&
                 scoreResult.ExtractedData.Languages.Count == 0 &&
                 scoreResult.ExtractedData.Certifications.Count == 0))
            {
                scoreResult.ExtractedData = extractResult.ExtractedData;
            }

            return scoreResult;
        }

        public async Task<AIQuizResponseDto> GenerateQuizAsync(string jobTitle, string jobDescription, int numQuestions = 10, string language = "fr", string difficulty = "Mid", List<string>? topics = null)
        {
            try
            {
                var payload = new
                {
                    jobTitle = string.IsNullOrWhiteSpace(jobTitle) ? "Poste g\u00e9n\u00e9ral" : jobTitle,
                    jobDescription = string.IsNullOrWhiteSpace(jobDescription) ? "Comp\u00e9tences professionnelles g\u00e9n\u00e9rales" : jobDescription,
                    numQuestions = numQuestions,
                    language = language,
                    difficulty = difficulty,
                    topics = topics ?? new List<string>()
                };

                var body = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                _logger.LogInformation("Requesting quiz generation from AI service for job: {JobTitle}", jobTitle);

                var response = await _httpClient.PostAsync($"{_aiServiceUrl}/generate-quiz", body);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Quiz generation failed: {Error}", error);
                    throw new Exception($"Erreur lors de la génération du quiz: {response.StatusCode} - {error}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AIQuizResponseDto>(json, JsonOptions)
                    ?? throw new Exception("Réponse de génération de quiz IA invalide.");

                return result;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogWarning("AI Service circuit is broken during quiz generation.");
                return new AIQuizResponseDto
                {
                    Title = "Quiz Indisponible",
                    Description = "Le service de g\u00e9n\u00e9ration de quiz est temporairement hors ligne.",
                    Questions = new List<AIQuizQuestionDto>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate quiz via AI service");
                throw;
            }
        }

        public async Task<string> GenerateJobDescriptionAsync(string jobTitle, string? keywords = null, string language = "fr")
        {
            try
            {
                var payload = new
                {
                    jobTitle = jobTitle,
                    keywords = keywords,
                    language = language
                };

                var body = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                _logger.LogInformation("Requesting JD generation from AI service for job: {JobTitle}", jobTitle);

                var response = await _httpClient.PostAsync($"{_aiServiceUrl}/generate-jd", body);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("JD generation failed: {Error}", error);
                    throw new Exception($"Erreur lors de la génération de la description: {response.StatusCode} - {error}");
                }

                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                return document.RootElement.GetProperty("description").GetString() ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate JD via AI service");
                throw;
            }
        }

        public async Task<string> GenerateRejectionFeedbackAsync(string candidateName, string jobTitle, string recruiterNotes, string language = "fr")
        {
            try
            {
                var payload = new
                {
                    candidateName = candidateName,
                    jobTitle = jobTitle,
                    recruiterNotes = recruiterNotes,
                    language = language
                };

                var body = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                _logger.LogInformation("Requesting rejection feedback from AI service for candidate: {CandidateName}", candidateName);

                var response = await _httpClient.PostAsync($"{_aiServiceUrl}/generate-feedback", body);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Feedback generation failed: {Error}", error);
                    return $"Bonjour {candidateName}, nous vous remercions pour votre intérêt mais nous ne pouvons pas retenir votre candidature.";
                }

                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                return document.RootElement.GetProperty("feedback").GetString() ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate rejection feedback via AI service");
                return $"Bonjour {candidateName}, nous vous remercions pour votre intérêt mais nous ne pouvons pas retenir votre candidature.";
            }
        }

        public async Task<string> GenerateOfferLetterAsync(string candidateName, string jobTitle, decimal salary, string currency, string startDate, string benefits, string recruiterName, string companyName, string language = "fr")
        {
            try
            {
                var payload = new
                {
                    candidateName = candidateName,
                    jobTitle = jobTitle,
                    salary = (float)salary,
                    currency = currency,
                    startDate = startDate,
                    benefits = benefits,
                    recruiterName = recruiterName,
                    companyName = companyName,
                    language = language
                };

                var body = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_aiServiceUrl}/generate-offer-letter", body);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Offer letter generation failed: {Error}", await response.Content.ReadAsStringAsync());
                    return "";
                }

                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                return document.RootElement.GetProperty("offerLetter").GetString() ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate offer letter via AI service");
                return "";
            }
        }

        public async Task<ExtractedDataDto> ExtractCVAsync(string filePath)
        {
            byte[] fileBytes;
            string fileName;

            if (filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var signedUrl = _storageService.GetDownloadUrl(filePath);
                using var downloadClient = new HttpClient();
                downloadClient.Timeout = TimeSpan.FromSeconds(60);
                var dlResponse = await downloadClient.GetAsync(signedUrl ?? filePath);
                if (!dlResponse.IsSuccessStatusCode)
                    throw new Exception("Impossible de télécharger le CV.");
                fileBytes = await dlResponse.Content.ReadAsByteArrayAsync();
                try { fileName = Path.GetFileName(new Uri(filePath).AbsolutePath); } catch { fileName = "resume.pdf"; }
            }
            else
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("CV file not found", filePath);
                fileBytes = await File.ReadAllBytesAsync(filePath);
                fileName = Path.GetFileName(filePath);
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !new[] { ".pdf", ".docx", ".doc" }.Contains(ext))
                fileName = Path.GetFileNameWithoutExtension(fileName) + ".pdf";

            // Use extract_only mode — no scoring, just parsing
            using var extractForm = BuildFormContent(
                fileBytes, fileName, "N/A", "N/A",
                33, 33, 34, "extract_only", "fr");

            var extractResponse = await _httpClient.PostAsync($"{_aiServiceUrl}/analyze", extractForm);
            if (!extractResponse.IsSuccessStatusCode)
            {
                var error = await extractResponse.Content.ReadAsStringAsync();
                throw new Exception($"Extraction IA échouée: {extractResponse.StatusCode} - {error}");
            }

            var extractJson = await extractResponse.Content.ReadAsStringAsync();
            var extractResult = JsonSerializer.Deserialize<AIAnalysisResponseDto>(extractJson, JsonOptions)
                ?? throw new Exception("Réponse d'extraction IA invalide.");

            return extractResult.ExtractedData ?? new ExtractedDataDto();
        }

        /// <summary>
        /// Synchronous mode: POST /analyze and wait for response.
        /// Backward compatible with v1.
        /// </summary>
        private async Task<AIAnalysisResponseDto> AnalyzeSynchronousAsync(
            byte[] fileBytes, string fileName, string jobTitle, string jobDescription,
            int weightExp, int weightEdu, int weightSkills, string language)
        {
            try
            {
                using var form = BuildFormContent(fileBytes, fileName, jobTitle, jobDescription,
                    weightExp, weightEdu, weightSkills, "full_analysis", language);

                _logger.LogInformation("Sending CV to AI service (sync) at {Url}/analyze",
                    _aiServiceUrl);

                var response = await _httpClient.PostAsync(
                    $"{_aiServiceUrl}/analyze", form);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AI service error: {Error}", error);
                    throw new Exception(
                        $"AI service error: {response.StatusCode} - {error}");
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AIAnalysisResponseDto>(json, JsonOptions)
                    ?? throw new Exception("Failed to deserialize AI response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Synchronous analysis failed");
                throw;
            }
        }

        /// <summary>
        /// Async mode: POST /jobs → poll GET /jobs/{id} until completed.
        /// Non-blocking for the AI service — supports high concurrency.
        /// </summary>
        private async Task<AIAnalysisResponseDto> AnalyzeViaJobQueueAsync(
            byte[] fileBytes, string fileName, string jobTitle, string jobDescription,
            int weightExp, int weightEdu, int weightSkills, string language)
        {
            // Step 1: Submit job
            string jobId;
            using (var form = BuildFormContent(fileBytes, fileName, jobTitle, jobDescription,
                weightExp, weightEdu, weightSkills, "full_analysis", language))
            {
                _logger.LogInformation("Submitting CV to AI job queue at {Url}/jobs",
                    _aiServiceUrl);

                var submitResponse = await _httpClient.PostAsync(
                    $"{_aiServiceUrl}/jobs", form);

                if (!submitResponse.IsSuccessStatusCode)
                {
                    var error = await submitResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Job submission failed: {error}");
                }

                var submitJson = await submitResponse.Content.ReadAsStringAsync();
                var submitResult = JsonSerializer.Deserialize<JobSubmitResponse>(
                    submitJson, JsonOptions);
                jobId = submitResult?.JobId
                    ?? throw new Exception("No job_id in submission response");

                _logger.LogInformation("Job submitted: {JobId}", jobId);
            }

            // Step 2: Poll until completed or failed
            for (int attempt = 0; attempt < _maxPollAttempts; attempt++)
            {
                await Task.Delay(_pollIntervalMs);

                var pollResponse = await _httpClient.GetAsync(
                    $"{_aiServiceUrl}/jobs/{jobId}");

                if (!pollResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Poll attempt {Attempt} failed for job {JobId}",
                        attempt, jobId);
                    continue;
                }

                var pollJson = await pollResponse.Content.ReadAsStringAsync();
                var jobStatus = JsonSerializer.Deserialize<JobPollResponse>(
                    pollJson, JsonOptions);

                if (jobStatus == null) continue;

                switch (jobStatus.Status)
                {
                    case "completed":
                        _logger.LogInformation(
                            "Job {JobId} completed (score={Score})",
                            jobId, jobStatus.Result?.OverallScore);
                        return jobStatus.Result
                            ?? throw new Exception("Job completed but result is null");

                    case "failed":
                        throw new Exception(
                            $"AI analysis job failed: {jobStatus.Error}");

                    case "queued":
                    case "processing":
                    case "retrying":
                        _logger.LogDebug(
                            "Job {JobId} status: {Status} (attempt {Attempt})",
                            jobId, jobStatus.Status, attempt);
                        break;
                }
            }

            throw new TimeoutException(
                $"AI analysis job {jobId} did not complete within " +
                $"{_maxPollAttempts * _pollIntervalMs / 1000}s");
        }

        private MultipartFormDataContent BuildFormContent(
            byte[] fileBytes, string fileName, string jobTitle, string jobDescription,
            int weightExp, int weightEdu, int weightSkills, string mode = "full_analysis", string language = "fr")
        {
            var form = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(fileBytes);
            // Set content-type based on file extension
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            fileContent.Headers.ContentType = ext switch
            {
                ".docx" or ".doc" => MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.wordprocessingml.document"),
                _ => MediaTypeHeaderValue.Parse("application/pdf"),
            };
            form.Add(fileContent, "file", fileName);

            form.Add(new StringContent(jobTitle), "jobTitle");
            form.Add(new StringContent(jobDescription), "jobDescription");
            form.Add(new StringContent(weightExp.ToString()), "weightExperience");
            form.Add(new StringContent(weightEdu.ToString()), "weightEducation");
            form.Add(new StringContent(weightSkills.ToString()), "weightSkills");
            form.Add(new StringContent(mode), "mode");
            form.Add(new StringContent(language), "language");

            return form;
        }

        // ── Internal DTOs for job queue communication ──

        private class JobSubmitResponse
        {
            [JsonPropertyName("job_id")]
            public string? JobId { get; set; }

            [JsonPropertyName("status")]
            public string? Status { get; set; }
        }

        private class JobPollResponse
        {
            [JsonPropertyName("job_id")]
            public string? JobId { get; set; }

            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("error")]
            public string? Error { get; set; }

            [JsonPropertyName("result")]
            public AIAnalysisResponseDto? Result { get; set; }

            [JsonPropertyName("stage_timings")]
            public Dictionary<string, double>? StageTimings { get; set; }
        }

        private class ScoreFromTextRequest
        {
            [JsonPropertyName("cvText")]
            public string CvText { get; set; } = string.Empty;

            [JsonPropertyName("jobTitle")]
            public string JobTitle { get; set; } = string.Empty;

            [JsonPropertyName("jobDescription")]
            public string JobDescription { get; set; } = string.Empty;

            [JsonPropertyName("weightExperience")]
            public int WeightExperience { get; set; }

            [JsonPropertyName("weightEducation")]
            public int WeightEducation { get; set; }

            [JsonPropertyName("weightSkills")]
            public int WeightSkills { get; set; }
        }
    }
}
