using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassAssistantBot.Migrations
{
    public partial class AddMiscellaneous : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MiscelaneousChannel",
                table: "ClassRooms",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MiscelaneousChannel",
                table: "ClassRooms");
        }
    }
}
