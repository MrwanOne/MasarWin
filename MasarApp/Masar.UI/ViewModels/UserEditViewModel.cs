using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Services;
using Masar.Domain.Enums;
using Masar.UI.Controls;
using Masar.UI.Models;
using Masar.UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Masar.UI.ViewModels;

public class UserEditViewModel : DialogViewModel
{
    private readonly IUserService _userService;
    private readonly IDoctorService _doctorService;
    private readonly IStudentService _studentService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    public ObservableCollection<OptionItem<UserRole>> DoctorRoleOptions { get; } = new();

    private UserDto _user = new();
    public UserDto User
    {
        get => _user;
        set => SetProperty(ref _user, value);
    }

    private string _fullName = string.Empty;
    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    private bool _isStudent = true;
    public bool IsStudent
    {
        get => _isStudent;
        set
        {
            if (SetProperty(ref _isStudent, value))
            {
                if (value)
                {
                    _isDoctor = false;
                    _isAdmin = false;
                    OnPropertyChanged(nameof(IsDoctor));
                    OnPropertyChanged(nameof(IsAdmin));
                }
                OnPropertyChanged(nameof(DoctorRoleVisibility));
                OnPropertyChanged(nameof(FullNameVisibility));
                if (value)
                {
                    SelectedDoctorRole = null;
                }
            }
        }
    }

    private bool _isDoctor = false;
    public bool IsDoctor
    {
        get => _isDoctor;
        set
        {
            if (SetProperty(ref _isDoctor, value))
            {
                if (value)
                {
                    _isStudent = false;
                    _isAdmin = false;
                    OnPropertyChanged(nameof(IsStudent));
                    OnPropertyChanged(nameof(IsAdmin));
                }
                OnPropertyChanged(nameof(DoctorRoleVisibility));
                OnPropertyChanged(nameof(FullNameVisibility));
            }
        }
    }

    private bool _isAdmin = false;
    public bool IsAdmin
    {
        get => _isAdmin;
        set
        {
            if (SetProperty(ref _isAdmin, value))
            {
                if (value)
                {
                    _isStudent = false;
                    _isDoctor = false;
                    OnPropertyChanged(nameof(IsStudent));
                    OnPropertyChanged(nameof(IsDoctor));
                    SelectedDoctorRole = null;
                }
                OnPropertyChanged(nameof(DoctorRoleVisibility));
                OnPropertyChanged(nameof(FullNameVisibility));
            }
        }
    }

    public Visibility DoctorRoleVisibility => IsDoctor ? Visibility.Visible : Visibility.Collapsed;
    public Visibility FullNameVisibility => (IsStudent || IsDoctor) ? Visibility.Visible : Visibility.Collapsed;

    private UserRole? _selectedDoctorRole;
    public UserRole? SelectedDoctorRole
    {
        get => _selectedDoctorRole;
        set => SetProperty(ref _selectedDoctorRole, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool IsEditMode { get; }

    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public UserEditViewModel(
        IUserService userService,
        IDoctorService doctorService,
        IStudentService studentService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        UserDto? user = null)
    {
        _userService = userService;
        _doctorService = doctorService;
        _studentService = studentService;
        _dialogService = dialogService;
        _localizationService = localizationService;

        User = user ?? new UserDto { IsActive = true };
        IsEditMode = user != null;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(_ => Close(false));
        _localizationService.LanguageChanged += OnLanguageChanged;
        LoadLookups();
    }

    private void LoadLookups()
    {
        DoctorRoleOptions.Clear();
        DoctorRoleOptions.Add(new OptionItem<UserRole>(default, _localizationService.GetString("Placeholder.SelectRole")));
        DoctorRoleOptions.Add(new OptionItem<UserRole>(UserRole.Admin, "Admin"));
        DoctorRoleOptions.Add(new OptionItem<UserRole>(UserRole.HeadOfDepartment, "Head of Department"));
        DoctorRoleOptions.Add(new OptionItem<UserRole>(UserRole.Supervisor, "Supervisor"));
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        LoadLookups();
    }

    private async Task SaveAsync()
    {
        try
        {
            // Validate username and password for new users
            if (!IsEditMode)
            {
                if (string.IsNullOrWhiteSpace(User.Username))
                {
                    _dialogService.ShowError(_localizationService.GetString("Label.Username") + " " + _localizationService.GetString("Validation.Required"), _localizationService.GetString("Title.Users"));
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    _dialogService.ShowError(_localizationService.GetString("Label.Password") + " " + _localizationService.GetString("Validation.Required"), _localizationService.GetString("Title.Users"));
                    return;
                }
            }

            // Process based on user type
            if (IsStudent)
            {
                // Validate full name for students
                if (string.IsNullOrWhiteSpace(FullName))
                {
                    _dialogService.ShowError(
                        _localizationService.IsArabic ? "يرجى إدخال الاسم الكامل" : "Please enter full name",
                        _localizationService.GetString("Title.Users"));
                    return;
                }

                // Find student by name
                var student = await _studentService.FindByFullNameAsync(FullName);
                if (student == null)
                {
                    _dialogService.ShowError(
                        _localizationService.IsArabic ? $"لم يتم العثور على طالب باسم '{FullName}'" : $"Student '{FullName}' not found in database",
                        _localizationService.GetString("Title.Users"));
                    return;
                }

                User.StudentId = student.StudentId;
                User.DoctorId = null;
                User.Role = UserRole.Student;
            }
            else if (IsDoctor)
            {
                // Validate full name for doctors
                if (string.IsNullOrWhiteSpace(FullName))
                {
                    _dialogService.ShowError(
                        _localizationService.IsArabic ? "يرجى إدخال الاسم الكامل" : "Please enter full name",
                        _localizationService.GetString("Title.Users"));
                    return;
                }

                // Validate doctor role selection
                if (!SelectedDoctorRole.HasValue || SelectedDoctorRole.Value == default(UserRole))
                {
                    _dialogService.ShowError(
                        _localizationService.IsArabic ? "يرجى اختيار دور الدكتور" : "Please select doctor role",
                        _localizationService.GetString("Title.Users"));
                    return;
                }

                // Find doctor by name
                var doctor = await _doctorService.FindByFullNameAsync(FullName);
                if (doctor == null)
                {
                    _dialogService.ShowError(
                        _localizationService.IsArabic ? $"لم يتم العثور على دكتور باسم '{FullName}'" : $"Doctor '{FullName}' not found in database",
                        _localizationService.GetString("Title.Users"));
                    return;
                }

                User.DoctorId = doctor.DoctorId;
                User.StudentId = null;
                User.Role = SelectedDoctorRole.Value;
            }
            else if (IsAdmin)
            {
                // Admin can optionally have a doctor link
                User.StudentId = null;
                User.DoctorId = null;
                User.Role = UserRole.Admin;

                // If name is provided, try to link to a doctor
                if (!string.IsNullOrWhiteSpace(FullName))
                {
                    var doctor = await _doctorService.FindByFullNameAsync(FullName);
                    if (doctor != null)
                    {
                        User.DoctorId = doctor.DoctorId;
                    }
                    // If not found, that's okay for Admin - just create without doctor link
                }
            }

            Result<UserDto> result;
            if (IsEditMode)
            {
                result = await _userService.UpdateAsync(User);
            }
            else
            {
                result = await _userService.AddAsync(User, Password);
            }

            if (result.IsSuccess)
            {
                User = result.Value!;
                Close(true);
            }
            else
            {
                _dialogService.ShowError(result.Message, _localizationService.GetString("Title.Users"));
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message, _localizationService.GetString("Title.Users"));
        }
    }
}
