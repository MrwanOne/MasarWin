using System;
using System.Threading.Tasks;

namespace Masar.UI.Controls;

/// <summary>
/// فئة مساعدة لتشغيل المهام غير المتزامنة بصيغة fire-and-forget بأمان
/// Helper class to safely run fire-and-forget async tasks in WPF
/// </summary>
public static class NotifyTask
{
    public static void Create(Task task, Action<Exception>? onError = null)
    {
        _ = ExecuteAsync(task, onError);
    }

    private static async Task ExecuteAsync(Task task, Action<Exception>? onError)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            // تسجيل الخطأ أو عرضه للمستخدم
            // في حالة عدم تمرير onError، يمكننا طباعته في الـ Debug على الأقل
            if (onError != null)
            {
                onError(ex);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled exception in fire-and-forget task: {ex}");
            }
        }
    }
}
