using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream content, string fileName, string folder = "Documents", CancellationToken cancellationToken = default);
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    bool Exists(string filePath);
}
