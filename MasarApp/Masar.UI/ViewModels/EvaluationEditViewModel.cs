using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class EvaluationEditViewModel : DialogViewModel
{
    private readonly IDiscussionService _discussionService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    public DiscussionDto Discussion { get; }

    private decimal _supervisorScore;
    public decimal SupervisorScore
    {
        get => _supervisorScore;
        set => SetProperty(ref _supervisorScore, value);
    }

    private decimal _committeeScore;
    public decimal CommitteeScore
    {
        get => _committeeScore;
        set => SetProperty(ref _committeeScore, value);
    }

    private string _reportText = string.Empty;
    public string ReportText
    {
        get => _reportText;
        set => SetProperty(ref _reportText, value);
    }

    private string _reportFilePath = string.Empty;
    public string ReportFilePath
    {
        get => _reportFilePath;
        set => SetProperty(ref _reportFilePath, value);
    }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public EvaluationEditViewModel(IDiscussionService discussionService, IDialogService dialogService, ILocalizationService localizationService, DiscussionDto discussion)
    {
        _discussionService = discussionService;
        _dialogService = dialogService;
        _localizationService = localizationService;
        Discussion = discussion;
        _supervisorScore = discussion.SupervisorScore;
        _committeeScore = discussion.CommitteeScore;
        _reportText = discussion.ReportText;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(_ => Close(false));
    }

    private async Task SaveAsync()
    {
        try
        {
            await _discussionService.SaveEvaluationAsync(Discussion.DiscussionId, SupervisorScore, CommitteeScore, ReportText);
            Close(true);
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Evaluation"));
        }
    }
}
