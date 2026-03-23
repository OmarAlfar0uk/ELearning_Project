using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearningProject.data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLectureAssignmentToOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assignments_LectureId",
                table: "Assignments");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_LectureId",
                table: "Assignments",
                column: "LectureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assignments_LectureId",
                table: "Assignments");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_LectureId",
                table: "Assignments",
                column: "LectureId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
