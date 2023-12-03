using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SatApiTest.Migrations
{
    /// <inheritdoc />
    public partial class initialdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SelfAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Institution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Domain1Competency1 = table.Column<int>(type: "int", nullable: false),
                    Domain1Competency2 = table.Column<int>(type: "int", nullable: false),
                    Domain1Competency3 = table.Column<int>(type: "int", nullable: false),
                    Domain1Competency4 = table.Column<int>(type: "int", nullable: false),
                    Domain1Competency5 = table.Column<int>(type: "int", nullable: false),
                    Domain1Competency6 = table.Column<int>(type: "int", nullable: false),
                    Domain1Competency1Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain1Competency2Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain1Competency3Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain1Competency4Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain1Competency5Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain1Competency6Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain2Competency1 = table.Column<int>(type: "int", nullable: false),
                    Domain2Competency2 = table.Column<int>(type: "int", nullable: false),
                    Domain2Competency3 = table.Column<int>(type: "int", nullable: false),
                    Domain2Competency4 = table.Column<int>(type: "int", nullable: false),
                    Domain2Competency1Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain2Competency2Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain2Competency3Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain2Competency4Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain3Competency1 = table.Column<int>(type: "int", nullable: false),
                    Domain3Competency2 = table.Column<int>(type: "int", nullable: false),
                    Domain3Competency3 = table.Column<int>(type: "int", nullable: false),
                    Domain3Competency4 = table.Column<int>(type: "int", nullable: false),
                    Domain3Competency1Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain3Competency2Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain3Competency3Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain3Competency4Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain4Competency1 = table.Column<int>(type: "int", nullable: false),
                    Domain4Competency2 = table.Column<int>(type: "int", nullable: false),
                    Domain4Competency3 = table.Column<int>(type: "int", nullable: false),
                    Domain4Competency4 = table.Column<int>(type: "int", nullable: false),
                    Domain4Competency5 = table.Column<int>(type: "int", nullable: false),
                    Domain4Competency1Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain4Competency2Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain4Competency3Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain4Competency4Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain4Competency5Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain5Competency1 = table.Column<int>(type: "int", nullable: false),
                    Domain5Competency2 = table.Column<int>(type: "int", nullable: false),
                    Domain5Competency3 = table.Column<int>(type: "int", nullable: false),
                    Domain5Competency4 = table.Column<int>(type: "int", nullable: false),
                    Domain5Competency1Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain5Competency2Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain5Competency3Important = table.Column<bool>(type: "bit", nullable: false),
                    Domain5Competency4Important = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssessments", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SelfAssessments");
        }
    }
}
