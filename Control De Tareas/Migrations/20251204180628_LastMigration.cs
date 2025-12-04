using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Control_De_Tareas.Migrations
{
    /// <inheritdoc />
    public partial class LastMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseOfferings_Periods_PeriodId",
                table: "CourseOfferings");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseOfferings_Periods_PeriodId",
                table: "CourseOfferings",
                column: "PeriodId",
                principalTable: "Periods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseOfferings_Periods_PeriodId",
                table: "CourseOfferings");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseOfferings_Periods_PeriodId",
                table: "CourseOfferings",
                column: "PeriodId",
                principalTable: "Periods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
