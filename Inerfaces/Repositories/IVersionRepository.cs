using backend_api.Models;

namespace backend_api.Interfaces.Repositories
{
    public interface IVersionRepository
    {
        List<DocumentVersion> GetByDocumentId(int documentId);

        DocumentVersion GetById(int id);

        DocumentVersion GetLastVersion(int documentId);

        void Add(DocumentVersion version);

        void Remove(DocumentVersion version);

        Task SaveAsync();

        Task<List<DocumentVersion>> GetByDocumentIdAsync(int documentId);

        Task<DocumentVersion> GetByIdAsync(int id);
    }
}