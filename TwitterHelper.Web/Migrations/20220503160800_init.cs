using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TwitterHelper.Web.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DateTimeReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsersLookupTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TweetsLookupTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimelinesTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilteredStreamTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FollowsTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateTimeReferences", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DateTimeReferences");
        }
    }
}
