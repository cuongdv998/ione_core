# Quy tắc Database Migration

## Mục đích
File này định nghĩa các quy tắc và best practices khi tạo database migration để đảm bảo tính nhất quán, an toàn và không lỗi.

---

## 1. Tạo Migration

### 1.1. Lệnh tạo migration

```bash
# Di chuyển đến thư mục EntityFrameworkCore
cd src/common/infra/iOne.EntityFrameworkCore

# Tạo migration mới
dotnet ef migrations add <MigrationName> --project . --startup-project ../../../../src/iOne.DbMigrator/iOne.DbMigrator.csproj

# Ví dụ:
dotnet ef migrations add AddResProvince --project . --startup-project ../../../../src/iOne.DbMigrator/iOne.DbMigrator.csproj
```

**Lưu ý:**
- Luôn chạy lệnh từ thư mục `src/common/infra/iOne.EntityFrameworkCore`
- Sử dụng tên migration mô tả rõ ràng (ví dụ: `AddResProvince`, `AddTypeToResOrganizationType`)
- Không sửa trực tiếp migration đã được commit (tạo migration mới thay vì sửa migration cũ)

---

## 2. Naming Conventions (PostgreSQL)

### 2.1. Table Names
- **Format**: `snake_case` (chữ thường, phân cách bằng dấu gạch dưới)
- **Ví dụ**: `res_province`, `res_partner`, `admin_config`
- **Quy tắc**: 
  - Tên table phải ngắn gọn, mô tả rõ ràng
  - Sử dụng prefix nếu cần (ví dụ: `res_` cho resource tables)

### 2.2. Column Names
- **Format**: `snake_case` (chữ thường, phân cách bằng dấu gạch dưới)
- **Ví dụ**: `country_id`, `full_address`, `creation_time`
- **Quy tắc**:
  - Foreign key columns: `<table_name>_id` (ví dụ: `country_id`, `province_id`)
  - Boolean columns: `is_<description>` (ví dụ: `is_deleted`)
  - Date/Time columns: `<description>_time` hoặc `<description>_date` (ví dụ: `creation_time`, `effect_date`)

### 2.3. Primary Key Names
- **Format**: `pk_<table_name>`
- **Ví dụ**: `pk_res_province`, `pk_admin_config`
- **Quy tắc**: Luôn sử dụng prefix `pk_` theo sau là tên table

### 2.4. Foreign Key Names
- **Format**: `fk_<table_name>_<referenced_table_name>_<column_name>`
- **Ví dụ**: 
  - `fk_res_province_country_id`
  - `fk_res_partner_res_province_province_id`
- **Quy tắc**: 
  - Prefix `fk_` + tên table chứa FK + tên table được tham chiếu + tên column
  - Nếu có nhiều FK đến cùng một table, thêm tên column để phân biệt

### 2.5. Index Names
- **Format**: `ix_<table_name>_<column_name(s)>`
- **Ví dụ**: 
  - `ix_res_province_code` (single column)
  - `ix_admin_config_code_sub_code` (composite index)
  - `ix_res_province_country_id` (index trên foreign key)
- **Quy tắc**:
  - Prefix `ix_` + tên table + tên column(s)
  - Với composite index, nối các column bằng dấu gạch dưới

### 2.6. Unique Index Names
- **Format**: Giống như index thông thường, nhưng có `unique: true`
- **Ví dụ**: `ix_res_province_code` với `unique: true`

---

## 3. Column Types

### 3.1. ID Columns
- **Type**: `uuid` (PostgreSQL UUID type)
- **Nullable**: `false`
- **Ví dụ**:
```csharp
id = table.Column<Guid>(type: "uuid", nullable: false)
```

### 3.2. String Columns
- **Type**: `character varying(<length>)` hoặc `text`
- **Ví dụ**:
```csharp
code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false)
description = table.Column<string>(type: "text", nullable: true) // Không giới hạn độ dài
```

### 3.3. Numeric Columns
- **Type**: `numeric(<precision>, <scale>)` hoặc `integer`, `bigint`
- **Ví dụ**:
```csharp
file_size = table.Column<decimal>(type: "numeric(20)", nullable: false)
```

### 3.4. Boolean Columns
- **Type**: `boolean`
- **Ví dụ**:
```csharp
is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
```

### 3.5. Date/Time Columns
- **Type**: `timestamp without time zone` (cho DateTime) hoặc `date` (cho Date only)
- **Ví dụ**:
```csharp
creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
effect_date = table.Column<DateTime>(type: "date", nullable: true)
```

---

## 4. Constraints

### 4.1. Primary Key
```csharp
table.PrimaryKey("pk_<table_name>", x => x.id);
```

### 4.2. Foreign Key
- **Quy tắc**: Luôn sử dụng `ReferentialAction.Restrict` (ON DELETE RESTRICT, ON UPDATE RESTRICT)
- **Ví dụ**:
```csharp
table.ForeignKey(
    name: "fk_res_province_country_id",
    column: x => x.country_id,
    principalTable: "res_country",
    principalColumn: "id",
    onDelete: ReferentialAction.Restrict);
```

### 4.3. Unique Constraint
- Sử dụng `CreateIndex` với `unique: true` thay vì `UniqueConstraint`
- **Ví dụ**:
```csharp
migrationBuilder.CreateIndex(
    name: "ix_res_province_code",
    table: "res_province",
    column: "code",
    unique: true);
```

### 4.4. Composite Unique Index
- **Ví dụ**:
```csharp
migrationBuilder.CreateIndex(
    name: "ix_admin_config_code_sub_code",
    table: "admin_config",
    columns: new[] { "code", "sub_code" },
    unique: true);
```

### 4.5. Filtered Unique Index (PostgreSQL)
- Sử dụng khi cần unique chỉ cho các record không bị xóa (soft delete)
- **Ví dụ**:
```csharp
migrationBuilder.Sql(@"
    CREATE UNIQUE INDEX IF NOT EXISTS ""ix_res_partner_code_not_deleted""
    ON ""res_partner"" (""code"")
    WHERE ""is_deleted"" = false AND ""code"" IS NOT NULL;
");
```

---

## 5. Comments

### 5.1. Table Comments
- Luôn thêm comment cho table để mô tả mục đích
- **Ví dụ**:
```csharp
comment: "Tỉnh/Thành"
```

### 5.2. Column Comments
- Thêm comment cho các column quan trọng, đặc biệt là:
  - Status columns (mô tả các giá trị có thể)
  - Enum columns (mô tả các giá trị enum)
  - Foreign key columns (nếu cần giải thích)
- **Ví dụ**:
```csharp
status = table.Column<string>(
    type: "character varying(10)", 
    maxLength: 10, 
    nullable: false, 
    comment: "Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động")
```

---

## 6. Safe Migration Practices

### 6.1. IF EXISTS / IF NOT EXISTS
- **Luôn sử dụng** `IF EXISTS` khi DROP trong method `Down()`
- **Luôn sử dụng** `IF NOT EXISTS` khi CREATE trong method `Up()` (nếu dùng raw SQL)
- **Ví dụ**:
```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_province_code"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_province"";");
}
```

### 6.2. Drop Order trong Down()
- Drop theo thứ tự ngược lại với thứ tự tạo:
  1. Drop indexes trước
  2. Drop foreign keys (thường tự động khi drop table)
  3. Drop tables (drop child tables trước, parent tables sau)
- **Ví dụ**:
```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    // Drop indexes
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_partner_agreement_partner_id"";");
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_partner_code"";");
    
    // Drop child tables trước
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_partner_agreement"";");
    
    // Drop parent tables sau
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_partner"";");
}
```

### 6.3. Default Values
- Luôn đặt default value cho các column có yêu cầu
- **Ví dụ**:
```csharp
is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "active")
```

### 6.4. Adding Columns to Existing Tables
- Khi thêm column vào table đã tồn tại, luôn đặt default value nếu column là `NOT NULL`
- **Ví dụ**:
```csharp
migrationBuilder.AddColumn<string>(
    name: "type",
    table: "res_organization_type",
    type: "character varying(15)",
    maxLength: 15,
    nullable: false,
    defaultValue: "TC",
    comment: "Loại tổ chức:\n- TC: Tổ chức\n- CN: Cá nhân");
```

---

## 7. ABP Framework Specific

### 7.1. Audit Fields
- Các entity kế thừa từ `FullAuditedAggregateRoot<Guid>` sẽ tự động có các field:
  - `creation_time` (DateTime, NOT NULL)
  - `creator_id` (Guid, nullable)
  - `last_modification_time` (DateTime, nullable)
  - `last_modifier_id` (Guid, nullable)
  - `is_deleted` (bool, NOT NULL, default: false)
  - `deleter_id` (Guid, nullable)
  - `deletion_time` (DateTime, nullable)
  - `extra_properties` (string, NOT NULL)
  - `concurrency_stamp` (string, nullable hoặc NOT NULL tùy entity)

- Các entity kế thừa từ `AuditedEntity<Guid>` sẽ có:
  - `creation_time`, `creator_id`, `last_modification_time`, `last_modifier_id`
  - **KHÔNG có** `is_deleted`, `deleter_id`, `deletion_time`
  - **KHÔNG có** `extra_properties`, `concurrency_stamp` (trừ khi entity tự định nghĩa)

### 7.2. Soft Delete
- Chỉ các entity kế thừa từ `FullAuditedAggregateRoot` mới có soft delete
- Entity kế thừa từ `AuditedEntity` sẽ bị hard delete
- Khi xóa, ABP sẽ tự động set `is_deleted = true` và `deletion_time = DateTime.UtcNow`

---

## 8. Indexes

### 8.1. Foreign Key Indexes
- **Luôn tạo index** trên foreign key columns để tối ưu query performance
- **Ví dụ**:
```csharp
migrationBuilder.CreateIndex(
    name: "ix_res_province_country_id",
    table: "res_province",
    column: "country_id");
```

### 8.2. Unique Indexes
- Tạo unique index cho các column/cột cần đảm bảo tính duy nhất
- **Ví dụ**:
```csharp
migrationBuilder.CreateIndex(
    name: "ix_res_province_code",
    table: "res_province",
    column: "code",
    unique: true);
```

### 8.3. Composite Indexes
- Tạo composite index khi cần unique hoặc tối ưu query trên nhiều columns
- **Ví dụ**:
```csharp
migrationBuilder.CreateIndex(
    name: "ix_admin_config_code_sub_code",
    table: "admin_config",
    columns: new[] { "code", "sub_code" },
    unique: true);
```

---

## 9. Status Fields

### 9.1. Status Column Type
- Sử dụng `character varying(10)` cho status columns
- Lưu giá trị dưới dạng **lowercase string** (`"active"`, `"deactive"`)
- **KHÔNG** sử dụng PascalCase (`"Active"`, `"Deactive"`)

### 9.2. Status Values
- `"active"`: Hoạt động
- `"deactive"`: Không hoạt động
- Default value: `"active"`

### 9.3. Status Comment
- Luôn thêm comment mô tả các giá trị có thể
- **Ví dụ**:
```csharp
status = table.Column<string>(
    type: "character varying(10)", 
    maxLength: 10, 
    nullable: false, 
    comment: "Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động")
```

---

## 10. Migration File Structure

### 10.1. Up() Method
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Drop existing objects nếu cần (với IF EXISTS)
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""table_name"";");
    
    // 2. Create tables
    migrationBuilder.CreateTable(...);
    
    // 3. Create indexes
    migrationBuilder.CreateIndex(...);
    
    // 4. Create foreign keys (thường tự động trong CreateTable, nhưng có thể tạo riêng nếu cần)
}
```

### 10.2. Down() Method
```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    // 1. Drop indexes (với IF EXISTS)
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_table_name_column"";");
    
    // 2. Drop tables (với IF EXISTS, child tables trước)
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""child_table"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""parent_table"";");
}
```

---

## 11. Checklist Trước Khi Commit Migration

- [ ] Tất cả tên table, column, index, constraint đều theo `snake_case`
- [ ] Primary key name: `pk_<table_name>`
- [ ] Foreign key name: `fk_<table_name>_<referenced_table>_<column>`
- [ ] Index name: `ix_<table_name>_<column(s)>`
- [ ] Tất cả foreign keys sử dụng `ReferentialAction.Restrict`
- [ ] Tất cả ID columns là `uuid` type
- [ ] Status columns lưu giá trị lowercase (`"active"`, `"deactive"`)
- [ ] Đã thêm comment cho table và các column quan trọng
- [ ] Method `Down()` sử dụng `IF EXISTS` khi DROP
- [ ] Đã tạo index trên tất cả foreign key columns
- [ ] Đã tạo unique index cho các column cần unique
- [ ] Default values đã được đặt cho các column NOT NULL
- [ ] Migration đã được test trên local database
- [ ] Không có lỗi khi chạy `dotnet build`

---

## 12. Common Mistakes to Avoid

### ❌ SAI
```csharp
// SAI: Tên table không theo snake_case
name: "ResProvince"

// SAI: Tên column không theo snake_case
table.Column<string>(name: "CountryId", ...)

// SAI: Primary key name không có prefix
table.PrimaryKey("ResProvince_PK", x => x.id)

// SAI: Foreign key không có prefix và không mô tả rõ
table.ForeignKey(name: "FK_Country", ...)

// SAI: Status lưu PascalCase
status = "Active"

// SAI: Không có IF EXISTS trong Down()
migrationBuilder.Sql(@"DROP TABLE ""res_province"";");

// SAI: Không tạo index trên foreign key
// (thiếu index trên country_id)
```

### ✅ ĐÚNG
```csharp
// ĐÚNG: Tên table theo snake_case
name: "res_province"

// ĐÚNG: Tên column theo snake_case
table.Column<string>(name: "country_id", ...)

// ĐÚNG: Primary key name có prefix
table.PrimaryKey("pk_res_province", x => x.id)

// ĐÚNG: Foreign key có prefix và mô tả rõ
table.ForeignKey(
    name: "fk_res_province_country_id",
    ...)

// ĐÚNG: Status lưu lowercase
status = "active"

// ĐÚNG: Có IF EXISTS trong Down()
migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_province"";");

// ĐÚNG: Có index trên foreign key
migrationBuilder.CreateIndex(
    name: "ix_res_province_country_id",
    table: "res_province",
    column: "country_id");
```

---

## 13. Resources

- [PostgreSQL Naming Conventions](https://www.postgresql.org/docs/current/sql-syntax-lexical.html#SQL-SYNTAX-IDENTIFIERS)
- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [ABP Framework Documentation](https://docs.abp.io/)

---

**Lưu ý cuối cùng**: Luôn review migration file trước khi commit và đảm bảo tuân thủ tất cả các quy tắc trên!

