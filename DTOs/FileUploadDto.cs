using Microsoft.AspNetCore.Http;

namespace server.DTOs
{
    public class FileUploadDto
    {
        public IFormFile File { get; set; }
    }
} 