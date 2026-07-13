using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearningProject.data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorAssignedByAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedByAdminId",
                table: "InstructorTracks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstructorTracks_AssignedByAdminId",
                table: "InstructorTracks",
                column: "AssignedByAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorTracks_Users_AssignedByAdminId",
                table: "InstructorTracks",
                column: "AssignedByAdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstructorTracks_Users_AssignedByAdminId",
                table: "InstructorTracks");

            migrationBuilder.DropIndex(
                name: "IX_InstructorTracks_AssignedByAdminId",
                table: "InstructorTracks");

            migrationBuilder.DropColumn(
                name: "AssignedByAdminId",
                table: "InstructorTracks");
        }
    }
}
