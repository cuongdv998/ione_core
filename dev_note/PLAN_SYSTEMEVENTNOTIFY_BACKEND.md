# Plan Xây Dựng Backend: Tra cứu tin nhắn (SystemEventNotify)

## 📋 Tổng Quan

**Module**: Master (Danh mục)  
**Entity**: SystemEventNotify (Bản tin sự kiện hệ thống – tin nhắn cần gửi / đã gửi / lỗi)  
**Table Name**: `system_event_notify` (snake_case - PostgreSQL convention)  
**Chức năng**: Tra cứu tin nhắn – Tìm kiếm, Thêm mới, Sửa (chỉ status), Xóa (soft delete + cập nhật status deactive)

---

## 🎯 Yêu Cầu Chức Năng

### SystemEventNotify (Tra cứu tin nhắn)
1. **CRUD Operations**: Tìm kiếm, Thêm mới, Sửa, Xóa
2. **Khi sửa**: **Chỉ cho phép sửa trạng thái (Status)** – các trường khác không được phép sửa (disable trên UI)
3. **Khi xóa**:
   - Bước 1: Gọi `Repository.DeleteAsync(entity)` → trigger ABP audit log (soft delete)
   - Bước 2: `CurrentUnitOfWork.SaveChangesAsync()`
   - Bước 3: Cập nhật `entity.Status = SystemEventNotifyStatus.Deactive`
   - Bước 4: `Repository.UpdateAsync(entity)` và `SaveChangesAsync()`
   - Đảm bảo cơ chế audit log của ABP (xóa trước, cập nhật status về deactive sau)
4. **Đa ngôn ngữ**: vi-VN và en (localization keys đầy đủ)
5. **Phân quyền**: View, Create, Edit, Delete (SystemEventNotifyPermissions)

### API Requirements
- **GetList**: Filter theo EventCode?, AppChannelId?, RecipientType?, RecipientId?, Recipient?, Status?, ScheduleAt từ/đến, SentAt từ/đến (tùy nghiệp vụ)
- **Get(id)**
- **Create(CreateSystemEventNotifyDto)**
- **Update(id, UpdateSystemEventNotifyDto)** – chỉ cho phép cập nhật **Status**
- **Delete(id)** – soft delete + cập nhật status deactive (Delete → Save → Update Status → Save)

---

## 📐 Ánh xạ DLL → Entity (PostgreSQL snake_case)

Bảng DLL: `SYSTEMEVENTNOTIFY` → Table: `system_event_notify`

| DLL (UPPERCASE)   | Entity Property     | Column (snake_case)   | Kiểu / Ghi chú                          |
|-------------------|---------------------|------------------------|------------------------------------------|
| ID                | Id                  | id                     | Guid, PK                                 |
| EVENTCODE         | EventCode           | event_code             | VARCHAR(50), not null                    |
| APPCHANNELID      | AppChannelId        | app_channel_id         | Guid, not null                           |
| TITLE             | Title               | title                  | VARCHAR(250), not null                   |
| BODY              | Body                | body                   | VARCHAR(500), not null                   |
| PAYLOAD           | Payload             | payload                | VARCHAR(500), null                       |
| RECIPIENTTYPE     | RecipientType       | recipient_type         | VARCHAR(10), not null (cus/emp)          |
| RECIPIENTID       | RecipientId         | recipient_id           | Guid, not null                           |
| RECIPIENT         | Recipient           | recipient              | VARCHAR(50), null                        |
| STATUS            | Status              | status                 | enum → varchar(10): pending/sent/fail/read/deactive |
| SCHEDULEAT        | ScheduleAt          | schedule_at            | DateTime (date), not null                |
| SENTAT            | SentAt              | sent_at                | DateTime?, null                          |
| READAT            | ReadAt              | read_at                | DateTime?, null                          |
| ERRORMESSAGE      | ErrorMessage        | error_message          | VARCHAR(1000), null                      |
| CREATIONTIME      | CreationTime        | creation_time          | Audit (ABP)                              |
| CREATORID         | CreatorId           | creator_id             | Audit (ABP)                              |
| LASTMODIFICATIONTIME | LastModificationTime | last_modification_time | Audit (ABP)                            |
| LASTMODIFIERID    | LastModifierId      | last_modifier_id       | Audit (ABP)                              |
| (bổ sung ABP)     | IsDeleted           | is_deleted             | Soft delete                              |
| (bổ sung ABP)     | DeletionTime        | deletion_time          | Soft delete                              |
| (bổ sung ABP)     | DeleterId           | deleter_id             | Soft delete                              |
| (bổ sung ABP)     | ConcurrencyStamp    | concurrency_stamp      | ABP                                      |
| (bổ sung ABP)     | TenantId            | tenant_id              | ABP                                      |

**Comment bảng**: Lưu các bản tin cần gửi của hệ thống (chưa gửi, hoặc gửi fail).

**Comment cột** (theo DLL):
- **title**: Tiêu đề bản tin, đã được thay thế tham số
- **body**: Nội dung bản tin, đã được thay thế tham số
- **payload**: Thông tin mô tả hành xử cho các hệ thống nhận tin, đã được thay thế các tham số
- **recipient_type**: Loại đối tượng nhận tin: cus (khách hàng), emp (nhân viên)
- **recipient_id**: Id tương ứng với nhân viên hoặc khách hàng
- **recipient**: Địa chỉ nhận tin
- **status**: Trạng thái: pending (chờ gửi), sent (đã gửi), fail (lỗi), read (đã đọc), deactive (đã xóa/ngừng)
- **schedule_at**: Thời gian dự kiến gửi
- **sent_at**: Thời điểm gửi
- **read_at**: Thời điểm đọc
- **error_message**: Bản tin lỗi nếu có lỗi xảy ra

---

## 📁 Cấu Trúc Files Cần Tạo

### 1. Domain Layer

#### 1.1. Entity SystemEventNotify
**Location**: `src/common/domain/iOne.Domain/SystemEventNotifies/SystemEventNotify.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>`
- Table name: `[Table("system_event_notify")]` (snake_case)
- Properties:
  - `EventCode` (string, max 50, required, private setter)
  - `AppChannelId` (Guid, required, private setter)
  - `Title` (string, max 250, required, private setter)
  - `Body` (string, max 500, required, private setter)
  - `Payload` (string, max 500, optional, private setter)
  - `RecipientType` (string, max 10, required, private setter) – cus/emp
  - `RecipientId` (Guid, required, private setter)
  - `Recipient` (string, max 50, optional, private setter)
  - `Status` (SystemEventNotifyStatus enum, required, private setter)
  - `ScheduleAt` (DateTime, required, private setter)
  - `SentAt` (DateTime?, optional, private setter)
  - `ReadAt` (DateTime?, optional, private setter)
  - `ErrorMessage` (string, max 1000, optional, private setter)
- Constructor với validation
- **Chỉ có method update cho Status**: `UpdateStatus(SystemEventNotifyStatus status)` – không có method update cho các trường khác (khi sửa chỉ cho phép sửa status)

#### 1.2. Enum SystemEventNotifyStatus
**Location**: `src/common/domain/iOne.Domain.Shared/SystemEventNotifies/SystemEventNotifyStatus.cs`

**Yêu cầu**:
- `Pending = 0` (Chờ gửi)
- `Sent = 1` (Đã gửi)
- `Fail = 2` (Lỗi)
- `Read = 3` (Đã đọc)
- `Deactive = 4` (Đã xóa / Ngừng – dùng khi soft delete)
- Lưu trong DB dưới dạng string: "pending", "sent", "fail", "read", "deactive" (max 10)

#### 1.3. Repository Interface
**Location**: `src/common/domain/iOne.Domain/SystemEventNotifies/ISystemEventNotifyRepository.cs`

**Yêu cầu**:
- Kế thừa từ `IRepository<SystemEventNotify, Guid>`
- Có thể thêm method custom filter nếu cần (ví dụ: GetListWithFilterAsync)

#### 1.4. Manager
**Location**: `src/common/domain/iOne.Domain/SystemEventNotifies/SystemEventNotifyManager.cs`

**Yêu cầu**:
- `CreateAsync(SystemEventNotify entity)`: Insert entity
- `UpdateStatusAsync(SystemEventNotify entity, SystemEventNotifyStatus status)`: Chỉ cập nhật Status (dùng khi user “sửa” từ UI – chỉ đổi status)
- **Không** có method UpdateAsync cho các trường khác (EventCode, Title, Body, ...) vì nghiệp vụ chỉ cho phép sửa status

---

### 2. Application Layer

#### 2.1. DTOs
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/SystemEventNotifies/`

**Files**:
- **SystemEventNotifyDto.cs**: Kế thừa `FullAuditedEntityDto<Guid>`
  - Id, EventCode, AppChannelId, Title, Body, Payload, RecipientType, RecipientId, Recipient, Status, ScheduleAt, SentAt, ReadAt, ErrorMessage + audit fields

- **CreateSystemEventNotifyDto.cs**:
  - EventCode (required, max 50)
  - AppChannelId (required)
  - Title (required, max 250)
  - Body (required, max 500)
  - Payload (optional, max 500)
  - RecipientType (required, max 10)
  - RecipientId (required)
  - Recipient (optional, max 50)
  - Status (required)
  - ScheduleAt (required)
  - SentAt (optional)
  - ReadAt (optional)
  - ErrorMessage (optional, max 1000)

- **UpdateSystemEventNotifyDto.cs** – **chỉ một trường**:
  - Status (required)
  - **KHÔNG có** bất kỳ trường nào khác (EventCode, Title, Body, ...)

- **GetSystemEventNotifiesInput.cs**: Kế thừa `PagedAndSortedResultRequestDto`
  - EventCode? (filter)
  - AppChannelId? (filter)
  - RecipientType? (filter)
  - RecipientId? (filter)
  - Recipient? (filter)
  - Status? (filter)
  - ScheduleAtFrom?, ScheduleAtTo? (filter, tùy nghiệp vụ)
  - SentAtFrom?, SentAtTo? (filter, tùy nghiệp vụ)

#### 2.2. Application Service Interface
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/SystemEventNotifies/ISystemEventNotifyAppService.cs`

**Yêu cầu**:
- Kế thừa từ `ICrudAppService<SystemEventNotifyDto, Guid, GetSystemEventNotifiesInput, CreateSystemEventNotifyDto, UpdateSystemEventNotifyDto>`
- Tất cả method có `[Authorize]` với permission tương ứng

#### 2.3. Application Service Implementation
**Location**: `modules/Master/src/iOne.Master.Application/SystemEventNotifies/SystemEventNotifyAppService.cs`

**Yêu cầu**:
- Kế thừa từ `CrudAppService<SystemEventNotify, SystemEventNotifyDto, Guid, GetSystemEventNotifiesInput, CreateSystemEventNotifyDto, UpdateSystemEventNotifyDto>`
- Inject: `ISystemEventNotifyRepository`, `SystemEventNotifyManager`
- **CreateAsync**: Map input → entity, gọi Manager.CreateAsync()
- **UpdateAsync**: 
  - Load entity
  - **Chỉ** lấy `input.Status` và gọi `Manager.UpdateStatusAsync(entity, input.Status)` (bỏ qua mọi trường khác trong UpdateDto)
- **DeleteAsync** (đảm bảo audit log):
  1. Load entity
  2. `Repository.DeleteAsync(entity)` → trigger ABP soft delete (audit log ghi nhận)
  3. `await CurrentUnitOfWork.SaveChangesAsync()`
  4. `entity.UpdateStatus(SystemEventNotifyStatus.Deactive)`
  5. `Repository.UpdateAsync(entity)`
  6. `await CurrentUnitOfWork.SaveChangesAsync()`
- **CreateFilteredQueryAsync**: Filter theo EventCode, AppChannelId, RecipientType, RecipientId, Recipient, Status, ScheduleAt, SentAt (theo GetSystemEventNotifiesInput)
- Set policies: GetPolicyName, GetListPolicyName, CreatePolicyName, UpdatePolicyName, DeletePolicyName → SystemEventNotifyPermissions

#### 2.4. AutoMapper Configuration
**Location**: `modules/Master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

**Yêu cầu**:
- `CreateMap<SystemEventNotify, SystemEventNotifyDto>()`
- `CreateMap<CreateSystemEventNotifyDto, SystemEventNotify>()`
- `CreateMap<UpdateSystemEventNotifyDto, SystemEventNotify>()` – chỉ map Status (các trường khác có thể ignore khi map Entity từ UpdateDto vì AppService không dùng map full UpdateDto → Entity mà chỉ đọc input.Status)

---

### 3. Permissions

#### 3.1. Permission Constants
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/Permissions/SystemEventNotifyPermissions.cs`

**Yêu cầu**:
```csharp
public const string GroupName = "MasterSystemEventNotify";
public const string Default = GroupName;
public const string Create = Default + ".Create";
public const string Edit = Default + ".Edit";
public const string Delete = Default + ".Delete";
public const string View = Default + ".View";
```

#### 3.2. Permission Definition Provider
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/Permissions/SystemEventNotifyPermissionDefinitionProvider.cs`

**Yêu cầu**:
- Tạo group `SystemEventNotifyPermissions.GroupName`
- Default permission = GroupName
- Child: Create, Edit, Delete, View
- Localization: `L("Permission:SystemEventNotify")`, `L("Permission:Create")`, ...

---

### 4. Entity Framework Core

#### 4.1. Entity Configuration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/SystemEventNotifies/SystemEventNotifyConfiguration.cs`

**Yêu cầu**:
- Table name: `"system_event_notify"` (snake_case)
- Comment table: Lưu các bản tin cần gửi của hệ thống (chưa gửi, hoặc gửi fail)
- Column names (snake_case): id, event_code, app_channel_id, title, body, payload, recipient_type, recipient_id, recipient, status, schedule_at, sent_at, read_at, error_message
- Audit columns (snake_case): creation_time, creator_id, last_modification_time, last_modifier_id, deletion_time, deleter_id, is_deleted, concurrency_stamp, tenant_id
- Status: conversion enum ↔ string "pending"/"sent"/"fail"/"read"/"deactive" (max 10)
- Index (tùy nghiệp vụ): ví dụ `ix_system_event_notify_event_code`, `ix_system_event_notify_status`, `ix_system_event_notify_schedule_at` để tìm kiếm nhanh
- **Không** bắt buộc FK tới ResEvent hay ResAppChannel nếu DLL không yêu cầu (có thể để Guid và validate ở tầng app nếu cần)

#### 4.2. Repository Implementation
**Location**: `src/common/infra/iOne.EntityFrameworkCore/SystemEventNotifies/EfCoreSystemEventNotifyRepository.cs`

**Yêu cầu**:
- Kế thừa `EfCoreRepository<iOneDbContext, SystemEventNotify, Guid>`
- Implement `ISystemEventNotifyRepository`
- Có thể override `GetQueryableAsync()` hoặc thêm method filter nếu cần

#### 4.3. Register
**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`
- `modelBuilder.ApplyConfiguration(new SystemEventNotifyConfiguration());`
- `DbSet<SystemEventNotify> SystemEventNotifies` (nếu dùng convention)

---

### 5. Database Migration

**Yêu cầu**:
- Table name: `"system_event_notify"` (snake_case)
- Column names: snake_case (id, event_code, app_channel_id, title, body, payload, recipient_type, recipient_id, recipient, status, schedule_at, sent_at, read_at, error_message + full audit columns)
- Primary key: `pk_system_event_notify`
- **LUÔN** dùng `IF EXISTS` khi DROP, `IF NOT EXISTS` khi CREATE
- Table comment và comment các cột theo DLL (title, body, payload, recipient_type, recipient_id, recipient, status, schedule_at, sent_at, read_at, error_message)

**Migration Script Structure (ý tưởng)**:
```sql
CREATE TABLE IF NOT EXISTS "system_event_notify" (
    "id" UUID NOT NULL,
    "event_code" VARCHAR(50) NOT NULL,
    "app_channel_id" UUID NOT NULL,
    "title" VARCHAR(250) NOT NULL,
    "body" VARCHAR(500) NOT NULL,
    "payload" VARCHAR(500) NULL,
    "recipient_type" VARCHAR(10) NOT NULL,
    "recipient_id" UUID NOT NULL,
    "recipient" VARCHAR(50) NULL,
    "status" VARCHAR(10) NOT NULL,
    "schedule_at" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    "sent_at" TIMESTAMP WITHOUT TIME ZONE NULL,
    "read_at" TIMESTAMP WITHOUT TIME ZONE NULL,
    "error_message" VARCHAR(1000) NULL,
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
    CONSTRAINT "pk_system_event_notify" PRIMARY KEY ("id")
);

COMMENT ON TABLE "system_event_notify" IS 'Lưu các bản tin cần gửi của hệ thống (chưa gửi, hoặc gửi fail)';
COMMENT ON COLUMN "system_event_notify"."title" IS 'Tiêu đề bản tin, đã được thay thế tham số';
COMMENT ON COLUMN "system_event_notify"."body" IS 'Nội dung bản tin, đã được thay thế tham số';
COMMENT ON COLUMN "system_event_notify"."payload" IS 'Thông tin mô tả hành xử cho các hệ thống nhận tin, đã được thay thế các tham số';
COMMENT ON COLUMN "system_event_notify"."recipient_type" IS 'Loại đối tượng nhận tin: cus (khách hàng), emp (nhân viên)';
COMMENT ON COLUMN "system_event_notify"."recipient_id" IS 'Id tương ứng với nhân viên hoặc khách hàng';
COMMENT ON COLUMN "system_event_notify"."recipient" IS 'Địa chỉ nhận tin';
COMMENT ON COLUMN "system_event_notify"."status" IS 'Trạng thái: pending (chờ gửi), sent (đã gửi), fail (lỗi), read (đã đọc), deactive (đã xóa)';
COMMENT ON COLUMN "system_event_notify"."schedule_at" IS 'Thời gian dự kiến gửi';
COMMENT ON COLUMN "system_event_notify"."sent_at" IS 'Thời điểm gửi';
COMMENT ON COLUMN "system_event_notify"."read_at" IS 'Thời điểm đọc';
COMMENT ON COLUMN "system_event_notify"."error_message" IS 'Bản tin lỗi nếu có lỗi xảy ra';

CREATE INDEX IF NOT EXISTS "ix_system_event_notify_event_code" ON "system_event_notify" ("event_code");
CREATE INDEX IF NOT EXISTS "ix_system_event_notify_status" ON "system_event_notify" ("status");
CREATE INDEX IF NOT EXISTS "ix_system_event_notify_schedule_at" ON "system_event_notify" ("schedule_at");
```

---

### 6. Localization

#### 6.1. Localization Files
**Location**:
- `modules/Master/src/iOne.Master.Application.Contracts/Localization/Master/vi-VN.json`
- `modules/Master/src/iOne.Master.Application.Contracts/Localization/Master/en.json`

**Keys cần thêm** (ví dụ):
- Menu: `Menu:SystemEventNotify` (vd: "Tra cứu tin nhắn" / "Message lookup")
- Permission: `Permission:SystemEventNotify`
- Field: SystemEventNotify:EventCode, AppChannelId, Title, Body, Payload, RecipientType, RecipientId, Recipient, Status, ScheduleAt, SentAt, ReadAt, ErrorMessage
- Validation: SystemEventNotify:EventCodeRequired, TitleRequired, ... (các trường bắt buộc/độ dài)
- Success: SystemEventNotify:CreatedSuccessfully, UpdatedSuccessfully, DeletedSuccessfully
- Edit form: SystemEventNotify:OnlyStatusEditable (chỉ được sửa trạng thái)
- Status: SystemEventNotify:Pending, Sent, Fail, Read, Deactive

---

### 7. HTTP API Controller

**Location**: `modules/Master/src/iOne.Master.HttpApi/Controllers/SystemEventNotifyController.cs`

**Yêu cầu**:
- Kế thừa `AbpControllerBase`
- Route: `"api/master/system-event-notifies"` (hoặc `"api/master/message-lookup"` tùy convention)
- Attributes: `[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]`, `[Area(MasterRemoteServiceConsts.ModuleName)]`, `[Authorize]`
- Methods:
  - GetAsync(Guid id) – `[HttpGet("{id}")]`, `[Authorize(SystemEventNotifyPermissions.View)]`
  - GetListAsync(GetSystemEventNotifiesInput input) – `[HttpGet]`, `[Authorize(SystemEventNotifyPermissions.View)]`
  - CreateAsync(CreateSystemEventNotifyDto input) – `[HttpPost]`, `[Authorize(SystemEventNotifyPermissions.Create)]`
  - UpdateAsync(Guid id, UpdateSystemEventNotifyDto input) – `[HttpPut("{id}")]`, `[Authorize(SystemEventNotifyPermissions.Edit)]`
  - DeleteAsync(Guid id) – `[HttpDelete("{id}")]`, `[Authorize(SystemEventNotifyPermissions.Delete)]`

#### Exclude Conventional Controllers
**Location**: `modules/Master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`
- Thêm `SystemEventNotifyAppService` vào `TypePredicate` để exclude khỏi conventional controller generation (tránh duplicate API).

---

### 8. Menu (Navigation)

**Location**: `modules/Master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

**Yêu cầu**:
- Thêm menu item (dưới nhóm Master):
  - Name: `"Master.SystemEventNotify"`
  - Text: `masterL["Menu:SystemEventNotify"]`
  - Url: `~/pages/master/system-event-notifies` (hoặc `~/pages/master/message-lookup` tùy route frontend)
  - Icon: ví dụ `pi pi-fw pi-envelope` hoặc `pi pi-fw pi-comments`
  - `RequirePermissions(SystemEventNotifyPermissions.Default)`

---

## ✅ Checklist Trước Khi Hoàn Thành

### Domain Layer
- [ ] Entity `SystemEventNotify.cs` với table `system_event_notify` (snake_case)
- [ ] Enum `SystemEventNotifyStatus.cs` (Pending, Sent, Fail, Read, Deactive)
- [ ] Repository interface `ISystemEventNotifyRepository.cs`
- [ ] Manager `SystemEventNotifyManager.cs` (CreateAsync, UpdateStatusAsync – chỉ status)
- [ ] Entity chỉ có method `UpdateStatus()` cho sửa (không update các trường khác)

### Application Layer
- [ ] DTOs: SystemEventNotifyDto, CreateSystemEventNotifyDto, UpdateSystemEventNotifyDto (chỉ Status), GetSystemEventNotifiesInput
- [ ] ISystemEventNotifyAppService, SystemEventNotifyAppService
- [ ] AutoMapper: SystemEventNotify ↔ DTOs
- [ ] UpdateAsync: chỉ đọc input.Status và gọi UpdateStatusAsync
- [ ] DeleteAsync: Delete → Save → Update Status Deactive → Update → Save

### Permissions
- [ ] SystemEventNotifyPermissions.cs, SystemEventNotifyPermissionDefinitionProvider.cs
- [ ] Đăng ký provider trong module

### EF Core
- [ ] SystemEventNotifyConfiguration: table + column snake_case, comment, status conversion
- [ ] EfCoreSystemEventNotifyRepository, đăng ký trong DbContext

### Migration
- [ ] Table/column/index naming snake_case, IF EXISTS / IF NOT EXISTS, comments theo DLL

### Localization
- [ ] vi-VN.json và en.json – đủ key cho Menu, Permission, form, validation, message, status

### HTTP API & Menu
- [ ] SystemEventNotifyController với đủ endpoint và Authorize
- [ ] Exclude SystemEventNotifyAppService trong HttpApi module
- [ ] Menu item với RequirePermissions(SystemEventNotifyPermissions.Default)

### Kiểm tra nghiệp vụ
- [ ] Sửa: chỉ cho phép sửa Status (UpdateDto chỉ có Status, UpdateAsync chỉ gọi UpdateStatusAsync)
- [ ] Xóa: audit log (soft delete) trước, cập nhật status deactive sau (đúng thứ tự)

---

## 🔍 Lưu Ý Quan Trọng

1. **Naming**: Table và column **bắt buộc** snake_case (PostgreSQL): `system_event_notify`, `event_code`, `app_channel_id`, `recipient_type`, `schedule_at`, `sent_at`, `read_at`, `error_message`, ...
2. **Chỉ sửa Status khi Update**: UpdateSystemEventNotifyDto chỉ có thuộc tính Status; UpdateAsync chỉ cập nhật Status, không đọc/ghi các trường khác từ input.
3. **Xóa (Delete)**: Thứ tự bắt buộc – Delete (soft delete → audit log) → SaveChanges → set Status = Deactive → Update → SaveChanges.
4. **Enum Status**: Thêm giá trị Deactive (ngoài pending, sent, fail, read) để đánh dấu bản tin đã bị xóa/ngừng.
5. **Đa ngôn ngữ**: Mọi label, message, permission đều có key vi-VN và en.
6. **Phân quyền**: Tất cả API bảo vệ bằng SystemEventNotifyPermissions (View, Create, Edit, Delete).
7. **AutoMapper**: UpdateSystemEventNotifyDto có thể chỉ map Status khi map sang Entity; trong AppService vẫn nên chỉ gọi UpdateStatusAsync(entity, input.Status) để rõ nghiệp vụ.

---

## 🚀 Thứ Tự Thực Hiện Đề Xuất

1. Domain: Enum → Entity → IRepository → Manager  
2. EF Core: Configuration → Repository implementation → DbContext  
3. Application: DTOs → IAppService → AppService → AutoMapper  
4. Permissions: Constants → DefinitionProvider → đăng ký  
5. Localization: vi-VN + en  
6. HTTP API: Controller + exclude conventional  
7. Menu: MasterMenuContributor  
8. Migration: tạo và kiểm tra  
9. Build + test API + test permission + test localization

---

## 📌 Reference

- Quy tắc: `dev_note/RULES_BACKEND_DEVELOPMENT.md`
- Plan mẫu: `dev_note/PLAN_RESEVENT_BACKEND.md`, `dev_note/PLAN_RESUSERDEVICE_BACKEND.md`
- Entity/AppService mẫu: ResEvent (DeleteAsync + status deactive), ResUserDevice (Update chỉ một số trường, Delete flow)
