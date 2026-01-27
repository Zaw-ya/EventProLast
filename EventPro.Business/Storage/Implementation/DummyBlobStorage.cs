using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            throw new NotImplementedException();
        }

        public Task<FileResponse> DownloadAsync(string fileId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFileAsync(string fileName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFolderAsync(string folderName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetFolderFilesAsync(string folderName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetFoldersInsideAFolderAsync(string folderName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FolderExistsAsync(string folderName)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountFilesInFolderAsync(string folderPath)
        {
            throw new NotImplementedException();
        }

        public Task<MemoryStream> DownloadFilesAsZipStreamAsync(string folderName)
        {
            throw new NotImplementedException();
        }
    }
}
