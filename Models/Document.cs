using System;
using System.Collections.Generic;
namespace backend_api.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileHash { get; set; }
    
        public string ContentType { get; set; } 
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<DocumentVersion> Versions { get; set; }
    }
}
