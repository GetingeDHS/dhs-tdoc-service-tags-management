using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TCUSTOMER",
                columns: table => new
                {
                    CUSTOMERKEY = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CUSTOMERNUMBER = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CUSTOMERNAME = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CUSTOMERCODE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ISACTIVE = table.Column<bool>(type: "bit", nullable: true),
                    CREATEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CREATEDUSERKEY = table.Column<int>(type: "int", nullable: true),
                    MODIFIEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MODIFIEDUSERKEY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCUSTOMER", x => x.CUSTOMERKEY);
                });

            migrationBuilder.CreateTable(
                name: "TINDICATOR",
                columns: table => new
                {
                    INDICATORKEY = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    INDICATORNUMBER = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    INDICATORNAME = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    INDICATORTYPE = table.Column<int>(type: "int", nullable: true),
                    ISACTIVE = table.Column<bool>(type: "bit", nullable: true),
                    CREATEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CREATEDUSERKEY = table.Column<int>(type: "int", nullable: true),
                    MODIFIEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MODIFIEDUSERKEY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TINDICATOR", x => x.INDICATORKEY);
                });

            migrationBuilder.CreateTable(
                name: "TLOCATION",
                columns: table => new
                {
                    LOCATIONKEY = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LOCATIONNAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LOCATIONCODE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PARENTLOCATIONKEY = table.Column<int>(type: "int", nullable: true),
                    ISACTIVE = table.Column<bool>(type: "bit", nullable: true),
                    CREATEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CREATEDUSERKEY = table.Column<int>(type: "int", nullable: true),
                    MODIFIEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MODIFIEDUSERKEY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TLOCATION", x => x.LOCATIONKEY);
                    table.ForeignKey(
                        name: "FK_TLOCATION_TLOCATION_PARENTLOCATIONKEY",
                        column: x => x.PARENTLOCATIONKEY,
                        principalTable: "TLOCATION",
                        principalColumn: "LOCATIONKEY");
                });

            migrationBuilder.CreateTable(
                name: "TTAGTYPE",
                columns: table => new
                {
                    TAGTYPEKEY = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TAGTYPENAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TAGTYPECODE = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ISACTIVE = table.Column<bool>(type: "bit", nullable: true),
                    CREATEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CREATEDUSERKEY = table.Column<int>(type: "int", nullable: true),
                    MODIFIEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MODIFIEDUSERKEY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TTAGTYPE", x => x.TAGTYPEKEY);
                });

            migrationBuilder.CreateTable(
                name: "TITEM",
                columns: table => new
                {
                    ITEMKEY = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ITEMNUMBER = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ITEMNAME = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DESCRIPTION = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CUSTOMERKEY = table.Column<int>(type: "int", nullable: true),
                    ITEMTYPE = table.Column<int>(type: "int", nullable: true),
                    ISACTIVE = table.Column<bool>(type: "bit", nullable: true),
                    CREATEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CREATEDUSERKEY = table.Column<int>(type: "int", nullable: true),
                    MODIFIEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MODIFIEDUSERKEY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TITEM", x => x.ITEMKEY);
                    table.ForeignKey(
                        name: "FK_TITEM_TCUSTOMER_CUSTOMERKEY",
                        column: x => x.CUSTOMERKEY,
                        principalTable: "TCUSTOMER",
                        principalColumn: "CUSTOMERKEY");
                });

            migrationBuilder.CreateTable(
                name: "TTAGS",
                columns: table => new
                {
                    TAGKEY = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TAGNUMBER = table.Column<int>(type: "int", nullable: true),
                    TAGTYPEKEY = table.Column<int>(type: "int", nullable: true),
                    LOCATIONKEY = table.Column<int>(type: "int", nullable: true),
                    PROCESSBATCHKEY = table.Column<int>(type: "int", nullable: true),
                    ISAUTOTAG = table.Column<bool>(type: "bit", nullable: true),
                    CREATEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CREATEDUSERKEY = table.Column<int>(type: "int", nullable: true),
                    MODIFIEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MODIFIEDUSERKEY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TTAGS", x => x.TAGKEY);
                    table.ForeignKey(
                        name: "FK_TTAGS_TLOCATION_LOCATIONKEY",
                        column: x => x.LOCATIONKEY,
                        principalTable: "TLOCATION",
                        principalColumn: "LOCATIONKEY");
                    table.ForeignKey(
                        name: "FK_TTAGS_TTAGTYPE_TAGTYPEKEY",
                        column: x => x.TAGTYPEKEY,
                        principalTable: "TTAGTYPE",
                        principalColumn: "TAGTYPEKEY");
                });

            migrationBuilder.CreateTable(
                name: "TUNIT",
                columns: table => new
                {
                    UNITKEY = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UNITNUMBER = table.Column<int>(type: "int", nullable: true),
                    SERIALNUMBER = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LOCATIONKEY = table.Column<int>(type: "int", nullable: true),
                    ITEMKEY = table.Column<int>(type: "int", nullable: true),
                    CUSTOMERKEY = table.Column<int>(type: "int", nullable: true),
                    PROCESSBATCHKEY = table.Column<int>(type: "int", nullable: true),
                    STATUS = table.Column<int>(type: "int", nullable: true),
                    CREATEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CREATEDUSERKEY = table.Column<int>(type: "int", nullable: true),
                    MODIFIEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MODIFIEDUSERKEY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TUNIT", x => x.UNITKEY);
                    table.ForeignKey(
                        name: "FK_TUNIT_TCUSTOMER_CUSTOMERKEY",
                        column: x => x.CUSTOMERKEY,
                        principalTable: "TCUSTOMER",
                        principalColumn: "CUSTOMERKEY");
                    table.ForeignKey(
                        name: "FK_TUNIT_TITEM_ITEMKEY",
                        column: x => x.ITEMKEY,
                        principalTable: "TITEM",
                        principalColumn: "ITEMKEY");
                    table.ForeignKey(
                        name: "FK_TUNIT_TLOCATION_LOCATIONKEY",
                        column: x => x.LOCATIONKEY,
                        principalTable: "TLOCATION",
                        principalColumn: "LOCATIONKEY");
                });

            migrationBuilder.CreateTable(
                name: "TTAGCONTENT",
                columns: table => new
                {
                    TAGCONTENTKEY = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PARENTTAGKEY = table.Column<int>(type: "int", nullable: false),
                    CHILDTAGKEY = table.Column<int>(type: "int", nullable: true),
                    UNITKEY = table.Column<int>(type: "int", nullable: true),
                    ITEMKEY = table.Column<int>(type: "int", nullable: true),
                    SERIALKEY = table.Column<int>(type: "int", nullable: true),
                    LOTINFOKEY = table.Column<int>(type: "int", nullable: true),
                    INDICATORKEY = table.Column<int>(type: "int", nullable: true),
                    LOCATIONKEY = table.Column<int>(type: "int", nullable: true),
                    CREATEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CREATEDUSERKEY = table.Column<int>(type: "int", nullable: true),
                    MODIFIEDTIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MODIFIEDUSERKEY = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TTAGCONTENT", x => x.TAGCONTENTKEY);
                    table.ForeignKey(
                        name: "FK_TTAGCONTENT_TINDICATOR_INDICATORKEY",
                        column: x => x.INDICATORKEY,
                        principalTable: "TINDICATOR",
                        principalColumn: "INDICATORKEY");
                    table.ForeignKey(
                        name: "FK_TTAGCONTENT_TITEM_ITEMKEY",
                        column: x => x.ITEMKEY,
                        principalTable: "TITEM",
                        principalColumn: "ITEMKEY");
                    table.ForeignKey(
                        name: "FK_TTAGCONTENT_TLOCATION_LOCATIONKEY",
                        column: x => x.LOCATIONKEY,
                        principalTable: "TLOCATION",
                        principalColumn: "LOCATIONKEY");
                    table.ForeignKey(
                        name: "FK_TTAGCONTENT_TTAGS_CHILDTAGKEY",
                        column: x => x.CHILDTAGKEY,
                        principalTable: "TTAGS",
                        principalColumn: "TAGKEY");
                    table.ForeignKey(
                        name: "FK_TTAGCONTENT_TTAGS_PARENTTAGKEY",
                        column: x => x.PARENTTAGKEY,
                        principalTable: "TTAGS",
                        principalColumn: "TAGKEY",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TTAGCONTENT_TUNIT_UNITKEY",
                        column: x => x.UNITKEY,
                        principalTable: "TUNIT",
                        principalColumn: "UNITKEY");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_Customer",
                table: "TITEM",
                column: "CUSTOMERKEY");

            migrationBuilder.CreateIndex(
                name: "IX_TLOCATION_PARENTLOCATIONKEY",
                table: "TLOCATION",
                column: "PARENTLOCATIONKEY");

            migrationBuilder.CreateIndex(
                name: "IX_TTAGCONTENT_CHILDTAGKEY",
                table: "TTAGCONTENT",
                column: "CHILDTAGKEY");

            migrationBuilder.CreateIndex(
                name: "IX_TTAGCONTENT_INDICATORKEY",
                table: "TTAGCONTENT",
                column: "INDICATORKEY");

            migrationBuilder.CreateIndex(
                name: "IX_TTAGCONTENT_ITEMKEY",
                table: "TTAGCONTENT",
                column: "ITEMKEY");

            migrationBuilder.CreateIndex(
                name: "IX_TTAGCONTENT_UNITKEY",
                table: "TTAGCONTENT",
                column: "UNITKEY");

            migrationBuilder.CreateIndex(
                name: "IX_TagContent_Location",
                table: "TTAGCONTENT",
                column: "LOCATIONKEY");

            migrationBuilder.CreateIndex(
                name: "IX_TagContent_ParentTag",
                table: "TTAGCONTENT",
                column: "PARENTTAGKEY");

            migrationBuilder.CreateIndex(
                name: "IX_TTAGS_TAGTYPEKEY",
                table: "TTAGS",
                column: "TAGTYPEKEY");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_IsAutoTag",
                table: "TTAGS",
                column: "ISAUTOTAG");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Location",
                table: "TTAGS",
                column: "LOCATIONKEY");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ProcessBatch",
                table: "TTAGS",
                column: "PROCESSBATCHKEY");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagNumber_TagType",
                table: "TTAGS",
                columns: new[] { "TAGNUMBER", "TAGTYPEKEY" });

            migrationBuilder.CreateIndex(
                name: "IX_TUNIT_CUSTOMERKEY",
                table: "TUNIT",
                column: "CUSTOMERKEY");

            migrationBuilder.CreateIndex(
                name: "IX_TUNIT_ITEMKEY",
                table: "TUNIT",
                column: "ITEMKEY");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Location",
                table: "TUNIT",
                column: "LOCATIONKEY");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TTAGCONTENT");

            migrationBuilder.DropTable(
                name: "TINDICATOR");

            migrationBuilder.DropTable(
                name: "TTAGS");

            migrationBuilder.DropTable(
                name: "TUNIT");

            migrationBuilder.DropTable(
                name: "TTAGTYPE");

            migrationBuilder.DropTable(
                name: "TITEM");

            migrationBuilder.DropTable(
                name: "TLOCATION");

            migrationBuilder.DropTable(
                name: "TCUSTOMER");
        }
    }
}
