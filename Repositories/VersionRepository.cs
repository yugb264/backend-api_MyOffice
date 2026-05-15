using backend_api.Data;
using backend_api.Interfaces.Repositories;
using backend_api.Models;
using Microsoft.EntityFrameworkCore;
public class VersionRepository : IVersionRepository
{
    private readonly AppDbContext _context;

    public VersionRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<DocumentVersion> GetByDocumentId(int documentId)
    {
        return _context.DocumentVersions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToList();
    }

    public DocumentVersion GetById(int id)
    {
        return _context.DocumentVersions.FirstOrDefault(v => v.Id == id);
    }

    public DocumentVersion GetLastVersion(int documentId)
    {
        return _context.DocumentVersions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefault();
    }

    public void Add(DocumentVersion version)
    {
        _context.DocumentVersions.Add(version);
    }

    public void Remove(DocumentVersion version)
    {
        _context.DocumentVersions.Remove(version);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<List<DocumentVersion>> GetByDocumentIdAsync(int documentId)
    {
        return await _context.DocumentVersions
            .Where(v => v.DocumentId == documentId)
            .ToListAsync();
    }
    public async Task<DocumentVersion> GetByIdAsync(int id)
    {
        return await _context.DocumentVersions
            .FirstOrDefaultAsync(v => v.Id == id);
    }
}