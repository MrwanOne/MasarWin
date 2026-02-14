using Masar.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<Department?> GetByNamesAsync(string nameAr, string nameEn, int collegeId, CancellationToken cancellationToken = default);
    Task<List<Department>> GetWithCollegeAsync(CancellationToken cancellationToken = default);
    Task<Department?> GetWithCollegeAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<bool> SetHeadOfDepartmentInternalAsync(int departmentId, int doctorId, CancellationToken cancellationToken = default);
}
