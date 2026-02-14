using Masar.Domain.Enums;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ISessionService _sessionService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly CollegesViewModel _collegesViewModel;
    private readonly DepartmentsViewModel _departmentsViewModel;
    private readonly DoctorsViewModel _doctorsViewModel;
    private readonly ProjectsViewModel _projectsViewModel;
    private readonly StudentsViewModel _studentsViewModel;
    private readonly TeamsViewModel _teamsViewModel;
    private readonly CommitteesViewModel _committeesViewModel;
    private readonly DiscussionsViewModel _discussionsViewModel;
    private readonly EvaluationsViewModel _evaluationsViewModel;
    private readonly ReportsViewModel _reportsViewModel;
    private readonly UsersViewModel _usersViewModel;
    private readonly AcademicTermsViewModel _academicTermsViewModel;
    private readonly AuditLogViewModel _auditLogViewModel;

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; } = new();

    private NavigationItemViewModel? _selectedNavigation;
    public NavigationItemViewModel? SelectedNavigation
    {
        get => _selectedNavigation;
        set
        {
            if (SetProperty(ref _selectedNavigation, value) && value != null)
            {
                CurrentViewModel = value.ViewModel;
                NotifyTask.Create(RefreshCurrentViewModel(value.ViewModel));
            }
        }
    }

    private async Task RefreshCurrentViewModel(ViewModelBase viewModel)
    {
        if (viewModel is DashboardViewModel dvm) await dvm.LoadAsync();
        else if (viewModel is ProjectsViewModel pvm) await pvm.LoadAsync();
        else if (viewModel is StudentsViewModel svm) await svm.LoadAsync();
        else if (viewModel is TeamsViewModel tvm) await tvm.LoadAsync();
        else if (viewModel is DoctorsViewModel docvm) await docvm.LoadAsync();
        else if (viewModel is DepartmentsViewModel deptvm) await deptvm.LoadAsync();
        else if (viewModel is CollegesViewModel colvm) await colvm.LoadAsync();
        else if (viewModel is CommitteesViewModel comvm) await comvm.LoadAsync();
        else if (viewModel is DiscussionsViewModel disvm) await disvm.LoadAsync();
        else if (viewModel is EvaluationsViewModel evm) await evm.LoadAsync();
        else if (viewModel is ReportsViewModel rvm) await rvm.LoadAsync();
        else if (viewModel is UsersViewModel uvm) await uvm.LoadAsync();
        else if (viewModel is AcademicTermsViewModel atvm) await atvm.LoadAsync();
        else if (viewModel is AuditLogViewModel alvm) await alvm.LoadAsync();
    }

    private ViewModelBase? _currentViewModel;
    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    private string _displayName = string.Empty;
    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    private string _roleName = string.Empty;
    public string RoleName
    {
        get => _roleName;
        set => SetProperty(ref _roleName, value);
    }

    public RelayCommand LogoutCommand { get; }
    public RelayCommand ToggleLanguageCommand { get; }
    public RelayCommand ShowAboutCommand { get; }

    public event EventHandler? LogoutRequested;

    public MainViewModel(
        ISessionService sessionService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        DashboardViewModel dashboardViewModel,
        CollegesViewModel collegesViewModel,
        DepartmentsViewModel departmentsViewModel,
        DoctorsViewModel doctorsViewModel,
        ProjectsViewModel projectsViewModel,
        StudentsViewModel studentsViewModel,
        TeamsViewModel teamsViewModel,
        CommitteesViewModel committeesViewModel,
        DiscussionsViewModel discussionsViewModel,
        EvaluationsViewModel evaluationsViewModel,
        ReportsViewModel reportsViewModel,
        UsersViewModel usersViewModel,
        AcademicTermsViewModel academicTermsViewModel,
        AuditLogViewModel auditLogViewModel)
    {
        _sessionService = sessionService;
        _dialogService = dialogService;
        _localizationService = localizationService;
        _dashboardViewModel = dashboardViewModel;
        _collegesViewModel = collegesViewModel;
        _departmentsViewModel = departmentsViewModel;
        _doctorsViewModel = doctorsViewModel;
        _projectsViewModel = projectsViewModel;
        _studentsViewModel = studentsViewModel;
        _teamsViewModel = teamsViewModel;
        _committeesViewModel = committeesViewModel;
        _discussionsViewModel = discussionsViewModel;
        _evaluationsViewModel = evaluationsViewModel;
        _reportsViewModel = reportsViewModel;
        _usersViewModel = usersViewModel;
        _academicTermsViewModel = academicTermsViewModel;
        _auditLogViewModel = auditLogViewModel;
        LogoutCommand = new RelayCommand(_ => Logout(), _ => _sessionService.CurrentUser != null);
        ToggleLanguageCommand = new RelayCommand(_ => _localizationService.ToggleLanguage());
        ShowAboutCommand = new RelayCommand(_ => ShowAbout());

        _dashboardViewModel.ProjectsDrillRequested += OnProjectsDrillRequested;
        _dashboardViewModel.StudentsDrillRequested += OnStudentsDrillRequested;
        _dashboardViewModel.CommitteesDrillRequested += OnCommitteesDrillRequested;

        _localizationService.LanguageChanged += OnLanguageChanged;
        ConfigureNavigation();
    }

    private void ConfigureNavigation()
    {
        NavigationItems.Clear();

        var role = _sessionService.CurrentUser?.Role ?? UserRole.Student;
        DisplayName = _sessionService.CurrentUser?.Username ?? _localizationService.GetString("User.Guest");
        RoleName = GetRoleName(role);

        var selectedViewModel = SelectedNavigation?.ViewModel;

        AddItem(_localizationService.GetString("Nav.Dashboard"), "\uE80F", _dashboardViewModel, true);
        AddItem(_localizationService.GetString("Nav.Colleges"), "\uE7BE", _collegesViewModel, role == UserRole.Admin);
        AddItem(_localizationService.GetString("Nav.Departments"), "\uE8EF", _departmentsViewModel, role == UserRole.Admin); // HoD cannot manage departments
        AddItem(_localizationService.GetString("Nav.Doctors"), "\uE8D4", _doctorsViewModel, role is UserRole.Admin or UserRole.HeadOfDepartment);
        AddItem(_localizationService.GetString("Nav.Projects"), "\uE7C3", _projectsViewModel, role != UserRole.Student);
        AddItem(_localizationService.GetString("Nav.Students"), "\uE77B", _studentsViewModel, role is UserRole.Admin or UserRole.HeadOfDepartment);
        AddItem(_localizationService.GetString("Nav.Teams"), "\uE902", _teamsViewModel, role is UserRole.Admin or UserRole.HeadOfDepartment or UserRole.Supervisor);
        AddItem(_localizationService.GetString("Nav.Committees"), "\uEAFD", _committeesViewModel, role is UserRole.Admin or UserRole.HeadOfDepartment);
        AddItem(_localizationService.GetString("Nav.Discussions"), "\uE70B", _discussionsViewModel, role != UserRole.Student);
        AddItem(_localizationService.GetString("Nav.Evaluations"), "\uE8D7", _evaluationsViewModel, role is UserRole.Admin or UserRole.Supervisor or UserRole.HeadOfDepartment);
        AddItem(_localizationService.GetString("Nav.Reports"), "\uE9D2", _reportsViewModel, role != UserRole.Student);
        AddItem(_localizationService.GetString("Nav.Users"), "\uE8FA", _usersViewModel, role == UserRole.Admin);
        AddItem(_localizationService.GetString("Nav.AcademicTerms"), "\uE7BE", _academicTermsViewModel, role == UserRole.Admin);
        AddItem(_localizationService.GetString("Nav.AuditLog"), "\uE7BA", _auditLogViewModel, role == UserRole.Admin);

        SelectedNavigation = NavigationItems.FirstOrDefault(i => i.ViewModel == selectedViewModel)
            ?? NavigationItems.FirstOrDefault(i => i.IsVisible);
    }

    private void AddItem(string title, string icon, ViewModelBase viewModel, bool visible)
    {
        var item = new NavigationItemViewModel(title, icon, viewModel)
        {
            IsVisible = visible
        };

        if (visible)
        {
            NavigationItems.Add(item);
        }
    }

    private void Logout()
    {
        if (_dialogService.Confirm(_localizationService.GetString("Confirm.Logout"), _localizationService.GetString("Confirm.LogoutTitle")))
        {
            _sessionService.Clear();
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ShowAbout()
    {
        var vm = new AboutViewModel(_localizationService);
        var dialog = new AboutDialog(vm);
        _dialogService.ShowDialog(dialog);
    }

    private void OnProjectsDrillRequested(object? sender, ProjectStatus? status)
    {
        var target = NavigationItems.FirstOrDefault(n => n.ViewModel == _projectsViewModel);
        if (target != null)
        {
            SelectedNavigation = target;
            _projectsViewModel.ApplyStatusFilter(status);
        }
    }

    private void OnStudentsDrillRequested(object? sender, EventArgs e)
    {
        var target = NavigationItems.FirstOrDefault(n => n.ViewModel == _studentsViewModel);
        if (target != null)
        {
            SelectedNavigation = target;
        }
    }

    private void OnCommitteesDrillRequested(object? sender, EventArgs e)
    {
        var target = NavigationItems.FirstOrDefault(n => n.ViewModel == _committeesViewModel);
        if (target != null)
        {
            SelectedNavigation = target;
        }
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        ConfigureNavigation();
    }

    private string GetRoleName(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => _localizationService.GetString("Role.Admin"),
            UserRole.HeadOfDepartment => _localizationService.GetString("Role.HeadOfDepartment"),
            UserRole.Supervisor => _localizationService.GetString("Role.Supervisor"),
            UserRole.Student => _localizationService.GetString("Role.Student"),
            _ => role.ToString()
        };
    }
}
