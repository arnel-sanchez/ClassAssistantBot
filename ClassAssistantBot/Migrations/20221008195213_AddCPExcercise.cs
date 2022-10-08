using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassAssistantBot.Migrations
{
    public partial class AddCPExcercise : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PracticClasses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ClassRoomId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticClasses_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Excercises",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<long>(type: "bigint", nullable: false),
                    PracticClassId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Excercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Excercises_PracticClasses_PracticClassId",
                        column: x => x.PracticClassId,
                        principalTable: "PracticClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Excercises_PracticClassId",
                table: "Excercises",
                column: "PracticClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticClasses_ClassRoomId",
                table: "PracticClasses",
                column: "ClassRoomId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Excercises");

            migrationBuilder.DropTable(
                name: "PracticClasses");
        }
    }
}
