using backend_api.DTOs.Document;
using backend_api.DTOs.Version;
using backend_api.Models;

namespace backend_api.Inerfaces.Services
{
    public interface IDocumentService
    {
        Task<UploadResponseDto> UploadAsync(IFormFile file, string userName);

        Task DeleteDocumentAsync(int id);

        Task DeleteVersionAsync(int documentId, int versionId);

        Task<DocumentDetailDto> GetDocumentByIdAsync(int id);

        Task<List<DocumentDto>> GetAllAsync();
        Task<List<VersionDto>> GetVersionsAsync(int documentId);
    }
}