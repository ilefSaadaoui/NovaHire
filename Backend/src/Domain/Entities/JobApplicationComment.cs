using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Entité JobApplicationComment - Représente un commentaire ou une note de discussion déposée par un recruteur sur une candidature spécifique.
    /// Supporte une structure de discussion simple (threading) via ParentCommentId.
    /// </summary>
    public class JobApplicationComment
    {
        /// <summary>
        /// Identifiant unique du commentaire.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Identifiant unique de la candidature associée.
        /// </summary>
        [Required]
        public Guid JobApplicationId { get; set; }

        /// <summary>
        /// Propriété de navigation vers la candidature associée.
        /// </summary>
        [ForeignKey("JobApplicationId")]
        public virtual JobApplication? JobApplication { get; set; }

        /// <summary>
        /// Identifiant unique du recruteur auteur du commentaire.
        /// </summary>
        [Required]
        public Guid AuthorId { get; set; }

        /// <summary>
        /// Propriété de navigation vers l'auteur (utilisateur) associé.
        /// </summary>
        [ForeignKey("AuthorId")]
        public virtual User? Author { get; set; }

        /// <summary>
        /// Contenu textuel brut du commentaire.
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Date et heure de publication du commentaire.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Identifiant du commentaire parent en cas de fil de discussion/réponse (facultatif).
        /// </summary>
        public Guid? ParentCommentId { get; set; }

        /// <summary>
        /// Date et heure de la dernière modification du commentaire.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
