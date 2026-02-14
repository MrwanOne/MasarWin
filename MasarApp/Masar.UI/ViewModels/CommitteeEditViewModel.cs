using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class CommitteeEditViewModel : DialogViewModel
{
    private readonly ICommitteeService _committeeService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly IDoctorService _doctorService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    public ObservableCollection<CollegeDto> Colleges { get; } = new();
    public ObservableCollection<DepartmentDto> Departments { get; } = new();
    public ObservableCollection<DoctorCheckItem> AvailableDoctors { get; } = new();

    private List<DoctorDto> _allDoctors = new();
    private List<int> _initialMemberDoctorIds = new();

    private int _selectedCollegeId;
    public int SelectedCollegeId
    {
        get => _selectedCollegeId;
        set
        {
            if (SetProperty(ref _selectedCollegeId, value))
            {
                Committee.CollegeId = value;
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
                Committee.DepartmentId = value;
                LoadDoctorsForCollege();
            }
        }
    }

    private CommitteeDto _committee = new();
    public CommitteeDto Committee
    {
        get => _committee;
        set => SetProperty(ref _committee, value);
    }

    public bool IsEditMode { get; }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public CommitteeEditViewModel(
        ICommitteeService committeeService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        IDoctorService doctorService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        CommitteeDto? committee = null)
    {
        _committeeService = committeeService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _doctorService = doctorService;
        _dialogService = dialogService;
        _localizationService = localizationService;

        Committee = committee ?? new CommitteeDto();
        IsEditMode = committee != null;

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
            var placeholder = _localizationService.GetString("Placeholder.SelectCollege");
            Colleges.Add(new CollegeDto { CollegeId = 0, NameAr = placeholder, NameEn = placeholder });
            foreach (var college in await _collegeService.GetAllAsync())
            {
                Colleges.Add(college);
            }

            // Load all doctors
            _allDoctors = await _doctorService.GetAllAsync();

            // For now, initial member IDs will be empty for new committees
            _initialMemberDoctorIds = new List<int>();

            // Set selected college (will trigger LoadDepartmentsForCollegeAsync)
            _selectedCollegeId = Committee.CollegeId;
            OnPropertyChanged(nameof(SelectedCollegeId));
            await LoadDepartmentsForCollegeAsync();

            // Set selected department
            _selectedDepartmentId = Committee.DepartmentId;
            OnPropertyChanged(nameof(SelectedDepartmentId));
            LoadDoctorsForCollege();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Committee"));
        }
    }

    private async Task LoadDepartmentsForCollegeAsync()
    {
        Departments.Clear();
        var placeholder = _localizationService.GetString("Placeholder.SelectDepartment");
        Departments.Add(new DepartmentDto { DepartmentId = 0, NameAr = placeholder, NameEn = placeholder });

        if (SelectedCollegeId > 0)
        {
            var departments = await _departmentService.GetAllAsync();
            foreach (var dept in departments.Where(d => d.CollegeId == SelectedCollegeId).OrderBy(d => d.NameAr))
            {
                Departments.Add(dept);
            }
        }

        _selectedDepartmentId = 0;
        OnPropertyChanged(nameof(SelectedDepartmentId));
    }

    private void LoadDoctorsForCollege()
    {
        AvailableDoctors.Clear();

        if (SelectedCollegeId <= 0)
            return;

        // Filter doctors by college
        var filteredDoctors = _allDoctors
            .Where(d => d.CollegeId == SelectedCollegeId)
            .OrderBy(d => d.FullName);

        foreach (var doctor in filteredDoctors)
        {
            var item = new DoctorCheckItem
            {
                DoctorId = doctor.DoctorId,
                FullName = doctor.FullName,
                DepartmentName = doctor.DepartmentName,
                IsSelected = _initialMemberDoctorIds.Contains(doctor.DoctorId),
                IsChair = false // Default, can be enhanced later
            };
            AvailableDoctors.Add(item);
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
            if (SelectedCollegeId <= 0)
            {
                _dialogService.ShowError(_localizationService.GetString("Placeholder.SelectCollege"), _localizationService.GetString("Title.Committee"));
                return;
            }

            var selectedDoctors = AvailableDoctors.Where(d => d.IsSelected).ToList();
            if (!selectedDoctors.Any())
            {
                _dialogService.ShowError(
                    _localizationService.IsArabic ? "يرجى اختيار عضو واحد على الأقل" : "Please select at least one member",
                    _localizationService.GetString("Title.Committee"));
                return;
            }

            // Use SelectedDepartmentId from combobox
            Committee.DepartmentId = SelectedDepartmentId;

            Result<CommitteeDto> result;
            if (IsEditMode)
            {
                result = await _committeeService.UpdateAsync(Committee);
            }
            else
            {
                result = await _committeeService.AddAsync(Committee);
            }

            if (result.IsSuccess)
            {
                Committee = result.Value!;

                // Update committee member assignments
                var currentSelectedIds = selectedDoctors.Select(d => d.DoctorId).ToHashSet();

                // Remove members that were deselected
                foreach (var doctorId in _initialMemberDoctorIds)
                {
                    if (!currentSelectedIds.Contains(doctorId))
                    {
                        await _committeeService.RemoveDoctorAsync(Committee.CommitteeId, doctorId);
                    }
                }

                // Add newly selected members
                foreach (var doctor in selectedDoctors)
                {
                    if (!_initialMemberDoctorIds.Contains(doctor.DoctorId))
                    {
                        await _committeeService.AssignDoctorAsync(Committee.CommitteeId, doctor.DoctorId, doctor.IsChair);
                    }
                }

                Close(true);
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Committee"));
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Committee"));
        }
    }
}

// Helper class for doctor selection
public class DoctorCheckItem : ViewModelBase
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private bool _isChair;
    public bool IsChair
    {
        get => _isChair;
        set => SetProperty(ref _isChair, value);
    }
}
