using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class DepartmentsViewModel : PagedViewModel<DepartmentDto>
{
    private readonly IDepartmentService _departmentService;
    private readonly ICollegeService _collegeService;
    private readonly IDoctorService _doctorService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;
    private bool _isLoading;

    public ObservableCollection<CollegeDto> Colleges { get; } = new();

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

    private DepartmentDto? _selectedDepartment;
    public DepartmentDto? SelectedDepartment
    {
        get => _selectedDepartment;
        set
        {
            if (SetProperty(ref _selectedDepartment, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanManage => _sessionService.CurrentUser?.Role is Domain.Enums.UserRole.Admin or Domain.Enums.UserRole.HeadOfDepartment;

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public DepartmentsViewModel(
        IDepartmentService departmentService,
        ICollegeService collegeService,
        IDoctorService doctorService,
        IDialogService dialogService,
        ISessionService sessionService,
        ILocalizationService localizationService)
    {
        _departmentService = departmentService;
        _collegeService = collegeService;
        _doctorService = doctorService;
        _dialogService = dialogService;
        _sessionService = sessionService;
        _localizationService = localizationService;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new RelayCommand(_ => AddDepartment(), _ => CanManage && SelectedCollegeId > 0);
        EditCommand = new RelayCommand(_ => EditDepartment(), _ => CanManage && SelectedDepartment != null);
        DeleteCommand = new RelayCommand(_ => DeleteDepartment(), _ => CanManage && SelectedDepartment != null);
    }

    public async Task LoadAsync()
    {
        if (_isLoading) return;
        _isLoading = true;
        try
        {
            Colleges.Clear();
            var placeholder = new CollegeDto { CollegeId = 0, NameAr = _localizationService.GetString("Placeholder.AllColleges") };
            Colleges.Add(placeholder);

            var colleges = await _collegeService.GetAllAsync();
            foreach (var college in colleges.OrderBy(c => c.NameAr))
            {
                Colleges.Add(college);
            }

            _selectedCollegeId = 0;
            OnPropertyChanged(nameof(SelectedCollegeId));
            await LoadDepartmentsForCollegeAsync();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Departments"));
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadDepartmentsForCollegeAsync()
    {
        try
        {
            var departments = await _departmentService.GetAllAsync();

            if (SelectedCollegeId == 0)
            {
                // Show all departments when "All Colleges" is selected
                SetItems(departments.OrderBy(d => d.NameAr));
            }
            else
            {
                var filtered = departments.Where(d => d.CollegeId == SelectedCollegeId).OrderBy(d => d.NameAr);
                SetItems(filtered);
            }

            AddCommand.RaiseCanExecuteChanged();
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Departments"));
        }
    }

    protected override bool FilterItem(DepartmentDto item, string searchText)
    {
        return item.NameAr.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.HeadOfDepartmentName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.Code.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.CollegeName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
    }

    private void AddDepartment()
    {
        var vm = new DepartmentEditViewModel(_departmentService, _collegeService, _doctorService, _dialogService, _localizationService);
        var dialog = new DepartmentDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private void EditDepartment()
    {
        if (SelectedDepartment == null)
        {
            return;
        }

        var vm = new DepartmentEditViewModel(_departmentService, _collegeService, _doctorService, _dialogService, _localizationService, SelectedDepartment);
        var dialog = new DepartmentDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private async void DeleteDepartment()
    {
        if (SelectedDepartment == null)
        {
            return;
        }

        if (_dialogService.Confirm(_localizationService.GetString("Confirm.DeleteDepartment"), _localizationService.GetString("Title.Departments")))
        {
            var result = await _departmentService.DeleteAsync(SelectedDepartment.DepartmentId);
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Departments"));
            }
        }
    }
}
