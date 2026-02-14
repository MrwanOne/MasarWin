using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.Domain.Enums;
using Masar.UI.Models;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class ReportsViewModel : ViewModelBase
{
    private readonly IReportService _reportService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly IDoctorService _doctorService;
    private readonly ReportDocumentBuilder _documentBuilder;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    public ObservableCollection<CollegeDto> Colleges { get; } = new();
    public ObservableCollection<DepartmentDto> Departments { get; } = new();
    public ObservableCollection<DepartmentDto> FilteredDepartments { get; } = new();
    public ObservableCollection<DoctorDto> Supervisors { get; } = new();
    public ObservableCollection<StatusOption> StatusOptions { get; } = new();
    public ObservableCollection<OptionItem<int>> YearOptions { get; } = new();

    private int? _selectedCollegeId;
    public int? SelectedCollegeId
    {
        get => _selectedCollegeId;
        set
        {
            if (SetProperty(ref _selectedCollegeId, value))
            {
                LoadDepartmentsForCollege();
            }
        }
    }

    private int? _selectedDepartmentId;
    public int? SelectedDepartmentId
    {
        get => _selectedDepartmentId;
        set => SetProperty(ref _selectedDepartmentId, value);
    }

    private int? _selectedSupervisorId;
    public int? SelectedSupervisorId
    {
        get => _selectedSupervisorId;
        set => SetProperty(ref _selectedSupervisorId, value);
    }

    private ProjectStatus? _selectedStatus;
    public ProjectStatus? SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    private int? _selectedYear;
    public int? SelectedYear
    {
        get => _selectedYear;
        set => SetProperty(ref _selectedYear, value);
    }

    private string _projectName = string.Empty;
    public string ProjectName
    {
        get => _projectName;
        set => SetProperty(ref _projectName, value);
    }

    public AsyncRelayCommand GenerateReportCommand { get; }

    public ReportsViewModel(
        IReportService reportService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        IDoctorService doctorService,
        ReportDocumentBuilder documentBuilder,
        IDialogService dialogService,
        ILocalizationService localizationService)
    {
        _reportService = reportService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _doctorService = doctorService;
        _documentBuilder = documentBuilder;
        _dialogService = dialogService;
        _localizationService = localizationService;

        GenerateReportCommand = new AsyncRelayCommand(GenerateReportAsync);
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    public Task LoadAsync()
    {
        LoadLookups();
        return Task.CompletedTask;
    }

    private async void LoadLookups()
    {
        try
        {
            Colleges.Clear();
            var collegePlaceholder = _localizationService.GetString("Placeholder.SelectCollege");
            Colleges.Add(new CollegeDto { CollegeId = 0, NameEn = collegePlaceholder, NameAr = collegePlaceholder });
            foreach (var college in await _collegeService.GetAllAsync())
            {
                Colleges.Add(college);
            }

            Departments.Clear();
            var departmentPlaceholder = _localizationService.GetString("Placeholder.SelectDepartment");
            Departments.Add(new DepartmentDto { DepartmentId = 0, NameEn = departmentPlaceholder, NameAr = departmentPlaceholder });
            foreach (var dept in await _departmentService.GetAllAsync())
            {
                Departments.Add(dept);
            }

            Supervisors.Clear();
            var supervisorPlaceholder = _localizationService.GetString("Placeholder.SelectSupervisor");
            Supervisors.Add(new DoctorDto { DoctorId = 0, FullName = supervisorPlaceholder });
            foreach (var doc in await _doctorService.GetAllAsync())
            {
                Supervisors.Add(doc);
            }

            RefreshStatuses();
            LoadYearOptions();

            SelectedCollegeId = SelectedCollegeId ?? 0;
            LoadDepartmentsForCollege();
            SelectedSupervisorId = SelectedSupervisorId ?? 0;
            SelectedYear = SelectedYear;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Reports"));
        }
    }

    private void LoadDepartmentsForCollege()
    {
        FilteredDepartments.Clear();
        var placeholder = _localizationService.GetString("Placeholder.SelectDepartment");
        FilteredDepartments.Add(new DepartmentDto { DepartmentId = 0, NameEn = placeholder, NameAr = placeholder });

        if (!SelectedCollegeId.HasValue || SelectedCollegeId.Value == 0)
        {
            SelectedDepartmentId = 0;
            return;
        }

        foreach (var dept in Departments.Where(d => d.CollegeId == SelectedCollegeId.Value))
        {
            FilteredDepartments.Add(dept);
        }

        SelectedDepartmentId = SelectedDepartmentId.HasValue && SelectedDepartmentId.Value != 0
            ? SelectedDepartmentId
            : 0;
    }

    private async Task GenerateReportAsync()
    {
        try
        {
            var filter = new ReportFilterDto
            {
                CollegeId = SelectedCollegeId == 0 ? null : SelectedCollegeId,
                DepartmentId = SelectedDepartmentId == 0 ? null : SelectedDepartmentId,
                SupervisorId = SelectedSupervisorId == 0 ? null : SelectedSupervisorId,
                Status = SelectedStatus,
                Year = SelectedYear,
                ProjectName = string.IsNullOrWhiteSpace(ProjectName) ? null : ProjectName.Trim()
            };

            var report = await _reportService.BuildProjectReportAsync(filter);
            var document = _documentBuilder.BuildProjectReport(report);
            var vm = new ReportPreviewViewModel(document, _localizationService);
            var window = new ReportPreviewWindow(vm);
            _dialogService.ShowDialog(window);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Reports"));
        }
    }

    private void RefreshStatuses()
    {
        var current = SelectedStatus;
        StatusOptions.Clear();
        StatusOptions.Add(new StatusOption(null, _localizationService.GetString("Placeholder.SelectStatus")));
        foreach (var option in _localizationService.GetStatusOptions())
        {
            StatusOptions.Add(option);
        }
        SelectedStatus = current;
    }

    private void LoadYearOptions()
    {
        var current = SelectedYear;
        YearOptions.Clear();
        YearOptions.Add(new OptionItem<int>(default, _localizationService.GetString("Placeholder.SelectYear")));
        var currentYear = DateTime.Now.Year;
        for (var year = currentYear; year >= currentYear - 10; year--)
        {
            YearOptions.Add(new OptionItem<int>(year, year.ToString()));
        }

        SelectedYear = current;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        LoadLookups();
    }
}
