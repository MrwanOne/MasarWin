using Masar.Domain.Common;
using Masar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System;

namespace Masar.Infrastructure.DbContext;

public class MasarDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public MasarDbContext(DbContextOptions<MasarDbContext> options) : base(options)
    {
    }

    public DbSet<College> Colleges => Set<College>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Committee> Committees => Set<Committee>();
    public DbSet<CommitteeMember> CommitteeMembers => Set<CommitteeMember>();
    public DbSet<Discussion> Discussions => Set<Discussion>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AcademicTerm> AcademicTerms => Set<AcademicTerm>();
    public DbSet<EvaluationCriteria> EvaluationCriteria => Set<EvaluationCriteria>();
    public DbSet<StudentEvaluation> StudentEvaluations => Set<StudentEvaluation>();
    public DbSet<CriteriaScore> CriteriaScores => Set<CriteriaScore>();
    public DbSet<ProjectStatusHistory> ProjectStatusHistories => Set<ProjectStatusHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("document");
            entity.HasKey(e => e.DocumentId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.DiscussionId).HasColumnName("discussion_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.ContentType).HasColumnName("content_type").HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.Version).HasColumnName("version").HasDefaultValue(1);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("Draft").IsRequired(false);
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(100).HasDefaultValue("General").IsRequired(false);
            entity.Property(e => e.Checksum).HasColumnName("checksum").HasMaxLength(64).IsRequired(false);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500).IsRequired(false);

            entity.HasOne(d => d.Project)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Discussion)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.DiscussionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Student)
                .WithMany()
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ... rest of the method ...
        // تطبيق مرشحات الحذف الناعم تلقائياً لجميع الكيانات
        // Apply soft delete filters automatically for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
                var isDeletedProperty = Expression.Call(null, propertyMethodInfo!, parameter, Expression.Constant("IsDeleted"));
                var compareExpression = Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));
                var lambda = Expression.Lambda(compareExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_log");
            entity.HasKey(e => e.AuditLogId);
            entity.Property(e => e.AuditLogId).HasColumnName("audit_log_id");
            entity.Property(e => e.EntityName).HasColumnName("entity_name").HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.EntityId).HasColumnName("entity_id").HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(50).IsRequired(false);
            entity.Property(e => e.OldValues).HasColumnName("old_values").IsRequired(false);
            entity.Property(e => e.NewValues).HasColumnName("new_values").IsRequired(false);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
        });

        modelBuilder.Entity<College>(entity =>
        {
            entity.ToTable("college");
            entity.HasKey(e => e.CollegeId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.CollegeId).HasColumnName("college_id");
            entity.Property(e => e.NameAr).HasColumnName("name_ar").HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasColumnName("name_en").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.NameAr).IsUnique();
            entity.HasIndex(e => e.NameEn).IsUnique();
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("department");
            entity.HasKey(e => e.DepartmentId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.NameAr).HasColumnName("name_ar").HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasColumnName("name_en").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired(false);
            entity.Property(e => e.CollegeId).HasColumnName("college_id");
            entity.Property(e => e.HeadOfDepartmentId).HasColumnName("head_of_department_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => new { e.CollegeId, e.NameAr }).IsUnique();
            entity.HasIndex(e => new { e.CollegeId, e.NameEn }).IsUnique();

            entity.HasOne(d => d.College)
                .WithMany(c => c.Departments)
                .HasForeignKey(d => d.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.HeadOfDepartment)
                .WithMany(h => h.DepartmentsHeaded)
                .HasForeignKey(d => d.HeadOfDepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.ToTable("doctor");
            entity.HasKey(e => e.DoctorId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Qualification).HasColumnName("qualification").HasMaxLength(150).IsRequired(false);
            entity.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(20).IsRequired(false);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50).IsRequired(false);
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.Specialization).HasColumnName("specialization").HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.MaxSupervisionCount).HasColumnName("max_supervision_count");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.CollegeId).HasColumnName("college_id");

            // Removed unique index on Email to allow optional emails
            // entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(d => d.College)
                .WithMany()
                .HasForeignKey(d => d.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Department)
                .WithMany(p => p.Doctors)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("student");
            entity.HasKey(e => e.StudentId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.StudentNumber).HasColumnName("student_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(20).IsRequired(false);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50).IsRequired(false);
            entity.Property(e => e.GPA).HasColumnName("gpa").HasPrecision(3, 2);
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.EnrollmentYear).HasColumnName("enrollment_year");

            entity.HasIndex(e => e.StudentNumber).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(d => d.Department)
                .WithMany(p => p.Students)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Team)
                .WithMany(p => p.Students)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("team");
            entity.HasKey(e => e.TeamId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.SupervisorId).HasColumnName("supervisor_id");
            entity.Property(e => e.CommitteeId).HasColumnName("committee_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(d => d.Department)
                .WithMany(p => p.Teams)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Supervisor)
                .WithMany(p => p.SupervisedTeams)
                .HasForeignKey(d => d.SupervisorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Committee)
                .WithMany(p => p.Teams)
                .HasForeignKey(d => d.CommitteeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("project");
            entity.HasKey(e => e.ProjectId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").IsRequired(false);
            entity.Property(e => e.Beneficiary).HasColumnName("beneficiary").HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CompletionRate).HasColumnName("completion_rate").HasPrecision(5, 2);
            entity.Property(e => e.DocumentationPath).HasColumnName("documentation_path").HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.ProposedAt).HasColumnName("proposed_at");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.SupervisorId).HasColumnName("supervisor_id");
            entity.Property(e => e.TermId).HasColumnName("term_id");

            entity.HasIndex(e => e.Title).IsUnique();

            entity.HasOne(d => d.Department)
                .WithMany(p => p.Projects)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Team)
                .WithMany(p => p.Projects)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Supervisor)
                .WithMany(p => p.SupervisedProjects)
                .HasForeignKey(d => d.SupervisorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Term)
                .WithMany(p => p.Projects)
                .HasForeignKey(d => d.TermId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Committee>(entity =>
        {
            entity.ToTable("committee");
            entity.HasKey(e => e.CommitteeId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.CommitteeId).HasColumnName("committee_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(e => e.DepartmentId).HasColumnName("department_id").IsRequired(false);
            entity.Property(e => e.TermId).HasColumnName("term_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(d => d.Department)
                .WithMany(p => p.Committees)
                .HasForeignKey(d => d.DepartmentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Term)
                .WithMany(p => p.Committees)
                .HasForeignKey(d => d.TermId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CommitteeMember>(entity =>
        {
            entity.ToTable("committee_member");
            entity.HasKey(e => new { e.CommitteeId, e.DoctorId });
            ConfigureBaseEntity(entity);
            entity.Property(e => e.CommitteeId).HasColumnName("committee_id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.Role).HasColumnName("role");

            entity.HasOne(d => d.Committee)
                .WithMany(p => p.Members)
                .HasForeignKey(d => d.CommitteeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Doctor)
                .WithMany(p => p.CommitteeMemberships)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Discussion>(entity =>
        {
            entity.ToTable("discussion");
            entity.HasKey(e => e.DiscussionId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.DiscussionId).HasColumnName("discussion_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.CommitteeId).HasColumnName("committee_id");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.Place).HasColumnName("place").HasMaxLength(200).IsRequired();
            entity.Property(e => e.SupervisorScore).HasColumnName("supervisor_score").HasPrecision(5, 2);
            entity.Property(e => e.CommitteeScore).HasColumnName("committee_score").HasPrecision(5, 2);
            entity.Property(e => e.FinalScore).HasColumnName("final_score").HasPrecision(5, 2);
            entity.Property(e => e.ReportText).HasColumnName("report_text").IsRequired(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(d => d.Team)
                .WithMany(p => p.Discussions)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Committee)
                .WithMany(p => p.Discussions)
                .HasForeignKey(d => d.CommitteeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            entity.Property(e => e.PasswordSalt).HasColumnName("password_salt").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasIndex(e => e.Username).IsUnique();

            entity.HasOne(d => d.Doctor)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Student)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AcademicTerm>(entity =>
        {
            entity.ToTable("academic_term");
            entity.HasKey(e => e.TermId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.TermId).HasColumnName("term_id");
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Semester).HasColumnName("semester");
            entity.Property(e => e.NameAr).HasColumnName("name_ar").HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasColumnName("name_en").HasMaxLength(200).IsRequired();
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => new { e.Year, e.Semester }).IsUnique();
        });

        modelBuilder.Entity<EvaluationCriteria>(entity =>
        {
            entity.ToTable("evaluation_criteria");
            entity.HasKey(e => e.CriteriaId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.CriteriaId).HasColumnName("criteria_id");
            entity.Property(e => e.NameAr).HasColumnName("name_ar").HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasColumnName("name_en").HasMaxLength(200).IsRequired();
            entity.Property(e => e.DescriptionAr).HasColumnName("description_ar").IsRequired(false);
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en").IsRequired(false);
            entity.Property(e => e.MaxScore).HasColumnName("max_score").HasPrecision(5, 2);
            entity.Property(e => e.Weight).HasColumnName("weight").HasPrecision(5, 4);
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");

            entity.HasOne(d => d.Department)
                .WithMany()
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StudentEvaluation>(entity =>
        {
            entity.ToTable("student_evaluation");
            entity.HasKey(e => e.EvaluationId);
            ConfigureBaseEntity(entity);
            entity.Property(e => e.EvaluationId).HasColumnName("evaluation_id");
            entity.Property(e => e.DiscussionId).HasColumnName("discussion_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TotalScore).HasColumnName("total_score").HasPrecision(5, 2);
            entity.Property(e => e.ContributionPercentage).HasColumnName("contribution_percentage").HasPrecision(5, 2);
            entity.Property(e => e.GeneralFeedback).HasColumnName("general_feedback").IsRequired(false);
            entity.Property(e => e.StrengthPoints).HasColumnName("strength_points").IsRequired(false);
            entity.Property(e => e.ImprovementAreas).HasColumnName("improvement_areas").IsRequired(false);

            entity.HasIndex(e => new { e.DiscussionId, e.StudentId }).IsUnique();

            entity.HasOne(d => d.Discussion)
                .WithMany(p => p.StudentEvaluations)
                .HasForeignKey(d => d.DiscussionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Student)
                .WithMany(p => p.Evaluations)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CriteriaScore>(entity =>
        {
            entity.ToTable("criteria_score");
            entity.HasKey(e => e.ScoreId);
            entity.Property(e => e.ScoreId).HasColumnName("score_id");
            entity.Property(e => e.EvaluationId).HasColumnName("evaluation_id");
            entity.Property(e => e.CriteriaId).HasColumnName("criteria_id");
            entity.Property(e => e.Score).HasColumnName("score").HasPrecision(5, 2);
            entity.Property(e => e.Comments).HasColumnName("comments").IsRequired(false);

            entity.HasIndex(e => new { e.EvaluationId, e.CriteriaId }).IsUnique();

            entity.HasOne(d => d.Evaluation)
                .WithMany(p => p.CriteriaScores)
                .HasForeignKey(d => d.EvaluationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Criteria)
                .WithMany(p => p.Scores)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectStatusHistory>(entity =>
        {
            entity.ToTable("project_status_history");
            entity.HasKey(e => e.HistoryId);
            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.OldStatus).HasColumnName("old_status");
            entity.Property(e => e.NewStatus).HasColumnName("new_status");
            entity.Property(e => e.ChangedByUserId).HasColumnName("changed_by_user_id");
            entity.Property(e => e.ChangeReason).HasColumnName("change_reason").HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");

            entity.HasOne(d => d.Project)
                .WithMany()
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.ChangedByUser)
                .WithMany()
                .HasForeignKey(d => d.ChangedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureBaseEntity<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> entity) where T : BaseEntity
    {
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
    }
}
