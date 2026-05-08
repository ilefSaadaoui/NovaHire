using System.IO;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStorageService
    {
        /// <summary>
        /// Télécharge un fichier vers le stockage cloud.
        /// </summary>
        /// <param name="fileStream">Le flux de données du fichier.</param>
        /// <param name="fileName">Le nom d'origine du fichier.</param>
        /// <param name="folderPath">Le chemin du dossier (ex: novahire/companies/CompanyName/cvs).</param>
        /// <returns>L'URL publique du fichier téléchargé.</returns>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath);

        /// <summary>
        /// Supprime un fichier du stockage cloud.
        /// </summary>
        /// <param name="fileUrl">L'URL complète du fichier à supprimer.</param>
        /// <returns>True si la suppression a réussi.</returns>
        Task<bool> DeleteFileAsync(string fileUrl);

        /// <summary>
        /// Génère une URL de téléchargement signée ou sécurisée pour un fichier.
        /// </summary>
        /// <param name="fileUrl">L'URL publique d'origine.</param>
        /// <returns>L'URL de téléchargement avec signature de sécurité.</returns>
        string GetDownloadUrl(string fileUrl);
    }
}
