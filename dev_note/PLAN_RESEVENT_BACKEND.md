# Plan Xây Dựng Backend: Danh mục Sự kiện (ResEvent) và Template Cảnh báo (ResEventNotifyTemplate)

## 📋 Tổng Quan

**Module**: Master (Danh mục)  
**Entity Chính**: ResEvent (Sự kiện)  
**Entity Phụ**: ResEventNotifyTemplate (Template cảnh báo)  
**Table Names**: 
- `res_event` (snake_case - PostgreSQL convention)
- `res_event_notify_template` (snake_case - PostgreSQL convention)  
**Entity Name Pattern**: Theo pattern `Res*` trong module Master  
**Relationship**: ResEvent (1) → ResEventNotifyTemplate (N)

---

## 🎯 Yêu Cầu Chức Năng

### ResEvent (Sự kiện)
1. ✅ **CRUD Operations**: Xem, Thêm, Sửa, Xóa
2. ✅ **Validation Code**: Mã sự kiện chỉ cho phép A-Z, _, 0-9 (uppercase)
3. ✅ **Code Immutable**: Khi sửa không được phép sửa mã sự kiện (disable trên UI)
4. ✅ **Code Unique**: Mã sự kiện là duy nhất
5. ✅ **Soft Delete**: Khi xóa → soft delete (ABP audit log) → cập nhật status về Deactive
6. ✅ **Đa ngôn ngữ**: vi-VN và en
7. ✅ **Phân quyền**: View, Create, Edit, Delete

### ResEventNotifyTemplate (Template cảnh báo)
1. ✅ **CRUD Operations**: Xem, Thêm, Sửa, Xóa (theo EventId)
2. ✅ **Unique Constraint**: Mỗi kênh (AppChannelId) có một template duy nhất trên một sự kiện (EventId)
3. ✅ **Relationship**: Foreign key đến ResEvent và ResAppChannel
4. ✅ **Data Field**: Lưu JSON string cho screen navigation data
5. ✅ **Đa ngôn ngữ**: vi-VN và en
6. ✅ **Phân quyền**: View, Create, Edit, Delete (có thể dùng chung với ResEvent hoặc tách riêng)

### API Requirements
1. ✅ **Get Templates by EventId**: API để lấy danh sách template theo EventId
2. ✅ **Create/Update/Delete Template**: CRUD operations cho template

---

## 📁 Cấu Trúc Files Cần Tạo

### 1. Domain Layer

#### 1.1. Entity ResEvent
**Location**: `src/common/domain/iOne.Domain/ResEvents/ResEvent.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>`
- Table name: `[Table("res_event")]` (snake_case)
- Properties:
  - `Code` (string, max 50, required, private setter)
  - `Name` (string, max 50, required, private setter)
  - `Description` (string, max 500, optional, private setter)
  - `Status` (ResEventStatus enum, required, private setter)
  - `NotifyTemplates` (ICollection<ResEventNotifyTemplate>, navigation property)
- Constructor với validation
- Private setters với methods: `SetCode()`, `SetName()`, `SetDescription()`, `SetStatus()`
- Public methods: `UpdateName()`, `UpdateDescription()`, `UpdateStatus()` (KHÔNG có `UpdateCode()`)
- Validation Code: Regex `^[A-Z0-9_]+$` (uppercase)

#### 1.2. Entity ResEventNotifyTemplate
**Location**: `src/common/domain/iOne.Domain/ResEvents/ResEventNotifyTemplate.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedEntity<Guid>` (không phải AggregateRoot vì là child entity)
- Table name: `[Table("res_event_notify_template")]` (snake_case)
- Properties:
  - `EventId` (Guid, required, private setter) - Foreign key đến ResEvent
  - `AppChannelId` (Guid, required, private setter) - Foreign key đến ResAppChannel
  - `RetryNumber` (int, required, default 0, private setter)
  - `Title` (string, max 250, required, private setter)
  - `Body` (string, max 500, required, private setter)
  - `Data` (string, max 1000, optional, private setter) - JSON string
  - `Event` (ResEvent, navigation property)
  - `AppChannel` (ResAppChannel, navigation property)
- Constructor với validation
- Private setters với methods: `SetEventId()`, `SetAppChannelId()`, `SetRetryNumber()`, `SetTitle()`, `SetBody()`, `SetData()`
- Public methods: `UpdateRetryNumber()`, `UpdateTitle()`, `UpdateBody()`, `UpdateData()`, `UpdateAppChannelId()`
- Validation: Title và Body có thể chứa `${param_name}` placeholders

#### 1.3. Enum Status
**Location**: `src/common/domain/iOne.Domain.Shared/ResEvents/ResEventStatus.cs`

**Yêu cầu**:
- `Active = 0` (Hoạt động)
- `Deactive = 1` (Không hoạt động)

#### 1.4. Repository Interfaces
**Location**: 
- `src/common/domain/iOne.Domain/ResEvents/IResEventRepository.cs`
- `src/common/domain/iOne.Domain/ResEvents/IResEventNotifyTemplateRepository.cs`

**Yêu cầu**:
- `IResEventRepository`: Kế thừa từ `IRepository<ResEvent, Guid>`
  - Method: `IsCodeExistsAsync(string code)` để check code uniqueness
- `IResEventNotifyTemplateRepository`: Kế thừa từ `IRepository<ResEventNotifyTemplate, Guid>`
  - Method: `GetListByEventIdAsync(Guid eventId)` để lấy danh sách template theo EventId
  - Method: `IsTemplateExistsAsync(Guid eventId, Guid appChannelId)` để check unique constraint (EventId + AppChannelId)

#### 1.5. Managers
**Location**: 
- `src/common/domain/iOne.Domain/ResEvents/ResEventManager.cs`
- `src/common/domain/iOne.Domain/ResEvents/ResEventNotifyTemplateManager.cs`

**Yêu cầu**:
- `ResEventManager`:
  - `CreateAsync(ResEvent entity)`: Check code uniqueness, insert
  - `UpdateAsync(ResEvent entity, string name, string? description, ResEventStatus status)`: Update name, description và status (KHÔNG có code parameter)
- `ResEventNotifyTemplateManager`:
  - `CreateAsync(ResEventNotifyTemplate entity)`: Check unique constraint (EventId + AppChannelId), insert
  - `UpdateAsync(ResEventNotifyTemplate entity, Guid appChannelId, int retryNumber, string title, string body, string? data)`: Update template

---

### 2. Application Layer

#### 2.1. DTOs - ResEvent
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/ResEvents/`

**Files**:
- `ResEventDto.cs`: Kế thừa `FullAuditedEntityDto<Guid>`
  - Properties: `Id`, `Code`, `Name`, `Description`, `Status`, audit fields
- `CreateResEventDto.cs`:
  - Properties: `Code` (required, max 50), `Name` (required, max 50), `Description` (optional, max 500), `Status` (required)
- `UpdateResEventDto.cs`:
  - Properties: `Name` (required, max 50), `Description` (optional, max 500), `Status` (required)
  - **KHÔNG có Code** (vì không được phép sửa)
- `GetResEventsInput.cs`: Kế thừa `PagedAndSortedResultRequestDto`
  - Properties: `Code?`, `Name?`, `Status?` (optional filters)

#### 2.2. DTOs - ResEventNotifyTemplate
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/ResEvents/`

**Files**:
- `ResEventNotifyTemplateDto.cs`: Kế thừa `FullAuditedEntityDto<Guid>`
  - Properties: `Id`, `EventId`, `AppChannelId`, `AppChannelCode`, `AppChannelName`, `RetryNumber`, `Title`, `Body`, `Data`, audit fields
  - Note: Include AppChannelCode và AppChannelName để hiển thị trên UI
- `CreateResEventNotifyTemplateDto.cs`:
  - Properties: `EventId` (required), `AppChannelId` (required), `RetryNumber` (required, default 0), `Title` (required, max 250), `Body` (required, max 500), `Data` (optional, max 1000)
- `UpdateResEventNotifyTemplateDto.cs`:
  - Properties: `AppChannelId` (required), `RetryNumber` (required), `Title` (required, max 250), `Body` (required, max 500), `Data` (optional, max 1000)
- `GetResEventNotifyTemplatesInput.cs`: Kế thừa `PagedAndSortedResultRequestDto`
  - Properties: `EventId` (required) - Filter theo EventId

#### 2.3. Application Service Interfaces
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/ResEvents/`

**Files**:
- `IResEventAppService.cs`: Kế thừa từ `ICrudAppService<ResEventDto, Guid, GetResEventsInput, CreateResEventDto, UpdateResEventDto>`
- `IResEventNotifyTemplateAppService.cs`: 
  - Methods: 
    - `GetListByEventIdAsync(Guid eventId)` → `Task<PagedResultDto<ResEventNotifyTemplateDto>>`
    - `GetAsync(Guid id)` → `Task<ResEventNotifyTemplateDto>`
    - `CreateAsync(CreateResEventNotifyTemplateDto input)` → `Task<ResEventNotifyTemplateDto>`
    - `UpdateAsync(Guid id, UpdateResEventNotifyTemplateDto input)` → `Task<ResEventNotifyTemplateDto>`
    - `DeleteAsync(Guid id)` → `Task`
  - Tất cả methods có `[Authorize]` attribute

#### 2.4. Application Service Implementations
**Location**: `modules/Master/src/iOne.Master.Application/ResEvents/`

**Files**:
- `ResEventAppService.cs`: 
  - Kế thừa từ `CrudAppService<ResEvent, ResEventDto, Guid, GetResEventsInput, CreateResEventDto, UpdateResEventDto>`
  - Inject: `IResEventRepository`, `ResEventManager`
  - `CreateAsync()`: Tạo entity mới, gọi Manager.CreateAsync()
  - `UpdateAsync()`: Load entity, gọi Manager.UpdateAsync() (chỉ name, description và status)
  - `DeleteAsync()`: 
    1. Load entity
    2. `Repository.DeleteAsync(entity)` → trigger ABP audit log (soft delete)
    3. `CurrentUnitOfWork.SaveChangesAsync()`
    4. `entity.UpdateStatus(ResEventStatus.Deactive)`
    5. `Repository.UpdateAsync(entity)`
    6. `CurrentUnitOfWork.SaveChangesAsync()`
  - `CreateFilteredQueryAsync()`: Filter theo Code, Name, Status
  - Set policies: `GetPolicyName`, `GetListPolicyName`, `CreatePolicyName`, `UpdatePolicyName`, `DeletePolicyName`

- `ResEventNotifyTemplateAppService.cs`:
  - Inject: `IResEventNotifyTemplateRepository`, `ResEventNotifyTemplateManager`, `IResAppChannelRepository` (để lấy AppChannel info)
  - `GetListByEventIdAsync()`: Lấy danh sách template theo EventId, map AppChannel info
  - `CreateAsync()`: Check unique constraint (EventId + AppChannelId), tạo entity mới
  - `UpdateAsync()`: Load entity, gọi Manager.UpdateAsync()
  - `DeleteAsync()`: Soft delete template
  - Set policies: `GetPolicyName`, `GetListPolicyName`, `CreatePolicyName`, `UpdatePolicyName`, `DeletePolicyName`

#### 2.5. AutoMapper Configuration
**Location**: `modules/Master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

**Yêu cầu**:
- Thêm mappings:
  - `CreateMap<ResEvent, ResEventDto>()`
  - `CreateMap<CreateResEventDto, ResEvent>()`
  - `CreateMap<UpdateResEventDto, ResEvent>()`
  - `CreateMap<ResEventNotifyTemplate, ResEventNotifyTemplateDto>()`
  - `CreateMap<CreateResEventNotifyTemplateDto, ResEventNotifyTemplate>()`
  - `CreateMap<UpdateResEventNotifyTemplateDto, ResEventNotifyTemplate>()`

---

### 3. Permissions

#### 3.1. Permission Constants
**Location**: `modules/Master/src/iOne.Master.Application.Contracts/Permissions/ResEventPermissions.cs`

**Yêu cầu**:
```csharp
public const string GroupName = "MasterResEvent";
public const string Default = GroupName;
public const string Create = Default + ".Create";
public const string Edit = Default + ".Edit";
public const string Delete = Default + ".Delete";
public const string View = Default + ".View";
```

**Location**: `modules/Master/src/iOne.Master.Application.Contracts/Permissions/ResEventNotifyTemplatePermissions.cs`

**Yêu cầu**:
```csharp
public const string GroupName = "MasterResEventNotifyTemplate";
public const string Default = GroupName;
public const string Create = Default + ".Create";
public const string Edit = Default + ".Edit";
public const string Delete = Default + ".Delete";
public const string View = Default + ".View";
```

#### 3.2. Permission Definition Providers
**Location**: 
- `modules/Master/src/iOne.Master.Application.Contracts/Permissions/ResEventPermissionDefinitionProvider.cs`
- `modules/Master/src/iOne.Master.Application.Contracts/Permissions/ResEventNotifyTemplatePermissionDefinitionProvider.cs`

**Yêu cầu**:
- Tạo permission group cho mỗi entity
- Tạo Default permission = GroupName
- Thêm child permissions: Create, Edit, Delete, View
- Sử dụng localization từ `MasterResource`

---

### 4. Entity Framework Core

#### 4.1. Entity Configurations

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResEvents/ResEventConfiguration.cs`

**Yêu cầu**:
- Table name: `"res_event"` (snake_case)
- Column names: `"id"`, `"code"`, `"name"`, `"description"`, `"status"` (snake_case)
- Audit columns: `"creation_time"`, `"creator_id"`, `"last_modification_time"`, `"last_modifier_id"`, `"deletion_time"`, `"deleter_id"`, `"is_deleted"`, `"concurrency_stamp"`, `"tenant_id"` (snake_case)
- Status enum conversion: `Active` → `"active"`, `Deactive` → `"deactive"` (lowercase string)
- Index: Unique index trên `code` với name `"ix_res_event_code"`
- Relationship: Configure one-to-many với ResEventNotifyTemplate
  ```csharp
  builder.HasMany(e => e.NotifyTemplates)
      .WithOne(e => e.Event)
      .HasForeignKey(e => e.EventId)
      .OnDelete(DeleteBehavior.Cascade)
      .HasConstraintName("fk_res_event_notify_template_event_id");
  ```

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResEvents/ResEventNotifyTemplateConfiguration.cs`

**Yêu cầu**:
- Table name: `"res_event_notify_template"` (snake_case)
- Column names: `"id"`, `"event_id"`, `"app_channel_id"`, `"retry_number"`, `"title"`, `"body"`, `"data"` (snake_case)
- Audit columns: snake_case
- Foreign keys:
  - `event_id` → `res_event.id` với constraint name `"fk_res_event_notify_template_event_id"`
  - `app_channel_id` → `res_app_channel.id` với constraint name `"fk_res_event_notify_template_app_channel_id"`
- Unique constraint: `(event_id, app_channel_id)` với name `"uq_res_event_notify_template_event_channel"`
- Index: Index trên `event_id` với name `"ix_res_event_notify_template_event_id"`
- Relationships:
  ```csharp
  builder.HasOne(e => e.Event)
      .WithMany(e => e.NotifyTemplates)
      .HasForeignKey(e => e.EventId)
      .OnDelete(DeleteBehavior.Cascade)
      .HasConstraintName("fk_res_event_notify_template_event_id");

  builder.HasOne(e => e.AppChannel)
      .WithMany()
      .HasForeignKey(e => e.AppChannelId)
      .OnDelete(DeleteBehavior.Restrict)
      .HasConstraintName("fk_res_event_notify_template_app_channel_id");
  ```

#### 4.2. Repository Implementations
**Location**: 
- `src/common/infra/iOne.EntityFrameworkCore/ResEvents/EfCoreResEventRepository.cs`
- `src/common/infra/iOne.EntityFrameworkCore/ResEvents/EfCoreResEventNotifyTemplateRepository.cs`

**Yêu cầu**:
- `EfCoreResEventRepository`: 
  - Kế thừa từ `EfCoreRepository<iOneDbContext, ResEvent, Guid>`
  - Implement `IResEventRepository`
  - Method `IsCodeExistsAsync()`: Check code uniqueness (case-insensitive)

- `EfCoreResEventNotifyTemplateRepository`:
  - Kế thừa từ `EfCoreRepository<iOneDbContext, ResEventNotifyTemplate, Guid>`
  - Implement `IResEventNotifyTemplateRepository`
  - Method `GetListByEventIdAsync()`: Query với Include AppChannel, filter theo EventId
  - Method `IsTemplateExistsAsync()`: Check unique constraint (EventId + AppChannelId), exclude current entity nếu update

#### 4.3. Register Configurations
**Location**: `src/common/infra/iOne.EntityFrameworkCore/iOneDbContext.cs`

**Yêu cầu**:
- Thêm `modelBuilder.ApplyConfiguration(new ResEventConfiguration());`
- Thêm `modelBuilder.ApplyConfiguration(new ResEventNotifyTemplateConfiguration());`

---

### 5. Database Migration

**Yêu cầu**:
- Table names: `"res_event"`, `"res_event_notify_template"` (snake_case)
- Column names: snake_case
- Primary keys: `"pk_res_event"`, `"pk_res_event_notify_template"` (snake_case với prefix)
- Indexes: snake_case với prefix
- Foreign keys: snake_case với prefix
- Unique constraint: `"uq_res_event_notify_template_event_channel"`
- **LUÔN** sử dụng `IF EXISTS` khi DROP, `IF NOT EXISTS` khi CREATE
- Table và column comments

**Migration Script Structure**:

```sql
-- Table res_event
CREATE TABLE IF NOT EXISTS "res_event" (
    "id" UUID NOT NULL,
    "code" VARCHAR(50) NOT NULL,
    "name" VARCHAR(50) NOT NULL,
    "description" VARCHAR(500),
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
    CONSTRAINT "pk_res_event" PRIMARY KEY ("id")
);

COMMENT ON TABLE "res_event" IS 'Danh sách sự kiện hệ thống và thiết lập template cảnh báo';
COMMENT ON COLUMN "res_event"."status" IS 'Trạng thái:
- active: Hoạt động
- deactive: Không hoạt động';

CREATE UNIQUE INDEX IF NOT EXISTS "ix_res_event_code" ON "res_event" ("code");

-- Table res_event_notify_template
CREATE TABLE IF NOT EXISTS "res_event_notify_template" (
    "id" UUID NOT NULL,
    "event_id" UUID NOT NULL,
    "app_channel_id" UUID NOT NULL,
    "retry_number" NUMERIC(2) NOT NULL DEFAULT 0,
    "title" VARCHAR(250) NOT NULL,
    "body" VARCHAR(500) NOT NULL,
    "data" VARCHAR(1000),
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
    CONSTRAINT "pk_res_event_notify_template" PRIMARY KEY ("id"),
    CONSTRAINT "fk_res_event_notify_template_event_id" FOREIGN KEY ("event_id") REFERENCES "res_event" ("id") ON DELETE CASCADE,
    CONSTRAINT "fk_res_event_notify_template_app_channel_id" FOREIGN KEY ("app_channel_id") REFERENCES "res_app_channel" ("id") ON DELETE RESTRICT
);

COMMENT ON TABLE "res_event_notify_template" IS 'Template cảnh báo tương ứng với sự kiện';
COMMENT ON COLUMN "res_event_notify_template"."retry_number" IS 'Số lần retry, mặc định là 0';
COMMENT ON COLUMN "res_event_notify_template"."title" IS 'Tiêu đề bản tin, có thể có tham số truyền vào, nếu có thêm số thì để dạng ${param_name}';
COMMENT ON COLUMN "res_event_notify_template"."body" IS 'Nội dung của bản tin, có thể có tham số truyền vào, nếu có tham số truyền vào thì tham số có dạng ${param_name}';
COMMENT ON COLUMN "res_event_notify_template"."data" IS 'Data có dạng json:
{
"screen": "invoice_detail",
"invoice_id": "INV20250911001",
....
}';

CREATE UNIQUE INDEX IF NOT EXISTS "uq_res_event_notify_template_event_channel" ON "res_event_notify_template" ("event_id", "app_channel_id");
CREATE INDEX IF NOT EXISTS "ix_res_event_notify_template_event_id" ON "res_event_notify_template" ("event_id");
```

---

### 6. Localization

#### 6.1. Localization Files
**Location**: 
- `modules/Master/src/iOne.Master.Application.Contracts/Localization/Master/vi-VN.json`
- `modules/Master/src/iOne.Master.Application.Contracts/Localization/Master/en.json`

**Keys cần thêm**:

**vi-VN.json**:
```json
{
  "Menu:ResEvent": "Danh mục Sự kiện",
  "Permission:ResEvent": "Sự kiện",
  "ResEvent:Code": "Mã sự kiện",
  "ResEvent:Name": "Tên sự kiện",
  "ResEvent:Description": "Mô tả",
  "ResEvent:Status": "Trạng thái",
  "ResEvent:CodeRequired": "Mã sự kiện là bắt buộc",
  "ResEvent:CodeMaxLength": "Mã sự kiện không được vượt quá 50 ký tự",
  "ResEvent:CodeInvalid": "Mã sự kiện chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
  "ResEvent:NameRequired": "Tên sự kiện là bắt buộc",
  "ResEvent:NameMaxLength": "Tên sự kiện không được vượt quá 50 ký tự",
  "ResEvent:DescriptionMaxLength": "Mô tả không được vượt quá 500 ký tự",
  "ResEvent:StatusRequired": "Trạng thái là bắt buộc",
  "ResEvent:CodeExists": "Mã sự kiện '{Code}' đã tồn tại",
  "ResEvent:CreatedSuccessfully": "Tạo sự kiện thành công",
  "ResEvent:UpdatedSuccessfully": "Cập nhật sự kiện thành công",
  "ResEvent:DeletedSuccessfully": "Xóa sự kiện thành công",
  "ResEvent:New": "Thêm mới sự kiện",
  "ResEvent:Edit": "Sửa sự kiện",
  "ResEvent:Delete": "Xóa sự kiện",
  "ResEvent:DeleteConfirm": "Bạn có chắc chắn muốn xóa sự kiện này?",
  "ResEvent:CodeCannotBeChanged": "Mã sự kiện không được phép thay đổi",
  "ResEvent:Active": "Hoạt động",
  "ResEvent:Deactive": "Không hoạt động",
  "ResEvent:SearchByCode": "Tìm theo mã sự kiện",
  "ResEvent:SearchByName": "Tìm theo tên sự kiện",
  "ResEvent:NotifyTemplates": "Template cảnh báo",
  "ResEvent:ConfigureTemplate": "Cấu hình template",
  
  "Permission:ResEventNotifyTemplate": "Template cảnh báo",
  "ResEventNotifyTemplate:EventId": "Sự kiện",
  "ResEventNotifyTemplate:AppChannelId": "Kênh",
  "ResEventNotifyTemplate:AppChannelCode": "Mã kênh",
  "ResEventNotifyTemplate:AppChannelName": "Tên kênh",
  "ResEventNotifyTemplate:RetryNumber": "Số lần retry",
  "ResEventNotifyTemplate:Title": "Tiêu đề",
  "ResEventNotifyTemplate:Body": "Nội dung",
  "ResEventNotifyTemplate:Data": "Dữ liệu JSON",
  "ResEventNotifyTemplate:EventIdRequired": "Sự kiện là bắt buộc",
  "ResEventNotifyTemplate:AppChannelIdRequired": "Kênh là bắt buộc",
  "ResEventNotifyTemplate:RetryNumberRequired": "Số lần retry là bắt buộc",
  "ResEventNotifyTemplate:TitleRequired": "Tiêu đề là bắt buộc",
  "ResEventNotifyTemplate:TitleMaxLength": "Tiêu đề không được vượt quá 250 ký tự",
  "ResEventNotifyTemplate:BodyRequired": "Nội dung là bắt buộc",
  "ResEventNotifyTemplate:BodyMaxLength": "Nội dung không được vượt quá 500 ký tự",
  "ResEventNotifyTemplate:DataMaxLength": "Dữ liệu JSON không được vượt quá 1000 ký tự",
  "ResEventNotifyTemplate:TemplateExists": "Template cho kênh '{AppChannelCode}' đã tồn tại cho sự kiện này",
  "ResEventNotifyTemplate:CreatedSuccessfully": "Tạo template thành công",
  "ResEventNotifyTemplate:UpdatedSuccessfully": "Cập nhật template thành công",
  "ResEventNotifyTemplate:DeletedSuccessfully": "Xóa template thành công",
  "ResEventNotifyTemplate:New": "Thêm mới template",
  "ResEventNotifyTemplate:Edit": "Sửa template",
  "ResEventNotifyTemplate:Delete": "Xóa template",
  "ResEventNotifyTemplate:DeleteConfirm": "Bạn có chắc chắn muốn xóa template này?"
}
```

**en.json**: (tương tự với bản dịch tiếng Anh)

---

### 7. HTTP API Controllers

#### 7.1. ResEvent Controller
**Location**: `modules/Master/src/iOne.Master.HttpApi/Controllers/ResEventController.cs`

**Yêu cầu**:
- Kế thừa từ `AbpControllerBase`
- Route: `"api/master/events"`
- Attributes:
  - `[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]`
  - `[Area(MasterRemoteServiceConsts.ModuleName)]`
  - `[Authorize]`
- Methods:
  - `GetAsync(Guid id)` → `[HttpGet("{id}")]` + `[Authorize(ResEventPermissions.View)]`
  - `GetListAsync(GetResEventsInput input)` → `[HttpGet]` + `[Authorize(ResEventPermissions.View)]`
  - `CreateAsync(CreateResEventDto input)` → `[HttpPost]` + `[Authorize(ResEventPermissions.Create)]`
  - `UpdateAsync(Guid id, UpdateResEventDto input)` → `[HttpPut("{id}")]` + `[Authorize(ResEventPermissions.Edit)]`
  - `DeleteAsync(Guid id)` → `[HttpDelete("{id}")]` + `[Authorize(ResEventPermissions.Delete)]`

#### 7.2. ResEventNotifyTemplate Controller
**Location**: `modules/Master/src/iOne.Master.HttpApi/Controllers/ResEventNotifyTemplateController.cs`

**Yêu cầu**:
- Kế thừa từ `AbpControllerBase`
- Route: `"api/master/event-notify-templates"`
- Attributes:
  - `[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]`
  - `[Area(MasterRemoteServiceConsts.ModuleName)]`
  - `[Authorize]`
- Methods:
  - `GetListByEventIdAsync(Guid eventId)` → `[HttpGet("by-event/{eventId}")]` + `[Authorize(ResEventNotifyTemplatePermissions.View)]`
  - `GetAsync(Guid id)` → `[HttpGet("{id}")]` + `[Authorize(ResEventNotifyTemplatePermissions.View)]`
  - `CreateAsync(CreateResEventNotifyTemplateDto input)` → `[HttpPost]` + `[Authorize(ResEventNotifyTemplatePermissions.Create)]`
  - `UpdateAsync(Guid id, UpdateResEventNotifyTemplateDto input)` → `[HttpPut("{id}")]` + `[Authorize(ResEventNotifyTemplatePermissions.Edit)]`
  - `DeleteAsync(Guid id)` → `[HttpDelete("{id}")]` + `[Authorize(ResEventNotifyTemplatePermissions.Delete)]`

#### 7.3. Exclude từ Conventional Controllers
**Location**: `modules/Master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`

**Yêu cầu**:
- Thêm `ResEventAppService` và `ResEventNotifyTemplateAppService` vào `TypePredicate` để exclude khỏi conventional controller generation

---

### 8. Menu Configuration

#### 8.1. Menu Contributor
**Location**: `modules/Master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

**Yêu cầu**:
- Thêm menu item:
```csharp
masterMenuItem.AddItem(new ApplicationMenuItem(
    "Master.ResEvent",
    masterL["Menu:ResEvent"],
    url: "~/pages/master/events",
    icon: "pi pi-fw pi-calendar"
).RequirePermissions(ResEventPermissions.Default));
```

---

## ✅ Checklist Trước Khi Hoàn Thành

### Domain Layer
- [ ] Entity `ResEvent.cs` với table name `res_event` (snake_case)
- [ ] Entity `ResEventNotifyTemplate.cs` với table name `res_event_notify_template` (snake_case)
- [ ] Enum `ResEventStatus.cs` (Active, Deactive)
- [ ] Repository interfaces: `IResEventRepository.cs`, `IResEventNotifyTemplateRepository.cs`
- [ ] Managers: `ResEventManager.cs`, `ResEventNotifyTemplateManager.cs`
- [ ] Entity ResEvent có private setters, không có `UpdateCode()` method
- [ ] Validation Code: Regex `^[A-Z0-9_]+$` (uppercase)
- [ ] Entity ResEventNotifyTemplate có navigation properties đến ResEvent và ResAppChannel

### Application Layer
- [ ] DTOs ResEvent: `ResEventDto`, `CreateResEventDto`, `UpdateResEventDto` (KHÔNG có Code), `GetResEventsInput`
- [ ] DTOs ResEventNotifyTemplate: `ResEventNotifyTemplateDto`, `CreateResEventNotifyTemplateDto`, `UpdateResEventNotifyTemplateDto`, `GetResEventNotifyTemplatesInput`
- [ ] Application Service Interfaces: `IResEventAppService.cs`, `IResEventNotifyTemplateAppService.cs`
- [ ] Application Service Implementations: `ResEventAppService.cs`, `ResEventNotifyTemplateAppService.cs`
- [ ] **AutoMapper configuration** đã thêm vào `iOneMasterApplicationAutoMapperProfile.cs`
- [ ] `ResEventAppService.DeleteAsync()` implement đúng: Delete → Save → Update Status → Save
- [ ] `ResEventNotifyTemplateAppService.GetListByEventIdAsync()` implement với Include AppChannel

### Permissions
- [ ] Permission Constants: `ResEventPermissions.cs`, `ResEventNotifyTemplatePermissions.cs`
- [ ] Permission Definition Providers: `ResEventPermissionDefinitionProvider.cs`, `ResEventNotifyTemplatePermissionDefinitionProvider.cs`
- [ ] Permissions được đăng ký trong module

### Entity Framework Core
- [ ] Entity Configuration `ResEventConfiguration.cs`
  - [ ] Table name: `"res_event"` (snake_case)
  - [ ] Column names: snake_case
  - [ ] Status enum conversion: Active → "active", Deactive → "deactive"
  - [ ] Unique index: `"ix_res_event_code"`
  - [ ] Relationship với ResEventNotifyTemplate
- [ ] Entity Configuration `ResEventNotifyTemplateConfiguration.cs`
  - [ ] Table name: `"res_event_notify_template"` (snake_case)
  - [ ] Column names: snake_case
  - [ ] Foreign keys: `fk_res_event_notify_template_event_id`, `fk_res_event_notify_template_app_channel_id`
  - [ ] Unique constraint: `uq_res_event_notify_template_event_channel`
  - [ ] Index: `ix_res_event_notify_template_event_id`
- [ ] Repository Implementations: `EfCoreResEventRepository.cs`, `EfCoreResEventNotifyTemplateRepository.cs`
- [ ] Configurations đã register trong `iOneDbContext.cs`

### Database Migration
- [ ] Migration script với table names: `"res_event"`, `"res_event_notify_template"` (snake_case)
- [ ] Column names: snake_case
- [ ] Index names: snake_case với prefix
- [ ] Primary key names: snake_case với prefix
- [ ] Foreign key names: snake_case với prefix
- [ ] Unique constraint name: snake_case với prefix
- [ ] Sử dụng `IF EXISTS`/`IF NOT EXISTS`
- [ ] Table và column comments

### Localization
- [ ] Localization keys đã thêm vào `vi-VN.json`
- [ ] Localization keys đã thêm vào `en.json`
- [ ] Keys match nhau giữa 2 file

### HTTP API
- [ ] Controller `ResEventController.cs` với đầy đủ endpoints
- [ ] Controller `ResEventNotifyTemplateController.cs` với đầy đủ endpoints
- [ ] Controllers có `[Authorize]` attributes với permissions
- [ ] `ResEventAppService` và `ResEventNotifyTemplateAppService` đã exclude khỏi conventional controllers

### Menu
- [ ] Menu item đã thêm vào `MasterMenuContributor.cs`
- [ ] Menu có permission check

### Testing
- [ ] Build solution thành công
- [ ] Migration chạy thành công
- [ ] Test API endpoints ResEvent:
  - [ ] GET `/api/master/events` (list)
  - [ ] GET `/api/master/events/{id}` (detail)
  - [ ] POST `/api/master/events` (create)
  - [ ] PUT `/api/master/events/{id}` (update - không được sửa Code)
  - [ ] DELETE `/api/master/events/{id}` (delete - soft delete + status deactive)
- [ ] Test API endpoints ResEventNotifyTemplate:
  - [ ] GET `/api/master/event-notify-templates/by-event/{eventId}` (list by eventId)
  - [ ] GET `/api/master/event-notify-templates/{id}` (detail)
  - [ ] POST `/api/master/event-notify-templates` (create)
  - [ ] PUT `/api/master/event-notify-templates/{id}` (update)
  - [ ] DELETE `/api/master/event-notify-templates/{id}` (delete)
- [ ] Test validation:
  - [ ] Code chỉ chấp nhận A-Z, 0-9, _
  - [ ] Code là unique
  - [ ] Update không được sửa Code
  - [ ] Unique constraint: EventId + AppChannelId
- [ ] Test permissions
- [ ] Test localization (vi-VN và en)
- [ ] Test menu hiển thị đúng

---

## 🔍 Lưu Ý Quan Trọng

1. **Table và Column Names**: **BẮT BUỘC** sử dụng **snake_case** (PostgreSQL convention)
   - Tables: `"res_event"`, `"res_event_notify_template"` (không phải UPPERCASE hay PascalCase)
   - Columns: snake_case

2. **Code Immutable**: Code không được phép sửa sau khi tạo
   - Entity không có `UpdateCode()` method
   - `UpdateResEventDto` không có `Code` property
   - Manager `UpdateAsync()` không có `code` parameter

3. **Soft Delete Flow**: 
   - Gọi `Repository.DeleteAsync()` trước để trigger ABP audit log
   - Sau đó mới update status về Deactive
   - Đảm bảo có 2 lần `SaveChangesAsync()` để audit log ghi nhận đúng

4. **Unique Constraint**: 
   - Mỗi kênh (AppChannelId) có một template duy nhất trên một sự kiện (EventId)
   - Unique constraint: `(event_id, app_channel_id)`
   - Validation trong Manager và AppService

5. **Relationship**: 
   - ResEventNotifyTemplate là child entity của ResEvent
   - Foreign key `event_id` với `ON DELETE CASCADE`
   - Foreign key `app_channel_id` với `ON DELETE RESTRICT`

6. **AutoMapper**: **BẮT BUỘC** thêm mappings vào `iOneMasterApplicationAutoMapperProfile.cs`

7. **Conventional Controllers**: Phải exclude `ResEventAppService` và `ResEventNotifyTemplateAppService` khỏi conventional controller generation

8. **Data Field**: 
   - Field `Data` lưu JSON string
   - Frontend cần validate JSON format
   - Backend chỉ validate max length, không validate JSON structure

9. **Template Placeholders**: 
   - Title và Body có thể chứa `${param_name}` placeholders
   - Backend không validate placeholder format, chỉ validate max length

---

## 🚀 Thứ Tự Thực Hiện

1. **Domain Layer** (Entities, Enums, Repository Interfaces, Managers)
2. **EF Core Configuration** (Entity Configurations, Repository Implementations)
3. **Application Layer** (DTOs, AppService Interfaces, AppService Implementations)
4. **AutoMapper Configuration**
5. **Permissions** (Constants, Definition Providers)
6. **Localization** (vi-VN và en)
7. **HTTP API Controllers**
8. **Menu Configuration**
9. **Database Migration**
10. **Testing**

---

## 📌 Reference Files

- Entity mẫu: `src/common/domain/iOne.Domain/ResAppChannels/ResAppChannel.cs`
- AppService mẫu: `modules/Master/src/iOne.Master.Application/ResAppChannels/ResAppChannelAppService.cs`
- Configuration mẫu: `src/common/infra/iOne.EntityFrameworkCore/ResAppChannels/ResAppChannelConfiguration.cs`
- Plan mẫu: `dev_note/PLAN_RESBANK_BACKEND.md`
- Rules: `dev_note/RULES_BACKEND_DEVELOPMENT.md`

