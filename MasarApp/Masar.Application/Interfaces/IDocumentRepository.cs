using Masar.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<List<Document>> GetByProjectIdAsync(int projectId, CancellationToken cancellationToken = default);
    Task<List<Document>> GetByDiscussionIdAsync(int discussionId, CancellationToken cancellationToken = default);
    Task<List<Document>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<Document?> GetLatestVersionAsync(int projectId, string fileName, CancellationToken cancellationToken = default);
}
