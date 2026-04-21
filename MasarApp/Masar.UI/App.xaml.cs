using Masar.Application;
using Masar.Application.Interfaces;
using Masar.Application.Reporting;
using Masar.Application.Services;
using Masar.Infrastructure;
using Masar.Infrastructure.DbContext;
using Masar.Infrastructure.Seed;
using Masar.UI.Services;
using Masar.UI.ViewModels;
using Masar.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Masar.UI;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    public IServiceProvider ServiceProvider => _host.Services;

    public App()
    {
        try 
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration.GetConnectionString("MasarDb");
                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        throw new InvalidOperationException("Connection string 'MasarDb' is not configured.");
                    }

                    services.AddInfrastructure(connectionString);
                    services.AddApplication();

                    services.AddSingleton<IPasswordHasher, PasswordHasher>();

                    // UI Services...

                    // Sprint 1.1: آلة حالة المشروع
                    services.AddSingleton<IProjectStateMachine, ProjectStateMachine>();

                    services.AddSingleton<IDialogService, DialogService>();
                    // Sprint 1.1: خدمة Toast للإشعارات
                    services.AddSingleton<IToastService, ToastService>();
                    services.AddSingleton<ISessionService, SessionService>();
                    services.AddSingleton<ICurrentUserService, CurrentUserService>();
                    services.AddSingleton<ILocalizationService, LocalizationService>();
                    services.AddSingleton<ReportDocumentBuilder>();
                    services.AddSingleton<IExcelImportService, ExcelImportService>();

                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<ProjectsViewModel>();
                    services.AddTransient<CollegesViewModel>();
                    services.AddTransient<DepartmentsViewModel>();
                    services.AddTransient<DoctorsViewModel>();
                    services.AddTransient<StudentsViewModel>();
                    services.AddTransient<TeamsViewModel>();
                    services.AddTransient<CommitteesViewModel>();
                    services.AddTransient<DiscussionsViewModel>();
                    services.AddTransient<ReportsViewModel>();
                    services.AddTransient<UsersViewModel>();
                    services.AddTransient<AcademicTermsViewModel>();
                    services.AddTransient<AuditLogViewModel>();
                    services.AddTransient<EntityHistoryViewModel>();
                    services.AddTransient<DocumentsViewModel>();

                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainWindow>();
                })
                .Build();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Critical Configuration Error: {ex.Message}\n{ex.StackTrace}", "Masar Startup Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try 
        {
            // Initialize QuestPDF - must be called before any PDF generation
            QuestPdfConfiguration.Initialize();
            
            LoggingConfiguration.Configure();
            Serilog.Log.Information("Application starting...");

            if (_host == null) throw new InvalidOperationException("Host failed to initialize.");

            await _host.StartAsync();

            using var scope = _host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var localizationService = services.GetRequiredService<ILocalizationService>();

            // ربط خدمة اللغة بالـ Converter حتى يعرض الاسم الصحيح حسب اللغة
            Masar.UI.Converters.LocalizedNameConverter.LocalizationService = localizationService;

            // ─── Migration ───────────────────────────────────────────────
            var dbFactory = services.GetRequiredService<IDbContextFactory<MasarDbContext>>();
            try
            {
                await using var dbContext = await dbFactory.CreateDbContextAsync();
                await dbContext.Database.MigrateAsync();
                Serilog.Log.Information("Database migration completed.");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Database migration failed.");
                MessageBox.Show($"Migration Error:\n{ex.Message}", "Masar - Migration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // ─── Views ───────────────────────────────────────────────────
            try
            {
                await DatabaseViewsInitializer.InitializeAsync(dbFactory);
                Serilog.Log.Information("Database Views initialized.");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "DatabaseViewsInitializer failed.");
                MessageBox.Show($"Views Init Error:\n{ex.Message}", "Masar - Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // ─── Functions, Procedures, Triggers ─────────────────────────
            try
            {
                await DatabaseProceduresInitializer.InitializeAsync(dbFactory);
                Serilog.Log.Information("Database Procedures/Functions/Triggers initialized.");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "DatabaseProceduresInitializer failed.");
                MessageBox.Show($"Procedures Init Error:\n{ex.Message}", "Masar - Procedures", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // ─── Seed ─────────────────────────────────────────────────────
            try
            {
                var passwordHasher = services.GetRequiredService<IPasswordHasher>();
                await DbSeeder.SeedAdminAsync(dbFactory, passwordHasher);
                Serilog.Log.Information("Database seeding completed.");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Database seeding failed.");
                var message = string.Format(localizationService?.GetString("Error.DatabaseInit") ?? "DB Error: {0}", ex.Message);
                MessageBox.Show(message, "Masar", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            var login = _host.Services.GetRequiredService<LoginWindow>();
            login.Show();

            Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Serilog.Log.Fatal(ex, "Application startup failed.");
            MessageBox.Show($"Critical Startup Error: {ex.Message}\n{ex.StackTrace}", "Masar", MessageBoxButton.OK, MessageBoxImage.Error);
            System.Windows.Application.Current.Shutdown(1);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleException(e.Exception);
        e.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            HandleException(ex);
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleException(e.Exception);
        e.SetObserved();
    }

    private void HandleException(Exception ex)
    {
        try
        {
            var localizationService = _host.Services.GetService<ILocalizationService>();
            var message = localizationService?.GetString("Error.Unhandled") ?? "An unexpected error occurred: ";
            var title = localizationService?.GetString("App.Name") ?? "Masar";

            MessageBox.Show($"{message}\n\n{ex.Message}", title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch
        {
            MessageBox.Show($"Critical error: {ex.Message}", "Masar", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host.StopAsync().Wait();
        _host.Dispose();
        base.OnExit(e);
    }
}
