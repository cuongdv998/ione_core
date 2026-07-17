# Plan: Backend - Danh mục Phân cấp duyệt (ResBusinessAuthority)

## 1. Tổng Quan

**Module**: Master (Danh mục)  
**Entity**: ResBusinessAuthority (Phân cấp duyệt / Thẩm quyền nghiệp vụ)  
**Table Name**: `res_business_authority` (snake_case - PostgreSQL convention)  
**Menu**: Đặt trong **Danh mục Quy trình** (submenu dưới Master)  
**Mô tả**: Định nghĩa các thẩm quyền thực hiện các bước của quy trình (flow). Mã nghiệp vụ (BusinessCode) lấy từ danh sách cấu hình AdminConfig với `code = 'BUSINESS_CODE'`.

### Yêu cầu chức năng

| Yêu cầu | Mô tả |
|--------|--------|
| Tìm kiếm | Filter theo Code, BusinessCode, Name, Status; phân trang, sắp xếp |
| Thêm mới | Code (unique), BusinessCode (chọn từ AdminConfig), Name, Status |
| Sửa | Chỉ cho phép sửa Name, Status — **không** được sửa Code, BusinessCode |
| Xóa | Soft delete trước (trigger ABP audit log), sau đó cập nhật Status = Deactive |
| Code unique | Mã (Code) là duy nhất toàn bảng |
| BusinessCode | Lựa chọn trong danh sách từ bảng `admin_config` WHERE `code = 'BUSINESS_CODE'`; giá trị option = `sub_code`, label = `value` |

---

## 2. Database Schema

### 2.1. DDL Tham Chiếu (Oracle style – chuyển sang PostgreSQL)

Bảng gốc (Oracle):

```sql
create table RESBUSINESSAUTHORITY (
  ID VARCHAR(36) not null,
  BUSINESSCODE VARCHAR(50) not null,
  CODE VARCHAR(25) not null,
  NAME VARCHAR(250) not null,
  STATUS VARCHAR(10) not null,
  CREATIONTIME DATE not null,
  CREATORID VARCHAR(50) not null,
  LASTMODIFICATIONTIME DATE null,
  LASTMODIFIERID VARCHAR(50) null,
  constraint PK_RESBUSINESSAUTHORITY primary key (ID)
);
```

### 2.2. Table Structure (PostgreSQL – snake_case)

- **Table name**: `res_business_authority`
- **Id**: `Guid` (uuid), PK
- **BusinessCode**: varchar(50), từ AdminConfig `code = 'BUSINESS_CODE'`, column `sub_code`
- **Code**: varchar(25), unique
- **Name**: varchar(250)
- **Status**: varchar(10) — `active` / `deactive`
- Audit: `creation_time`, `creator_id`, `last_modification_time`, `last_modifier_id`, `is_deleted`, `deletion_time`, `deleter_id`, `concurrency_stamp`, `tenant_id` (theo FullAuditedAggregateRoot)

```sql
CREATE TABLE res_business_authority (
    id uuid NOT NULL,
    business_code character varying(50) NOT NULL,
    code character varying(25) NOT NULL,
    name character varying(250) NOT NULL,
    status character varying(10) NOT NULL,
    creation_time timestamp without time zone NOT NULL,
    creator_id uuid NULL,
    last_modification_time timestamp without time zone NULL,
    last_modifier_id uuid NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deletion_time timestamp without time zone NULL,
    deleter_id uuid NULL,
    concurrency_stamp character varying(40) NULL,
    tenant_id uuid NULL,
    CONSTRAINT pk_res_business_authority PRIMARY KEY (id)
);

COMMENT ON TABLE res_business_authority IS 'Định nghĩa các thẩm quyền thực hiện các step của flow';

COMMENT ON COLUMN res_business_authority.business_code IS 'Định nghĩa trong bảng AdminConfig với code = BUSINESS_CODE';

COMMENT ON COLUMN res_business_authority.status IS 'Trạng thái: active - Hoạt động; deactive - Không hoạt động';

CREATE UNIQUE INDEX ix_res_business_authority_code ON res_business_authority (code);
```

### 2.3. Naming Conventions (PostgreSQL)

- **Table**: `res_business_authority` (snake_case)
- **Columns**: `id`, `business_code`, `code`, `name`, `status`, `creation_time`, `creator_id`, `last_modification_time`, `last_modifier_id`, `is_deleted`, `deletion_time`, `deleter_id`, `concurrency_stamp`, `tenant_id`
- **Primary key**: `pk_res_business_authority`
- **Unique index**: `ix_res_business_authority_code` trên `code`

---

## 3. Domain Layer

### 3.1. Enum: ResBusinessAuthorityStatus

**Location**: `src/common/domain/iOne.Domain.Shared/ResBusinessAuthorities/ResBusinessAuthorityStatus.cs`

```csharp
namespace iOne.ResBusinessAuthorities;

public enum ResBusinessAuthorityStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

- Lưu DB dạng string lowercase: `"active"`, `"deactive"` (cấu hình HasConversion trong EF).

### 3.2. Entity: ResBusinessAuthority

**Location**: `src/common/domain/iOne.Domain/ResBusinessAuthorities/ResBusinessAuthority.cs`

**Yêu cầu**:

- Kế thừa `FullAuditedAggregateRoot<Guid>`.
- `[Table("res_business_authority")]`.
- Properties:
  - `Id`: Guid (base)
  - `BusinessCode`: string, MaxLength(50), Required, **private set** (immutable khi update)
  - `Code`: string, MaxLength(25), Required, **private set** (immutable khi update)
  - `Name`: string, MaxLength(250), Required
  - `Status`: ResBusinessAuthorityStatus, Required
- Constructor nhận (id, businessCode, code, name, status); gọi các Set*.
- Private setters: SetBusinessCode, SetCode, SetName, SetStatus.
- Validation Code: chỉ A-Z, 0-9, _ (uppercase), tương tự ResEvent/AdminConfig.
- Public methods: `UpdateName(string)`, `UpdateStatus(ResBusinessAuthorityStatus)`.
- **Không** có `UpdateCode()` hay `UpdateBusinessCode()`.

**Gợi ý cấu trúc**:

```csharp
[Table("res_business_authority")]
public class ResBusinessAuthority : FullAuditedAggregateRoot<Guid>
{
    public virtual string BusinessCode { get; private set; }
    public virtual string Code { get; private set; }
    public virtual string Name { get; private set; }
    public virtual ResBusinessAuthorityStatus Status { get; private set; }

    // Constructor + SetBusinessCode, SetCode, SetName, SetStatus
    // UpdateName(...), UpdateStatus(...)
}
```

### 3.3. Repository Interface

**Location**: `src/common/domain/iOne.Domain/ResBusinessAuthorities/IResBusinessAuthorityRepository.cs`

- Kế thừa `IRepository<ResBusinessAuthority, Guid>`.
- Method: `Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null)` dùng cho validate unique Code khi Create/Update (excludeId cho trường hợp update cùng entity).

### 3.4. Manager

**Location**: `src/common/domain/iOne.Domain/ResBusinessAuthorities/ResBusinessAuthorityManager.cs`

- `CreateAsync(ResBusinessAuthority entity)`: kiểm tra Code chưa tồn tại (qua Repository), rồi `InsertAsync`.
- `UpdateAsync(ResBusinessAuthority entity, string name, ResBusinessAuthorityStatus status)`: chỉ gọi `entity.UpdateName(name)`, `entity.UpdateStatus(status)` rồi `UpdateAsync` — **không** nhận Code/BusinessCode.

---

## 4. Application Layer

### 4.1. DTOs

**Thư mục**: `modules/master/src/iOne.Master.Application.Contracts/ResBusinessAuthorities/`

| File | Nội dung |
|------|----------|
| **ResBusinessAuthorityDto** | Kế thừa FullAuditedEntityDto&lt;Guid&gt;. Properties: Id, BusinessCode, Code, Name, Status + audit. |
| **CreateResBusinessAuthorityDto** | Code (required, max 25), BusinessCode (required, max 50), Name (required, max 250), Status (default Active). |
| **UpdateResBusinessAuthorityDto** | Chỉ **Name** (required, max 250), **Status**. **Không** có Code, BusinessCode. |
| **GetResBusinessAuthoritiesInput** | Kế thừa PagedAndSortedResultRequestDto. Filter: Code, BusinessCode, Name, Status? (optional). |

- Display names dùng key localization `Master::ResBusinessAuthority:...`.

### 4.2. API Danh sách BusinessCode (dropdown)

**Nguồn**: Bảng `admin_config` với `Code = "BUSINESS_CODE"`.

- **Giá trị option** (value): `SubCode`.
- **Nhãn** (label): `Value`.

Có thể:

- Dùng sẵn API AdminConfig với filter `Code = "BUSINESS_CODE"` (GetList), frontend map `SubCode` → value, `Value` → label; hoặc
- Thêm endpoint tiện trong Master: ví dụ `GetBusinessCodeOptionsAsync()` trả về `List<KeyValuePair<string, string>>` hoặc DTO `{ Value = SubCode, Label = Value }` từ AdminConfig, chỉ đọc các bản ghi `Code == "BUSINESS_CODE"` (và có thể chỉ Status = Active).

**Location (nếu tạo riêng)**:  
- Interface: `modules/master/src/iOne.Master.Application.Contracts/ResBusinessAuthorities/IResBusinessAuthorityAppService.cs` (thêm method) hoặc contract riêng.  
- Implementation: gọi `IAdminConfigRepository` hoặc AppService AdminConfig, filter `Code == "BUSINESS_CODE"`.

### 4.3. Application Service Interface

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResBusinessAuthorities/IResBusinessAuthorityAppService.cs`

- Kế thừa `ICrudAppService<ResBusinessAuthorityDto, Guid, GetResBusinessAuthoritiesInput, CreateResBusinessAuthorityDto, UpdateResBusinessAuthorityDto>`.
- (Tùy chọn) Thêm `Task<List<BusinessCodeOptionDto>> GetBusinessCodeOptionsAsync()` nếu có API dropdown riêng.

### 4.4. Application Service Implementation

**Location**: `modules/master/src/iOne.Master.Application/ResBusinessAuthorities/ResBusinessAuthorityAppService.cs`

- CrudAppService với Repository + ResBusinessAuthorityManager.
- **CreateAsync**: validate Code unique (qua Manager/Repository), tạo entity từ input, gọi Manager.CreateAsync, map sang DTO.
- **UpdateAsync**: load entity, gọi Manager.UpdateAsync(entity, input.Name, input.Status) — không truyền Code/BusinessCode.
- **DeleteAsync** (đúng thứ tự audit):
  1. `var entity = await Repository.GetAsync(id);`
  2. `await Repository.DeleteAsync(entity);` → soft delete, kích hoạt audit log ABP (ChangeType Deleted).
  3. `await CurrentUnitOfWork.SaveChangesAsync();`
  4. `entity.UpdateStatus(ResBusinessAuthorityStatus.Deactive);`
  5. `await Repository.UpdateAsync(entity);`
  6. `await CurrentUnitOfWork.SaveChangesAsync();`
- **CreateFilteredQueryAsync**: filter theo Code, BusinessCode, Name (ILike), Status (nếu có).
- Gán policy: GetPolicyName, GetListPolicyName = View; CreatePolicyName = Create; UpdatePolicyName = Edit; DeletePolicyName = Delete.
- LocalizationResource = typeof(MasterResource).

### 4.5. AutoMapper

**Location**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

Thêm:

- `CreateMap<ResBusinessAuthority, ResBusinessAuthorityDto>();`
- `CreateMap<CreateResBusinessAuthorityDto, ResBusinessAuthority>();`
- `CreateMap<UpdateResBusinessAuthorityDto, ResBusinessAuthority>();` (chỉ map Name, Status).

---

## 5. Permissions

### 5.1. Constants

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResBusinessAuthorityPermissions.cs`

- GroupName = `"MasterResBusinessAuthority"`.
- Default = GroupName.
- Create, Edit, Delete, View = Default + ".Create", ".Edit", ".Delete", ".View".

### 5.2. Permission Definition Provider

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResBusinessAuthorityPermissionDefinitionProvider.cs`

- AddGroup(GroupName, L("Permission:ResBusinessAuthority")).
- AddPermission(Default, L("Permission:ResBusinessAuthority")).
- AddChild cho Create, Edit, Delete, View.
- LocalizableString.Create&lt;MasterResource&gt;.

---

## 6. Entity Framework Core

### 6.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResBusinessAuthorities/ResBusinessAuthorityConfiguration.cs`

- `ToTable("res_business_authority", t => t.HasComment("..."))`.
- `ConfigureByConvention()`.
- Column name snake_case: `business_code`, `code`, `name`, `status`, `creation_time`, `creator_id`, `last_modification_time`, `last_modifier_id`, `deletion_time`, `deleter_id`, `is_deleted`, `concurrency_stamp`, `tenant_id`.
- Status: HasConversion string lowercase (Active/Deactive → "active"/"deactive").
- Unique index: `HasIndex(e => e.Code, "ix_res_business_authority_code").IsUnique()`.

### 6.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResBusinessAuthorities/EfCoreResBusinessAuthorityRepository.cs`

- Kế thừa `EfCoreRepository<iOneDbContext, ResBusinessAuthority, Guid>`, implement `IResBusinessAuthorityRepository`.
- Implement `IsCodeExistsAsync(string code, Guid? excludeId)` (dùng GetQueryableAsync, filter Code, bỏ qua excludeId nếu có).

### 6.3. DbContext & Module

- **iOneDbContext**: thêm `DbSet<ResBusinessAuthority> ResBusinessAuthorities`, áp dụng `ResBusinessAuthorityConfiguration`.
- **iOneEntityFrameworkCoreModule**: `options.AddRepository<ResBusinessAuthority, EfCoreResBusinessAuthorityRepository>()`.

---

## 7. HTTP API

### 7.1. Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/Controllers/ResBusinessAuthorityController.cs`

- Route: `"api/master/res-business-authorities"`.
- RemoteService, Area = Master.
- Authorize từng action: View (Get, GetList), Create (Post), Edit (Put), Delete (Delete).
- Gọi IResBusinessAuthorityAppService tương ứng.

### 7.2. Exclude Conventional Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`

- Trong Configure AbpAspNetCoreMvcOptions, TypePredicate thêm: `type.Name != "ResBusinessAuthorityAppService"` để tránh trùng endpoint.

---

## 8. Localization

**Location**:  
- `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/vi-VN.json`  
- `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/en.json`

Thêm (ví dụ key):

- Menu: `Menu:Process` (Danh mục Quy trình), `Menu:ResBusinessAuthority` (Phân cấp duyệt).
- Permission: `Permission:ResBusinessAuthority`.
- Field: ResBusinessAuthority:Code, BusinessCode, Name, Status.
- Validation: CodeRequired, CodeMaxLength, CodeInvalid, CodeExists, NameRequired, NameMaxLength, BusinessCodeRequired, StatusRequired.
- Message: CreatedSuccessfully, UpdatedSuccessfully, DeletedSuccessfully, DeleteConfirm.
- (Tùy chọn) CodeCannotBeChanged, BusinessCodeCannotBeChanged.

Đảm bảo cả vi-VN và en cùng bộ key.

---

## 9. Menu (Navigation)

**Location**: `modules/master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

- Tạo **submenu “Danh mục Quy trình”** (Process) dưới Master:
  - ApplicationMenuItem `"Master.Process"`, text = masterL["Menu:Process"], icon (ví dụ `pi pi-fw pi-sitemap`).
- Trong submenu đó, thêm:
  - ApplicationMenuItem `"Master.ResBusinessAuthority"`, text = masterL["Menu:ResBusinessAuthority"], url = `"~/pages/master/res-business-authorities"`, icon, `.RequirePermissions(ResBusinessAuthorityPermissions.Default)`.

Thứ tự có thể đặt ngay sau nhóm “Sự kiện / Thiết bị / Thông báo” (ResEvent, ResUserDevice, SystemEventNotify) hoặc theo yêu cầu product.

---

## 10. Migration

- Tạo migration từ project EF Core, startup project = DbMigrator.
- **Up**:  
  - DROP TABLE IF EXISTS `res_business_authority`;  
  - CREATE TABLE `res_business_authority` với đủ cột snake_case và comment;  
  - CREATE UNIQUE INDEX `ix_res_business_authority_code` ON `res_business_authority` (code).
- **Down**: DROP INDEX IF EXISTS, DROP TABLE IF EXISTS.
- Đảm bảo tên bảng/cột/index dùng snake_case.

---

## 11. Checklist

- [ ] Domain: Enum ResBusinessAuthorityStatus, Entity ResBusinessAuthority (Code/BusinessCode immutable), IResBusinessAuthorityRepository, ResBusinessAuthorityManager.
- [ ] Application: DTOs (Create không có Id; Update không có Code, BusinessCode), IResBusinessAuthorityAppService, ResBusinessAuthorityAppService (Delete: delete trước, rồi set status Deactive), AutoMapper.
- [ ] Permissions: ResBusinessAuthorityPermissions, ResBusinessAuthorityPermissionDefinitionProvider.
- [ ] EF Core: ResBusinessAuthorityConfiguration (snake_case, unique code), EfCoreResBusinessAuthorityRepository, DbContext & Module đăng ký.
- [ ] HTTP: ResBusinessAuthorityController, exclude ResBusinessAuthorityAppService khỏi conventional controller.
- [ ] Localization: vi-VN + en (menu, permission, field, validation, message).
- [ ] Menu: Submenu “Danh mục Quy trình” + item “Phân cấp duyệt” với permission.
- [ ] Migration: snake_case, IF EXISTS / IF NOT EXISTS.
- [ ] (Tùy chọn) API lấy danh sách BusinessCode từ AdminConfig (GetBusinessCodeOptionsAsync hoặc dùng GetList AdminConfig filter Code).

---

## 12. Lưu Ý Quan Trọng

1. **Code và BusinessCode immutable khi sửa**: UpdateDto không có hai field này; Entity không có UpdateCode/UpdateBusinessCode.
2. **Xóa**: Luôn thực hiện **xóa trước** (soft delete để ghi audit), **sau đó** cập nhật Status = Deactive.
3. **Code unique**: Validate trong Manager/AppService khi Create; index unique trên `code`.
4. **BusinessCode**: Chỉ lấy từ `admin_config` WHERE `code = 'BUSINESS_CODE'`; option value = `sub_code`, label = `value`.
5. **PostgreSQL**: Toàn bộ table/column/index theo **snake_case**.
6. **Menu**: Đặt “Phân cấp duyệt” trong submenu “Danh mục Quy trình” của Master.

---

## 13. Tạo migration (chạy sau khi build thành công)

Từ thư mục gốc solution:

```bash
cd src/common/infra/iOne.EntityFrameworkCore
dotnet ef migrations add AddResBusinessAuthority --startup-project ../../../web/iOne.HttpApi.Host/iOne.HttpApi.Host.csproj --context iOneDbContext
```

Sau đó kiểm tra file migration được sinh ra: đảm bảo primary key name là `pk_res_business_authority`, unique index là `ix_res_business_authority_code`, và comment bảng/cột nếu cần.
