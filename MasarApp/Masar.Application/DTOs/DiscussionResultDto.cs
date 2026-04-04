using System;

namespace Masar.Application.DTOs;

/// <summary>
/// نتائج جلسة مناقشة من VW_DISCUSSION_RESULTS
/// </summary>
public class DiscussionResultDto
{
    public int      DiscussionId           { get; set; }
    public DateTime StartTime              { get; set; }
    public DateTime EndTime                { get; set; }
    public string   Place                  { get; set; } = string.Empty;
    public decimal  SupervisorScore        { get; set; }
    public decimal  CommitteeScore         { get; set; }
    public decimal  FinalScore             { get; set; }
    public string?  ReportText             { get; set; }
    public DateTime CreatedAt              { get; set; }

    // الفريق
    public int    TeamId   { get; set; }
    public string TeamName { get; set; } = string.Empty;

    // اللجنة
    public int    CommitteeId   { get; set; }
    public string CommitteeName { get; set; } = string.Empty;

    // القسم والكلية
    public int    DepartmentId     { get; set; }
    public string DepartmentNameAr { get; set; } = string.Empty;
    public string CollegeNameAr    { get; set; } = string.Empty;

    // إحصاء الطلاب المُقيَّمين
    public int EvaluatedStudentsCount { get; set; }
}
