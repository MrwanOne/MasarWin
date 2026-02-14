namespace Masar.Domain.Enums;

/// <summary>
/// حالة الطالب الأكاديمية
/// </summary>
public enum StudentStatus
{
    /// <summary>منتظم</summary>
    Active = 1,
    
    /// <summary>متخرج</summary>
    Graduated = 2,
    
    /// <summary>منسحب</summary>
    Withdrawn = 3,
    
    /// <summary>موقوف</summary>
    Suspended = 4,
    
    /// <summary>مؤجل</summary>
    Deferred = 5
}
