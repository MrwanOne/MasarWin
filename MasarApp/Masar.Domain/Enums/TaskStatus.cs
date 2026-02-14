namespace Masar.Domain.Enums;

/// <summary>
/// حالة مهمة المشروع
/// </summary>
public enum TaskStatus
{
    /// <summary>معلقة - لم تبدأ</summary>
    Pending = 0,
    
    /// <summary>قيد التنفيذ</summary>
    InProgress = 1,
    
    /// <summary>مكتملة</summary>
    Completed = 2,
    
    /// <summary>ملغاة</summary>
    Cancelled = 3
}
