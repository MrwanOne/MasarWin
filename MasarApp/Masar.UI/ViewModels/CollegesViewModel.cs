using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class CollegesViewModel : PagedViewModel<CollegeDto>
{
    private readonly ICollegeService _collegeService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;

    private CollegeDto? _selectedCollege;
    public CollegeDto? SelectedCollege
    {
        get => _selectedCollege;
        set
        {
            if (SetProperty(ref _selectedCollege, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanManage => _sessionService.CurrentUser?.Role is Domain.Enums.UserRole.Admin;

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public CollegesViewModel(
        ICollegeService collegeService,
        IDialogService dialogService,
        ISessionService sessionService,
        ILocalizationService localizationService)
    {
        _collegeService = collegeService;
        _dialogService = dialogService;
        _sessionService = sessionService;
        _localizationService = localizationService;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new RelayCommand(_ => AddCollege(), _ => CanManage);
        EditCommand = new RelayCommand(_ => EditCollege(), _ => CanManage && SelectedCollege != null);
        DeleteCommand = new RelayCommand(_ => DeleteCollege(), _ => CanManage && SelectedCollege != null);
    }

    public async Task LoadAsync()
    {
        try
        {
            var colleges = await _collegeService.GetAllAsync();
            SetItems(colleges.OrderBy(c => c.NameEn));
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Colleges"));
        }
    }

    protected override bool FilterItem(CollegeDto item, string searchText)
    {
        return item.NameAr.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.NameEn.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.Code.Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
    }

    private void AddCollege()
    {
        var vm = new CollegeEditViewModel(_collegeService, _dialogService, _localizationService);
        var dialog = new CollegeDialog(vm);
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private void EditCollege()
    {
        if (SelectedCollege == null)
        {
            return;
        }

        var vm = new CollegeEditViewModel(_collegeService, _dialogService, _localizationService, SelectedCollege);
        var dialog = new CollegeDialog(vm);
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private async void DeleteCollege()
    {
        if (SelectedCollege == null)
        {
            return;
        }

        if (_dialogService.Confirm(_localizationService.GetString("Confirm.DeleteCollege"), _localizationService.GetString("Title.Colleges")))
        {
            var result = await _collegeService.DeleteAsync(SelectedCollege.CollegeId);
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Colleges"));
            }
        }
    }
}
