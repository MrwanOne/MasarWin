using Masar.Application.Interfaces;
using Masar.Infrastructure.DbContext;
using Masar.Infrastructure.Interceptors;
using Masar.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Masar.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<AuditInterceptor>();

        services.AddDbContextFactory<MasarDbContext>((sp, options) =>
        {
            options.UseOracle(connectionString)
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddTransient<ICollegeRepository, CollegeRepository>();
        services.AddTransient<IDepartmentRepository, DepartmentRepository>();
        services.AddTransient<IDoctorRepository, DoctorRepository>();
        services.AddTransient<IStudentRepository, StudentRepository>();
        services.AddTransient<ITeamRepository, TeamRepository>();
        services.AddTransient<IProjectRepository, ProjectRepository>();
        services.AddTransient<ICommitteeRepository, CommitteeRepository>();
        services.AddTransient<IDiscussionRepository, DiscussionRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IAcademicTermRepository, AcademicTermRepository>();
        services.AddTransient<IStudentEvaluationRepository, StudentEvaluationRepository>();
        services.AddTransient<IEvaluationCriteriaRepository, EvaluationCriteriaRepository>();
        services.AddTransient<IProjectStatusHistoryRepository, ProjectStatusHistoryRepository>();
        services.AddTransient<IAuditLogRepository, AuditLogRepository>();
        services.AddTransient<IDocumentRepository, DocumentRepository>();

        return services;
    }
}
