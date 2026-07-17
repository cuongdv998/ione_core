# Plan: Backend - Cấu hình phân cấp duyệt (ResBusinessAssignee)

## 1. Tổng Quan

**Module**: Master (Danh mục)  
**Entity**: ResBusinessAssignee (Cấu hình phân cấp duyệt / Gán đối tượng thực hiện theo thẩm quyền)  
**Table Name**: `res_business_assignee` (snake_case - PostgreSQL convention)  
**Menu**: Đặt trong **Danh mục Quy trình** (submenu dưới Master, cùng cấp với Phân cấp duyệt)  
**Mô tả**: Bảng định nghĩa đối tượng sẽ thực hiện theo thẩm quyền (emp = nhân viên cụ thể, role = theo vai trò, system = hệ thống). Khi sửa chỉ cho phép sửa nhân viên duyệt (AssigneeId).

### Yêu cầu chức năng

| Yêu cầu | Mô tả |
|--------|--------|
| Tìm kiếm | Filter theo BusinessCode, AuthorityCode, AssigneeType, DepartmentId, Status, ...; phân trang, sắp xếp |
| Thêm mới | Đủ các field theo DDL; BusinessCode từ AdminConfig, AuthorityCode từ res_business_authority, DepartmentId từ hr_department, AssigneeRole từ hr_employee_role, AssigneeId từ hr_employee (có thể filter theo DepartmentId; nếu chọn role thì load nhân viên theo hr_employee_role_rel) |
| Sửa | **Chỉ cho phép sửa AssigneeId** (nhân viên duyệt); các field khác immutable khi update |
| Xóa | Soft delete trước (trigger ABP audit log), sau đó cập nhật Status = Deactive |
| Nguồn dữ liệu | Phòng ban: hr_department; Nhân viên: hr_employee (filter theo department khi chọn phòng ban); Mã thẩm quyền: res_business_authority; Mã nghiệp vụ: admin_config code = 'BUSINESS_CODE' (value = sub_code, label = value); Vai trò: hr_employee_role; Quan hệ NV–Vai trò: hr_employee_role_rel |

---

## 2. Database Schema

### 2.1. DDL Tham Chiếu (Oracle style)

```sql
create table BUSINESSASSIGNEE (
  ID CHAR(36) not null,
  ORGANIZATIONID CHAR(36) null,
  BUSINESSCODE VARCHAR(50) not null,
  AUTHORITYCODE VARCHAR(50) not null,
  ASSIGNEETYPE VARCHAR(15) not null,
  EFFECTDATE DATE not null,
  EXPIREDATE DATE null,
  ASSIGNEEROLE VARCHAR(50) null,
  ASSIGNEEID CHAR(36) null,
  DEPARTMENTID CHAR(36) null,
  DEPARTMENTLEVEL VARCHAR(15) null,
  CREATIONTIME DATE not null,
  CREATORID CHAR(36) not null,
  LASTMODIFICATIONTIME DATE null,
  LASTMODIFIERID CHAR(36) null,
  constraint PK_BUSINESSASSIGNEE primary key (ID)
);

comment on table BUSINESSASSIGNEE is
  'Bảng định nghĩa đối tượng sẽ thực hiện theo thẩm quyền';

comment on column BUSINESSASSIGNEE.BUSINESSCODE is
  'Mã nghiệp vụ liên quan, định nghĩa trong bảng AdminConfig với code = BUSINESS_CODE và subcode là các nghiệp vụ tương ứng';

comment on column BUSINESSASSIGNEE.AUTHORITYCODE is
  'Mã thẩm quyền thực hiện';

comment on column BUSINESSASSIGNEE.ASSIGNEETYPE is
  'Phân loại: emp - đích danh; role - theo vai trò; system - hệ thống tự động';

comment on column BUSINESSASSIGNEE.ASSIGNEEROLE is
  'Mã vai trò, bắt buộc nếu assigneeType = role';

comment on column BUSINESSASSIGNEE.ASSIGNEEID is
  'ID nhân viên, bắt buộc nếu assigneeType = emp';

comment on column BUSINESSASSIGNEE.DEPARTMENTID is
  'Đơn vị thực hiện';

comment on column BUSINESSASSIGNEE.DEPARTMENTLEVEL is
  'in - trong phân cấp; out - trên phân cấp';
```

### 2.2. Bổ sung cho ABP và xóa mềm

- Thêm cột **status** (varchar 10): `active` / `deactive` (khi xóa: soft delete rồi set status = deactive).
- Chuẩn ABP **FullAuditedAggregateRoot**: thêm `is_deleted`, `deletion_time`, `deleter_id`, `concurrency_stamp`, `tenant_id` (nếu dùng multi-tenant).

### 2.3. Table Structure (PostgreSQL – snake_case)

- **Table name**: `res_business_assignee`
- **id**: uuid, PK
- **organization_id**: uuid, nullable
- **business_code**: varchar(50), NOT NULL — từ admin_config `code = 'BUSINESS_CODE'`, giá trị = sub_code
- **authority_code**: varchar(50), NOT NULL — từ res_business_authority.code
- **assignee_type**: varchar(15), NOT NULL — enum: `emp` | `role` | `system`
- **effect_date**: date, NOT NULL
- **expire_date**: date, nullable
- **assignee_role**: varchar(50), nullable — mã vai trò từ hr_employee_role.code
- **assignee_id**: uuid, nullable — hr_employee.id
- **department_id**: uuid, nullable — hr_department.id
- **department_level**: varchar(15), nullable — `in` | `out`
- **status**: varchar(10), NOT NULL — `active` | `deactive`
- Audit: creation_time, creator_id, last_modification_time, last_modifier_id, is_deleted, deletion_time, deleter_id, concurrency_stamp, tenant_id (nếu có)

### 2.4. Naming Conventions (PostgreSQL)

- Tên bảng: `res_business_assignee`
- Tên cột: snake_case (id, organization_id, business_code, authority_code, assignee_type, effect_date, expire_date, assignee_role, assignee_id, department_id, department_level, status, creation_time, creator_id, ...)
- Primary key: `pk_res_business_assignee`
- Index (tùy nghiệp vụ): có thể thêm index cho business_code, authority_code, department_id, assignee_id, effect_date để tìm kiếm/join nhanh.

---

## 3. Domain Layer

### 3.1. Enums

**Location**: `src/common/domain/iOne.Domain.Shared/ResBusinessAssignees/`

- **ResBusinessAssigneeStatus.cs**: `Active = 0`, `Deactive = 1` (lưu DB dạng `"active"` / `"deactive"`).
- **ResBusinessAssigneeType.cs**: `Emp = 0` (emp), `Role = 1` (role), `System = 2` (system) — lưu DB dạng `"emp"` / `"role"` / `"system"`.
- **ResBusinessAssigneeDepartmentLevel.cs**: `In = 0` (in), `Out = 1` (out) — lưu DB dạng `"in"` / `"out"`.

### 3.2. Entity: ResBusinessAssignee

**Location**: `src/common/domain/iOne.Domain/ResBusinessAssignees/ResBusinessAssignee.cs`

**Yêu cầu**:

- Kế thừa `FullAuditedAggregateRoot<Guid>`.
- `[Table("res_business_assignee")]`.
- Properties: Id, OrganizationId?, BusinessCode, AuthorityCode, AssigneeType, EffectDate, ExpireDate?, AssigneeRole?, AssigneeId?, DepartmentId?, DepartmentLevel?, Status.
- **Khi sửa chỉ cho phép đổi AssigneeId**: chỉ có method `UpdateAssigneeId(Guid? assigneeId)` (và có thể `UpdateStatus` cho xóa mềm). Các field khác **immutable** khi update (không có UpdateBusinessCode, UpdateAuthorityCode, ...).
- Constructor đầy đủ tham số; validation trong Set* (EffectDate/ExpireDate, AssigneeType vs AssigneeId/AssigneeRole, v.v.).
- Quan hệ (navigation, optional): không bắt buộc FK trong Domain nếu tất cả đều lấy từ Master/HR; nếu cần có thể thêm navigation tới HrEmployee, HrDepartment, HrEmployeeRole (tùy thiết kế).

### 3.3. Repository Interface

**Location**: `src/common/domain/iOne.Domain/ResBusinessAssignees/IResBusinessAssigneeRepository.cs`

- Kế thừa `IRepository<ResBusinessAssignee, Guid>`.
- Có thể thêm method tùy nghiệp vụ (ví dụ filter theo business_code + authority_code + department_id) nếu cần tối ưu.

### 3.4. Manager

**Location**: `src/common/domain/iOne.Domain/ResBusinessAssignees/ResBusinessAssigneeManager.cs`

- **CreateAsync(ResBusinessAssignee entity)**: validate nghiệp vụ (AssigneeType vs AssigneeId/AssigneeRole, EffectDate/ExpireDate, BusinessCode/AuthorityCode tồn tại nếu cần), rồi `InsertAsync`.
- **UpdateAsync(ResBusinessAssignee entity, Guid? assigneeId)**: chỉ gọi `entity.UpdateAssigneeId(assigneeId)` rồi `UpdateAsync` — không nhận tham số nào khác.

---

## 4. Application Layer

### 4.1. DTOs

**Thư mục**: `modules/master/src/iOne.Master.Application.Contracts/ResBusinessAssignees/`

| File | Nội dung |
|------|----------|
| **ResBusinessAssigneeDto** | FullAuditedEntityDto; các field: Id, OrganizationId?, BusinessCode, AuthorityCode, AssigneeType, EffectDate, ExpireDate?, AssigneeRole?, AssigneeId?, DepartmentId?, DepartmentLevel?, Status; có thể thêm DisplayName cho Assignee/Department/Role (tùy chọn). |
| **CreateResBusinessAssigneeDto** | Đủ field để tạo mới: OrganizationId?, BusinessCode, AuthorityCode, AssigneeType, EffectDate, ExpireDate?, AssigneeRole?, AssigneeId?, DepartmentId?, DepartmentLevel?, Status (default Active). |
| **UpdateResBusinessAssigneeDto** | **Chỉ có AssigneeId?** (nullable) — vì khi sửa chỉ cho phép sửa nhân viên duyệt. |
| **GetResBusinessAssigneesInput** | PagedAndSortedResultRequestDto; filter: BusinessCode?, AuthorityCode?, AssigneeType?, DepartmentId?, AssigneeId?, Status?, EffectDateFrom?, EffectDateTo?, ... |

- Display names dùng key localization `Master::ResBusinessAssignee:...`.

### 4.2. API phụ trợ (dropdown / danh sách)

- **Phòng ban**: Dùng API HrDepartment (GetList) — module HR.
- **Nhân viên theo phòng ban**: API HrEmployee GetList với filter `departmentId` (khi chọn phòng ban, frontend gọi với departmentId).
- **Mã thẩm quyền (AuthorityCode)**: API ResBusinessAuthority GetList (hoặc endpoint chỉ trả về list code/name) — value = Code, label = Name.
- **Mã nghiệp vụ (BusinessCode)**: admin_config với `code = 'BUSINESS_CODE'` — value = sub_code, label = value (giống ResBusinessAuthority).
- **Vai trò (AssigneeRole)**: API HrEmployeeRole GetList — value = Code, label = Name.
- **Nhân viên theo vai trò**: Lấy qua hr_employee_role_rel (EmployeeId theo RoleId); có thể thêm endpoint trong HR hoặc Master trả về danh sách nhân viên theo role (hoặc frontend gọi HrEmployeeRoleRel + HrEmployee).

Backend Master **không bắt buộc** implement toàn bộ API HR; có thể giả định đã có sẵn HrDepartment, HrEmployee, HrEmployeeRole, HrEmployeeRoleRel. Plan chỉ nêu rõ nguồn dữ liệu và cách dùng (khi chọn phòng ban → gọi danh sách nhân viên theo department; khi chọn role → danh sách nhân viên theo role qua hr_employee_role_rel).

### 4.3. Application Service Interface

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResBusinessAssignees/IResBusinessAssigneeAppService.cs`

- CRUD: GetList (GetResBusinessAssigneesInput), Get(id), Create(CreateResBusinessAssigneeDto), Update(id, UpdateResBusinessAssigneeDto), Delete(id).
- Tất cả method `[Authorize]` với permission tương ứng.

### 4.4. Application Service Implementation

**Location**: `modules/master/src/iOne.Master.Application/ResBusinessAssignees/ResBusinessAssigneeAppService.cs`

- **CreateAsync**: Map CreateDto → Entity, gọi Manager.CreateAsync, return DTO.
- **UpdateAsync**: Chỉ đọc UpdateDto.AssigneeId; load entity, gọi Manager.UpdateAsync(entity, input.AssigneeId) — **không** map các field khác.
- **DeleteAsync**: Thứ tự đúng audit — `Repository.DeleteAsync(entity)` → SaveChanges → `entity.UpdateStatus(ResBusinessAssigneeStatus.Deactive)` → UpdateAsync → SaveChanges.
- **CreateFilteredQueryAsync**: Filter theo BusinessCode, AuthorityCode, AssigneeType, DepartmentId, AssigneeId, Status, khoảng EffectDate (nếu có trong input).

### 4.5. AutoMapper

**Location**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

- `CreateMap<ResBusinessAssignee, ResBusinessAssigneeDto>()`.
- `CreateMap<CreateResBusinessAssigneeDto, ResBusinessAssignee>()`.
- `CreateMap<UpdateResBusinessAssigneeDto, ResBusinessAssignee>()` — có thể Ignore mọi thứ trừ AssigneeId (hoặc chỉ map AssigneeId trong AppService thủ công và vẫn giữ mapping để tránh lỗi thiếu map).

---

## 5. Permissions

### 5.1. Constants

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResBusinessAssigneePermissions.cs`

- GroupName = `"MasterResBusinessAssignee"`.
- Default = GroupName.
- Create, Edit, Delete, View = Default + ".Create", ".Edit", ".Delete", ".View".

### 5.2. Permission Definition Provider

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResBusinessAssigneePermissionDefinitionProvider.cs`

- AddGroup(GroupName, L("Permission:ResBusinessAssignee")).
- AddPermission(Default, ...).
- AddChild cho Create, Edit, Delete, View.

---

## 6. Entity Framework Core

### 6.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResBusinessAssignees/ResBusinessAssigneeConfiguration.cs`

- ToTable("res_business_assignee", comment).
- ConfigureByConvention().
- Tất cả cột snake_case; enum (Status, AssigneeType, DepartmentLevel) HasConversion string lowercase.
- Index (tùy chọn): business_code, authority_code, department_id, assignee_id, effect_date.

### 6.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResBusinessAssignees/EfCoreResBusinessAssigneeRepository.cs`

- Kế thừa EfCoreRepository<iOneDbContext, ResBusinessAssignee, Guid>, implement IResBusinessAssigneeRepository.

### 6.3. DbContext & Module

- **iOneDbContext**: DbSet<ResBusinessAssignee> ResBusinessAssignees; ApplyConfiguration(ResBusinessAssigneeConfiguration).
- **iOneEntityFrameworkCoreModule**: AddRepository<ResBusinessAssignee, EfCoreResBusinessAssigneeRepository>.

---

## 7. HTTP API

### 7.1. Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/Controllers/ResBusinessAssigneeController.cs`

- Route: `"api/master/res-business-assignees"`.
- GetList(GetResBusinessAssigneesInput), Get(id), Create(CreateResBusinessAssigneeDto), Update(id, UpdateResBusinessAssigneeDto), Delete(id).
- Authorize từng action với ResBusinessAssigneePermissions.

### 7.2. Exclude Conventional Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`

- TypePredicate thêm: `type.Name != "ResBusinessAssigneeAppService"`.

---

## 8. Localization

**Location**: `modules/master/.../Localization/Master/vi-VN.json` và `en.json`

- Menu: `Menu:ResBusinessAssignee` (Cấu hình phân cấp duyệt).
- Permission: `Permission:ResBusinessAssignee`.
- Các key ResBusinessAssignee: OrganizationId, BusinessCode, AuthorityCode, AssigneeType, EffectDate, ExpireDate, AssigneeRole, AssigneeId, DepartmentId, DepartmentLevel, Status, AssigneeTypeEmp, AssigneeTypeRole, AssigneeTypeSystem, DepartmentLevelIn, DepartmentLevelOut, validation messages, CreatedSuccessfully, UpdatedSuccessfully, DeletedSuccessfully, DeleteConfirm, AssigneeIdOnlyEditable, v.v.

---

## 9. Menu (Navigation)

**Location**: `modules/master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

- Trong submenu **Danh mục Quy trình** (Master.Process), thêm item:
  - `"Master.ResBusinessAssignee"`, text = masterL["Menu:ResBusinessAssignee"], url = `"~/pages/master/res-business-assignees"`, icon, RequirePermissions(ResBusinessAssigneePermissions.Default).

---

## 10. Migration

- Tạo migration: `AddResBusinessAssignee`.
- Up: CREATE TABLE res_business_assignee (…); comment bảng/cột; index (nếu có).
- Down: DROP TABLE IF EXISTS res_business_assignee.
- Tên bảng/cột/index đều snake_case.

---

## 11. Checklist

- [ ] Domain: Enums (Status, AssigneeType, DepartmentLevel), Entity ResBusinessAssignee (chỉ UpdateAssigneeId khi sửa), IResBusinessAssigneeRepository, ResBusinessAssigneeManager.
- [ ] Application: DTOs (Create đủ field, Update chỉ AssigneeId), IResBusinessAssigneeAppService, ResBusinessAssigneeAppService (Delete: delete trước rồi set status Deactive), AutoMapper.
- [ ] Permissions: ResBusinessAssigneePermissions, ResBusinessAssigneePermissionDefinitionProvider.
- [ ] EF Core: ResBusinessAssigneeConfiguration (snake_case, enum conversion), EfCoreResBusinessAssigneeRepository, DbContext & Module.
- [ ] HTTP: ResBusinessAssigneeController, exclude ResBusinessAssigneeAppService.
- [ ] Localization: vi-VN + en.
- [ ] Menu: Item Cấu hình phân cấp duyệt trong submenu Danh mục Quy trình.
- [ ] Migration: snake_case, IF EXISTS/IF NOT EXISTS.

---

## 12. Lưu Ý Quan Trọng

1. **Khi sửa chỉ được sửa AssigneeId**: UpdateResBusinessAssigneeDto chỉ có AssigneeId; Entity chỉ có UpdateAssigneeId(...); AppService Update chỉ gọi Manager.UpdateAsync(entity, input.AssigneeId).
2. **Xóa**: Luôn soft delete trước (trigger audit log), sau đó cập nhật Status = Deactive.
3. **Nguồn dữ liệu**: BusinessCode (admin_config BUSINESS_CODE); AuthorityCode (res_business_authority); Department (hr_department); Employee (hr_employee, filter theo department khi chọn phòng ban); AssigneeRole (hr_employee_role); nhân viên theo role qua hr_employee_role_rel.
4. **PostgreSQL**: Toàn bộ table/column/index theo snake_case.
5. **Menu**: Đặt “Cấu hình phân cấp duyệt” trong submenu “Danh mục Quy trình” của Master.
