using Masar.Domain.Enums;
using System;

namespace Masar.Application.DTOs;

/// <summary>
/// بيانات المشروع الكاملة من VW_PROJECT_FULL_DETAIL
/// </summary>
public class ProjectFullDetailDto
{
    public int       ProjectId           { get; set; }
    public string    Title               { get; set; } = string.Empty;
    public string    Description         { get; set; } = string.Empty;
    public string    Beneficiary         { get; set; } = string.Empty;
    public int       Status              { get; set; }
    public decimal   CompletionRate      { get; set; }
    public DateTime  ProposedAt          { get; set; }
    public DateTime? ApprovedAt          { get; set; }
    public string?   RejectionReason     { get; set; }

    // القسم والكلية
    public int    DepartmentId     { get; set; }
    public string DepartmentNameAr { get; set; } = string.Empty;
    public string DepartmentNameEn { get; set; } = string.Empty;
    public int    CollegeId        { get; set; }
    public string CollegeNameAr    { get; set; } = string.Empty;
    public string CollegeNameEn    { get; set; } = string.Empty;

    // الفريق
    public int?   TeamId   { get; set; }
    public string TeamName { get; set; } = string.Empty;

    // المشرف
    public int?   SupervisorId   { get; set; }
    public string SupervisorName { get; set; } = string.Empty;

    // الفصل الدراسي
    public int?   TermId      { get; set; }
    public string TermNameAr  { get; set; } = string.Empty;
    public string TermNameEn  { get; set; } = string.Empty;

    /// <summary> حالة المشروع كـ enum </summary>
    public ProjectStatus ProjectStatus => (ProjectStatus)Status;
}
