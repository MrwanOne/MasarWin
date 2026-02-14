using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SchemaEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_project_team_id",
                table: "project");

            migrationBuilder.DropColumn(
                name: "year",
                table: "committee");

            migrationBuilder.AddColumn<string>(
                name: "gender",
                table: "student",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "gpa",
                table: "student",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "level",
                table: "student",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "student",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "term_id",
                table: "project",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "doctor",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "max_supervision_count",
                table: "doctor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "rank",
                table: "doctor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "specialization",
                table: "doctor",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "term_id",
                table: "committee",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "academic_term",
                columns: table => new
                {
                    term_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    year = table.Column<int>(type: "int", nullable: false),
                    semester = table.Column<int>(type: "int", nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academic_term", x => x.term_id);
                });

            migrationBuilder.CreateTable(
                name: "evaluation_criteria",
                columns: table => new
                {
                    criteria_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description_ar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description_en = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    max_score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    weight = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluation_criteria", x => x.criteria_id);
                    table.ForeignKey(
                        name: "FK_evaluation_criteria_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "department_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "student_evaluation",
                columns: table => new
                {
                    evaluation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    discussion_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    total_score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    contribution_percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    general_feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    strength_points = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    improvement_areas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    evaluated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    evaluated_by_user_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_evaluation", x => x.evaluation_id);
                    table.ForeignKey(
                        name: "FK_student_evaluation_discussion_discussion_id",
                        column: x => x.discussion_id,
                        principalTable: "discussion",
                        principalColumn: "discussion_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_evaluation_student_student_id",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_evaluation_users_evaluated_by_user_id",
                        column: x => x.evaluated_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "criteria_score",
                columns: table => new
                {
                    score_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    evaluation_id = table.Column<int>(type: "int", nullable: false),
                    criteria_id = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    comments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_criteria_score", x => x.score_id);
                    table.ForeignKey(
                        name: "FK_criteria_score_evaluation_criteria_criteria_id",
                        column: x => x.criteria_id,
                        principalTable: "evaluation_criteria",
                        principalColumn: "criteria_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_criteria_score_student_evaluation_evaluation_id",
                        column: x => x.evaluation_id,
                        principalTable: "student_evaluation",
                        principalColumn: "evaluation_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_team_id",
                table: "project",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_term_id",
                table: "project",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "IX_committee_term_id",
                table: "committee",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "IX_academic_term_year_semester",
                table: "academic_term",
                columns: new[] { "year", "semester" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_criteria_score_criteria_id",
                table: "criteria_score",
                column: "criteria_id");

            migrationBuilder.CreateIndex(
                name: "IX_criteria_score_evaluation_id_criteria_id",
                table: "criteria_score",
                columns: new[] { "evaluation_id", "criteria_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evaluation_criteria_department_id",
                table: "evaluation_criteria",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_evaluation_discussion_id_student_id",
                table: "student_evaluation",
                columns: new[] { "discussion_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_evaluation_evaluated_by_user_id",
                table: "student_evaluation",
                column: "evaluated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_evaluation_student_id",
                table: "student_evaluation",
                column: "student_id");

            migrationBuilder.AddForeignKey(
                name: "FK_committee_academic_term_term_id",
                table: "committee",
                column: "term_id",
                principalTable: "academic_term",
                principalColumn: "term_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_project_academic_term_term_id",
                table: "project",
                column: "term_id",
                principalTable: "academic_term",
                principalColumn: "term_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_committee_academic_term_term_id",
                table: "committee");

            migrationBuilder.DropForeignKey(
                name: "FK_project_academic_term_term_id",
                table: "project");

            migrationBuilder.DropTable(
                name: "academic_term");

            migrationBuilder.DropTable(
                name: "criteria_score");

            migrationBuilder.DropTable(
                name: "evaluation_criteria");

            migrationBuilder.DropTable(
                name: "student_evaluation");

            migrationBuilder.DropIndex(
                name: "IX_project_team_id",
                table: "project");

            migrationBuilder.DropIndex(
                name: "IX_project_term_id",
                table: "project");

            migrationBuilder.DropIndex(
                name: "IX_committee_term_id",
                table: "committee");

            migrationBuilder.DropColumn(
                name: "gender",
                table: "student");

            migrationBuilder.DropColumn(
                name: "gpa",
                table: "student");

            migrationBuilder.DropColumn(
                name: "level",
                table: "student");

            migrationBuilder.DropColumn(
                name: "status",
                table: "student");

            migrationBuilder.DropColumn(
                name: "term_id",
                table: "project");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "doctor");

            migrationBuilder.DropColumn(
                name: "max_supervision_count",
                table: "doctor");

            migrationBuilder.DropColumn(
                name: "rank",
                table: "doctor");

            migrationBuilder.DropColumn(
                name: "specialization",
                table: "doctor");

            migrationBuilder.DropColumn(
                name: "term_id",
                table: "committee");

            migrationBuilder.AddColumn<int>(
                name: "year",
                table: "committee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_project_team_id",
                table: "project",
                column: "team_id",
                unique: true,
                filter: "[team_id] IS NOT NULL");
        }
    }
}
