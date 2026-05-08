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
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IAIService _aiService;
        private readonly ICurrentUserService _currentUserService;

        public QuizController(
            ApplicationDbContext db,
            IAIService aiService,
            ICurrentUserService currentUserService)
        {
            _db = db;
            _aiService = aiService;
            _currentUserService = currentUserService;
        }

        [HttpPost("generate")]
        [Authorize(Policy = "RecruiterOrAbove")]
        public async Task<IActionResult> GenerateQuiz([FromBody] CreateQuizRequest request)
        {
            try
            {
                var jobOffer = await _db.JobOffers!
                    .FirstOrDefaultAsync(j => j.Id == request.JobOfferId);

                if (jobOffer == null)
                    return NotFound(new { message = "Offre d'emploi introuvable." });

                // Call AI Service to generate quiz content
                var aiResponse = await _aiService.GenerateQuizAsync(
                    jobOffer.Title, 
                    jobOffer.Description, 
                    request.NumQuestions, 
                    request.Language,
                    request.Difficulty,
                    request.Topics);

                // Create the Quiz entity
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

        [HttpGet("job-offer/{jobOfferId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuizByJobOffer(Guid jobOfferId)
        {
            var quiz = await _db.Quizzes!
                .IgnoreQueryFilters() // Candidates might not have company context
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
                    // Mask correct answers for candidates
                    CorrectAnswerIndex = -1, 
                    Explanation = null
                }).ToList()
            });
        }

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

                // Update questions (simplest way is to remove old and add new to avoid complex tracking)
                _db.QuizQuestions!.RemoveRange(quiz.Questions);
                
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

                // Calculate Score
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
                
                // Update application score cache for immediate access in dashboard
                application.QuizScore = (int)Math.Round(score);
                application.UpdatedAt = DateTime.UtcNow;
                
                await _db.SaveChangesAsync();

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

        [HttpGet("results/{applicationId}")]
        [Authorize(Policy = "RecruiterOrAbove")]
        public async Task<IActionResult> GetQuizResults(Guid applicationId)
        {
            var result = await _db.CandidateQuizResults!
                .Include(r => r.Quiz)
                .ThenInclude(q => q.Questions)
                .Where(r => r.JobApplicationId == applicationId)
                .OrderByDescending(r => r.CompletedAt)
                .FirstOrDefaultAsync();

            if (result == null) return NotFound();

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
