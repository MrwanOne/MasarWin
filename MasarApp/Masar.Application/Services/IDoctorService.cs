using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface IDoctorService
{
    Task<List<DoctorDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DoctorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DoctorDto?> FindByFullNameAsync(string fullName, CancellationToken cancellationToken = default);
    Task<Result<DoctorDto>> AddAsync(DoctorDto dto, CancellationToken cancellationToken = default);
    Task<Result<DoctorDto>> UpdateAsync(DoctorDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
