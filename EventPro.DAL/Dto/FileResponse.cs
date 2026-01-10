using System.IO;

namespace EventPro.DAL.Dto
{
    public class FileResponse
    {
        public FileResponse(Stream stream , string contentType)
        {
            this.Stream = stream;
            this.ContentType = contentType;
        }
        public Stream Stream { get; set; }
        public string ContentType { get; set; }
    }
}
