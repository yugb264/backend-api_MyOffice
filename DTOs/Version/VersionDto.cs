namespace backend_api.DTOs.Version
{
    public class VersionDto
    {
        public int Id { get; set; }
        public int VersionNumber { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }

        public int? ParentVersionNumber { get; set; }

        public int DocumentId { get; set; }
    }
}
