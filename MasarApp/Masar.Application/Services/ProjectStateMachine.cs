using Masar.Application.Common;
using Masar.Application.Interfaces;
using Masar.Domain.Enums;

namespace Masar.Application.Services;

/// <summary>
/// تطبيق آلة حالة المشروع
/// تحدد الانتقالات المسموحة بين الحالات بناءً على دور المستخدم
/// </summary>
public class ProjectStateMachine : IProjectStateMachine
{
    /// <summary>
    /// مصفوفة الانتقالات المسموحة: [من الحالة] -> [إلى الحالة] -> [الأدوار المسموحة]
    /// </summary>
    private static readonly Dictionary<ProjectStatus, Dictionary<ProjectStatus, UserRole[]>> Transitions = new()
    {
        // من "مقترح" يمكن الانتقال إلى:
        [ProjectStatus.Proposed] = new()
        {
            // موافقة - فقط المدير أو رئيس القسم
            [ProjectStatus.Approved] = new[] { UserRole.Admin, UserRole.HeadOfDepartment },
            // رفض - فقط المدير أو رئيس القسم
            [ProjectStatus.Rejected] = new[] { UserRole.Admin, UserRole.HeadOfDepartment }
        },

        // من "معتمد" يمكن الانتقال إلى:
        [ProjectStatus.Approved] = new()
        {
            // بدء التنفيذ - المدير، رئيس القسم، أو المشرف
            [ProjectStatus.InProgress] = new[] { UserRole.Admin, UserRole.HeadOfDepartment, UserRole.Supervisor },
            // رفض (إلغاء الموافقة) - فقط المدير أو رئيس القسم
            [ProjectStatus.Rejected] = new[] { UserRole.Admin, UserRole.HeadOfDepartment }
        },

        // من "قيد التنفيذ" يمكن الانتقال إلى:
        [ProjectStatus.InProgress] = new()
        {
            // إكمال المشروع - فقط المدير أو رئيس القسم
            [ProjectStatus.Completed] = new[] { UserRole.Admin, UserRole.HeadOfDepartment }
        },

        // من "مرفوض" يمكن الانتقال إلى:
        [ProjectStatus.Rejected] = new()
        {
            // إعادة تقديم - المدير، رئيس القسم، أو المشرف
            [ProjectStatus.Proposed] = new[] { UserRole.Admin, UserRole.HeadOfDepartment, UserRole.Supervisor }
        },

        // "مكتمل" - حالة نهائية، لا انتقالات منها
        [ProjectStatus.Completed] = new()
    };

    /// <summary>
    /// أسماء الحالات بالعربية والإنجليزية
    /// </summary>
    private static readonly Dictionary<ProjectStatus, (string Arabic, string English)> StatusNames = new()
    {
        [ProjectStatus.Proposed] = ("مقترح", "Proposed"),
        [ProjectStatus.Approved] = ("معتمد", "Approved"),
        [ProjectStatus.InProgress] = ("قيد التنفيذ", "In Progress"),
        [ProjectStatus.Completed] = ("مكتمل", "Completed"),
        [ProjectStatus.Rejected] = ("مرفوض", "Rejected")
    };

    /// <summary>
    /// ألوان الحالات (Hex)
    /// </summary>
    private static readonly Dictionary<ProjectStatus, string> StatusColors = new()
    {
        [ProjectStatus.Proposed] = "#FFA726",    // برتقالي - انتظار
        [ProjectStatus.Approved] = "#42A5F5",    // أزرق - موافقة
        [ProjectStatus.InProgress] = "#66BB6A",  // أخضر - تنفيذ
        [ProjectStatus.Completed] = "#26A69A",   // تركواز - مكتمل
        [ProjectStatus.Rejected] = "#EF5350"     // أحمر - مرفوض
    };

    public bool CanTransition(ProjectStatus from, ProjectStatus to)
    {
        if (from == to) return true; // البقاء في نفس الحالة مسموح دائماً
        return Transitions.ContainsKey(from) && Transitions[from].ContainsKey(to);
    }

    public Result<ProjectStatus> TryTransition(ProjectStatus current, ProjectStatus target, UserRole userRole)
    {
        // البقاء في نفس الحالة
        if (current == target)
            return Result<ProjectStatus>.Success(target);

        // التحقق من وجود الانتقال
        if (!CanTransition(current, target))
        {
            var fromName = GetStatusDisplayName(current);
            var toName = GetStatusDisplayName(target);
            return Result<ProjectStatus>.Failure(
                $"لا يمكن الانتقال من '{fromName}' إلى '{toName}' / " +
                $"Cannot transition from '{GetStatusDisplayName(current, false)}' to '{GetStatusDisplayName(target, false)}'");
        }

        // التحقق من صلاحية الدور
        var allowedRoles = Transitions[current][target];
        if (!allowedRoles.Contains(userRole))
        {
            var roleNames = string.Join(", ", allowedRoles.Select(r => r.ToString()));
            return Result<ProjectStatus>.Failure(
                $"الدور '{userRole}' غير مصرح له بهذا الانتقال. الأدوار المسموحة: {roleNames} / " +
                $"Role '{userRole}' is not authorized for this transition. Allowed roles: {roleNames}");
        }

        return Result<ProjectStatus>.Success(target);
    }

    public IReadOnlyList<ProjectStatus> GetAllowedTransitions(ProjectStatus current, UserRole userRole)
    {
        if (!Transitions.ContainsKey(current))
            return Array.Empty<ProjectStatus>();

        return Transitions[current]
            .Where(t => t.Value.Contains(userRole))
            .Select(t => t.Key)
            .ToList()
            .AsReadOnly();
    }

    public string GetStatusDisplayName(ProjectStatus status, bool arabic = true)
    {
        if (StatusNames.TryGetValue(status, out var names))
            return arabic ? names.Arabic : names.English;
        return status.ToString();
    }

    public string GetStatusColor(ProjectStatus status)
    {
        return StatusColors.TryGetValue(status, out var color) ? color : "#757575";
    }
}
