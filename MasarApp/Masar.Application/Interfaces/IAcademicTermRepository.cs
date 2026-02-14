using Masar.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface IAcademicTermRepository : IRepository<AcademicTerm>
{
    Task<AcademicTerm?> GetActiveTermAsync(CancellationToken cancellationToken = default);
    Task<List<AcademicTerm>> GetAllOrderedAsync(CancellationToken cancellationToken = default);
    Task SetActiveTermAsync(int termId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int year, int semester, int? excludeTermId = null, CancellationToken cancellationToken = default);
}
