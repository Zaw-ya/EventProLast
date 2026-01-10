using Microsoft.AspNetCore.StaticFiles;

namespace EventPro.Utility
{
    public static class ContentType
    {
        public static string GetContentType(string FileName)
        {
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(FileName, out contentType);
            return contentType ?? "application/octet-stream";
        }
    }
}
