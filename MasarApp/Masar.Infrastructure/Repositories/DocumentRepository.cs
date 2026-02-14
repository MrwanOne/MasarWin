using Masar.Domain.Entities;
using Masar.Application.Interfaces;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class DocumentRepository : EfRepository<Document>, IDocumentRepository
{
    public DocumentRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<List<Document>> GetByProjectIdAsync(int projectId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Documents.AsNoTracking()
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Document>> GetByDiscussionIdAsync(int discussionId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Documents.AsNoTracking()
            .Where(d => d.DiscussionId == discussionId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Document>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Documents.AsNoTracking()
            .Where(d => d.StudentId == studentId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Document?> GetLatestVersionAsync(int projectId, string fileName, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Documents.AsNoTracking()
            .Where(d => d.ProjectId == projectId && d.FileName == fileName)
            .OrderByDescending(d => d.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
