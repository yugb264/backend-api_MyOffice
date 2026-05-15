using backend_api.Data;
using backend_api.Interfaces.Repositories;
using backend_api.Models;
using Microsoft.EntityFrameworkCore;
public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Document GetById(int id)
    {
        return _context.Documents.Find(id);
    }

    public Document GetByHash(string hash)
    {
        return _context.Documents.FirstOrDefault(d => d.FileHash == hash);
    }

    public List<Document> GetAll()
    {
        return _context.Documents.ToList();
    }

    public void Add(Document doc)
    {
        _context.Documents.Add(doc);
    }

    public void Remove(Document doc)
    {
        _context.Documents.Remove(doc);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<Document> GetWithVersionsAsync(int id)
    {
        return await _context.Documents
       .AsNoTracking()                 
       .Include(d => d.Versions)       
       .FirstOrDefaultAsync(d => d.Id == id);
    }
    public async Task<List<Document>> GetAllAsync()
    {
        return await _context.Documents
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<Document> GetByIdAsync(int id)
    {
        return await _context.Documents.FindAsync(id);
    }
}