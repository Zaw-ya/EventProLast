using EventPro.DAL.Dto;

namespace EventPro.Business.Storage.Interface
{
    public interface IBlobStorage
    {
        Task<string> UploadAsync(Stream stream, string contentType, string fileName, CancellationToken cancellationToken);
        Task<FileResponse> DownloadAsync(string fileId, CancellationToken cancellationToken);
        Task DeleteFileAsync(string fileName, CancellationToken cancellationToken);
        Task DeleteFolderAsync(string folderName, CancellationToken cancellationToken);
        Task<bool> FileExistsAsync(string filePath);
        Task<List<string>> GetFolderFilesAsync(string folderName, CancellationToken cancellationToken);
        Task<List<string>> GetFoldersInsideAFolderAsync(string folderName, CancellationToken cancellationToken);
        Task<bool> FolderExistsAsync(string folderName);
        Task<int> CountFilesInFolderAsync(string folderPath);
        Task<MemoryStream> DownloadFilesAsZipStreamAsync(string folderName);
    }
}
