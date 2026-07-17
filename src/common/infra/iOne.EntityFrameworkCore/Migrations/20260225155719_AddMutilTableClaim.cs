using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddMutilTableClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "claim_adjust_at_location",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claimid = table.Column<Guid>(type: "uuid", nullable: false),
                    adjustorid = table.Column<Guid>(type: "uuid", nullable: false),
                    startdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    enddate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Trạng thái giám định hiện trường"),
                    lossposition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    haslossthirdparty = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    witnesstestimony = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    causedescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    locationdescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    damagedescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    partiesinvolveddescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    addressplan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    customerrecommendation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    otherdescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    isinscope = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    garageid = table.Column<Guid>(type: "uuid", nullable: true),
                    issuedate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ClaimId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    AdjustorId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    GarageId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_adjust_at_location", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_adjust_at_location_claim_ClaimId1",
                        column: x => x.ClaimId1,
                        principalTable: "claim",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_adjust_at_location_claim_claimid",
                        column: x => x.claimid,
                        principalTable: "claim",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_adjust_at_location_hr_employee_AdjustorId1",
                        column: x => x.AdjustorId1,
                        principalTable: "hr_employee",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_adjust_at_location_hr_employee_adjustorid",
                        column: x => x.adjustorid,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_adjust_at_location_res_partner_GarageId1",
                        column: x => x.GarageId1,
                        principalTable: "res_partner",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_adjust_at_location_res_partner_garageid",
                        column: x => x.garageid,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu thông tin giám định hiện trường");

            migrationBuilder.CreateTable(
                name: "claim_folder",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    folderno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Số hồ sơ"),
                    foldername = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true, comment: "Tên hồ sơ"),
                    insurerid = table.Column<Guid>(type: "uuid", nullable: true, comment: "Đối tác bảo hiểm gốc liên quan"),
                    claimid = table.Column<Guid>(type: "uuid", nullable: false, comment: "Lần thông báo tổn thất"),
                    incidentid = table.Column<Guid>(type: "uuid", nullable: true, comment: "Vụ tổn thất"),
                    productid = table.Column<Guid>(type: "uuid", nullable: true),
                    claimtypeid = table.Column<Guid>(type: "uuid", nullable: false, comment: "Loại claim"),
                    policyno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Mã đơn bảo hiểm"),
                    insurerpolicyno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Mã đơn bảo hiểm của Cty bảo hiểm gốc"),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, comment: "Trạng thái hiện tại của hồ sơ: new, inprogress, closed, cancelled"),
                    stage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Giai đoạn xử lý hiện tại"),
                    opendate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    openemployeeid = table.Column<Guid>(type: "uuid", nullable: true),
                    closedate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    closeemployeeid = table.Column<Guid>(type: "uuid", nullable: true),
                    closenote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cancelreasonid = table.Column<Guid>(type: "uuid", nullable: true),
                    canceldate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancelemployeeid = table.Column<Guid>(type: "uuid", nullable: true),
                    cancelnote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    priority = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "Mức độ ưu tiên: high, medium, low"),
                    hasadjustlocation = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, comment: "Có giám định hiện trường hay không: Y/N"),
                    ClaimId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimTypeId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    InsurerId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    OpenEmployeeId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    CloseEmployeeId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_folder", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_folder_claim_ClaimId1",
                        column: x => x.ClaimId1,
                        principalTable: "claim",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_claim_claimid",
                        column: x => x.claimid,
                        principalTable: "claim",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_hr_employee_CloseEmployeeId1",
                        column: x => x.CloseEmployeeId1,
                        principalTable: "hr_employee",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_hr_employee_OpenEmployeeId1",
                        column: x => x.OpenEmployeeId1,
                        principalTable: "hr_employee",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_hr_employee_closeemployeeid",
                        column: x => x.closeemployeeid,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_hr_employee_openemployeeid",
                        column: x => x.openemployeeid,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_res_claim_type_ClaimTypeId1",
                        column: x => x.ClaimTypeId1,
                        principalTable: "res_claim_type",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_res_claim_type_claimtypeid",
                        column: x => x.claimtypeid,
                        principalTable: "res_claim_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_res_partner_InsurerId1",
                        column: x => x.InsurerId1,
                        principalTable: "res_partner",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_res_partner_insurerid",
                        column: x => x.insurerid,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Hồ sơ bồi thường");

            migrationBuilder.CreateTable(
                name: "res_claim_plan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    paymenttype = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true, comment: "in: thu tiền về, out: thanh toán ra"),
                    isexpenses = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true, comment: "Y: có chi phí, N: không"),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_res_claim_plan", x => x.id);
                },
                comment: "Phương án bồi thường");

            migrationBuilder.CreateTable(
                name: "res_claim_stage",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claimtypeid = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    surveyplanid = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimTypeId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_res_claim_stage", x => x.id);
                    table.ForeignKey(
                        name: "FK_res_claim_stage_res_claim_type_ClaimTypeId1",
                        column: x => x.ClaimTypeId1,
                        principalTable: "res_claim_type",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_res_claim_stage_res_claim_type_claimtypeid",
                        column: x => x.claimtypeid,
                        principalTable: "res_claim_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng định nghĩa các giai đoạn xử lý bồi thường");

            migrationBuilder.CreateTable(
                name: "claim_document",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claimid = table.Column<Guid>(type: "uuid", nullable: true),
                    documentid = table.Column<Guid>(type: "uuid", nullable: true),
                    claimfolderid = table.Column<Guid>(type: "uuid", nullable: true),
                    claimfolderobjectid = table.Column<Guid>(type: "uuid", nullable: true),
                    adjustatlocationid = table.Column<Guid>(type: "uuid", nullable: true),
                    claimfolderitemid = table.Column<Guid>(type: "uuid", nullable: true),
                    quotationid = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    complete = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    iscopy = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    issue_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ClaimId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimFolderId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    AdjustAtLocationId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_document", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_document_claim_ClaimId1",
                        column: x => x.ClaimId1,
                        principalTable: "claim",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_document_claim_adjust_at_location_AdjustAtLocationId1",
                        column: x => x.AdjustAtLocationId1,
                        principalTable: "claim_adjust_at_location",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_document_claim_adjust_at_location_adjustatlocationid",
                        column: x => x.adjustatlocationid,
                        principalTable: "claim_adjust_at_location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_document_claim_claimid",
                        column: x => x.claimid,
                        principalTable: "claim",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_document_claim_folder_ClaimFolderId1",
                        column: x => x.ClaimFolderId1,
                        principalTable: "claim_folder",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_document_claim_folder_claimfolderid",
                        column: x => x.claimfolderid,
                        principalTable: "claim_folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_document_res_document_DocumentId1",
                        column: x => x.DocumentId1,
                        principalTable: "res_document",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_document_res_document_documentid",
                        column: x => x.documentid,
                        principalTable: "res_document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu tài liệu liên quan đến vụ tổn thất");

            migrationBuilder.CreateTable(
                name: "claim_folder_exposure",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claimfolderid = table.Column<Guid>(type: "uuid", nullable: false),
                    claimfolderincidentobjectid = table.Column<Guid>(type: "uuid", nullable: false),
                    coveragecode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    coverageparentcode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    insurercoveragecode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    estimateamount = table.Column<decimal>(type: "numeric", nullable: true),
                    repairdiscount = table.Column<decimal>(type: "numeric", nullable: true),
                    repairdiscountpercent = table.Column<decimal>(type: "numeric", nullable: true),
                    mechanismindemnifyreasonid = table.Column<Guid>(type: "uuid", nullable: true),
                    mechanismindemnifyamount = table.Column<decimal>(type: "numeric", nullable: true),
                    mechanismindemnifypercent = table.Column<decimal>(type: "numeric", nullable: true),
                    claimamount = table.Column<decimal>(type: "numeric", nullable: true),
                    deductibleamount = table.Column<decimal>(type: "numeric", nullable: true),
                    deductibletaxid = table.Column<decimal>(type: "numeric", nullable: true),
                    liabilityamount = table.Column<decimal>(type: "numeric", nullable: true),
                    limitliabilityamount = table.Column<decimal>(type: "numeric", nullable: true),
                    expenseamount = table.Column<decimal>(type: "numeric", nullable: true),
                    depreciationamount = table.Column<decimal>(type: "numeric", nullable: true),
                    losslimitamount = table.Column<decimal>(type: "numeric", nullable: true),
                    ClaimFolderId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_folder_exposure", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_folder_exposure_claim_folder_ClaimFolderId1",
                        column: x => x.ClaimFolderId1,
                        principalTable: "claim_folder",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_exposure_claim_folder_claimfolderid",
                        column: x => x.claimfolderid,
                        principalTable: "claim_folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lưu các phạm vi bồi thường");

            migrationBuilder.CreateTable(
                name: "claim_folder_exposure_estimate",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claimfolderid = table.Column<Guid>(type: "uuid", nullable: true),
                    feeitemid = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    ClaimFolderId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    FeeItemId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_folder_exposure_estimate", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_folder_exposure_estimate_claim_folder_ClaimFolderId1",
                        column: x => x.ClaimFolderId1,
                        principalTable: "claim_folder",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_exposure_estimate_claim_folder_claimfolderid",
                        column: x => x.claimfolderid,
                        principalTable: "claim_folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_exposure_estimate_res_fee_item_FeeItemId1",
                        column: x => x.FeeItemId1,
                        principalTable: "res_fee_item",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_exposure_estimate_res_fee_item_feeitemid",
                        column: x => x.feeitemid,
                        principalTable: "res_fee_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng ước bồi thường");

            migrationBuilder.CreateTable(
                name: "claim_folder_incident_object",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claimfolderid = table.Column<Guid>(type: "uuid", nullable: false),
                    objecttypeid = table.Column<Guid>(type: "uuid", nullable: false),
                    carplate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    carenginenumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    carvin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    idno = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    passportno = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    exitdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    profileno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClaimFolderId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ObjectTypeId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_folder_incident_object", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_folder_incident_object_claim_folder_ClaimFolderId1",
                        column: x => x.ClaimFolderId1,
                        principalTable: "claim_folder",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_incident_object_claim_folder_claimfolderid",
                        column: x => x.claimfolderid,
                        principalTable: "claim_folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_incident_object_res_object_type_ObjectTypeId1",
                        column: x => x.ObjectTypeId1,
                        principalTable: "res_object_type",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_incident_object_res_object_type_objecttypeid",
                        column: x => x.objecttypeid,
                        principalTable: "res_object_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Đối tượng tổn thất");

            migrationBuilder.CreateTable(
                name: "claim_sla",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    insurerid = table.Column<Guid>(type: "uuid", nullable: false),
                    claimstageid = table.Column<Guid>(type: "uuid", nullable: false),
                    taskid = table.Column<Guid>(type: "uuid", nullable: true),
                    slatime = table.Column<int>(type: "integer", nullable: false),
                    effectdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expiredate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    InsurerId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimStageId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    TaskId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_sla", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_sla_res_claim_stage_ClaimStageId1",
                        column: x => x.ClaimStageId1,
                        principalTable: "res_claim_stage",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_sla_res_claim_stage_claimstageid",
                        column: x => x.claimstageid,
                        principalTable: "res_claim_stage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_sla_res_partner_InsurerId1",
                        column: x => x.InsurerId1,
                        principalTable: "res_partner",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_sla_res_partner_insurerid",
                        column: x => x.insurerid,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_sla_res_task_category_TaskId1",
                        column: x => x.TaskId1,
                        principalTable: "res_task_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_sla_res_task_category_taskid",
                        column: x => x.taskid,
                        principalTable: "res_task_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Định nghĩa SLA cho quá trình claim");

            migrationBuilder.CreateTable(
                name: "claim_stage",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claimid = table.Column<Guid>(type: "uuid", nullable: false),
                    claimfolderid = table.Column<Guid>(type: "uuid", nullable: true),
                    partnerid = table.Column<Guid>(type: "uuid", nullable: true),
                    stageid = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    startdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    duedate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    enddate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ClaimId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimFolderId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    StageId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    PartnerId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_stage", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_stage_claim_ClaimId1",
                        column: x => x.ClaimId1,
                        principalTable: "claim",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_stage_claim_claimid",
                        column: x => x.claimid,
                        principalTable: "claim",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_stage_claim_folder_ClaimFolderId1",
                        column: x => x.ClaimFolderId1,
                        principalTable: "claim_folder",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_stage_claim_folder_claimfolderid",
                        column: x => x.claimfolderid,
                        principalTable: "claim_folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_stage_res_claim_stage_StageId1",
                        column: x => x.StageId1,
                        principalTable: "res_claim_stage",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_stage_res_claim_stage_stageid",
                        column: x => x.stageid,
                        principalTable: "res_claim_stage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_stage_res_partner_PartnerId1",
                        column: x => x.PartnerId1,
                        principalTable: "res_partner",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_stage_res_partner_partnerid",
                        column: x => x.partnerid,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Các giai đoạn xử lý bồi thường");

            migrationBuilder.CreateTable(
                name: "res_claim_stage_task",
                columns: table => new
                {
                    claimstageid = table.Column<Guid>(type: "uuid", nullable: false),
                    taskcategoryid = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimStageId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    TaskCategoryId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_res_claim_stage_task", x => new { x.claimstageid, x.taskcategoryid });
                    table.ForeignKey(
                        name: "FK_res_claim_stage_task_res_claim_stage_ClaimStageId1",
                        column: x => x.ClaimStageId1,
                        principalTable: "res_claim_stage",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_res_claim_stage_task_res_claim_stage_claimstageid",
                        column: x => x.claimstageid,
                        principalTable: "res_claim_stage",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_res_claim_stage_task_res_task_category_TaskCategoryId1",
                        column: x => x.TaskCategoryId1,
                        principalTable: "res_task_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_res_claim_stage_task_res_task_category_taskcategoryid",
                        column: x => x.taskcategoryid,
                        principalTable: "res_task_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "claim_folder_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claimfolderid = table.Column<Guid>(type: "uuid", nullable: false),
                    claimfolderincidentobjectid = table.Column<Guid>(type: "uuid", nullable: false),
                    claimfolderexposureid = table.Column<Guid>(type: "uuid", nullable: false),
                    itemid = table.Column<Guid>(type: "uuid", nullable: false),
                    riskid = table.Column<Guid>(type: "uuid", nullable: true),
                    claimplanid = table.Column<Guid>(type: "uuid", nullable: true),
                    demagelevelid = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    uomid = table.Column<Guid>(type: "uuid", nullable: true),
                    issuedate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lossvalue = table.Column<decimal>(type: "numeric", nullable: true),
                    isrecovery = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    iscover = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    depreciationpercent = table.Column<double>(type: "double precision", nullable: true),
                    isgenuien = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    position = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    coveragepercent = table.Column<double>(type: "double precision", nullable: true),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ClaimFolderId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimFolderIncidentObjectId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimFolderExposureId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    RiskId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    DemageLevelId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    UomId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_folder_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_folder_item_claim_folder_ClaimFolderId1",
                        column: x => x.ClaimFolderId1,
                        principalTable: "claim_folder",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_item_claim_folder_claimfolderid",
                        column: x => x.claimfolderid,
                        principalTable: "claim_folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_item_claim_folder_exposure_ClaimFolderExposure~",
                        column: x => x.ClaimFolderExposureId1,
                        principalTable: "claim_folder_exposure",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_item_claim_folder_exposure_claimfolderexposure~",
                        column: x => x.claimfolderexposureid,
                        principalTable: "claim_folder_exposure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_item_claim_folder_incident_object_ClaimFolderI~",
                        column: x => x.ClaimFolderIncidentObjectId1,
                        principalTable: "claim_folder_incident_object",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_item_claim_folder_incident_object_claimfolderi~",
                        column: x => x.claimfolderincidentobjectid,
                        principalTable: "claim_folder_incident_object",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_item_res_damage_level_DemageLevelId1",
                        column: x => x.DemageLevelId1,
                        principalTable: "res_damage_level",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_item_res_damage_level_demagelevelid",
                        column: x => x.demagelevelid,
                        principalTable: "res_damage_level",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_item_res_object_type_item_ItemId1",
                        column: x => x.ItemId1,
                        principalTable: "res_object_type_item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_claim_folder_item_res_object_type_item_itemid",
                        column: x => x.itemid,
                        principalTable: "res_object_type_item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_item_res_risk_RiskId1",
                        column: x => x.RiskId1,
                        principalTable: "res_risk",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_item_res_risk_riskid",
                        column: x => x.riskid,
                        principalTable: "res_risk",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_item_res_uom_UomId1",
                        column: x => x.UomId1,
                        principalTable: "res_uom",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_item_res_uom_uomid",
                        column: x => x.uomid,
                        principalTable: "res_uom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Các hạng mục tổn thất (khi giám định chi tiết)");

            migrationBuilder.CreateTable(
                name: "claim_folder_quotation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claimfolderid = table.Column<Guid>(type: "uuid", nullable: false),
                    claimfolderincidentobjectid = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    partnerid = table.Column<Guid>(type: "uuid", nullable: false),
                    quotationdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    amounttotal = table.Column<decimal>(type: "numeric", nullable: false),
                    isaccept = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ClaimFolderId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    PartnerId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_folder_quotation", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_folder_quotation_claim_folder_ClaimFolderId1",
                        column: x => x.ClaimFolderId1,
                        principalTable: "claim_folder",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_quotation_claim_folder_claimfolderid",
                        column: x => x.claimfolderid,
                        principalTable: "claim_folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_quotation_claim_folder_incident_object_claimfo~",
                        column: x => x.claimfolderincidentobjectid,
                        principalTable: "claim_folder_incident_object",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_quotation_res_partner_PartnerId1",
                        column: x => x.PartnerId1,
                        principalTable: "res_partner",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_quotation_res_partner_partnerid",
                        column: x => x.partnerid,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Báo giá của đối tác");

            migrationBuilder.CreateTable(
                name: "claim_folder_item_plan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claimfolderitemid = table.Column<Guid>(type: "uuid", nullable: false),
                    claimplanid = table.Column<Guid>(type: "uuid", nullable: false),
                    partnerid = table.Column<Guid>(type: "uuid", nullable: true),
                    adjustorid = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<decimal>(type: "numeric", nullable: true),
                    predictamountmin = table.Column<decimal>(type: "numeric", nullable: true),
                    predictamountmax = table.Column<decimal>(type: "numeric", nullable: true),
                    predictamountmedium = table.Column<decimal>(type: "numeric", nullable: true),
                    iswarning = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    ClaimFolderItemId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimPlanId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    PartnerId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    AdjustorId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_folder_item_plan", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_folder_item_plan_claim_folder_item_ClaimFolderItemId1",
                        column: x => x.ClaimFolderItemId1,
                        principalTable: "claim_folder_item",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_item_plan_claim_folder_item_claimfolderitemid",
                        column: x => x.claimfolderitemid,
                        principalTable: "claim_folder_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_item_plan_hr_employee_AdjustorId1",
                        column: x => x.AdjustorId1,
                        principalTable: "hr_employee",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_item_plan_hr_employee_adjustorid",
                        column: x => x.adjustorid,
                        principalTable: "hr_employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_item_plan_res_claim_plan_ClaimPlanId1",
                        column: x => x.ClaimPlanId1,
                        principalTable: "res_claim_plan",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_item_plan_res_claim_plan_claimplanid",
                        column: x => x.claimplanid,
                        principalTable: "res_claim_plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claim_folder_item_plan_res_partner_PartnerId1",
                        column: x => x.PartnerId1,
                        principalTable: "res_partner",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_claim_folder_item_plan_res_partner_partnerid",
                        column: x => x.partnerid,
                        principalTable: "res_partner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Phương án bồi thường chi tiết cho hạng mục tổn thất");

            migrationBuilder.CreateTable(
                name: "claim_folder_quotation_detail",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quotationid = table.Column<Guid>(type: "uuid", nullable: true),
                    itemname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    plan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    amounttotal = table.Column<decimal>(type: "numeric", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    creationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creatorid = table.Column<Guid>(type: "uuid", nullable: true),
                    lastmodificationtime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lastmodifierid = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_folder_quotation_detail", x => x.id);
                    table.ForeignKey(
                        name: "FK_claim_folder_quotation_detail_claim_folder_quotation_quotat~",
                        column: x => x.quotationid,
                        principalTable: "claim_folder_quotation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Chi tiết các hạng mục báo giá");

            migrationBuilder.CreateIndex(
                name: "IX_claim_adjust_at_location_adjustorid",
                table: "claim_adjust_at_location",
                column: "adjustorid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_adjust_at_location_AdjustorId1",
                table: "claim_adjust_at_location",
                column: "AdjustorId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_adjust_at_location_claimid",
                table: "claim_adjust_at_location",
                column: "claimid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_adjust_at_location_ClaimId1",
                table: "claim_adjust_at_location",
                column: "ClaimId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_adjust_at_location_garageid",
                table: "claim_adjust_at_location",
                column: "garageid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_adjust_at_location_GarageId1",
                table: "claim_adjust_at_location",
                column: "GarageId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_adjustatlocationid",
                table: "claim_document",
                column: "adjustatlocationid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_AdjustAtLocationId1",
                table: "claim_document",
                column: "AdjustAtLocationId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_claimfolderid",
                table: "claim_document",
                column: "claimfolderid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_ClaimFolderId1",
                table: "claim_document",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_claimid",
                table: "claim_document",
                column: "claimid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_ClaimId1",
                table: "claim_document",
                column: "ClaimId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_documentid",
                table: "claim_document",
                column: "documentid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_document_DocumentId1",
                table: "claim_document",
                column: "DocumentId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_claimid",
                table: "claim_folder",
                column: "claimid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_ClaimId1",
                table: "claim_folder",
                column: "ClaimId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_claimtypeid",
                table: "claim_folder",
                column: "claimtypeid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_ClaimTypeId1",
                table: "claim_folder",
                column: "ClaimTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_closeemployeeid",
                table: "claim_folder",
                column: "closeemployeeid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_CloseEmployeeId1",
                table: "claim_folder",
                column: "CloseEmployeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_insurerid",
                table: "claim_folder",
                column: "insurerid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_InsurerId1",
                table: "claim_folder",
                column: "InsurerId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_openemployeeid",
                table: "claim_folder",
                column: "openemployeeid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_OpenEmployeeId1",
                table: "claim_folder",
                column: "OpenEmployeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_exposure_claimfolderid",
                table: "claim_folder_exposure",
                column: "claimfolderid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_exposure_ClaimFolderId1",
                table: "claim_folder_exposure",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_exposure_estimate_claimfolderid",
                table: "claim_folder_exposure_estimate",
                column: "claimfolderid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_exposure_estimate_ClaimFolderId1",
                table: "claim_folder_exposure_estimate",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_exposure_estimate_feeitemid",
                table: "claim_folder_exposure_estimate",
                column: "feeitemid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_exposure_estimate_FeeItemId1",
                table: "claim_folder_exposure_estimate",
                column: "FeeItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_incident_object_claimfolderid",
                table: "claim_folder_incident_object",
                column: "claimfolderid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_incident_object_ClaimFolderId1",
                table: "claim_folder_incident_object",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_incident_object_objecttypeid",
                table: "claim_folder_incident_object",
                column: "objecttypeid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_incident_object_ObjectTypeId1",
                table: "claim_folder_incident_object",
                column: "ObjectTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_claimfolderexposureid",
                table: "claim_folder_item",
                column: "claimfolderexposureid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_ClaimFolderExposureId1",
                table: "claim_folder_item",
                column: "ClaimFolderExposureId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_claimfolderid",
                table: "claim_folder_item",
                column: "claimfolderid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_ClaimFolderId1",
                table: "claim_folder_item",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_claimfolderincidentobjectid",
                table: "claim_folder_item",
                column: "claimfolderincidentobjectid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_ClaimFolderIncidentObjectId1",
                table: "claim_folder_item",
                column: "ClaimFolderIncidentObjectId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_demagelevelid",
                table: "claim_folder_item",
                column: "demagelevelid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_DemageLevelId1",
                table: "claim_folder_item",
                column: "DemageLevelId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_itemid",
                table: "claim_folder_item",
                column: "itemid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_ItemId1",
                table: "claim_folder_item",
                column: "ItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_riskid",
                table: "claim_folder_item",
                column: "riskid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_RiskId1",
                table: "claim_folder_item",
                column: "RiskId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_uomid",
                table: "claim_folder_item",
                column: "uomid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_UomId1",
                table: "claim_folder_item",
                column: "UomId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_adjustorid",
                table: "claim_folder_item_plan",
                column: "adjustorid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_AdjustorId1",
                table: "claim_folder_item_plan",
                column: "AdjustorId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_claimfolderitemid",
                table: "claim_folder_item_plan",
                column: "claimfolderitemid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_ClaimFolderItemId1",
                table: "claim_folder_item_plan",
                column: "ClaimFolderItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_claimplanid",
                table: "claim_folder_item_plan",
                column: "claimplanid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_ClaimPlanId1",
                table: "claim_folder_item_plan",
                column: "ClaimPlanId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_partnerid",
                table: "claim_folder_item_plan",
                column: "partnerid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_item_plan_PartnerId1",
                table: "claim_folder_item_plan",
                column: "PartnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_quotation_claimfolderid",
                table: "claim_folder_quotation",
                column: "claimfolderid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_quotation_ClaimFolderId1",
                table: "claim_folder_quotation",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_quotation_claimfolderincidentobjectid",
                table: "claim_folder_quotation",
                column: "claimfolderincidentobjectid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_quotation_partnerid",
                table: "claim_folder_quotation",
                column: "partnerid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_quotation_PartnerId1",
                table: "claim_folder_quotation",
                column: "PartnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_folder_quotation_detail_quotationid",
                table: "claim_folder_quotation_detail",
                column: "quotationid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_sla_claimstageid",
                table: "claim_sla",
                column: "claimstageid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_sla_ClaimStageId1",
                table: "claim_sla",
                column: "ClaimStageId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_sla_insurerid",
                table: "claim_sla",
                column: "insurerid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_sla_InsurerId1",
                table: "claim_sla",
                column: "InsurerId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_sla_taskid",
                table: "claim_sla",
                column: "taskid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_sla_TaskId1",
                table: "claim_sla",
                column: "TaskId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_claimfolderid",
                table: "claim_stage",
                column: "claimfolderid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_ClaimFolderId1",
                table: "claim_stage",
                column: "ClaimFolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_claimid",
                table: "claim_stage",
                column: "claimid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_ClaimId1",
                table: "claim_stage",
                column: "ClaimId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_partnerid",
                table: "claim_stage",
                column: "partnerid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_PartnerId1",
                table: "claim_stage",
                column: "PartnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_stageid",
                table: "claim_stage",
                column: "stageid");

            migrationBuilder.CreateIndex(
                name: "IX_claim_stage_StageId1",
                table: "claim_stage",
                column: "StageId1");

            migrationBuilder.CreateIndex(
                name: "IX_res_claim_stage_claimtypeid",
                table: "res_claim_stage",
                column: "claimtypeid");

            migrationBuilder.CreateIndex(
                name: "IX_res_claim_stage_ClaimTypeId1",
                table: "res_claim_stage",
                column: "ClaimTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_res_claim_stage_task_ClaimStageId1",
                table: "res_claim_stage_task",
                column: "ClaimStageId1");

            migrationBuilder.CreateIndex(
                name: "IX_res_claim_stage_task_taskcategoryid",
                table: "res_claim_stage_task",
                column: "taskcategoryid");

            migrationBuilder.CreateIndex(
                name: "IX_res_claim_stage_task_TaskCategoryId1",
                table: "res_claim_stage_task",
                column: "TaskCategoryId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "claim_document");

            migrationBuilder.DropTable(
                name: "claim_folder_exposure_estimate");

            migrationBuilder.DropTable(
                name: "claim_folder_item_plan");

            migrationBuilder.DropTable(
                name: "claim_folder_quotation_detail");

            migrationBuilder.DropTable(
                name: "claim_sla");

            migrationBuilder.DropTable(
                name: "claim_stage");

            migrationBuilder.DropTable(
                name: "res_claim_stage_task");

            migrationBuilder.DropTable(
                name: "claim_adjust_at_location");

            migrationBuilder.DropTable(
                name: "claim_folder_item");

            migrationBuilder.DropTable(
                name: "res_claim_plan");

            migrationBuilder.DropTable(
                name: "claim_folder_quotation");

            migrationBuilder.DropTable(
                name: "res_claim_stage");

            migrationBuilder.DropTable(
                name: "claim_folder_exposure");

            migrationBuilder.DropTable(
                name: "claim_folder_incident_object");

            migrationBuilder.DropTable(
                name: "claim_folder");
        }
    }
}
