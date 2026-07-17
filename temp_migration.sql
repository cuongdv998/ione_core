START TRANSACTION;
ALTER TABLE res_document_type ADD document_group_code character varying(50);
COMMENT ON COLUMN res_document_type.document_group_code IS 'Mã nhóm tài liệu (tùy chọn)';

CREATE TABLE claim_folde_revaluate (
    id uuid NOT NULL,
    claimfolderid uuid NOT NULL,
    employeeid uuid NOT NULL,
    result character varying(1) NOT NULL,
    description character varying(250),
    "ExtraProperties" text NOT NULL,
    "ConcurrencyStamp" character varying(40) NOT NULL,
    creationtime timestamp without time zone NOT NULL,
    creatorid uuid,
    lastmodificationtime timestamp without time zone,
    lastmodifierid uuid,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "DeleterId" uuid,
    "DeletionTime" timestamp without time zone,
    CONSTRAINT "PK_claim_folde_revaluate" PRIMARY KEY (id),
    CONSTRAINT "FK_claim_folde_revaluate_claim_folder_claimfolderid" FOREIGN KEY (claimfolderid) REFERENCES claim_folder (id) ON DELETE RESTRICT
);

CREATE TABLE res_claim_evaluate_item (
    id uuid NOT NULL,
    claimtypeid uuid NOT NULL,
    code character varying(25) NOT NULL,
    name character varying(250) NOT NULL,
    status character varying(10) NOT NULL,
    "ExtraProperties" text NOT NULL,
    "ConcurrencyStamp" character varying(40) NOT NULL,
    creationtime timestamp without time zone NOT NULL,
    creatorid uuid,
    lastmodificationtime timestamp without time zone,
    lastmodifierid uuid,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "DeleterId" uuid,
    "DeletionTime" timestamp without time zone,
    CONSTRAINT "PK_res_claim_evaluate_item" PRIMARY KEY (id)
);

CREATE TABLE claim_folde_revaluate_detail (
    id uuid NOT NULL,
    claimfolderevaluateid uuid NOT NULL,
    evaluateitemid uuid NOT NULL,
    result character varying(1) NOT NULL,
    description character varying(250),
    "ExtraProperties" text NOT NULL,
    "ConcurrencyStamp" character varying(40) NOT NULL,
    creationtime timestamp without time zone NOT NULL,
    creatorid uuid,
    lastmodificationtime timestamp without time zone,
    lastmodifierid uuid,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "DeleterId" uuid,
    "DeletionTime" timestamp without time zone,
    CONSTRAINT "PK_claim_folde_revaluate_detail" PRIMARY KEY (id),
    CONSTRAINT "FK_claim_folde_revaluate_detail_claim_folde_revaluate_claimfol~" FOREIGN KEY (claimfolderevaluateid) REFERENCES claim_folde_revaluate (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_claim_folde_revaluate_detail_res_claim_evaluate_item_evalua~" FOREIGN KEY (evaluateitemid) REFERENCES res_claim_evaluate_item (id) ON DELETE RESTRICT
);

CREATE INDEX "IX_claim_folde_revaluate_claimfolderid" ON claim_folde_revaluate (claimfolderid);

CREATE INDEX "IX_claim_folde_revaluate_detail_claimfolderevaluateid" ON claim_folde_revaluate_detail (claimfolderevaluateid);

CREATE INDEX "IX_claim_folde_revaluate_detail_evaluateitemid" ON claim_folde_revaluate_detail (evaluateitemid);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260405150402_AddClaimFolderEvaluateTables', '9.0.5');

COMMIT;

