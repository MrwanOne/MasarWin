using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.Domain.Enums;
using Masar.UI.Controls;
using Masar.UI.Models;
using Masar.UI.Services;
using FluentValidation;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class ProjectEditViewModel : DialogViewModel
{
    private readonly IProjectService _projectService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly ITeamService _teamService;
    private readonly IDoctorService _doctorService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly IValidator<ProjectDto> _validator;

    public ObservableCollection<StatusOption> StatusOptions { get; } = new();
    public ObservableCollection<CollegeDto> Colleges { get; } = new();
    public ObservableCollection<DepartmentDto> Departments { get; } = new();
    public ObservableCollection<TeamDto> Teams { get; } = new();
    public ObservableCollection<DoctorDto> Supervisors { get; } = new();

    private ProjectDto _project = new();
    public ProjectDto Project
    {
        get => _project;
        set => SetProperty(ref _project, value);
    }

    public string Title
    {
        get => Project.Title;
        set
        {
            Project.Title = value;
            OnPropertyChanged();
            ValidateProject();
        }
    }

    public string? Beneficiary
    {
        get => Project.Beneficiary;
        set
        {
            Project.Beneficiary = value;
            OnPropertyChanged();
            ValidateProject();
        }
    }

    public decimal CompletionRate
    {
        get => Project.CompletionRate;
        set
        {
            Project.CompletionRate = value;
            OnPropertyChanged();
            ValidateProject();
        }
    }

    private int _selectedCollegeId;
    public int SelectedCollegeId
    {
        get => _selectedCollegeId;
        set
        {
            if (SetProperty(ref _selectedCollegeId, value))
            {
                NotifyTask.Create(LoadDepartmentsAsync());
            }
        }
    }

    private int _selectedDepartmentId;
    public int SelectedDepartmentId
    {
        get => _selectedDepartmentId;
        set
        {
            if (SetProperty(ref _selectedDepartmentId, value))
            {
                NotifyTask.Create(LoadTeamsAndSupervisorsAsync());
            }
        }
    }

    private ProjectStatus? _selectedStatus;
    public ProjectStatus? SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    public bool IsEditMode { get; }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public ProjectEditViewModel(
        IProjectService projectService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        ITeamService teamService,
        IDoctorService doctorService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        IValidator<ProjectDto> validator,
        ProjectDto? project = null)
    {
        _projectService = projectService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _teamService = teamService;
        _doctorService = doctorService;
        _dialogService = dialogService;
        _localizationService = localizationService;
        _validator = validator;
        Project = project ?? new ProjectDto { Status = ProjectStatus.Proposed };
        IsEditMode = project != null;
        SelectedStatus = IsEditMode ? Project.Status : null;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(_ => Close(false));
        _localizationService.LanguageChanged += OnLanguageChanged;
        RefreshStatuses();
    }

    public async Task LoadAsync()
    {
        try
        {
            await LoadCollegesAsync();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Project"));
        }
    }

    private async Task LoadCollegesAsync()
    {
        try
        {
            Colleges.Clear();
            var placeholder = Placeholder("Placeholder.SelectCollege");
            Colleges.Add(new CollegeDto { CollegeId = 0, NameEn = placeholder, NameAr = placeholder });
            var collegesFromDb = await _collegeService.GetAllAsync();
            foreach (var college in collegesFromDb)
            {
                Colleges.Add(college);
            }

            // Set the college ID - use the backing field to avoid triggering twice
            _selectedCollegeId = Project.CollegeId != 0 ? Project.CollegeId : 0;
            OnPropertyChanged(nameof(SelectedCollegeId));
            
            // Always load departments after colleges are loaded
            await LoadDepartmentsAsync();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Project"));
        }
    }

    private void RefreshStatuses()
    {
        var current = SelectedStatus;
        StatusOptions.Clear();
        StatusOptions.Add(new StatusOption(null, Placeholder("Placeholder.SelectStatus")));
        foreach (var option in _localizationService.GetStatusOptions())
        {
            StatusOptions.Add(option);
        }
        SelectedStatus = current ?? (IsEditMode ? Project.Status : null);
    }

    private void OnLanguageChanged(object? sender, System.EventArgs e)
    {
        RefreshStatuses();
        NotifyTask.Create(LoadCollegesAsync());
        NotifyTask.Create(LoadDepartmentsAsync());
        NotifyTask.Create(LoadTeamsAsync());
        NotifyTask.Create(LoadSupervisorsAsync());
    }

    private string Placeholder(string resourceKey) => _localizationService.GetString(resourceKey);

    private async Task LoadDepartmentsAsync()
    {
        try
        {
            Departments.Clear();
            var placeholder = Placeholder("Placeholder.SelectDepartment");
            Departments.Add(new DepartmentDto { DepartmentId = 0, NameEn = placeholder, NameAr = placeholder });
            var departments = await _departmentService.GetAllAsync();
            var filtered = departments
                .Where(d => d.CollegeId == SelectedCollegeId)
                .GroupBy(d => d.DepartmentId)
                .Select(g => g.First())
                .OrderBy(d => d.NameEn)
                .ToList();

            foreach (var dept in filtered)
            {
                Departments.Add(dept);
            }

            // Set the department ID - use the backing field to avoid triggering twice
            _selectedDepartmentId = Project.DepartmentId != 0 ? Project.DepartmentId : 0;
            OnPropertyChanged(nameof(SelectedDepartmentId));
            
            // Always load teams and supervisors after departments are loaded
            await LoadTeamsAndSupervisorsAsync();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Project"));
        }
    }

    private async Task LoadTeamsAndSupervisorsAsync()
    {
        await LoadTeamsAsync();
        await LoadSupervisorsAsync();
    }

    private async Task LoadTeamsAsync()
    {
        try
        {
            Teams.Clear();
            var placeholder = Placeholder("Placeholder.SelectTeam");
            Teams.Add(new TeamDto { TeamId = 0, Name = placeholder, ProjectTitle = placeholder });
            var teams = await _teamService.GetAllAsync();
            var filtered = teams
                .Where(t => t.DepartmentId == SelectedDepartmentId)
                .GroupBy(t => t.TeamId)
                .Select(g => g.First())
                .OrderBy(t => t.Name)
                .ToList();

            foreach (var team in filtered)
            {
                Teams.Add(team);
            }

            Project.TeamId ??= 0;
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Project"));
        }
    }

    private async Task LoadSupervisorsAsync()
    {
        try
        {
            Supervisors.Clear();
            var placeholder = Placeholder("Placeholder.SelectSupervisor");
            Supervisors.Add(new DoctorDto { DoctorId = 0, FullName = placeholder });
            var doctors = await _doctorService.GetAllAsync();
            var filtered = doctors
                .Where(d => d.CollegeId == SelectedCollegeId)
                .GroupBy(d => d.DoctorId)
                .Select(g => g.First())
                .OrderBy(d => d.FullName);

            foreach (var doctor in filtered)
            {
                Supervisors.Add(doctor);
            }

            Project.SupervisorId ??= 0;
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Project"));
        }
    }

    private void ValidateProject()
    {
        ClearErrors();
        var result = _validator.Validate(Project);
        foreach (var error in result.Errors)
        {
            AddError(error.PropertyName, error.ErrorMessage);
        }
    }

    private async Task SaveAsync()
    {

        // Assign selected values BEFORE validation so the validator sees them
        Project.CollegeId = SelectedCollegeId;
        Project.DepartmentId = SelectedDepartmentId;
        if (SelectedStatus.HasValue)
            Project.Status = SelectedStatus.Value;

        ValidateProject();
        if (HasErrors)
        {
            // Show the first validation error to the user
            var validationResult = _validator.Validate(Project);
            var firstError = validationResult.Errors.FirstOrDefault()?.ErrorMessage;
            if (firstError != null)
            {
                _dialogService.ShowError(firstError, _localizationService.GetString("Title.Project"));
            }
            return;
        }

        try
        {
            if (SelectedStatus == null)
            {
                _dialogService.ShowError(Placeholder("Placeholder.SelectStatus"), _localizationService.GetString("Title.Project"));
                return;
            }

            Project.TeamId = Project.TeamId == 0 ? null : Project.TeamId;
            Project.SupervisorId = Project.SupervisorId == 0 ? null : Project.SupervisorId;
            
            Result<ProjectDto> result;
            if (IsEditMode)
            {
                result = await _projectService.UpdateAsync(Project);
            }
            else
            {
                result = await _projectService.AddAsync(Project);
            }

            if (result.IsSuccess)
            {
                Project = result.Value!;
                Close(true);
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Project"));
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Project"));
        }
    }
}
