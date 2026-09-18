using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEmployeeSupport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsEmbeddingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmbeddingModel",
                table: "AIModels",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmbeddingModel",
                table: "AIModels");
        }
    }
}
