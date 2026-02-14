using Masar.Application.Services;
using Masar.Domain.Enums;
using Masar.UI.Controls;
using Masar.UI.Services;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IAcademicTermService _termService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    public event EventHandler<ProjectStatus?>? ProjectsDrillRequested;
    public event EventHandler? StudentsDrillRequested;
    public event EventHandler? CommitteesDrillRequested;

    private int _totalProjects;
    public int TotalProjects
    {
        get => _totalProjects;
        set => SetProperty(ref _totalProjects, value);
    }

    private int _proposedProjects;
    public int ProposedProjects
    {
        get => _proposedProjects;
        set => SetProperty(ref _proposedProjects, value);
    }

    private int _approvedProjects;
    public int ApprovedProjects
    {
        get => _approvedProjects;
        set => SetProperty(ref _approvedProjects, value);
    }

    private int _completedProjects;
    public int CompletedProjects
    {
        get => _completedProjects;
        set => SetProperty(ref _completedProjects, value);
    }

    private int _totalStudents;
    public int TotalStudents
    {
        get => _totalStudents;
        set => SetProperty(ref _totalStudents, value);
    }

    private int _totalCommittees;
    public int TotalCommittees
    {
        get => _totalCommittees;
        set => SetProperty(ref _totalCommittees, value);
    }

    private string _activeTermName = string.Empty;
    public string ActiveTermName
    {
        get => _activeTermName;
        set => SetProperty(ref _activeTermName, value);
    }

    private string _activeTermDates = string.Empty;
    public string ActiveTermDates
    {
        get => _activeTermDates;
        set => SetProperty(ref _activeTermDates, value);
    }

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand ShowAllProjectsCommand { get; }
    public RelayCommand ShowProposedProjectsCommand { get; }
    public RelayCommand ShowApprovedProjectsCommand { get; }
    public RelayCommand ShowCompletedProjectsCommand { get; }
    public RelayCommand ShowStudentsCommand { get; }
    public RelayCommand ShowCommitteesCommand { get; }

    public DashboardViewModel(
        IDashboardService dashboardService, 
        IAcademicTermService termService,
        IDialogService dialogService, 
        ILocalizationService localizationService)
    {
        _dashboardService = dashboardService;
        _termService = termService;
        _dialogService = dialogService;
        _localizationService = localizationService;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        ShowAllProjectsCommand = new RelayCommand(_ => ProjectsDrillRequested?.Invoke(this, null));
        ShowProposedProjectsCommand = new RelayCommand(_ => ProjectsDrillRequested?.Invoke(this, ProjectStatus.Proposed));
        ShowApprovedProjectsCommand = new RelayCommand(_ => ProjectsDrillRequested?.Invoke(this, ProjectStatus.Approved));
        ShowCompletedProjectsCommand = new RelayCommand(_ => ProjectsDrillRequested?.Invoke(this, ProjectStatus.Completed));
        ShowStudentsCommand = new RelayCommand(_ => StudentsDrillRequested?.Invoke(this, EventArgs.Empty));
        ShowCommitteesCommand = new RelayCommand(_ => CommitteesDrillRequested?.Invoke(this, EventArgs.Empty));
    }

    public async Task LoadAsync()
    {
        try
        {
            var stats = await _dashboardService.GetStatsAsync();
            TotalProjects = stats.TotalProjects;
            ProposedProjects = stats.ProposedProjects;
            ApprovedProjects = stats.ApprovedProjects;
            CompletedProjects = stats.CompletedProjects;
            TotalStudents = stats.TotalStudents;
            TotalCommittees = stats.TotalCommittees;

            var activeTerm = await _termService.GetActiveTermAsync();
            if (activeTerm != null)
            {
                ActiveTermName = _localizationService.IsArabic ? activeTerm.NameAr : activeTerm.NameEn;
                ActiveTermDates = $"{activeTerm.StartDate:yyyy-MM-dd} - {activeTerm.EndDate:yyyy-MM-dd}";
            }
            else
            {
                ActiveTermName = _localizationService.IsArabic ? "لا يوجد فصل نشط" : "No Active Term";
                ActiveTermDates = string.Empty;
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Dashboard"));
        }
    }
}
