namespace EventPro.Business.Storage.Interface
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(Stream stream, string fileName, string folder = null);
        Task<string> UploadVideoAsync(Stream stream, string fileName, string folder = null);
        Task<string> UploadFileAsync(Stream stream, string fileName, string folder = null);
        Task<string> UploadRawFileAsync(Stream stream, string fileName, string folder = null);
        Task<string> UpdateImageAsync(string publicId, Stream stream, string fileName);
        Task<string> UpdateFileAsync(string publicId, Stream stream, string fileName);
        Task<bool> DeleteAsync(string publicId);
        Task<bool> DeleteByPrefixAsync(string prefix);
        Task<MemoryStream> DownloadFilesAsZipStreamAsync(string folderName, int maxResults = 500);
        Task<string> GetLatestVersionUrlAsync(
            string publicId, // e.g., "QR/123/456"
            string resourceType = "image");
    }
}
