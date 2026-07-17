using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearningProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCoinTransactionStage2Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedExamAttemptId",
                table: "CoinTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedSubmissionId",
                table: "CoinTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoinTransactions_RelatedExamAttemptId",
                table: "CoinTransactions",
                column: "RelatedExamAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinTransactions_RelatedSubmissionId",
                table: "CoinTransactions",
                column: "RelatedSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoinTransactions_ExamAttempts_RelatedExamAttemptId",
                table: "CoinTransactions",
                column: "RelatedExamAttemptId",
                principalTable: "ExamAttempts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CoinTransactions_Submissions_RelatedSubmissionId",
                table: "CoinTransactions",
                column: "RelatedSubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoinTransactions_ExamAttempts_RelatedExamAttemptId",
                table: "CoinTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_CoinTransactions_Submissions_RelatedSubmissionId",
                table: "CoinTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CoinTransactions_RelatedExamAttemptId",
                table: "CoinTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CoinTransactions_RelatedSubmissionId",
                table: "CoinTransactions");

            migrationBuilder.DropColumn(
                name: "RelatedExamAttemptId",
                table: "CoinTransactions");

            migrationBuilder.DropColumn(
                name: "RelatedSubmissionId",
                table: "CoinTransactions");
        }
    }
}
