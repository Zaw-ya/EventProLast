using EventPro.Business.Storage.Interface;
using EventPro.DAL.Dto;

namespace EventPro.Business.Storage.Implementation
{
    public class DummyBlobStorage : IBlobStorage
    {
        public Task UploadAsync(string container, string blobName, Stream stream) => Task.CompletedTask;
        public Task<Stream> DownloadAsync(string container, string blobName) => Task.FromResult<Stream>(Stream.Null);

        public Task<string> UploadAsync(Stream stream, string contentType, string fileName, CancellationToken cancellationToken)
        {
            return Task.FromResult(fileName);
        }

        public Task<FileResponse> DownloadAsync(string fileId, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FileResponse(Stream.Null, "application/octet-stream"));
        }

        public Task DeleteFileAsync(string fileName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteFolderAsync(string folderName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            return Task.FromResult(false);
        }

        public Task<List<string>> GetFolderFilesAsync(string folderName, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<string>());
        }

        public Task<List<string>> GetFoldersInsideAFolderAsync(string folderName, CancellationToken cancellationToken)
        {
             return Task.FromResult(new List<string>());
        }

        public Task<bool> FolderExistsAsync(string folderName)
        {
            return Task.FromResult(false);
        }

        public Task<int> CountFilesInFolderAsync(string folderPath)
        {
             return Task.FromResult(0);
        }

        public Task<MemoryStream> DownloadFilesAsZipStreamAsync(string folderName)
        {
            return Task.FromResult(new MemoryStream());
        }

        public string GetFileUrl(string fileName) => string.Empty;
    }
}
