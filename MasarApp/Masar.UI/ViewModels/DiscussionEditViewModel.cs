using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class DiscussionEditViewModel : DialogViewModel
{
    private readonly IDiscussionService _discussionService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly ITeamService _teamService;
    private readonly ICommitteeService _committeeService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    public ObservableCollection<CollegeDto> Colleges { get; } = new();
    public ObservableCollection<TeamDto> Teams { get; } = new();
    public ObservableCollection<CommitteeDto> Committees { get; } = new();

    private int _selectedCollegeId;
    public int SelectedCollegeId
    {
        get => _selectedCollegeId;
        set
        {
            if (SetProperty(ref _selectedCollegeId, value))
            {
                _ = LoadCollegeDependentDataAsync();
            }
        }
    }

    private DiscussionDto _discussion = new();
    public DiscussionDto Discussion
    {
        get => _discussion;
        set => SetProperty(ref _discussion, value);
    }

    public bool IsEditMode { get; }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public DiscussionEditViewModel(
        IDiscussionService discussionService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        ITeamService teamService,
        ICommitteeService committeeService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        DiscussionDto? discussion = null)
    {
        _discussionService = discussionService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _teamService = teamService;
        _committeeService = committeeService;
        _dialogService = dialogService;
        _localizationService = localizationService;

        Discussion = discussion ?? new DiscussionDto
        {
            StartTime = System.DateTime.Now,
            EndTime = System.DateTime.Now.AddHours(1)
        };
        IsEditMode = discussion != null;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(_ => Close(false));
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    public async Task LoadAsync()
    {
        try
        {
            // Load colleges
            Colleges.Clear();
            var collegePlaceholder = _localizationService.GetString("Placeholder.SelectCollege");
            Colleges.Add(new CollegeDto { CollegeId = 0, NameEn = collegePlaceholder, NameAr = collegePlaceholder });
            foreach (var college in await _collegeService.GetAllAsync())
            {
                Colleges.Add(college);
            }

            // Determine college from team if editing
            if (IsEditMode && Discussion.TeamId > 0)
            {
                var allTeams = await _teamService.GetAllAsync();
                var team = allTeams.FirstOrDefault(t => t.TeamId == Discussion.TeamId);
                if (team != null)
                {
                    var allDepts = await _departmentService.GetAllAsync();
                    var dept = allDepts.FirstOrDefault(d => d.DepartmentId == team.DepartmentId);
                    if (dept != null)
                    {
                        _selectedCollegeId = dept.CollegeId;
                        OnPropertyChanged(nameof(SelectedCollegeId));
                    }
                }
            }

            // Load data filtered by college
            await LoadCollegeDependentDataAsync();

            Discussion.TeamId = Discussion.TeamId != 0 ? Discussion.TeamId : 0;
            Discussion.CommitteeId = Discussion.CommitteeId != 0 ? Discussion.CommitteeId : 0;
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Discussion"));
        }
    }

    private async Task LoadCollegeDependentDataAsync()
    {
        try
        {
            // Get departments for this college (for filtering teams)
            var collegeDeptIds = new System.Collections.Generic.HashSet<int>();
            if (SelectedCollegeId > 0)
            {
                var allDepts = await _departmentService.GetAllAsync();
                collegeDeptIds = allDepts.Where(d => d.CollegeId == SelectedCollegeId).Select(d => d.DepartmentId).ToHashSet();
            }

            // Load teams filtered by college's departments
            Teams.Clear();
            var teamPlaceholder = _localizationService.GetString("Placeholder.SelectTeam");
            Teams.Add(new TeamDto { TeamId = 0, Name = teamPlaceholder, ProjectTitle = teamPlaceholder });
            if (SelectedCollegeId > 0)
            {
                var allTeams = await _teamService.GetAllAsync();
                foreach (var team in allTeams.Where(t => collegeDeptIds.Contains(t.DepartmentId)).OrderBy(t => t.Name))
                {
                    Teams.Add(team);
                }
            }

            // Load committees filtered by college
            Committees.Clear();
            var committeePlaceholder = _localizationService.GetString("Placeholder.SelectCommittee");
            Committees.Add(new CommitteeDto { CommitteeId = 0, Name = committeePlaceholder });
            if (SelectedCollegeId > 0)
            {
                var allCommittees = await _committeeService.GetAllAsync();
                foreach (var committee in allCommittees.Where(c => collegeDeptIds.Contains(c.DepartmentId)))
                {
                    Committees.Add(committee);
                }
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Discussion"));
        }
    }

    private void OnLanguageChanged(object? sender, System.EventArgs e)
    {
        _ = LoadAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            if (Discussion.TeamId == 0)
            {
                _dialogService.ShowError(_localizationService.GetString("Placeholder.SelectTeam"), _localizationService.GetString("Title.Discussion"));
                return;
            }

            if (Discussion.CommitteeId == 0)
            {
                _dialogService.ShowError(_localizationService.GetString("Placeholder.SelectCommittee"), _localizationService.GetString("Title.Discussion"));
                return;
            }

            Result<DiscussionDto> result;
            if (IsEditMode)
            {
                result = await _discussionService.UpdateAsync(Discussion);
            }
            else
            {
                result = await _discussionService.ScheduleAsync(Discussion);
            }

            if (result.IsSuccess)
            {
                Discussion = result.Value!;
                Close(true);
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Discussion"));
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Discussion"));
        }
    }
}
