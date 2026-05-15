using Newtonsoft.Json.Linq;

namespace backend_api.DTOs.Requests
{
    public class CallbackRequest
    {
        public int DocumentId { get; set; }
        public JObject Data { get; set; }
        public string UserName { get; set; }
        public int? VersionId { get; set; }
    }
}
