# Plan Xây Dựng Backend: Quản lý Nhân viên (HrEmployee)

## 📋 Tổng Quan

**Module**: Hr (Quản lý nhân viên)  
**Entity**: HrEmployee (Nhân viên)  
**Table Name**: `hr_employee` (snake_case - PostgreSQL convention)  
**Related Entity**: HrEmployeeRoleRel (Vai trò nhân viên)  
**Related Table**: `hr_employee_role_rel` (snake_case - PostgreSQL convention)

---

## 🎯 Yêu Cầu Chức Năng

### HrEmployee
1. ✅ **CRUD Operations**: Tìm kiếm, Thêm, Sửa, Xóa
2. ✅ **Validation Code**: Mã nhân viên chỉ cho phép A-Z, _, 0-9 (uppercase)
3. ✅ **Code Immutable**: Khi sửa không được phép sửa mã nhân viên (disable trên UI)
4. ✅ **Code Unique**: Mã nhân viên là duy nhất
5. ✅ **Partner Selection**: Cho phép chọn đối tác (res_partner.id) khi tạo
6. ✅ **Organization Selection**: Cho phép chọn đơn vị (phòng ban có cấp là đơn vị - DeptLevel = Unit)
7. ✅ **Department Selection**: Cho phép chọn phòng ban khi tạo
8. ✅ **User Mapping**: Cho phép chọn user tương ứng với nhân viên (IdentityUser.Id)
9. ✅ **Soft Delete**: Khi xóa → soft delete (ABP audit log) → cập nhật status về Deactive
10. ✅ **Tree View Filter**: Danh sách có tree phòng ban bên trái, bảng nhân viên bên phải, click node hiển thị nhân viên thuộc phòng ban
11. ✅ **Đa ngôn ngữ**: vi-VN và en
12. ✅ **Phân quyền**: View, Create, Edit, Delete

### HrEmployeeRoleRel
1. ✅ **Role Assignment**: Gán vai trò cho nhân viên
2. ✅ **Date Validation**: Cùng vai trò phải có ngày hiệu lực và hết hạn không overlap nhau
3. ✅ **CRUD Operations**: Thêm, Sửa, Xóa vai trò cho nhân viên

---

## 📁 Cấu Trúc Files Cần Tạo

### 1. Domain Layer

#### 1.1. Enum Status
**Location**: `src/common/domain/iOne.Domain.Shared/HrEmployees/HrEmployeeStatus.cs`

**Yêu cầu**:
```csharp
namespace iOne.HrEmployees;

public enum HrEmployeeStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1  // Không hoạt động
}
```

**Lưu ý**: 
- Enum values sẽ được convert sang string trong database: "active", "deactive"
- MaxLength(10) trong EF Core configuration

---

#### 1.2. Entity: HrEmployee
**Location**: `src/common/domain/iOne.Domain/HrEmployees/HrEmployee.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>`
- Table name: `[Table("hr_employee")]` (snake_case)
- Properties:
  - `Code` (string, max 50, required, private setter, immutable)
  - `FullName` (string, max 50, required, private setter)
  - `Status` (HrEmployeeStatus enum, required, private setter)
  - `PositionId` (Guid?, optional, FK to HrEmployeePosition)
  - `LevelId` (Guid?, optional, FK to HrEmployeeLevel)
  - `PartnerId` (Guid?, optional, FK to ResPartner)
  - `OrgId` (Guid?, optional, FK to HrDepartment where DeptLevel = Unit)
  - `DepartmentId` (Guid, required, FK to HrDepartment)
  - `IsManager` (bool, optional - đánh dấu có phải lãnh đạo đơn vị không)
  - `ManagerId` (Guid?, optional, FK to HrEmployee - self-reference)
  - `ProvinceId` (Guid?, optional, FK to ResProvince)
  - `WardId` (Guid?, optional, FK to ResWard)
  - `Address` (string, max 250, optional)
  - `FullAddress` (string, max 500, optional)
  - `Phone` (string, max 15, optional)
  - `Email` (string, max 50, optional)
  - `UserId` (Guid?, optional, FK to IdentityUser)
- Constructor với validation
- Private setters với methods: `SetCode()`, `SetFullName()`, etc.
- Public methods: `UpdateFullName()`, `UpdateStatus()`, etc. (KHÔNG có `UpdateCode()`)
- Validation Code: Regex `^[A-Z0-9_]+$` (uppercase)
- Navigation properties:
  - `Position` (HrEmployeePosition?)
  - `Level` (HrEmployeeLevel?)
  - `Partner` (ResPartner?)
  - `Org` (HrDepartment?)
  - `Department` (HrDepartment)
  - `Manager` (HrEmployee?)
  - `Province` (ResProvince?)
  - `Ward` (ResWard?)
  - `User` (IdentityUser?)
  - `Roles` (ICollection<HrEmployeeRoleRel>)
  - `ManagedEmployees` (ICollection<HrEmployee>) - nhân viên được quản lý

**Lưu ý**:
- `IsManager` trong DDL là VARCHAR(1) với giá trị "Y"/"N", nhưng trong Entity nên dùng `bool` để dễ xử lý
- `UserId` trong DDL là VARCHAR(10), nhưng IdentityUser.Id là Guid, nên dùng `Guid?`

---

#### 1.3. Entity: HrEmployeeRoleRel
**Location**: `src/common/domain/iOne.Domain/HrEmployees/HrEmployeeRoleRel.cs`

**Yêu cầu**:
- Kế thừa từ `AuditedEntity<Guid>` (không phải FullAudited vì không có soft delete)
- Table name: `[Table("hr_employee_role_rel")]` (snake_case)
- Properties:
  - `EmployeeId` (Guid, required, FK to HrEmployee)
  - `RoleId` (Guid, required, FK to HrEmployeeRole)
  - `EffectDate` (DateTime, required - ngày hiệu lực)
  - `ExpireDate` (DateTime?, optional - ngày hết hạn)
- Constructor với validation
- Public methods: `UpdateEffectDate()`, `UpdateExpireDate()`
- Validation: Date overlap check (cùng EmployeeId + RoleId không được overlap)
- Navigation properties:
  - `Employee` (HrEmployee)
  - `Role` (HrEmployeeRole)

**Lưu ý**:
- Không có soft delete (IsDeleted), chỉ có audit fields (CreationTime, CreatorId, LastModificationTime, LastModifierId)
- Date overlap validation: Cùng vai trò (RoleId) của cùng nhân viên (EmployeeId) không được có ngày overlap

---

#### 1.4. Repository Interface: IHrEmployeeRepository
**Location**: `src/common/domain/iOne.Domain/HrEmployees/IHrEmployeeRepository.cs`

**Yêu cầu**:
- Kế thừa từ `IRepository<HrEmployee, Guid>`
- Methods:
  - `IsCodeExistsAsync(string code, Guid? excludeId = null)`: Check code uniqueness
  - `GetByDepartmentIdAsync(Guid departmentId)`: Lấy danh sách nhân viên theo phòng ban
  - `GetByOrgIdAsync(Guid orgId)`: Lấy danh sách nhân viên theo đơn vị

---

#### 1.5. Repository Interface: IHrEmployeeRoleRelRepository
**Location**: `src/common/domain/iOne.Domain/HrEmployees/IHrEmployeeRoleRelRepository.cs`

**Yêu cầu**:
- Kế thừa từ `IRepository<HrEmployeeRoleRel, Guid>`
- Methods:
  - `GetByEmployeeIdAsync(Guid employeeId)`: Lấy danh sách vai trò của nhân viên
  - `HasOverlappingDatesAsync(Guid employeeId, Guid roleId, DateTime effectDate, DateTime? expireDate, Guid? excludeId = null)`: Check date overlap

---

#### 1.6. Manager: HrEmployeeManager
**Location**: `src/common/domain/iOne.Domain/HrEmployees/HrEmployeeManager.cs`

**Yêu cầu**:
- Inject: `IHrEmployeeRepository`, `IHrEmployeeRoleRelRepository`
- Methods:
  - `CreateAsync(HrEmployee employee)`: Validate code uniqueness
  - `UpdateAsync(...)`: Validate (KHÔNG có UpdateCode)
  - `DeleteAsync(HrEmployee employee)`: Soft delete → set status = Deactive

---

#### 1.7. Manager: HrEmployeeRoleRelManager
**Location**: `src/common/domain/iOne.Domain/HrEmployees/HrEmployeeRoleRelManager.cs`

**Yêu cầu**:
- Inject: `IHrEmployeeRoleRelRepository`
- Methods:
  - `CreateAsync(HrEmployeeRoleRel roleRel)`: Validate date overlap
  - `UpdateAsync(HrEmployeeRoleRel roleRel, DateTime effectDate, DateTime? expireDate)`: Validate date overlap
  - `ValidateDateOverlapAsync(Guid employeeId, Guid roleId, DateTime effectDate, DateTime? expireDate, Guid? excludeId = null)`: Check overlap

**Date Overlap Logic**:
```
Có overlap nếu:
- (newEffectDate <= existingExpireDate || existingExpireDate == null) 
  AND 
  (newExpireDate >= existingEffectDate || newExpireDate == null)
```

---

### 2. Entity Framework Core

#### 2.1. Configuration: HrEmployeeConfiguration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrEmployees/HrEmployeeConfiguration.cs`

**Yêu cầu**:
- Table name: `"hr_employee"` (snake_case)
- Column names: snake_case (code, full_name, status, position_id, level_id, partner_id, org_id, department_id, is_manager, manager_id, province_id, ward_id, address, full_address, phone, email, user_id)
- Audit columns: snake_case (creation_time, creator_id, last_modification_time, last_modifier_id, deletion_time, deleter_id, is_deleted, concurrency_stamp, tenant_id)
- Indexes:
  - `ix_hr_employee_code` (unique)
  - `ix_hr_employee_department_id`
  - `ix_hr_employee_org_id`
  - `ix_hr_employee_manager_id`
- Foreign keys:
  - `fk_hr_employee_position_id` → `hr_employee_position(id)`
  - `fk_hr_employee_level_id` → `hr_employee_level(id)`
  - `fk_hr_employee_partner_id` → `res_partner(id)`
  - `fk_hr_employee_org_id` → `hr_department(id)` (where dept_level = 'unit')
  - `fk_hr_employee_department_id` → `hr_department(id)`
  - `fk_hr_employee_manager_id` → `hr_employee(id)`
  - `fk_hr_employee_province_id` → `res_province(id)`
  - `fk_hr_employee_ward_id` → `res_ward(id)`
  - `fk_hr_employee_user_id` → `AbpUsers(id)` (IdentityUser)
- Enum conversion: Status → string (lowercase)

**Lưu ý**:
- `IsManager`: Convert bool → string "Y"/"N" trong database (hoặc dùng bool trực tiếp nếu PostgreSQL hỗ trợ)
- `UserId`: FK đến `AbpUsers` table (IdentityUser)

---

#### 2.2. Configuration: HrEmployeeRoleRelConfiguration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrEmployees/HrEmployeeRoleRelConfiguration.cs`

**Yêu cầu**:
- Table name: `"hr_employee_role_rel"` (snake_case)
- Column names: snake_case (id, employee_id, role_id, effect_date, expire_date, creation_time, creator_id, last_modification_time, last_modifier_id)
- Audit columns: snake_case (creation_time, creator_id, last_modification_time, last_modifier_id) - KHÔNG có soft delete
- Indexes:
  - `ix_hr_employee_role_rel_employee_id`
  - `ix_hr_employee_role_rel_role_id`
  - `ix_hr_employee_role_rel_employee_role` (composite: employee_id + role_id)
- Foreign keys:
  - `fk_hr_employee_role_rel_employee_id` → `hr_employee(id)`
  - `fk_hr_employee_role_rel_role_id` → `hr_employee_role(id)`

---

#### 2.3. Repository Implementation: EfCoreHrEmployeeRepository
**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrEmployees/EfCoreHrEmployeeRepository.cs`

**Yêu cầu**:
- Kế thừa từ `EfCoreRepository<iOneDbContext, HrEmployee, Guid>`
- Implement `IHrEmployeeRepository`
- Implement custom methods

---

#### 2.4. Repository Implementation: EfCoreHrEmployeeRoleRelRepository
**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrEmployees/EfCoreHrEmployeeRoleRelRepository.cs`

**Yêu cầu**:
- Kế thừa từ `EfCoreRepository<iOneDbContext, HrEmployeeRoleRel, Guid>`
- Implement `IHrEmployeeRoleRelRepository`
- Implement custom methods

---

#### 2.5. Register trong DbContext
**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`

**Yêu cầu**:
- Thêm `DbSet<HrEmployee> Employees { get; set; }`
- Thêm `DbSet<HrEmployeeRoleRel> EmployeeRoleRels { get; set; }`

---

#### 2.6. Register trong Module
**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneEntityFrameworkCoreModule.cs`

**Yêu cầu**:
- Đăng ký repository implementations

---

### 3. Application Layer

#### 3.1. DTOs: HrEmployeeDto
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployees/HrEmployeeDto.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedEntityDto<Guid>`
- Properties: Tất cả fields của Entity + navigation property names (PositionName, LevelName, PartnerName, OrgName, DepartmentName, ManagerName, ProvinceName, WardName, UserName)

---

#### 3.2. DTOs: CreateHrEmployeeDto
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployees/CreateHrEmployeeDto.cs`

**Yêu cầu**:
- Properties: Tất cả fields cần thiết để tạo mới (KHÔNG có Id, Code có thể optional nếu auto-generate)
- Validation attributes: `[Required]`, `[MaxLength]`, etc.

---

#### 3.3. DTOs: UpdateHrEmployeeDto
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployees/UpdateHrEmployeeDto.cs`

**Yêu cầu**:
- Properties: Tất cả fields có thể update (KHÔNG có Code, KHÔNG có Id)
- Validation attributes

---

#### 3.4. DTOs: GetHrEmployeesInput
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployees/GetHrEmployeesInput.cs`

**Yêu cầu**:
- Kế thừa từ `PagedAndSortedResultRequestDto`
- Properties:
  - `Code` (string?, optional)
  - `FullName` (string?, optional)
  - `Status` (HrEmployeeStatus?, optional)
  - `DepartmentId` (Guid?, optional) - filter theo phòng ban
  - `OrgId` (Guid?, optional) - filter theo đơn vị
  - `PartnerId` (Guid?, optional)
  - `PositionId` (Guid?, optional)
  - `LevelId` (Guid?, optional)

---

#### 3.5. DTOs: HrEmployeeRoleRelDto
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployees/HrEmployeeRoleRelDto.cs`

**Yêu cầu**:
- Kế thừa từ `AuditedEntityDto<Guid>` (không phải FullAudited)
- Properties: EmployeeId, RoleId, EffectDate, ExpireDate + RoleName

---

#### 3.6. DTOs: CreateHrEmployeeRoleRelDto
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployees/CreateHrEmployeeRoleRelDto.cs`

**Yêu cầu**:
- Properties: EmployeeId, RoleId, EffectDate, ExpireDate

---

#### 3.7. DTOs: UpdateHrEmployeeRoleRelDto
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployees/UpdateHrEmployeeRoleRelDto.cs`

**Yêu cầu**:
- Properties: RoleId, EffectDate, ExpireDate (KHÔNG có EmployeeId)

---

#### 3.8. Application Service Interface: IHrEmployeeAppService
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployees/IHrEmployeeAppService.cs`

**Yêu cầu**:
- Kế thừa từ `ICrudAppService<HrEmployeeDto, Guid, GetHrEmployeesInput, CreateHrEmployeeDto, UpdateHrEmployeeDto>`
- Methods:
  - `GetListAsync(GetHrEmployeesInput input)`: Lấy danh sách có filter
  - `GetAsync(Guid id)`: Lấy chi tiết
  - `CreateAsync(CreateHrEmployeeDto input)`: Tạo mới
  - `UpdateAsync(Guid id, UpdateHrEmployeeDto input)`: Cập nhật
  - `DeleteAsync(Guid id)`: Xóa (soft delete + set status = Deactive)
  - `GetByDepartmentIdAsync(Guid departmentId)`: Lấy danh sách theo phòng ban (cho tree view)
  - `GetRolesAsync(Guid employeeId)`: Lấy danh sách vai trò của nhân viên
  - `AddRoleAsync(Guid employeeId, CreateHrEmployeeRoleRelDto input)`: Thêm vai trò
  - `UpdateRoleAsync(Guid employeeId, Guid roleRelId, UpdateHrEmployeeRoleRelDto input)`: Cập nhật vai trò
  - `RemoveRoleAsync(Guid employeeId, Guid roleRelId)`: Xóa vai trò

---

#### 3.9. Application Service Implementation: HrEmployeeAppService
**Location**: `modules/hr/src/iOne.Hr.Application/HrEmployees/HrEmployeeAppService.cs`

**Yêu cầu**:
- Kế thừa từ `CrudAppService<HrEmployee, HrEmployeeDto, Guid, GetHrEmployeesInput, CreateHrEmployeeDto, UpdateHrEmployeeDto>`
- Inject: `HrEmployeeManager`, `HrEmployeeRoleRelManager`, `IHrEmployeeRepository`, `IHrEmployeeRoleRelRepository`
- Override methods:
  - `CreateAsync`: Validate code uniqueness, tạo entity
  - `UpdateAsync`: Validate (KHÔNG cho update Code), cập nhật entity
  - `DeleteAsync`: Soft delete → set status = Deactive
  - `GetListAsync`: Filter theo DepartmentId nếu có
- Implement custom methods:
  - `GetByDepartmentIdAsync`: Lấy danh sách theo phòng ban
  - `GetRolesAsync`: Lấy danh sách vai trò
  - `AddRoleAsync`: Thêm vai trò với date overlap validation
  - `UpdateRoleAsync`: Cập nhật vai trò với date overlap validation
  - `RemoveRoleAsync`: Xóa vai trò

**Lưu ý**:
- Eager load navigation properties để hiển thị tên trong DTO
- Map navigation properties sang DTO (PositionName, LevelName, etc.)

---

#### 3.10. AutoMapper Configuration
**Location**: `modules/hr/src/iOne.Hr.Application/iOneHrApplicationAutoMapperProfile.cs`

**Yêu cầu**:
- Mapping `HrEmployee` → `HrEmployeeDto`
- Mapping `CreateHrEmployeeDto` → `HrEmployee`
- Mapping `UpdateHrEmployeeDto` → `HrEmployee`
- Mapping `HrEmployeeRoleRel` → `HrEmployeeRoleRelDto`
- Mapping `CreateHrEmployeeRoleRelDto` → `HrEmployeeRoleRel`
- Mapping `UpdateHrEmployeeRoleRelDto` → `HrEmployeeRoleRel`

---

### 4. Permissions

#### 4.1. Permission Constants
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrEmployeePermissions.cs`

**Yêu cầu**:
```csharp
namespace iOne.Hr.Permissions;

public static class HrEmployeePermissions
{
    public const string GroupName = "HrEmployee";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

---

#### 4.2. Permission Definition Provider
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrEmployeePermissionDefinitionProvider.cs`

**Yêu cầu**:
- Kế thừa từ `PermissionDefinitionProvider`
- Tạo permission group với `GroupName = "HrEmployee"`
- Tạo Default permission = `"HrEmployee"`
- Thêm các child permissions (Create, Edit, Delete, View)

---

### 5. Localization

#### 5.1. Localization Files
**Location**: 
- `modules/hr/src/iOne.Hr.Application.Contracts/Localization/Hr/vi-VN.json`
- `modules/hr/src/iOne.Hr.Application.Contracts/Localization/Hr/en.json`

**Yêu cầu**:
- Keys cho HrEmployee:
  - `HrEmployee:Code`, `HrEmployee:FullName`, `HrEmployee:Status`, etc.
  - `HrEmployee:CodeRequired`, `HrEmployee:CodeMaxLength`, `HrEmployee:CodeInvalidFormat`, `HrEmployee:CodeExists`
  - `HrEmployee:FullNameRequired`, `HrEmployee:FullNameMaxLength`
  - `HrEmployee:CreatedSuccessfully`, `HrEmployee:UpdatedSuccessfully`, `HrEmployee:DeletedSuccessfully`
  - `HrEmployee:CodeCannotBeChanged`
- Keys cho HrEmployeeRoleRel:
  - `HrEmployeeRoleRel:RoleId`, `HrEmployeeRoleRel:EffectDate`, `HrEmployeeRoleRel:ExpireDate`
  - `HrEmployeeRoleRel:DateOverlap`
- Menu keys:
  - `Menu:Employees`
- Permission keys:
  - `Permission:HrEmployee`

---

### 6. HTTP API Controllers

#### 6.1. Controller: HrEmployeeController
**Location**: `modules/hr/src/iOne.Hr.HttpApi/Controllers/HrEmployeeController.cs`

**Yêu cầu**:
- Kế thừa từ `AbpControllerBase`
- Route: `"api/hr/employees"`
- Methods:
  - `GetListAsync(GetHrEmployeesInput input)`: `[HttpGet]`, `[Authorize(HrEmployeePermissions.View)]`
  - `GetAsync(Guid id)`: `[HttpGet("{id}")]`, `[Authorize(HrEmployeePermissions.View)]`
  - `CreateAsync(CreateHrEmployeeDto input)`: `[HttpPost]`, `[Authorize(HrEmployeePermissions.Create)]`
  - `UpdateAsync(Guid id, UpdateHrEmployeeDto input)`: `[HttpPut("{id}")]`, `[Authorize(HrEmployeePermissions.Edit)]`
  - `DeleteAsync(Guid id)`: `[HttpDelete("{id}")]`, `[Authorize(HrEmployeePermissions.Delete)]`
  - `GetByDepartmentIdAsync(Guid departmentId)`: `[HttpGet("by-department/{departmentId}")]`, `[Authorize(HrEmployeePermissions.View)]`
  - `GetRolesAsync(Guid employeeId)`: `[HttpGet("{employeeId}/roles")]`, `[Authorize(HrEmployeePermissions.View)]`
  - `AddRoleAsync(Guid employeeId, CreateHrEmployeeRoleRelDto input)`: `[HttpPost("{employeeId}/roles")]`, `[Authorize(HrEmployeePermissions.Edit)]`
  - `UpdateRoleAsync(Guid employeeId, Guid roleRelId, UpdateHrEmployeeRoleRelDto input)`: `[HttpPut("{employeeId}/roles/{roleRelId}")]`, `[Authorize(HrEmployeePermissions.Edit)]`
  - `RemoveRoleAsync(Guid employeeId, Guid roleRelId)`: `[HttpDelete("{employeeId}/roles/{roleRelId}")]`, `[Authorize(HrEmployeePermissions.Edit)]`

---

#### 6.2. Exclude từ Conventional Controllers
**Location**: `modules/hr/src/iOne.Hr.HttpApi/iOneHrHttpApiModule.cs`

**Yêu cầu**:
- Exclude `HrEmployeeAppService` khỏi conventional controller generation

---

### 7. Menu Configuration

#### 7.1. Menu Contributor
**Location**: `modules/hr/src/iOne.Hr.Application/Navigation/HrMenuContributor.cs`

**Yêu cầu**:
- Thêm menu item "Employees" vào HR menu
- URL: `"~/pages/hr/employees"`
- Permission: `HrEmployeePermissions.Default`

---

## 🔍 Business Rules Chi Tiết

### 1. Code Validation
- Format: Chỉ cho phép A-Z, _, 0-9 (uppercase)
- Regex: `^[A-Z0-9_]+$`
- Unique: Mã nhân viên là duy nhất
- Immutable: Khi update, không được phép thay đổi Code

### 2. Soft Delete
- Khi xóa nhân viên:
  1. Gọi `Repository.DeleteAsync(entity)` → ABP soft delete (set IsDeleted = true, DeletionTime, DeleterId)
  2. Sau đó gọi `entity.UpdateStatus(HrEmployeeStatus.Deactive)` → set Status = Deactive
  3. Gọi `Repository.UpdateAsync(entity)` → lưu status

### 3. Organization Selection
- `OrgId` chỉ cho phép chọn phòng ban có `DeptLevel = Unit` (đơn vị)
- Validation trong Application Service: Check `HrDepartment.DeptLevel == HrDepartmentLevel.Unit`

### 4. Department Selection
- `DepartmentId` là required
- Có thể chọn bất kỳ phòng ban nào (Unit hoặc Dept)

### 5. Tree View Filter
- API `GetByDepartmentIdAsync(Guid departmentId)` trả về danh sách nhân viên thuộc phòng ban
- Frontend sẽ gọi API này khi click vào node trong tree

### 6. Role Assignment - Date Overlap Validation
- Cùng nhân viên (EmployeeId) + cùng vai trò (RoleId) không được có ngày overlap
- Logic overlap:
  ```
  Có overlap nếu:
  - (newEffectDate <= existingExpireDate || existingExpireDate == null) 
    AND 
    (newExpireDate >= existingEffectDate || newExpireDate == null)
  ```
- Validation trong `HrEmployeeRoleRelManager.CreateAsync()` và `UpdateAsync()`

### 7. User Mapping
- `UserId` là optional
- FK đến `IdentityUser.Id` (Guid)
- Có thể null nếu nhân viên chưa có user account

---

## 📝 Migration Script

### Table: hr_employee
```sql
CREATE TABLE IF NOT EXISTS hr_employee (
    id UUID NOT NULL,
    code VARCHAR(50) NOT NULL,
    full_name VARCHAR(50) NOT NULL,
    status VARCHAR(10) NOT NULL,
    position_id UUID NULL,
    level_id UUID NULL,
    partner_id UUID NULL,
    org_id UUID NULL,
    department_id UUID NOT NULL,
    is_manager BOOLEAN NULL,
    manager_id UUID NULL,
    province_id UUID NULL,
    ward_id UUID NULL,
    address VARCHAR(250) NULL,
    full_address VARCHAR(500) NULL,
    phone VARCHAR(15) NULL,
    email VARCHAR(50) NULL,
    user_id UUID NULL,
    creation_time TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    creator_id UUID NULL,
    last_modification_time TIMESTAMP WITHOUT TIME ZONE NULL,
    last_modifier_id UUID NULL,
    deletion_time TIMESTAMP WITHOUT TIME ZONE NULL,
    deleter_id UUID NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    concurrency_stamp VARCHAR(40) NULL,
    tenant_id UUID NULL,
    CONSTRAINT pk_hr_employee PRIMARY KEY (id)
);

COMMENT ON TABLE hr_employee IS 'Bảng định nghĩa nhân viên';
COMMENT ON COLUMN hr_employee.position_id IS 'Liên kết chức danh';
COMMENT ON COLUMN hr_employee.level_id IS 'Liên kết cấp bậc';
COMMENT ON COLUMN hr_employee.org_id IS 'ID đơn vị';
COMMENT ON COLUMN hr_employee.is_manager IS 'Đánh dấu có phải lãnh đạo đơn vị không: Y: có, N: không';
COMMENT ON COLUMN hr_employee.manager_id IS 'Người quản lý trực tiếp';
COMMENT ON COLUMN hr_employee.status IS 'Trạng thái: active: Hoạt động, deactive: Không hoạt động';
COMMENT ON COLUMN hr_employee.user_id IS 'ID liên kết với 1 user login';

CREATE UNIQUE INDEX IF NOT EXISTS ix_hr_employee_code ON hr_employee(code);
CREATE INDEX IF NOT EXISTS ix_hr_employee_department_id ON hr_employee(department_id);
CREATE INDEX IF NOT EXISTS ix_hr_employee_org_id ON hr_employee(org_id);
CREATE INDEX IF NOT EXISTS ix_hr_employee_manager_id ON hr_employee(manager_id);

ALTER TABLE hr_employee
    ADD CONSTRAINT fk_hr_employee_position_id FOREIGN KEY (position_id) REFERENCES hr_employee_position(id) ON DELETE RESTRICT ON UPDATE RESTRICT;
ALTER TABLE hr_employee
    ADD CONSTRAINT fk_hr_employee_level_id FOREIGN KEY (level_id) REFERENCES hr_employee_level(id) ON DELETE RESTRICT ON UPDATE RESTRICT;
ALTER TABLE hr_employee
    ADD CONSTRAINT fk_hr_employee_partner_id FOREIGN KEY (partner_id) REFERENCES res_partner(id) ON DELETE RESTRICT ON UPDATE RESTRICT;
ALTER TABLE hr_employee
    ADD CONSTRAINT fk_hr_employee_org_id FOREIGN KEY (org_id) REFERENCES hr_department(id) ON DELETE RESTRICT ON UPDATE RESTRICT;
ALTER TABLE hr_employee
    ADD CONSTRAINT fk_hr_employee_department_id FOREIGN KEY (department_id) REFERENCES hr_department(id) ON DELETE RESTRICT ON UPDATE RESTRICT;
ALTER TABLE hr_employee
    ADD CONSTRAINT fk_hr_employee_manager_id FOREIGN KEY (manager_id) REFERENCES hr_employee(id) ON DELETE RESTRICT ON UPDATE RESTRICT;
ALTER TABLE hr_employee
    ADD CONSTRAINT fk_hr_employee_province_id FOREIGN KEY (province_id) REFERENCES res_province(id) ON DELETE RESTRICT ON UPDATE RESTRICT;
ALTER TABLE hr_employee
    ADD CONSTRAINT fk_hr_employee_ward_id FOREIGN KEY (ward_id) REFERENCES res_ward(id) ON DELETE RESTRICT ON UPDATE RESTRICT;
ALTER TABLE hr_employee
    ADD CONSTRAINT fk_hr_employee_user_id FOREIGN KEY (user_id) REFERENCES AbpUsers(id) ON DELETE RESTRICT ON UPDATE RESTRICT;
```

### Table: hr_employee_role_rel
```sql
CREATE TABLE IF NOT EXISTS hr_employee_role_rel (
    id UUID NOT NULL,
    employee_id UUID NOT NULL,
    role_id UUID NOT NULL,
    effect_date DATE NOT NULL,
    expire_date DATE NULL,
    creation_time TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    creator_id UUID NOT NULL,
    last_modification_time TIMESTAMP WITHOUT TIME ZONE NULL,
    last_modifier_id UUID NULL,
    CONSTRAINT pk_hr_employee_role_rel PRIMARY KEY (id)
);

COMMENT ON TABLE hr_employee_role_rel IS 'Bảng lưu các vai trò của nhân viên, một nhân viên có thể có nhiều hơn 1 vai trò';

CREATE INDEX IF NOT EXISTS ix_hr_employee_role_rel_employee_id ON hr_employee_role_rel(employee_id);
CREATE INDEX IF NOT EXISTS ix_hr_employee_role_rel_role_id ON hr_employee_role_rel(role_id);
CREATE INDEX IF NOT EXISTS ix_hr_employee_role_rel_employee_role ON hr_employee_role_rel(employee_id, role_id);

ALTER TABLE hr_employee_role_rel
    ADD CONSTRAINT fk_hr_employee_role_rel_employee_id FOREIGN KEY (employee_id) REFERENCES hr_employee(id) ON DELETE RESTRICT ON UPDATE RESTRICT;
ALTER TABLE hr_employee_role_rel
    ADD CONSTRAINT fk_hr_employee_role_rel_role_id FOREIGN KEY (role_id) REFERENCES hr_employee_role(id) ON DELETE RESTRICT ON UPDATE RESTRICT;
```

---

## ✅ Checklist Trước Khi Hoàn Thành

- [ ] Entity `HrEmployee` đã tạo với đầy đủ properties và validation
- [ ] Entity `HrEmployeeRoleRel` đã tạo với date overlap validation
- [ ] Repository interfaces và implementations đã tạo
- [ ] Manager classes đã tạo với business logic validation
- [ ] EF Core configurations đã tạo (snake_case cho table và columns)
- [ ] DTOs đã tạo (HrEmployeeDto, CreateHrEmployeeDto, UpdateHrEmployeeDto, GetHrEmployeesInput, HrEmployeeRoleRelDto, etc.)
- [ ] Application Service interface và implementation đã tạo
- [ ] AutoMapper configuration đã thêm mappings
- [ ] Permissions đã được định nghĩa và sử dụng
- [ ] Localization keys đã đầy đủ (vi-VN và en)
- [ ] HTTP API Controller đã tạo với đầy đủ endpoints
- [ ] Conventional Controllers đã exclude HrEmployeeAppService
- [ ] Menu Contributor đã thêm menu item
- [ ] Migration script đã tạo và test
- [ ] Code validation (A-Z, _, 0-9) đã implement
- [ ] Code immutable khi update đã implement
- [ ] Soft delete + status deactive đã implement
- [ ] Date overlap validation cho role assignment đã implement
- [ ] Tree view filter API đã implement
- [ ] Build solution thành công
- [ ] Test API endpoints thành công

---

## 📌 Lưu Ý Quan Trọng

1. **Table và Column Names**: Tất cả phải theo **snake_case** (PostgreSQL convention)
2. **Code Validation**: Regex `^[A-Z0-9_]+$` (uppercase)
3. **Code Immutable**: Không có method `UpdateCode()` trong Entity
4. **Soft Delete**: Xóa trước (ABP audit log) → cập nhật status = Deactive sau
5. **Organization Selection**: Chỉ cho phép chọn phòng ban có `DeptLevel = Unit`
6. **Date Overlap**: Cùng vai trò không được overlap ngày hiệu lực
7. **User Mapping**: `UserId` là `Guid?`, FK đến `IdentityUser.Id`
8. **IsManager**: Dùng `bool` trong Entity, convert sang "Y"/"N" trong database nếu cần

