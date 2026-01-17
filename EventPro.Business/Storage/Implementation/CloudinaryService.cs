using System.IO.Compression;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

using EventPro.Business.Storage.Interface;

using Microsoft.Extensions.Configuration;

namespace EventPro.Business.Storage.Implementation
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var settings = configuration.GetSection("CloudinarySettings");
            var cloudName = settings["CloudName"];
            var apiKey = settings["ApiKey"];
            var apiSecret = settings["ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new InvalidOperationException("Cloudinary settings are not configured properly in appsettings.json");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadImageAsync(Stream stream, string fileName, string folder = null)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentException("Stream cannot be null or empty", nameof(stream));
            }

            try
            {
                // Ensure stream is at the beginning
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                // Centralized image validation
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException("Invalid file type. Only images (jpg, png, gif, webp) are allowed.");
                }

                if (stream.Length > 5 * 1024 * 1024)
                {
                    throw new ArgumentException("File size must be less than 5MB.");
                }

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(fileName, stream),
                    PublicId = Path.GetFileNameWithoutExtension(fileName),
                    Overwrite = true,
                    UseFilename = true,
                    UniqueFilename = true
                };

                if (!string.IsNullOrEmpty(folder))
                {
                    uploadParams.Folder = folder;
                }

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload image to Cloudinary: {ex.Message}", ex);
            }
        }

        public async Task<string> UploadVideoAsync(Stream stream, string fileName, string folder = null)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentException("Stream cannot be null or empty", nameof(stream));
            }

            try
            {
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                // Centralized video validation
                var allowedExtensions = new[] { ".mp4", ".mov", ".avi" };
                var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException("Invalid file type. Only videos (mp4, mov, avi) are allowed.");
                }

                if (stream.Length > 15 * 1024 * 1024)
                {
                    throw new ArgumentException("Video size must be less than 15MB.");
                }

                var uploadParams = new VideoUploadParams()
                {
                    File = new FileDescription(fileName, stream),
                    PublicId = Path.GetFileNameWithoutExtension(fileName),
                    Overwrite = true,
                    UseFilename = true,
                    UniqueFilename = true
                };

                if (!string.IsNullOrEmpty(folder))
                {
                    uploadParams.Folder = folder;
                }

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload video to Cloudinary: {ex.Message}", ex);
            }
        }

        public async Task<string> UploadFileAsync(Stream stream, string fileName, string folder = null)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentException("Stream cannot be null or empty", nameof(stream));
            }

            try
            {
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                // Centralized file validation (primarily for PDF based on Events.cs)
                var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
                if (fileExtension != ".pdf")
                {
                    throw new ArgumentException("Invalid file type. Only PDF files are allowed here.");
                }

                if (stream.Length > 15 * 1024 * 1024)
                {
                    throw new ArgumentException("File size must be less than 15MB.");
                }

                var uploadParams = new RawUploadParams()
                {
                    File = new FileDescription(fileName, stream),
                    PublicId = Path.GetFileNameWithoutExtension(fileName),
                    Overwrite = true,
                    UseFilename = true,
                    UniqueFilename = true
                };

                if (!string.IsNullOrEmpty(folder))
                {
                    uploadParams.Folder = folder;
                }

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload file to Cloudinary: {ex.Message}", ex);
            }
        }

        public async Task<string> UpdateImageAsync(string publicId, Stream stream, string fileName)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(fileName, stream),
                PublicId = publicId,
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl?.ToString() ?? string.Empty;
        }

        public async Task<string> UpdateFileAsync(string publicId, Stream stream, string fileName)
        {
            var uploadParams = new RawUploadParams()
            {
                File = new FileDescription(fileName, stream),
                PublicId = publicId,
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl?.ToString() ?? string.Empty;
        }

        public async Task<bool> DeleteAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }

        /// <summary>
        /// Downloads all images from a specific Cloudinary folder and returns them as a ZIP stream.
        /// Using cursor logic to handle if the results goes over 500
        /// </summary>
        /// <param name="folderName">Folder path, e.g., "cards/25/"</param>
        /// <param name="maxResults">Default number of resources per request (default 500)</param>
        /// <returns>MemoryStream containing the ZIP file</returns>
        public async Task<MemoryStream> DownloadFilesAsZipStreamAsync(string folderName, int maxResults = 500)
        {
            if (!folderName.EndsWith("/"))
                folderName += "/";
            if (folderName.StartsWith("/"))
                folderName = folderName.Substring(1);

            var zipStream = new MemoryStream();

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                string nextCursor = null;
                bool hasMore = true;

                while (hasMore)
                {
                    var searchResult = await _cloudinary.Search()
                        .Expression($"folder:{folderName}*")   // أو prefix:cards/25/
                        .MaxResults(maxResults)
                        .NextCursor(nextCursor)
                        .ExecuteAsync();

                    if (searchResult?.Resources == null || !searchResult.Resources.Any())
                    {
                        hasMore = false;
                        continue;
                    }

                    foreach (var resource in searchResult.Resources)
                    {
                        string fileName = Path.GetFileName(resource.PublicId) + ".jpg";

                        try
                        {
                            using var client = new HttpClient();
                            var imageBytes = await client.GetByteArrayAsync(resource.SecureUrl);

                            var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                            using var entryStream = entry.Open();
                            await entryStream.WriteAsync(imageBytes);
                        }
                        catch (Exception ex)
                        {
                            // سجّل الخطأ بدون توقف العملية كلها
                            Console.WriteLine($"Failed to download {fileName}: {ex.Message}");
                        }
                    }

                    nextCursor = searchResult.NextCursor;
                    hasMore = !string.IsNullOrEmpty(nextCursor);
                }
            }

            zipStream.Position = 0;
            return zipStream;
        }

        // https://res.cloudinary.com/{cloud}/image/upload/v{version}/{publicId}.{format}
        // In our case we has : 
        // QR/{event_id}/{guestId}
        // card/{event_id}/E00000_{guestId}_{noOfMembers}.jpg
        public async Task<string> GetLatestVersionUrlAsync(
            string publicId, // e.g., "QR/123/456"
            string resourceType = "image")
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new ArgumentException("publicId is required");

            var getParams = new GetResourceParams(publicId)
            {
                ResourceType = resourceType switch
                {
                    "video" => ResourceType.Video,
                    "raw" => ResourceType.Raw,
                    _ => ResourceType.Image
                }
            };

            var resource = await _cloudinary.GetResourceAsync(getParams);

            if (resource == null)
                throw new Exception("Resource not found on Cloudinary");

            // https://res.cloudinary.com/{cloud}/image/upload/v{version}/{publicId}.{format}
            var url = _cloudinary.Api.UrlImgUp
                .Version(resource.Version)
                .BuildUrl($"{resource.PublicId}.{resource.Format}");

            return url;
        }


    }
}
