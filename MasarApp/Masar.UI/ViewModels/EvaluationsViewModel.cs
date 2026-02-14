using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.Domain.Enums;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class EvaluationsViewModel : PagedViewModel<DiscussionDto>
{
    private readonly IDiscussionService _discussionService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;

    private DiscussionDto? _selectedDiscussion;
    public DiscussionDto? SelectedDiscussion
    {
        get => _selectedDiscussion;
        set
        {
            if (SetProperty(ref _selectedDiscussion, value))
            {
                EvaluateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanEvaluate => _sessionService.CurrentUser?.Role is UserRole.Admin or UserRole.HeadOfDepartment or UserRole.Supervisor;

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand EvaluateCommand { get; }

    public EvaluationsViewModel(IDiscussionService discussionService, IDialogService dialogService, ISessionService sessionService, ILocalizationService localizationService)
    {
        _discussionService = discussionService;
        _dialogService = dialogService;
        _sessionService = sessionService;
        _localizationService = localizationService;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        EvaluateCommand = new RelayCommand(_ => Evaluate(), _ => CanEvaluate && SelectedDiscussion != null);
    }

    public async Task LoadAsync()
    {
        try
        {
            var discussions = await _discussionService.GetAllAsync();
            SetItems(discussions.OrderBy(d => d.StartTime));
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Evaluations"));
        }
    }

    protected override bool FilterItem(DiscussionDto item, string searchText)
    {
        return item.TeamName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.CommitteeName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.Place.Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
    }

    private void Evaluate()
    {
        if (SelectedDiscussion == null)
        {
            return;
        }

        var vm = new EvaluationEditViewModel(_discussionService, _dialogService, _localizationService, SelectedDiscussion);
        var dialog = new EvaluationDialog(vm);
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }
}
