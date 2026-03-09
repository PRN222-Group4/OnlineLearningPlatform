using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineLearningPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixGradedAttemptFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GradedAttempts_GradedItems_GradedAttemptId",
                table: "GradedAttempts");

            migrationBuilder.AddColumn<Guid>(
                name: "GradedItemId1",
                table: "GradedAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradedAttempts_GradedItemId",
                table: "GradedAttempts",
                column: "GradedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GradedAttempts_GradedItemId1",
                table: "GradedAttempts",
                column: "GradedItemId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GradedAttempts_GradedItems_GradedItemId",
                table: "GradedAttempts",
                column: "GradedItemId",
                principalTable: "GradedItems",
                principalColumn: "GradedItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GradedAttempts_GradedItems_GradedItemId1",
                table: "GradedAttempts",
                column: "GradedItemId1",
                principalTable: "GradedItems",
                principalColumn: "GradedItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GradedAttempts_GradedItems_GradedItemId",
                table: "GradedAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_GradedAttempts_GradedItems_GradedItemId1",
                table: "GradedAttempts");

            migrationBuilder.DropIndex(
                name: "IX_GradedAttempts_GradedItemId",
                table: "GradedAttempts");

            migrationBuilder.DropIndex(
                name: "IX_GradedAttempts_GradedItemId1",
                table: "GradedAttempts");

            migrationBuilder.DropColumn(
                name: "GradedItemId1",
                table: "GradedAttempts");

            migrationBuilder.AddForeignKey(
                name: "FK_GradedAttempts_GradedItems_GradedAttemptId",
                table: "GradedAttempts",
                column: "GradedAttemptId",
                principalTable: "GradedItems",
                principalColumn: "GradedItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
