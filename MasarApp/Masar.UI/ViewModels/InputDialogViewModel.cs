using Masar.UI.Controls;

namespace Masar.UI.ViewModels;

public class InputDialogViewModel : DialogViewModel
{
    public string Title { get; }
    public string Prompt { get; }

    private string _inputText = string.Empty;
    public string InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public InputDialogViewModel(string title, string prompt)
    {
        Title = title;
        Prompt = prompt;
        SaveCommand = new RelayCommand(_ => Close(true));
        CancelCommand = new RelayCommand(_ => Close(false));
    }
}
