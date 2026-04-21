using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.Domain.Enums;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class TeamsViewModel : PagedViewModel<TeamDto>
{
    private readonly ITeamService _teamService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly IDoctorService _doctorService;
    private readonly ICommitteeService _committeeService;
    private readonly IStudentService _studentService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;

    private IEnumerable<TeamDto> _allTeams = [];

    public ObservableCollection<DepartmentDto> Departments { get; } = new();

    private int _selectedDepartmentId;
    public int SelectedDepartmentId
    {
        get => _selectedDepartmentId;
        set
        {
            if (SetProperty(ref _selectedDepartmentId, value))
            {
                ApplyDepartmentFilter();
            }
        }
    }

    private TeamDto? _selectedTeam;
    public TeamDto? SelectedTeam
    {
        get => _selectedTeam;
        set
        {
            if (SetProperty(ref _selectedTeam, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanManage => _sessionService.CurrentUser?.Role is UserRole.Admin or UserRole.HeadOfDepartment or UserRole.Supervisor;

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public TeamsViewModel(
        ITeamService teamService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        IDoctorService doctorService,
        ICommitteeService committeeService,
        IStudentService studentService,
        IDialogService dialogService,
        ISessionService sessionService,
        ILocalizationService localizationService)
    {
        _teamService = teamService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _doctorService = doctorService;
        _committeeService = committeeService;
        _studentService = studentService;
        _dialogService = dialogService;
        _sessionService = sessionService;
        _localizationService = localizationService;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new RelayCommand(_ => AddTeam(), _ => CanManage);
        EditCommand = new RelayCommand(_ => EditTeam(), _ => CanManage && SelectedTeam != null);
        DeleteCommand = new RelayCommand(_ => DeleteTeam(), _ => CanManage && SelectedTeam != null);
    }

    public async Task LoadAsync()
    {
        try
        {
            // Load departments for filter
            Departments.Clear();
            var placeholder = new DepartmentDto { DepartmentId = 0, NameAr = _localizationService.GetString("Placeholder.AllDepartments"), NameEn = "All Departments" };
            Departments.Add(placeholder);

            var departments = await _departmentService.GetAllAsync();
            foreach (var dept in departments.OrderBy(d => d.NameAr))
            {
                Departments.Add(dept);
            }

            // إشعار WPF بإعادة تحديد "الكل" بعد إعادة بناء القائمة
            _selectedDepartmentId = 0;
            OnPropertyChanged(nameof(SelectedDepartmentId));

            // Load all teams
            _allTeams = await _teamService.GetAllAsync();
            
            // Apply filter
            ApplyDepartmentFilter();
            
            // Refresh button states
            AddCommand.RaiseCanExecuteChanged();
            EditCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Teams"));
        }
    }

    private void ApplyDepartmentFilter()
    {
        var filtered = _selectedDepartmentId == 0
            ? _allTeams
            : _allTeams.Where(t => t.DepartmentId == _selectedDepartmentId);
        
        SetItems(filtered.OrderBy(t => t.Name));
    }

    protected override bool FilterItem(TeamDto item, string searchText)
    {
        return item.Name.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.ProjectTitle.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.StudentNames.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.StudentNumbers.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.DepartmentName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.CollegeName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.SupervisorName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.CommitteeName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.AcademicYear.ToString().Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
    }

    private void AddTeam()
    {
        var vm = new TeamEditViewModel(_teamService, _collegeService, _departmentService, _doctorService, _committeeService, _studentService, _dialogService, _localizationService);
        var dialog = new TeamDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private void EditTeam()
    {
        if (SelectedTeam == null)
        {
            return;
        }

        var vm = new TeamEditViewModel(_teamService, _collegeService, _departmentService, _doctorService, _committeeService, _studentService, _dialogService, _localizationService, SelectedTeam);
        var dialog = new TeamDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private async void DeleteTeam()
    {
        if (SelectedTeam == null)
        {
            return;
        }

        if (_dialogService.Confirm(_localizationService.GetString("Confirm.DeleteTeam"), _localizationService.GetString("Title.Teams")))
        {
            var result = await _teamService.DeleteAsync(SelectedTeam.TeamId);
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Teams"));
            }
        }
    }
}

