using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddTaleClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "res_claim_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, comment: "Mã loại claim (unique, uppercase)"),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Tên loại claim"),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động"),
                    extra_properties = table.Column<string>(type: "text", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_res_claim_type", x => x.id);
                },
                comment: "Bảng định nghĩa loại claim:\n- vehicle_claim\n- health_claim\n- house_claim\n....");

            migrationBuilder.CreateTable(
                name: "RESINCIDENTCAUSE",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CODE = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, comment: "Mã nguyên nhân tổn thất (unique, uppercase)"),
                    NAME = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Tên nguyên nhân tổn thất"),
                    STATUS = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động"),
                    EXTRA_PROPERTIES = table.Column<string>(type: "text", nullable: false),
                    CONCURRENCY_STAMP = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: false),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LASTMODIFIERID = table.Column<Guid>(type: "uuid", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DELETER_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    DELETION_TIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RESINCIDENTCAUSE", x => x.ID);
                },
                comment: "Định nghĩa các nguyên nhân tổn thất");

            migrationBuilder.CreateTable(
                name: "RESINCIDENTLEVEL",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CODE = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, comment: "Mã mức độ tổn thất (unique, uppercase)"),
                    NAME = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Tên mức độ tổn thất"),
                    STATUS = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động"),
                    EXTRA_PROPERTIES = table.Column<string>(type: "text", nullable: false),
                    CONCURRENCY_STAMP = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: false),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LASTMODIFIERID = table.Column<Guid>(type: "uuid", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DELETER_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    DELETION_TIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RESINCIDENTLEVEL", x => x.ID);
                },
                comment: "Định nghĩa mức độ tổn thất");

            migrationBuilder.CreateTable(
                name: "CLAIM",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    LOBID = table.Column<Guid>(type: "uuid", nullable: false, comment: "Id link đến nghiệp vụ bảo hiểm"),
                    CLAIMTYPEID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Loại claim, dựa vào loại này có thể có các process claim khác nhau (ví dụ: vehicle, property, health sẽ có process khác nhau)"),
                    INSURERID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Đối tác bảo hiểm gốc liên quan"),
                    CANCELREASONID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Lý do khi hủy yêu cầu"),
                    OPENEMPLOYEEID = table.Column<Guid>(type: "uuid", nullable: false, comment: "Nhân viên mở yêu cầu"),
                    CLOSEEMPLOYEEID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Nhân viên đóng yêu cầu"),
                    CANCELEMPLOYEEID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Nhân viên hủy yêu cầu"),
                    CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Mã tiếp nhận"),
                    PROCESSCLAIMTYPE = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Phân loại xử lý: own (broker xử lý), insurer (bảo hiểm gốc xử lý - cần theo dõi tiến độ)"),
                    INSURERINCIDENTCODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Mã tiếp nhận của đối tác"),
                    STATUS = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái vụ tổn thất: draft (Nháp), pending_receive (chờ xử lý), inprogress (đang xử lý), closed (đã đóng), called (đã hủy)"),
                    OPENDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày mở"),
                    CLOSEDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày đóng"),
                    CANCELDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày hủy"),
                    NOTIFYDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Ngày thông báo"),
                    NOTIFIERNAME = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Tên người thông báo"),
                    NOTIFIERPHONE = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Số điện thoại người thông báo"),
                    NOTIFIEREMAIL = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Email người thông báo"),
                    NOTIFIERINRELATIONSHIP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Moi quan he cua nguoi thong bao voi doi tuong dc bao hiem hoac nguoi dai dien. Lay theo cau hinh trong bang AdminConfig (code = CLAIM_RELATIONSHIP)"),
                    CONTACTNAME = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Tên người liên hệ"),
                    CONTACTPHONE = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Số điện thoại người liên hệ"),
                    CONTACTEMAIL = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Email người liên hệ"),
                    CONTACTINRELATIONSHIP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Moi quan he cua nguoi liên hệ voi doi tuong dc bao hiem hoac nguoi dai dien. Lay theo cau hinh trong bang app_domain (code = CLAIM_RELATIONSHIP, group = M)"),
                    PRIORITY = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Mức độ ưu tiên: high (cao), medium (trung bình), low (thấp)"),
                    SNAPSHOTLINK = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Link webview để người báo tổn thất bổ sung thông tin hiện trường và theo dõi tiến độ xử lý bồi thường"),
                    CANCELNOTE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Ghi chú khi hủy yêu cầu"),
                    EXTRA_PROPERTIES = table.Column<string>(type: "text", nullable: false),
                    CONCURRENCY_STAMP = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: true),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LASTMODIFIERID = table.Column<Guid>(type: "uuid", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DELETER_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    DELETION_TIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLAIM", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CLAIM_REFERENCE_PROLINEO",
                        column: x => x.LOBID,
                        principalTable: "pro_line_of_business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIM_REFERENCE_RESCLAIM",
                        column: x => x.CLAIMTYPEID,
                        principalTable: "res_claim_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIM_REFERENCE_RESPARTN",
                        column: x => x.INSURERID,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIM_hr_employee_CANCELEMPLOYEEID",
                        column: x => x.CANCELEMPLOYEEID,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIM_hr_employee_CLOSEEMPLOYEEID",
                        column: x => x.CLOSEEMPLOYEEID,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIM_hr_employee_OPENEMPLOYEEID",
                        column: x => x.OPENEMPLOYEEID,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIM_res_reason_CANCELREASONID",
                        column: x => x.CANCELREASONID,
                        principalTable: "res_reason",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu yêu cầu bồi thường");

            migrationBuilder.CreateTable(
                name: "CLAIMINCIDENT",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    INCIDENTID = table.Column<Guid>(type: "uuid", nullable: false, comment: "Id của claim"),
                    OBJECTTYPEID = table.Column<Guid>(type: "uuid", nullable: false, comment: "Loại đối tượng tổn thất"),
                    ASSESSMENTPARTNERID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Bên thực hiện giám định"),
                    INCIDENTLEVELID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Mức độ tổn thất"),
                    INCIDENTPROVINCEID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tỉnh nơi xảy ra tổn thất"),
                    INCIDENTWARDID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phường/xã nơi xảy ra tổn thất"),
                    INCIDENTCAUSEID = table.Column<Guid>(type: "uuid", nullable: false, comment: "Nguyên nhân xảy ra tổn thất"),
                    ONLOCATION = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, comment: "Còn ở hiện trường hay không: Y (có), N (không)"),
                    ASSESSMENTDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày dự kiến giám định chi tiết"),
                    INCIDENTDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Thời điểm tổn thất"),
                    INCIDENTADDRESS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Địa chỉ nơi xảy ra tổn thất"),
                    INCIDENTFULLADDRESS = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Địa chỉ đầy đủ nơi xảy ra tổn thất"),
                    INCIDENTLAT = table.Column<double>(type: "FLOAT8", nullable: true, comment: "Vĩ độ nơi xảy ra tổn thất"),
                    INCIDENTLONG = table.Column<double>(type: "FLOAT8", nullable: true, comment: "Kinh độ nơi xảy ra tổn thất"),
                    INCIDENTDESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Mô tả sự việc dẫn đến tổn thất"),
                    INCIDENTRESULT = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Hậu quả của tổn thất"),
                    NOTE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Ghi chú khác"),
                    EXTRA_PROPERTIES = table.Column<string>(type: "text", nullable: false),
                    CONCURRENCY_STAMP = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: true),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LASTMODIFIERID = table.Column<Guid>(type: "uuid", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DELETER_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    DELETION_TIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLAIMINCIDENT", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CLAIMINCIDENT_res_object_type_OBJECTTYPEID",
                        column: x => x.OBJECTTYPEID,
                        principalTable: "res_object_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIMINCIDENT_res_partner_ASSESSMENTPARTNERID",
                        column: x => x.ASSESSMENTPARTNERID,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIMINCIDENT_res_province_INCIDENTPROVINCEID",
                        column: x => x.INCIDENTPROVINCEID,
                        principalTable: "res_province",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIMINCIDENT_res_ward_INCIDENTWARDID",
                        column: x => x.INCIDENTWARDID,
                        principalTable: "res_ward",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIMINC_REFERENCE_CLAIM",
                        column: x => x.INCIDENTID,
                        principalTable: "CLAIM",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIMINC_REFERENCE_RESINCID_CAUSE",
                        column: x => x.INCIDENTCAUSEID,
                        principalTable: "RESINCIDENTCAUSE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CLAIMINC_REFERENCE_RESINCID_LEVEL",
                        column: x => x.INCIDENTLEVELID,
                        principalTable: "RESINCIDENTLEVEL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Đối tượng tổn thất");

            migrationBuilder.CreateTable(
                name: "CLAIMINCIDENTRISKMOTOR",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    INCIDENTOBJECTID = table.Column<Guid>(type: "uuid", nullable: true, comment: "Id của đối tượng tổn thất"),
                    CARPLATE = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Biển số xe"),
                    VIN = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true, comment: "Số khung"),
                    ENGINENUMBER = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true, comment: "Số máy"),
                    PERSONSONCAR = table.Column<decimal>(type: "numeric(3,0)", nullable: true, comment: "Số người trên xe khi xảy ra tổn thất"),
                    DRIVERNAME = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Tên lái xe"),
                    DRIVERPHONE = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Số điện thoại lái xe"),
                    DRIVERLICENSENO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Số giấy phép lái xe"),
                    DRIVERLICENSELEVEL = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true, comment: "Hạng giấy phép lái xe của người lái xe: cấu hình trong bảng AdminConfig (code: DERIVER_LICENSE_LEVEL)"),
                    DRIVERLICENSEEFFECTDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày hiệu lực giấy phép lái xe"),
                    DRIVERLICENSEEXPIREDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày hết hạn giấy phép lái xe"),
                    DRIVERSEX = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Giới tính của lái xe: M (nam), F (nữ)"),
                    CARREGISTRYNO = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Số giấy đăng ký xe"),
                    CARREGISTRYEFFECTDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày hiệu lực giấy đăng ký xe"),
                    CARREGISTRYEXPIREDATE = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Ngày hết hạn giấy đăng ký xe"),
                    EXTRA_PROPERTIES = table.Column<string>(type: "text", nullable: false),
                    CONCURRENCY_STAMP = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CREATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CREATORID = table.Column<Guid>(type: "uuid", nullable: false),
                    LASTMODIFICATIONTIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LASTMODIFIERID = table.Column<Guid>(type: "uuid", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DELETER_ID = table.Column<Guid>(type: "uuid", nullable: true),
                    DELETION_TIME = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CLAIMINCIDENTRISKMOTOR", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CLAIMINC_REFERENCE_CLAIMINC",
                        column: x => x.INCIDENTOBJECTID,
                        principalTable: "CLAIMINCIDENT",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu thông tin đối tượng bảo hiểm (Oto, Xe máy)");

            migrationBuilder.CreateIndex(
                name: "IX_CLAIM_CANCELEMPLOYEEID",
                table: "CLAIM",
                column: "CANCELEMPLOYEEID");

            migrationBuilder.CreateIndex(
                name: "IX_CLAIM_CANCELREASONID",
                table: "CLAIM",
                column: "CANCELREASONID");

            migrationBuilder.CreateIndex(
                name: "ix_claim_claim_type_id",
                table: "CLAIM",
                column: "CLAIMTYPEID");

            migrationBuilder.CreateIndex(
                name: "IX_CLAIM_CLOSEEMPLOYEEID",
                table: "CLAIM",
                column: "CLOSEEMPLOYEEID");

            migrationBuilder.CreateIndex(
                name: "ix_claim_code",
                table: "CLAIM",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_claim_insurer_id",
                table: "CLAIM",
                column: "INSURERID");

            migrationBuilder.CreateIndex(
                name: "ix_claim_lob_id",
                table: "CLAIM",
                column: "LOBID");

            migrationBuilder.CreateIndex(
                name: "ix_claim_open_employee_id",
                table: "CLAIM",
                column: "OPENEMPLOYEEID");

            migrationBuilder.CreateIndex(
                name: "ix_claim_status",
                table: "CLAIM",
                column: "STATUS");

            migrationBuilder.CreateIndex(
                name: "ix_claim_incident_incident_cause_id",
                table: "CLAIMINCIDENT",
                column: "INCIDENTCAUSEID");

            migrationBuilder.CreateIndex(
                name: "ix_claim_incident_incident_id",
                table: "CLAIMINCIDENT",
                column: "INCIDENTID");

            migrationBuilder.CreateIndex(
                name: "ix_claim_incident_object_type_id",
                table: "CLAIMINCIDENT",
                column: "OBJECTTYPEID");

            migrationBuilder.CreateIndex(
                name: "IX_CLAIMINCIDENT_ASSESSMENTPARTNERID",
                table: "CLAIMINCIDENT",
                column: "ASSESSMENTPARTNERID");

            migrationBuilder.CreateIndex(
                name: "IX_CLAIMINCIDENT_INCIDENTLEVELID",
                table: "CLAIMINCIDENT",
                column: "INCIDENTLEVELID");

            migrationBuilder.CreateIndex(
                name: "IX_CLAIMINCIDENT_INCIDENTPROVINCEID",
                table: "CLAIMINCIDENT",
                column: "INCIDENTPROVINCEID");

            migrationBuilder.CreateIndex(
                name: "IX_CLAIMINCIDENT_INCIDENTWARDID",
                table: "CLAIMINCIDENT",
                column: "INCIDENTWARDID");

            migrationBuilder.CreateIndex(
                name: "ix_claim_incident_risk_motor_incident_object_id",
                table: "CLAIMINCIDENTRISKMOTOR",
                column: "INCIDENTOBJECTID");

            migrationBuilder.CreateIndex(
                name: "ix_res_claim_type_code",
                table: "res_claim_type",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_incident_cause_code",
                table: "RESINCIDENTCAUSE",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_res_incident_level_code",
                table: "RESINCIDENTLEVEL",
                column: "CODE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CLAIMINCIDENTRISKMOTOR");

            migrationBuilder.DropTable(
                name: "CLAIMINCIDENT");

            migrationBuilder.DropTable(
                name: "CLAIM");

            migrationBuilder.DropTable(
                name: "RESINCIDENTCAUSE");

            migrationBuilder.DropTable(
                name: "RESINCIDENTLEVEL");

            migrationBuilder.DropTable(
                name: "res_claim_type");
        }
    }
}
