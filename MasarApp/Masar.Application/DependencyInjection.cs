using FluentValidation;
using Masar.Application.Interfaces;
using Masar.Application.Reporting;
using Masar.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Masar.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // تسجيل جميع المصادقات (Validators) تلقائياً من التجميع الحالي
        // Register all validators automatically from the current assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICommitteeService, CommitteeService>();
        services.AddScoped<ICollegeService, CollegeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IDoctorService, DoctorServiceV2>();
        services.AddScoped<IStudentService, StudentServiceV2>();
        services.AddScoped<IDiscussionService, DiscussionService>();
        services.AddScoped<IStudentEvaluationService, StudentEvaluationService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAcademicTermService, AcademicTermService>();
        
        // QuestPDF Academic Report Builder (stateless — Transient)
        services.AddTransient<IAcademicReportBuilder, AcademicReportBuilder>();

        return services;
    }
}
