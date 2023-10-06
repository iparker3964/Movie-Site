using Microsoft.EntityFrameworkCore.Migrations;

namespace MovieProDemo.Data.Migrations
{
    public partial class movieProModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "VoteAverage",
                table: "Movie",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoteAverage",
                table: "Movie");
        }
    }
}
