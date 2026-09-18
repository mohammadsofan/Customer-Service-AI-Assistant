using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEmployeeSupport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLLMRerankingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MathTopScenarioId",
                table: "SupportQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RerankedScenarioId",
                table: "SupportQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RerankingFailed",
                table: "SupportQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RerankingNoMatch",
                table: "SupportQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RerankingUsed",
                table: "SupportQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "LLMRerankingConfidenceThreshold",
                table: "AIConfigurations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "LLMRerankingEnabled",
                table: "AIConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LLMRerankingTopK",
                table: "AIConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MathTopScenarioId",
                table: "SupportQuestions");

            migrationBuilder.DropColumn(
                name: "RerankedScenarioId",
                table: "SupportQuestions");

            migrationBuilder.DropColumn(
                name: "RerankingFailed",
                table: "SupportQuestions");

            migrationBuilder.DropColumn(
                name: "RerankingNoMatch",
                table: "SupportQuestions");

            migrationBuilder.DropColumn(
                name: "RerankingUsed",
                table: "SupportQuestions");

            migrationBuilder.DropColumn(
                name: "LLMRerankingConfidenceThreshold",
                table: "AIConfigurations");

            migrationBuilder.DropColumn(
                name: "LLMRerankingEnabled",
                table: "AIConfigurations");

            migrationBuilder.DropColumn(
                name: "LLMRerankingTopK",
                table: "AIConfigurations");
        }
    }
}
