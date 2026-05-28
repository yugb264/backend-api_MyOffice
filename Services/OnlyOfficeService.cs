using backend_api.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using backend_api.Interfaces.Services;
using backend_api.Interfaces.Repositories;
using Microsoft.AspNetCore.SignalR;
using backend_api.Hubs;

public class OnlyOfficeService : IOnlyOfficeService
{
    private readonly IDocumentRepository _docRepo;
    private readonly IVersionRepository _versionRepo;
    private readonly FileStorageService _fileStorage;
    private readonly IHubContext<DocumentHub> _hub;

    public OnlyOfficeService(
        IDocumentRepository docRepo,
        IVersionRepository versionRepo,
        FileStorageService fileStorage,
        IHubContext<DocumentHub> hub)
    {
        _docRepo = docRepo;
        _versionRepo = versionRepo;
        _fileStorage = fileStorage;
        _hub = hub;

    }
    public object GetConfig(int documentId, int? versionId, string userName, string email, string oid)
    {
        var doc = _docRepo.GetById(documentId);
        if (doc == null) throw new Exception("Document not found");

        string fileName;

        if (versionId.HasValue)
        {
            var version = _versionRepo.GetById(versionId.Value);
            if (version == null) throw new Exception("Version not found");

            fileName = version.FilePath;
        }
        else
        {
            fileName = doc.FileName;
        }

        var fileUrl = $"http://host.docker.internal:5000/api/document/files/{Uri.EscapeDataString(fileName)}";

        var payload = new
        {
            document = new
            {
                fileType = Path.GetExtension(doc.FileName).Replace(".", ""),
                key = versionId.HasValue
    ? $"doc-{doc.Id}-v{versionId}"
    : $"doc-{doc.Id}-latest",
                title = doc.FileName,
                url = fileUrl
            },
            editorConfig = new
            {
                mode = "edit",
                callbackUrl = $"http://host.docker.internal:5000/api/document/callback/{documentId}?user={Uri.EscapeDataString(userName)}&versionId={versionId}",
                forcesave = true,
                user = new
                {
                    id = oid,
                    name = userName
                },
                customization = new
                {
                    autosave = false,
                    forcesave = true
                },
                userInfo = new
                {
                    name = userName,
                    email = email,
                    oid = oid
                },
                coEditing = new
                {
                    mode = "fast",
                    change = true
                }
            }
        };

        return new
        {
            document = payload.document,
            editorConfig = payload.editorConfig
        };
    }
    public async Task<object> HandleCallback(int documentId, JObject data, string userName, int? versionId)
    {
        var status = data["status"]?.Value<int>();

        if (data["notmodified"]?.Value<bool>() == true)
        {
            return new { error = 0 };
        }

        if (status == 6)
        {
            var isForceSave = data["forcesavetype"] != null;

            if (!isForceSave)
            {
                return new { error = 0 };
            }

            var downloadUrl = data["url"]?.ToString();

            if (string.IsNullOrEmpty(downloadUrl))
            {
                return new { error = 1 };
            }

            using var client = new HttpClient();
            var response = await client.GetAsync(downloadUrl);

            if (!response.IsSuccessStatusCode)
            {
                return new { error = 1 };
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();

            var textPreview = Encoding.UTF8.GetString(bytes.Take(200).ToArray());

            if (textPreview.Contains("<html") || textPreview.Contains("<!DOCTYPE"))
            {
                return new { error = 1 };
            }

            var isZip = bytes.Take(2).SequenceEqual(new byte[] { 0x50, 0x4B });
            var isPdf = bytes.Take(5).SequenceEqual(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D });

            if (!isZip && !isPdf)
            {
                return new { error = 1 };
            }

            if (bytes.Length == 0)
            {
                return new { error = 1 };
            }

            var doc = _docRepo.GetById(documentId);
            if (doc == null)
            {
                return new { error = 1 };
            }

            var fileExt = Path.GetExtension(doc.FileName);

            var hash = FileHelper.GenerateHash(bytes);

            var lastVersion = _versionRepo.GetLastVersion(documentId);

            int? parentVersionNumber = null;

            if (versionId.HasValue)
            {
                var parentVersion = _versionRepo.GetById(versionId.Value);
                parentVersionNumber = parentVersion?.VersionNumber;
            }
            else
            {
                parentVersionNumber = lastVersion?.VersionNumber;
            }

            if (lastVersion != null && lastVersion.FileHash == hash)
            {
                return new { error = 0 };
            }

            var versionFileName = $"version_{DateTime.Now.Ticks}{fileExt}";

            await _fileStorage.SaveFileAsync(bytes, versionFileName);
          
            var version = new DocumentVersion
            {
                DocumentId = documentId,
                VersionNumber = lastVersion != null ? lastVersion.VersionNumber + 1 : 1,
                FilePath = versionFileName,
                FileHash = hash,
                ModifiedBy = userName,
                ModifiedAt = DateTime.UtcNow,
                ParentVersionNumber = parentVersionNumber
            };

            _versionRepo.Add(version);
            await _versionRepo.SaveAsync();
            Console.WriteLine("🔥 VERSION SAVED + NOTIFIED");


            await _hub.Clients.Group(documentId.ToString())
     .SendAsync("DocumentUpdated", new
     {
         documentId = documentId,
         versionNumber = version.VersionNumber
     });

            // Update original file AFTER versioning
            await _fileStorage.SaveFileAsync(bytes, doc.FileName);
        }

        return new { error = 0 };
    }
}