using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DepartmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<DepartmentDto>> AddAsync(DepartmentDto dto, CancellationToken cancellationToken = default);
    Task<Result<DepartmentDto>> UpdateAsync(DepartmentDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> SetHeadOfDepartmentAsync(int departmentId, int doctorId, CancellationToken cancellationToken = default);
    Task<Result> ClearHeadOfDepartmentAsync(int departmentId, CancellationToken cancellationToken = default);
}
