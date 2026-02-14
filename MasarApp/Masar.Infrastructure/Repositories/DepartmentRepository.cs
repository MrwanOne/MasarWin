using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class DepartmentRepository : EfRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<Department?> GetByNamesAsync(string nameAr, string nameEn, int collegeId, CancellationToken cancellationToken = default)
    {
        var ar = nameAr.Trim().ToLower();
        var en = nameEn.Trim().ToLower();
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Departments.FirstOrDefaultAsync(d =>
            d.CollegeId == collegeId &&
            (d.NameAr.ToLower() == ar || d.NameEn.ToLower() == en),
            cancellationToken);
    }

    public async Task<List<Department>> GetWithCollegeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Departments.AsNoTracking()
            .Include(d => d.College)
            .Include(d => d.HeadOfDepartment)
            .ToListAsync(cancellationToken);
    }

    public async Task<Department?> GetWithCollegeAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Departments
            .Include(d => d.College)
            .Include(d => d.HeadOfDepartment)
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId, cancellationToken);
    }

    public async Task<bool> SetHeadOfDepartmentInternalAsync(int departmentId, int doctorId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        
        var department = await context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId, cancellationToken);
        if (department == null) return false;

        var doctor = await context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId, cancellationToken);
        if (doctor == null) return false;

        // If department already has a HOD, clear that doctor's DepartmentId
        if (department.HeadOfDepartmentId.HasValue && department.HeadOfDepartmentId.Value != doctorId)
        {
            var oldHOD = await context.Doctors.FindAsync([department.HeadOfDepartmentId.Value], cancellationToken);
            if (oldHOD != null)
            {
                oldHOD.DepartmentId = null;
            }
        }

        // If this doctor is currently HOD of another department, clear that
        var currentHODDept = await context.Departments
            .FirstOrDefaultAsync(d => d.HeadOfDepartmentId == doctorId && d.DepartmentId != departmentId, cancellationToken);
        if (currentHODDept != null)
        {
            currentHODDept.HeadOfDepartmentId = null;
        }

        // Set the new assignments
        doctor.DepartmentId = departmentId;
        department.HeadOfDepartmentId = doctorId;

        // Save all changes in single transaction
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
