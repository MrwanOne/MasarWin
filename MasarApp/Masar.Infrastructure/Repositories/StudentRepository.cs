using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class StudentRepository : EfRepository<Student>, IStudentRepository
{
    public StudentRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<Student?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLower();
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Students.FirstOrDefaultAsync(s => s.Email != null && s.Email.ToLower() == normalized, cancellationToken);
    }

    public async Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        var normalized = studentNumber.Trim().ToLower();
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Students.FirstOrDefaultAsync(s => s.StudentNumber.ToLower() == normalized, cancellationToken);
    }

    public async Task<List<Student>> GetWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Students.AsNoTracking()
            .Include(s => s.Department)
            .ThenInclude(d => d!.College)
            .Include(s => s.Team)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Students
            .Include(s => s.Department)
            .ThenInclude(d => d!.College)
            .Include(s => s.Team)
            .FirstOrDefaultAsync(s => s.StudentId == id, cancellationToken);
    }
}
