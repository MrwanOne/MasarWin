using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class TeamEditViewModel : DialogViewModel
{
    private readonly ITeamService _teamService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly IDoctorService _doctorService;
    private readonly ICommitteeService _committeeService;
    private readonly IStudentService _studentService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    public ObservableCollection<CollegeDto> Colleges { get; } = new();
    public ObservableCollection<DepartmentDto> Departments { get; } = new();
    public ObservableCollection<DoctorDto> Supervisors { get; } = new();
    public ObservableCollection<CommitteeDto> Committees { get; } = new();
    public ObservableCollection<StudentCheckItem> AvailableStudents { get; } = new();
    public ObservableCollection<int> EnrollmentYears { get; } = new();

    private int _selectedCollegeId;
    public int SelectedCollegeId
    {
        get => _selectedCollegeId;
        set
        {
            if (SetProperty(ref _selectedCollegeId, value))
            {
                _ = OnCollegeChangedAsync();
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
                Team.DepartmentId = value;
                _ = LoadStudentsForDepartmentAsync();
            }
        }
    }

    private int _selectedEnrollmentYear;
    public int SelectedEnrollmentYear
    {
        get => _selectedEnrollmentYear;
        set
        {
            if (SetProperty(ref _selectedEnrollmentYear, value))
            {
                _ = LoadStudentsForDepartmentAsync();
            }
        }
    }

    private TeamDto _team = new();
    public TeamDto Team
    {
        get => _team;
        set => SetProperty(ref _team, value);
    }

    public bool IsEditMode { get; }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    private System.Collections.Generic.List<StudentDto> _allStudents = new();
    private System.Collections.Generic.List<int> _initialSelectedStudentIds = new();

    public TeamEditViewModel(
        ITeamService teamService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        IDoctorService doctorService,
        ICommitteeService committeeService,
        IStudentService studentService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        TeamDto? team = null)
    {
        _teamService = teamService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _doctorService = doctorService;
        _committeeService = committeeService;
        _studentService = studentService;
        _dialogService = dialogService;
        _localizationService = localizationService;

        Team = team ?? new TeamDto();
        IsEditMode = team != null;

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

            // Load all students for later filtering
            _allStudents = await _studentService.GetAllAsync();

            // Populate enrollment years
            EnrollmentYears.Clear();
            var currentYear = System.DateTime.Now.Year;
            for (var year = currentYear; year >= 2016; year--)
            {
                EnrollmentYears.Add(year);
            }

            // Get students already in this team
            _initialSelectedStudentIds = _allStudents
                .Where(s => s.TeamId == Team.TeamId && Team.TeamId != 0)
                .Select(s => s.StudentId)
                .ToList();

            // Set default values
            Team.SupervisorId = Team.SupervisorId == 0 ? null : Team.SupervisorId;
            Team.CommitteeId = Team.CommitteeId == 0 ? null : Team.CommitteeId;

            // Set selected values
            if (EnrollmentYears.Any())
            {
                SelectedEnrollmentYear = Team.AcademicYear != 0 ? Team.AcademicYear : EnrollmentYears.First();
            }

            // Determine college from department if editing
            if (IsEditMode && Team.DepartmentId > 0)
            {
                var allDepts = await _departmentService.GetAllAsync();
                var teamDept = allDepts.FirstOrDefault(d => d.DepartmentId == Team.DepartmentId);
                if (teamDept != null)
                {
                    _selectedCollegeId = teamDept.CollegeId;
                    OnPropertyChanged(nameof(SelectedCollegeId));
                }
            }

            // Load cascaded data for the selected college
            await LoadCollegeDependentDataAsync();

            // Set department after loading departments
            SelectedDepartmentId = Team.DepartmentId;
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Team"));
        }
    }

    private async Task OnCollegeChangedAsync()
    {
        await LoadCollegeDependentDataAsync();
        // Reset department selection when college changes
        SelectedDepartmentId = 0;
    }

    private async Task LoadCollegeDependentDataAsync()
    {
        try
        {
            // Load departments filtered by college
            Departments.Clear();
            var departmentPlaceholder = _localizationService.GetString("Placeholder.SelectDepartment");
            Departments.Add(new DepartmentDto { DepartmentId = 0, NameEn = departmentPlaceholder, NameAr = departmentPlaceholder });
            if (SelectedCollegeId > 0)
            {
                var allDepts = await _departmentService.GetAllAsync();
                foreach (var dept in allDepts.Where(d => d.CollegeId == SelectedCollegeId).OrderBy(d => d.NameEn))
                {
                    Departments.Add(dept);
                }
            }

            // Load supervisors filtered by college (doctors belong to college, not department)
            Supervisors.Clear();
            var supervisorPlaceholder = _localizationService.GetString("Placeholder.SelectSupervisor");
            Supervisors.Add(new DoctorDto { DoctorId = 0, FullName = supervisorPlaceholder });
            if (SelectedCollegeId > 0)
            {
                var allDoctors = await _doctorService.GetAllAsync();
                foreach (var doc in allDoctors.Where(d => d.CollegeId == SelectedCollegeId).OrderBy(d => d.FullName))
                {
                    Supervisors.Add(doc);
                }
            }

            // Load committees filtered by college
            Committees.Clear();
            var committeePlaceholder = _localizationService.GetString("Placeholder.SelectCommittee");
            Committees.Add(new CommitteeDto { CommitteeId = 0, Name = committeePlaceholder });
            if (SelectedCollegeId > 0)
            {
                var allCommittees = await _committeeService.GetAllAsync();
                foreach (var committee in allCommittees.Where(c => Departments.Any(d => d.DepartmentId == c.DepartmentId && d.DepartmentId != 0)))
                {
                    Committees.Add(committee);
                }
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Team"));
        }
    }

    private async Task LoadStudentsForDepartmentAsync()
    {
        try
        {
            AvailableStudents.Clear();

            if (SelectedDepartmentId <= 0)
                return;

            // Filter students by department and enrollment year
            var filteredStudents = _allStudents
                .Where(s => s.DepartmentId == SelectedDepartmentId)
                .Where(s => SelectedEnrollmentYear == 0 || s.EnrollmentYear == SelectedEnrollmentYear)
                .Where(s => s.TeamId == null || s.TeamId == 0 || s.TeamId == Team.TeamId) // Only available or already in this team
                .OrderBy(s => s.FullName);

            foreach (var student in filteredStudents)
            {
                var item = new StudentCheckItem
                {
                    StudentId = student.StudentId,
                    StudentNumber = student.StudentNumber,
                    FullName = student.FullName,
                    IsSelected = _initialSelectedStudentIds.Contains(student.StudentId)
                };
                AvailableStudents.Add(item);
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Team"));
        }
        await Task.CompletedTask;
    }

    private void OnLanguageChanged(object? sender, System.EventArgs e)
    {
        _ = LoadAsync();
    }

    private async Task SaveAsync()
    {
        try
        {
            if (Team.DepartmentId == 0)
            {
                _dialogService.ShowError(_localizationService.GetString("Placeholder.SelectDepartment"), _localizationService.GetString("Title.Team"));
                return;
            }

            var selectedStudents = AvailableStudents.Where(s => s.IsSelected).ToList();
            if (!selectedStudents.Any())
            {
                _dialogService.ShowError(
                    _localizationService.IsArabic ? "يرجى اختيار طالب واحد على الأقل" : "Please select at least one student",
                    _localizationService.GetString("Title.Team"));
                return;
            }

            Team.SupervisorId = Team.SupervisorId == 0 ? null : Team.SupervisorId;
            Team.CommitteeId = Team.CommitteeId == 0 ? null : Team.CommitteeId;
            Team.AcademicYear = SelectedEnrollmentYear;

            Result<TeamDto> result;
            if (IsEditMode)
            {
                result = await _teamService.UpdateAsync(Team);
            }
            else
            {
                result = await _teamService.AddAsync(Team);
            }

            if (result.IsSuccess)
            {
                Team = result.Value!;

                // Update student team assignments
                var currentSelectedIds = selectedStudents.Select(s => s.StudentId).ToHashSet();

                // Remove students that were deselected
                foreach (var studentId in _initialSelectedStudentIds)
                {
                    if (!currentSelectedIds.Contains(studentId))
                    {
                        await _studentService.AssignTeamAsync(studentId, null);
                    }
                }

                // Add newly selected students
                foreach (var studentId in currentSelectedIds)
                {
                    if (!_initialSelectedStudentIds.Contains(studentId))
                    {
                        await _studentService.AssignTeamAsync(studentId, Team.TeamId);
                    }
                }

                Close(true);
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Team"));
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Team"));
        }
    }
}

// Helper class for student selection
public class StudentCheckItem : ViewModelBase
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
