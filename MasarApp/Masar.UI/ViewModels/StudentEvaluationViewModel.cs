using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class StudentEvaluationViewModel : ViewModelBase
{
    private readonly IStudentEvaluationService _evaluationService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    public DiscussionDto Discussion { get; }
    public int SelectedStudentId { get; }
    public string StudentName { get; }

    public ObservableCollection<CriteriaInputItem> CriteriaItems { get; } = new();

    private string _generalFeedback = string.Empty;
    public string GeneralFeedback
    {
        get => _generalFeedback;
        set => SetProperty(ref _generalFeedback, value);
    }

    private string _strengthPoints = string.Empty;
    public string StrengthPoints
    {
        get => _strengthPoints;
        set => SetProperty(ref _strengthPoints, value);
    }

    private string _improvementAreas = string.Empty;
    public string ImprovementAreas
    {
        get => _improvementAreas;
        set => SetProperty(ref _improvementAreas, value);
    }

    private decimal _contributionPercentage = 100;
    public decimal ContributionPercentage
    {
        get => _contributionPercentage;
        set => SetProperty(ref _contributionPercentage, value);
    }

    public decimal TotalScore => CriteriaItems.Sum(c => c.Score);
    public decimal MaxTotalScore => CriteriaItems.Sum(c => c.MaxScore);

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public event EventHandler<bool>? RequestClose;

    public StudentEvaluationViewModel(
        IStudentEvaluationService evaluationService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        DiscussionDto discussion,
        int studentId,
        string studentName)
    {
        _evaluationService = evaluationService;
        _dialogService = dialogService;
        _localizationService = localizationService;
        Discussion = discussion;
        SelectedStudentId = studentId;
        StudentName = studentName;

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => CriteriaItems.Count > 0);
        CancelCommand = new RelayCommand(_ => Close(false));
    }

    public async Task LoadAsync()
    {
        try
        {
            CriteriaItems.Clear();
            var criteria = await _evaluationService.GetCriteriaAsync();
            
            foreach (var c in criteria.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder))
            {
                var item = new CriteriaInputItem
                {
                    CriteriaId = c.CriteriaId,
                    NameAr = c.NameAr,
                    NameEn = c.NameEn,
                    DescriptionAr = c.DescriptionAr,
                    DescriptionEn = c.DescriptionEn,
                    MaxScore = c.MaxScore,
                    Score = 0,
                    Comments = string.Empty
                };
                item.PropertyChanged += (_, _) => OnPropertyChanged(nameof(TotalScore));
                CriteriaItems.Add(item);
            }

            SaveCommand.RaiseCanExecuteChanged();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Evaluation"));
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            var dto = new StudentEvaluationDto
            {
                DiscussionId = Discussion.DiscussionId,
                StudentId = SelectedStudentId,
                TotalScore = TotalScore,
                ContributionPercentage = ContributionPercentage,
                GeneralFeedback = GeneralFeedback,
                StrengthPoints = StrengthPoints,
                ImprovementAreas = ImprovementAreas,
                EvaluatedAt = DateTime.Now,
                CriteriaScores = CriteriaItems.Select(c => new CriteriaScoreDto
                {
                    CriteriaId = c.CriteriaId,
                    Score = c.Score,
                    MaxScore = c.MaxScore,
                    Comments = c.Comments
                }).ToList()
            };

            var result = await _evaluationService.AddAsync(dto);
            if (result.IsSuccess)
            {
                Close(true);
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Evaluation"));
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Evaluation"));
        }
    }

    private void Close(bool result)
    {
        RequestClose?.Invoke(this, result);
    }
}

public class CriteriaInputItem : ViewModelBase
{
    public int CriteriaId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }

    private decimal _score;
    public decimal Score
    {
        get => _score;
        set
        {
            if (value < 0) value = 0;
            if (value > MaxScore) value = MaxScore;
            SetProperty(ref _score, value);
        }
    }

    private string _comments = string.Empty;
    public string Comments
    {
        get => _comments;
        set => SetProperty(ref _comments, value);
    }
}
