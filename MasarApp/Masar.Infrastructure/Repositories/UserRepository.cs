using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class UserRepository : EfRepository<User>, IUserRepository
{
    public UserRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim().ToLower();
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized, cancellationToken);
    }

    public async Task<List<User>> GetWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Users.AsNoTracking()
            .Include(u => u.Doctor)
            .Include(u => u.Student)
            .ToListAsync(cancellationToken);
    }

    public override async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Users
            .Include(u => u.Doctor)
            .Include(u => u.Student)
            .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
    }
}
