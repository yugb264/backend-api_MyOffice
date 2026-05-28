using backend_api.DTOs.Document;
using backend_api.DTOs.Version;
using backend_api.Inerfaces.Services;
using backend_api.Interfaces.Repositories;
using backend_api.Models;
using Microsoft.AspNetCore.Http;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _docRepo;
    private readonly IVersionRepository _versionRepo;
    private readonly FileStorageService _fileStorage;

    public DocumentService(
        IDocumentRepository docRepo,
       IVersionRepository versionRepo,
        FileStorageService fileStorage)
    {
        _docRepo = docRepo;
        _versionRepo = versionRepo;
        _fileStorage = fileStorage;
    }

    public async Task<UploadResponseDto> UploadAsync(IFormFile file, string userName)
    {
        if (file == null || file.Length == 0)
            throw new Exception("No file uploaded");



        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        var fileHash = FileHelper.GenerateHash(fileBytes);

        //  Check duplicate
        var existingDoc = _docRepo.GetByHash(fileHash);
        if (existingDoc != null)
        {
            return new UploadResponseDto
            {
                IsDuplicate = true,
                DocumentId = existingDoc.Id,
                FileName = existingDoc.FileName,
                Message = "File already exists"
            };
        }

        //  Create document
        var doc = new Document
        {
            FileName = file.FileName,
            FilePath = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FileHash = fileHash
        };

        _docRepo.Add(doc);
        await _docRepo.SaveAsync();

        //  Save original file
        await _fileStorage.SaveFileAsync(fileBytes, file.FileName);

        //  Create version
        var versionFileName = $"version_{DateTime.Now.Ticks}.docx";

        await _fileStorage.SaveFileAsync(fileBytes, versionFileName);

        var version = new DocumentVersion
        {
            DocumentId = doc.Id,
            VersionNumber = 1,
            FilePath = versionFileName,
            FileHash = fileHash,
            ModifiedBy = userName,
            ModifiedAt = DateTime.UtcNow
        };

        _versionRepo.Add(version);
        await _versionRepo.SaveAsync();

        return new UploadResponseDto

        {
            IsDuplicate = false,
            DocumentId = doc.Id,
            FileName = doc.FileName,
            Message = "Uploaded successfully"
        };
    }
    public async Task DeleteDocumentAsync(int id)
    {
        var doc =await _docRepo.GetByIdAsync(id);
        if (doc == null)
            throw new KeyNotFoundException("Document not found");

        //  Delete versions
        var versions = await _versionRepo.GetByDocumentIdAsync(id);

        foreach (var v in versions)
        {
            _fileStorage.DeleteFile(v.FilePath);
            _versionRepo.Remove(v);
        }

        //  Delete original file
        _fileStorage.DeleteFile(doc.FileName);

        _docRepo.Remove(doc);

        await _docRepo.SaveAsync();
    }
    public async Task<DocumentDetailDto> GetDocumentByIdAsync(int id)
    {
        var doc = await _docRepo.GetWithVersionsAsync(id);

        if (doc == null)
            throw new KeyNotFoundException("Document not found");

        return MapToDetailDto(doc);
    }
    public async Task DeleteVersionAsync(int documentId, int versionId)
    {
        var version = await _versionRepo.GetByIdAsync(versionId);

        if (version == null || version.DocumentId != documentId)
            throw new KeyNotFoundException("Version not found");

        if (version.VersionNumber == 1)
            throw new Exception("Cannot delete original version");

        _fileStorage.DeleteFile(version.FilePath);

        _versionRepo.Remove(version);

        await _versionRepo.SaveAsync();
    }
    private DocumentDto MapToDto(Document doc)
    {
        return new DocumentDto
        {
            Id = doc.Id,
            FileName = doc.FileName,
            FileSize = doc.FileSize,
            ContentType = doc.ContentType,
            CreatedAt = doc.CreatedAt
        };
    }

    private DocumentDetailDto MapToDetailDto(Document doc)
    {
        return new DocumentDetailDto
        {
            Id = doc.Id,
            FileName = doc.FileName,
            FileSize = doc.FileSize,
            ContentType = doc.ContentType,
            CreatedAt = doc.CreatedAt,
            Versions = doc.Versions?.Select(v => new VersionDto
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                ModifiedBy = v.ModifiedBy,
                ModifiedAt = v.ModifiedAt,
                ParentVersionNumber = v.ParentVersionNumber
            }).ToList()
        };
    }

    public async Task<List<DocumentDto>> GetAllAsync()
    {
        var docs = await _docRepo.GetAllAsync();

        return docs.Select(d => MapToDto(d)).ToList();
    }

    public async Task<List<VersionDto>> GetVersionsAsync(int documentId)
    {
        var versions = await _versionRepo.GetByDocumentIdAsync(documentId);

        return versions.OrderByDescending(v => v.VersionNumber).Select(v => new VersionDto
        {
            Id = v.Id,
            DocumentId = v.DocumentId,
            VersionNumber = v.VersionNumber,
            ModifiedBy = v.ModifiedBy,
            ModifiedAt = v.ModifiedAt,
            ParentVersionNumber = v.ParentVersionNumber
        }).ToList();
    }
}