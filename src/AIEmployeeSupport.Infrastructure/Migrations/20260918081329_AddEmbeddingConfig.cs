using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEmployeeSupport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddingConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActiveEmbeddingModelId",
                table: "AIConfigurations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActiveEmbeddingProviderId",
                table: "AIConfigurations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIConfigurations_ActiveEmbeddingModelId",
                table: "AIConfigurations",
                column: "ActiveEmbeddingModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AIConfigurations_ActiveEmbeddingProviderId",
                table: "AIConfigurations",
                column: "ActiveEmbeddingProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIConfigurations_AIModels_ActiveEmbeddingModelId",
                table: "AIConfigurations",
                column: "ActiveEmbeddingModelId",
                principalTable: "AIModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AIConfigurations_AIProviders_ActiveEmbeddingProviderId",
                table: "AIConfigurations",
                column: "ActiveEmbeddingProviderId",
                principalTable: "AIProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIConfigurations_AIModels_ActiveEmbeddingModelId",
                table: "AIConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_AIConfigurations_AIProviders_ActiveEmbeddingProviderId",
                table: "AIConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_AIConfigurations_ActiveEmbeddingModelId",
                table: "AIConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_AIConfigurations_ActiveEmbeddingProviderId",
                table: "AIConfigurations");

            migrationBuilder.DropColumn(
                name: "ActiveEmbeddingModelId",
                table: "AIConfigurations");

            migrationBuilder.DropColumn(
                name: "ActiveEmbeddingProviderId",
                table: "AIConfigurations");
        }
    }
}
