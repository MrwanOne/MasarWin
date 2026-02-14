using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.Domain.Enums;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class DoctorsViewModel : PagedViewModel<DoctorDto>
{
    private readonly IDoctorService _doctorService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;
    private bool _isLoading;

    public ObservableCollection<CollegeDto> Colleges { get; } = new();
    public ObservableCollection<DepartmentDto> Departments { get; } = new();

    private int _selectedCollegeId;
    public int SelectedCollegeId
    {
        get => _selectedCollegeId;
        set
        {
            if (SetProperty(ref _selectedCollegeId, value))
            {
                _ = LoadDepartmentsForCollegeAsync();
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
                _ = LoadDoctorsForDepartmentAsync();
            }
        }
    }

    private DoctorDto? _selectedDoctor;
    public DoctorDto? SelectedDoctor
    {
        get => _selectedDoctor;
        set
        {
            if (SetProperty(ref _selectedDoctor, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                SetAsHODCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanManage => _sessionService.CurrentUser?.Role is UserRole.Admin or UserRole.HeadOfDepartment;
    public bool IsAdmin => _sessionService.CurrentUser?.Role is UserRole.Admin;

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public AsyncRelayCommand SetAsHODCommand { get; }

    public DoctorsViewModel(
        IDoctorService doctorService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        IDialogService dialogService,
        ISessionService sessionService,
        ILocalizationService localizationService)
    {
        _doctorService = doctorService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _dialogService = dialogService;
        _sessionService = sessionService;
        _localizationService = localizationService;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new RelayCommand(_ => AddDoctor(), _ => CanManage);
        EditCommand = new RelayCommand(_ => EditDoctor(), _ => CanManage && SelectedDoctor != null);
        DeleteCommand = new RelayCommand(_ => DeleteDoctor(), _ => CanManage && SelectedDoctor != null);
        SetAsHODCommand = new AsyncRelayCommand(SetAsHODAsync, () => CanManage && SelectedDoctor != null);
    }

    public async Task LoadAsync()
    {
        if (_isLoading) return;
        _isLoading = true;
        try
        {
            var currentUser = _sessionService.CurrentUser;
            var isHoD = currentUser?.Role == UserRole.HeadOfDepartment;

            if (isHoD && currentUser?.DoctorId.HasValue == true)
            {
                // HoD: Load only their department's doctors
                var doctors = await _doctorService.GetAllAsync();
                var hodDoctor = doctors.FirstOrDefault(d => d.DoctorId == currentUser.DoctorId.Value);
                
                if (hodDoctor != null)
                {
                    // Store HoD's department and college for later use
                    _hodDepartmentId = hodDoctor.DepartmentId;
                    _hodCollegeId = hodDoctor.CollegeId;

                    // Load only doctors from this department
                    var deptDoctors = doctors.Where(d => d.DepartmentId == hodDoctor.DepartmentId);
                    SetItems(deptDoctors.OrderBy(d => d.FullName));

                    // Clear filters for HoD (they only see their department)
                    Colleges.Clear();
                    Departments.Clear();
                }
            }
            else
            {
                // Admin: Load all colleges and departments
                Colleges.Clear();
                var allCollegesPlaceholder = new CollegeDto { CollegeId = 0, NameAr = _localizationService.GetString("Placeholder.AllColleges"), NameEn = "All Colleges" };
                Colleges.Add(allCollegesPlaceholder);
                var colleges = await _collegeService.GetAllAsync();
                foreach (var college in colleges.OrderBy(c => c.NameAr))
                {
                    Colleges.Add(college);
                }

                _selectedCollegeId = 0;
                OnPropertyChanged(nameof(SelectedCollegeId));

                // Load departments (all) and all doctors explicitly
                await LoadDepartmentsForCollegeAsync();
                await LoadDoctorsForDepartmentAsync();
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Doctors"));
        }
        finally
        {
            _isLoading = false;
        }
    }

    private int? _hodDepartmentId;
    private int? _hodCollegeId;

    private async Task LoadDepartmentsForCollegeAsync()
    {
        try
        {
            Departments.Clear();
            var allDeptsPlaceholder = new DepartmentDto { DepartmentId = 0, NameAr = _localizationService.GetString("Placeholder.AllDepartments"), NameEn = "All Departments" };
            Departments.Add(allDeptsPlaceholder);

            if (SelectedCollegeId > 0)
            {
                var departments = await _departmentService.GetAllAsync();
                var filtered = departments.Where(d => d.CollegeId == SelectedCollegeId).OrderBy(d => d.NameAr);
                foreach (var dept in filtered)
                {
                    Departments.Add(dept);
                }
            }

            SelectedDepartmentId = 0; // Show all departments
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Doctors"));
        }
    }

    private async Task LoadDoctorsForDepartmentAsync()
    {
        try
        {
            var doctors = await _doctorService.GetAllAsync();
            var filtered = doctors.AsEnumerable();

            // Filter by college if selected
            if (SelectedCollegeId > 0)
            {
                filtered = filtered.Where(d => d.CollegeId == SelectedCollegeId);
            }

            // Filter by department if selected
            if (SelectedDepartmentId > 0)
            {
                filtered = filtered.Where(d => d.DepartmentId == SelectedDepartmentId);
            }

            SetItems(filtered.OrderBy(d => d.FullName));
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Doctors"));
        }
    }

    protected override bool FilterItem(DoctorDto item, string searchText)
    {
        return item.FullName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.Qualification.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.Phone.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.DepartmentName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.CollegeName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
    }

    private void AddDoctor()
    {
        var vm = new DoctorEditViewModel(_doctorService, _collegeService, _departmentService, _dialogService, _localizationService);
        
        // If HoD, pre-select their department and college
        if (_hodDepartmentId.HasValue && _hodCollegeId.HasValue)
        {
            vm.SetHeadOfDepartmentContext(_hodCollegeId.Value, _hodDepartmentId.Value);
        }
        
        var dialog = new DoctorDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private void EditDoctor()
    {
        if (SelectedDoctor == null)
        {
            return;
        }

        var vm = new DoctorEditViewModel(_doctorService, _collegeService, _departmentService, _dialogService, _localizationService, SelectedDoctor);
        var dialog = new DoctorDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private async void DeleteDoctor()
    {
        if (SelectedDoctor == null)
        {
            return;
        }

        if (_dialogService.Confirm(_localizationService.GetString("Confirm.DeleteDoctor"), _localizationService.GetString("Title.Doctors")))
        {
            var result = await _doctorService.DeleteAsync(SelectedDoctor.DoctorId);
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Doctors"));
            }
        }
    }

    private async Task SetAsHODAsync()
    {
        if (SelectedDoctor == null) return;

        try
        {
            // Check if doctor is already HOD of any department
            if (SelectedDoctor.IsHeadOfDepartment)
            {
                var confirmChange = _dialogService.Confirm(
                    _localizationService.IsArabic
                        ? $"{SelectedDoctor.FullName} هو بالفعل رئيس قسم. هل تريد تعيينه رئيساً لقسم آخر؟\n(سيتم إلغاء تعيينه من القسم الحالي)"
                        : $"{SelectedDoctor.FullName} is already HOD. Do you want to assign to a different department?\n(Current HOD status will be removed)",
                    _localizationService.GetString("Title.Doctors"));
                
                if (!confirmChange) return;
            }

            // Get all departments in the doctor's college
            var departments = await _departmentService.GetAllAsync();
            var collegeDepts = departments.Where(d => d.CollegeId == SelectedDoctor.CollegeId).ToList();

            if (!collegeDepts.Any())
            {
                _dialogService.ShowError(
                    _localizationService.IsArabic ? "لا توجد أقسام في كلية هذا الدكتور" : "No departments in this doctor's college",
                    _localizationService.GetString("Title.Doctors"));
                return;
            }

            // Show selection dialog with HOD status
            var deptList = collegeDepts.Select((d, i) =>
            {
                var hodInfo = d.HeadOfDepartmentId.HasValue 
                    ? (_localizationService.IsArabic ? $" (رئيسه: {d.HeadOfDepartmentName})" : $" (HOD: {d.HeadOfDepartmentName})")
                    : (_localizationService.IsArabic ? " (لا يوجد رئيس)" : " (No HOD)");
                return $"{i + 1}. {(_localizationService.IsArabic ? d.NameAr : d.NameEn)}{hodInfo}";
            });
            
            var deptNames = string.Join("\n", deptList);
            var message = _localizationService.IsArabic
                ? $"اختر رقم القسم لتعيين {SelectedDoctor.FullName} كرئيس له:\n\n{deptNames}\n\nأدخل الرقم:"
                : $"Select department number to set {SelectedDoctor.FullName} as HOD:\n\n{deptNames}\n\nEnter number:";

            var inputVm = new InputDialogViewModel(_localizationService.GetString("Title.Doctors"), message);
            var inputDialog = new InputDialogWindow(inputVm);
            if (_dialogService.ShowDialog(inputDialog) != true) return;
            var input = inputVm.InputText;

            if (string.IsNullOrWhiteSpace(input)) return;

            if (int.TryParse(input, out int selection) && selection > 0 && selection <= collegeDepts.Count)
            {
                var selectedDept = collegeDepts[selection - 1];
                
                // SetHeadOfDepartmentAsync already handles the confirmation if department has existing HOD
                var result = await _departmentService.SetHeadOfDepartmentAsync(selectedDept.DepartmentId, SelectedDoctor.DoctorId);
                
                if (result.IsSuccess)
                {
                    _dialogService.ShowMessage(
                        _localizationService.IsArabic 
                            ? $"تم تعيين {SelectedDoctor.FullName} كرئيس لقسم {selectedDept.NameAr}" 
                            : $"{SelectedDoctor.FullName} set as HOD of {selectedDept.NameEn}",
                        _localizationService.GetString("Title.Doctors"));
                    await LoadAsync();
                }
                else
                {
                    _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Doctors"));
                }
            }
            else
            {
                _dialogService.ShowError(
                    _localizationService.IsArabic ? "رقم غير صحيح" : "Invalid number",
                    _localizationService.GetString("Title.Doctors"));
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Doctors"));
        }
    }
}
