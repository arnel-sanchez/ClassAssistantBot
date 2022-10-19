using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassAssistantBot.Migrations
{
    public partial class UpdatePracticalClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PracticClassPendings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PracticClassPendings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PracticClassId = table.Column<string>(type: "text", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticClassPendings", x => x.Id);
                });
        }
    }
}
