using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.UI.Controls;
using Masar.UI.Services;
using System;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    private readonly ILocalizationService _localizationService;
    private string? _errorKey;

    private string _username = "admin";
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string _password = "Admin@123";
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public AsyncRelayCommand LoginCommand { get; }
    public RelayCommand ToggleLanguageCommand { get; }

    public event EventHandler? LoginSucceeded;

    public LoginViewModel(IAuthService authService, ISessionService sessionService, ILocalizationService localizationService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _localizationService = localizationService;
        LoginCommand = new AsyncRelayCommand(LoginAsync, () => !IsBusy);
        ToggleLanguageCommand = new RelayCommand(_ => _localizationService.ToggleLanguage());
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    private async Task LoginAsync()
    {
        try
        {
            IsBusy = true;
            _errorKey = null;
            ErrorMessage = string.Empty;

            var authResult = await _authService.AuthenticateAsync(Username, Password);
            if (!authResult.IsSuccess)
            {
                ErrorMessage = authResult.Message;
                return;
            }

            _sessionService.SetUser(authResult.Value!);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsBusy = false;
            LoginCommand.RaiseCanExecuteChanged();
        }
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_errorKey))
        {
            ErrorMessage = _localizationService.GetString(_errorKey);
        }
    }
}
