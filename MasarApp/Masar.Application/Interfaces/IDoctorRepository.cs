using Masar.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface IDoctorRepository : IRepository<Doctor>
{
    Task<List<Doctor>> GetWithDepartmentAsync(CancellationToken cancellationToken = default);
    Task<Doctor?> GetWithDepartmentAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<List<Doctor>> GetByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<List<Doctor>> GetByCollegeAsync(int collegeId, CancellationToken cancellationToken = default);
    Task<Doctor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
