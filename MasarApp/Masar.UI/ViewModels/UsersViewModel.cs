using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.Domain.Enums;
using Masar.UI.Controls;
using Masar.UI.Services;
using Masar.UI.Views;
using System.Linq;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class UsersViewModel : PagedViewModel<UserDto>
{
    private readonly IUserService _userService;
    private readonly IDoctorService _doctorService;
    private readonly IStudentService _studentService;
    private readonly IDialogService _dialogService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;

    private UserDto? _selectedUser;
    public UserDto? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                ToggleActiveCommand.RaiseCanExecuteChanged();
                ResetPasswordCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanManage => _sessionService.CurrentUser?.Role == UserRole.Admin;

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand AddCommand { get; }
    public AsyncRelayCommand EditCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }
    public AsyncRelayCommand ToggleActiveCommand { get; }
    public AsyncRelayCommand ResetPasswordCommand { get; }

    public UsersViewModel(
        IUserService userService,
        IDoctorService doctorService,
        IStudentService studentService,
        IDialogService dialogService,
        ISessionService sessionService,
        ILocalizationService localizationService)
    {
        _userService = userService;
        _doctorService = doctorService;
        _studentService = studentService;
        _dialogService = dialogService;
        _sessionService = sessionService;
        _localizationService = localizationService;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new AsyncRelayCommand(AddUserAsync, () => CanManage);
        EditCommand = new AsyncRelayCommand(EditUserAsync, () => CanManage && SelectedUser != null);
        DeleteCommand = new AsyncRelayCommand(DeleteUserAsync, () => CanManage && SelectedUser != null);
        ToggleActiveCommand = new AsyncRelayCommand(ToggleActiveAsync, () => CanManage && SelectedUser != null);
        ResetPasswordCommand = new AsyncRelayCommand(ResetPasswordAsync, () => CanManage && SelectedUser != null);
    }

    public async Task LoadAsync()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            SetItems(users.OrderBy(u => u.Username));
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Users"));
        }
    }

    protected override bool FilterItem(UserDto item, string searchText)
    {
        return item.Username.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
            || item.Role.ToString().Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
    }

    private async Task AddUserAsync()
    {
        var vm = new UserEditViewModel(_userService, _doctorService, _studentService, _dialogService, _localizationService);
        var dialog = new UserDialog(vm);
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            await LoadAsync();
        }
    }

    private async Task EditUserAsync()
    {
        if (SelectedUser == null)
        {
            return;
        }

        var vm = new UserEditViewModel(_userService, _doctorService, _studentService, _dialogService, _localizationService, SelectedUser);
        var dialog = new UserDialog(vm);
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            await LoadAsync();
        }
    }

    private async Task DeleteUserAsync()
    {
        if (SelectedUser == null)
        {
            return;
        }

        if (_dialogService.Confirm(_localizationService.GetString("Confirm.DeleteUser"), _localizationService.GetString("Title.Users")))
        {
            var result = await _userService.DeleteAsync(SelectedUser.UserId);
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Users"));
            }
        }
    }

    private async Task ToggleActiveAsync()
    {
        if (SelectedUser == null)
        {
            return;
        }

        var result = await _userService.SetActiveAsync(SelectedUser.UserId, !SelectedUser.IsActive);
        if (result.IsSuccess)
        {
            await LoadAsync();
        }
        else
        {
            _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Users"));
        }
    }

    private async Task ResetPasswordAsync()
    {
        if (SelectedUser == null)
        {
            return;
        }

        var vm = new InputDialogViewModel(
            _localizationService.GetString("Dialog.ResetPasswordTitle"),
            _localizationService.GetString("Dialog.ResetPasswordPrompt"));
        var dialog = new InputDialogWindow(vm);
        var result = _dialogService.ShowDialog(dialog);
        if (result == true)
        {
            await ResetPasswordInternalAsync(vm.InputText);
        }
    }

    // ...existing code...
    private async Task ResetPasswordInternalAsync(string password)
    {
        if (SelectedUser == null)
        {
            return;
        }

        try
        {
            var result = await _userService.ResetPasswordAsync(SelectedUser.UserId, password);
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Users"));
            }
        }
        catch (System.Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Users"));
        }
    }
}

