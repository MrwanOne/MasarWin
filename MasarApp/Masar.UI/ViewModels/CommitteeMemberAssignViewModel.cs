using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class CommitteeMemberAssignViewModel : DialogViewModel
{
    private readonly IDoctorService _doctorService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly int _departmentId;
    private readonly int _collegeId;

    public ObservableCollection<DoctorDto> Doctors { get; } = new();

    private DoctorDto? _selectedDoctor;
    public DoctorDto? SelectedDoctor
    {
        get => _selectedDoctor;
        set => SetProperty(ref _selectedDoctor, value);
    }

    private bool _isChair;
    public bool IsChair
    {
        get => _isChair;
        set => SetProperty(ref _isChair, value);
    }

    public AsyncRelayCommand LoadCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public CommitteeMemberAssignViewModel(IDoctorService doctorService, IDialogService dialogService, ILocalizationService localizationService, int departmentId, int collegeId)
    {
        _doctorService = doctorService;
        _dialogService = dialogService;
        _localizationService = localizationService;
        _departmentId = departmentId;
        _collegeId = collegeId;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => Close(false));
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    public async Task LoadAsync()
    {
        try
        {
            Doctors.Clear();
            var placeholder = CreatePlaceholderDoctor();
            Doctors.Add(placeholder);
            var doctors = await _doctorService.GetAllAsync();
            foreach (var doctor in doctors.Where(d => d.CollegeId == _collegeId && d.DepartmentId == _departmentId))
            {
                Doctors.Add(doctor);
            }

            SelectedDoctor = SelectedDoctor != null && SelectedDoctor.DoctorId != 0
                ? Doctors.FirstOrDefault(d => d.DoctorId == SelectedDoctor.DoctorId)
                : placeholder;
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Committee"));
        }
    }

    private DoctorDto CreatePlaceholderDoctor() => new() { DoctorId = 0, FullName = _localizationService.GetString("Placeholder.SelectDoctor") };

    private void OnLanguageChanged(object? sender, System.EventArgs e)
    {
        _ = LoadAsync();
    }

    private void Save()
    {
        if (SelectedDoctor == null || SelectedDoctor.DoctorId == 0)
        {
            _dialogService.ShowError(_localizationService.GetString("Placeholder.SelectDoctor"), _localizationService.GetString("Title.Committee"));
            return;
        }

        Close(true);
    }
}
