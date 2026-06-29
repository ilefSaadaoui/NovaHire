using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Entité Department - Représente un service ou département interne de l'entreprise (ex: "Ressources Humaines", "R&D").
    /// Utilisé pour segmenter les recruteurs et les offres d'emploi.
    /// </summary>
    public class Department
    {
        /// <summary>
        /// Identifiant unique du département.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Nom du département (ex: "Engineering").
        /// </summary>
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        /// <summary>
        /// Description du département et de son périmètre d'action.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Identifiant unique de l'entreprise (tenant) propriétaire du département.
        /// </summary>
        public Guid CompanyId { get; set; }

        /// <summary>
        /// Propriété de navigation vers l'entreprise associée.
        /// </summary>
        public virtual Company? Company { get; set; }

        /// <summary>
        /// Date et heure de création de l'enregistrement.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date et heure de la dernière mise à jour.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Liste des utilisateurs (recruteurs/collaborateurs) rattachés à ce département.
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
