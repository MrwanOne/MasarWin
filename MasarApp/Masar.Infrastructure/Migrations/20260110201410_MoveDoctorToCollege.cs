using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveDoctorToCollege : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_doctor_department_department_id",
                table: "doctor");

            migrationBuilder.AlterColumn<int>(
                name: "department_id",
                table: "doctor",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "college_id",
                table: "doctor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_doctor_college_id",
                table: "doctor",
                column: "college_id");

            // FIX: Update existing doctors with their department's college_id
            migrationBuilder.Sql(@"
                UPDATE doctor 
                SET college_id = (SELECT d.college_id FROM department d WHERE d.department_id = doctor.department_id) 
                WHERE department_id IS NOT NULL;
                
                -- For any remaining doctors without a department, assign to first college
                UPDATE doctor SET college_id = (SELECT TOP 1 college_id FROM college ORDER BY college_id) 
                WHERE college_id = 0;
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_doctor_college_college_id",
                table: "doctor",
                column: "college_id",
                principalTable: "college",
                principalColumn: "college_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_doctor_department_department_id",
                table: "doctor",
                column: "department_id",
                principalTable: "department",
                principalColumn: "department_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_doctor_college_college_id",
                table: "doctor");

            migrationBuilder.DropForeignKey(
                name: "FK_doctor_department_department_id",
                table: "doctor");

            migrationBuilder.DropIndex(
                name: "IX_doctor_college_id",
                table: "doctor");

            migrationBuilder.DropColumn(
                name: "college_id",
                table: "doctor");

            migrationBuilder.AlterColumn<int>(
                name: "department_id",
                table: "doctor",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_doctor_department_department_id",
                table: "doctor",
                column: "department_id",
                principalTable: "department",
                principalColumn: "department_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
