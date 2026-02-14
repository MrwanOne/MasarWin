using Masar.Application.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(string basePath = "Uploads")
    {
        _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream content, string fileName, string folder = "Documents", CancellationToken cancellationToken = default)
    {
        var targetFolder = Path.Combine(_basePath, folder);
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(targetFolder, uniqueFileName);

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await content.CopyToAsync(fileStream, cancellationToken);

        return Path.Combine(folder, uniqueFileName);
    }

    public async Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("File not found on disk.", fullPath);
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    public bool Exists(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        return File.Exists(fullPath);
    }
}
