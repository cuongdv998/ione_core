# Plan: Backend - Danh mục công việc (ResTaskCategory)

## 1. Tổng Quan

**Module**: Master (Danh mục)  
**Entity**: ResTaskCategory (Danh mục công việc)  
**Table Name**: `res_task_category` (snake_case - PostgreSQL convention)  
**Menu**: Đặt trong **Danh mục Quy trình** (submenu dưới Master)  
**Mô tả**: Danh sách công việc thực hiện. Bảng lưu danh mục công việc theo loại nghiệp vụ (policy, claim, common).

### Yêu cầu chức năng

| Yêu cầu | Mô tả |
|--------|--------|
| Tìm kiếm | Filter theo Code, BusinessType, Name, Status; phân trang, sắp xếp |
| Thêm mới | BusinessType, Code, Name, Status |
| Sửa | **Chỉ** cho phép sửa Name, Status — **không** được sửa Code, BusinessType |
| Xóa | Soft delete trước (trigger ABP audit log), sau đó cập nhật Status = Deactive |
| Code | Mã công việc; unique toàn bảng (hoặc unique theo cặp BusinessType + Code — xem mục 2.3) |
| BusinessType | Loại nghiệp vụ: policy (cấp đơn), claim (bồi thường), common (chung) — enum |

---

## 2. Database Schema

### 2.1. DDL Tham Chiếu (Postgres style – chuyển sang snake_case)

Bảng gốc (user cung cấp):

```sql
create table RESTASKCATEGORY (
  ID VARCHAR(36) not null,
  BUSINESSTYPE VARCHAR(15) not null,
  CODE VARCHAR(25) not null,
  NAME VARCHAR(250) not null,
  STATUS VARCHAR(10) not null,
  CREATIONTIME DATE not null,
  CREATORID VARCHAR(50) not null,
  LASTMODIFICATIONTIME DATE null,
  LASTMODIFIERID VARCHAR(50) null,
  constraint PK_RESTASKCATEGORY primary key (ID)
);

comment on table RESTASKCATEGORY is 'Danh sách công việc thực hiện';

comment on column RESTASKCATEGORY.BUSINESSTYPE is
'Loại nghiệp vụ:
- policy: cấp đơn
- claim: bồi thường
- common: chung';

comment on column RESTASKCATEGORY.STATUS is
'Trạng thái:
- active: Hoạt động
- deactive: Không hoạt động';
```

### 2.2. Ánh xạ DDL → Entity (PostgreSQL snake_case)

Theo **RULES_BACKEND_DEVELOPMENT.md**: table và column dùng **snake_case**.

| DDL (UPPERCASE)   | Entity Property     | Column (snake_case)   | Kiểu / Ghi chú                          |
|-------------------|---------------------|------------------------|------------------------------------------|
| ID                | Id                  | id                     | Guid, PK                                 |
| BUSINESSTYPE      | BusinessType        | business_type          | ResTaskCategoryBusinessType (enum), varchar(15) |
| CODE              | Code                | code                   | string(25), not null                     |
| NAME              | Name                | name                   | string(250), not null                    |
| STATUS            | Status              | status                 | ResTaskCategoryStatus (enum), varchar(10) |
| CREATIONTIME      | CreationTime        | creation_time          | Audit (ABP)                              |
| CREATORID         | CreatorId           | creator_id             | Audit (ABP)                              |
| LASTMODIFICATIONTIME | LastModificationTime | last_modification_time | Audit (ABP)                            |
| LASTMODIFIERID    | LastModifierId      | last_modifier_id       | Audit (ABP)                              |
| (bổ sung ABP)     | IsDeleted           | is_deleted             | Soft delete                              |
| (bổ sung ABP)     | DeletionTime        | deletion_time          | Soft delete                              |
| (bổ sung ABP)     | DeleterId           | deleter_id             | Soft delete                              |
| (bổ sung ABP)     | ConcurrencyStamp    | concurrency_stamp      | ABP                                      |
| (bổ sung ABP)     | TenantId            | tenant_id              | ABP                                      |

### 2.3. Table Structure (PostgreSQL – snake_case)

- **Table name**: `res_task_category`
- **Comment**: Danh sách công việc thực hiện
- **Comment cột**:
  - `business_type`: Loại nghiệp vụ: policy (cấp đơn), claim (bồi thường), common (chung)
  - `code`: Mã công việc
  - `name`: Tên công việc
  - `status`: Trạng thái: active - Hoạt động; deactive - Không hoạt động

**Unique**: Code unique toàn bảng (giống ResBusinessAuthority). Có thể chọn unique theo cặp (business_type, code) nếu nghiệp vụ cho phép trùng code khác loại.

---

## 3. Domain Layer

### 3.1. Enum: ResTaskCategoryBusinessType

**Location**: `src/common/domain/iOne.Domain.Shared/ResTaskCategories/ResTaskCategoryBusinessType.cs`

```csharp
namespace iOne.ResTaskCategories;

public enum ResTaskCategoryBusinessType
{
    Policy = 0,   // cấp đơn
    Claim = 1,    // bồi thường
    Common = 2    // chung
}
```

- Lưu DB dạng string lowercase: `"policy"`, `"claim"`, `"common"` (HasConversion trong EF).

### 3.2. Enum: ResTaskCategoryStatus

**Location**: `src/common/domain/iOne.Domain.Shared/ResTaskCategories/ResTaskCategoryStatus.cs`

```csharp
namespace iOne.ResTaskCategories;

public enum ResTaskCategoryStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

- Lưu DB dạng string lowercase: `"active"`, `"deactive"` (HasConversion trong EF).

### 3.3. Entity: ResTaskCategory

**Location**: `src/common/domain/iOne.Domain/ResTaskCategories/ResTaskCategory.cs`

**Yêu cầu**:

- Kế thừa `FullAuditedAggregateRoot<Guid>`.
- `[Table("res_task_category")]`.
- Properties:
  - `Id`: Guid (base)
  - `BusinessType`: ResTaskCategoryBusinessType, Required, **private set** (immutable khi update)
  - `Code`: string, MaxLength(25), Required, **private set** (immutable khi update)
  - `Name`: string, MaxLength(250), Required
  - `Status`: ResTaskCategoryStatus, Required
- Constructor nhận đủ tham số; private setters cho các field immutable.
- **Chỉ có method update**: `UpdateName(string name)`, `UpdateStatus(ResTaskCategoryStatus status)`.
- **Không** có method UpdateCode hay UpdateBusinessType.

**Gợi ý cấu trúc**:

```csharp
[Table("res_task_category")]
public class ResTaskCategory : FullAuditedAggregateRoot<Guid>
{
    public virtual ResTaskCategoryBusinessType BusinessType { get; private set; }
    public virtual string Code { get; private set; }
    public virtual string Name { get; private set; }
    public virtual ResTaskCategoryStatus Status { get; private set; }

    // Constructor + private setters (SetBusinessType, SetCode, SetName, SetStatus)
    // UpdateName(string name)
    // UpdateStatus(ResTaskCategoryStatus status)
}
```

- Validation Code: chỉ A-Z, 0-9, _ (uppercase), tương tự ResBusinessAuthority/ResEvent (tùy chuẩn dự án).

### 3.4. Repository Interface

**Location**: `src/common/domain/iOne.Domain/ResTaskCategories/IResTaskCategoryRepository.cs`

- Kế thừa `IRepository<ResTaskCategory, Guid>`.
- Method: `Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null)` dùng cho validate unique Code khi Create/Update (excludeId cho trường hợp update cùng entity).

### 3.5. Manager

**Location**: `src/common/domain/iOne.Domain/ResTaskCategories/ResTaskCategoryManager.cs`

- `CreateAsync(ResTaskCategory entity)`: kiểm tra Code chưa tồn tại (qua Repository), rồi `InsertAsync`.
- `UpdateAsync(ResTaskCategory entity, string name, ResTaskCategoryStatus status)`: chỉ gọi `entity.UpdateName(name)`, `entity.UpdateStatus(status)` rồi `UpdateAsync` — **không** nhận Code, BusinessType.

---

## 4. Application Layer

### 4.1. DTOs

**Thư mục**: `modules/master/src/iOne.Master.Application.Contracts/ResTaskCategories/`

| File | Nội dung |
|------|----------|
| **ResTaskCategoryDto** | Kế thừa FullAuditedEntityDto&lt;Guid&gt;. Properties: Id, BusinessType, Code, Name, Status + audit. |
| **CreateResTaskCategoryDto** | BusinessType (required), Code (required, max 25), Name (required, max 250), Status (default Active). |
| **UpdateResTaskCategoryDto** | Chỉ **Name** (required, max 250), **Status**. **Không** có Code, BusinessType. |
| **GetResTaskCategoriesInput** | Kế thừa PagedAndSortedResultRequestDto. Filter: Code?, BusinessType?, Name?, Status? (optional). |

- Display names dùng key localization `Master::ResTaskCategory::...`.

### 4.2. Application Service Interface

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResTaskCategories/IResTaskCategoryAppService.cs`

- Kế thừa `ICrudAppService<ResTaskCategoryDto, Guid, GetResTaskCategoriesInput, CreateResTaskCategoryDto, UpdateResTaskCategoryDto>`.

### 4.3. Application Service Implementation

**Location**: `modules/master/src/iOne.Master.Application/ResTaskCategories/ResTaskCategoryAppService.cs`

- CrudAppService với Repository + ResTaskCategoryManager.
- **CreateAsync**: validate Code unique (qua Manager/Repository), tạo entity từ input, gọi Manager.CreateAsync, map sang DTO.
- **UpdateAsync**: load entity, gọi Manager.UpdateAsync(entity, input.Name, input.Status) — không truyền Code, BusinessType.
- **DeleteAsync** (đúng thứ tự audit):
  1. `var entity = await Repository.GetAsync(id);`
  2. `await Repository.DeleteAsync(entity);` → soft delete, kích hoạt audit log ABP (ChangeType Deleted).
  3. `await CurrentUnitOfWork.SaveChangesAsync();`
  4. `entity.UpdateStatus(ResTaskCategoryStatus.Deactive);`
  5. `await Repository.UpdateAsync(entity);`
  6. `await CurrentUnitOfWork.SaveChangesAsync();`
- **CreateFilteredQueryAsync**: filter theo Code, BusinessType, Name (ILike), Status (nếu có).
- Gán policy: GetPolicyName, GetListPolicyName = View; CreatePolicyName = Create; UpdatePolicyName = Edit; DeletePolicyName = Delete.
- LocalizationResource = typeof(MasterResource).

### 4.4. AutoMapper

**Location**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

Thêm:

- `CreateMap<ResTaskCategory, ResTaskCategoryDto>();`
- `CreateMap<CreateResTaskCategoryDto, ResTaskCategory>();`
- `CreateMap<UpdateResTaskCategoryDto, ResTaskCategory>();` (chỉ map Name, Status).

---

## 5. Permissions

### 5.1. Constants

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResTaskCategoryPermissions.cs`

- GroupName = `"MasterResTaskCategory"`.
- Default = GroupName.
- Create, Edit, Delete, View = Default + ".Create", ".Edit", ".Delete", ".View".

### 5.2. Permission Definition Provider

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResTaskCategoryPermissionDefinitionProvider.cs`

- AddGroup(GroupName, L("Permission:ResTaskCategory")).
- AddPermission(Default, L("Permission:ResTaskCategory")).
- AddChild cho Create, Edit, Delete, View.
- LocalizableString.Create&lt;MasterResource&gt;.

---

## 6. Entity Framework Core

### 6.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResTaskCategories/ResTaskCategoryConfiguration.cs`

- `ToTable("res_task_category", t => t.HasComment("Danh sách công việc thực hiện"))`.
- `ConfigureByConvention()`.
- Column name snake_case: `business_type`, `code`, `name`, `status`, `creation_time`, `creator_id`, `last_modification_time`, `last_modifier_id`, `deletion_time`, `deleter_id`, `is_deleted`, `concurrency_stamp`, `tenant_id`.
- BusinessType: HasConversion string lowercase (Policy/Claim/Common → "policy"/"claim"/"common").
- Status: HasConversion string lowercase (Active/Deactive → "active"/"deactive").
- Unique index: `HasIndex(e => e.Code, "ix_res_task_category_code").IsUnique()`.

### 6.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResTaskCategories/EfCoreResTaskCategoryRepository.cs`

- Kế thừa `EfCoreRepository<iOneDbContext, ResTaskCategory, Guid>`, implement `IResTaskCategoryRepository`.
- Implement `IsCodeExistsAsync(string code, Guid? excludeId)` (dùng GetQueryableAsync, filter Code, bỏ qua excludeId nếu có).

### 6.3. DbContext & Module

- **iOneDbContext**: thêm `DbSet<ResTaskCategory> ResTaskCategories`, áp dụng `ResTaskCategoryConfiguration`.
- **iOneEntityFrameworkCoreModule**: `options.AddRepository<ResTaskCategory, EfCoreResTaskCategoryRepository>()`.

---

## 7. HTTP API

### 7.1. Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/Controllers/ResTaskCategoryController.cs`

- Route: `"api/master/res-task-categories"`.
- RemoteService, Area = Master.
- Authorize từng action: View (Get, GetList), Create (Post), Edit (Put), Delete (Delete).
- Gọi IResTaskCategoryAppService tương ứng.

### 7.2. Exclude Conventional Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`

- Trong Configure AbpAspNetCoreMvcOptions, TypePredicate thêm: `type.Name != "ResTaskCategoryAppService"` để tránh trùng endpoint.

---

## 8. Localization

**Location**:  
- `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/vi-VN.json`  
- `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/en.json`

Thêm (ví dụ key):

- Menu: `Menu:ResTaskCategory` (Danh mục công việc).
- Permission: `Permission:ResTaskCategory`.
- Field: ResTaskCategory:Code, BusinessType, Name, Status.
- Validation: CodeRequired, CodeMaxLength, CodeInvalid, CodeExists, NameRequired, NameMaxLength, BusinessTypeRequired, StatusRequired.
- Message: CreatedSuccessfully, UpdatedSuccessfully, DeletedSuccessfully, DeleteConfirm.
- (Tùy chọn) CodeCannotBeChanged, BusinessTypeCannotBeChanged.
- BusinessType enum: Policy, Claim, Common (label cho dropdown).

Đảm bảo cả vi-VN và en cùng bộ key.

---

## 9. Menu (Navigation)

**Location**: `modules/master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

- Trong submenu **"Danh mục Quy trình"** (processMenuItem), thêm item:
  - ApplicationMenuItem `"Master.ResTaskCategory"`, text = masterL["Menu:ResTaskCategory"], url = `"~/pages/master/res-task-categories"`, icon (ví dụ `pi pi-fw pi-list`), `.RequirePermissions(ResTaskCategoryPermissions.Default)`.
- Thứ tự: có thể đặt sau ResBusinessAssignee hoặc theo yêu cầu product.

---

## 10. Migration

- Tạo migration từ project EF Core, startup project = DbMigrator.
- **Up**:  
  - `DROP TABLE IF EXISTS "res_task_category";`  
  - CREATE TABLE `res_task_category` với đủ cột snake_case và comment;  
  - CREATE UNIQUE INDEX `ix_res_task_category_code` ON `res_task_category` (code).
- **Down**: DROP INDEX IF EXISTS, DROP TABLE IF EXISTS.
- Đảm bảo tên bảng/cột/index dùng snake_case (theo RULES_BACKEND_DEVELOPMENT.md).

---

## 11. Checklist

- [ ] Domain: Enum ResTaskCategoryBusinessType, ResTaskCategoryStatus; Entity ResTaskCategory (Code, BusinessType immutable); IResTaskCategoryRepository; ResTaskCategoryManager.
- [ ] Application: DTOs (Create đủ field; Update chỉ Name, Status), IResTaskCategoryAppService, ResTaskCategoryAppService (Delete: delete trước, rồi set status Deactive), AutoMapper.
- [ ] Permissions: ResTaskCategoryPermissions, ResTaskCategoryPermissionDefinitionProvider.
- [ ] EF Core: ResTaskCategoryConfiguration (snake_case, unique code, conversion enum → string), EfCoreResTaskCategoryRepository, DbContext & Module đăng ký.
- [ ] HTTP: ResTaskCategoryController, exclude ResTaskCategoryAppService khỏi conventional controller.
- [ ] Localization: vi-VN + en (menu, permission, field, validation, message, BusinessType enum).
- [ ] Menu: Submenu "Danh mục Quy trình" + item "Danh mục công việc" với permission.
- [ ] Migration: snake_case, IF EXISTS / IF NOT EXISTS.

---

## 12. Lưu Ý Quan Trọng

1. **Code và BusinessType immutable khi sửa**: UpdateDto không có hai field này; Entity không có UpdateCode/UpdateBusinessType.
2. **Xóa**: Luôn thực hiện **xóa trước** (soft delete để ghi audit log ABP), **sau đó** cập nhật Status = Deactive.
3. **Code unique**: Validate trong Manager/AppService khi Create; index unique trên `code`.
4. **PostgreSQL**: Toàn bộ table/column/index theo **snake_case** (table: `res_task_category`, columns: `business_type`, `code`, `name`, `status`, ...).
5. **Menu**: Đặt "Danh mục công việc" trong submenu "Danh mục Quy trình" của Master.

---

## 13. Tạo migration (chạy sau khi build thành công)

Từ thư mục gốc solution:

```bash
cd src/common/infra/iOne.EntityFrameworkCore
dotnet ef migrations add AddResTaskCategory --startup-project ../../../web/iOne.HttpApi.Host/iOne.HttpApi.Host.csproj --context iOneDbContext
```

Sau đó kiểm tra file migration: đảm bảo primary key name là `pk_res_task_category`, unique index là `ix_res_task_category_code`, và comment bảng/cột nếu cần.
