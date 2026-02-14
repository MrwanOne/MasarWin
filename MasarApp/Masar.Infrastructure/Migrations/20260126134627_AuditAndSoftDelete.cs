using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_student_evaluation_users_evaluated_by_user_id",
                table: "student_evaluation");

            migrationBuilder.DropIndex(
                name: "IX_student_evaluation_evaluated_by_user_id",
                table: "student_evaluation");

            migrationBuilder.RenameColumn(
                name: "evaluated_by_user_id",
                table: "student_evaluation",
                newName: "updated_by_user_id");

            migrationBuilder.RenameColumn(
                name: "evaluated_at",
                table: "student_evaluation",
                newName: "created_at");

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "team",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "team",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "team",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "team",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "student_evaluation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "student_evaluation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "student_evaluation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "student",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "student",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "student",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "student",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "student",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "project",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "project",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "project",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "project",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "project",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "evaluation_criteria",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "evaluation_criteria",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "evaluation_criteria",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "evaluation_criteria",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "evaluation_criteria",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "doctor",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "doctor",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "doctor",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "doctor",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "doctor",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "discussion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "discussion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "discussion",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "discussion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "department",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "department",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "department",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "department",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "committee_member",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "committee_member",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "committee_member",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "committee_member",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "committee_member",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "committee",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "committee",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "committee",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "committee",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "college",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "college",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "college",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "college",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "created_by_user_id",
                table: "academic_term",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "academic_term",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "academic_term",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_user_id",
                table: "academic_term",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    audit_log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    entity_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    username = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.audit_log_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "team");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "team");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "team");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "team");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "student_evaluation");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "student_evaluation");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "student_evaluation");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "student");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "student");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "student");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "student");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "student");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "project");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "project");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "project");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "project");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "project");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "evaluation_criteria");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "evaluation_criteria");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "evaluation_criteria");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "evaluation_criteria");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "evaluation_criteria");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "doctor");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "doctor");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "doctor");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "doctor");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "doctor");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "discussion");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "discussion");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "discussion");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "discussion");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "department");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "department");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "department");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "department");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "committee_member");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "committee_member");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "committee_member");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "committee_member");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "committee_member");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "committee");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "committee");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "committee");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "committee");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "college");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "college");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "college");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "college");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "academic_term");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "academic_term");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "academic_term");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "academic_term");

            migrationBuilder.RenameColumn(
                name: "updated_by_user_id",
                table: "student_evaluation",
                newName: "evaluated_by_user_id");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "student_evaluation",
                newName: "evaluated_at");

            migrationBuilder.CreateIndex(
                name: "IX_student_evaluation_evaluated_by_user_id",
                table: "student_evaluation",
                column: "evaluated_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_student_evaluation_users_evaluated_by_user_id",
                table: "student_evaluation",
                column: "evaluated_by_user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
