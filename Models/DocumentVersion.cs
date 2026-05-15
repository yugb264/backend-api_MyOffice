using System;

namespace backend_api.Models
{
    public class DocumentVersion
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int VersionNumber { get; set; }
        public string FilePath { get; set; }
        public string FileHash { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
        public Document Document { get; set; }

        public int? ParentVersionNumber { get; set; }
    }
}
