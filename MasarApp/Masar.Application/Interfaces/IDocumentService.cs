using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface IDocumentService
{
    Task<Result<DocumentDto>> UploadAsync(int projectId, Stream content, string fileName, string? description = null, CancellationToken cancellationToken = default);
    Task<List<DocumentDto>> GetByProjectAsync(int projectId, CancellationToken cancellationToken = default);
    Task<Result<Stream>> DownloadAsync(int documentId, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int documentId, CancellationToken cancellationToken = default);
    Task<Result<DocumentDto>> ApproveAsync(int documentId, CancellationToken cancellationToken = default);
    Task<Result<DocumentDto>> RejectAsync(int documentId, CancellationToken cancellationToken = default);
}
