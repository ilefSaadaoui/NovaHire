using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class CloudinaryService : IStorageService
    {
        private readonly Cloudinary _cloudinary;

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
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderPath)
        {
            // On utilise RawUploadParams pour les documents (PDF, DOCX)
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

                // Generate a signed delivery URL for raw files on the "upload" delivery type.
                // With version kept from the original URL, this avoids v1 mismatches.
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
