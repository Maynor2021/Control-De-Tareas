using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Control_De_Tareas.Migrations
{
    /// <inheritdoc />
    public partial class AddAllEntitiesAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_CourseOfferings_CourseOfferingId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_CourseOfferings_CourseOfferingId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_CourseOfferings_CourseOfferingsId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_CreatedBy",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CourseOfferingsId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CreatedBy",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CourseOfferingsId",
                table: "Tasks");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserUserId",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedByUserUserId",
                table: "Tasks",
                column: "CreatedByUserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_CourseId_PeriodId_Section",
                table: "CourseOfferings",
                columns: new[] { "CourseId", "PeriodId", "Section" },
                unique: true,
                filter: "[IsSoftDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_CourseOfferings_CourseOfferingId",
                table: "Announcements",
                column: "CourseOfferingId",
                principalTable: "CourseOfferings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_CourseOfferings_CourseOfferingId",
                table: "Enrollments",
                column: "CourseOfferingId",
                principalTable: "CourseOfferings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_CreatedByUserUserId",
                table: "Tasks",
                column: "CreatedByUserUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_CourseOfferings_CourseOfferingId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_CourseOfferings_CourseOfferingId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_CreatedByUserUserId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CreatedByUserUserId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferings_CourseId_PeriodId_Section",
                table: "CourseOfferings");

            migrationBuilder.DropColumn(
                name: "CreatedByUserUserId",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "CourseOfferingsId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CourseOfferingsId",
                table: "Tasks",
                column: "CourseOfferingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedBy",
                table: "Tasks",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_CourseOfferings_CourseOfferingId",
                table: "Announcements",
                column: "CourseOfferingId",
                principalTable: "CourseOfferings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_CourseOfferings_CourseOfferingId",
                table: "Enrollments",
                column: "CourseOfferingId",
                principalTable: "CourseOfferings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_CourseOfferings_CourseOfferingsId",
                table: "Tasks",
                column: "CourseOfferingsId",
                principalTable: "CourseOfferings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_CreatedBy",
                table: "Tasks",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
