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

public class StudentsViewModel : PagedViewModel<StudentDto>
{
    private readonly IStudentService _studentService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly ITeamService _teamService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;
    private readonly IExcelImportService _excelImportService;

    private IEnumerable<StudentDto> _allStudents = [];

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

    private StudentDto? _selectedStudent;
    public StudentDto? SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            if (SetProperty(ref _selectedStudent, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanManage => _sessionService.CurrentUser?.Role == UserRole.Admin;
    public bool IsHeadOfDepartment => _sessionService.CurrentUser?.Role == UserRole.HeadOfDepartment;
    public bool CanImport => _sessionService.CurrentUser?.Role is UserRole.Admin or UserRole.HeadOfDepartment;
    public bool ShowManageButtons => CanManage;

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public AsyncRelayCommand ImportFromExcelCommand { get; }

    public StudentsViewModel(
        IStudentService studentService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        ITeamService teamService,
        IDialogService dialogService,
        ISessionService sessionService,
        ILocalizationService localizationService,
        IExcelImportService excelImportService)
    {
        _studentService = studentService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _teamService = teamService;
        _dialogService = dialogService;
        _sessionService = sessionService;
        _localizationService = localizationService;
        _excelImportService = excelImportService;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new RelayCommand(_ => AddStudent(), _ => CanManage);
        EditCommand = new RelayCommand(_ => EditStudent(), _ => CanManage && SelectedStudent != null);
        DeleteCommand = new RelayCommand(_ => DeleteStudent(), _ => CanManage && SelectedStudent != null);
        ImportFromExcelCommand = new AsyncRelayCommand(ImportFromExcelAsync, () => CanImport);
    }

    public async Task LoadAsync()
    {
        try
        {
            // Load departments for filter
            Departments.Clear();
            var placeholder = new DepartmentDto { DepartmentId = 0, NameAr = _localizationService.GetString("Placeholder.AllDepartments") };
            Departments.Add(placeholder);

            var departments = await _departmentService.GetAllAsync();
            foreach (var dept in departments.OrderBy(d => d.NameAr))
            {
                Departments.Add(dept);
            }

            // Load all students
            _allStudents = await _studentService.GetAllAsync();
            
            // Apply filter
            ApplyDepartmentFilter();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Students"));
        }
    }

    private void ApplyDepartmentFilter()
    {
        var filtered = _selectedDepartmentId == 0
            ? _allStudents
            : _allStudents.Where(s => s.DepartmentId == _selectedDepartmentId);
        
        SetItems(filtered.OrderBy(s => s.FullName));
    }

    protected override bool FilterItem(StudentDto item, string searchText)
    {
        return item.FullName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.StudentNumber.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.DepartmentName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.CollegeName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.TeamName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
    }

    private async Task ImportFromExcelAsync()
    {
        try
        {
            var filePath = _excelImportService.OpenExcelFileDialog();
            if (string.IsNullOrEmpty(filePath))
                return;

            var excelRows = _excelImportService.ReadStudentsFromExcel(filePath);
            if (!excelRows.Any())
            {
                _dialogService.ShowError(
                    _localizationService.IsArabic ? "لا توجد بيانات في الملف" : "No data found in file",
                    _localizationService.GetString("Title.Students"));
                return;
            }

            // Get all departments for matching by code
            var departments = await _departmentService.GetAllAsync();
            var existingStudents = await _studentService.GetAllAsync();
            
            int imported = 0, updated = 0, skipped = 0;
            var errors = new System.Collections.Generic.List<string>();

            foreach (var row in excelRows)
            {
                try
                {
                    // Find department by code
                    var dept = departments.FirstOrDefault(d => 
                        d.Code.Equals(row.DepartmentCode, System.StringComparison.OrdinalIgnoreCase));
                    
                    if (dept == null)
                    {
                        errors.Add($"Row '{row.StudentNumber}': Department code '{row.DepartmentCode}' not found");
                        skipped++;
                        continue;
                    }

                    // Check if student already exists
                    var existing = existingStudents.FirstOrDefault(s => 
                        s.StudentNumber.Equals(row.StudentNumber, System.StringComparison.OrdinalIgnoreCase));

                    var studentDto = new StudentDto
                    {
                        StudentNumber = row.StudentNumber,
                        FullName = row.FullName,
                        Email = row.Email,
                        Phone = row.Phone,
                        DepartmentId = dept.DepartmentId,
                        CollegeId = dept.CollegeId,
                        EnrollmentYear = row.EnrollmentYear
                    };

                    if (existing != null)
                    {
                        // Update existing student
                        studentDto.StudentId = existing.StudentId;
                        studentDto.TeamId = existing.TeamId; // Keep team assignment
                        var result = await _studentService.UpdateAsync(studentDto);
                        if (result.IsSuccess)
                            updated++;
                        else
                            errors.Add($"Row '{row.StudentNumber}': {result.Message}");
                    }
                    else
                    {
                        // Add new student
                        var result = await _studentService.AddAsync(studentDto);
                        if (result.IsSuccess)
                            imported++;
                        else
                            errors.Add($"Row '{row.StudentNumber}': {result.Message}");
                    }
                }
                catch (System.Exception ex)
                {
                    errors.Add($"Row '{row.StudentNumber}': {ex.Message}");
                    skipped++;
                }
            }

            // Show result
            var message = _localizationService.IsArabic
                ? $"تم الاستيراد بنجاح!\n\nجديد: {imported}\nمُحدَّث: {updated}\nتخطي: {skipped}"
                : $"Import completed!\n\nNew: {imported}\nUpdated: {updated}\nSkipped: {skipped}";

            if (errors.Any())
            {
                message += _localizationService.IsArabic 
                    ? $"\n\nأخطاء ({errors.Count}):\n" + string.Join("\n", errors.Take(5))
                    : $"\n\nErrors ({errors.Count}):\n" + string.Join("\n", errors.Take(5));
            }

            System.Windows.MessageBox.Show(message, _localizationService.GetString("Title.Students"),
                System.Windows.MessageBoxButton.OK, 
                errors.Any() ? System.Windows.MessageBoxImage.Warning : System.Windows.MessageBoxImage.Information);

            await LoadAsync();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Students"));
        }
    }

    private void AddStudent()
    {
        var vm = new StudentEditViewModel(_studentService, _collegeService, _departmentService, _teamService, _dialogService, _localizationService);
        var dialog = new StudentDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private void EditStudent()
    {
        if (SelectedStudent == null)
        {
            return;
        }

        var vm = new StudentEditViewModel(_studentService, _collegeService, _departmentService, _teamService, _dialogService, _localizationService, SelectedStudent);
        var dialog = new StudentDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private async void DeleteStudent()
    {
        if (SelectedStudent == null)
        {
            return;
        }

        if (_dialogService.Confirm(_localizationService.GetString("Confirm.DeleteStudent"), _localizationService.GetString("Title.Students")))
        {
            var result = await _studentService.DeleteAsync(SelectedStudent.StudentId);
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Students"));
            }
        }
    }
}

