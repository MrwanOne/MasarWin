using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Masar.Application.Services;
using Masar.Domain.Enums;
using Masar.UI;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class ProjectsViewModel : PagedViewModel<ProjectDto>
{
    private readonly IProjectService _projectService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly ITeamService _teamService;
    private readonly IDoctorService _doctorService;
    private readonly ILocalizationService _localizationService;
    private readonly IToastService _toastService;
    private readonly IValidator<ProjectDto> _projectValidator;
    private readonly IAuditLogRepository _auditLogRepository;
    private ProjectStatus? _statusFilter;

    private ProjectDto? _selectedProject;
    public ProjectDto? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (SetProperty(ref _selectedProject, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                AcceptCommand.RaiseCanExecuteChanged();
                RejectCommand.RaiseCanExecuteChanged();
                OpenDocumentsCommand.RaiseCanExecuteChanged();
                AssignSupervisorCommand.RaiseCanExecuteChanged();
                ViewHistoryCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanManage => _sessionService.CurrentUser?.Role is UserRole.Admin or UserRole.HeadOfDepartment or UserRole.Supervisor;
    public bool CanApprove => _sessionService.CurrentUser?.Role is UserRole.Admin or UserRole.HeadOfDepartment;

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }
    public AsyncRelayCommand AcceptCommand { get; }
    public AsyncRelayCommand RejectCommand { get; }
    public AsyncRelayCommand OpenDocumentsCommand { get; }
    public AsyncRelayCommand AssignSupervisorCommand { get; }
    public AsyncRelayCommand ViewHistoryCommand { get; }

    public ProjectsViewModel(
        IProjectService projectService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        ITeamService teamService,
        IDoctorService doctorService,
        IDialogService dialogService,
        ISessionService sessionService,
        ILocalizationService localizationService,
        IToastService toastService,
        IValidator<ProjectDto> projectValidator,
        IAuditLogRepository auditLogLogRepository)
    {
        _projectService = projectService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _teamService = teamService;
        _doctorService = doctorService;
        _dialogService = dialogService;
        _sessionService = sessionService;
        _localizationService = localizationService;
        _toastService = toastService;
        _projectValidator = projectValidator;
        _auditLogRepository = auditLogLogRepository;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new RelayCommand(_ => AddProject(), _ => CanManage);
        EditCommand = new RelayCommand(_ => EditProject(), _ => CanManage && SelectedProject != null);
        DeleteCommand = new AsyncRelayCommand(DeleteProjectAsync, () => CanManage && SelectedProject != null);
        
        AcceptCommand = new AsyncRelayCommand(AcceptProjectAsync, () => CanApprove && SelectedProject?.Status == ProjectStatus.Proposed);
        RejectCommand = new AsyncRelayCommand(RejectProjectAsync, () => CanApprove && SelectedProject?.Status == ProjectStatus.Proposed);
        OpenDocumentsCommand = new AsyncRelayCommand(OpenDocumentsAsync, () => SelectedProject != null);
        AssignSupervisorCommand = new AsyncRelayCommand(AssignSupervisorAsync, () => CanManage && SelectedProject != null);
        ViewHistoryCommand = new AsyncRelayCommand(ViewHistoryAsync, () => CanManage && SelectedProject != null);
    }

    public void ApplyStatusFilter(ProjectStatus? status)
    {
        _statusFilter = status;
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        try
        {
            var projects = await _projectService.GetAllAsync();
            
            if (_statusFilter.HasValue)
            {
                projects = projects.Where(p => p.Status == _statusFilter.Value).ToList();
            }

            SetItems(projects);
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.ProjectManagement"));
        }
    }

    protected override bool FilterItem(ProjectDto item, string searchText)
    {
        return item.Title.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.Beneficiary.Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
    }

    private async void AddProject()
    {
        var vm = new ProjectEditViewModel(_projectService, _collegeService, _departmentService, _teamService, _doctorService, _dialogService, _localizationService, _projectValidator);
        var dialog = new ProjectDialog(vm);
        await vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private async void EditProject()
    {
        if (SelectedProject == null)
        {
            return;
        }

        var vm = new ProjectEditViewModel(_projectService, _collegeService, _departmentService, _teamService, _doctorService, _dialogService, _localizationService, _projectValidator, SelectedProject);
        var dialog = new ProjectDialog(vm);
        await vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private async Task DeleteProjectAsync()
    {
        if (SelectedProject == null)
        {
            return;
        }

        if (_dialogService.Confirm(_localizationService.GetString("Confirm.DeleteProject"), _localizationService.GetString("Title.ProjectManagement")))
        {
            var result = await _projectService.DeleteAsync(SelectedProject.ProjectId);
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.ProjectManagement"));
            }
        }
    }

    private async Task OpenDocumentsAsync()
    {
        if (SelectedProject == null) return;

        var services = ((App)System.Windows.Application.Current).ServiceProvider;
        
        var vm = new DocumentsViewModel(
            services.GetRequiredService<IDocumentService>(),
            _dialogService,
            _localizationService,
            _toastService,
            SelectedProject.ProjectId,
            "Project");

        var dialog = new DocumentsDialog { DataContext = vm };
        await vm.LoadAsync();
        _dialogService.ShowDialog(dialog);
    }

    private async Task AcceptProjectAsync()
    {
        if (SelectedProject == null)
        {
            return;
        }

        var vm = new ProjectAcceptViewModel(_doctorService, _dialogService, _localizationService, SelectedProject.DepartmentId);
        var dialog = new ProjectAcceptDialog(vm);
        await vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            var acceptResult = await _projectService.AcceptAsync(SelectedProject.ProjectId, vm.SelectedSupervisor?.DoctorId);
            if (acceptResult.IsSuccess)
            {
                _toastService.ShowSuccess(_localizationService.GetString("Success.AcceptProject"));
                await LoadAsync();
            }
            else
            {
                _toastService.ShowError(acceptResult.Message);
            }
        }
    }

    private async Task RejectProjectAsync()
    {
        if (SelectedProject == null)
        {
            return;
        }

        var title = _localizationService.GetString("Title.Project");
        var prompt = _localizationService.IsArabic ? "أدخل سبب الرفض:" : "Enter rejection reason:";
        var inputVm = new InputDialogViewModel(title, prompt);
        var inputDialog = new InputDialogWindow(inputVm);

        if (_dialogService.ShowDialog(inputDialog) != true || string.IsNullOrWhiteSpace(inputVm.InputText))
        {
            return;
        }

        var result = await _projectService.RejectAsync(SelectedProject.ProjectId, inputVm.InputText.Trim());
        if (result.IsSuccess)
        {
            _toastService.ShowSuccess(_localizationService.GetString("Success.RejectProject"));
            await LoadAsync();
        }
        else
        {
            _toastService.ShowError(result.Message);
        }
    }

    private async Task AssignSupervisorAsync()
    {
        if (SelectedProject == null)
        {
            return;
        }

        var vm = new ProjectAcceptViewModel(_doctorService, _dialogService, _localizationService, SelectedProject.DepartmentId);
        var dialog = new ProjectAcceptDialog(vm);
        await vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true && vm.SelectedSupervisor != null)
        {
            await AssignSupervisorAsync(vm.SelectedSupervisor.DoctorId);
        }
    }

    private async Task AssignSupervisorAsync(int supervisorId)
    {
        if (SelectedProject == null)
        {
            return;
        }

        var result = await _projectService.AssignSupervisorAsync(SelectedProject.ProjectId, supervisorId);
        if (result.IsSuccess)
        {
            _toastService.ShowSuccess(_localizationService.GetString("Success.AssignSupervisor"));
            await LoadAsync();
        }
        else
        {
            _toastService.ShowError(result.Message);
        }
    }

    private async Task ViewHistoryAsync()
    {
        if (SelectedProject == null) return;

        var vm = new EntityHistoryViewModel(_auditLogRepository, "Project", SelectedProject.ProjectId.ToString());
        var dialog = new EntityHistoryDialog { DataContext = vm };
        await vm.LoadAsync();
        _dialogService.ShowDialog(dialog);
    }
}
