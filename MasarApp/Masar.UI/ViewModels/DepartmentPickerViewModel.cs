using Masar.Application.DTOs;
using Masar.UI.Controls;
using Masar.UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Masar.UI.ViewModels;

public class DepartmentPickerViewModel : ViewModelBase
{
    public event EventHandler<bool>? RequestClose;

    public string Title { get; }
    public string Instruction { get; }
    public ObservableCollection<DepartmentDto> Departments { get; }

    private DepartmentDto? _selectedDepartment;
    public DepartmentDto? SelectedDepartment
    {
        get => _selectedDepartment;
        set
        {
            if (_selectedDepartment == value) return;
            _selectedDepartment = value;
            OnPropertyChanged(nameof(SelectedDepartment));
            ConfirmCommand.RaiseCanExecuteChanged();
        }
    }

    public RelayCommand ConfirmCommand { get; }
    public RelayCommand CancelCommand { get; }

    public DepartmentPickerViewModel(
        ILocalizationService localizationService,
        DoctorDto doctor,
        IEnumerable<DepartmentDto> departments)
    {
        Title = localizationService.IsArabic ? "تعيين رئيس قسم" : "Set Head of Department";
        Instruction = localizationService.IsArabic
            ? $"اختر القسم الذي سيُعيَّن له {doctor.FullName} كرئيس:"
            : $"Select the department for {doctor.FullName} to head:";

        Departments = new ObservableCollection<DepartmentDto>(departments);

        ConfirmCommand = new RelayCommand(
            _ => RequestClose?.Invoke(this, true),
            _ => SelectedDepartment != null);

        CancelCommand = new RelayCommand(
            _ => RequestClose?.Invoke(this, false));
    }
}
