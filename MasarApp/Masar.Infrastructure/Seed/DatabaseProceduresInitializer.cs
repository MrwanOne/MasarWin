using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Seed;

/// <summary>
/// يُنشئ Functions وStored Procedures وTriggers في Oracle مرة واحدة عند بدء التطبيق.
/// يستخدم CREATE OR REPLACE حتى لا يفشل عند التكرار.
/// يجب استدعاؤه بعد DatabaseViewsInitializer في Program.cs.
/// </summary>
public static class DatabaseProceduresInitializer
{
    public static async Task InitializeAsync(
        IDbContextFactory<MasarDbContext> contextFactory,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        Console.WriteLine("DatabaseProceduresInitializer: Creating/updating Functions, Procedures, Triggers...");

        var failures = new List<string>();

        // Functions (تُنشأ أولاً لأن الـ Procedures تعتمد عليها)
        await CreateObject(context, "FN_CALC_FINAL_SCORE",
            SqlScripts.FN_CALC_FINAL_SCORE, failures, cancellationToken);

        await CreateObject(context, "FN_GET_SUPERVISOR_PROJECT_COUNT",
            SqlScripts.FN_GET_SUPERVISOR_PROJECT_COUNT, failures, cancellationToken);

        await CreateObject(context, "FN_SUPERVISOR_IS_COMMITTEE_MEMBER",
            SqlScripts.FN_SUPERVISOR_IS_COMMITTEE_MEMBER, failures, cancellationToken);

        await CreateObject(context, "FN_GET_STUDENT_COUNT_BY_DEPT",
            SqlScripts.FN_GET_STUDENT_COUNT_BY_DEPT, failures, cancellationToken);

        await CreateObject(context, "FN_STUDENT_HAS_TEAM",
            SqlScripts.FN_STUDENT_HAS_TEAM, failures, cancellationToken);

        // Stored Procedures (تعتمد على الـ Functions)
        await CreateObject(context, "SP_ACCEPT_PROJECT",
            SqlScripts.SP_ACCEPT_PROJECT, failures, cancellationToken);

        await CreateObject(context, "SP_SAVE_EVALUATION",
            SqlScripts.SP_SAVE_EVALUATION, failures, cancellationToken);

        await CreateObject(context, "SP_ADD_STUDENT",
            SqlScripts.SP_ADD_STUDENT, failures, cancellationToken);

        await CreateObject(context, "SP_UPDATE_STUDENT",
            SqlScripts.SP_UPDATE_STUDENT, failures, cancellationToken);

        await CreateObject(context, "SP_DELETE_STUDENT",
            SqlScripts.SP_DELETE_STUDENT, failures, cancellationToken);

        await CreateObject(context, "SP_ASSIGN_STUDENT_TO_TEAM",
            SqlScripts.SP_ASSIGN_STUDENT_TO_TEAM, failures, cancellationToken);

        // Triggers
        await CreateObject(context, "TRG_NO_SUPERVISOR_IN_OWN_COMMITTEE",
            SqlScripts.TRG_NO_SUPERVISOR_IN_OWN_COMMITTEE, failures, cancellationToken);

        await CreateObject(context, "TRG_CALC_FINAL_SCORE",
            SqlScripts.TRG_CALC_FINAL_SCORE, failures, cancellationToken);

        if (failures.Count == 0)
        {
            Console.WriteLine("DatabaseProceduresInitializer: All objects created/updated successfully.");
        }
        else
        {
            var msg = $"DatabaseProceduresInitializer: Completed with {failures.Count} failure(s): {string.Join(", ", failures)}";
            Console.Error.WriteLine(msg);
            throw new InvalidOperationException(msg);
        }
    }

    private static async Task CreateObject(
        MasarDbContext context,
        string objectName,
        string sql,
        List<string> failures,
        CancellationToken cancellationToken)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            Console.WriteLine($"DatabaseProceduresInitializer: '{objectName}' created/updated.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"DatabaseProceduresInitializer: Failed to create '{objectName}'. Exception: {ex.Message}");
            failures.Add(objectName);
            // لا نوقف العملية - نكمل إنشاء باقي الكائنات
        }
    }
}
