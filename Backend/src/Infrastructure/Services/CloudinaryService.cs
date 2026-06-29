using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    /// <summary>
    /// Service de gestion du stockage de fichiers sur Cloudinary.
    /// Gère l'upload, la suppression et la génération d'URLs signées pour les fichiers de CV (PDF, DOCX)
    /// et tout autre actif binaire de la plateforme NovaHire.
    /// </summary>
    public class CloudinaryService : IStorageService
    {
        private readonly Cloudinary _cloudinary;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="CloudinaryService"/>.
        /// Lit les clés d'API Cloudinary depuis la configuration et active les URLs sécurisées (HTTPS).
        /// </summary>
        /// <param name="configuration">La configuration de l'application (section 'CloudinarySettings').</param>
        public CloudinaryService(IConfiguration configuration)
        {
            var cloudName = configuration["CloudinarySettings:CloudName"];
            var apiKey = configuration["CloudinarySettings:ApiKey"];
            var apiSecret = configuration["CloudinarySettings:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new ArgumentException("Les identifiants Cloudinary ne sont pas configurés dans appsettings.json");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; // Forcer HTTPS sur toutes les URLs générées
        }

        /// <summary>
        /// Téléverse un fichier brut (PDF ou DOCX) sur Cloudinary dans le répertoire spécifié.
        /// Utilise le type de ressource 'raw' pour préserver l'intégrité du fichier binaire.
        /// </summary>
        /// <param name="fileStream">Le flux de données binaires du fichier.</param>
        /// <param name="fileName">Le nom du fichier avec son extension.</param>
        /// <param name="folderPath">Le chemin du dossier de destination sur Cloudinary.</param>
        /// <returns>L'URL HTTPS sécurisée du fichier téléversé.</returns>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath)
        {
            // Utilisation de RawUploadParams pour les documents binaires (PDF, DOCX)
            // Cela préserve l'extension du fichier, contrairement à ImageUploadParams
            var uploadParams = new RawUploadParams()
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folderPath,
                UseFilename = true,
                UniqueFilename = true,
                Type = "upload",
                AccessMode = "public"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Erreur Cloudinary lors de l'upload du fichier {fileName} : {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl.ToString();
        }

        /// <summary>
        /// Supprime un fichier de type 'raw' (PDF, DOCX) sur Cloudinary à partir de son URL.
        /// Extrait dynamiquement le PublicId Cloudinary depuis l'URL du fichier.
        /// </summary>
        /// <param name="fileUrl">L'URL HTTPS complète du fichier Cloudinary à supprimer.</param>
        /// <returns><c>true</c> si la suppression a réussi, <c>false</c> sinon.</returns>
        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl) || !fileUrl.Contains("cloudinary.com"))
                return false;

            try
            {
                // Extraction du PublicID à partir de l'URL
                // Format type: https://res.cloudinary.com/cloudname/raw/upload/v12345/folder/subfolder/file.pdf
                // On a besoin de : folder/subfolder/file.pdf (sans l'extension .pdf pour les images, mais AVEC l'extension pour les "raw" files)
                
                var uri = new Uri(fileUrl);
                var path = uri.AbsolutePath; // /cloudname/raw/upload/v12345/folder/subfolder/file.pdf
                
                // On enlève les segments inutiles
                var segments = path.Split('/');
                int uploadIndex = Array.IndexOf(segments, "upload");
                
                if (uploadIndex == -1 || segments.Length <= uploadIndex + 2)
                    return false;

                // On saute le segment de version (commençant par 'v')
                int startIndex = uploadIndex + 2; 
                var publicId = string.Join("/", segments.Skip(startIndex));
                
                // Pour les fichiers "raw", le publicId inclut l'extension
                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Raw
                };

                var result = await _cloudinary.DestroyAsync(deletionParams);
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CloudinaryService] Erreur lors de la suppression : {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Génère une URL de téléchargement signée pour un fichier 'raw' Cloudinary.
        /// L'URL signée inclut un token de sécurité temporaire permettant d'accéder au fichier sans exposition publique des clés API.
        /// </summary>
        /// <param name="fileUrl">L'URL HTTPS du fichier Cloudinary.</param>
        /// <returns>L'URL signée et sécurisée prête à être utilisée pour le téléchargement.</returns>
        public string GetDownloadUrl(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl) || !fileUrl.Contains("cloudinary.com"))
                return fileUrl;

            try
            {
                // On extrait le PublicID de l'URL pour générer une URL signée
                var uri = new Uri(fileUrl);
                var path = uri.AbsolutePath;
                var segments = path.Split('/');
                int uploadIndex = Array.IndexOf(segments, "upload");
                
                if (uploadIndex == -1 || segments.Length <= uploadIndex + 2)
                    return fileUrl;

                var versionSegment = segments[uploadIndex + 1];
                int startIndex = uploadIndex + 2;
                var publicId = string.Join("/", segments.Skip(startIndex));

                // Construction de l'URL signée pour les ressources 'raw' en HTTPS
                // On préserve la version de l'URL originale pour éviter les incohérences de version
                var urlBuilder = _cloudinary.Api.Url
                    .ResourceType("raw")
                    .Type("upload")
                    .Secure(true)
                    .Signed(true);

                if (!string.IsNullOrWhiteSpace(versionSegment) && versionSegment.StartsWith("v"))
                {
                    urlBuilder = urlBuilder.Version(versionSegment.TrimStart('v'));
                }

                return urlBuilder.BuildUrl(publicId);
            }
            catch
            {
                return fileUrl; // En cas d'erreur, on retourne l'URL d'origine
            }
        }
    }
}
