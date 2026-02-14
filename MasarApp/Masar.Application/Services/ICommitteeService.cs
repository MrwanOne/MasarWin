using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface ICommitteeService
{
    Task<List<CommitteeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CommitteeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<CommitteeDto>> AddAsync(CommitteeDto dto, CancellationToken cancellationToken = default);
    Task<Result<CommitteeDto>> UpdateAsync(CommitteeDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<CommitteeDto>> AssignDoctorAsync(int committeeId, int doctorId, bool isChair, CancellationToken cancellationToken = default);
    Task<Result> RemoveDoctorAsync(int committeeId, int doctorId, CancellationToken cancellationToken = default);
}
