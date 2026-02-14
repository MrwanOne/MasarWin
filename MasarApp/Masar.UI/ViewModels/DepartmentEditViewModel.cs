using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class DepartmentEditViewModel : DialogViewModel
{
    private readonly IDepartmentService _departmentService;
    private readonly ICollegeService _collegeService;
    private readonly IDoctorService _doctorService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    public ObservableCollection<CollegeDto> Colleges { get; } = new();
    public ObservableCollection<DoctorDto> Doctors { get; } = new();

    private DepartmentDto _department = new();
    public DepartmentDto Department
    {
        get => _department;
        set => SetProperty(ref _department, value);
    }

    private int _selectedCollegeId;
    public int SelectedCollegeId
    {
        get => _selectedCollegeId;
        set
        {
            if (_selectedCollegeId != value)
            {
                _selectedCollegeId = value;
                Department.CollegeId = value;
                OnPropertyChanged(nameof(SelectedCollegeId));
                _ = LoadDoctorsAsync();
            }
        }
    }

    public bool IsEditMode { get; }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public DepartmentEditViewModel(
        IDepartmentService departmentService,
        ICollegeService collegeService,
        IDoctorService doctorService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        DepartmentDto? department = null)
    {
        _departmentService = departmentService;
        _collegeService = collegeService;
        _doctorService = doctorService;
        _dialogService = dialogService;
        _localizationService = localizationService;

        Department = department ?? new DepartmentDto();
        IsEditMode = department != null;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(_ => Close(false));
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    public async Task LoadAsync()
    {
        try
        {
            Colleges.Clear();
            var placeholder = _localizationService.GetString("Placeholder.SelectCollege");
            Colleges.Add(new CollegeDto { CollegeId = 0, NameEn = placeholder, NameAr = placeholder });
            
            var colleges = await _collegeService.GetAllAsync();
            foreach (var college in colleges)
            {
                Colleges.Add(college);
            }

            // Set the selected college if not already set (for edit mode it should be set)
            _selectedCollegeId = Department.CollegeId;
            OnPropertyChanged(nameof(SelectedCollegeId));

            if (_selectedCollegeId != 0)
            {
                await LoadDoctorsAsync();
            }

            // Notify UI that the Department object and its fields might need refresh
            OnPropertyChanged(nameof(Department));
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Departments"));
        }
    }

    private async Task LoadDoctorsAsync()
    {
        try
        {
            Doctors.Clear();
            var placeholder = _localizationService.GetString("Placeholder.SelectDoctor");
            Doctors.Add(new DoctorDto { DoctorId = 0, FullName = placeholder });
            
            if (Department.CollegeId > 0)
            {
                var doctors = await _doctorService.GetAllAsync();
                // Filter doctors by CollegeId (doctors in departments of the same college)
                foreach (var doctor in doctors.Where(d => d.CollegeId == Department.CollegeId))
                {
                    Doctors.Add(doctor);
                }
            }

            Department.HeadOfDepartmentId = Department.HeadOfDepartmentId.HasValue && Department.HeadOfDepartmentId.Value != 0
                ? Department.HeadOfDepartmentId
                : 0;
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Departments"));
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
            if (Department.CollegeId == 0)
            {
                _dialogService.ShowError(_localizationService.GetString("Placeholder.SelectCollege"), _localizationService.GetString("Title.Departments"));
                return;
            }

            Department.HeadOfDepartmentId = Department.HeadOfDepartmentId == 0 ? null : Department.HeadOfDepartmentId;
            Result<DepartmentDto> result;
            if (IsEditMode)
            {
                result = await _departmentService.UpdateAsync(Department);
            }
            else
            {
                result = await _departmentService.AddAsync(Department);
            }

            if (result.IsSuccess)
            {
                Department = result.Value!;
                Close(true);
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Departments"));
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Departments"));
        }
    }
}
