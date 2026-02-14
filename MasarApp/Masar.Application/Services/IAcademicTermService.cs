using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface IAcademicTermService
{
    Task<List<AcademicTermDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AcademicTermDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AcademicTermDto?> GetActiveTermAsync(CancellationToken cancellationToken = default);
    Task<AcademicTermDto> AddAsync(AcademicTermDto dto, CancellationToken cancellationToken = default);
    Task<AcademicTermDto> UpdateAsync(AcademicTermDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task SetActiveTermAsync(int termId, CancellationToken cancellationToken = default);
}
