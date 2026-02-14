using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface ICollegeService
{
    Task<List<CollegeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CollegeDto>> AddAsync(CollegeDto dto, CancellationToken cancellationToken = default);
    Task<Result<CollegeDto>> UpdateAsync(CollegeDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
