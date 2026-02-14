using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class CollegeEditViewModel : DialogViewModel
{
    private readonly ICollegeService _collegeService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    private CollegeDto _college = new();
    public CollegeDto College
    {
        get => _college;
        set => SetProperty(ref _college, value);
    }

    public bool IsEditMode { get; }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public CollegeEditViewModel(
        ICollegeService collegeService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        CollegeDto? college = null)
    {
        _collegeService = collegeService;
        _dialogService = dialogService;
        _localizationService = localizationService;

        College = college ?? new CollegeDto();
        IsEditMode = college != null;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(_ => Close(false));
    }

    private async Task SaveAsync()
    {
        try
        {
            Result<CollegeDto> result;
            if (IsEditMode)
            {
                result = await _collegeService.UpdateAsync(College);
            }
            else
            {
                result = await _collegeService.AddAsync(College);
            }

            if (result.IsSuccess)
            {
                College = result.Value!;
                Close(true);
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Colleges"));
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Colleges"));
        }
    }
}
