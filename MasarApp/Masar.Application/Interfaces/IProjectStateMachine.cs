using Masar.Application.Common;
using Masar.Domain.Enums;

namespace Masar.Application.Interfaces;

/// <summary>
/// آلة حالة المشروع - تتحكم في انتقالات الحالة المسموحة
/// Project State Machine - Controls allowed status transitions
/// </summary>
public interface IProjectStateMachine
{
    /// <summary>
    /// هل يمكن الانتقال من حالة إلى أخرى؟
    /// </summary>
    bool CanTransition(ProjectStatus from, ProjectStatus to);

    /// <summary>
    /// محاولة الانتقال من حالة إلى أخرى مع التحقق من الصلاحيات
    /// </summary>
    Result<ProjectStatus> TryTransition(ProjectStatus current, ProjectStatus target, UserRole userRole);

    /// <summary>
    /// الحصول على قائمة الانتقالات المتاحة لدور معين
    /// </summary>
    IReadOnlyList<ProjectStatus> GetAllowedTransitions(ProjectStatus current, UserRole userRole);

    /// <summary>
    /// الحصول على وصف الحالة بالعربية
    /// </summary>
    string GetStatusDisplayName(ProjectStatus status, bool arabic = true);

    /// <summary>
    /// الحصول على لون الحالة
    /// </summary>
    string GetStatusColor(ProjectStatus status);
}
