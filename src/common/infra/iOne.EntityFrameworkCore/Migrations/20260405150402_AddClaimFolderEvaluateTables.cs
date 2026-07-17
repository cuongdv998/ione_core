using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimFolderEvaluateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "document_group_code",
                table: "res_document_type",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Mã nhóm tài liệu (tùy chọn)");

            // Bảng đã có trên một số DB (thiếu snapshot / migration history) — tạo nếu chưa tồn tại.
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS res_claim_evaluate_item (
    id uuid NOT NULL,
    claimtypeid uuid NOT NULL,
    code character varying(25) NOT NULL,
    name character varying(250) NOT NULL,
    status character varying(10) NOT NULL,
    ""ExtraProperties"" text NOT NULL,
    ""ConcurrencyStamp"" character varying(40) NOT NULL,
    creationtime timestamp without time zone NOT NULL,
    creatorid uuid,
    lastmodificationtime timestamp without time zone,
    lastmodifierid uuid,
    ""IsDeleted"" boolean NOT NULL DEFAULT FALSE,
    ""DeleterId"" uuid,
    ""DeletionTime"" timestamp without time zone,
    CONSTRAINT ""PK_res_claim_evaluate_item"" PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS claim_folde_revaluate (
    id uuid NOT NULL,
    claimfolderid uuid NOT NULL,
    employeeid uuid NOT NULL,
    result character varying(1) NOT NULL,
    description character varying(250),
    ""ExtraProperties"" text NOT NULL,
    ""ConcurrencyStamp"" character varying(40) NOT NULL,
    creationtime timestamp without time zone NOT NULL,
    creatorid uuid,
    lastmodificationtime timestamp without time zone,
    lastmodifierid uuid,
    ""IsDeleted"" boolean NOT NULL DEFAULT FALSE,
    ""DeleterId"" uuid,
    ""DeletionTime"" timestamp without time zone,
    CONSTRAINT ""PK_claim_folde_revaluate"" PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS claim_folde_revaluate_detail (
    id uuid NOT NULL,
    claimfolderevaluateid uuid NOT NULL,
    evaluateitemid uuid NOT NULL,
    result character varying(1) NOT NULL,
    description character varying(250),
    ""ExtraProperties"" text NOT NULL,
    ""ConcurrencyStamp"" character varying(40) NOT NULL,
    creationtime timestamp without time zone NOT NULL,
    creatorid uuid,
    lastmodificationtime timestamp without time zone,
    lastmodifierid uuid,
    ""IsDeleted"" boolean NOT NULL DEFAULT FALSE,
    ""DeleterId"" uuid,
    ""DeletionTime"" timestamp without time zone,
    CONSTRAINT ""PK_claim_folde_revaluate_detail"" PRIMARY KEY (id)
);

DO $EF$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint c
    JOIN pg_class t ON c.conrelid = t.oid
    WHERE t.relname = 'claim_folde_revaluate' AND c.contype = 'f'
      AND pg_get_constraintdef(c.oid) LIKE '%claim_folder%'
  ) THEN
    ALTER TABLE claim_folde_revaluate
      ADD CONSTRAINT ""FK_claim_folde_revaluate_claim_folder_claimfolderid""
      FOREIGN KEY (claimfolderid) REFERENCES claim_folder (id);
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint c
    JOIN pg_class t ON c.conrelid = t.oid
    WHERE t.relname = 'claim_folde_revaluate_detail' AND c.contype = 'f'
      AND pg_get_constraintdef(c.oid) LIKE '%claim_folde_revaluate%'
  ) THEN
    ALTER TABLE claim_folde_revaluate_detail
      ADD CONSTRAINT ""FK_claim_folde_revaluate_detail_claim_folde_revaluate_claimfol~""
      FOREIGN KEY (claimfolderevaluateid) REFERENCES claim_folde_revaluate (id);
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint c
    JOIN pg_class t ON c.conrelid = t.oid
    WHERE t.relname = 'claim_folde_revaluate_detail' AND c.contype = 'f'
      AND pg_get_constraintdef(c.oid) LIKE '%res_claim_evaluate_item%'
  ) THEN
    ALTER TABLE claim_folde_revaluate_detail
      ADD CONSTRAINT ""FK_claim_folde_revaluate_detail_res_claim_evaluate_item_evalua~""
      FOREIGN KEY (evaluateitemid) REFERENCES res_claim_evaluate_item (id);
  END IF;
END $EF$;

CREATE INDEX IF NOT EXISTS ""IX_claim_folde_revaluate_claimfolderid"" ON claim_folde_revaluate (claimfolderid);
CREATE INDEX IF NOT EXISTS ""IX_claim_folde_revaluate_detail_claimfolderevaluateid"" ON claim_folde_revaluate_detail (claimfolderevaluateid);
CREATE INDEX IF NOT EXISTS ""IX_claim_folde_revaluate_detail_evaluateitemid"" ON claim_folde_revaluate_detail (evaluateitemid);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS claim_folde_revaluate_detail;
DROP TABLE IF EXISTS claim_folde_revaluate;
DROP TABLE IF EXISTS res_claim_evaluate_item;
");

            migrationBuilder.DropColumn(
                name: "document_group_code",
                table: "res_document_type");
        }
    }
}
