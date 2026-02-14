using Masar.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface IReportService
{
    Task<ReportResultDto> BuildProjectReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default);
}
