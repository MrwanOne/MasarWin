using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─── Functions (تُنشأ أولاً لأن الـ Procedures تعتمد عليها) ──────
            migrationBuilder.Sql(Seed.SqlScripts.FN_GET_STUDENT_COUNT_BY_DEPT);
            migrationBuilder.Sql(Seed.SqlScripts.FN_STUDENT_HAS_TEAM);

            // ─── Stored Procedures ────────────────────────────────────────────
            migrationBuilder.Sql(Seed.SqlScripts.SP_ADD_STUDENT);
            migrationBuilder.Sql(Seed.SqlScripts.SP_UPDATE_STUDENT);
            migrationBuilder.Sql(Seed.SqlScripts.SP_DELETE_STUDENT);
            migrationBuilder.Sql(Seed.SqlScripts.SP_ASSIGN_STUDENT_TO_TEAM);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_ASSIGN_STUDENT_TO_TEAM");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_DELETE_STUDENT");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_UPDATE_STUDENT");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_ADD_STUDENT");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS FN_STUDENT_HAS_TEAM");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS FN_GET_STUDENT_COUNT_BY_DEPT");
        }
    }
}
