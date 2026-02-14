using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_status_history",
                columns: table => new
                {
                    history_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    project_id = table.Column<int>(type: "int", nullable: false),
                    old_status = table.Column<int>(type: "int", nullable: false),
                    new_status = table.Column<int>(type: "int", nullable: false),
                    changed_by_user_id = table.Column<int>(type: "int", nullable: true),
                    change_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "IX_project_status_history_changed_by_user_id",
                table: "project_status_history",
                column: "changed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_status_history_project_id",
                table: "project_status_history",
                column: "project_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_status_history");
        }
    }
}
