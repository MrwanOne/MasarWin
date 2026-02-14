using Masar.Application.DTOs;
using Masar.Domain.Entities;

namespace Masar.Application.Services;

public static class MappingExtensions
{
    public static CollegeDto ToDto(this College entity)
    {
        return new CollegeDto
        {
            CollegeId = entity.CollegeId,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Code = entity.Code
        };
    }

    public static DepartmentDto ToDto(this Department entity)
    {
        return new DepartmentDto
        {
            DepartmentId = entity.DepartmentId,
            NameAr = entity.NameAr,
            NameEn = entity.NameEn,
            Code = entity.Code,
            CollegeId = entity.CollegeId,
            CollegeName = entity.College?.NameEn ?? entity.College?.NameAr ?? string.Empty,
            HeadOfDepartmentId = entity.HeadOfDepartmentId,
            HeadOfDepartmentName = entity.HeadOfDepartment?.FullName ?? string.Empty
        };
    }

    public static DoctorDto ToDto(this Doctor entity)
    {
        return new DoctorDto
        {
            DoctorId = entity.DoctorId,
            FullName = entity.FullName,
            Qualification = entity.Qualification,
            Gender = entity.Gender,
            Email = entity.Email ?? string.Empty,
            Phone = entity.Phone,
            DepartmentId = entity.DepartmentId,
            DepartmentName = entity.Department?.NameEn ?? entity.Department?.NameAr ?? string.Empty,
            CollegeId = entity.CollegeId,
            CollegeName = entity.College?.NameEn ?? entity.College?.NameAr ?? string.Empty,
            Rank = entity.Rank.ToString(),
            IsHeadOfDepartment = entity.DepartmentsHeaded != null && entity.DepartmentsHeaded.Any(),
            HeadOfDepartmentName = entity.DepartmentsHeaded?.FirstOrDefault()?.NameEn ?? entity.DepartmentsHeaded?.FirstOrDefault()?.NameAr ?? string.Empty,
            Specialization = entity.Specialization,
            MaxSupervisionCount = entity.MaxSupervisionCount,
            IsActive = entity.IsActive
        };
    }

    public static StudentDto ToDto(this Student entity)
    {
        return new StudentDto
        {
            StudentId = entity.StudentId,
            StudentNumber = entity.StudentNumber,
            FullName = entity.FullName,
            Email = entity.Email ?? string.Empty,
            Phone = entity.Phone,
            DepartmentId = entity.DepartmentId,
            DepartmentName = entity.Department?.NameEn ?? entity.Department?.NameAr ?? string.Empty,
            CollegeId = entity.Department?.CollegeId ?? 0,
            CollegeName = entity.Department?.College?.NameEn ?? entity.Department?.College?.NameAr ?? string.Empty,
            TeamId = entity.TeamId,
            TeamName = entity.Team?.Name ?? string.Empty,
            EnrollmentYear = entity.EnrollmentYear,
            Gender = entity.Gender,
            GPA = entity.GPA ?? 0,
            Level = entity.Level,
            Status = entity.Status
        };
    }

    public static TeamDto ToDto(this Team entity)
    {
        return new TeamDto
        {
            TeamId = entity.TeamId,
            Name = entity.Name,
            DepartmentId = entity.DepartmentId,
            DepartmentName = entity.Department?.NameEn ?? entity.Department?.NameAr ?? string.Empty,
            CollegeId = entity.Department?.CollegeId ?? 0,
            CollegeName = entity.Department?.College?.NameEn ?? entity.Department?.College?.NameAr ?? string.Empty,
            SupervisorId = entity.SupervisorId,
            SupervisorName = entity.Supervisor?.FullName ?? string.Empty,
            CommitteeId = entity.CommitteeId,
            CommitteeName = entity.Committee?.Name ?? string.Empty,
            StudentCount = entity.Students.Count,
            ProjectTitle = entity.Projects.FirstOrDefault()?.Title ?? string.Empty,
            StudentNames = string.Join(", ", entity.Students.Select(s => s.FullName)),
            StudentNumbers = string.Join(", ", entity.Students.Select(s => s.StudentNumber)),
            AcademicYear = GetAcademicYear(entity)
        };
    }

    private static int GetAcademicYear(Team entity)
    {
        // Try project year if available
        var project = entity.Projects.FirstOrDefault();
        if (project != null && project.ProposedAt.Year > 2000)
        {
            return project.ProposedAt.Year;
        }

        // Try committee year if team belongs to one
        if (entity.Committee?.Term != null && entity.Committee.Term.Year > 2000)
        {
            return entity.Committee.Term.Year;
        }

        // Fallback to student enrollment + predicted graduation
        var maxEnrollmentYear = entity.Students.Any() ? entity.Students.Max(s => s.EnrollmentYear) : 0;
        if (maxEnrollmentYear > 2000)
        {
            return maxEnrollmentYear + 4; // Assuming 4 year programs
        }

        return entity.CreatedAt.Year;
    }

    public static ProjectDto ToDto(this Project entity)
    {
        return new ProjectDto
        {
            ProjectId = entity.ProjectId,
            Title = entity.Title,
            Description = entity.Description,
            Beneficiary = entity.Beneficiary,
            Status = entity.Status,
            CompletionRate = entity.CompletionRate,
            ProposedAt = entity.ProposedAt,
            ApprovedAt = entity.ApprovedAt,
            RejectionReason = entity.RejectionReason,
            DepartmentId = entity.DepartmentId,
            DepartmentName = entity.Department?.NameEn ?? entity.Department?.NameAr ?? string.Empty,
            CollegeId = entity.Department?.CollegeId ?? 0,
            CollegeName = entity.Department?.College?.NameEn ?? entity.Department?.College?.NameAr ?? string.Empty,
            TeamId = entity.TeamId,
            TeamName = entity.Team?.Name ?? string.Empty,
            SupervisorId = entity.SupervisorId,
            SupervisorName = entity.Supervisor?.FullName ?? string.Empty,
            TermId = entity.TermId,
            TermName = entity.Term?.NameEn ?? entity.Term?.NameAr ?? string.Empty,
            Documents = entity.Documents?.Select(d => d.ToDto()).ToList() ?? new List<DocumentDto>()
        };
    }

    public static DocumentDto ToDto(this Document entity)
    {
        return new DocumentDto
        {
            DocumentId = entity.DocumentId,
            ProjectId = entity.ProjectId ?? 0,
            FileName = entity.FileName,
            ContentType = entity.ContentType,
            FileSize = entity.FileSize,
            Version = entity.Version,
            Status = entity.Status,
            Checksum = entity.Checksum,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            CreatedByName = entity.CreatedByUserId.ToString() // Placeholder until we have name mapping
        };
    }

    public static CommitteeDto ToDto(this Committee entity)
    {
        return new CommitteeDto
        {
            CommitteeId = entity.CommitteeId,
            Name = entity.Name,
            DepartmentId = entity.DepartmentId ?? 0,
            DepartmentName = entity.Department?.NameEn ?? entity.Department?.NameAr ?? string.Empty,
            CollegeId = entity.Department?.CollegeId ?? 0,
            CollegeName = entity.Department?.College?.NameEn ?? entity.Department?.College?.NameAr ?? string.Empty,
            TermId = entity.TermId,
            TermName = entity.Term?.NameEn ?? entity.Term?.NameAr ?? string.Empty,
            MemberCount = entity.Members.Count
        };
    }

    public static CommitteeMemberDto ToDto(this CommitteeMember entity)
    {
        return new CommitteeMemberDto
        {
            CommitteeId = entity.CommitteeId,
            DoctorId = entity.DoctorId,
            DoctorName = entity.Doctor?.FullName ?? string.Empty,
            Role = entity.Role
        };
    }

    public static DiscussionDto ToDto(this Discussion entity)
    {
        return new DiscussionDto
        {
            DiscussionId = entity.DiscussionId,
            TeamId = entity.TeamId,
            TeamName = entity.Team?.Name ?? string.Empty,
            CommitteeId = entity.CommitteeId,
            CommitteeName = entity.Committee?.Name ?? string.Empty,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Place = entity.Place,
            SupervisorScore = entity.SupervisorScore,
            CommitteeScore = entity.CommitteeScore,
            FinalScore = entity.FinalScore,
            ReportText = entity.ReportText,
            Documents = entity.Documents?.Select(d => d.ToDto()).ToList() ?? new List<DocumentDto>()
        };
    }

    public static UserDto ToDto(this User entity)
    {
        return new UserDto
        {
            UserId = entity.UserId,
            Username = entity.Username,
            Role = entity.Role,
            IsActive = entity.IsActive,
            DoctorId = entity.DoctorId,
            StudentId = entity.StudentId
        };
    }
}
