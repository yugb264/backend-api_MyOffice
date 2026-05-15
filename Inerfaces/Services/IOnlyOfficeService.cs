using Newtonsoft.Json.Linq;

namespace backend_api.Interfaces.Services
{
    public interface IOnlyOfficeService
    {
        object GetConfig(int documentId, int? versionId, string userName, string email, string oid);

        Task<object> HandleCallback(int documentId, JObject data, string userName, int? versionId);
    }
}