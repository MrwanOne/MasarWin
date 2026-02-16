using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialOracle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academic_term",
                columns: table => new
                {
                    term_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    year = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    semester = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    name_ar = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    start_date = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    end_date = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    is_active = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academic_term", x => x.term_id);
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    audit_log_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    entity_name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    entity_id = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    action = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    old_values = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    new_values = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    username = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    changed_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.audit_log_id);
                });

            migrationBuilder.CreateTable(
                name: "college",
                columns: table => new
                {
                    college_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    name_ar = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_college", x => x.college_id);
                });

            migrationBuilder.CreateTable(
                name: "committee",
                columns: table => new
                {
                    committee_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    department_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    term_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_committee", x => x.committee_id);
                    table.ForeignKey(
                        name: "FK_committee_academic_term_term_id",
                        column: x => x.term_id,
                        principalTable: "academic_term",
                        principalColumn: "term_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "committee_member",
                columns: table => new
                {
                    committee_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    doctor_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    role = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
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
                name: "criteria_score",
                columns: table => new
                {
                    score_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    evaluation_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    criteria_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    score = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false),
                    comments = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_criteria_score", x => x.score_id);
                });

            migrationBuilder.CreateTable(
                name: "department",
                columns: table => new
                {
                    department_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    name_ar = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    college_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    head_of_department_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
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
                    doctor_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    full_name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    qualification = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    gender = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    rank = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    specialization = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    max_supervision_count = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    is_active = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    college_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    department_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor", x => x.doctor_id);
                    table.ForeignKey(
                        name: "FK_doctor_college_college_id",
                        column: x => x.college_id,
                        principalTable: "college",
                        principalColumn: "college_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_doctor_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "department_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "evaluation_criteria",
                columns: table => new
                {
                    criteria_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    name_ar = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    description_ar = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    description_en = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    max_score = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false),
                    weight = table.Column<decimal>(type: "DECIMAL(5,4)", precision: 5, scale: 4, nullable: false),
                    display_order = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    is_active = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    department_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
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
                name: "team",
                columns: table => new
                {
                    team_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    department_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    supervisor_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    committee_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
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
                    discussion_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    team_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    committee_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    start_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    end_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    place = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    supervisor_score = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false),
                    committee_score = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false),
                    final_score = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false),
                    report_text = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
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
                    project_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    title = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    beneficiary = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    completion_rate = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false),
                    documentation_path = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    proposed_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    approved_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    rejection_reason = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    department_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    team_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    supervisor_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    term_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project", x => x.project_id);
                    table.ForeignKey(
                        name: "FK_project_academic_term_term_id",
                        column: x => x.term_id,
                        principalTable: "academic_term",
                        principalColumn: "term_id",
                        onDelete: ReferentialAction.SetNull);
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
                    student_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    student_number = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    gender = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    gpa = table.Column<decimal>(type: "DECIMAL(3,2)", precision: 3, scale: 2, nullable: true),
                    level = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    department_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    team_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    enrollment_year = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
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
                name: "document",
                columns: table => new
                {
                    document_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    project_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    discussion_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    student_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    file_name = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    content = table.Column<byte[]>(type: "RAW(2000)", nullable: false),
                    content_type = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    file_size = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    version = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 1),
                    status = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true, defaultValue: "Draft"),
                    checksum = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: true),
                    description = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    category = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true, defaultValue: "General"),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document", x => x.document_id);
                    table.ForeignKey(
                        name: "FK_document_discussion_discussion_id",
                        column: x => x.discussion_id,
                        principalTable: "discussion",
                        principalColumn: "discussion_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_project_project_id",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_student_student_id",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_evaluation",
                columns: table => new
                {
                    evaluation_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    discussion_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    student_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    total_score = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false),
                    contribution_percentage = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false),
                    general_feedback = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    strength_points = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    improvement_areas = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
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
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    username = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    password_salt = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    role = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    is_active = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    doctor_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    student_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    created_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    created_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    updated_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false, defaultValue: false)
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

            migrationBuilder.CreateTable(
                name: "project_status_history",
                columns: table => new
                {
                    history_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    project_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    old_status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    new_status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    changed_by_user_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    change_reason = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    changed_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_status_history", x => x.history_id);
                    table.ForeignKey(
                        name: "FK_project_status_history_project_project_id",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_status_history_users_changed_by_user_id",
                        column: x => x.changed_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_academic_term_year_semester",
                table: "academic_term",
                columns: new[] { "year", "semester" },
                unique: true);

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
                name: "IX_committee_term_id",
                table: "committee",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "IX_committee_member_doctor_id",
                table: "committee_member",
                column: "doctor_id");

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
                name: "IX_doctor_college_id",
                table: "doctor",
                column: "college_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_department_id",
                table: "doctor",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_discussion_id",
                table: "document",
                column: "discussion_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_project_id",
                table: "document",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_student_id",
                table: "document",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_evaluation_criteria_department_id",
                table: "evaluation_criteria",
                column: "department_id");

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
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_term_id",
                table: "project",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_title",
                table: "project",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_status_history_changed_by_user_id",
                table: "project_status_history",
                column: "changed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_status_history_project_id",
                table: "project_status_history",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_department_id",
                table: "student",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_email",
                table: "student",
                column: "email",
                unique: true,
                filter: "\"email\" IS NOT NULL");

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
                name: "IX_student_evaluation_discussion_id_student_id",
                table: "student_evaluation",
                columns: new[] { "discussion_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_evaluation_student_id",
                table: "student_evaluation",
                column: "student_id");

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
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_committee_member_doctor_doctor_id",
                table: "committee_member",
                column: "doctor_id",
                principalTable: "doctor",
                principalColumn: "doctor_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_criteria_score_evaluation_criteria_criteria_id",
                table: "criteria_score",
                column: "criteria_id",
                principalTable: "evaluation_criteria",
                principalColumn: "criteria_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_criteria_score_student_evaluation_evaluation_id",
                table: "criteria_score",
                column: "evaluation_id",
                principalTable: "student_evaluation",
                principalColumn: "evaluation_id",
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
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "committee_member");

            migrationBuilder.DropTable(
                name: "criteria_score");

            migrationBuilder.DropTable(
                name: "document");

            migrationBuilder.DropTable(
                name: "project_status_history");

            migrationBuilder.DropTable(
                name: "evaluation_criteria");

            migrationBuilder.DropTable(
                name: "student_evaluation");

            migrationBuilder.DropTable(
                name: "project");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "discussion");

            migrationBuilder.DropTable(
                name: "student");

            migrationBuilder.DropTable(
                name: "team");

            migrationBuilder.DropTable(
                name: "committee");

            migrationBuilder.DropTable(
                name: "academic_term");

            migrationBuilder.DropTable(
                name: "department");

            migrationBuilder.DropTable(
                name: "doctor");

            migrationBuilder.DropTable(
                name: "college");
        }
    }
}
