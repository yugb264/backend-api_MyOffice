using backend_api.DTOs.Version;

namespace backend_api.DTOs.Document
{
    public class DocumentDetailDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<VersionDto> Versions { get; set; }
    }
}
