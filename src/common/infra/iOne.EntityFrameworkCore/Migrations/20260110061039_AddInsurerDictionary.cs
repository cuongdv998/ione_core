using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddInsurerDictionary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "insurerdictionary",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    businessname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    owncode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    insurercode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    extradata = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái: active (hoạt động), deactive (không hoạt động)"),
                    effectdate = table.Column<DateTime>(type: "date", nullable: false),
                    expiredate = table.Column<DateTime>(type: "date", nullable: false),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_insurerdictionary", x => x.id);
                },
                comment: "Bảng mapping các dữ liệu master data với công ty bảo hiểm gốc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "insurerdictionary");
        }
    }
}
