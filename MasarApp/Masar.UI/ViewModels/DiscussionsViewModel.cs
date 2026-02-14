using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Masar.Application.Services;
using Masar.Domain.Enums;
using Masar.UI;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class DiscussionsViewModel : PagedViewModel<DiscussionDto>
{
    private readonly IDiscussionService _discussionService;
    private readonly ICollegeService _collegeService;
    private readonly IDepartmentService _departmentService;
    private readonly ITeamService _teamService;
    private readonly ICommitteeService _committeeService;
    private readonly IStudentEvaluationService _evaluationService;
    private readonly IStudentService _studentService;
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
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                EvaluateCommand.RaiseCanExecuteChanged();
                OpenDocumentsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanManage => _sessionService.CurrentUser?.Role is UserRole.Admin or UserRole.HeadOfDepartment;
    public bool CanEvaluate => _sessionService.CurrentUser?.Role is UserRole.Admin or UserRole.HeadOfDepartment or UserRole.Supervisor;

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public AsyncRelayCommand EvaluateCommand { get; }
    public AsyncRelayCommand OpenDocumentsCommand { get; }

    public DiscussionsViewModel(
        IDiscussionService discussionService,
        ICollegeService collegeService,
        IDepartmentService departmentService,
        ITeamService teamService,
        ICommitteeService committeeService,
        IStudentEvaluationService evaluationService,
        IStudentService studentService,
        IDialogService dialogService,
        ISessionService sessionService,
        ILocalizationService localizationService)
    {
        _discussionService = discussionService;
        _collegeService = collegeService;
        _departmentService = departmentService;
        _teamService = teamService;
        _committeeService = committeeService;
        _evaluationService = evaluationService;
        _studentService = studentService;
        _dialogService = dialogService;
        _sessionService = sessionService;
        _localizationService = localizationService;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new RelayCommand(_ => AddDiscussion(), _ => CanManage);
        EditCommand = new RelayCommand(_ => EditDiscussion(), _ => CanManage && SelectedDiscussion != null);
        DeleteCommand = new RelayCommand(_ => DeleteDiscussion(), _ => CanManage && SelectedDiscussion != null);
        EvaluateCommand = new AsyncRelayCommand(EvaluateStudentAsync, () => CanEvaluate && SelectedDiscussion != null);
        OpenDocumentsCommand = new AsyncRelayCommand(OpenDocumentsAsync, () => SelectedDiscussion != null);
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
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Discussions"));
        }
    }

    protected override bool FilterItem(DiscussionDto item, string searchText)
    {
        return item.TeamName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.CommitteeName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.Place.Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
    }

    private void AddDiscussion()
    {
        var vm = new DiscussionEditViewModel(_discussionService, _collegeService, _departmentService, _teamService, _committeeService, _dialogService, _localizationService);
        var dialog = new DiscussionDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private void EditDiscussion()
    {
        if (SelectedDiscussion == null)
        {
            return;
        }

        var vm = new DiscussionEditViewModel(_discussionService, _collegeService, _departmentService, _teamService, _committeeService, _dialogService, _localizationService, SelectedDiscussion);
        var dialog = new DiscussionDialog(vm);
        _ = vm.LoadAsync();
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    private async void DeleteDiscussion()
    {
        if (SelectedDiscussion == null)
        {
            return;
        }

        if (_dialogService.Confirm(_localizationService.GetString("Confirm.DeleteDiscussion"), _localizationService.GetString("Title.Discussions")))
        {
            var result = await _discussionService.DeleteAsync(SelectedDiscussion.DiscussionId);
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Discussions"));
            }
        }
    }

    private async Task EvaluateStudentAsync()
    {
        if (SelectedDiscussion == null) return;

        try
        {
            // Get all students and filter by team
            var allStudents = await _studentService.GetAllAsync();
            var teamStudents = allStudents.Where(s => s.TeamId == SelectedDiscussion.TeamId).ToList();
            
            if (teamStudents.Count == 0)
            {
                _dialogService.ShowError(
                    _localizationService.IsArabic ? "الفريق لا يحتوي على طلاب" : "Team has no students",
                    _localizationService.GetString("Title.Evaluation"));
                return;
            }

            // Let user select a student if multiple
            int selectedStudentId;
            string selectedStudentName;

            if (teamStudents.Count == 1)
            {
                selectedStudentId = teamStudents[0].StudentId;
                selectedStudentName = teamStudents[0].FullName;
            }
            else
            {
                // Show selection dialog
                var studentList = string.Join("\n", teamStudents.Select((s, i) => $"{i + 1}. {s.FullName} ({s.StudentNumber})"));
                var message = _localizationService.IsArabic
                    ? $"اختر الطالب للتقييم:\n\n{studentList}\n\nأدخل الرقم:"
                    : $"Select student to evaluate:\n\n{studentList}\n\nEnter number:";

                var input = "";
                var inputVm = new InputDialogViewModel(
                    _localizationService.GetString("Title.Evaluation"),
                    message);
                var inputDialog = new InputDialogWindow(inputVm);
                if (_dialogService.ShowDialog(inputDialog) == true)
                {
                    input = inputVm.InputText;
                }
                else
                {
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(input)) return;

                if (int.TryParse(input, out int selection) && selection > 0 && selection <= teamStudents.Count)
                {
                    var student = teamStudents[selection - 1];
                    selectedStudentId = student.StudentId;
                    selectedStudentName = student.FullName;
                }
                else
                {
                    _dialogService.ShowError(
                        _localizationService.IsArabic ? "رقم غير صحيح" : "Invalid number",
                        _localizationService.GetString("Title.Evaluation"));
                    return;
                }
            }

            // Open evaluation dialog
            var vm = new StudentEvaluationViewModel(
                _evaluationService,
                _dialogService,
                _localizationService,
                SelectedDiscussion,
                selectedStudentId,
                selectedStudentName);
            
            var dialog = new StudentEvaluationDialog(vm);
            _ = vm.LoadAsync();
            var result = _dialogService.ShowDialog(dialog);
            
            if (result == true)
            {
                System.Windows.MessageBox.Show(
                    _localizationService.IsArabic 
                        ? $"تم تقييم الطالب {selectedStudentName} بنجاح" 
                        : $"Student {selectedStudentName} evaluated successfully",
                    _localizationService.GetString("Title.Evaluation"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Evaluation"));
        }
    }

    private async Task OpenDocumentsAsync()
    {
        if (SelectedDiscussion == null) return;

        // Assuming _host is available or we use service locator for now if not injected.
        // Actually, better to inject IDocumentService and others.
        // But for now, I'll use the same pattern as ProjectsViewModel if I can get access to IServiceProvider.
        // DependencyInjection in this app seems to use App.Current as a wrapper for host.
        
        var services = ((App)System.Windows.Application.Current).ServiceProvider;
        
        var vm = new DocumentsViewModel(
            services.GetRequiredService<IDocumentService>(),
            _dialogService,
            _localizationService,
            services.GetRequiredService<IToastService>(),
            SelectedDiscussion.DiscussionId,
            "Discussion");

        var dialog = new DocumentsDialog { DataContext = vm };
        await vm.LoadAsync();
        _dialogService.ShowDialog(dialog);
    }
}
