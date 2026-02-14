using Masar.Domain.Enums;

namespace Masar.Application.DTOs;

public class ReportFilterDto
{
    public int? CollegeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? Year { get; set; }
    public ProjectStatus? Status { get; set; }
    public int? SupervisorId { get; set; }
    public string? ProjectName { get; set; }
}
