using Masar.Application.Interfaces;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

/// <summary>
/// ينفذ استدعاءات Stored Procedures وFunctions في Oracle.
/// يستخدم DbConnection الخاص بـ EF Core مباشرةً بدون OracleParameter.
/// </summary>
public class ProjectProcedureRepository : IProjectProcedureRepository
{
    private readonly IDbContextFactory<MasarDbContext> _factory;

    public ProjectProcedureRepository(IDbContextFactory<MasarDbContext> factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task<(int Code, string Message)> AcceptProjectAsync(
        int projectId,
        int supervisorId,
        int changedByUserId,
        CancellationToken ct = default)
    {
        await using var context = await _factory.CreateDbContextAsync(ct);
        var conn = context.Database.GetDbConnection();

        try
        {
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SP_ACCEPT_PROJECT";
            cmd.CommandType = CommandType.StoredProcedure;

            AddInputParam(cmd, "p_project_id",    projectId);
            AddInputParam(cmd, "p_supervisor_id", supervisorId);
            AddInputParam(cmd, "p_changed_by",    changedByUserId);

            var pCode = AddOutputParam(cmd, "p_result_code", DbType.Int32);
            var pMsg  = AddOutputParam(cmd, "p_result_msg",  DbType.String, 500);

            await cmd.ExecuteNonQueryAsync(ct);

            var code = pCode.Value is DBNull ? 1 : Convert.ToInt32(pCode.Value);
            var msg  = pMsg.Value is DBNull ? string.Empty : pMsg.Value?.ToString() ?? string.Empty;
            return (code, msg);
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                await conn.CloseAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<(int Code, string Message)> SaveEvaluationAsync(
        int discussionId,
        decimal supervisorScore,
        decimal committeeScore,
        string reportText,
        CancellationToken ct = default)
    {
        await using var context = await _factory.CreateDbContextAsync(ct);
        var conn = context.Database.GetDbConnection();

        try
        {
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SP_SAVE_EVALUATION";
            cmd.CommandType = CommandType.StoredProcedure;

            AddInputParam(cmd, "p_discussion_id",    discussionId);
            AddInputParam(cmd, "p_supervisor_score", supervisorScore);
            AddInputParam(cmd, "p_committee_score",  committeeScore);
            AddInputParam(cmd, "p_report_text",      reportText ?? string.Empty);

            var pCode = AddOutputParam(cmd, "p_result_code", DbType.Int32);
            var pMsg  = AddOutputParam(cmd, "p_result_msg",  DbType.String, 500);

            await cmd.ExecuteNonQueryAsync(ct);

            var code = pCode.Value is DBNull ? 1 : Convert.ToInt32(pCode.Value);
            var msg  = pMsg.Value is DBNull ? string.Empty : pMsg.Value?.ToString() ?? string.Empty;
            return (code, msg);
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                await conn.CloseAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetSupervisorActiveProjectCountAsync(
        int supervisorId,
        CancellationToken ct = default)
    {
        await using var context = await _factory.CreateDbContextAsync(ct);

        // استدعاء مباشر لـ Oracle Function عبر SELECT .. FROM DUAL
        var sql = $"SELECT FN_GET_SUPERVISOR_PROJECT_COUNT({supervisorId}) FROM DUAL";

        var result = await context.Database
            .SqlQueryRaw<decimal>(sql)
            .FirstOrDefaultAsync(ct);

        return (int)result;
    }

    // ─── Helpers ──────────────────────────────────────────────────

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
