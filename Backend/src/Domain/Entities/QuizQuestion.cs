using System;

namespace Domain.Entities
{
    /// <summary>
    /// Entité QuizQuestion - Représente une question individuelle faisant partie d'un quiz.
    /// Contient le texte de la question, son type (Technique ou SoftSkill), ses options (JSON) et la réponse correcte.
    /// </summary>
    public class QuizQuestion
    {
        /// <summary>
        /// Identifiant unique de la question.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Identifiant unique du quiz parent.
        /// </summary>
        public Guid QuizId { get; set; }

        /// <summary>
        /// Propriété de navigation vers le quiz associé.
        /// </summary>
        public virtual Quiz Quiz { get; set; }

        /// <summary>
        /// Libellé ou texte de la question.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Type de question : "Technical" (technique) ou "SoftSkill" (comportemental).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Liste des options de réponse sous forme de tableau JSON (ex: ["Option A", "Option B", ...]).
        /// </summary>
        public string OptionsJson { get; set; } 
        
        /// <summary>
        /// Index de la réponse correcte dans le tableau d'options (0-indexed).
        /// </summary>
        public int CorrectAnswerIndex { get; set; }

        /// <summary>
        /// Explication facultative justifiant la réponse correcte.
        /// </summary>
        public string Explanation { get; set; }
    }
}
