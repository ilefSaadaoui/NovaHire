namespace Domain.Enums
{
    /// <summary>
    /// Rôles utilisateur dans la plateforme
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Super administrateur - Accès complet à toutes les sociétés
        /// </summary>
        SuperAdmin = 0,

        /// <summary>
        /// Administrateur de société - Gestion complète de sa société
        /// </summary>
        CompanyAdmin = 1,

        /// <summary>
        /// Recruteur - Création d'offres et gestion des candidatures
        /// </summary>
        Recruiter = 2
    }
}
