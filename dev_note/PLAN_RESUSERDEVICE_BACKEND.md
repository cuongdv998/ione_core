# Plan Xây Dựng Backend: Quản lý Thiết bị di động (ResUserDevice)

## 📋 Tổng Quan

**Module**: Master (Quản trị/Danh mục)  
**Entity**: ResUserDevice (Thiết bị di động của user)  
**Table Name**: `res_user_device` (snake_case - PostgreSQL convention)  
**Entity Name Pattern**: Theo pattern `Res*` trong module Master  
**Chức năng**: Xem, Thêm, Sửa, Xóa thiết bị di động của user để gửi notify đến app

---

## 🎯 Yêu Cầu Chức Năng

### ResUserDevice (Thiết bị di động)
1. **CRUD Operations**: Xem, Thêm, Sửa, Xóa
2. **Khi sửa – các trường KHÔNG được phép sửa** (disable trên giao diện):
   - **UserName** (USERNAME)
   - **DeviceUid** (DEVICEUID)
   - **AppChannelCode** (APPCHANNELCODE)
   - **EffectDate** (EFFECTDATE – ngày hiệu lực)
3. **Unique constraint**: Bộ ba (UserName, DeviceUid, AppChannelCode) là duy nhất
4. **Soft Delete + Audit**: Khi xóa:
   - Bước 1: Gọi `Repository.DeleteAsync(entity)` → trigger ABP audit log (soft delete)
   - Bước 2: `CurrentUnitOfWork.SaveChangesAsync()`
   - Bước 3: Cập nhật `entity.Status = ResUserDeviceStatus.Deactive`
   - Bước 4: `Repository.UpdateAsync(entity)` và `SaveChangesAsync()`
5. **Đa ngôn ngữ**: vi-VN và en (localization keys đầy đủ)
6. **Phân quyền**: View, Create, Edit, Delete (ResUserDevicePermissions)

### API Requirements
- GetList (filter: UserName?, DeviceUid?, AppChannelCode?, Status?)
- Get(id)
- Create(CreateResUserDeviceDto)
- Update(id, UpdateResUserDeviceDto) – chỉ cho phép sửa: DeviceToken, ExpirDate, Status, Os, DeviceName
- Delete(id) – soft delete + cập nhật status deactive

---

## 📐 Ánh xạ DLL → Entity (PostgreSQL snake_case)

| DLL (UPPERCASE) | Entity Property | Column (snake_case) | Ghi chú |
|-----------------|-----------------|---------------------|---------|
| ID | Id | id | Guid, PK |
| USERNAME | UserName | username | VARCHAR(50), not null |
| DEVICEUID | DeviceUid | device_uid | VARCHAR(250), not null |
| DEVICETOKEN | DeviceToken | device_token | VARCHAR(500), not null |
| APPCHANNELCODE | AppChannelCode | app_channel_code | VARCHAR(50), not null |
| EFFECTDATE | EffectDate | effect_date | DATE, not null |
| EXPIRDATE | ExpirDate | expir_date | DATE, null |
| STATUS | Status | status | enum → varchar(15): active/deactive |
| OS | Os | os | VARCHAR(50), null |
| DEVICENAME | DeviceName | device_name | VARCHAR(250), null |
| CREATIONTIME | CreationTime | creation_time | Audit (ABP) |
| CREATORID | CreatorId | creator_id | Audit (ABP) |

**Comment bảng**: Bảng lưu trữ định danh thiết bị mobile của user để gửi notify đến app.

**Comment cột STATUS**: Trạng thái: active (Hoạt động), deactive (Không hoạt động).

---

## 📁 Cấu Trúc Files Cần Tạo

### 1. Domain Layer

#### 1.1. Entity ResUserDevice
**Location**: `src/common/domain/iOne.Domain/ResUserDevices/ResUserDevice.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>`
- Table name: `[Table("res_user_device")]` (snake_case)
- Properties:
  - `UserName` (string, max 50, required, private setter)
  - `DeviceUid` (string, max 250, required, private setter)
  - `DeviceToken` (string, max 500, required, private setter)
  - `AppChannelCode` (string, max 50, required, private setter)
  - `EffectDate` (DateTime, required, private setter) – chỉ date part
  - `ExpirDate` (DateTime?, optional, private setter)
  - `Status` (ResUserDeviceStatus enum, required, private setter)
  - `Os` (string, max 50, optional, private setter)
  - `DeviceName` (string, max 250, optional, private setter)
- Constructor với validation
- Private setters; public methods chỉ cho phép update: `UpdateDeviceToken()`, `UpdateExpirDate()`, `UpdateStatus()`, `UpdateOs()`, `UpdateDeviceName()`  
- **KHÔNG** có method update cho: UserName, DeviceUid, AppChannelCode, EffectDate

#### 1.2. Enum ResUserDeviceStatus
**Location**: `src/common/domain/iOne.Domain.Shared/ResUserDevices/ResUserDeviceStatus.cs`

**Yêu cầu**:
- `Active = 0` (Hoạt động)
- `Deactive = 1` (Không hoạt động)
- Lưu trong DB dưới dạng string: "active", "deactive" (max 15)

#### 1.3. Repository Interface
**Location**: `src/common/domain/iOne.Domain/ResUserDevices/IResUserDeviceRepository.cs`

**Yêu cầu**:
- Kế thừa từ `IRepository<ResUserDevice, Guid>`
- Method: `IsUniqueExistsAsync(string userName, string deviceUid, string appChannelCode, Guid? excludeId = null)` để kiểm tra unique (UserName, DeviceUid, AppChannelCode), excludeId dùng khi update

#### 1.4. Manager
**Location**: `src/common/domain/iOne.Domain/ResUserDevices/ResUserDeviceManager.cs`

**Yêu cầu**:
- `CreateAsync(ResUserDevice entity)`: Kiểm tra unique (UserName, DeviceUid, AppChannelCode), insert
- `UpdateAsync(ResUserDevice entity, string deviceToken, DateTime? expirDate, ResUserDeviceStatus status, string? os, string? deviceName)`: Chỉ cập nhật các trường được phép; **không** có tham số UserName, DeviceUid, AppChannelCode, EffectDate

---

### 2. Application Layer

#### 2.1. DTOs
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/ResUserDevices/`

**Files**:
- **ResUserDeviceDto.cs**: Kế thừa `FullAuditedEntityDto<Guid>`
  - Id, UserName, DeviceUid, DeviceToken, AppChannelCode, EffectDate, ExpirDate, Status, Os, DeviceName + audit fields

- **CreateResUserDeviceDto.cs**:
  - UserName (required, max 50)
  - DeviceUid (required, max 250)
  - DeviceToken (required, max 500)
  - AppChannelCode (required, max 50)
  - EffectDate (required)
  - ExpirDate (optional)
  - Status (required)
  - Os (optional, max 50)
  - DeviceName (optional, max 250)

- **UpdateResUserDeviceDto.cs** (chỉ các trường được phép sửa):
  - DeviceToken (required, max 500)
  - ExpirDate (optional)
  - Status (required)
  - Os (optional, max 50)
  - DeviceName (optional, max 250)
  - **KHÔNG có**: UserName, DeviceUid, AppChannelCode, EffectDate

- **GetResUserDevicesInput.cs**: Kế thừa `PagedAndSortedResultRequestDto`
  - UserName? (filter)
  - DeviceUid? (filter)
  - AppChannelCode? (filter)
  - Status? (filter)

#### 2.2. Application Service Interface
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/ResUserDevices/IResUserDeviceAppService.cs`

**Yêu cầu**:
- Kế thừa từ `ICrudAppService<ResUserDeviceDto, Guid, GetResUserDevicesInput, CreateResUserDeviceDto, UpdateResUserDeviceDto>`
- Tất cả method có `[Authorize]` với permission tương ứng

#### 2.3. Application Service Implementation
**Location**: `modules/Master/src/iOne.Master.Application/ResUserDevices/ResUserDeviceAppService.cs`

**Yêu cầu**:
- Kế thừa từ `CrudAppService<ResUserDevice, ResUserDeviceDto, Guid, GetResUserDevicesInput, CreateResUserDeviceDto, UpdateResUserDeviceDto>`
- Inject: `IResUserDeviceRepository`, `ResUserDeviceManager`
- **CreateAsync**: Validate unique (UserName, DeviceUid, AppChannelCode) trước khi tạo; throw `BusinessException` với localization key nếu trùng
- **UpdateAsync**: Chỉ map/gọi Manager với DeviceToken, ExpirDate, Status, Os, DeviceName; **không** đọc/ghi UserName, DeviceUid, AppChannelCode, EffectDate từ input
- **DeleteAsync**:
  1. Load entity
  2. `Repository.DeleteAsync(entity)` → audit log
  3. `CurrentUnitOfWork.SaveChangesAsync()`
  4. Set `entity.UpdateStatus(ResUserDeviceStatus.Deactive)` (hoặc method tương ứng)
  5. `Repository.UpdateAsync(entity)`
  6. `CurrentUnitOfWork.SaveChangesAsync()`
- **CreateFilteredQueryAsync**: Filter theo UserName, DeviceUid, AppChannelCode, Status (case-insensitive where phù hợp)
- Set policies: GetPolicyName, GetListPolicyName, CreatePolicyName, UpdatePolicyName, DeletePolicyName → ResUserDevicePermissions

#### 2.4. AutoMapper Configuration
**Location**: `modules/Master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

**Yêu cầu**:
- `CreateMap<ResUserDevice, ResUserDeviceDto>()`
- `CreateMap<CreateResUserDeviceDto, ResUserDevice>()`
- `CreateMap<UpdateResUserDeviceDto, ResUserDevice>()` – chỉ map các field được phép sửa (có thể dùng `ForMember` ignore UserName, DeviceUid, AppChannelCode, EffectDate)

---

### 3. Permissions

#### 3.1. Permission Constants
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/Permissions/ResUserDevicePermissions.cs`

**Yêu cầu**:
```csharp
public const string GroupName = "MasterResUserDevice";
public const string Default = GroupName;
public const string Create = Default + ".Create";
public const string Edit = Default + ".Edit";
public const string Delete = Default + ".Delete";
public const string View = Default + ".View";
```

#### 3.2. Permission Definition Provider
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/Permissions/ResUserDevicePermissionDefinitionProvider.cs`

**Yêu cầu**:
- Tạo group `ResUserDevicePermissions.GroupName`
- Default permission = GroupName
- Child: Create, Edit, Delete, View
- Localization: `L("Permission:ResUserDevice")`, `L("Permission:Create")`, ...

---

### 4. Entity Framework Core

#### 4.1. Entity Configuration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResUserDevices/ResUserDeviceConfiguration.cs`

**Yêu cầu**:
- Table name: `"res_user_device"` (snake_case)
- Comment table: Bảng lưu trữ định danh thiết bị mobile của user để gửi notify đến app
- Column names (snake_case): id, username, device_uid, device_token, app_channel_code, effect_date, expir_date, status, os, device_name
- Audit columns (snake_case): creation_time, creator_id, last_modification_time, last_modifier_id, deletion_time, deleter_id, is_deleted, concurrency_stamp, tenant_id
- Status: conversion enum ↔ string "active"/"deactive" (max 15)
- Unique index: `(username, device_uid, app_channel_code)` – tên ví dụ: `ix_res_user_device_username_device_uid_app_channel_code` (unique)
- Index names: snake_case, prefix `ix_` / `pk_` theo RULES

#### 4.2. Repository Implementation
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResUserDevices/EfCoreResUserDeviceRepository.cs`

**Yêu cầu**:
- Kế thừa `EfCoreRepository<iOneDbContext, ResUserDevice, Guid>`
- Implement `IResUserDeviceRepository`
- `IsUniqueExistsAsync`: query `AnyAsync` với điều kiện UserName, DeviceUid, AppChannelCode trùng và (excludeId == null || Id != excludeId)

#### 4.3. Register
**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`
- `modelBuilder.ApplyConfiguration(new ResUserDeviceConfiguration());`
- `DbSet<ResUserDevice> ResUserDevices` (nếu dùng convention)

---

### 5. Database Migration

**Yêu cầu**:
- Table name: `"res_user_device"` (snake_case)
- Column names: snake_case (id, username, device_uid, device_token, app_channel_code, effect_date, expir_date, status, os, device_name + full audit columns)
- Primary key: `pk_res_user_device`
- Unique index: `(username, device_uid, app_channel_code)` – tên ví dụ: `uq_res_user_device_username_device_uid_app_channel_code`
- **LUÔN** dùng `IF EXISTS` khi DROP, `IF NOT EXISTS` khi CREATE
- Table comment và comment cột STATUS theo DLL

**Migration Script Structure (ý tưởng)**:
```sql
CREATE TABLE IF NOT EXISTS "res_user_device" (
    "id" UUID NOT NULL,
    "username" VARCHAR(50) NOT NULL,
    "device_uid" VARCHAR(250) NOT NULL,
    "device_token" VARCHAR(500) NOT NULL,
    "app_channel_code" VARCHAR(50) NOT NULL,
    "effect_date" DATE NOT NULL,
    "expir_date" DATE NULL,
    "status" VARCHAR(15) NOT NULL,
    "os" VARCHAR(50) NULL,
    "device_name" VARCHAR(250) NULL,
    "creation_time" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    "creator_id" UUID NULL,
    "last_modification_time" TIMESTAMP WITHOUT TIME ZONE NULL,
    "last_modifier_id" UUID NULL,
    "is_deleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "deletion_time" TIMESTAMP WITHOUT TIME ZONE NULL,
    "deleter_id" UUID NULL,
    "concurrency_stamp" VARCHAR(40) NULL,
    "tenant_id" UUID NULL,
    "extra_properties" TEXT,
    CONSTRAINT "pk_res_user_device" PRIMARY KEY ("id")
);

COMMENT ON TABLE "res_user_device" IS 'Bảng lưu trữ định danh thiết bị mobile của user để gửi notify đến app';
COMMENT ON COLUMN "res_user_device"."status" IS 'Trạng thái: active: Hoạt động; deactive: Không hoạt động';

CREATE UNIQUE INDEX IF NOT EXISTS "uq_res_user_device_username_device_uid_app_channel_code"
    ON "res_user_device" ("username", "device_uid", "app_channel_code");
```

---

### 6. Localization

#### 6.1. Localization Files
**Location**:
- `modules/Master/src/iOne.Master.Application.Contracts/Localization/Master/vi-VN.json`
- `modules/Master/src/iOne.Master.Application.Contracts/Localization/Master/en.json`

**Keys cần thêm** (ví dụ, đủ cho menu + permission + form + validation + message):
- Menu: `Menu:ResUserDevice` (vd: "Thiết bị di động" / "Mobile devices")
- Permission: `Permission:ResUserDevice`
- Field: ResUserDevice:UserName, DeviceUid, DeviceToken, AppChannelCode, EffectDate, ExpirDate, Status, Os, DeviceName
- Validation: ResUserDevice:UserNameRequired, UserNameMaxLength, ... (tương tự các trường bắt buộc/độ dài)
- Unique: ResUserDevice:UniqueExists (vd: "Bộ (Tên đăng nhập, UID thiết bị, Mã kênh) đã tồn tại.")
- Success: ResUserDevice:CreatedSuccessfully, UpdatedSuccessfully, DeletedSuccessfully
- Edit form: ResUserDevice:UserNameCannotBeChanged, DeviceUidCannotBeChanged, AppChannelCodeCannotBeChanged, EffectDateCannotBeChanged (để hiển thị tooltip/label trên UI disable)
- Status: ResUserDevice:Active, ResUserDevice:Deactive

---

### 7. HTTP API Controller

**Location**: `modules/Master/src/iOne.Master.HttpApi/Controllers/ResUserDeviceController.cs`

**Yêu cầu**:
- Kế thừa `AbpControllerBase`
- Route: `"api/master/user-devices"` (hoặc `"api/master/res-user-devices"` tùy convention dự án)
- Attributes: `[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]`, `[Area(MasterRemoteServiceConsts.ModuleName)]`, `[Authorize]`
- Methods:
  - GetAsync(Guid id) – `[HttpGet("{id}")]`, `[Authorize(ResUserDevicePermissions.View)]`
  - GetListAsync(GetResUserDevicesInput input) – `[HttpGet]`, `[Authorize(ResUserDevicePermissions.View)]`
  - CreateAsync(CreateResUserDeviceDto input) – `[HttpPost]`, `[Authorize(ResUserDevicePermissions.Create)]`
  - UpdateAsync(Guid id, UpdateResUserDeviceDto input) – `[HttpPut("{id}")]`, `[Authorize(ResUserDevicePermissions.Edit)]`
  - DeleteAsync(Guid id) – `[HttpDelete("{id}")]`, `[Authorize(ResUserDevicePermissions.Delete)]`

#### Exclude Conventional Controllers
**Location**: `modules/Master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`
- Thêm `ResUserDeviceAppService` vào `TypePredicate` để exclude khỏi conventional controller generation (tránh duplicate API).

---

### 8. Menu (Navigation)

**Location**: `modules/Master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

**Yêu cầu**:
- Thêm menu item (dưới nhóm Quản trị/Master phù hợp):
  - Text: L["Menu:ResUserDevice"]
  - Url: `~/pages/master/user-devices` (hoặc đường dẫn theo convention frontend)
  - Icon: ví dụ `pi pi-fw pi-mobile`
  - `RequirePermissions(ResUserDevicePermissions.Default)`

---

## ✅ Checklist Trước Khi Hoàn Thành

### Domain Layer
- [ ] Entity `ResUserDevice.cs` với table `res_user_device` (snake_case)
- [ ] Enum `ResUserDeviceStatus.cs` (Active, Deactive)
- [ ] Repository interface `IResUserDeviceRepository.cs` với `IsUniqueExistsAsync`
- [ ] Manager `ResUserDeviceManager.cs` (CreateAsync, UpdateAsync không có UserName, DeviceUid, AppChannelCode, EffectDate)
- [ ] Entity không có method update cho UserName, DeviceUid, AppChannelCode, EffectDate

### Application Layer
- [ ] DTOs: ResUserDeviceDto, CreateResUserDeviceDto, UpdateResUserDeviceDto (chỉ trường được sửa), GetResUserDevicesInput
- [ ] IResUserDeviceAppService, ResUserDeviceAppService
- [ ] AutoMapper: ResUserDevice ↔ DTOs, UpdateResUserDeviceDto chỉ map trường được phép
- [ ] DeleteAsync: Delete → Save → Update Status Deactive → Update → Save

### Permissions
- [ ] ResUserDevicePermissions.cs, ResUserDevicePermissionDefinitionProvider.cs
- [ ] Đăng ký provider trong module

### EF Core
- [ ] ResUserDeviceConfiguration: table + column snake_case, unique (username, device_uid, app_channel_code), comment
- [ ] EfCoreResUserDeviceRepository, đăng ký trong DbContext

### Migration
- [ ] Table/column/index naming snake_case, IF EXISTS / IF NOT EXISTS, comments

### Localization
- [ ] vi-VN.json và en.json – đủ key cho Menu, Permission, form, validation, message (kể cả “không được sửa” cho 4 trường)

### HTTP API & Menu
- [ ] ResUserDeviceController với đủ endpoint và Authorize
- [ ] Exclude ResUserDeviceAppService trong HttpApi module
- [ ] Menu item với RequirePermissions(ResUserDevicePermissions.Default)

### Kiểm tra nghiệp vụ
- [ ] Sửa: không gửi/không cập nhật UserName, DeviceUid, AppChannelCode, EffectDate (backend + ghi chú disable trên UI)
- [ ] Xóa: audit log (soft delete) trước, cập nhật status deactive sau
- [ ] Unique: (UserName, DeviceUid, AppChannelCode) được kiểm tra khi Create và (nếu có) khi Update không đổi 3 trường này

---

## 🔍 Lưu Ý Quan Trọng

1. **Naming**: Table và column **bắt buộc** snake_case (PostgreSQL): `res_user_device`, `username`, `device_uid`, `app_channel_code`, `effect_date`, ...
2. **Immutable khi sửa**: UserName, DeviceUid, AppChannelCode, EffectDate – không có trong UpdateResUserDeviceDto và không cập nhật trong UpdateAsync.
3. **Unique**: Chỉ một bộ (UserName, DeviceUid, AppChannelCode) trong hệ thống; validate trong Manager/AppService và unique index trong DB.
4. **Xóa**: Thứ tự bắt buộc – Delete (soft delete + audit) → Save → set Status = Deactive → Update → Save.
5. **Đa ngôn ngữ**: Mọi label, message, permission đều có key vi-VN và en.
6. **Phân quyền**: Tất cả API đều bảo vệ bằng ResUserDevicePermissions (View, Create, Edit, Delete).
7. **AutoMapper**: UpdateResUserDeviceDto không map sang UserName, DeviceUid, AppChannelCode, EffectDate (dùng ForMember(..., x => x.Ignore()) nếu cần).
8. **APPCHANNELCODE**: Lưu dạng string trong bảng, không bắt buộc FK tới bảng ResAppChannel (theo DLL hiện tại).

---

## 🚀 Thứ Tự Thực Hiện Đề Xuất

1. Domain: Enum → Entity → IRepository → Manager  
2. EF Core: Configuration → Repository implementation → DbContext  
3. Application: DTOs → IAppService → AppService → AutoMapper  
4. Permissions: Constants → DefinitionProvider → đăng ký  
5. Localization: vi-VN + en  
6. HTTP API: Controller + exclude conventional  
7. Menu: MenuContributor  
8. Migration: tạo và kiểm tra  
9. Build + test API + test permission + test localization

---

## 📌 Reference

- Quy tắc: `dev_note/RULES_BACKEND_DEVELOPMENT.md`
- Plan mẫu: `dev_note/PLAN_RESEVENT_BACKEND.md`
- Entity/AppService mẫu: ResEvent, ResEventAppService (DeleteAsync + immutable Code)
