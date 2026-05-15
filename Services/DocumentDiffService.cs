using backend_api.Interfaces.Repositories;
using backend_api.Interfaces.Services;
using backend_api.Helpers; // 👈 IMPORTANT
using System;
using System.Threading.Tasks;

public class DocumentDiffService : IDocumentDiffService
{
    private readonly IVersionRepository _versionRepo;
    private readonly FileStorageService _fileStorage;

    public DocumentDiffService(
        IVersionRepository versionRepo,
        FileStorageService fileStorage)
    {
        _versionRepo = versionRepo;
        _fileStorage = fileStorage;
    }

    public async Task<string> GenerateDiffAsync(int documentId, int v1, int v2)
    {
        
        var version1 = _versionRepo.GetById(v1);
        var version2 = _versionRepo.GetById(v2);

        if (version1 == null || version2 == null)
            throw new Exception("Version not found");

       
        var file1 = await _fileStorage.GetFileAsync(version1.FilePath);
        var file2 = await _fileStorage.GetFileAsync(version2.FilePath);

        if (file1 == null || file2 == null)
            throw new Exception("File not found");

       
        var diffHelper = new DocumentDiffHelper();
        var diffBytes = diffHelper.GenerateDiff(file1, file2);

       
        var diffFileName = $"diff_{Guid.NewGuid()}.docx";
        await _fileStorage.SaveFileAsync(diffBytes, diffFileName);

       
        return diffFileName;
    }
}