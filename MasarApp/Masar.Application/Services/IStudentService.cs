using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface IStudentService
{
    Task<List<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StudentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<StudentDto?> FindByFullNameAsync(string fullName, CancellationToken cancellationToken = default);
    Task<Result<StudentDto>> AddAsync(StudentDto dto, CancellationToken cancellationToken = default);
    Task<Result<StudentDto>> UpdateAsync(StudentDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<StudentDto>> AssignTeamAsync(int studentId, int? teamId, CancellationToken cancellationToken = default);
}
