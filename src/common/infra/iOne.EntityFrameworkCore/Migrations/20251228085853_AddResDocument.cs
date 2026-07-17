using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddResDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_document",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Phân nhóm (theo định nghĩa trong bảng AdminConfig với code = 'DOCUMENT_GROUP':\n1: Cac tai lieu khac,\n2: Tai lieu hien truong,\n3: Tai lieu toan canh,\n4: Ho so,\n5: Bao gia,\n6: Tai lieu trinh ky,\n7: Hang muc ton that"),
                    doc_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false, comment: "Kích thước file upload (đơn vị byte)"),
                    file_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Tên file gốc do người dùng upload lên"),
                    store_file_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Tên file lưu trữ (do hệ thống tự sinh)"),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Đường dẫn lưu file (nếu có)"),
                    thumbnail_url = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Hình ảnh dung lượng thấp, đại diện cho hình ảnh thật"),
                    checksum = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true, comment: "Chuỗi mã hóa để kiểm tra tính toàn vẹn của file"),
                    mime_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Loại file"),
                    bucket_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Tên Buket chứa file"),
                    version_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true, comment: "Version file nếu có bật tính năng này trên MinIO"),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_res_document", x => x.id);
                    table.ForeignKey(
                        name: "fk_res_document_res_document_type_doc_type_id",
                        column: x => x.doc_type_id,
                        principalTable: "res_document_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu trữ các tài liệu đính kèm");

            migrationBuilder.CreateIndex(
                name: "ix_res_document_doc_type_id",
                table: "res_document",
                column: "doc_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_document_doc_type_id"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_document"";");
        }
    }
}
