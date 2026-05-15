namespace backend_api.DTOs.Requests
{
    public class DeleteVersionRequest
    {
        public int DocumentId { get; set; }
        public int VersionId { get; set; }
    }
}
