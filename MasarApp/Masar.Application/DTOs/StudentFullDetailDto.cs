using Masar.Domain.Enums;

namespace Masar.Application.DTOs;

/// <summary>
/// البيانات الشاملة للطالب من VW_STUDENT_FULL_DETAIL
/// </summary>
public class StudentFullDetailDto
{
    public int     StudentId      { get; set; }
    public string  StudentNumber  { get; set; } = string.Empty;
    public string  FullName       { get; set; } = string.Empty;
    public string? Email          { get; set; }
    public string? Phone          { get; set; }
    public string? Gender         { get; set; }
    public decimal Gpa            { get; set; }
    public int     Level          { get; set; }
    public int     Status         { get; set; }
    public int     EnrollmentYear { get; set; }

    // القسم والكلية
    public int    DepartmentId     { get; set; }
    public string DepartmentNameAr { get; set; } = string.Empty;
    public string DepartmentNameEn { get; set; } = string.Empty;
    public int    CollegeId        { get; set; }
    public string CollegeNameAr    { get; set; } = string.Empty;

    // الفريق
    public int?   TeamId   { get; set; }
    public string TeamName { get; set; } = string.Empty;

    // المشروع
    public int?   ProjectId              { get; set; }
    public string ProjectTitle           { get; set; } = string.Empty;
    public int?   ProjectStatus          { get; set; }
    public decimal ProjectCompletionRate { get; set; }

    public StudentStatus StudentStatus => (StudentStatus)Status;
}
