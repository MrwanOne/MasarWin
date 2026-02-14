using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public class ReportService : IReportService
{
    private readonly IProjectRepository _projects;

    public ReportService(IProjectRepository projects)
    {
        _projects = projects;
    }

    public async Task<ReportResultDto> BuildProjectReportAsync(ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var list = await _projects.GetWithDetailsAsync(cancellationToken);
        IEnumerable<Domain.Entities.Project> query = list;

        if (filter.CollegeId.HasValue)
        {
            query = query.Where(p => p.Department?.CollegeId == filter.CollegeId.Value);
        }

        if (filter.DepartmentId.HasValue)
        {
            query = query.Where(p => p.DepartmentId == filter.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.ProjectName))
        {
            query = query.Where(p => p.Title.Contains(filter.ProjectName, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.Year.HasValue)
        {
            query = query.Where(p => p.ProposedAt.Year == filter.Year.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(p => p.Status == filter.Status.Value);
        }

        if (filter.SupervisorId.HasValue)
        {
            query = query.Where(p => p.SupervisorId == filter.SupervisorId.Value);
        }

        var projects = query.Select(p => p.ToDto()).ToList();
        return new ReportResultDto
        {
            Title = BuildTitle(filter),
            Projects = projects
        };
    }

    private static string BuildTitle(ReportFilterDto filter)
    {
        var parts = new List<string> { "Projects Report" };

        if (filter.CollegeId.HasValue)
        {
            parts.Add($"College #{filter.CollegeId.Value}");
        }

        if (filter.DepartmentId.HasValue)
        {
            parts.Add($"Department #{filter.DepartmentId.Value}");
        }

        if (filter.Year.HasValue)
        {
            parts.Add($"Year {filter.Year.Value}");
        }

        if (filter.Status.HasValue)
        {
            parts.Add(filter.Status.Value.ToString());
        }

        if (filter.SupervisorId.HasValue)
        {
            parts.Add($"Supervisor #{filter.SupervisorId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(filter.ProjectName))
        {
            parts.Add($"Project: {filter.ProjectName}");
        }

        return string.Join(" - ", parts);
    }
}
