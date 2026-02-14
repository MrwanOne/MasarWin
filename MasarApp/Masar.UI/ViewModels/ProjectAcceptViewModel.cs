using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class ProjectAcceptViewModel : DialogViewModel
{
    private readonly IDoctorService _doctorService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly int _departmentId;

    public ObservableCollection<DoctorDto> Supervisors { get; } = new();

    private DoctorDto? _selectedSupervisor;
    public DoctorDto? SelectedSupervisor
    {
        get => _selectedSupervisor;
        set => SetProperty(ref _selectedSupervisor, value);
    }

    public AsyncRelayCommand LoadCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public ProjectAcceptViewModel(IDoctorService doctorService, IDialogService dialogService, ILocalizationService localizationService, int departmentId)
    {
        _doctorService = doctorService;
        _dialogService = dialogService;
        _localizationService = localizationService;
        _departmentId = departmentId;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => Close(false));
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    public async Task LoadAsync()
    {
        try
        {
            Supervisors.Clear();
            var placeholder = CreatePlaceholder();
            Supervisors.Add(placeholder);
            var doctors = await _doctorService.GetAllAsync();
            var filtered = doctors.Where(d => d.DepartmentId == _departmentId).ToList();
            foreach (var doctor in filtered)
            {
                Supervisors.Add(doctor);
            }

            SelectedSupervisor = SelectedSupervisor != null && SelectedSupervisor.DoctorId != 0
                ? Supervisors.FirstOrDefault(d => d.DoctorId == SelectedSupervisor.DoctorId)
                : placeholder;
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Project"));
        }
    }

    private DoctorDto CreatePlaceholder() => new() { DoctorId = 0, FullName = _localizationService.GetString("Placeholder.SelectSupervisor") };

    private void OnLanguageChanged(object? sender, System.EventArgs e)
    {
        _ = LoadAsync();
    }

    private void Save()
    {
        if (SelectedSupervisor == null || SelectedSupervisor.DoctorId == 0)
        {
            _dialogService.ShowError(_localizationService.GetString("Placeholder.SelectSupervisor"), _localizationService.GetString("Title.Project"));
            return;
        }

        Close(true);
    }
}
