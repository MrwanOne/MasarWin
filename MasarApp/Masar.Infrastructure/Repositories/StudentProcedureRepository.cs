using Masar.Application.Interfaces;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

/// <summary>
/// ينفذ استدعاءات Stored Procedures وFunctions الخاصة بإدارة الطلاب في Oracle.
/// يتبع نفس نمط ProjectProcedureRepository.
/// </summary>
public class StudentProcedureRepository : IStudentProcedureRepository
{
    private readonly IDbContextFactory<MasarDbContext> _factory;

    public StudentProcedureRepository(IDbContextFactory<MasarDbContext> factory)
    {
        _factory = factory;
    }

    // ══════════════════════════════════════════════════════════════════
    // STORED PROCEDURES
    // ══════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<(int Code, string Message, int? NewStudentId)> AddStudentAsync(
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
        CancellationToken ct = default)
    {
        await using var context = await _factory.CreateDbContextAsync(ct);
        var conn = context.Database.GetDbConnection();

        try
        {
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SP_ADD_STUDENT";
            cmd.CommandType = CommandType.StoredProcedure;

            AddInputParam(cmd, "p_student_number",  studentNumber);
            AddInputParam(cmd, "p_full_name",         fullName);
            AddInputParam(cmd, "p_gender",            (object?)gender ?? DBNull.Value);
            AddInputParam(cmd, "p_email",             (object?)email  ?? DBNull.Value);
            AddInputParam(cmd, "p_phone",             (object?)phone  ?? DBNull.Value);
            AddInputParam(cmd, "p_gpa",               (object?)gpa    ?? DBNull.Value);
            AddInputParam(cmd, "p_level",             level);
            AddInputParam(cmd, "p_status",            status);
            AddInputParam(cmd, "p_department_id",     departmentId);
            AddInputParam(cmd, "p_enrollment_year",   enrollmentYear);
            AddInputParam(cmd, "p_created_by",        createdByUserId);

            var pCode      = AddOutputParam(cmd, "p_result_code",    DbType.Int32);
            var pMsg       = AddOutputParam(cmd, "p_result_msg",     DbType.String, 500);
            var pStudentId = AddOutputParam(cmd, "p_new_student_id", DbType.Int32);

            await cmd.ExecuteNonQueryAsync(ct);

            var code   = pCode.Value      is DBNull ? 1    : Convert.ToInt32(pCode.Value);
            var msg    = pMsg.Value       is DBNull ? ""   : pMsg.Value?.ToString() ?? "";
            int? newId = pStudentId.Value is DBNull ? null : Convert.ToInt32(pStudentId.Value);

            return (code, msg, newId);
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                await conn.CloseAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<(int Code, string Message)> UpdateStudentAsync(
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
        CancellationToken ct = default)
    {
        await using var context = await _factory.CreateDbContextAsync(ct);
        var conn = context.Database.GetDbConnection();

        try
        {
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SP_UPDATE_STUDENT";
            cmd.CommandType = CommandType.StoredProcedure;

            AddInputParam(cmd, "p_student_id",       studentId);
            AddInputParam(cmd, "p_student_number",   studentNumber);
            AddInputParam(cmd, "p_full_name",         fullName);
            AddInputParam(cmd, "p_gender",            (object?)gender ?? DBNull.Value);
            AddInputParam(cmd, "p_email",             (object?)email  ?? DBNull.Value);
            AddInputParam(cmd, "p_phone",             (object?)phone  ?? DBNull.Value);
            AddInputParam(cmd, "p_gpa",               (object?)gpa    ?? DBNull.Value);
            AddInputParam(cmd, "p_level",             level);
            AddInputParam(cmd, "p_status",            status);
            AddInputParam(cmd, "p_department_id",     departmentId);
            AddInputParam(cmd, "p_enrollment_year",   enrollmentYear);
            AddInputParam(cmd, "p_updated_by",        updatedByUserId);

            var pCode = AddOutputParam(cmd, "p_result_code", DbType.Int32);
            var pMsg  = AddOutputParam(cmd, "p_result_msg",  DbType.String, 500);

            await cmd.ExecuteNonQueryAsync(ct);

            var code = pCode.Value is DBNull ? 1  : Convert.ToInt32(pCode.Value);
            var msg  = pMsg.Value  is DBNull ? "" : pMsg.Value?.ToString() ?? "";

            return (code, msg);
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                await conn.CloseAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<(int Code, string Message)> DeleteStudentAsync(
        int studentId,
        int deletedByUserId,
        CancellationToken ct = default)
    {
        await using var context = await _factory.CreateDbContextAsync(ct);
        var conn = context.Database.GetDbConnection();

        try
        {
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SP_DELETE_STUDENT";
            cmd.CommandType = CommandType.StoredProcedure;

            AddInputParam(cmd, "p_student_id", studentId);
            AddInputParam(cmd, "p_deleted_by", deletedByUserId);

            var pCode = AddOutputParam(cmd, "p_result_code", DbType.Int32);
            var pMsg  = AddOutputParam(cmd, "p_result_msg",  DbType.String, 500);

            await cmd.ExecuteNonQueryAsync(ct);

            var code = pCode.Value is DBNull ? 1  : Convert.ToInt32(pCode.Value);
            var msg  = pMsg.Value  is DBNull ? "" : pMsg.Value?.ToString() ?? "";

            return (code, msg);
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                await conn.CloseAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<(int Code, string Message)> AssignStudentToTeamAsync(
        int studentId,
        int? teamId,
        int updatedByUserId,
        CancellationToken ct = default)
    {
        await using var context = await _factory.CreateDbContextAsync(ct);
        var conn = context.Database.GetDbConnection();

        try
        {
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SP_ASSIGN_STUDENT_TO_TEAM";
            cmd.CommandType = CommandType.StoredProcedure;

            AddInputParam(cmd, "p_student_id", studentId);
            AddInputParam(cmd, "p_team_id",    (object?)teamId ?? DBNull.Value);
            AddInputParam(cmd, "p_updated_by", updatedByUserId);

            var pCode = AddOutputParam(cmd, "p_result_code", DbType.Int32);
            var pMsg  = AddOutputParam(cmd, "p_result_msg",  DbType.String, 500);

            await cmd.ExecuteNonQueryAsync(ct);

            var code = pCode.Value is DBNull ? 1  : Convert.ToInt32(pCode.Value);
            var msg  = pMsg.Value  is DBNull ? "" : pMsg.Value?.ToString() ?? "";

            return (code, msg);
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                await conn.CloseAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // FUNCTIONS (استدعاء مباشر عبر SELECT .. FROM DUAL)
    // ══════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<int> GetStudentCountByDepartmentAsync(
        int departmentId,
        CancellationToken ct = default)
    {
        await using var context = await _factory.CreateDbContextAsync(ct);

        var sql = $"SELECT FN_GET_STUDENT_COUNT_BY_DEPT({departmentId}) FROM DUAL";

        var result = await context.Database
            .SqlQueryRaw<decimal>(sql)
            .FirstOrDefaultAsync(ct);

        return (int)result;
    }

    /// <inheritdoc/>
    public async Task<bool> StudentHasTeamAsync(
        int studentId,
        CancellationToken ct = default)
    {
        await using var context = await _factory.CreateDbContextAsync(ct);

        var sql = $"SELECT FN_STUDENT_HAS_TEAM({studentId}) FROM DUAL";

        var result = await context.Database
            .SqlQueryRaw<decimal>(sql)
            .FirstOrDefaultAsync(ct);

        return result == 1;
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    private static void AddInputParam(IDbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value ?? DBNull.Value;
        p.Direction     = ParameterDirection.Input;
        cmd.Parameters.Add(p);
    }

    private static IDbDataParameter AddOutputParam(
        IDbCommand cmd, string name, DbType dbType, int size = 0)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.DbType        = dbType;
        p.Direction     = ParameterDirection.Output;
        if (size > 0) p.Size = size;
        cmd.Parameters.Add(p);
        return p;
    }
}
