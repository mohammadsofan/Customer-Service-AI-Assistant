using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEmployeeSupport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseUrlToAIProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseUrl",
                table: "AIProviders",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseUrl",
                table: "AIProviders");
        }
    }
}
