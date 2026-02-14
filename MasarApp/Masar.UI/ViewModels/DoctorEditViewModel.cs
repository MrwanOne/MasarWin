using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Models;
using Masar.UI.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class DoctorEditViewModel : DialogViewModel
{
    private readonly IDoctorService _doctorService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    public ObservableCollection<CollegeDto> Colleges { get; } = new();
    public ObservableCollection<DepartmentDto> Departments { get; } = new();
    public ObservableCollection<OptionItem<string>> Genders { get; } = new();
    public ObservableCollection<OptionItem<string>> Ranks { get; } = new();

    private string? _selectedGender;
    public string? SelectedGender
    {
        get => _selectedGender;
        set => SetProperty(ref _selectedGender, value);
    }

    private DoctorDto _doctor = new();
    public DoctorDto Doctor
    {
        get => _doctor;
        set => SetProperty(ref _doctor, value);
    }

    private string? _selectedRank;
    public string? SelectedRank
    {
        get => _selectedRank;
        set => SetProperty(ref _selectedRank, value);
    }

    private int _selectedCollegeId;
    private bool _isInitialLoad = true;

    public int SelectedCollegeId
    {
        get => _selectedCollegeId;
        set
        {
            if (SetProperty(ref _selectedCollegeId, value))
            {
                // Only reset department when user explicitly changes college (not on initial load)
                if (!_isInitialLoad)
                {
                    Doctor.DepartmentId = null;
                    OnPropertyChanged(nameof(SelectedDepartmentId));
                }
                _ = LoadDepartmentsAsync();
            }
        }
    }

    private bool _isHeadOfDepartment;
    public bool IsHeadOfDepartment
    {
        get => _isHeadOfDepartment;
        set => SetProperty(ref _isHeadOfDepartment, value);
    }

    public int? SelectedDepartmentId
    {
        get => Doctor.DepartmentId;
        set
        {
            if (Doctor.DepartmentId != value)
            {
                Doctor.DepartmentId = value;
                OnPropertyChanged(nameof(SelectedDepartmentId));
                _ = RefreshIsHeadStatusAsync();
            }
        }
    }

    public bool IsEditMode { get; }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public DoctorEditViewModel(
        IDoctorService doctorService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        DoctorDto? doctor = null)
    {
        _doctorService = doctorService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _dialogService = dialogService;
        _localizationService = localizationService;

        Doctor = doctor ?? new DoctorDto();
        IsEditMode = doctor != null;
        LoadGenderOptions();
        LoadRankOptions();
        SelectedGender = string.IsNullOrWhiteSpace(Doctor.Gender) ? (IsEditMode ? null : "Male") : Doctor.Gender;
        SelectedRank = string.IsNullOrWhiteSpace(Doctor.Rank) ? (IsEditMode ? null : "Lecturer") : Doctor.Rank;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(_ => Close(false));
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    // HoD context fields
    private int? _hodCollegeId;
    private int? _hodDepartmentId;
    public bool IsHoDContext => _hodCollegeId.HasValue && _hodDepartmentId.HasValue;

    /// <summary>
    /// Called by DoctorsViewModel when HoD is adding a doctor - pre-selects their college/department
    /// </summary>
    public void SetHeadOfDepartmentContext(int collegeId, int departmentId)
    {
        _hodCollegeId = collegeId;
        _hodDepartmentId = departmentId;
    }

    private void LoadGenderOptions()
    {
        var current = SelectedGender;
        Genders.Clear();
        Genders.Add(new OptionItem<string>(null, _localizationService.GetString("Placeholder.SelectGender")));
        Genders.Add(new OptionItem<string>("Male", _localizationService.IsArabic ? "ذكر" : "Male"));
        Genders.Add(new OptionItem<string>("Female", _localizationService.IsArabic ? "أنثى" : "Female"));
        SelectedGender = current;
    }

    private void LoadRankOptions()
    {
        var current = SelectedRank;
        Ranks.Clear();
        Ranks.Add(new OptionItem<string>(null, _localizationService.GetString("Placeholder.SelectRank")));
        Ranks.Add(new OptionItem<string>("TeachingAssistant", _localizationService.IsArabic ? "معيد" : "Teaching Assistant"));
        Ranks.Add(new OptionItem<string>("Lecturer", _localizationService.IsArabic ? "محاضر" : "Lecturer"));
        Ranks.Add(new OptionItem<string>("AssistantProfessor", _localizationService.IsArabic ? "أستاذ مساعد" : "Assistant Professor"));
        Ranks.Add(new OptionItem<string>("AssociateProfessor", _localizationService.IsArabic ? "أستاذ مشارك" : "Associate Professor"));
        Ranks.Add(new OptionItem<string>("Professor", _localizationService.IsArabic ? "أستاذ" : "Professor"));
        SelectedRank = current;
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

            // For HoD context, auto-select their college and department
            if (IsHoDContext && !IsEditMode)
            {
                SelectedCollegeId = _hodCollegeId!.Value;
                Doctor.DepartmentId = _hodDepartmentId;
            }
            else
            {
                SelectedCollegeId = Doctor.CollegeId != 0 ? Doctor.CollegeId : 0;
            }
            _isInitialLoad = false;  // After initial load, allow department reset on college change
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Doctors"));
        }
    }

    private void OnLanguageChanged(object? sender, System.EventArgs e)
    {
        LoadGenderOptions();
        LoadRankOptions();
        _ = LoadAsync();
    }

    private async Task LoadDepartmentsAsync()
    {
        try
        {
            Departments.Clear();
            var departments = await _departmentService.GetAllAsync();
            var list = departments.Where(d => d.CollegeId == SelectedCollegeId).ToList();
            
            // Add placeholder
            var placeholder = _localizationService.GetString("Placeholder.SelectDepartment");
            Departments.Add(new DepartmentDto { DepartmentId = 0, NameEn = placeholder, NameAr = placeholder });
            
            foreach (var dept in list)
            {
                Departments.Add(dept);
            }

            // Only auto-select first department in edit mode if already set
            if (!Doctor.DepartmentId.HasValue || Doctor.DepartmentId.Value == 0)
            {
                // Clear selection for add mode - user must select explicitly
                Doctor.DepartmentId = null;
            }
            OnPropertyChanged(nameof(SelectedDepartmentId));
            _ = RefreshIsHeadStatusAsync();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Doctors"));
        }
    }

    private async Task RefreshIsHeadStatusAsync()
    {
        if (!Doctor.DepartmentId.HasValue || Doctor.DepartmentId.Value == 0 || !IsEditMode) 
        {
            IsHeadOfDepartment = false;
            return;
        }

        try
        {
            var dept = await _departmentService.GetByIdAsync(Doctor.DepartmentId.Value);
            IsHeadOfDepartment = dept?.HeadOfDepartmentId == Doctor.DoctorId;
        }
        catch { /* Ignore */ }
    }

    private async Task SaveAsync()
    {
        try
        {
            if (SelectedCollegeId == 0)
            {
                _dialogService.ShowError(_localizationService.GetString("Placeholder.SelectCollege"), _localizationService.GetString("Title.Doctors"));
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedGender))
            {
                _dialogService.ShowError(_localizationService.GetString("Placeholder.SelectGender"), _localizationService.GetString("Title.Doctors"));
                return;
            }

            Doctor.Gender = SelectedGender;
            Doctor.CollegeId = SelectedCollegeId;
            Doctor.Rank = SelectedRank ?? string.Empty;
            
            Result<DoctorDto> result;
            if (IsEditMode)
            {
                result = await _doctorService.UpdateAsync(Doctor);
            }
            else
            {
                result = await _doctorService.AddAsync(Doctor);
            }

            if (result.IsSuccess)
            {
                Doctor = result.Value!;
                Close(true);
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Doctors"));
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Doctors"));
        }
    }
}
