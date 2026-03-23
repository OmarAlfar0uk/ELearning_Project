using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearningProject.data.Migrations
{
    /// <inheritdoc />
    public partial class AddFilteredUniqueIndexToAssignmentLectureId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assignments_LectureId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Submissions");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_LectureId",
                table: "Assignments",
                column: "LectureId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assignments_LectureId",
                table: "Assignments");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Submissions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_LectureId",
                table: "Assignments",
                column: "LectureId",
                unique: true);
        }
    }
}
