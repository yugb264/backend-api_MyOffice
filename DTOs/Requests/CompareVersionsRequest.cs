namespace backend_api.DTOs.Requests
{
    public class CompareVersionsRequest
    {
        public int DocumentId { get; set; }
        public int V1 { get; set; }
        public int V2 { get; set; }
    }
}
