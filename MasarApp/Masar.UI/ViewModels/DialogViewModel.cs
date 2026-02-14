using System;

namespace Masar.UI.ViewModels;

public abstract class DialogViewModel : ViewModelBase
{
    public event EventHandler<bool?>? RequestClose;

    protected void Close(bool? result)
    {
        RequestClose?.Invoke(this, result);
    }
}
