using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineLearningPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseCreationFlowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VideoSourceType",
                table: "LessonResources",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subtitle",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.LanguageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CreatedBy",
                table: "Courses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Status",
                table: "Courses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_ModuleId_OrderIndex",
                table: "Lessons",
                columns: new[] { "ModuleId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_GradedItemId_OrderIndex",
                table: "Questions",
                columns: new[] { "GradedItemId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionId_OrderIndex",
                table: "AnswerOptions",
                columns: new[] { "QuestionId", "OrderIndex" });

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Languages_LanguageId",
                table: "Courses",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "LanguageId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Languages_LanguageId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CreatedBy",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Status",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_ModuleId_OrderIndex",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Questions_GradedItemId_OrderIndex",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_AnswerOptions_QuestionId_OrderIndex",
                table: "AnswerOptions");

            migrationBuilder.DropColumn(
                name: "VideoSourceType",
                table: "LessonResources");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Subtitle",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Courses");
        }
    }
}
