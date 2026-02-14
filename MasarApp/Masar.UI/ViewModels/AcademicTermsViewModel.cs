using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class AcademicTermsViewModel : PagedViewModel<AcademicTermDto>
{
    private readonly IAcademicTermService _termService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;

    private AcademicTermDto? _selectedTerm;
    public AcademicTermDto? SelectedTerm
    {
        get => _selectedTerm;
        set
        {
            if (SetProperty(ref _selectedTerm, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                SetActiveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanManage => _sessionService.CurrentUser?.Role is Domain.Enums.UserRole.Admin;

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand SetActiveCommand { get; }

    public AcademicTermsViewModel(
        IAcademicTermService termService,
        IDialogService dialogService,
        ISessionService sessionService,
        ILocalizationService localizationService)
    {
        _termService = termService;
        _dialogService = dialogService;
        _sessionService = sessionService;
        _localizationService = localizationService;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new RelayCommand(_ => AddTerm(), _ => CanManage);
        EditCommand = new RelayCommand(_ => EditTerm(), _ => CanManage && SelectedTerm != null);
        DeleteCommand = new RelayCommand(_ => DeleteTerm(), _ => CanManage && SelectedTerm != null);
        SetActiveCommand = new RelayCommand(_ => SetActiveTerm(), _ => CanManage && SelectedTerm != null && !SelectedTerm.IsActive);
    }

    public async Task LoadAsync()
    {
        try
        {
            var terms = await _termService.GetAllAsync();
            SetItems(terms.OrderByDescending(t => t.Year).ThenByDescending(t => t.Semester));
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.AcademicTerms"));
        }
    }

    protected override bool FilterItem(AcademicTermDto item, string searchText)
    {
        return item.NameAr.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.NameEn.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.Year.ToString().Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
    }

    private void AddTerm()
    {
        var vm = new AcademicTermEditViewModel(_termService, _dialogService, _localizationService);
        var dialog = new AcademicTermDialog(vm);
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private void EditTerm()
    {
        if (SelectedTerm == null) return;

        var vm = new AcademicTermEditViewModel(_termService, _dialogService, _localizationService, SelectedTerm);
        var dialog = new AcademicTermDialog(vm);
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private async void DeleteTerm()
    {
        if (SelectedTerm == null) return;

        if (_dialogService.Confirm(_localizationService.GetString("Confirm.DeleteTerm"), _localizationService.GetString("Title.AcademicTerms")))
        {
            try
            {
                await _termService.DeleteAsync(SelectedTerm.TermId);
                await LoadAsync();
            }
            catch (System.Exception ex)
            {
                _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.AcademicTerms"));
            }
        }
    }

    private async void SetActiveTerm()
    {
        if (SelectedTerm == null) return;

        try
        {
            await _termService.SetActiveTermAsync(SelectedTerm.TermId);
            await LoadAsync();
            System.Windows.MessageBox.Show(_localizationService.GetString("Message.TermActivated"), _localizationService.GetString("Title.AcademicTerms"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.AcademicTerms"));
        }
    }
}
