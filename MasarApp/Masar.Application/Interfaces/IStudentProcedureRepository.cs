using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

/// <summary>
/// واجهة استدعاء Stored Procedures وFunctions المتعلقة بإدارة الطلاب.
/// تُنفَّذ في Infrastructure وتُحقن في StudentService.
/// </summary>
public interface IStudentProcedureRepository
{
    // ══════════════════════════════════════════════════════════════════
    // STORED PROCEDURES
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// تستدعي SP_ADD_STUDENT:
    /// تضيف طالباً جديداً بعد التحقق من تفرّد الرقم الجامعي والبريد الإلكتروني،
    /// وتُرجع معرف الطالب المُنشأ عند النجاح.
    /// </summary>
    Task<(int Code, string Message, int? NewStudentId)> AddStudentAsync(
        string studentNumber,
        string fullName,
        string? gender,
        string? email,
        string? phone,
        decimal? gpa,
        int level,
        int status,
        int departmentId,
        int enrollmentYear,
        int createdByUserId,
        CancellationToken ct = default);

    /// <summary>
    /// تستدعي SP_UPDATE_STUDENT:
    /// تعدّل بيانات طالب موجود بعد التحقق من وجوده وتفرّد رقمه وبريده الإلكتروني.
    /// </summary>
    Task<(int Code, string Message)> UpdateStudentAsync(
        int studentId,
        string studentNumber,
        string fullName,
        string? gender,
        string? email,
        string? phone,
        decimal? gpa,
        int level,
        int status,
        int departmentId,
        int enrollmentYear,
        int updatedByUserId,
        CancellationToken ct = default);

    /// <summary>
    /// تستدعي SP_DELETE_STUDENT:
    /// تحذف طالباً حذفاً منطقياً (Soft Delete) بعد التحقق من أنه غير منتسب لفريق نشط.
    /// </summary>
    /// <param name="studentId">معرف الطالب المراد حذفه</param>
    /// <param name="deletedByUserId">معرف المستخدم الذي أجرى الحذف</param>
    /// <returns>(Code=0 نجاح, Message) أو (Code=1 فشل, Message رسالة الخطأ)</returns>
    Task<(int Code, string Message)> DeleteStudentAsync(
        int studentId,
        int deletedByUserId,
        CancellationToken ct = default);

    /// <summary>
    /// تستدعي SP_ASSIGN_STUDENT_TO_TEAM:
    /// تُضيف طالباً إلى فريق بعد التحقق من القسم وعدم الانتساب المزدوج.
    /// تدعم إلغاء الانتساب بتمرير teamId = null.
    /// </summary>
    /// <param name="studentId">معرف الطالب</param>
    /// <param name="teamId">معرف الفريق (null لإلغاء الانتساب)</param>
    /// <param name="updatedByUserId">معرف المستخدم الذي أجرى العملية</param>
    /// <returns>(Code=0 نجاح, Message) أو (Code=1 فشل, Message رسالة الخطأ)</returns>
    Task<(int Code, string Message)> AssignStudentToTeamAsync(
        int studentId,
        int? teamId,
        int updatedByUserId,
        CancellationToken ct = default);

    // ══════════════════════════════════════════════════════════════════
    // FUNCTIONS
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// تستدعي FN_GET_STUDENT_COUNT_BY_DEPT مباشرة من Oracle.
    /// تُرجع عدد الطلاب النشطين في القسم المحدد لعرض الإحصائيات.
    /// </summary>
    /// <param name="departmentId">معرف القسم</param>
    /// <returns>عدد الطلاب النشطين</returns>
    Task<int> GetStudentCountByDepartmentAsync(
        int departmentId,
        CancellationToken ct = default);

    /// <summary>
    /// تستدعي FN_STUDENT_HAS_TEAM مباشرة من Oracle.
    /// تتحقق إن كان الطالب منتسباً لفريق نشط.
    /// </summary>
    /// <param name="studentId">معرف الطالب</param>
    /// <returns>true إذا كان الطالب في فريق، false إذا لم يكن</returns>
    Task<bool> StudentHasTeamAsync(
        int studentId,
        CancellationToken ct = default);
}
