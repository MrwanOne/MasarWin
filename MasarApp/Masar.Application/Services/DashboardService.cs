using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Masar.Domain.Enums;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IProjectRepository _projects;
    private readonly IStudentRepository _students;
    private readonly ICommitteeRepository _committees;

    public DashboardService(IProjectRepository projects, IStudentRepository students, ICommitteeRepository committees)
    {
        _projects = projects;
        _students = students;
        _committees = committees;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projects.GetAllAsync(cancellationToken);
        var students = await _students.GetAllAsync(cancellationToken);
        var committees = await _committees.GetAllAsync(cancellationToken);

        return new DashboardStatsDto
        {
            TotalProjects = projects.Count,
            ProposedProjects = projects.Count(p => p.Status == ProjectStatus.Proposed),
            ApprovedProjects = projects.Count(p => p.Status == ProjectStatus.Approved),
            CompletedProjects = projects.Count(p => p.Status == ProjectStatus.Completed),
            TotalStudents = students.Count,
            TotalCommittees = committees.Count
        };
    }
}
