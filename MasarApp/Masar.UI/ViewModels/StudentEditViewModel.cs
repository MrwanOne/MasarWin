using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class StudentEditViewModel : DialogViewModel
{
    private readonly IStudentService _studentService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly ITeamService _teamService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    public ObservableCollection<CollegeDto> Colleges { get; } = new();
    public ObservableCollection<DepartmentDto> Departments { get; } = new();
    public ObservableCollection<TeamDto> Teams { get; } = new();

    private StudentDto _student = new();
    public StudentDto Student
    {
        get => _student;
        set => SetProperty(ref _student, value);
    }

    private int _selectedCollegeId;
    public int SelectedCollegeId
    {
        get => _selectedCollegeId;
        set
        {
            if (SetProperty(ref _selectedCollegeId, value))
            {
                _ = LoadDepartmentsAsync();
            }
        }
    }

    public bool IsEditMode { get; }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public StudentEditViewModel(
        IStudentService studentService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        ITeamService teamService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        StudentDto? student = null)
    {
        _studentService = studentService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _teamService = teamService;
        _dialogService = dialogService;
        _localizationService = localizationService;

        Student = student ?? new StudentDto();
        IsEditMode = student != null;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(_ => Close(false));
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    public async Task LoadAsync()
    {
        try
        {
            Colleges.Clear();
            var placeholder = Placeholder("Placeholder.SelectCollege");
            Colleges.Add(new CollegeDto { CollegeId = 0, NameEn = placeholder, NameAr = placeholder });
            foreach (var college in await _collegeService.GetAllAsync())
            {
                Colleges.Add(college);
            }

            SelectedCollegeId = Student.CollegeId != 0 ? Student.CollegeId : 0;

            await LoadDepartmentsAsync();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Student"));
        }
    }

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
                .OrderBy(d => d.NameEn);

            foreach (var dept in filtered)
            {
                Departments.Add(dept);
            }

            Student.DepartmentId = Student.DepartmentId != 0 ? Student.DepartmentId : 0;

            await LoadTeamsAsync();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Student"));
        }
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
                .Where(t => t.DepartmentId == Student.DepartmentId)
                .GroupBy(t => t.TeamId)
                .Select(g => g.First())
                .OrderBy(t => t.Name);

            foreach (var team in filtered)
            {
                Teams.Add(team);
            }

            Student.TeamId ??= 0;
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Student"));
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
            if (SelectedCollegeId == 0)
            {
                _dialogService.ShowError(Placeholder("Placeholder.SelectCollege"), _localizationService.GetString("Title.Student"));
                return;
            }

            if (Student.DepartmentId == 0)
            {
                _dialogService.ShowError(Placeholder("Placeholder.SelectDepartment"), _localizationService.GetString("Title.Student"));
                return;
            }

            Student.CollegeId = SelectedCollegeId;
            Student.TeamId = Student.TeamId == 0 ? null : Student.TeamId;
            
            Result<StudentDto> result;
            if (IsEditMode)
            {
                result = await _studentService.UpdateAsync(Student);
            }
            else
            {
                result = await _studentService.AddAsync(Student);
            }

            if (result.IsSuccess)
            {
                Student = result.Value!;
                Close(true);
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Student"));
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Student"));
        }
    }

    private string Placeholder(string resourceKey) => _localizationService.GetString(resourceKey);
}
