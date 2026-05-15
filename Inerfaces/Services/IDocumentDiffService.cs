using System.Threading.Tasks;

namespace backend_api.Interfaces.Services
{
    public interface IDocumentDiffService
    {
        Task<string> GenerateDiffAsync(int documentId, int v1, int v2);
    }
}