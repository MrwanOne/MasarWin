using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class AcademicTermEditViewModel : DialogViewModel
{
    private readonly IAcademicTermService _termService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly AcademicTermDto? _existingTerm;

    public bool IsEdit => _existingTerm != null;
    public string Title => IsEdit ? _localizationService.GetString("Title.EditTerm") : _localizationService.GetString("Title.AddTerm");

    private int _year = DateTime.Now.Year;
    public int Year
    {
        get => _year;
        set => SetProperty(ref _year, value);
    }

    private int _semester = 1;
    public int Semester
    {
        get => _semester;
        set => SetProperty(ref _semester, value);
    }

    private string _nameAr = string.Empty;
    public string NameAr
    {
        get => _nameAr;
        set => SetProperty(ref _nameAr, value);
    }

    private string _nameEn = string.Empty;
    public string NameEn
    {
        get => _nameEn;
        set => SetProperty(ref _nameEn, value);
    }

    private DateTime _startDate = DateTime.Today;
    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    private DateTime _endDate = DateTime.Today.AddMonths(4);
    public DateTime EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public AcademicTermEditViewModel(
        IAcademicTermService termService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        AcademicTermDto? existingTerm = null)
    {
        _termService = termService;
        _dialogService = dialogService;
        _localizationService = localizationService;
        _existingTerm = existingTerm;

        if (_existingTerm != null)
        {
            Year = _existingTerm.Year;
            Semester = _existingTerm.Semester;
            NameAr = _existingTerm.NameAr;
            NameEn = _existingTerm.NameEn;
            StartDate = _existingTerm.StartDate;
            EndDate = _existingTerm.EndDate;
            IsActive = _existingTerm.IsActive;
        }

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(_ => Close(false));
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(NameAr) || string.IsNullOrWhiteSpace(NameEn))
        {
            _dialogService.ShowError(_localizationService.GetString("Validation.TermNameRequired"), Title);
            return;
        }

        if (EndDate <= StartDate)
        {
            _dialogService.ShowError(_localizationService.GetString("Validation.EndDateAfterStart"), Title);
            return;
        }

        try
        {
            var dto = new AcademicTermDto
            {
                TermId = _existingTerm?.TermId ?? 0,
                Year = Year,
                Semester = Semester,
                NameAr = NameAr.Trim(),
                NameEn = NameEn.Trim(),
                StartDate = StartDate,
                EndDate = EndDate,
                IsActive = IsActive
            };

            if (IsEdit)
            {
                await _termService.UpdateAsync(dto);
                System.Windows.MessageBox.Show(
                    _localizationService.GetString("Message.TermUpdated"),
                    Title,
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            else
            {
                await _termService.AddAsync(dto);
                System.Windows.MessageBox.Show(
                    _localizationService.GetString("Message.TermAdded"),
                    Title,
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }

            Close(true);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message, Title);
        }
    }
}

