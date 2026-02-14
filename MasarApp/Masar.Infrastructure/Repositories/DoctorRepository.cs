using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class DoctorRepository : EfRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<Doctor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLower();
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Doctors.FirstOrDefaultAsync(d => d.Email != null && d.Email.ToLower() == normalized, cancellationToken);
    }

    public async Task<List<Doctor>> GetWithDepartmentAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Doctors.AsNoTracking()
            .Include(d => d.College)
            .Include(d => d.Department)
            .ThenInclude(dep => dep!.College)
            .Include(d => d.DepartmentsHeaded)
            .ToListAsync(cancellationToken);
    }

    public async Task<Doctor?> GetWithDepartmentAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Doctors
            .Include(d => d.College)
            .Include(d => d.Department)
            .ThenInclude(dep => dep!.College)
            .Include(d => d.DepartmentsHeaded)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId, cancellationToken);
    }

    public async Task<List<Doctor>> GetByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Doctors.AsNoTracking()
            .Include(d => d.College)
            .Include(d => d.Department)
            .ThenInclude(dep => dep!.College)
            .Include(d => d.DepartmentsHeaded)
            .Where(d => d.DepartmentId == departmentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Doctor>> GetByCollegeAsync(int collegeId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Doctors.AsNoTracking()
            .Include(d => d.College)
            .Include(d => d.Department)
            .ThenInclude(dep => dep!.College)
            .Include(d => d.DepartmentsHeaded)
            .Where(d => d.CollegeId == collegeId)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Doctor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetWithDepartmentAsync(id, cancellationToken);
    }
}
