# Plan Xây Dựng Backend: Danh mục Ngành nghề (ResIndustry)

## 📋 Tổng Quan

**Module**: Customer (Quản lý khách hàng)  
**Entity**: ResIndustry (Ngành nghề)  
**Table Name**: `res_industry` (snake_case - PostgreSQL convention)  
**DDL Table Name**: `RESINDUSTRY` (sẽ convert sang `res_industry`)

---

## 🎯 Yêu Cầu Chức Năng

1. ✅ **CRUD Operations**: Xem, Thêm, Sửa, Xóa
2. ✅ **Validation Code**: Mã ngành nghề chỉ cho phép A-Z, _, 0-9 (uppercase)
3. ✅ **Code Immutable**: Khi sửa không được phép sửa mã ngành nghề (disable trên UI)
4. ✅ **Code Unique**: Mã ngành nghề là duy nhất
5. ✅ **Soft Delete**: Khi xóa → soft delete (ABP audit log) → cập nhật status về Deactive
6. ✅ **Đa ngôn ngữ**: vi-VN và en
7. ✅ **Phân quyền**: View, Create, Edit, Delete

---

## 📁 Cấu Trúc Files Cần Tạo

### 1. Domain Layer

#### 1.1. Enum Status
**Location**: `src/common/domain/iOne.Domain.Shared/ResIndustries/ResIndustryStatus.cs`

**Yêu cầu**:
```csharp
namespace iOne.ResIndustries;

public enum ResIndustryStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

**Lưu ý**: 
- Enum values sẽ được convert sang string trong database: "active", "deactive"
- MaxLength(15) trong EF Core configuration (theo DDL: VARCHAR(15))

---

#### 1.2. Entity: ResIndustry
**Location**: `src/common/domain/iOne.Domain/ResIndustries/ResIndustry.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>`
- Table name: `[Table("res_industry")]` (snake_case)
- Properties:
  - `Code` (string, max 50, required, private setter, immutable)
  - `Name` (string, max 250, required, private setter)
  - `Description` (string, max 500, optional, private setter)
  - `Status` (ResIndustryStatus enum, required, private setter)
- Constructor với validation
- Private setters với methods: `SetCode()`, `SetName()`, `SetDescription()`, `SetStatus()`
- Public methods: `UpdateName()`, `UpdateDescription()`, `UpdateStatus()` (KHÔNG có `UpdateCode()`)
- Validation Code: Regex `^[A-Z0-9_]+$` (uppercase)

**Lưu ý**:
- DDL có `CREATIONTIME`, `CREATORID`, `LASTMODIFICATIONTIME`, `LASTMODIFIERID` → ABP FullAuditedAggregateRoot đã có sẵn
- DDL không có `DELETIONTIME`, `DELETERID`, `ISDELETED` → nhưng FullAuditedAggregateRoot có soft delete, nên sẽ có trong database

---

#### 1.3. Repository Interface: IResIndustryRepository
**Location**: `src/common/domain/iOne.Domain/ResIndustries/IResIndustryRepository.cs`

**Yêu cầu**:
- Kế thừa từ `IRepository<ResIndustry, Guid>`
- Methods:
  - `IsCodeExistsAsync(string code, Guid? excludeId = null)`: Check code uniqueness

---

#### 1.4. Manager: ResIndustryManager
**Location**: `src/common/domain/iOne.Domain/ResIndustries/ResIndustryManager.cs`

**Yêu cầu**:
- Inject: `IResIndustryRepository`
- Methods:
  - `CreateAsync(ResIndustry industry)`: Validate code uniqueness
  - `UpdateAsync(ResIndustry industry, string name, string description, ResIndustryStatus status)`: Validate (KHÔNG có UpdateCode)
  - `DeleteAsync(ResIndustry industry)`: Soft delete → set status = Deactive

---

### 2. Entity Framework Core

#### 2.1. Configuration: ResIndustryConfiguration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResIndustries/ResIndustryConfiguration.cs`

**Yêu cầu**:
- Table name: `"res_industry"` (snake_case)
- Column names: snake_case (id, code, name, description, status)
- Audit columns: snake_case (creation_time, creator_id, last_modification_time, last_modifier_id, deletion_time, deleter_id, is_deleted, concurrency_stamp, tenant_id)
- Indexes:
  - `ix_res_industry_code` (unique)
- Enum conversion: Status → string (lowercase) với MaxLength(15)

**Lưu ý**:
- DDL có `CREATIONTIME DATE` nhưng ABP FullAuditedAggregateRoot dùng `DateTime` (TIMESTAMP), nên sẽ dùng TIMESTAMP trong migration
- DDL có `CREATORID CHAR(36)` và `LASTMODIFIERID CHAR(36)` → ABP dùng `Guid?`

---

#### 2.2. Repository Implementation: EfCoreResIndustryRepository
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResIndustries/EfCoreResIndustryRepository.cs`

**Yêu cầu**:
- Kế thừa từ `EfCoreRepository<iOneDbContext, ResIndustry, Guid>`
- Implement `IResIndustryRepository`
- Implement custom methods

---

#### 2.3. Register trong DbContext
**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`

**Yêu cầu**:
- Thêm `DbSet<ResIndustry> Industries { get; set; }`

---

#### 2.4. Register trong Module
**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneEntityFrameworkCoreModule.cs`

**Yêu cầu**:
- Đăng ký repository implementation

---

### 3. Application Layer

#### 3.1. DTOs: ResIndustryDto
**Location**: `modules/customer/src/iOne.Customer.Application.Contracts/ResIndustries/ResIndustryDto.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedEntityDto<Guid>`
- Properties: Id, Code, Name, Description, Status, CreationTime, CreatorId, LastModificationTime, LastModifierId

---

#### 3.2. DTOs: CreateResIndustryDto
**Location**: `modules/customer/src/iOne.Customer.Application.Contracts/ResIndustries/CreateResIndustryDto.cs`

**Yêu cầu**:
- Properties: Code (required, max 50), Name (required, max 250), Description (optional, max 500), Status (required)
- Validation attributes: `[Required]`, `[MaxLength]`, etc.

---

#### 3.3. DTOs: UpdateResIndustryDto
**Location**: `modules/customer/src/iOne.Customer.Application.Contracts/ResIndustries/UpdateResIndustryDto.cs`

**Yêu cầu**:
- Properties: Name (required, max 250), Description (optional, max 500), Status (required)
- **KHÔNG có Code** (immutable)
- Validation attributes

---

#### 3.4. DTOs: GetResIndustriesInput
**Location**: `modules/customer/src/iOne.Customer.Application.Contracts/ResIndustries/GetResIndustriesInput.cs`

**Yêu cầu**:
- Kế thừa từ `PagedAndSortedResultRequestDto`
- Properties:
  - `Code` (string?, optional)
  - `Name` (string?, optional)
  - `Status` (ResIndustryStatus?, optional)

---

#### 3.5. Application Service Interface: IResIndustryAppService
**Location**: `modules/customer/src/iOne.Customer.Application.Contracts/ResIndustries/IResIndustryAppService.cs`

**Yêu cầu**:
- Kế thừa từ `ICrudAppService<ResIndustryDto, Guid, GetResIndustriesInput, CreateResIndustryDto, UpdateResIndustryDto>`
- Methods:
  - `GetListAsync(GetResIndustriesInput input)`: Lấy danh sách có filter
  - `GetAsync(Guid id)`: Lấy chi tiết
  - `CreateAsync(CreateResIndustryDto input)`: Tạo mới
  - `UpdateAsync(Guid id, UpdateResIndustryDto input)`: Cập nhật
  - `DeleteAsync(Guid id)`: Xóa (soft delete + set status = Deactive)

---

#### 3.6. Application Service Implementation: ResIndustryAppService
**Location**: `modules/customer/src/iOne.Customer.Application/ResIndustries/ResIndustryAppService.cs`

**Yêu cầu**:
- Kế thừa từ `CrudAppService<ResIndustry, ResIndustryDto, Guid, GetResIndustriesInput, CreateResIndustryDto, UpdateResIndustryDto>`
- Inject: `ResIndustryManager`, `IResIndustryRepository`
- Override methods:
  - `CreateAsync`: Validate code uniqueness, tạo entity
  - `UpdateAsync`: Validate (KHÔNG cho update Code), cập nhật entity
  - `DeleteAsync`: Soft delete → set status = Deactive
  - `GetListAsync`: Filter theo Code, Name, Status

---

#### 3.7. AutoMapper Configuration
**Location**: `modules/customer/src/iOne.Customer.Application/iOneCustomerApplicationAutoMapperProfile.cs`

**Yêu cầu**:
- Mapping `ResIndustry` → `ResIndustryDto`
- Mapping `CreateResIndustryDto` → `ResIndustry`
- Mapping `UpdateResIndustryDto` → `ResIndustry`

---

### 4. Permissions

#### 4.1. Permission Constants
**Location**: `modules/customer/src/iOne.Customer.Application.Contracts/Permissions/ResIndustryPermissions.cs`

**Yêu cầu**:
```csharp
namespace iOne.Customer.Permissions;

public static class ResIndustryPermissions
{
    public const string GroupName = "ResIndustry";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

---

#### 4.2. Permission Definition Provider
**Location**: `modules/customer/src/iOne.Customer.Application.Contracts/Permissions/ResIndustryPermissionDefinitionProvider.cs`

**Yêu cầu**:
- Kế thừa từ `PermissionDefinitionProvider`
- Tạo permission group với `GroupName = "ResIndustry"`
- Tạo Default permission = `"ResIndustry"`
- Thêm các child permissions (Create, Edit, Delete, View)

---

### 5. Localization

#### 5.1. Localization Files
**Location**: 
- `modules/customer/src/iOne.Customer.Application.Contracts/Localization/Customer/vi-VN.json`
- `modules/customer/src/iOne.Customer.Application.Contracts/Localization/Customer/en.json`

**Yêu cầu**:
- Keys cho ResIndustry:
  - `ResIndustry:Code`, `ResIndustry:Name`, `ResIndustry:Description`, `ResIndustry:Status`
  - `ResIndustry:CodeRequired`, `ResIndustry:CodeMaxLength`, `ResIndustry:CodeInvalidFormat`, `ResIndustry:CodeExists`
  - `ResIndustry:NameRequired`, `ResIndustry:NameMaxLength`
  - `ResIndustry:DescriptionMaxLength`
  - `ResIndustry:StatusRequired`
  - `ResIndustry:Active`, `ResIndustry:Deactive`
  - `ResIndustry:New`, `ResIndustry:Edit`, `ResIndustry:Delete`
  - `ResIndustry:CreatedSuccessfully`, `ResIndustry:UpdatedSuccessfully`, `ResIndustry:DeletedSuccessfully`
  - `ResIndustry:DeleteConfirm`
  - `ResIndustry:CodeCannotBeChanged`
- Menu keys:
  - `Menu:Industries` hoặc `Menu:ResIndustries`
- Permission keys:
  - `Permission:ResIndustry`

---

### 6. HTTP API Controllers

#### 6.1. Controller: ResIndustryController
**Location**: `modules/customer/src/iOne.Customer.HttpApi/Controllers/ResIndustryController.cs`

**Yêu cầu**:
- Kế thừa từ `AbpControllerBase`
- Route: `"api/customer/industries"` hoặc `"api/customer/res-industries"`
- Methods:
  - `GetListAsync(GetResIndustriesInput input)`: `[HttpGet]`, `[Authorize(ResIndustryPermissions.View)]`
  - `GetAsync(Guid id)`: `[HttpGet("{id}")]`, `[Authorize(ResIndustryPermissions.View)]`
  - `CreateAsync(CreateResIndustryDto input)`: `[HttpPost]`, `[Authorize(ResIndustryPermissions.Create)]`
  - `UpdateAsync(Guid id, UpdateResIndustryDto input)`: `[HttpPut("{id}")]`, `[Authorize(ResIndustryPermissions.Edit)]`
  - `DeleteAsync(Guid id)`: `[HttpDelete("{id}")]`, `[Authorize(ResIndustryPermissions.Delete)]`

---

#### 6.2. Exclude từ Conventional Controllers
**Location**: `modules/customer/src/iOne.Customer.HttpApi/iOneCustomerHttpApiModule.cs`

**Yêu cầu**:
- Exclude `ResIndustryAppService` khỏi conventional controller generation (nếu có manual controller)

---

### 7. Menu Configuration

#### 7.1. Menu Contributor
**Location**: `modules/customer/src/iOne.Customer.Application/Navigation/CustomerMenuContributor.cs`

**Yêu cầu**:
- Thêm menu item "Ngành nghề" hoặc "Industries" vào Customer menu
- URL: `"~/pages/customer/industries"` hoặc `"~/pages/customer/res-industries"`
- Permission: `ResIndustryPermissions.Default`

---

## 🔍 Business Rules Chi Tiết

### 1. Code Validation
- Format: Chỉ cho phép A-Z, _, 0-9 (uppercase)
- Regex: `^[A-Z0-9_]+$`
- Unique: Mã ngành nghề là duy nhất
- Immutable: Khi update, không được phép thay đổi Code

### 2. Soft Delete
- Khi xóa ngành nghề:
  1. Gọi `Repository.DeleteAsync(entity)` → ABP soft delete (set IsDeleted = true, DeletionTime, DeleterId)
  2. Sau đó gọi `entity.UpdateStatus(ResIndustryStatus.Deactive)` → set Status = Deactive
  3. Gọi `Repository.UpdateAsync(entity)` → lưu status

---

## 📝 Migration Script

### Table: res_industry
```sql
CREATE TABLE IF NOT EXISTS res_industry (
    id UUID NOT NULL,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(250) NOT NULL,
    description VARCHAR(500) NULL,
    status VARCHAR(15) NOT NULL,
    creation_time TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    creator_id UUID NULL,
    last_modification_time TIMESTAMP WITHOUT TIME ZONE NULL,
    last_modifier_id UUID NULL,
    deletion_time TIMESTAMP WITHOUT TIME ZONE NULL,
    deleter_id UUID NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    concurrency_stamp VARCHAR(40) NULL,
    tenant_id UUID NULL,
    CONSTRAINT pk_res_industry PRIMARY KEY (id)
);

COMMENT ON TABLE res_industry IS 'Ngành nghề';
COMMENT ON COLUMN res_industry.status IS 'Trạng thái: active: hoạt động, deactive: không hoạt động';

CREATE UNIQUE INDEX IF NOT EXISTS ix_res_industry_code ON res_industry(code);
```

**Lưu ý**:
- DDL có `CREATIONTIME DATE` nhưng ABP FullAuditedAggregateRoot dùng `DateTime` (TIMESTAMP), nên migration sẽ dùng `TIMESTAMP WITHOUT TIME ZONE`
- DDL có `CREATORID CHAR(36)` → migration sẽ dùng `UUID`
- DDL không có soft delete fields nhưng FullAuditedAggregateRoot có, nên sẽ thêm vào migration

---

## ✅ Checklist Trước Khi Hoàn Thành

- [ ] Entity `ResIndustry` đã tạo với đầy đủ properties và validation
- [ ] Enum `ResIndustryStatus` đã tạo
- [ ] Repository interface và implementation đã tạo
- [ ] Manager class đã tạo với business logic validation
- [ ] EF Core configuration đã tạo (snake_case cho table và columns)
- [ ] DTOs đã tạo (ResIndustryDto, CreateResIndustryDto, UpdateResIndustryDto, GetResIndustriesInput)
- [ ] Application Service interface và implementation đã tạo
- [ ] AutoMapper configuration đã thêm mappings
- [ ] Permissions đã được định nghĩa và sử dụng
- [ ] Localization keys đã đầy đủ (vi-VN và en)
- [ ] HTTP API Controller đã tạo với đầy đủ endpoints
- [ ] Conventional Controllers đã exclude ResIndustryAppService (nếu có manual controller)
- [ ] Menu Contributor đã thêm menu item
- [ ] Migration script đã tạo và test
- [ ] Code validation (A-Z, _, 0-9) đã implement
- [ ] Code immutable khi update đã implement
- [ ] Soft delete + status deactive đã implement
- [ ] Build solution thành công
- [ ] Test API endpoints thành công

---

## 📌 Lưu Ý Quan Trọng

1. **Table và Column Names**: Tất cả phải theo **snake_case** (PostgreSQL convention)
   - Table: `res_industry` (không phải `RESINDUSTRY` hay `ResIndustry`)
   - Columns: `code`, `name`, `description`, `status`, `creation_time`, `creator_id`, etc.
2. **Code Validation**: Regex `^[A-Z0-9_]+$` (uppercase)
3. **Code Immutable**: Không có method `UpdateCode()` trong Entity
4. **Soft Delete**: Xóa trước (ABP audit log) → cập nhật status = Deactive sau
5. **Module Location**: Đặt trong module **Customer** (Quản lý khách hàng)
6. **Permission GroupName**: `"ResIndustry"` (không phải `"CustomerResIndustry"`)
7. **DDL vs Migration**: 
   - DDL có `CREATIONTIME DATE` → Migration sẽ dùng `TIMESTAMP WITHOUT TIME ZONE` (theo ABP convention)
   - DDL có `CREATORID CHAR(36)` → Migration sẽ dùng `UUID` (theo ABP convention)
   - DDL không có soft delete fields → Migration sẽ thêm (theo FullAuditedAggregateRoot)

---

## 🔄 So Sánh với DDL

| DDL Column | Entity Property | Migration Type | Notes |
|------------|----------------|----------------|-------|
| `ID CHAR(36)` | `Id` (Guid) | `UUID` | ✅ |
| `CODE VARCHAR(50)` | `Code` (string) | `VARCHAR(50)` | ✅ |
| `NAME VARCHAR(250)` | `Name` (string) | `VARCHAR(250)` | ✅ |
| `DESCRIPTION VARCHAR(500)` | `Description` (string?) | `VARCHAR(500)` | ✅ |
| `STATUS VARCHAR(15)` | `Status` (ResIndustryStatus) | `VARCHAR(15)` | ✅ Enum → string |
| `CREATIONTIME DATE` | `CreationTime` (DateTime) | `TIMESTAMP WITHOUT TIME ZONE` | ⚠️ ABP dùng TIMESTAMP, không phải DATE |
| `CREATORID CHAR(36)` | `CreatorId` (Guid?) | `UUID` | ✅ |
| `LASTMODIFICATIONTIME DATE` | `LastModificationTime` (DateTime?) | `TIMESTAMP WITHOUT TIME ZONE` | ⚠️ ABP dùng TIMESTAMP, không phải DATE |
| `LASTMODIFIERID CHAR(36)` | `LastModifierId` (Guid?) | `UUID` | ✅ |
| - | `DeletionTime` (DateTime?) | `TIMESTAMP WITHOUT TIME ZONE` | ✅ Thêm từ FullAuditedAggregateRoot |
| - | `DeleterId` (Guid?) | `UUID` | ✅ Thêm từ FullAuditedAggregateRoot |
| - | `IsDeleted` (bool) | `BOOLEAN` | ✅ Thêm từ FullAuditedAggregateRoot |
| - | `ConcurrencyStamp` (string?) | `VARCHAR(40)` | ✅ Thêm từ FullAuditedAggregateRoot |
| - | `TenantId` (Guid?) | `UUID` | ✅ Thêm từ FullAuditedAggregateRoot (nếu multi-tenant) |

---

## 📚 Tham Khảo

- Xem `dev_note/PLAN_HREMPLOYEE_BACKEND.md` để tham khảo cấu trúc tương tự
- Xem `dev_note/RULES_BACKEND_DEVELOPMENT.md` để tham khảo quy tắc chung


