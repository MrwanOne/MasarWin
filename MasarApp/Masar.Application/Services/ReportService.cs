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
    private readonly ICollegeRepository _colleges;
    private readonly IDepartmentRepository _departments;
    private readonly IDoctorRepository _doctors;

    public ReportService(
        IProjectRepository projects,
        ICollegeRepository colleges,
        IDepartmentRepository departments,
        IDoctorRepository doctors)
    {
        _projects = projects;
        _colleges = colleges;
        _departments = departments;
        _doctors = doctors;
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
            Title = await BuildTitleAsync(filter, cancellationToken),
            Projects = projects
        };
    }

    private async Task<string> BuildTitleAsync(ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var parts = new List<string> { "تقرير المشاريع" };

        if (filter.CollegeId.HasValue)
        {
            var college = await _colleges.GetByIdAsync(filter.CollegeId.Value, cancellationToken);
            parts.Add(college != null ? college.NameAr : $"كلية #{filter.CollegeId.Value}");
        }

        if (filter.DepartmentId.HasValue)
        {
            var dept = await _departments.GetByIdAsync(filter.DepartmentId.Value, cancellationToken);
            parts.Add(dept != null ? dept.NameAr : $"قسم #{filter.DepartmentId.Value}");
        }

        if (filter.Year.HasValue)
        {
            parts.Add($"سنة {filter.Year.Value}");
        }

        if (filter.Status.HasValue)
        {
            parts.Add(filter.Status.Value.ToString());
        }

        if (filter.SupervisorId.HasValue)
        {
            var doctor = await _doctors.GetByIdAsync(filter.SupervisorId.Value, cancellationToken);
            parts.Add(doctor != null ? $"المشرف: {doctor.FullName}" : $"المشرف #{filter.SupervisorId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(filter.ProjectName))
        {
            parts.Add($"مشروع: {filter.ProjectName}");
        }

        return string.Join(" - ", parts);
    }
}

