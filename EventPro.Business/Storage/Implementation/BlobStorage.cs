using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Google.Api.Gax.ResourceNames;
using EventPro.Business.Storage.Interface;
using EventPro.DAL.Dto;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace EventPro.Business.Storage.Implementation
{
    public class BlobStorage : IBlobStorage
    {
        private readonly BlobServiceClient _blobStorage;
        public BlobStorage(BlobServiceClient blobStorage)
        {
            _blobStorage = blobStorage;
        }
        private readonly string ContainerName = "upload";
        public async Task DeleteFileAsync(string fileName, CancellationToken cancellationToken)
        {
            if (fileName.StartsWith("/"))
            {
                fileName = fileName.Substring(1);
            }

            BlobContainerClient containerClient = _blobStorage.GetBlobContainerClient(ContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }

        public async Task DeleteFolderAsync(string folderName, CancellationToken cancellationToken)
        {
            if (!folderName.EndsWith("/"))
            {
                folderName += "/";
            }

            if (folderName.StartsWith("/"))
            {
                folderName = folderName.Substring(1);
            }

            BlobContainerClient containerClient = _blobStorage.GetBlobContainerClient(ContainerName);

            await foreach (BlobItem blob in containerClient.GetBlobsAsync(prefix: folderName.ToString(), cancellationToken: cancellationToken))
            {
                BlobClient selectedBlob = containerClient.GetBlobClient(blob.Name);
                await selectedBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
            }

        }

        public async Task<MemoryStream> DownloadFilesAsZipStreamAsync(string folderName)
        {
            if (!folderName.EndsWith("/"))
            {
                folderName += "/";
            }

            if (folderName.StartsWith("/"))
            {
                folderName = folderName.Substring(1);
            }

            BlobContainerClient containerClient = _blobStorage.GetBlobContainerClient(ContainerName);
            MemoryStream zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: folderName))
                {
                    if (!blobItem.Name.EndsWith("/"))
                    {
                        BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                        using (MemoryStream fileStream = new MemoryStream())
                        {
                            await blobClient.DownloadToAsync(fileStream);
                            fileStream.Position = 0;
                            string baseName = Path.GetFileName(blobItem.Name);
                            var entry = archive.CreateEntry(baseName, CompressionLevel.Optimal);
                            using (var entryStream = entry.Open())
                            {
                                await fileStream.CopyToAsync(entryStream);
                            }
                        }
                    }
                }
            }
            zipStream.Position = 0;
            return zipStream;
        }

        public async Task<List<string>> GetFolderFilesAsync(string folderName, CancellationToken cancellationToken)
        {
            List<string> files = new List<string>();
            if (!folderName.EndsWith("/"))
            {
                folderName += "/";
            }

            if (folderName.StartsWith("/"))
            {
                folderName = folderName.Substring(1);
            }

            BlobContainerClient containerClient = _blobStorage.GetBlobContainerClient(ContainerName);

            await foreach (BlobItem blob in containerClient.GetBlobsAsync(prefix: folderName.ToString(), cancellationToken: cancellationToken))
            {
                if (!blob.Name.EndsWith("/"))
                {
                    files.Add(blob.Name);
                }
            }

            return files;

        }

        public async Task<FileResponse> DownloadAsync(string fileName, CancellationToken cancellationToken)
        {
            BlobContainerClient containerClient = _blobStorage.GetBlobContainerClient(ContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            Response<BlobDownloadResult> response = await blobClient.DownloadContentAsync(cancellationToken: cancellationToken);

            return new FileResponse(response.Value.Content.ToStream(), response.Value.Details.ToString());
        }

        /// <summary>
        /// Sanitizes the file name portion of a blob path.
        /// Replaces spaces and unsafe characters with underscores, and truncates to 40 characters (preserving extension).
        /// The folder prefix (everything before the last '/') is kept unchanged.
        /// </summary>
        private static string SanitizeFileName(string blobPath)
        {
            int lastSlash = blobPath.LastIndexOf('/');
            string folder = lastSlash >= 0 ? blobPath.Substring(0, lastSlash + 1) : string.Empty;
            string raw    = lastSlash >= 0 ? blobPath.Substring(lastSlash + 1) : blobPath;

            string ext  = Path.GetExtension(raw);
            string name = Path.GetFileNameWithoutExtension(raw);

            // Replace any character that is not alphanumeric, dash, or dot with underscore
            name = Regex.Replace(name, @"[^a-zA-Z0-9\-]", "_");
            // Collapse consecutive underscores and trim edges
            name = Regex.Replace(name, @"_+", "_").Trim('_');
            // Truncate to 40 characters
            if (name.Length > 40)
                name = name.Substring(0, 40).TrimEnd('_');

            return folder + name + ext;
        }

        public async Task<string> UploadAsync(Stream stream, string contentType, string fileName, CancellationToken cancellationToken)
        {
            fileName = SanitizeFileName(fileName);
            stream.Position = 0;

            BlobContainerClient containerClient = _blobStorage.GetBlobContainerClient(ContainerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } },
                cancellationToken: cancellationToken);

            return GetFileUrl(fileName);
        }

        public string GetFileUrl(string fileName)
        {
            if (fileName.StartsWith("/"))
                fileName = fileName.Substring(1);
            BlobContainerClient containerClient = _blobStorage.GetBlobContainerClient(ContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            var sasBuilder = new Azure.Storage.Sas.BlobSasBuilder
            {
                BlobContainerName = ContainerName,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddYears(1)
            };
            sasBuilder.SetPermissions(Azure.Storage.Sas.BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
        public async Task<bool> FileExistsAsync(string filePath)
        {
            if (filePath.StartsWith("/"))
            {
                filePath = filePath.Substring(1);
            }

            BlobContainerClient containerClient = _blobStorage.GetBlobContainerClient(ContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(filePath);
            return await blobClient.ExistsAsync();
        }

        public async Task<bool> FolderExistsAsync(string folderName)
        {
            if (!folderName.EndsWith("/"))
            {
                folderName += "/";
            }

            if (folderName.StartsWith("/"))
            {
                folderName = folderName.Substring(1);
            }

            BlobContainerClient containerClient = _blobStorage.GetBlobContainerClient(ContainerName);
            var blobs = containerClient.GetBlobsAsync(prefix: folderName);
            return await blobs.AnyAsync();
        }

        public async Task<int> CountFilesInFolderAsync(string folderName)
        {

            if (!folderName.EndsWith("/"))
            {
                folderName += "/";
            }

            if (folderName.StartsWith("/"))
            {
                folderName = folderName.Substring(1);
            }

            int fileCount = 0;
            BlobContainerClient containerClient = _blobStorage.GetBlobContainerClient(ContainerName);
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: folderName))
            {
                if (!blobItem.Name.EndsWith("/"))
                {
                    fileCount++;
                }
            }
            return fileCount;
        }

        public async Task<List<string>> GetFoldersInsideAFolderAsync(string folderName, CancellationToken cancellationToken)
        {
            HashSet<string> folders = new HashSet<string>(); // Use HashSet to avoid duplicates

            if (!folderName.EndsWith("/"))
            {
                folderName += "/";
            }

            if (folderName.StartsWith("/"))
            {
                folderName = folderName.Substring(1);
            }

            BlobContainerClient containerClient = _blobStorage.GetBlobContainerClient(ContainerName);

            await foreach (BlobItem blob in containerClient.GetBlobsAsync(prefix: folderName, cancellationToken: cancellationToken))
            {
                string relativePath = blob.Name.Substring(folderName.Length); // Get path inside given folder

                int slashIndex = relativePath.IndexOf('/');
                if (slashIndex != -1)
                {
                    string subfolder = relativePath.Substring(0, slashIndex);
                    folders.Add(subfolder);
                }
            }

            return folders.ToList();
        }
    }
}
