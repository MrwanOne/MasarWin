using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Masar.Application.Reporting;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Models;
using Masar.UI.Services;
using Masar.UI.Views;
using Masar.Domain.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class ReportsViewModel : ViewModelBase
{
    private System.Collections.Generic.List<DoctorDto> _allSupervisors = new();
    private readonly IReportService _reportService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly IDoctorService _doctorService;
    private readonly ReportDocumentBuilder _documentBuilder;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly IAcademicReportBuilder _academicReportBuilder;

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
        set
        {
            if (SetProperty(ref _selectedDepartmentId, value))
            {
                FilterSupervisors();
            }
        }
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
    public AsyncRelayCommand ExportPdfCommand { get; }

    public ReportsViewModel(
        IReportService reportService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        IDoctorService doctorService,
        ReportDocumentBuilder documentBuilder,
        IDialogService dialogService,
        ILocalizationService localizationService,
        IAcademicReportBuilder academicReportBuilder)
    {
        _reportService = reportService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _doctorService = doctorService;
        _documentBuilder = documentBuilder;
        _dialogService = dialogService;
        _localizationService = localizationService;
        _academicReportBuilder = academicReportBuilder;

        GenerateReportCommand = new AsyncRelayCommand(GenerateReportAsync);
        ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    public async Task LoadAsync()
    {
        LoadLookups();
        await Task.CompletedTask;
    }

    public async void LoadLookups()
    {
        try
        {
            Colleges.Clear();
            Colleges.Add(new CollegeDto
            {
                CollegeId = 0,
                NameAr = _localizationService.GetString("Placeholder.AllColleges"),
                NameEn = _localizationService.GetString("Placeholder.AllColleges")
            });
            var collegesToAdd = await _collegeService.GetAllAsync();
            foreach (var c in collegesToAdd) Colleges.Add(c);
            SelectedCollegeId = 0;

            var allDepts = await _departmentService.GetAllAsync();
            Departments.Clear();
            foreach (var d in allDepts) Departments.Add(d);

            _allSupervisors = (await _doctorService.GetAllAsync()).ToList();
            FilterSupervisors();

            RefreshStatuses();
            LoadYearOptions();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Reports"));
        }
    }

    private void LoadDepartmentsForCollege()
    {
        FilteredDepartments.Clear();
        FilteredDepartments.Add(new DepartmentDto
        {
            DepartmentId = 0,
            NameAr = _localizationService.GetString("Placeholder.AllDepartments"),
            NameEn = _localizationService.GetString("Placeholder.AllDepartments")
        });

        var filtered = Departments.AsEnumerable();
        if (SelectedCollegeId.HasValue && SelectedCollegeId.Value != 0)
        {
            filtered = filtered.Where(d => d.CollegeId == SelectedCollegeId.Value);
        }

        foreach (var dept in filtered.OrderBy(d => d.NameAr))
        {
            FilteredDepartments.Add(dept);
        }

        SelectedDepartmentId = 0;
    }

    private void FilterSupervisors()
    {
        Supervisors.Clear();
        Supervisors.Add(new DoctorDto
        {
            DoctorId = 0,
            FullName = _localizationService.GetString("Placeholder.SelectSupervisor")
        });

        var filtered = _allSupervisors.AsEnumerable();

        if (SelectedDepartmentId.HasValue && SelectedDepartmentId.Value != 0)
        {
            filtered = filtered.Where(d => d.DepartmentId == SelectedDepartmentId.Value);
        }
        else if (SelectedCollegeId.HasValue && SelectedCollegeId.Value != 0)
        {
            filtered = filtered.Where(d => d.CollegeId == SelectedCollegeId.Value);
        }

        foreach (var doc in filtered.OrderBy(d => d.FullName))
        {
            Supervisors.Add(doc);
        }

        SelectedSupervisorId = 0;
    }

    private async Task GenerateReportAsync()
    {
        try
        {
            var filter = CreateFilter();
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

    private async Task ExportPdfAsync()
    {
        try
        {
            var filter = CreateFilter();
            var report = await _reportService.BuildProjectReportAsync(filter);

            // التحقق من وجود بيانات
            if (report.Projects == null || report.Projects.Count == 0)
            {
                _dialogService.ShowMessage(
                    _localizationService.GetString("Message.NoProjects") ?? "لا توجد مشاريع للتصدير",
                    _localizationService.GetString("Title.Reports"));
                return;
            }

            var isArabic = _localizationService.IsArabic;

            // معاينة التقرير أولاً عبر FlowDocument
            var previewDocument = _documentBuilder.BuildProjectReport(report);
            var previewVm = new ReportPreviewViewModel(previewDocument, _localizationService);
            var previewWindow = new ReportPreviewWindow(previewVm);
            var previewResult = _dialogService.ShowDialog(previewWindow);

            // إذا أغلق المستخدم نافذة المعاينة بدون تأكيد، لا نصدر
            if (previewResult != true)
                return;

            // تسمية الملف حسب اللغة
            var fileName = isArabic
                ? $"تقرير_المشاريع_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                : $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            // فتح نافذة حفظ الملف
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = fileName,
                DefaultExt = ".pdf",
                Title = _localizationService.GetString("Dialog.SaveReport")
            };

            if (saveDialog.ShowDialog() == true)
            {
                // تحقق أمان المسار
                var outputPath = System.IO.Path.GetFullPath(saveDialog.FileName);
                if (!outputPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    outputPath += ".pdf";

                _academicReportBuilder.GenerateProjectReport(report, outputPath, isArabic);
                
                var message = _localizationService.GetString("Message.ReportExported") ?? "تم تصدير التقرير بنجاح!";
                _dialogService.ShowMessage(message, _localizationService.GetString("Title.Reports"));
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Reports"));
        }
    }

    /// <summary>
    /// بناء فلتر التقرير من القيم المحددة
    /// </summary>
    private ReportFilterDto CreateFilter()
    {
        return new ReportFilterDto
        {
            CollegeId = SelectedCollegeId == 0 ? null : SelectedCollegeId,
            DepartmentId = SelectedDepartmentId == 0 ? null : SelectedDepartmentId,
            SupervisorId = SelectedSupervisorId == 0 ? null : SelectedSupervisorId,
            Status = SelectedStatus,
            Year = SelectedYear,
            ProjectName = string.IsNullOrWhiteSpace(ProjectName) ? null : ProjectName.Trim()
        };
    }
}
