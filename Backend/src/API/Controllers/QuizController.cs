#pragma warning disable CS8601, CS8602, CS8604, CS8625
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs.AI;
using Application.DTOs.Recruiter;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Contrôleur gérant la création, la mise à jour, la soumission et les résultats des quiz d'évaluation.
    /// Fournit des fonctionnalités de génération automatique de quiz par l'IA.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IAIService _aiService;
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="QuizController"/>.
        /// </summary>
        /// <param name="db">Contexte de la base de données.</param>
        /// <param name="aiService">Service d'intégration de l'IA.</param>
        /// <param name="currentUserService">Service de gestion de l'utilisateur connecté.</param>
        public QuizController(
            ApplicationDbContext db,
            IAIService aiService,
            ICurrentUserService currentUserService)
        {
            _db = db;
            _aiService = aiService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Génère un nouveau quiz basé sur les détails d'une offre d'emploi en utilisant le service IA.
        /// Uniquement accessible aux recruteurs et rôles supérieurs.
        /// </summary>
        /// <param name="request">La requête contenant les paramètres de génération du quiz (ID offre, thèmes, difficulté, nombre de questions, langue).</param>
        /// <returns>Le DTO du quiz créé contenant la liste des questions générées.</returns>
        [HttpPost("generate")]
        [Authorize(Policy = "RecruiterOrAbove")]
        public async Task<IActionResult> GenerateQuiz([FromBody] CreateQuizRequest request)
        {
            try
            {
                // Recherche de l'offre d'emploi correspondante pour fournir le contexte à l'IA
                var jobOffer = await _db.JobOffers!
                    .FirstOrDefaultAsync(j => j.Id == request.JobOfferId);

                if (jobOffer == null)
                    return NotFound(new { message = "Offre d'emploi introuvable." });

                // Appel au service d'IA pour générer les questions adaptées
                var aiResponse = await _aiService.GenerateQuizAsync(
                    jobOffer.Title, 
                    jobOffer.Description, 
                    request.NumQuestions, 
                    request.Language,
                    request.Difficulty,
                    request.Topics);

                // Instanciation de l'entité Quiz avec les questions sérialisées
                var quiz = new Quiz
                {
                    Id = Guid.NewGuid(),
                    JobOfferId = request.JobOfferId,
                    Title = aiResponse.Title,
                    Description = aiResponse.Description,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Questions = aiResponse.Questions.Select(q => new QuizQuestion
                    {
                        Id = Guid.NewGuid(),
                        Text = q.Text,
                        Type = q.Type,
                        OptionsJson = JsonSerializer.Serialize(q.Options),
                        CorrectAnswerIndex = q.CorrectAnswerIndex,
                        Explanation = q.Explanation
                    }).ToList()
                };

                _db.Quizzes!.Add(quiz);
                await _db.SaveChangesAsync();

                // Retourne le DTO complet avec les réponses correctes
                return Ok(new QuizDto
                {
                    Id = quiz.Id,
                    JobOfferId = quiz.JobOfferId,
                    Title = quiz.Title,
                    Description = quiz.Description,
                    TimeLimitMinutes = quiz.TimeLimitMinutes,
                    Questions = quiz.Questions.Select(q => new QuizQuestionDto
                    {
                        Id = q.Id,
                        Text = q.Text,
                        Type = q.Type,
                        Options = JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? new List<string>(),
                        CorrectAnswerIndex = q.CorrectAnswerIndex,
                        Explanation = q.Explanation
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(new { message = errorMsg });
            }
        }

        /// <summary>
        /// Récupère le quiz actif associé à une offre d'emploi spécifique.
        /// Méthode publique anonyme pour permettre aux candidats de passer le test. Les bonnes réponses sont masquées.
        /// </summary>
        /// <param name="jobOfferId">L'identifiant de l'offre d'emploi.</param>
        /// <returns>Le quiz DTO avec les index de réponses correctes masqués (-1) et les explications retirées.</returns>
        [HttpGet("job-offer/{jobOfferId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuizByJobOffer(Guid jobOfferId)
        {
            // IgnoreQueryFilters est utilisé car les candidats anonymes n'ont pas de contexte de compagnie associé
            var quiz = await _db.Quizzes!
                .IgnoreQueryFilters() 
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.JobOfferId == jobOfferId && q.IsActive);

            if (quiz == null)
                return NotFound(new { message = "Aucun quiz actif trouvé pour cette offre." });

            return Ok(new QuizDto
            {
                Id = quiz.Id,
                JobOfferId = quiz.JobOfferId,
                Title = quiz.Title,
                Description = quiz.Description,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                Questions = quiz.Questions.Select(q => new QuizQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Type = q.Type,
                    Options = JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? new List<string>(),
                    // Masquer la bonne réponse pour éviter la triche côté client
                    CorrectAnswerIndex = -1, 
                    Explanation = null
                }).ToList()
            });
        }

        /// <summary>
        /// Met à jour un quiz existant en remplaçant la liste de ses questions.
        /// Uniquement accessible aux recruteurs et rôles supérieurs.
        /// </summary>
        /// <param name="id">Identifiant unique du quiz à modifier.</param>
        /// <param name="request">Les nouvelles données du quiz incluant la nouvelle liste de questions.</param>
        [HttpPut("{id}")]
        [Authorize(Policy = "RecruiterOrAbove")]
        public async Task<IActionResult> UpdateQuiz(Guid id, [FromBody] QuizDto request)
        {
            try
            {
                var quiz = await _db.Quizzes!
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quiz == null)
                    return NotFound(new { message = "Quiz introuvable." });

                quiz.Title = request.Title;
                quiz.Description = request.Description;
                quiz.TimeLimitMinutes = request.TimeLimitMinutes;

                // Suppression de l'ancienne liste de questions pour éviter le tracking complexe d'entités orphelines
                _db.QuizQuestions!.RemoveRange(quiz.Questions);
                
                // Assignation des nouvelles questions
                quiz.Questions = request.Questions.Select(q => new QuizQuestion
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    Text = q.Text,
                    Type = q.Type,
                    OptionsJson = JsonSerializer.Serialize(q.Options),
                    CorrectAnswerIndex = q.CorrectAnswerIndex,
                    Explanation = q.Explanation
                }).ToList();

                await _db.SaveChangesAsync();

                return Ok(new { message = "Quiz mis à jour avec succès." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Soumet les réponses d'un candidat à un quiz, calcule le score de réussite et enregistre les résultats.
        /// Permet un appel anonyme pour les candidats en cours de processus.
        /// </summary>
        /// <param name="request">La requête de soumission contenant les réponses choisies et les identifiants requis.</param>
        /// <returns>Le résultat du quiz incluant le score calculé et la correction détaillée question par question.</returns>
        [HttpPost("submit")]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizRequest request)
        {
            try
            {
                var quiz = await _db.Quizzes!
                    .IgnoreQueryFilters()
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == request.QuizId);

                if (quiz == null)
                    return NotFound(new { message = "Quiz introuvable." });

                var application = await _db.JobApplications!
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(a => a.Id == request.JobApplicationId);

                if (application == null)
                {
                    Console.WriteLine($"[QUIZ SUBMIT ERROR] JobApplicationId {request.JobApplicationId} not found in DB.");
                    return NotFound(new { message = $"Candidature avec l'ID {request.JobApplicationId} introuvable. Assurez-vous d'avoir bien postulé à l'offre avant de passer le test." });
                }

                // Calcul du score en comparant l'index soumis avec l'index de réponse correcte stocké
                int correctCount = 0;
                var questionsList = quiz.Questions.ToList();
                for (int i = 0; i < questionsList.Count; i++)
                {
                    if (i < request.Answers.Count && request.Answers[i] == questionsList[i].CorrectAnswerIndex)
                    {
                        correctCount++;
                    }
                }

                double score = (double)correctCount / questionsList.Count * 100;

                var result = new CandidateQuizResult
                {
                    Id = Guid.NewGuid(),
                    JobApplicationId = request.JobApplicationId,
                    QuizId = request.QuizId,
                    Score = Math.Round(score, 2),
                    CompletedAt = DateTime.UtcNow,
                    CandidateAnswersJson = JsonSerializer.Serialize(request.Answers)
                };

                _db.CandidateQuizResults!.Add(result);
                
                // Met en cache le score de quiz directement dans la candidature pour un accès rapide sur le tableau de bord
                application.QuizScore = (int)Math.Round(score);
                application.UpdatedAt = DateTime.UtcNow;
                
                await _db.SaveChangesAsync();

                // Retourne la correction détaillée au candidat
                return Ok(new QuizResultDto
                {
                    Score = result.Score,
                    CompletedAt = result.CompletedAt,
                    Reviews = questionsList.Select((q, i) => new QuestionReviewDto
                    {
                        QuestionIndex = i,
                        CorrectAnswerIndex = q.CorrectAnswerIndex,
                        CandidateAnswerIndex = i < request.Answers.Count ? request.Answers[i] : -1,
                        IsCorrect = i < request.Answers.Count && request.Answers[i] == q.CorrectAnswerIndex,
                        Explanation = q.Explanation
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Récupère le résultat détaillé du quiz pour une candidature spécifique.
        /// Uniquement accessible aux recruteurs et rôles supérieurs pour évaluer les compétences.
        /// </summary>
        /// <param name="applicationId">Identifiant unique de la candidature.</param>
        /// <returns>Les détails de correction du quiz avec les choix du candidat et les bonnes réponses.</returns>
        [HttpGet("results/{applicationId}")]
        [Authorize(Policy = "RecruiterOrAbove")]
        public async Task<IActionResult> GetQuizResults(Guid applicationId)
        {
            var appExists = await _db.JobApplications!.AnyAsync(a => a.Id == applicationId);
            if (!appExists) return NotFound(new { message = "Candidature introuvable." });

            var result = await _db.CandidateQuizResults!
                .Include(r => r.Quiz)
                .ThenInclude(q => q.Questions)
                .Where(r => r.JobApplicationId == applicationId)
                .OrderByDescending(r => r.CompletedAt)
                .FirstOrDefaultAsync();

            if (result == null) return Ok(null);

            var answers = JsonSerializer.Deserialize<List<int>>(result.CandidateAnswersJson) ?? new List<int>();
            var questionsList = result.Quiz.Questions.ToList();

            return Ok(new QuizResultDto
            {
                Score = result.Score,
                CompletedAt = result.CompletedAt,
                Reviews = questionsList.Select((q, i) => new QuestionReviewDto
                {
                    QuestionIndex = i,
                    Text = q.Text,
                    Options = JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? new List<string>(),
                    CorrectAnswerIndex = q.CorrectAnswerIndex,
                    CandidateAnswerIndex = i < answers.Count ? answers[i] : -1,
                    IsCorrect = i < answers.Count && answers[i] == q.CorrectAnswerIndex,
                    Explanation = q.Explanation
                }).ToList()
            });
        }
    }
}
