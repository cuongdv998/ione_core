# Plan: Backend - Cấu hình quy trình (BusinessFlow)

## 1. Tổng Quan

**Module**: Master (Danh mục)  
**Entity**: BusinessFlow (Cấu hình quy trình)  
**Table Name**: `business_flow` (snake_case - PostgreSQL convention)  
**Menu**: Đặt trong **Danh mục Quy trình** (submenu dưới Master)  
**Mô tả**: Định nghĩa workflow cho business. Bảng lưu cấu hình quy trình theo tổ chức, công ty bảo hiểm, mã nghiệp vụ và khoảng thời gian hiệu lực.

### Yêu cầu chức năng

| Yêu cầu | Mô tả |
|--------|--------|
| Tìm kiếm | Filter theo OrganizationId, InsurerId, BusinessCode, WorkflowName, WorkflowVersion, Status; phân trang, sắp xếp |
| Thêm mới | OrganizationId, InsurerId, BusinessCode, WorkflowName, WorkflowVersion, EffectDate, ExpireDate (optional) |
| Sửa | **Chỉ** cho phép sửa WorkflowName, WorkflowVersion, ExpireDate — **không** được sửa OrganizationId, InsurerId, BusinessCode, EffectDate |
| Xóa | Soft delete trước (trigger ABP audit log), sau đó cập nhật Status = Deactive |
| BusinessCode | Lựa chọn trong danh sách từ bảng `admin_config` WHERE `code = 'BUSINESS_CODE'`; option value = `sub_code`, label = `value` |
| OrganizationId | Lấy từ bảng `hr_department` WHERE `dept_level = 'unit'` (HrDepartmentLevel.Unit) — dropdown đơn vị |
| InsurerId | Công ty bảo hiểm: lấy từ bảng `res_partner` với `partner_type_id` tương ứng bản ghi trong `res_partner_type` có mã (code) = `'INSURER'` |
| Không overlap | Các bộ (OrganizationId, InsurerId, BusinessCode, EffectDate, ExpireDate) không được trùng khoảng thời gian: với cùng (OrganizationId, InsurerId, BusinessCode), hai bản ghi không được có khoảng [EffectDate, ExpireDate] giao nhau |

---

## 2. Database Schema

### 2.1. DDL Tham Chiếu (Oracle style – chuyển sang PostgreSQL)

Bảng gốc (user cung cấp):

```sql
create table BUSINESSFLOW (
  ID CHAR(36) not null,
  ORGANIZATIONID CHAR(36) null,
  INSURERID CHAR(36) null,
  BUSINESSCODE VARCHAR(50) not null,
  WORKFLOWNAME VARCHAR(50) not null,
  WORKFLOWVERSION VARCHAR(50) not null,
  EFFECTDATE DATE not null,
  EXPIREDATE DATE null,
  CREATIONTIME DATE not null,
  CREATORID CHAR(36) not null,
  LASTMODIFICATIONTIME DATE null,
  LASTMODIFIERID CHAR(36) null,
  constraint PK_BUSINESSFLOW primary key (ID)
);

comment on table BUSINESSFLOW is 'Định nghĩa workflow cho business';
comment on column BUSINESSFLOW.INSURERID is 'Công ty bảo hiểm áp dụng';
comment on column BUSINESSFLOW.BUSINESSCODE is 'Mã nghiệp vụ liên quan, định nghĩa trong bảng AdminConfig với code = BUSINESS_CODE và subcode là các nghiệp vụ tương ứng';
```

### 2.2. Ánh xạ DDL → Entity (PostgreSQL snake_case)

| DDL (UPPERCASE)   | Entity Property     | Column (snake_case)   | Kiểu / Ghi chú                          |
|-------------------|---------------------|------------------------|------------------------------------------|
| ID                | Id                  | id                     | Guid, PK                                 |
| ORGANIZATIONID    | OrganizationId      | organization_id        | Guid?, null                              |
| INSURERID         | InsurerId           | insurer_id             | Guid?, null                              |
| BUSINESSCODE      | BusinessCode        | business_code          | string(50), not null                     |
| WORKFLOWNAME      | WorkflowName        | workflow_name          | string(50), not null                     |
| WORKFLOWVERSION   | WorkflowVersion     | workflow_version       | string(50), not null                     |
| EFFECTDATE        | EffectDate          | effect_date            | DateTime (date), not null                |
| EXPIREDATE        | ExpireDate          | expire_date            | DateTime?, null                          |
| (bổ sung)         | Status              | status                 | enum → varchar(10): active/deactive (cho flow xóa) |
| CREATIONTIME      | CreationTime        | creation_time          | Audit (ABP)                              |
| CREATORID         | CreatorId           | creator_id             | Audit (ABP)                              |
| LASTMODIFICATIONTIME | LastModificationTime | last_modification_time | Audit (ABP)                            |
| LASTMODIFIERID    | LastModifierId      | last_modifier_id       | Audit (ABP)                              |
| (bổ sung ABP)     | IsDeleted           | is_deleted             | Soft delete                              |
| (bổ sung ABP)     | DeletionTime        | deletion_time          | Soft delete                              |
| (bổ sung ABP)     | DeleterId           | deleter_id             | Soft delete                              |
| (bổ sung ABP)     | ConcurrencyStamp    | concurrency_stamp      | ABP                                      |
| (bổ sung ABP)     | TenantId            | tenant_id              | ABP                                      |

**Lưu ý**: Thêm cột `status` (active/deactive) để khi xóa: soft delete trước, sau đó set status = deactive (giống ResBusinessAuthority / SystemEventNotify).

### 2.3. Table Structure (PostgreSQL – snake_case)

- **Table name**: `business_flow`
- **Comment**: Định nghĩa workflow cho business
- **Comment cột**:
  - `organization_id`: Đơn vị (từ hr_department, dept_level = unit)
  - `insurer_id`: Công ty bảo hiểm áp dụng
  - `business_code`: Mã nghiệp vụ liên quan, định nghĩa trong bảng admin_config với code = BUSINESS_CODE và sub_code là các nghiệp vụ tương ứng
  - `workflow_name`: Tên workflow
  - `workflow_version`: Phiên bản workflow
  - `effect_date`: Ngày hiệu lực
  - `expire_date`: Ngày hết hạn
  - `status`: Trạng thái: active - Hoạt động; deactive - Không hoạt động

**Overlap rule (nghiệp vụ)**: Với cùng (organization_id, insurer_id, business_code), hai bản ghi không được có khoảng [effect_date, expire_date] giao nhau.  
- Hai khoảng [a1, a2] và [b1, b2] (a2/b2 null coi như “chưa hết hạn” = DateTime.MaxValue) giao nhau khi: `a1 <= end2 && b1 <= end1` với end = expire_date ?? MaxValue.  
- Validate khi Create và Update (khi Update chỉ đổi WorkflowName/WorkflowVersion/ExpireDate nên vẫn kiểm tra overlap với EffectDate/ExpireDate mới).

---

## 3. Domain Layer

### 3.1. Enum: BusinessFlowStatus

**Location**: `src/common/domain/iOne.Domain.Shared/BusinessFlows/BusinessFlowStatus.cs`

```csharp
namespace iOne.BusinessFlows;

public enum BusinessFlowStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

- Lưu DB dạng string lowercase: `"active"`, `"deactive"` (HasConversion trong EF).

### 3.2. Entity: BusinessFlow

**Location**: `src/common/domain/iOne.Domain/BusinessFlows/BusinessFlow.cs`

**Yêu cầu**:

- Kế thừa `FullAuditedAggregateRoot<Guid>`.
- `[Table("business_flow")]`.
- Properties:
  - `Id`: Guid (base)
  - `OrganizationId`: Guid?, **private set** (immutable khi update)
  - `InsurerId`: Guid?, **private set** (immutable khi update)
  - `BusinessCode`: string, MaxLength(50), Required, **private set** (immutable khi update)
  - `WorkflowName`: string, MaxLength(50), Required
  - `WorkflowVersion`: string, MaxLength(50), Required
  - `EffectDate`: DateTime, Required, **private set** (immutable khi update)
  - `ExpireDate`: DateTime?, optional
  - `Status`: BusinessFlowStatus, Required
- Constructor nhận đủ tham số; private setters cho từng field.
- **Chỉ có method update**: `UpdateWorkflowInfo(string workflowName, string workflowVersion, DateTime? expireDate)` và `UpdateStatus(BusinessFlowStatus status)`.
- **Không** có method UpdateOrganizationId, UpdateInsurerId, UpdateBusinessCode, UpdateEffectDate.

**Gợi ý cấu trúc**:

```csharp
[Table("business_flow")]
public class BusinessFlow : FullAuditedAggregateRoot<Guid>
{
    public virtual Guid? OrganizationId { get; private set; }
    public virtual Guid? InsurerId { get; private set; }
    public virtual string BusinessCode { get; private set; }
    public virtual string WorkflowName { get; private set; }
    public virtual string WorkflowVersion { get; private set; }
    public virtual DateTime EffectDate { get; private set; }
    public virtual DateTime? ExpireDate { get; private set; }
    public virtual BusinessFlowStatus Status { get; private set; }

    // Constructor + private setters
    // UpdateWorkflowInfo(workflowName, workflowVersion, expireDate)
    // UpdateStatus(status)
}
```

### 3.3. Repository Interface

**Location**: `src/common/domain/iOne.Domain/BusinessFlows/IBusinessFlowRepository.cs`

- Kế thừa `IRepository<BusinessFlow, Guid>`.
- Method kiểm tra overlap (dùng khi Create/Update):

  `Task<bool> ExistsOverlapAsync(Guid? organizationId, Guid? insurerId, string businessCode, DateTime effectDate, DateTime? expireDate, Guid? excludeId = null)`

  - Logic: tồn tại bản ghi khác (Id != excludeId, chưa bị soft delete) có cùng (OrganizationId, InsurerId, BusinessCode) và khoảng [EffectDate, ExpireDate ?? Max] giao với [effectDate, expireDate ?? Max].  
  - excludeId: khi Update, loại trừ chính bản ghi đang sửa.

### 3.4. Manager

**Location**: `src/common/domain/iOne.Domain/BusinessFlows/BusinessFlowManager.cs`

- `CreateAsync(BusinessFlow entity)`: Gọi `ExistsOverlapAsync` với (entity.OrganizationId, entity.InsurerId, entity.BusinessCode, entity.EffectDate, entity.ExpireDate, null); nếu true thì throw UserFriendlyException (overlap). Sau đó InsertAsync.
- `UpdateWorkflowInfoAsync(BusinessFlow entity, string workflowName, string workflowVersion, DateTime? expireDate)`:
  - Gọi `ExistsOverlapAsync(entity.OrganizationId, entity.InsurerId, entity.BusinessCode, entity.EffectDate, expireDate, entity.Id)`; nếu true thì throw (overlap).
  - Gọi `entity.UpdateWorkflowInfo(workflowName, workflowVersion, expireDate)` rồi UpdateAsync.
- **Không** có method update OrganizationId/InsurerId/BusinessCode/EffectDate.

---

## 4. Application Layer

### 4.1. DTOs

**Thư mục**: `modules/master/src/iOne.Master.Application.Contracts/BusinessFlows/`

| File | Nội dung |
|------|----------|
| **BusinessFlowDto** | Kế thừa FullAuditedEntityDto&lt;Guid&gt;. Properties: Id, OrganizationId, InsurerId, BusinessCode, WorkflowName, WorkflowVersion, EffectDate, ExpireDate, Status + audit. |
| **CreateBusinessFlowDto** | OrganizationId? (optional), InsurerId? (optional), BusinessCode (required, max 50), WorkflowName (required, max 50), WorkflowVersion (required, max 50), EffectDate (required), ExpireDate? (optional). |
| **UpdateBusinessFlowDto** | **Chỉ** WorkflowName (required, max 50), WorkflowVersion (required, max 50), ExpireDate? (optional). **Không** có OrganizationId, InsurerId, BusinessCode, EffectDate. |
| **GetBusinessFlowsInput** | Kế thừa PagedAndSortedResultRequestDto. Filter: OrganizationId?, InsurerId?, BusinessCode?, WorkflowName?, WorkflowVersion?, Status? (optional). EffectDateFrom?, EffectDateTo?, ExpireDateFrom?, ExpireDateTo? (optional). |

- Display names dùng key localization `Master::BusinessFlow:...`.

### 4.2. API Danh sách BusinessCode (dropdown)

**Nguồn**: Bảng `admin_config` với `Code = "BUSINESS_CODE"`.

- **Giá trị option** (value): `SubCode`.
- **Nhãn** (label): `Value`.

Có thể dùng sẵn API AdminConfig với filter `Code = "BUSINESS_CODE"` (GetList), frontend map `SubCode` → value, `Value` → label; hoặc thêm endpoint trong Master: `GetBusinessCodeOptionsAsync()` trả về list option (value = SubCode, label = Value).

### 4.3. API Danh sách OrganizationId (dropdown – đơn vị)

**Nguồn**: Bảng `hr_department` với `dept_level = unit` (HrDepartmentLevel.Unit).

- **Giá trị option** (value): Id (hoặc Code tùy nghiệp vụ).
- **Nhãn** (label): Tên đơn vị (Name hoặc Code).

Có thể dùng sẵn API HrDepartment với filter DeptLevel = Unit; hoặc thêm endpoint trong Master: `GetOrganizationUnitOptionsAsync()` trả về list đơn vị (unit).

### 4.4. Application Service Interface

**Location**: `modules/master/src/iOne.Master.Application.Contracts/BusinessFlows/IBusinessFlowAppService.cs`

- Kế thừa `ICrudAppService<BusinessFlowDto, Guid, GetBusinessFlowsInput, CreateBusinessFlowDto, UpdateBusinessFlowDto>`.
- (Tùy chọn) Thêm `Task<List<OptionDto>> GetBusinessCodeOptionsAsync()` và `Task<List<OptionDto>> GetOrganizationUnitOptionsAsync()` nếu có API dropdown riêng.

### 4.5. Application Service Implementation

**Location**: `modules/master/src/iOne.Master.Application/BusinessFlows/BusinessFlowAppService.cs`

- CrudAppService với Repository + BusinessFlowManager.
- **CreateAsync**: Map input → entity (Status = Active), gọi Manager.CreateAsync (Manager đã validate overlap).
- **UpdateAsync**: Load entity, **chỉ** lấy WorkflowName, WorkflowVersion, ExpireDate từ input; gọi Manager.UpdateWorkflowInfoAsync(entity, input.WorkflowName, input.WorkflowVersion, input.ExpireDate). Không truyền OrganizationId, InsurerId, BusinessCode, EffectDate.
- **DeleteAsync** (đúng thứ tự audit):
  1. `var entity = await Repository.GetAsync(id);`
  2. `await Repository.DeleteAsync(entity);` → soft delete, kích hoạt audit log ABP (ChangeType Deleted).
  3. `await CurrentUnitOfWork.SaveChangesAsync();`
  4. `entity.UpdateStatus(BusinessFlowStatus.Deactive);`
  5. `await Repository.UpdateAsync(entity);`
  6. `await CurrentUnitOfWork.SaveChangesAsync();`
- **CreateFilteredQueryAsync**: Filter theo OrganizationId, InsurerId, BusinessCode, WorkflowName, WorkflowVersion, Status, EffectDate, ExpireDate (theo GetBusinessFlowsInput).
- Gán policy: GetPolicyName, GetListPolicyName = View; CreatePolicyName = Create; UpdatePolicyName = Edit; DeletePolicyName = Delete.
- LocalizationResource = typeof(MasterResource).

### 4.6. AutoMapper

**Location**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

Thêm:

- `CreateMap<BusinessFlow, BusinessFlowDto>();`
- `CreateMap<CreateBusinessFlowDto, BusinessFlow>();`
- `CreateMap<UpdateBusinessFlowDto, BusinessFlow>();` (chỉ map WorkflowName, WorkflowVersion, ExpireDate; các field khác bỏ qua khi map sang entity trong Update flow — thực tế Update dùng Manager nên có thể chỉ cần map cho đủ cấu hình, AppService không map UpdateDto → Entity mà gọi entity.UpdateWorkflowInfo(...)).

---

## 5. Permissions

### 5.1. Constants

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/BusinessFlowPermissions.cs`

- GroupName = `"MasterBusinessFlow"`.
- Default = GroupName.
- Create, Edit, Delete, View = Default + ".Create", ".Edit", ".Delete", ".View".

### 5.2. Permission Definition Provider

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/BusinessFlowPermissionDefinitionProvider.cs`

- AddGroup(GroupName, L("Permission:BusinessFlow")).
- AddPermission(Default, L("Permission:BusinessFlow")).
- AddChild cho Create, Edit, Delete, View.
- LocalizableString.Create&lt;MasterResource&gt;.

---

## 6. Entity Framework Core

### 6.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/BusinessFlows/BusinessFlowConfiguration.cs`

- `ToTable("business_flow", t => t.HasComment("Định nghĩa workflow cho business"))`.
- `ConfigureByConvention()`.
- Column name snake_case: `organization_id`, `insurer_id`, `business_code`, `workflow_name`, `workflow_version`, `effect_date`, `expire_date`, `status`, `creation_time`, `creator_id`, `last_modification_time`, `last_modifier_id`, `deletion_time`, `deleter_id`, `is_deleted`, `concurrency_stamp`, `tenant_id`.
- Status: HasConversion string lowercase (Active/Deactive → "active"/"deactive").
- **Không** tạo unique index trên (organization_id, insurer_id, business_code, effect_date) vì cho phép nhiều bản ghi cùng bộ nhưng khác khoảng thời gian; overlap được kiểm tra trong code (Repository/Manager).

### 6.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/BusinessFlows/EfCoreBusinessFlowRepository.cs`

- Kế thừa `EfCoreRepository<iOneDbContext, BusinessFlow, Guid>`, implement `IBusinessFlowRepository`.
- Implement `ExistsOverlapAsync`:
  - Query: bản ghi chưa xóa (IsDeleted == false), (OrganizationId, InsurerId, BusinessCode) trùng, Id != excludeId.
  - Với mỗi bản ghi có EffectDate, ExpireDate; so sánh khoảng [effectDate, expireDate ?? Max] với [input effectDate, input expireDate ?? Max]. Overlap khi: start1 <= end2 && start2 <= end1 (với end = expire ?? MaxValue).
  - Return true nếu có ít nhất một bản ghi overlap.

### 6.3. DbContext & Module

- **iOneDbContext**: thêm `DbSet<BusinessFlow> BusinessFlows`, áp dụng `BusinessFlowConfiguration`.
- **iOneEntityFrameworkCoreModule**: `options.AddRepository<BusinessFlow, EfCoreBusinessFlowRepository>()`.

---

## 7. HTTP API

### 7.1. Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/Controllers/BusinessFlowController.cs`

- Route: `"api/master/business-flows"`.
- RemoteService, Area = Master.
- Authorize từng action: View (Get, GetList), Create (Post), Edit (Put), Delete (Delete).
- Gọi IBusinessFlowAppService tương ứng.
- (Tùy chọn) Endpoint GET options: GetBusinessCodeOptions, GetOrganizationUnitOptions.

### 7.2. Exclude Conventional Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`

- Trong Configure AbpAspNetCoreMvcOptions, TypePredicate thêm: `type.Name != "BusinessFlowAppService"` để tránh trùng endpoint.

---

## 8. Localization

**Location**:  
- `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/vi-VN.json`  
- `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/en.json`

Thêm (ví dụ key):

- Menu: `Menu:Process` (Danh mục Quy trình — nếu chưa có), `Menu:BusinessFlow` (Cấu hình quy trình).
- Permission: `Permission:BusinessFlow`.
- Field: BusinessFlow:OrganizationId, InsurerId, BusinessCode, WorkflowName, WorkflowVersion, EffectDate, ExpireDate, Status.
- Validation: WorkflowNameRequired, WorkflowNameMaxLength, WorkflowVersionRequired, WorkflowVersionMaxLength, BusinessCodeRequired, EffectDateRequired, DateRangeOverlap, ...
- Message: CreatedSuccessfully, UpdatedSuccessfully, DeletedSuccessfully, DeleteConfirm.

Đảm bảo cả vi-VN và en cùng bộ key.

---

## 9. Menu (Navigation)

**Location**: `modules/master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

- Nếu chưa có submenu **“Danh mục Quy trình”** (Process): tạo ApplicationMenuItem `"Master.Process"`, text = masterL["Menu:Process"], icon (ví dụ `pi pi-fw pi-sitemap`).
- Trong submenu **“Danh mục Quy trình”**, thêm:
  - ApplicationMenuItem `"Master.BusinessFlow"`, text = masterL["Menu:BusinessFlow"], url = `"~/pages/master/business-flows"`, icon, `.RequirePermissions(BusinessFlowPermissions.Default)`.

Thứ tự: đặt cùng nhóm “Danh mục Quy trình” với các item khác (ResBusinessAuthority, ResBusinessAssignee, …) theo yêu cầu product.

---

## 10. Migration

- Tạo migration từ project EF Core, startup project = DbMigrator.
- **Up**:  
  - `DROP TABLE IF EXISTS "business_flow";`  
  - `CREATE TABLE business_flow` với đủ cột snake_case và comment (id, organization_id, insurer_id, business_code, workflow_name, workflow_version, effect_date, expire_date, status, creation_time, creator_id, last_modification_time, last_modifier_id, is_deleted, deletion_time, deleter_id, concurrency_stamp, tenant_id).  
  - Primary key: `pk_business_flow`.
- **Down**: `DROP TABLE IF EXISTS "business_flow";`
- Đảm bảo tên bảng/cột dùng snake_case.

---

## 11. Logic “Không Overlap” (Chi tiết)

Với cùng (OrganizationId, InsurerId, BusinessCode), hai khoảng [EffectDate, ExpireDate] không được giao nhau.

- Khoảng 1: `start1 = EffectDate`, `end1 = ExpireDate ?? DateTime.MaxValue`.
- Khoảng 2: `start2`, `end2` tương tự.
- Giao nhau khi: `start1 <= end2 && start2 <= end1`.

Trong Repository:

```csharp
public async Task<bool> ExistsOverlapAsync(
    Guid? organizationId,
    Guid? insurerId,
    string businessCode,
    DateTime effectDate,
    DateTime? expireDate,
    Guid? excludeId = null)
{
    var end = expireDate ?? DateTime.MaxValue;
    var query = await GetQueryableAsync();
    query = query.Where(x => !x.IsDeleted
        && x.OrganizationId == organizationId
        && x.InsurerId == insurerId
        && x.BusinessCode == businessCode
        && (excludeId == null || x.Id != excludeId.Value));

    foreach (var x in query)
    {
        var xEnd = x.ExpireDate ?? DateTime.MaxValue;
        if (effectDate <= xEnd && x.EffectDate <= end)
            return true;
    }
    return false;
}
```

(Có thể tối ưu bằng một câu SQL/LINQ duy nhất: tồn tại bản ghi x sao cho khoảng [x.EffectDate, x.ExpireDate ?? Max] giao [effectDate, expireDate ?? Max].)

---

## 12. Checklist

- [ ] Domain: Enum BusinessFlowStatus, Entity BusinessFlow (OrganizationId, InsurerId, BusinessCode, EffectDate immutable khi update), IBusinessFlowRepository (ExistsOverlapAsync), BusinessFlowManager (CreateAsync, UpdateWorkflowInfoAsync, validate overlap).
- [ ] Application: DTOs (Create đủ field; Update chỉ WorkflowName, WorkflowVersion, ExpireDate), IBusinessFlowAppService, BusinessFlowAppService (Delete: delete trước, rồi set status Deactive), AutoMapper.
- [ ] Permissions: BusinessFlowPermissions, BusinessFlowPermissionDefinitionProvider.
- [ ] EF Core: BusinessFlowConfiguration (snake_case, status conversion), EfCoreBusinessFlowRepository (ExistsOverlapAsync), DbContext & Module đăng ký.
- [ ] HTTP: BusinessFlowController, exclude BusinessFlowAppService khỏi conventional controller.
- [ ] Localization: vi-VN + en (menu, permission, field, validation, message).
- [ ] Menu: Submenu “Danh mục Quy trình” + item “Cấu hình quy trình” với permission.
- [ ] Migration: snake_case, IF EXISTS / IF NOT EXISTS.
- [ ] (Tùy chọn) API GetBusinessCodeOptionsAsync (admin_config, code = BUSINESS_CODE), GetOrganizationUnitOptionsAsync (hr_department, dept_level = unit).

---

## 13. Lưu Ý Quan Trọng

1. **Sửa chỉ WorkflowName, WorkflowVersion, ExpireDate**: UpdateDto và Entity không cho phép sửa OrganizationId, InsurerId, BusinessCode, EffectDate.
2. **Xóa**: Luôn thực hiện **xóa trước** (soft delete để ghi audit), **sau đó** cập nhật Status = Deactive.
3. **Overlap**: Validate trong Manager khi Create và khi UpdateWorkflowInfo (vì ExpireDate có thể đổi); dùng Repository.ExistsOverlapAsync.
4. **BusinessCode**: Chỉ lấy từ `admin_config` WHERE `code = 'BUSINESS_CODE'`; option value = `sub_code`, label = `value`.
5. **OrganizationId**: Lấy từ `hr_department` WHERE `dept_level = unit` (HrDepartmentLevel.Unit).
6. **PostgreSQL**: Toàn bộ table/column/index theo **snake_case**.
7. **Menu**: Đặt “Cấu hình quy trình” trong submenu “Danh mục Quy trình” của Master.

---

## 14. Tạo migration (chạy sau khi build thành công)

Từ thư mục gốc solution:

```bash
cd src/common/infra/iOne.EntityFrameworkCore
dotnet ef migrations add AddBusinessFlow --startup-project ../../../web/iOne.HttpApi.Host/iOne.HttpApi.Host.csproj --context iOneDbContext
```

Sau đó kiểm tra file migration: đảm bảo primary key name là `pk_business_flow`, tên bảng `business_flow`, tất cả cột snake_case và comment bảng/cột nếu cần.
