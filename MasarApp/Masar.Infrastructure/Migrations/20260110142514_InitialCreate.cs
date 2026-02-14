using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "college",
                columns: table => new
                {
                    college_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_college", x => x.college_id);
                });

            migrationBuilder.CreateTable(
                name: "committee",
                columns: table => new
                {
                    committee_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_committee", x => x.committee_id);
                });

            migrationBuilder.CreateTable(
                name: "committee_member",
                columns: table => new
                {
                    committee_id = table.Column<int>(type: "int", nullable: false),
                    doctor_id = table.Column<int>(type: "int", nullable: false),
                    role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_committee_member", x => new { x.committee_id, x.doctor_id });
                    table.ForeignKey(
                        name: "FK_committee_member_committee_committee_id",
                        column: x => x.committee_id,
                        principalTable: "committee",
                        principalColumn: "committee_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "department",
                columns: table => new
                {
                    department_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    college_id = table.Column<int>(type: "int", nullable: false),
                    head_of_department_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department", x => x.department_id);
                    table.ForeignKey(
                        name: "FK_department_college_college_id",
                        column: x => x.college_id,
                        principalTable: "college",
                        principalColumn: "college_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "doctor",
                columns: table => new
                {
                    doctor_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    qualification = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor", x => x.doctor_id);
                    table.ForeignKey(
                        name: "FK_doctor_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "department_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "team",
                columns: table => new
                {
                    team_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    supervisor_id = table.Column<int>(type: "int", nullable: true),
                    committee_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team", x => x.team_id);
                    table.ForeignKey(
                        name: "FK_team_committee_committee_id",
                        column: x => x.committee_id,
                        principalTable: "committee",
                        principalColumn: "committee_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_team_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "department_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_team_doctor_supervisor_id",
                        column: x => x.supervisor_id,
                        principalTable: "doctor",
                        principalColumn: "doctor_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "discussion",
                columns: table => new
                {
                    discussion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    team_id = table.Column<int>(type: "int", nullable: false),
                    committee_id = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    place = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    supervisor_score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    committee_score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    final_score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    report_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    report_file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discussion", x => x.discussion_id);
                    table.ForeignKey(
                        name: "FK_discussion_committee_committee_id",
                        column: x => x.committee_id,
                        principalTable: "committee",
                        principalColumn: "committee_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_discussion_team_team_id",
                        column: x => x.team_id,
                        principalTable: "team",
                        principalColumn: "team_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    beneficiary = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    completion_rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    documentation_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    proposed_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    rejection_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    team_id = table.Column<int>(type: "int", nullable: true),
                    supervisor_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project", x => x.project_id);
                    table.ForeignKey(
                        name: "FK_project_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "department_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_project_doctor_supervisor_id",
                        column: x => x.supervisor_id,
                        principalTable: "doctor",
                        principalColumn: "doctor_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_project_team_team_id",
                        column: x => x.team_id,
                        principalTable: "team",
                        principalColumn: "team_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "student",
                columns: table => new
                {
                    student_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    team_id = table.Column<int>(type: "int", nullable: true),
                    enrollment_year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student", x => x.student_id);
                    table.ForeignKey(
                        name: "FK_student_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "department_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_team_team_id",
                        column: x => x.team_id,
                        principalTable: "team",
                        principalColumn: "team_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    password_salt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    role = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    doctor_id = table.Column<int>(type: "int", nullable: true),
                    student_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_users_doctor_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctor",
                        principalColumn: "doctor_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_users_student_student_id",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_college_name_ar",
                table: "college",
                column: "name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_college_name_en",
                table: "college",
                column: "name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_committee_department_id",
                table: "committee",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_committee_member_doctor_id",
                table: "committee_member",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_college_id_name_ar",
                table: "department",
                columns: new[] { "college_id", "name_ar" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_department_college_id_name_en",
                table: "department",
                columns: new[] { "college_id", "name_en" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_department_head_of_department_id",
                table: "department",
                column: "head_of_department_id");

            migrationBuilder.CreateIndex(
                name: "IX_discussion_committee_id",
                table: "discussion",
                column: "committee_id");

            migrationBuilder.CreateIndex(
                name: "IX_discussion_team_id",
                table: "discussion",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_department_id",
                table: "doctor",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_email",
                table: "doctor",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_project_department_id",
                table: "project",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_supervisor_id",
                table: "project",
                column: "supervisor_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_team_id",
                table: "project",
                column: "team_id",
                unique: true,
                filter: "[team_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_project_title",
                table: "project",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_department_id",
                table: "student",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_email",
                table: "student",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_student_student_number",
                table: "student",
                column: "student_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_team_id",
                table: "student",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_committee_id",
                table: "team",
                column: "committee_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_department_id",
                table: "team",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_supervisor_id",
                table: "team",
                column: "supervisor_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_doctor_id",
                table: "users",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_student_id",
                table: "users",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_committee_department_department_id",
                table: "committee",
                column: "department_id",
                principalTable: "department",
                principalColumn: "department_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_committee_member_doctor_doctor_id",
                table: "committee_member",
                column: "doctor_id",
                principalTable: "doctor",
                principalColumn: "doctor_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_doctor_head_of_department_id",
                table: "department",
                column: "head_of_department_id",
                principalTable: "doctor",
                principalColumn: "doctor_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_doctor_department_department_id",
                table: "doctor");

            migrationBuilder.DropTable(
                name: "committee_member");

            migrationBuilder.DropTable(
                name: "discussion");

            migrationBuilder.DropTable(
                name: "project");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "student");

            migrationBuilder.DropTable(
                name: "team");

            migrationBuilder.DropTable(
                name: "committee");

            migrationBuilder.DropTable(
                name: "department");

            migrationBuilder.DropTable(
                name: "college");

            migrationBuilder.DropTable(
                name: "doctor");
        }
    }
}
