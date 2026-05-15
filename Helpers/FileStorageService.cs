public class FileStorageService
{
    private readonly string _storagePath;

    public FileStorageService()
    {
        _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "document-storage");

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task SaveFileAsync(byte[] bytes, string fileName)
    {
        var path = Path.Combine(_storagePath, fileName);
        await System.IO.File.WriteAllBytesAsync(path, bytes);
    }

    public byte[] GetFile(string fileName)
    {
        var path = Path.Combine(_storagePath, fileName);
        if (!System.IO.File.Exists(path)) return null;

        return System.IO.File.ReadAllBytes(path);
    }

    public void DeleteFile(string fileName)
    {
        var path = Path.Combine(_storagePath, fileName);
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }

    public string GetFullPath(string fileName)
    {
        return Path.Combine(_storagePath, fileName);
    }

    public async Task<byte[]> GetFileAsync(string fileName)
    {
        var fullPath = Path.Combine(_storagePath, fileName);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found");

        return await File.ReadAllBytesAsync(fullPath);
    }

}