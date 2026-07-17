# Plan Xây Dựng Backend: Danh mục Ngân hàng (ResBank)

## 📋 Tổng Quan

**Module**: Master (Danh mục)  
**Entity**: ResBank (Ngân hàng)  
**Table Name**: `res_bank` (snake_case - PostgreSQL convention)  
**Entity Name Pattern**: Theo pattern `Res*` trong module Master (ResCountry, ResProvince, ResWard, etc.)

---

## 🎯 Yêu Cầu Chức Năng

1. ✅ **CRUD Operations**: Xem, Thêm, Sửa, Xóa
2. ✅ **Validation Code**: Mã ngân hàng chỉ cho phép A-Z, _, 0-9 (uppercase)
3. ✅ **Code Immutable**: Khi sửa không được phép sửa mã ngân hàng (disable trên UI)
4. ✅ **Code Unique**: Mã ngân hàng là duy nhất
5. ✅ **Soft Delete**: Khi xóa → soft delete (ABP audit log) → cập nhật status về Deactive
6. ✅ **Đa ngôn ngữ**: vi-VN và en
7. ✅ **Phân quyền**: View, Create, Edit, Delete

---

## 📁 Cấu Trúc Files Cần Tạo

### 1. Domain Layer

#### 1.1. Entity
**Location**: `src/common/domain/iOne.Domain/ResBanks/ResBank.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>`
- Table name: `[Table("res_bank")]` (snake_case)
- Properties:
  - `Code` (string, max 25, required, private setter)
  - `Name` (string, max 250, required, private setter)
  - `Status` (ResBankStatus enum, required, private setter)
- Constructor với validation
- Private setters với methods: `SetCode()`, `SetName()`, `SetStatus()`
- Public methods: `UpdateName()`, `UpdateStatus()` (KHÔNG có `UpdateCode()`)
- Validation Code: Regex `^[A-Z0-9_]+$` (uppercase)

#### 1.2. Enum Status
**Location**: `src/common/domain/iOne.Domain.Shared/ResBanks/ResBankStatus.cs`

**Yêu cầu**:
- `Active = 0` (Hoạt động)
- `Deactive = 1` (Không hoạt động)

#### 1.3. Repository Interface
**Location**: `src/common/domain/iOne.Domain/ResBanks/IResBankRepository.cs`

**Yêu cầu**:
- Kế thừa từ `IRepository<ResBank, Guid>`
- Method: `IsCodeExistsAsync(string code)` để check code uniqueness

#### 1.4. Manager
**Location**: `src/common/domain/iOne.Domain/ResBanks/ResBankManager.cs`

**Yêu cầu**:
- `CreateAsync(ResBank bank)`: Check code uniqueness, insert
- `UpdateAsync(ResBank bank, string name, ResBankStatus status)`: Update name và status (KHÔNG có code parameter)

---

### 2. Application Layer

#### 2.1. DTOs
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/ResBanks/`

**Files**:
- `ResBankDto.cs`: Kế thừa `FullAuditedEntityDto<Guid>`
  - Properties: `Id`, `Code`, `Name`, `Status`, audit fields
- `CreateResBankDto.cs`:
  - Properties: `Code` (required, max 25), `Name` (required, max 250), `Status` (required)
- `UpdateResBankDto.cs`:
  - Properties: `Name` (required, max 250), `Status` (required)
  - **KHÔNG có Code** (vì không được phép sửa)
- `GetResBanksInput.cs`: Kế thừa `PagedAndSortedResultRequestDto`
  - Properties: `Code?`, `Name?`, `Status?` (optional filters)

#### 2.2. Application Service Interface
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/ResBanks/IResBankAppService.cs`

**Yêu cầu**:
- Kế thừa từ `ICrudAppService<ResBankDto, Guid, GetResBanksInput, CreateResBankDto, UpdateResBankDto>`
- Methods: `GetAsync()`, `GetListAsync()`, `CreateAsync()`, `UpdateAsync()`, `DeleteAsync()`
- Tất cả methods có `[Authorize]` attribute

#### 2.3. Application Service Implementation
**Location**: `modules/Master/src/iOne.Master.Application/ResBanks/ResBankAppService.cs`

**Yêu cầu**:
- Kế thừa từ `CrudAppService<ResBank, ResBankDto, Guid, GetResBanksInput, CreateResBankDto, UpdateResBankDto>`
- Inject: `IResBankRepository`, `ResBankManager`
- `CreateAsync()`: Tạo entity mới, gọi Manager.CreateAsync()
- `UpdateAsync()`: Load entity, gọi Manager.UpdateAsync() (chỉ name và status)
- `DeleteAsync()`: 
  1. Load entity
  2. `Repository.DeleteAsync(entity)` → trigger ABP audit log (soft delete)
  3. `CurrentUnitOfWork.SaveChangesAsync()`
  4. `entity.UpdateStatus(ResBankStatus.Deactive)`
  5. `Repository.UpdateAsync(entity)`
  6. `CurrentUnitOfWork.SaveChangesAsync()`
- `CreateFilteredQueryAsync()`: Filter theo Code, Name, Status
- Set policies: `GetPolicyName`, `GetListPolicyName`, `CreatePolicyName`, `UpdatePolicyName`, `DeletePolicyName`

#### 2.4. AutoMapper Configuration
**Location**: `modules/Master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

**Yêu cầu**:
- Thêm mappings:
  - `CreateMap<ResBank, ResBankDto>()`
  - `CreateMap<CreateResBankDto, ResBank>()`
  - `CreateMap<UpdateResBankDto, ResBank>()`

---

### 3. Permissions

#### 3.1. Permission Constants
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/Permissions/ResBankPermissions.cs`

**Yêu cầu**:
```csharp
public const string GroupName = "MasterResBank";
public const string Default = GroupName;
public const string Create = Default + ".Create";
public const string Edit = Default + ".Edit";
public const string Delete = Default + ".Delete";
public const string View = Default + ".View";
```

#### 3.2. Permission Definition Provider
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/Permissions/ResBankPermissionDefinitionProvider.cs`

**Yêu cầu**:
- Tạo permission group `MasterResBank`
- Tạo Default permission = `MasterResBank`
- Thêm child permissions: Create, Edit, Delete, View
- Sử dụng localization từ `MasterResource`

---

### 4. Entity Framework Core

#### 4.1. Entity Configuration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResBanks/ResBankConfiguration.cs`

**Yêu cầu**:
- Table name: `"res_bank"` (snake_case)
- Column names: `"id"`, `"code"`, `"name"`, `"status"` (snake_case)
- Audit columns: `"creation_time"`, `"creator_id"`, `"last_modification_time"`, `"last_modifier_id"`, `"deletion_time"`, `"deleter_id"`, `"is_deleted"`, `"concurrency_stamp"`, `"tenant_id"` (snake_case)
- Status enum conversion: `Active` → `"active"`, `Deactive` → `"deactive"` (lowercase string)
- Index: Unique index trên `code` với name `"ix_res_bank_code"`

#### 4.2. Repository Implementation
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResBanks/EfCoreResBankRepository.cs`

**Yêu cầu**:
- Kế thừa từ `EfCoreRepository<iOneDbContext, ResBank, Guid>`
- Implement `IResBankRepository`
- Method `IsCodeExistsAsync()`: Check code uniqueness (case-insensitive)

#### 4.3. Register Configuration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/iOneDbContext.cs`

**Yêu cầu**:
- Thêm `modelBuilder.ApplyConfiguration(new ResBankConfiguration());`

---

### 5. Database Migration

**Yêu cầu**:
- Table name: `"res_bank"` (snake_case)
- Column names: `"id"`, `"code"`, `"name"`, `"status"`, audit columns (snake_case)
- Primary key: `"pk_res_bank"` (snake_case với prefix)
- Index: `"ix_res_bank_code"` (unique, snake_case với prefix)
- **LUÔN** sử dụng `IF EXISTS` khi DROP, `IF NOT EXISTS` khi CREATE
- Comment table: `"Danh sách ngân hàng"`
- Comment column status: `"Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động"`

**Migration Script Structure**:
```sql
CREATE TABLE IF NOT EXISTS "res_bank" (
    "id" UUID NOT NULL,
    "code" VARCHAR(25) NOT NULL,
    "name" VARCHAR(250) NOT NULL,
    "status" VARCHAR(10) NOT NULL,
    "creation_time" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    "creator_id" UUID,
    "last_modification_time" TIMESTAMP WITHOUT TIME ZONE,
    "last_modifier_id" UUID,
    "is_deleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "deletion_time" TIMESTAMP WITHOUT TIME ZONE,
    "deleter_id" UUID,
    "concurrency_stamp" VARCHAR(40),
    "tenant_id" UUID,
    "extra_properties" TEXT,
    CONSTRAINT "pk_res_bank" PRIMARY KEY ("id")
);

COMMENT ON TABLE "res_bank" IS 'Danh sách ngân hàng';
COMMENT ON COLUMN "res_bank"."status" IS 'Trạng thái:
- active: Hoạt động
- deactive: Không hoạt động';

CREATE UNIQUE INDEX IF NOT EXISTS "ix_res_bank_code" ON "res_bank" ("code");
```

---

### 6. Localization

#### 6.1. Localization Files
**Location**: 
- `modules/Master/src/iOne.Master.Application.Contracts/Localization/Master/vi-VN.json`
- `modules/Master/src/iOne.Master.Application.Contracts/Localization/Master/en.json`

**Keys cần thêm** (theo pattern của ResCountry):

**vi-VN.json**:
```json
{
  "Menu:ResBank": "Danh mục Ngân hàng",
  "Permission:ResBank": "Ngân hàng",
  "ResBank:Code": "Mã ngân hàng",
  "ResBank:Name": "Tên ngân hàng",
  "ResBank:Status": "Trạng thái",
  "ResBank:CodeRequired": "Mã ngân hàng là bắt buộc",
  "ResBank:CodeMaxLength": "Mã ngân hàng không được vượt quá 25 ký tự",
  "ResBank:CodeInvalid": "Mã ngân hàng chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
  "ResBank:NameRequired": "Tên ngân hàng là bắt buộc",
  "ResBank:NameMaxLength": "Tên ngân hàng không được vượt quá 250 ký tự",
  "ResBank:StatusRequired": "Trạng thái là bắt buộc",
  "ResBank:CodeExists": "Mã ngân hàng '{Code}' đã tồn tại",
  "ResBank:CreatedSuccessfully": "Tạo ngân hàng thành công",
  "ResBank:UpdatedSuccessfully": "Cập nhật ngân hàng thành công",
  "ResBank:DeletedSuccessfully": "Xóa ngân hàng thành công",
  "ResBank:New": "Thêm mới ngân hàng",
  "ResBank:Edit": "Sửa ngân hàng",
  "ResBank:Delete": "Xóa ngân hàng",
  "ResBank:DeleteConfirm": "Bạn có chắc chắn muốn xóa ngân hàng này?",
  "ResBank:CodeCannotBeChanged": "Mã ngân hàng không được phép thay đổi",
  "ResBank:Active": "Hoạt động",
  "ResBank:Deactive": "Không hoạt động",
  "ResBank:SearchByCode": "Tìm theo mã ngân hàng",
  "ResBank:SearchByName": "Tìm theo tên ngân hàng"
}
```

**en.json**:
```json
{
  "Menu:ResBank": "Bank Master",
  "Permission:ResBank": "Bank",
  "ResBank:Code": "Bank Code",
  "ResBank:Name": "Bank Name",
  "ResBank:Status": "Status",
  "ResBank:CodeRequired": "Bank code is required",
  "ResBank:CodeMaxLength": "Bank code cannot exceed 25 characters",
  "ResBank:CodeInvalid": "Bank code can only contain letters (A-Z), numbers (0-9) and underscore (_)",
  "ResBank:NameRequired": "Bank name is required",
  "ResBank:NameMaxLength": "Bank name cannot exceed 250 characters",
  "ResBank:StatusRequired": "Status is required",
  "ResBank:CodeExists": "Bank code '{Code}' already exists",
  "ResBank:CreatedSuccessfully": "Bank created successfully",
  "ResBank:UpdatedSuccessfully": "Bank updated successfully",
  "ResBank:DeletedSuccessfully": "Bank deleted successfully",
  "ResBank:New": "New Bank",
  "ResBank:Edit": "Edit Bank",
  "ResBank:Delete": "Delete Bank",
  "ResBank:DeleteConfirm": "Are you sure you want to delete this bank?",
  "ResBank:CodeCannotBeChanged": "Bank code cannot be changed",
  "ResBank:Active": "Active",
  "ResBank:Deactive": "Inactive",
  "ResBank:SearchByCode": "Search by bank code",
  "ResBank:SearchByName": "Search by bank name"
}
```

---

### 7. HTTP API Controllers

#### 7.1. Controller
**Location**: `modules/Master/src/iOne.Master.HttpApi/Controllers/ResBankController.cs`

**Yêu cầu**:
- Kế thừa từ `AbpControllerBase`
- Route: `"api/master/banks"`
- Attributes:
  - `[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]`
  - `[Area(MasterRemoteServiceConsts.ModuleName)]`
  - `[Authorize]`
- Methods:
  - `GetAsync(Guid id)` → `[HttpGet("{id}")]` + `[Authorize(ResBankPermissions.View)]`
  - `GetListAsync(GetResBanksInput input)` → `[HttpGet]` + `[Authorize(ResBankPermissions.View)]`
  - `CreateAsync(CreateResBankDto input)` → `[HttpPost]` + `[Authorize(ResBankPermissions.Create)]`
  - `UpdateAsync(Guid id, UpdateResBankDto input)` → `[HttpPut("{id}")]` + `[Authorize(ResBankPermissions.Edit)]`
  - `DeleteAsync(Guid id)` → `[HttpDelete("{id}")]` + `[Authorize(ResBankPermissions.Delete)]`

#### 7.2. Exclude từ Conventional Controllers
**Location**: `modules/Master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`

**Yêu cầu**:
- Thêm `ResBankAppService` vào `TypePredicate` để exclude khỏi conventional controller generation

---

### 8. Menu Configuration

#### 8.1. Menu Contributor
**Location**: `modules/Master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

**Yêu cầu**:
- Thêm menu item:
```csharp
masterMenuItem.AddItem(new ApplicationMenuItem(
    "Master.ResBank",
    masterL["Menu:ResBank"],
    url: "~/pages/master/banks",
    icon: "pi pi-fw pi-building"
).RequirePermissions(ResBankPermissions.Default));
```

---

## ✅ Checklist Trước Khi Hoàn Thành

### Domain Layer
- [ ] Entity `ResBank.cs` với table name `res_bank` (snake_case)
- [ ] Enum `ResBankStatus.cs` (Active, Deactive)
- [ ] Repository interface `IResBankRepository.cs` với method `IsCodeExistsAsync()`
- [ ] Manager `ResBankManager.cs` với `CreateAsync()` và `UpdateAsync()`
- [ ] Entity có private setters, không có `UpdateCode()` method
- [ ] Validation Code: Regex `^[A-Z0-9_]+$` (uppercase)

### Application Layer
- [ ] DTOs: `ResBankDto`, `CreateResBankDto`, `UpdateResBankDto` (KHÔNG có Code), `GetResBanksInput`
- [ ] Application Service Interface `IResBankAppService.cs`
- [ ] Application Service Implementation `ResBankAppService.cs`
- [ ] **AutoMapper configuration** đã thêm vào `iOneMasterApplicationAutoMapperProfile.cs`
- [ ] `DeleteAsync()` implement đúng: Delete → Save → Update Status → Save

### Permissions
- [ ] Permission Constants `ResBankPermissions.cs`
- [ ] Permission Definition Provider `ResBankPermissionDefinitionProvider.cs`
- [ ] Permissions được đăng ký trong module

### Entity Framework Core
- [ ] Entity Configuration `ResBankConfiguration.cs`
  - [ ] Table name: `"res_bank"` (snake_case)
  - [ ] Column names: snake_case (code, name, status, audit columns)
  - [ ] Status enum conversion: Active → "active", Deactive → "deactive"
  - [ ] Unique index: `"ix_res_bank_code"`
- [ ] Repository Implementation `EfCoreResBankRepository.cs`
- [ ] Configuration đã register trong `iOneDbContext.cs`

### Database Migration
- [ ] Migration script với table name `"res_bank"` (snake_case)
- [ ] Column names: snake_case
- [ ] Index name: `"ix_res_bank_code"` (snake_case)
- [ ] Primary key name: `"pk_res_bank"` (snake_case)
- [ ] Sử dụng `IF EXISTS`/`IF NOT EXISTS`
- [ ] Table và column comments

### Localization
- [ ] Localization keys đã thêm vào `vi-VN.json`
- [ ] Localization keys đã thêm vào `en.json`
- [ ] Keys match nhau giữa 2 file

### HTTP API
- [ ] Controller `ResBankController.cs` với đầy đủ endpoints
- [ ] Controller có `[Authorize]` attributes với permissions
- [ ] `ResBankAppService` đã exclude khỏi conventional controllers

### Menu
- [ ] Menu item đã thêm vào `MasterMenuContributor.cs`
- [ ] Menu có permission check

### Testing
- [ ] Build solution thành công
- [ ] Migration chạy thành công
- [ ] Test API endpoints:
  - [ ] GET `/api/master/banks` (list)
  - [ ] GET `/api/master/banks/{id}` (detail)
  - [ ] POST `/api/master/banks` (create)
  - [ ] PUT `/api/master/banks/{id}` (update - không được sửa Code)
  - [ ] DELETE `/api/master/banks/{id}` (delete - soft delete + status deactive)
- [ ] Test validation:
  - [ ] Code chỉ chấp nhận A-Z, 0-9, _
  - [ ] Code là unique
  - [ ] Update không được sửa Code
- [ ] Test permissions
- [ ] Test localization (vi-VN và en)
- [ ] Test menu hiển thị đúng

---

## 🔍 Lưu Ý Quan Trọng

1. **Table và Column Names**: **BẮT BUỘC** sử dụng **snake_case** (PostgreSQL convention)
   - Table: `"res_bank"` (không phải `"RESBANK"` hay `"ResBank"`)
   - Columns: `"code"`, `"name"`, `"status"` (không phải `"Code"`, `"Name"`, `"Status"`)

2. **Code Immutable**: Code không được phép sửa sau khi tạo
   - Entity không có `UpdateCode()` method
   - `UpdateResBankDto` không có `Code` property
   - Manager `UpdateAsync()` không có `code` parameter

3. **Soft Delete Flow**: 
   - Gọi `Repository.DeleteAsync()` trước để trigger ABP audit log
   - Sau đó mới update status về Deactive
   - Đảm bảo có 2 lần `SaveChangesAsync()` để audit log ghi nhận đúng

4. **Status Enum Conversion**: 
   - Enum: `Active`, `Deactive`
   - Database: `"active"`, `"deactive"` (lowercase string)
   - Sử dụng `HasConversion<string>()` trong Entity Configuration

5. **Code Validation**: 
   - Regex: `^[A-Z0-9_]+$`
   - Tự động convert sang uppercase trong Entity constructor

6. **AutoMapper**: **BẮT BUỘC** thêm mappings vào `iOneMasterApplicationAutoMapperProfile.cs`

7. **Conventional Controllers**: Phải exclude `ResBankAppService` khỏi conventional controller generation

---

## 📝 So Sánh với DLL Hiện Tại

**DLL Hiện Tại**:
- Table: `RESBANK` (UPPERCASE)
- Columns: `ID`, `CODE`, `NAME`, `STATUS`, `CREATIONTIME`, `CREATORID`, `LASTMODIFICATIONTIME`, `LASTMODIFIERID` (UPPERCASE)
- Primary Key: `PK_RESBANK` (UPPERCASE)

**Implementation Mới**:
- Table: `res_bank` (snake_case)
- Columns: `id`, `code`, `name`, `status`, `creation_time`, `creator_id`, `last_modification_time`, `last_modifier_id`, `is_deleted`, `deletion_time`, `deleter_id`, `concurrency_stamp`, `tenant_id` (snake_case)
- Primary Key: `pk_res_bank` (snake_case với prefix)

**Lưu ý**: Cần migration script để convert từ DLL hiện tại sang cấu trúc mới (nếu có dữ liệu cũ).

---

## 🚀 Thứ Tự Thực Hiện

1. **Domain Layer** (Entity, Enum, Repository Interface, Manager)
2. **EF Core Configuration** (Entity Configuration, Repository Implementation)
3. **Application Layer** (DTOs, AppService Interface, AppService Implementation)
4. **AutoMapper Configuration**
5. **Permissions** (Constants, Definition Provider)
6. **Localization** (vi-VN và en)
7. **HTTP API Controller**
8. **Menu Configuration**
9. **Database Migration**
10. **Testing**

---

## 📌 Reference Files

- Entity mẫu: `src/common/domain/iOne.Domain/ResCountries/ResCountry.cs`
- AppService mẫu: `modules/Master/src/iOne.Master.Application/ResCountries/ResCountryAppService.cs`
- Configuration mẫu: `src/common/infra/iOne.EntityFrameworkCore/ResCountries/ResCountryConfiguration.cs`
- Rules: `dev_note/RULES_BACKEND_DEVELOPMENT.md`

