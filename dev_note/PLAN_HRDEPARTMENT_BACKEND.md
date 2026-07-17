# Plan Xây Dựng Backend: Quản lý Phòng ban (HrDepartment)

## 📋 Tổng Quan

**Module**: Hr (Quản lý nhân viên)  
**Entity**: HrDepartment (Phòng ban)  
**Table Name**: `hr_department` (snake_case - PostgreSQL convention)  
**Entity Name Pattern**: Theo pattern trong module HR (HrDepartmentType, HrEmployeeRole, etc.)

---

## 🎯 Yêu Cầu Chức Năng

1. ✅ **CRUD Operations**: Tìm kiếm, Thêm, Sửa, Xóa
2. ✅ **Validation Code**: Mã phòng ban chỉ cho phép A-Z, _, 0-9 (uppercase)
3. ✅ **Code Immutable**: Khi sửa không được phép sửa mã phòng ban (disable trên UI)
4. ✅ **Code Unique**: Mã phòng ban là duy nhất
5. ✅ **Parent Department**: Cho phép chọn phòng ban cha (self-referencing), có thể bỏ trống
6. ✅ **Auto Create Partner**: Khi tạo phòng ban, tự động tạo Partner tương ứng với PartnerType có code = "DIRECT"
7. ✅ **Partner Reference**: Phòng ban có trường `partner_id` tham chiếu đến Partner đã tạo
8. ✅ **Soft Delete**: Khi xóa → soft delete (ABP audit log) → cập nhật status về Deactive
9. ✅ **Tree Structure**: Hỗ trợ cấu trúc tree (parent-child relationship)
10. ✅ **OrgId Reference**: `org_id` tham chiếu đến phòng ban khác (đơn vị trực thuộc)
11. ✅ **BankId Reference**: `bank_id` tham chiếu đến `res_bank`
12. ✅ **Đa ngôn ngữ**: vi-VN và en
13. ✅ **Phân quyền**: View, Create, Edit, Delete

---

## 📁 Cấu Trúc Files Cần Tạo

### 1. Domain Layer

#### 1.1. Enum Status
**Location**: `src/common/domain/iOne.Domain.Shared/HrDepartments/HrDepartmentStatus.cs`

**Yêu cầu**:
- `Active = 0` (Hoạt động)
- `Deactive = 1` (Không hoạt động)

#### 1.2. Enum Department Level
**Location**: `src/common/domain/iOne.Domain.Shared/HrDepartments/HrDepartmentLevel.cs`

**Yêu cầu**:
- `Unit = 0` (Đơn vị)
- `Dept = 1` (Phòng/Ban)

#### 1.3. Entity
**Location**: `src/common/domain/iOne.Domain/HrDepartments/HrDepartment.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>`
- Table name: `[Table("hr_department")]` (snake_case)
- Properties:
  - `Code` (string, max 25, required, private setter)
  - `Name` (string, max 250, required, private setter)
  - `Description` (string, max 500, optional, private setter)
  - `Status` (HrDepartmentStatus enum, required, private setter)
  - `DeptLevel` (HrDepartmentLevel enum, required, private setter)
  - `ParentId` (Guid?, optional, self-reference)
  - `OrgId` (Guid, required, self-reference - đơn vị trực thuộc)
  - `TypeId` (Guid?, optional, FK to HrDepartmentType)
  - `PartnerId` (Guid, required, FK to ResPartner - tự động tạo)
  - `ProvinceId` (Guid?, optional, FK to ResProvince)
  - `WardId` (Guid?, optional, FK to ResWard)
  - `Address` (string, max 500, optional)
  - `FullAddress` (string, max 500, optional)
  - `BankId` (Guid?, optional, FK to ResBank)
  - `BankNo` (string, max 50, optional)
- Constructor với validation
- Private setters với methods: `SetCode()`, `SetName()`, etc.
- Public methods: `UpdateName()`, `UpdateStatus()`, etc. (KHÔNG có `UpdateCode()`)
- Validation Code: Regex `^[A-Z0-9_]+$` (uppercase)
- Navigation properties:
  - `Parent` (HrDepartment?)
  - `Org` (HrDepartment)
  - `Type` (HrDepartmentType?)
  - `Partner` (ResPartner)
  - `Province` (ResProvince?)
  - `Ward` (ResWard?)
  - `Bank` (ResBank?)
  - `Children` (ICollection<HrDepartment>)

#### 1.4. Repository Interface
**Location**: `src/common/domain/iOne.Domain/HrDepartments/IHrDepartmentRepository.cs`

**Yêu cầu**:
- Kế thừa từ `IRepository<HrDepartment, Guid>`
- Methods:
  - `IsCodeExistsAsync(string code, Guid? excludeId = null)`: Check code uniqueness
  - `GetTreeAsync()`: Lấy danh sách dạng tree (parent-child) - return `List<HrDepartmentTreeDto>`
  - `GetByParentIdAsync(Guid? parentId)`: Lấy các phòng ban con của parent - return `List<HrDepartment>`
  - `GetRootDepartmentsAsync()`: Lấy các phòng ban gốc (ParentId = null) - return `List<HrDepartment>`
  - `GetAllActiveAsync()`: Lấy tất cả departments active (để build tree)

#### 1.5. Manager
**Location**: `src/common/domain/iOne.Domain/HrDepartments/HrDepartmentManager.cs`

**Yêu cầu**:
- Inject: `IHrDepartmentRepository`, `IResPartnerRepository`, `IResPartnerTypeRepository`, `IResProvinceRepository`, `IResWardRepository`, `ResPartnerManager` (để compute FullAddress)
- `CreateAsync(HrDepartment department, ResPartner partner)`: 
  - Check code uniqueness
  - Validate parent exists (nếu có)
  - Validate org exists
  - Validate type exists (nếu có)
  - Validate province/ward exists (nếu có)
  - Validate bank exists (nếu có)
  - Insert partner trước
  - Insert department sau
- `UpdateAsync(HrDepartment department, ...)`: Update các fields (KHÔNG có code parameter)
- `CreatePartnerForDepartmentAsync(CreateHrDepartmentDto input)`: 
  - **Bước 1**: Tìm ResPartnerType có code = "DIRECT"
    - `var partnerType = await partnerTypeRepository.FirstOrDefaultAsync(x => x.Code == "DIRECT")`
    - Nếu không tìm thấy: throw `BusinessException("Hr:HrDepartment:PartnerTypeDirectNotFound")`
  - **Bước 2**: Load Province và Ward (nếu có) để compute FullAddress
    - Nếu có `input.ProvinceId`: Load Province
    - Nếu có `input.WardId`: Load Ward
    - Nếu không có Province/Ward: Cần có default Province/Ward hoặc throw exception
  - **Bước 3**: Compute FullAddress
    - Address = `input.Address ?? ""`
    - WardName = Ward?.Name ?? ""
    - ProvinceName = Province?.Name ?? ""
    - FullAddress = `ResPartnerManager.ComputeFullAddress(Address, WardName, ProvinceName)`
  - **Bước 4**: Tạo ResPartner
    - PartnerTypeId = partnerType.Id
    - Name = input.Name
    - Address = input.Address ?? "" (required field)
    - FullAddress = computed FullAddress (required field)
    - ProvinceId = input.ProvinceId ?? defaultProvinceId (required field - cần có default hoặc validate)
    - WardId = input.WardId ?? defaultWardId (required field - cần có default hoặc validate)
    - Phone = "" hoặc default phone (required field)
    - Status = ResPartnerStatus.Active
    - Code = null (optional)
  - **Bước 5**: Insert Partner và return PartnerId

---

### 2. Application Layer

#### 2.1. DTOs
**Location**: `modules/Hr/src/iOne.Hr.Application.Contracts/HrDepartments/`

**Files**:
- `HrDepartmentDto.cs`: Kế thừa `FullAuditedEntityDto<Guid>`
  - Properties: Tất cả fields + navigation properties (ParentName, OrgName, TypeName, PartnerName, ProvinceName, WardName, BankName)
- `CreateHrDepartmentDto.cs`:
  - Properties: `Code`, `Name`, `Description?`, `Status`, `DeptLevel`, `ParentId?`, `OrgId`, `TypeId?`, `ProvinceId?`, `WardId?`, `Address?`, `FullAddress?`, `BankId?`, `BankNo?`
  - **Lưu ý**: `ProvinceId` và `WardId` là optional trong Department, nhưng **BẮT BUỘC** khi tạo Partner (vì Partner.ProvinceId và Partner.WardId là required)
  - **Giải pháp**: Validate `ProvinceId` và `WardId` phải có trong `CreateAsync()` trước khi tạo Partner, hoặc có default values
- `UpdateHrDepartmentDto.cs`:
  - Properties: `Name`, `Description?`, `Status`, `DeptLevel`, `ParentId?`, `OrgId`, `TypeId?`, `ProvinceId?`, `WardId?`, `Address?`, `FullAddress?`, `BankId?`, `BankNo?`
  - **KHÔNG có Code** (vì không được phép sửa)
- `GetHrDepartmentsInput.cs`: Kế thừa `PagedAndSortedResultRequestDto`
  - Properties: `Code?`, `Name?`, `Status?`, `ParentId?`, `OrgId?`, `TypeId?`, `DeptLevel?` (optional filters)
- `HrDepartmentTreeDto.cs`: DTO cho tree structure
  - Properties: `Id`, `Code`, `Name`, `Status`, `Children` (List<HrDepartmentTreeDto>)

#### 2.2. Application Service Interface
**Location**: `modules/Hr/src/iOne.Hr.Application.Contracts/HrDepartments/IHrDepartmentAppService.cs`

**Yêu cầu**:
- Kế thừa từ `ICrudAppService<HrDepartmentDto, Guid, GetHrDepartmentsInput, CreateHrDepartmentDto, UpdateHrDepartmentDto>`
- Additional methods:
  - `GetTreeAsync()`: Lấy danh sách dạng tree
  - `GetByParentIdAsync(Guid? parentId)`: Lấy các phòng ban con
- Tất cả methods có `[Authorize]` attribute

#### 2.3. Application Service Implementation
**Location**: `modules/Hr/src/iOne.Hr.Application/HrDepartments/HrDepartmentAppService.cs`

**Yêu cầu**:
- Kế thừa từ `CrudAppService<HrDepartment, HrDepartmentDto, Guid, GetHrDepartmentsInput, CreateHrDepartmentDto, UpdateHrDepartmentDto>`
- Inject: `IHrDepartmentRepository`, `HrDepartmentManager`, `IResPartnerRepository`, `IResPartnerTypeRepository`, `IResProvinceRepository`, `IResWardRepository`, `IResBankRepository`, `ResPartnerManager` (để compute FullAddress)
- `CreateAsync()`: 
  1. Validate input (Code, Name, OrgId, etc.)
  2. Validate ParentId tồn tại (nếu có)
  3. Validate OrgId tồn tại
  4. Validate TypeId tồn tại (nếu có)
  5. **Validate ProvinceId và WardId BẮT BUỘC** (vì Partner cần các fields này)
     - Nếu không có: throw BusinessException hoặc sử dụng default values
  6. Validate ProvinceId tồn tại
  7. Validate WardId tồn tại
  8. Validate BankId tồn tại (nếu có)
  9. Tạo Partner trước (gọi Manager.CreatePartnerForDepartmentAsync(input))
  10. Tạo Department với PartnerId
  11. Gọi Manager.CreateAsync(department, partner)
- `UpdateAsync()`: Load entity, gọi Manager.UpdateAsync() (chỉ name và các fields khác, KHÔNG có code)
- `DeleteAsync()`: 
  1. Load entity
  2. `Repository.DeleteAsync(entity)` → trigger ABP audit log (soft delete)
  3. `CurrentUnitOfWork.SaveChangesAsync()`
  4. `entity.UpdateStatus(HrDepartmentStatus.Deactive)`
  5. `Repository.UpdateAsync(entity)`
  6. `CurrentUnitOfWork.SaveChangesAsync()`
- `GetTreeAsync()`: Lấy danh sách dạng tree
- `GetByParentIdAsync()`: Lấy các phòng ban con
- `CreateFilteredQueryAsync()`: Filter theo Code, Name, Status, ParentId, OrgId, TypeId, DeptLevel
- Set policies: `GetPolicyName`, `GetListPolicyName`, `CreatePolicyName`, `UpdatePolicyName`, `DeletePolicyName`

#### 2.4. AutoMapper Configuration
**Location**: `modules/Hr/src/iOne.Hr.Application/iOneHrApplicationAutoMapperProfile.cs`

**Yêu cầu**:
- Thêm mappings:
  - `CreateMap<HrDepartment, HrDepartmentDto>()`
  - `CreateMap<CreateHrDepartmentDto, HrDepartment>()`
  - `CreateMap<UpdateHrDepartmentDto, HrDepartment>()`
  - `CreateMap<HrDepartment, HrDepartmentTreeDto>()`

---

### 3. Permissions

#### 3.1. Permission Constants
**Location**: `modules/Hr/src/iOne.Hr.Application.Contracts/Permissions/HrDepartmentPermissions.cs`

**Yêu cầu**:
```csharp
public const string GroupName = "HrDepartment";
public const string Default = GroupName;
public const string Create = Default + ".Create";
public const string Edit = Default + ".Edit";
public const string Delete = Default + ".Delete";
public const string View = Default + ".View";
```

#### 3.2. Permission Definition Provider
**Location**: `modules/Hr/src/iOne.Hr.Application.Contracts/Permissions/HrDepartmentPermissionDefinitionProvider.cs`

**Yêu cầu**:
- Tạo permission group `HrDepartment`
- Tạo Default permission = `HrDepartment`
- Thêm child permissions: Create, Edit, Delete, View
- Sử dụng localization từ `HrResource`

---

### 4. Entity Framework Core

#### 4.1. Entity Configuration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrDepartments/HrDepartmentConfiguration.cs`

**Yêu cầu**:
- Table name: `"hr_department"` (snake_case)
- Column names: `"id"`, `"code"`, `"name"`, `"description"`, `"status"`, `"dept_level"`, `"parent_id"`, `"org_id"`, `"type_id"`, `"partner_id"`, `"province_id"`, `"ward_id"`, `"address"`, `"full_address"`, `"bank_id"`, `"bank_no"` (snake_case)
- Audit columns: snake_case
- Status enum conversion: `Active` → `"active"`, `Deactive` → `"deactive"` (lowercase string)
- DeptLevel enum conversion: `Unit` → `"unit"`, `Dept` → `"dept"` (lowercase string)
- Indexes:
  - Unique index trên `code` với name `"ix_hr_department_code"`
  - Index trên `parent_id` với name `"ix_hr_department_parent_id"`
  - Index trên `org_id` với name `"ix_hr_department_org_id"`
- Foreign keys:
  - `parent_id` → `hr_department(id)` với name `"fk_hr_department_parent_id"`
  - `org_id` → `hr_department(id)` với name `"fk_hr_department_org_id"`
  - `type_id` → `hr_department_type(id)` với name `"fk_hr_department_type_id"`
  - `partner_id` → `res_partner(id)` với name `"fk_hr_department_partner_id"`
  - `province_id` → `res_province(id)` với name `"fk_hr_department_province_id"`
  - `ward_id` → `res_ward(id)` với name `"fk_hr_department_ward_id"`
  - `bank_id` → `res_bank(id)` với name `"fk_hr_department_bank_id"`

#### 4.2. Repository Implementation
**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrDepartments/EfCoreHrDepartmentRepository.cs`

**Yêu cầu**:
- Kế thừa từ `EfCoreRepository<iOneDbContext, HrDepartment, Guid>`
- Implement `IHrDepartmentRepository`
- Method `IsCodeExistsAsync()`: Check code uniqueness (case-insensitive)
- Method `GetTreeAsync()`: Lấy danh sách dạng tree (recursive query)
  - Load tất cả departments (chỉ Active)
  - Build tree structure trong memory (hoặc dùng recursive CTE nếu cần)
  - Return root departments với Children nested
- Method `GetByParentIdAsync(Guid? parentId)`: Lấy các phòng ban con
  - Query: `Where(x => x.ParentId == parentId)`
  - Include navigation properties nếu cần
- Method `GetRootDepartmentsAsync()`: Lấy các phòng ban gốc
  - Query: `Where(x => x.ParentId == null)`

#### 4.3. Register Configuration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`

**Yêu cầu**:
- Thêm `modelBuilder.ApplyConfiguration(new HrDepartmentConfiguration());`
- Thêm `DbSet<HrDepartment> HrDepartments { get; set; }`

---

### 5. Database Migration

**Yêu cầu**:
- Table name: `"hr_department"` (snake_case)
- Column names: snake_case
- Primary key: `"pk_hr_department"` (snake_case với prefix)
- Indexes: snake_case với prefix
- Foreign keys: snake_case với prefix
- **LUÔN** sử dụng `IF EXISTS` khi DROP, `IF NOT EXISTS` khi CREATE
- Comment table: `"Phòng ban/Đơn vị"`
- Comment columns theo DLL

**Migration Script Structure**:
```sql
CREATE TABLE IF NOT EXISTS "hr_department" (
    "id" UUID NOT NULL,
    "code" VARCHAR(25) NOT NULL,
    "name" VARCHAR(250) NOT NULL,
    "description" VARCHAR(500),
    "status" VARCHAR(10) NOT NULL,
    "dept_level" VARCHAR(15) NOT NULL,
    "parent_id" UUID,
    "org_id" UUID NOT NULL,
    "type_id" UUID,
    "partner_id" UUID NOT NULL,
    "province_id" UUID,
    "ward_id" UUID,
    "address" VARCHAR(500),
    "full_address" VARCHAR(500),
    "bank_id" UUID,
    "bank_no" VARCHAR(50),
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
    CONSTRAINT "pk_hr_department" PRIMARY KEY ("id")
);

COMMENT ON TABLE "hr_department" IS 'Phòng ban/Đơn vị';
COMMENT ON COLUMN "hr_department"."status" IS 'Trạng thái:
- active: Hoạt động
- deactive: Không hoạt động';
COMMENT ON COLUMN "hr_department"."dept_level" IS 'Đơn vị hay phòng/ban:
- unit: đơn vị
- dept: phòng/ban';

CREATE UNIQUE INDEX IF NOT EXISTS "ix_hr_department_code" ON "hr_department" ("code");
CREATE INDEX IF NOT EXISTS "ix_hr_department_parent_id" ON "hr_department" ("parent_id");
CREATE INDEX IF NOT EXISTS "ix_hr_department_org_id" ON "hr_department" ("org_id");

ALTER TABLE "hr_department"
ADD CONSTRAINT "fk_hr_department_parent_id" FOREIGN KEY ("parent_id") REFERENCES "hr_department" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE "hr_department"
ADD CONSTRAINT "fk_hr_department_org_id" FOREIGN KEY ("org_id") REFERENCES "hr_department" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE "hr_department"
ADD CONSTRAINT "fk_hr_department_type_id" FOREIGN KEY ("type_id") REFERENCES "hr_department_type" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE "hr_department"
ADD CONSTRAINT "fk_hr_department_partner_id" FOREIGN KEY ("partner_id") REFERENCES "res_partner" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE "hr_department"
ADD CONSTRAINT "fk_hr_department_province_id" FOREIGN KEY ("province_id") REFERENCES "res_province" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE "hr_department"
ADD CONSTRAINT "fk_hr_department_ward_id" FOREIGN KEY ("ward_id") REFERENCES "res_ward" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE "hr_department"
ADD CONSTRAINT "fk_hr_department_bank_id" FOREIGN KEY ("bank_id") REFERENCES "res_bank" ("id") ON DELETE RESTRICT ON UPDATE RESTRICT;
```

---

### 6. Localization

#### 6.1. Localization Files
**Location**: 
- `modules/Hr/src/iOne.Hr.Application.Contracts/Localization/Hr/vi-VN.json`
- `modules/Hr/src/iOne.Hr.Application.Contracts/Localization/Hr/en.json`

**Keys cần thêm** (theo pattern của HrDepartmentType):

**vi-VN.json**:
```json
{
  "Menu:HrDepartment": "Quản lý Phòng ban",
  "Permission:HrDepartment": "Phòng ban",
  "HrDepartment:Code": "Mã phòng ban",
  "HrDepartment:Name": "Tên phòng ban",
  "HrDepartment:Description": "Mô tả",
  "HrDepartment:Status": "Trạng thái",
  "HrDepartment:DeptLevel": "Cấp độ",
  "HrDepartment:Parent": "Phòng ban cha",
  "HrDepartment:Org": "Đơn vị trực thuộc",
  "HrDepartment:Type": "Loại phòng ban",
  "HrDepartment:Partner": "Đối tác",
  "HrDepartment:Province": "Tỉnh/Thành",
  "HrDepartment:Ward": "Phường/Xã",
  "HrDepartment:Address": "Địa chỉ",
  "HrDepartment:FullAddress": "Địa chỉ đầy đủ",
  "HrDepartment:Bank": "Ngân hàng",
  "HrDepartment:BankNo": "Số tài khoản",
  "HrDepartment:CodeRequired": "Mã phòng ban là bắt buộc",
  "HrDepartment:CodeMaxLength": "Mã phòng ban không được vượt quá 25 ký tự",
  "HrDepartment:CodeInvalid": "Mã phòng ban chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
  "HrDepartment:NameRequired": "Tên phòng ban là bắt buộc",
  "HrDepartment:NameMaxLength": "Tên phòng ban không được vượt quá 250 ký tự",
  "HrDepartment:DescriptionMaxLength": "Mô tả không được vượt quá 500 ký tự",
  "HrDepartment:StatusRequired": "Trạng thái là bắt buộc",
  "HrDepartment:DeptLevelRequired": "Cấp độ là bắt buộc",
  "HrDepartment:OrgRequired": "Đơn vị trực thuộc là bắt buộc",
  "HrDepartment:CodeExists": "Mã phòng ban '{Code}' đã tồn tại",
  "HrDepartment:ParentNotFound": "Phòng ban cha không tồn tại",
  "HrDepartment:OrgNotFound": "Đơn vị trực thuộc không tồn tại",
  "HrDepartment:TypeNotFound": "Loại phòng ban không tồn tại",
  "HrDepartment:ProvinceNotFound": "Tỉnh/Thành không tồn tại",
  "HrDepartment:WardNotFound": "Phường/Xã không tồn tại",
  "HrDepartment:BankNotFound": "Ngân hàng không tồn tại",
  "HrDepartment:PartnerTypeDirectNotFound": "Không tìm thấy loại đối tác với mã 'DIRECT'",
  "HrDepartment:CreatedSuccessfully": "Tạo phòng ban thành công",
  "HrDepartment:UpdatedSuccessfully": "Cập nhật phòng ban thành công",
  "HrDepartment:DeletedSuccessfully": "Xóa phòng ban thành công",
  "HrDepartment:New": "Thêm mới phòng ban",
  "HrDepartment:Edit": "Sửa phòng ban",
  "HrDepartment:Delete": "Xóa phòng ban",
  "HrDepartment:DeleteConfirm": "Bạn có chắc chắn muốn xóa phòng ban này?",
  "HrDepartment:CodeCannotBeChanged": "Mã phòng ban không được phép thay đổi",
  "HrDepartment:Active": "Hoạt động",
  "HrDepartment:Deactive": "Không hoạt động",
  "HrDepartment:Unit": "Đơn vị",
  "HrDepartment:Dept": "Phòng/Ban",
  "HrDepartment:SearchByCode": "Tìm theo mã phòng ban",
  "HrDepartment:SearchByName": "Tìm theo tên phòng ban"
}
```

**en.json**: Tương tự với bản dịch tiếng Anh

---

### 7. HTTP API Controllers

#### 7.1. Controller
**Location**: `modules/Hr/src/iOne.Hr.HttpApi/Controllers/HrDepartmentController.cs`

**Yêu cầu**:
- Kế thừa từ `AbpControllerBase`
- Route: `"api/hr/departments"`
- Attributes:
  - `[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]`
  - `[Area(HrRemoteServiceConsts.ModuleName)]`
  - `[Authorize]`
- Methods:
  - `GetAsync(Guid id)` → `[HttpGet("{id}")]` + `[Authorize(HrDepartmentPermissions.View)]`
  - `GetListAsync(GetHrDepartmentsInput input)` → `[HttpGet]` + `[Authorize(HrDepartmentPermissions.View)]`
  - `GetTreeAsync()` → `[HttpGet("tree")]` + `[Authorize(HrDepartmentPermissions.View)]`
  - `GetByParentIdAsync(Guid? parentId)` → `[HttpGet("by-parent/{parentId}")]` + `[Authorize(HrDepartmentPermissions.View)]`
  - `CreateAsync(CreateHrDepartmentDto input)` → `[HttpPost]` + `[Authorize(HrDepartmentPermissions.Create)]`
  - `UpdateAsync(Guid id, UpdateHrDepartmentDto input)` → `[HttpPut("{id}")]` + `[Authorize(HrDepartmentPermissions.Edit)]`
  - `DeleteAsync(Guid id)` → `[HttpDelete("{id}")]` + `[Authorize(HrDepartmentPermissions.Delete)]`

#### 7.2. Exclude từ Conventional Controllers
**Location**: `modules/Hr/src/iOne.Hr.HttpApi/iOneHrHttpApiModule.cs`

**Yêu cầu**:
- Thêm `HrDepartmentAppService` vào `TypePredicate` để exclude khỏi conventional controller generation

---

### 8. Menu Configuration

#### 8.1. Menu Contributor
**Location**: `modules/Hr/src/iOne.Hr.Application/Navigation/HrMenuContributor.cs`

**Yêu cầu**:
- Thêm menu item:
```csharp
hrMenuItem.AddItem(new ApplicationMenuItem(
    "HR.Departments",
    hrL["Menu:HrDepartment"],
    url: "~/pages/hr/departments",
    icon: "pi pi-fw pi-building"
).RequirePermissions(HrDepartmentPermissions.Default));
```

---

## ✅ Checklist Trước Khi Hoàn Thành

### Domain Layer
- [ ] Enum `HrDepartmentStatus.cs` (Active, Deactive)
- [ ] Enum `HrDepartmentLevel.cs` (Unit, Dept)
- [ ] Entity `HrDepartment.cs` với table name `hr_department` (snake_case)
- [ ] Entity có private setters, không có `UpdateCode()` method
- [ ] Entity có navigation properties cho tất cả foreign keys
- [ ] Repository interface `IHrDepartmentRepository.cs` với methods: `IsCodeExistsAsync()`, `GetTreeAsync()`, `GetByParentIdAsync()`, `GetRootDepartmentsAsync()`
- [ ] Manager `HrDepartmentManager.cs` với `CreateAsync()`, `UpdateAsync()`, `CreatePartnerForDepartmentAsync()`
- [ ] Validation Code: Regex `^[A-Z0-9_]+$` (uppercase)

### Application Layer
- [ ] DTOs: `HrDepartmentDto`, `CreateHrDepartmentDto`, `UpdateHrDepartmentDto` (KHÔNG có Code), `GetHrDepartmentsInput`, `HrDepartmentTreeDto`
- [ ] Application Service Interface `IHrDepartmentAppService.cs` với methods: `GetTreeAsync()`, `GetByParentIdAsync()`
- [ ] Application Service Implementation `HrDepartmentAppService.cs`
- [ ] **AutoMapper configuration** đã thêm vào `iOneHrApplicationAutoMapperProfile.cs`
- [ ] `CreateAsync()` implement đúng: Tạo Partner trước → Tạo Department sau
- [ ] `DeleteAsync()` implement đúng: Delete → Save → Update Status → Save

### Permissions
- [ ] Permission Constants `HrDepartmentPermissions.cs`
- [ ] Permission Definition Provider `HrDepartmentPermissionDefinitionProvider.cs`
- [ ] Permissions được đăng ký trong module

### Entity Framework Core
- [ ] Entity Configuration `HrDepartmentConfiguration.cs`
  - [ ] Table name: `"hr_department"` (snake_case)
  - [ ] Column names: snake_case
  - [ ] Status enum conversion: Active → "active", Deactive → "deactive"
  - [ ] DeptLevel enum conversion: Unit → "unit", Dept → "dept"
  - [ ] Unique index: `"ix_hr_department_code"`
  - [ ] Indexes: `"ix_hr_department_parent_id"`, `"ix_hr_department_org_id"`
  - [ ] Foreign keys: Tất cả với snake_case naming
- [ ] Repository Implementation `EfCoreHrDepartmentRepository.cs` với tree methods
- [ ] Configuration đã register trong `iOneDbContext.cs`
- [ ] Repository đã register trong `iOneEntityFrameworkCoreModule.cs`

### Database Migration
- [ ] Migration script với table name `"hr_department"` (snake_case)
- [ ] Column names: snake_case
- [ ] Index names: snake_case với prefix
- [ ] Primary key name: `"pk_hr_department"` (snake_case)
- [ ] Foreign key names: snake_case với prefix `fk_`
- [ ] Sử dụng `IF EXISTS`/`IF NOT EXISTS`
- [ ] Table và column comments

### Localization
- [ ] Localization keys đã thêm vào `vi-VN.json`
- [ ] Localization keys đã thêm vào `en.json`
- [ ] Keys match nhau giữa 2 file

### HTTP API
- [ ] Controller `HrDepartmentController.cs` với đầy đủ endpoints (bao gồm GetTreeAsync, GetByParentIdAsync)
- [ ] Controller có `[Authorize]` attributes với permissions
- [ ] `HrDepartmentAppService` đã exclude khỏi conventional controllers

### Menu
- [ ] Menu item đã thêm vào `HrMenuContributor.cs`
- [ ] Menu có permission check

### Testing
- [ ] Build solution thành công
- [ ] Migration chạy thành công
- [ ] Test API endpoints:
  - [ ] GET `/api/hr/departments` (list)
  - [ ] GET `/api/hr/departments/tree` (tree)
  - [ ] GET `/api/hr/departments/by-parent/{parentId}` (by parent)
  - [ ] GET `/api/hr/departments/{id}` (detail)
  - [ ] POST `/api/hr/departments` (create - tự động tạo Partner)
  - [ ] PUT `/api/hr/departments/{id}` (update - không được sửa Code)
  - [ ] DELETE `/api/hr/departments/{id}` (delete - soft delete + status deactive)
- [ ] Test validation:
  - [ ] Code chỉ chấp nhận A-Z, 0-9, _
  - [ ] Code là unique
  - [ ] Update không được sửa Code
  - [ ] Parent validation
  - [ ] Org validation
  - [ ] Auto create Partner với PartnerType code = "DIRECT"
- [ ] Test permissions
- [ ] Test localization (vi-VN và en)
- [ ] Test menu hiển thị đúng
- [ ] Test tree structure

---

## 🔍 Lưu Ý Quan Trọng

1. **Table và Column Names**: **BẮT BUỘC** sử dụng **snake_case** (PostgreSQL convention)
   - Table: `"hr_department"` (không phải `"HRDEPARTMENT"` hay `"HrDepartment"`)
   - Columns: `"code"`, `"name"`, `"parent_id"`, `"org_id"`, etc. (snake_case)

2. **Code Immutable**: Code không được phép sửa sau khi tạo
   - Entity không có `UpdateCode()` method
   - `UpdateHrDepartmentDto` không có `Code` property
   - Manager `UpdateAsync()` không có `code` parameter

3. **Soft Delete Flow**: 
   - Gọi `Repository.DeleteAsync()` trước để trigger ABP audit log
   - Sau đó mới update status về Deactive
   - Đảm bảo có 2 lần `SaveChangesAsync()` để audit log ghi nhận đúng

4. **Status Enum Conversion**: 
   - Enum: `Active`, `Deactive`
   - Database: `"active"`, `"deactive"` (lowercase string)
   - Sử dụng `HasConversion<string>()` trong Entity Configuration

5. **DeptLevel Enum Conversion**: 
   - Enum: `Unit`, `Dept`
   - Database: `"unit"`, `"dept"` (lowercase string)
   - Sử dụng `HasConversion<string>()` trong Entity Configuration

6. **Code Validation**: 
   - Regex: `^[A-Z0-9_]+$`
   - Tự động convert sang uppercase trong Entity constructor

7. **Auto Create Partner**: 
   - Khi tạo Department, tự động tạo Partner
   - Partner có PartnerType với code = "DIRECT"
   - Partner được tạo với thông tin từ Department (Name, Address, Province, Ward, etc.)
   - PartnerId được set vào Department

8. **Self-Referencing Relationships**: 
   - `ParentId` → `HrDepartment` (optional)
   - `OrgId` → `HrDepartment` (required)
   - Cần validate parent và org tồn tại
   - Cần tránh circular reference (parent không thể là chính nó hoặc con của nó)

9. **Tree Structure**: 
   - Hỗ trợ query tree với recursive CTE hoặc recursive loading
   - Method `GetTreeAsync()` trả về danh sách dạng tree
   - Method `GetByParentIdAsync()` trả về các phòng ban con

10. **AutoMapper**: **BẮT BUỘC** thêm mappings vào `iOneHrApplicationAutoMapperProfile.cs`

11. **Conventional Controllers**: Phải exclude `HrDepartmentAppService` khỏi conventional controller generation

12. **Foreign Key Constraints**: 
    - Tất cả foreign keys có `ON DELETE RESTRICT` và `ON UPDATE RESTRICT`
    - Đảm bảo không xóa được parent/organization nếu có department con
    - Self-referencing: `parent_id` và `org_id` đều reference đến `hr_department(id)`

13. **Auto Create Partner Logic**:
    - Tìm ResPartnerType với code = "DIRECT": `await partnerTypeRepository.FirstOrDefaultAsync(x => x.Code == "DIRECT")`
    - Nếu không tìm thấy: throw `BusinessException("Hr:HrDepartment:PartnerTypeDirectNotFound")`
    - Load Province và Ward để compute FullAddress (nếu có)
    - Nếu không có Province/Ward: FullAddress = Address (hoặc empty string)
    - Tạo ResPartner với thông tin từ Department:
      - PartnerTypeId = ResPartnerType có code = "DIRECT"
      - Name = department.Name
      - Address = department.Address ?? "" (required field, nếu null thì dùng empty string)
      - FullAddress = computed FullAddress (required field)
    - ProvinceId = input.ProvinceId ?? defaultProvinceId (required field - **QUAN TRỌNG**: Cần có default ProvinceId hoặc validate input phải có ProvinceId)
    - WardId = input.WardId ?? defaultWardId (required field - **QUAN TRỌNG**: Cần có default WardId hoặc validate input phải có WardId)
    - Phone = "" hoặc default phone value (required field - có thể set default như "0000000000")
      - Status = ResPartnerStatus.Active
      - Code = null (optional)
    - PartnerId được set vào Department sau khi Partner được tạo
    - **Lưu ý**: Partner.ProvinceId và Partner.WardId là required, nên cần có default values hoặc validate Department phải có Province/Ward

14. **Tree Structure Implementation**:
    - Method `GetTreeAsync()`: Load tất cả departments active, build tree trong memory
    - Method `GetByParentIdAsync()`: Query trực tiếp từ database với filter ParentId
    - Tree DTO có structure: `{ Id, Code, Name, Status, Children: List<HrDepartmentTreeDto> }`
    - Recursive build tree: Group by ParentId, build nested structure

15. **Circular Reference Prevention**:
    - Validate ParentId không thể là chính nó (khi update)
    - Validate ParentId không thể là con của nó (recursive check - check tất cả descendants)
    - Validate OrgId không thể là chính nó (khi update)
    - Validate OrgId không thể là con của nó (recursive check)
    - Method helper: `IsDescendantOfAsync(Guid departmentId, Guid ancestorId)` để check recursive

16. **Required Fields Validation**:
    - **Department Fields**:
      - Code: Required, max 25, format A-Z, 0-9, _
      - Name: Required, max 250
      - Status: Required
      - DeptLevel: Required
      - OrgId: Required (phải tồn tại trong database)
      - PartnerId: Required (tự động tạo, không cần validate trong DTO)
      - ParentId: Optional (nhưng nếu có thì phải tồn tại)
      - TypeId: Optional (nhưng nếu có thì phải tồn tại)
      - ProvinceId, WardId: **Optional trong Department** (nhưng **BẮT BUỘC khi tạo Partner**)
      - Address, FullAddress: Optional
      - BankId: Optional (nhưng nếu có thì phải tồn tại)
      - BankNo: Optional, max 50
    - **Partner Fields** (khi auto create):
      - ProvinceId: **Required** (lấy từ Department.ProvinceId hoặc default)
      - WardId: **Required** (lấy từ Department.WardId hoặc default)
      - Address: Required (lấy từ Department.Address hoặc empty string)
      - FullAddress: Required (computed)
      - Phone: Required (default value nếu không có)

17. **Tree Query Performance**:
    - Method `GetTreeAsync()` có thể load tất cả departments active rồi build tree trong memory
    - Hoặc sử dụng recursive CTE trong PostgreSQL để query tree trực tiếp
    - Nên cache tree structure nếu data không thay đổi thường xuyên

---

## 📝 So Sánh với DLL Hiện Tại

**DLL Hiện Tại**:
- Table: `HRDEPARTMENT` (UPPERCASE)
- Columns: `ID`, `PARENTID`, `PARTNERID`, `TYPEID`, `CODE`, `NAME`, `DESCRIPTION`, `STATUS`, `PROVINCEID`, `WARDID`, `ADDRESS`, `FULLADDRESS`, `BANKID`, `BANKNO`, `ORGID`, `DEPTLEVEL` (UPPERCASE)
- Primary Key: `PK_HRDEPARTMENT` (UPPERCASE)
- ID Type: `NUMERIC(10)` (trong DLL) → `UUID` (trong implementation mới)

**Implementation Mới**:
- Table: `hr_department` (snake_case)
- Columns: `id`, `code`, `name`, `description`, `status`, `dept_level`, `parent_id`, `org_id`, `type_id`, `partner_id`, `province_id`, `ward_id`, `address`, `full_address`, `bank_id`, `bank_no` (snake_case)
- Primary Key: `pk_hr_department` (snake_case với prefix)
- ID Type: `UUID` (Guid trong C#)

**Lưu ý**: 
- DLL sử dụng `NUMERIC(10)` cho ID, nhưng implementation mới sử dụng `UUID` (Guid) để phù hợp với ABP convention
- Cần migration script để convert từ DLL hiện tại sang cấu trúc mới (nếu có dữ liệu cũ)
- **QUAN TRỌNG**: Khi migrate từ DLL cũ, cần map `NUMERIC(10)` ID sang `UUID` cho tất cả foreign keys
- DLL có field `MANAGERID` trong comment nhưng không có trong CREATE TABLE → Có thể bỏ qua hoặc thêm sau

---

## 🚀 Thứ Tự Thực Hiện

1. **Domain Layer** (Enum Status, Enum Level, Entity, Repository Interface, Manager)
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

- Entity mẫu: `src/common/domain/iOne.Domain/HrDepartmentTypes/HrDepartmentType.cs`
- AppService mẫu: `modules/Hr/src/iOne.Hr.Application/HrDepartmentTypes/HrDepartmentTypeAppService.cs`
- Configuration mẫu: `src/common/infra/iOne.EntityFrameworkCore/HrDepartmentTypes/HrDepartmentTypeConfiguration.cs`
- ResPartner mẫu: `src/common/domain/iOne.Domain/ResPartners/ResPartner.cs`
- Rules: `dev_note/RULES_BACKEND_DEVELOPMENT.md`

