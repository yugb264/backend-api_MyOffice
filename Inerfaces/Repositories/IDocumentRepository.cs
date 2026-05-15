using backend_api.Models;

namespace backend_api.Interfaces.Repositories
{
    public interface IDocumentRepository
    {
        Document GetById(int id);
        Document GetByHash(string hash);
        List<Document> GetAll();

        void Add(Document document);
        void Remove(Document document);

        Task SaveAsync();

        Task<Document> GetWithVersionsAsync(int id);
        Task<List<Document>> GetAllAsync();
        Task<Document> GetByIdAsync(int id);

    }
}