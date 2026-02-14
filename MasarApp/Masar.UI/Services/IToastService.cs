namespace Masar.UI.Services;

/// <summary>
/// خدمة إشعارات Toast
/// Toast Notification Service Interface
/// </summary>
public interface IToastService
{
    /// <summary>
    /// عرض رسالة نجاح
    /// </summary>
    void ShowSuccess(string message, string? title = null);

    /// <summary>
    /// عرض رسالة خطأ
    /// </summary>
    void ShowError(string message, string? title = null);

    /// <summary>
    /// عرض رسالة تحذير
    /// </summary>
    void ShowWarning(string message, string? title = null);

    /// <summary>
    /// عرض رسالة معلومات
    /// </summary>
    void ShowInfo(string message, string? title = null);
}
