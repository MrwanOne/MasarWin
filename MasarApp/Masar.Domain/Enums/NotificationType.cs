namespace Masar.Domain.Enums;

/// <summary>
/// نوع الإشعار
/// </summary>
public enum NotificationType
{
    /// <summary>عام</summary>
    General = 0,
    
    /// <summary>تم قبول المشروع</summary>
    ProjectApproved = 1,
    
    /// <summary>تم رفض المشروع</summary>
    ProjectRejected = 2,
    
    /// <summary>تم تعيين مشرف</summary>
    SupervisorAssigned = 3,
    
    /// <summary>تمت جدولة المناقشة</summary>
    DiscussionScheduled = 4,
    
    /// <summary>تم تكليفك بمهمة</summary>
    TaskAssigned = 5,
    
    /// <summary>موعد تسليم قريب</summary>
    TaskDueSoon = 6,
    
    /// <summary>تم إكمال التقييم</summary>
    EvaluationCompleted = 7
}
