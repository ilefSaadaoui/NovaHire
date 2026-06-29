using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Entité JobOffer - Représente une offre d'emploi ou de stage publiée par une entreprise.
    /// Contient les critères de l'offre, la configuration du formulaire de candidature, les pondérations de score d'évaluation par l'IA et les informations d'isolation multi-tenant.
    /// </summary>
    public class JobOffer
    {
        /// <summary>
        /// Identifiant unique de l'offre d'emploi.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Titre de l'offre d'emploi (ex: "Développeur Full-Stack C#").
        /// </summary>
        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        /// <summary>
        /// Description détaillée des missions, responsabilités et conditions du poste.
        /// </summary>
        [Required]
        public required string Description { get; set; }

        /// <summary>
        /// Type de contrat de l'offre (FullTime, PartTime, Internship, etc.).
        /// </summary>
        public JobType Type { get; set; } = JobType.FullTime;

        /// <summary>
        /// Lieu de travail (ex: "Paris, France").
        /// </summary>
        [MaxLength(100)]
        public string? Location { get; set; }

        /// <summary>
        /// Nom du département associé à l'offre.
        /// </summary>
        public string? Department { get; set; }

        /// <summary>
        /// Fourchette de rémunération proposée (ex: "45k - 55k EUR").
        /// </summary>
        public string? SalaryRange { get; set; }

        /// <summary>
        /// Politique de télétravail applicable (OnSite, Remote, Hybrid).
        /// </summary>
        public RemotePolicy? RemotePolicy { get; set; }

        /// <summary>
        /// Niveau d'expérience minimal requis pour postuler (Junior, Intermediate, Senior, Expert, etc.).
        /// </summary>
        public ExperienceLevel? ExperienceLevel { get; set; }

        /// <summary>
        /// Liste des compétences ou technologies requises.
        /// </summary>
        public List<string>? Skills { get; set; } = new();

        /// <summary>
        /// Date limite de dépôt des candidatures.
        /// </summary>
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// Identifiant unique de l'entreprise (tenant) publiant l'offre.
        /// </summary>
        [Required]
        public Guid CompanyId { get; set; }

        /// <summary>
        /// Propriété de navigation vers l'entreprise associée.
        /// </summary>
        public virtual Company? Company { get; set; }

        /// <summary>
        /// Identifiant unique de l'utilisateur qui a créé l'offre.
        /// </summary>
        public Guid CreatedById { get; set; }

        /// <summary>
        /// Propriété de navigation vers le créateur de l'offre.
        /// </summary>
        public virtual User? CreatedBy { get; set; }

        /// <summary>
        /// Configuration personnalisée des champs requis dans le formulaire de candidature public.
        /// </summary>
        public ApplicationFormConfig FormConfig { get; set; } = new ApplicationFormConfig();

        /// <summary>
        /// Poids accordé à l'expérience professionnelle dans le calcul du score global IA (de 0 à 100).
        /// </summary>
        public int WeightExperience { get; set; } = 40; // Par défaut 40%

        /// <summary>
        /// Poids accordé au niveau d'études (Education) dans le calcul du score global IA (de 0 à 100).
        /// </summary>
        public int WeightEducation { get; set; } = 30;  // Par défaut 30%

        /// <summary>
        /// Poids accordé aux compétences clés dans le calcul du score global IA (de 0 à 100).
        /// </summary>
        public int WeightSkills { get; set; } = 30;     // Par défaut 30%

        /// <summary>
        /// Seuil de rejet automatique (Auto-rejection) basé sur le score de matching IA (0 = désactivé).
        /// Les candidats obtenant un score strictement inférieur à ce seuil seront directement exclus.
        /// </summary>
        public int AutoRejectThreshold { get; set; } = 0;

        /// <summary>
        /// Configuration de l'affichage public de la page de l'offre d'emploi (branding, CSS personnalisé, etc.).
        /// </summary>
        public PublicDisplayConfig DisplayConfig { get; set; } = new PublicDisplayConfig();

        /// <summary>
        /// URL publique générée de l'offre d'emploi.
        /// </summary>
        [MaxLength(500)]
        public string? PublicUrl { get; set; }

        /// <summary>
        /// Jeton unique d'accès public utilisé pour partager ou postuler de manière anonyme à l'offre.
        /// </summary>
        [MaxLength(100)]
        public string? ShareToken { get; set; }

        /// <summary>
        /// Statut actuel de l'offre d'emploi (Draft, Published, Closed, Archived).
        /// </summary>
        public JobOfferStatus Status { get; set; } = JobOfferStatus.Draft;

        /// <summary>
        /// Date et heure de création de l'enregistrement de l'offre.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date et heure de la dernière mise à jour de l'offre.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Date et heure de publication effective de l'offre.
        /// </summary>
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// Date et heure de clôture/expiration planifiée de l'offre.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Collection de toutes les candidatures soumises pour cette offre.
        /// </summary>
        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();

        /// <summary>
        /// Indique si l'offre d'emploi est actuellement active (publiée et non expirée).
        /// </summary>
        /// <returns>True si l'offre est active, sinon False.</returns>
        public bool IsActive()
        {
            return Status == JobOfferStatus.Published
                && (!Deadline.HasValue || Deadline.Value > DateTime.UtcNow);
        }

        /// <summary>
        /// Publie officiellement l'offre et enregistre le timestamp de publication.
        /// </summary>
        public void Publish()
        {
            Status = JobOfferStatus.Published;
            PublishedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Clôture l'offre d'emploi, empêchant de nouvelles candidatures.
        /// </summary>
        public void Close()
        {
            Status = JobOfferStatus.Closed;
        }
    }

    /// <summary>
    /// Représente les différents types de postes ou contrats possibles.
    /// </summary>
    public enum JobType
    {
        /// <summary>
        /// Contrat à plein temps (CDI, etc.).
        /// </summary>
        FullTime = 0,

        /// <summary>
        /// Contrat à temps partiel.
        /// </summary>
        PartTime = 1,

        /// <summary>
        /// Contrat temporaire ou CDD.
        /// </summary>
        Contract = 2,

        /// <summary>
        /// Stage.
        /// </summary>
        Internship = 3,

        /// <summary>
        /// Activité en freelance/indépendant.
        /// </summary>
        Freelance = 4
    }

    /// <summary>
    /// Représente les statuts du cycle de vie d'une offre d'emploi.
    /// </summary>
    public enum JobOfferStatus
    {
        /// <summary>
        /// L'offre est en brouillon et non visible du public.
        /// </summary>
        Draft = 0,

        /// <summary>
        /// L'offre est publiée et ouverte aux candidatures.
        /// </summary>
        Published = 1,

        /// <summary>
        /// L'offre est fermée mais reste lisible.
        /// </summary>
        Closed = 2,

        /// <summary>
        /// L'offre est archivée (supprimée logiquement).
        /// </summary>
        Archived = 3
    }

    /// <summary>
    /// Représente les politiques de présence au travail de l'offre.
    /// </summary>
    public enum RemotePolicy
    {
        /// <summary>
        /// Travail exclusivement sur site.
        /// </summary>
        OnSite = 0,

        /// <summary>
        /// Télétravail complet.
        /// </summary>
        Remote = 1,

        /// <summary>
        /// Mode de travail hybride (mix site et télétravail).
        /// </summary>
        Hybrid = 2
    }

    /// <summary>
    /// Représente les niveaux d'expérience requis pour le poste.
    /// </summary>
    public enum ExperienceLevel
    {
        /// <summary>
        /// Débutant / Junior (0-2 ans).
        /// </summary>
        Junior = 0,

        /// <summary>
        /// Niveau intermédiaire (2-5 ans).
        /// </summary>
        Intermediate = 1,

        /// <summary>
        /// Expérimenté / Senior (5 ans et +).
        /// </summary>
        Senior = 2,

        /// <summary>
        /// Expert ou poste de direction.
        /// </summary>
        Expert = 3,

        /// <summary>
        /// Jeune diplômé.
        /// </summary>
        Graduate = 4,

        /// <summary>
        /// Tout niveau accepté.
        /// </summary>
        All = 5,

        /// <summary>
        /// Novice sans expérience spécifique requise.
        /// </summary>
        Beginner = 6
    }

    /// <summary>
    /// Configuration personnalisée du formulaire public pour recueillir les données candidat.
    /// </summary>
    public class ApplicationFormConfig
    {
        /// <summary>
        /// Indique si le nom complet est requis.
        /// </summary>
        public bool RequireFullName { get; set; } = true;

        /// <summary>
        /// Indique si l'e-mail est requis.
        /// </summary>
        public bool RequireEmail { get; set; } = true;

        /// <summary>
        /// Indique si le numéro de téléphone est requis.
        /// </summary>
        public bool RequirePhone { get; set; } = false;

        /// <summary>
        /// Indique si le dépôt de CV est obligatoire.
        /// </summary>
        public bool RequireCV { get; set; } = true;

        /// <summary>
        /// Indique si une lettre de motivation est requise.
        /// </summary>
        public bool RequireCoverLetter { get; set; } = false;

        /// <summary>
        /// Indique si un lien de portfolio est requis.
        /// </summary>
        public bool RequirePortfolio { get; set; } = false;

        /// <summary>
        /// Indique si le lien du profil LinkedIn est requis.
        /// </summary>
        public bool RequireLinkedIn { get; set; } = false;

        /// <summary>
        /// Liste de libellés de champs texte personnalisés additionnels.
        /// </summary>
        public List<string> CustomFields { get; set; } = new List<string>();

        /// <summary>
        /// Liste de noms de fichiers ou pièces complémentaires obligatoires.
        /// </summary>
        public List<string> RequiredDocuments { get; set; } = new List<string>();
    }

    /// <summary>
    /// Configuration d'affichage visuel de la page publique de l'offre d'emploi.
    /// </summary>
    public class PublicDisplayConfig
    {
        /// <summary>
        /// Affiche ou non le nom de l'entreprise.
        /// </summary>
        public bool ShowCompanyName { get; set; } = true;

        /// <summary>
        /// Affiche ou non le logo de l'entreprise.
        /// </summary>
        public bool ShowCompanyLogo { get; set; } = true;

        /// <summary>
        /// Affiche ou non la fourchette de salaire.
        /// </summary>
        public bool ShowSalary { get; set; } = false;

        /// <summary>
        /// Affiche ou non le lieu du poste.
        /// </summary>
        public bool ShowLocation { get; set; } = true;

        /// <summary>
        /// Feuille de style CSS personnalisée appliquée à la page.
        /// </summary>
        public string? CustomCSS { get; set; }

        /// <summary>
        /// URL de l'image d'en-tête de la page de l'offre.
        /// </summary>
        public string? HeaderImageUrl { get; set; }
    }
}