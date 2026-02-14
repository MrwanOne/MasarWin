using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.Domain.Enums;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class CommitteesViewModel : PagedViewModel<CommitteeDto>
{
    private readonly ICommitteeService _committeeService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly IDoctorService _doctorService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;

    private CommitteeDto? _selectedCommittee;
    public CommitteeDto? SelectedCommittee
    {
        get => _selectedCommittee;
        set
        {
            if (SetProperty(ref _selectedCommittee, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                AddMemberCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanManage => _sessionService.CurrentUser?.Role is UserRole.Admin or UserRole.HeadOfDepartment;

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand AddMemberCommand { get; }

    public CommitteesViewModel(
        ICommitteeService committeeService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        IDoctorService doctorService,
        IDialogService dialogService,
        ISessionService sessionService,
        ILocalizationService localizationService)
    {
        _committeeService = committeeService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _doctorService = doctorService;
        _dialogService = dialogService;
        _sessionService = sessionService;
        _localizationService = localizationService;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new RelayCommand(_ => AddCommittee(), _ => CanManage);
        EditCommand = new RelayCommand(_ => EditCommittee(), _ => CanManage && SelectedCommittee != null);
        DeleteCommand = new RelayCommand(_ => DeleteCommittee(), _ => CanManage && SelectedCommittee != null);
        AddMemberCommand = new RelayCommand(_ => AddMember(), _ => CanManage && SelectedCommittee != null);
    }

    public async Task LoadAsync()
    {
        try
        {
            var committees = await _committeeService.GetAllAsync();
            SetItems(committees.OrderBy(c => c.Name));
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Committees"));
        }
    }

    protected override bool FilterItem(CommitteeDto item, string searchText)
    {
        return item.Name.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.DepartmentName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.CollegeName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.TermName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
    }

    private void AddCommittee()
    {
        var vm = new CommitteeEditViewModel(_committeeService, _collegeService, _departmentService, _doctorService, _dialogService, _localizationService);
        var dialog = new CommitteeDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private void EditCommittee()
    {
        if (SelectedCommittee == null)
        {
            return;
        }

        var vm = new CommitteeEditViewModel(_committeeService, _collegeService, _departmentService, _doctorService, _dialogService, _localizationService, SelectedCommittee);
        var dialog = new CommitteeDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private async void DeleteCommittee()
    {
        if (SelectedCommittee == null)
        {
            return;
        }

        if (_dialogService.Confirm(_localizationService.GetString("Confirm.DeleteCommittee"), _localizationService.GetString("Title.Committees")))
        {
            var result = await _committeeService.DeleteAsync(SelectedCommittee.CommitteeId);
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Committees"));
            }
        }
    }

    private async void AddMember()
    {
        if (SelectedCommittee == null)
        {
            return;
        }

        var vm = new CommitteeMemberAssignViewModel(_doctorService, _dialogService, _localizationService, SelectedCommittee.DepartmentId, SelectedCommittee.CollegeId);
        var dialog = new CommitteeMemberDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true && vm.SelectedDoctor != null)
        {
            var assignResult = await _committeeService.AssignDoctorAsync(SelectedCommittee.CommitteeId, vm.SelectedDoctor.DoctorId, vm.IsChair);
            if (assignResult.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                _dialogService.ShowError(assignResult.Message, _localizationService.GetString("Title.Committees"));
            }
        }
    }
}
