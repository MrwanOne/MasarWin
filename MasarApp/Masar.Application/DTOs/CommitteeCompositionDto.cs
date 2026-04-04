using Masar.Domain.Enums;

namespace Masar.Application.DTOs;

/// <summary>
/// تشكيل اللجنة وأعضاؤها من VW_COMMITTEE_COMPOSITION
/// </summary>
public class CommitteeCompositionDto
{
    public int    CommitteeId     { get; set; }
    public string CommitteeName   { get; set; } = string.Empty;
    public System.DateTime CreatedAt { get; set; }

    // القسم والفصل
    public int?   DepartmentId     { get; set; }
    public string DepartmentNameAr { get; set; } = string.Empty;
    public int?   TermId           { get; set; }
    public string TermNameAr       { get; set; } = string.Empty;

    // العضو
    public int    DoctorId       { get; set; }
    public string MemberName     { get; set; } = string.Empty;
    public string Qualification  { get; set; } = string.Empty;
    public int    Rank           { get; set; }
    public int    MemberRole     { get; set; }

    public CommitteeMemberRole Role         => (CommitteeMemberRole)MemberRole;
    public AcademicRank        AcademicRank => (AcademicRank)Rank;
}
