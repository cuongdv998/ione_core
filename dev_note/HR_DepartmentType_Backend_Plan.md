# Plan: Backend Danh mục Loại đơn vị (HR Department Type)

## 1. Tổng quan
Tạo module backend cho quản lý Danh mục Loại đơn vị với các tính năng CRUD đầy đủ, validation nghiêm ngặt và audit logging.

## 2. Cấu trúc Database
**Bảng: HrDepartmentType**
- **Table Comment**: "Loại đơn vị: - CT: công ty - CN: chi nhánh - DV: đơn vị - PB: phòng ban"
- Id (UUID) - Primary Key (PostgreSQL UUID type)
- Code (VARCHAR(50)) - NOT NULL, UNIQUE
- Name (VARCHAR(250)) - NOT NULL
- Status (VARCHAR(10)) - NOT NULL (values: "active" hoặc "deactive")
  - **Column Comment**: "Trạng thái: - active: hoạt động - deactive: không hoạt động"
- CreationTime (TIMESTAMP) - NOT NULL
- CreatorId (UUID) - NOT NULL
- LastModificationTime (TIMESTAMP) - NULL
- LastModifierId (UUID) - NULL

## 3. Chi tiết từng Layer

### 3.1 Domain Layer (`src/common/domain/iOne.Domain/HrDepartmentTypes/`)
**Files cần tạo:**

#### 3.1.1 Entity: `HrDepartmentType.cs`
- Kế thừa `FullAuditedAggregateRoot<Guid>` (tự động có audit fields, Guid map với PostgreSQL UUID)
- **Data Annotations** (ưu tiên):
  - `[Table("HrDepartmentType")]` - Table name (PascalCase)
  - `[Comment("Loại đơn vị: - CT: công ty - CN: chi nhánh - DV: đơn vị - PB: phòng ban")]` - Table comment
  - **Không cần `[Column]` annotation** vì column name trùng với property name (PascalCase)
  - `[MaxLength(50)]` trên Code
  - `[MaxLength(250)]` trên Name
  - `[Required]` trên Code, Name, Status
  - `[Index("IX_HrDepartmentType_Code", IsUnique = true)]` trên Code (unique index)
- Properties:
  - `Code` (string) - private setter, với annotations: `[MaxLength(50)]`, `[Required]`, `[Index(IsUnique = true)]`
  - `Name` (string) - private setter, với annotations: `[MaxLength(250)]`, `[Required]`
  - `Status` (HrDepartmentTypeStatus enum) - private setter, với annotations: `[Required]`, `[Comment("Trạng thái: - active: hoạt động - deactive: không hoạt động")]`
- Constructor:
  - `HrDepartmentType(Guid id, string code, string name, HrDepartmentTypeStatus status)`
  - Validate code format (A-Z, 0-9, _) trong constructor
- **Lưu ý**: 
  - Guid trong C# sẽ được EF Core map tự động sang UUID của PostgreSQL
  - Audit fields (CreationTime, CreatorId, LastModificationTime, LastModifierId) được ABP tự động map, không cần annotation
- Methods:
  - `UpdateName(string name)` - public method để update name
  - `UpdateStatus(HrDepartmentTypeStatus status)` - public method để update status
  - `ChangeCode(string code)` - private method (chỉ dùng khi create)

#### 3.1.2 Repository Interface: `IHrDepartmentTypeRepository.cs`
- Kế thừa `IRepository<HrDepartmentType, Guid>`
- Methods:
  - `Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)`
  - `Task<HrDepartmentType?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)`

#### 3.1.3 Domain Service: `HrDepartmentTypeManager.cs`
- Kế thừa `DomainService`
- Methods:
  - `Task CreateAsync(HrDepartmentType departmentType)` - Validate code unique trước khi create
  - `Task UpdateAsync(HrDepartmentType departmentType, string name, HrDepartmentTypeStatus status)` - Validate và update

### 3.2 Domain.Shared Layer (`src/common/domain/iOne.Domain.Shared/HrDepartmentTypes/`)
**Files cần tạo:**

#### 3.2.1 Enum: `HrDepartmentTypeStatus.cs`
```csharp
public enum HrDepartmentTypeStatus
{
    Active,
    Deactive
}
```
- **Lưu ý**: Enum values sẽ được convert sang string "active" và "deactive" khi lưu vào database
- **Converter**: Cần configure enum -> string conversion trong Entity Configuration (vì annotation không hỗ trợ converter)
  - Active -> "active"
  - Deactive -> "deactive"

#### 3.2.2 Permissions: `HrDepartmentTypePermissions.cs`
- `HrDepartmentTypePermissions` class với các permissions:
  - `Default` - Base permission
  - `Create` - Tạo mới
  - `Edit` - Sửa
  - `Delete` - Xóa
  - `View` - Xem

#### 3.2.3 Permission Definition Provider: `HrDepartmentTypePermissionDefinitionProvider.cs`
- Định nghĩa permissions trong `Define()` method

### 3.3 Application.Contracts Layer (`modules/hr/src/iOne.Hr.Application.Contracts/`)
**Files cần tạo:**

#### 3.3.1 DTOs:
- `HrDepartmentTypeDto.cs`
  - Properties: Id (Guid - map với PostgreSQL UUID), Code, Name, Status, CreationTime, CreatorId (Guid), LastModificationTime, LastModifierId (Guid)
  - Kế thừa `FullAuditedEntityDto<Guid>`

- `CreateHrDepartmentTypeDto.cs`
  - Properties: Code (required, max 50), Name (required, max 250), Status (required)
  - Data Annotations: `[Required]`, `[StringLength]`, `[RegularExpression]` cho Code

- `UpdateHrDepartmentTypeDto.cs`
  - Properties: Name (required, max 250), Status (required)
  - Data Annotations: `[Required]`, `[StringLength]`
  - **LƯU Ý: Không có Code** (theo yêu cầu chỉ cho phép sửa Name và Status)

#### 3.3.2 AppService Interface: `IHrDepartmentTypeAppService.cs`
- Kế thừa `ICrudAppService<HrDepartmentTypeDto, Guid, PagedAndSortedResultRequestDto, CreateHrDepartmentTypeDto, UpdateHrDepartmentTypeDto>`
- Methods:
  - `Task<HrDepartmentTypeDto> CreateAsync(CreateHrDepartmentTypeDto input)` - [Authorize(HrDepartmentTypePermissions.Create)]
  - `Task<HrDepartmentTypeDto> UpdateAsync(Guid id, UpdateHrDepartmentTypeDto input)` - [Authorize(HrDepartmentTypePermissions.Edit)]
  - `Task DeleteAsync(Guid id)` - [Authorize(HrDepartmentTypePermissions.Delete)]
  - `Task<HrDepartmentTypeDto> GetAsync(Guid id)` - [Authorize(HrDepartmentTypePermissions.View)]
  - `Task<PagedResultDto<HrDepartmentTypeDto>> GetListAsync(PagedAndSortedResultRequestDto input)` - [Authorize(HrDepartmentTypePermissions.View)]

### 3.4 Application Layer (`modules/hr/src/iOne.Hr.Application/`)
**Files cần tạo:**

#### 3.4.1 AppService: `HrDepartmentTypeAppService.cs`
- Kế thừa `CrudAppService<HrDepartmentType, HrDepartmentTypeDto, Guid, PagedAndSortedResultRequestDto, CreateHrDepartmentTypeDto, UpdateHrDepartmentTypeDto>`
- Implement `IHrDepartmentTypeAppService`
- Inject: `IHrDepartmentTypeRepository`, `HrDepartmentTypeManager`
- Override methods:
  - `CreateAsync`: 
    - Validate code format (A-Z, 0-9, _)
    - Check code unique qua Manager
    - Map DTO -> Entity
    - Call Manager.CreateAsync
    - Map Entity -> DTO return
  - `UpdateAsync`:
    - Get entity by id
    - Validate chỉ cho phép update Name và Status
    - Call Manager.UpdateAsync
    - **ABP tự động ghi audit log** (vì entity kế thừa FullAuditedAggregateRoot)
    - Map Entity -> DTO return
  - `DeleteAsync`: Standard delete
  - `GetAsync`: Standard get
  - `GetListAsync`: Standard list với filter, sort, paging

#### 3.4.2 AutoMapper Profile: Update `iOneHrApplicationAutoMapperProfile.cs`
- Mapping: Entity <-> DTO

### 3.5 EntityFrameworkCore Layer (`src/common/infra/iOne.EntityFrameworkCore/HrDepartmentTypes/`)
**Files cần tạo:**

#### 3.5.1 Entity Configuration: `HrDepartmentTypeConfiguration.cs` (CẦN THIẾT CHO ENUM CONVERSION)
- **Ưu tiên**: Sử dụng Data Annotations trên Entity class (xem 3.1.1)
- **Cần tạo file này vì**: Enum -> String conversion không thể dùng annotation được
- Configure:
  - Table name: `HrDepartmentType` (đã có trong `[Table]` annotation, nhưng cần để apply converter)
  - Table comment: `"Loại đơn vị: - CT: công ty - CN: chi nhánh - DV: đơn vị - PB: phòng ban"` (đã có trong `[Comment]` annotation trên class)
  - Column names: Tự động map theo property name (PascalCase: Code, Name, Status)
  - Unique index: Đã có trong `[Index]` annotation
  - **Status enum -> string conversion** (BẮT BUỘC):
    ```csharp
    builder.Property(e => e.Status)
        .HasConversion<string>()
        .HasMaxLength(10);
    ```
    - Convert: `Active` -> `"active"`, `Deactive` -> `"deactive"`
    - Column comment: `"Trạng thái: - active: hoạt động - deactive: không hoạt động"` (đã có trong `[Comment]` annotation trên property)
- **Lưu ý**: 
  - Audit fields (CreationTime, CreatorId, LastModificationTime, LastModifierId) được ABP tự động configure qua `ConfigureByConvention()`
  - Column names sẽ là PascalCase: `Code`, `Name`, `Status`, `CreationTime`, `CreatorId`, `LastModificationTime`, `LastModifierId`
  - Comments được định nghĩa bằng Data Annotations `[Comment]` trên class và property

#### 3.5.2 Repository Implementation: `EfCoreHrDepartmentTypeRepository.cs`
- Kế thừa `EfCoreRepository<iOneDbContext, HrDepartmentType, Guid>`
- Implement `IHrDepartmentTypeRepository`
- Implement `IsCodeExistsAsync`: Query DbSet với Where clause
- Implement `FindByCodeAsync`: Query DbSet với FirstOrDefaultAsync

#### 3.5.3 DbContext Configuration:
- Update `iOneDbContext.cs`:
  - Add `DbSet<HrDepartmentType> HrDepartmentTypes { get; set; }`
  - Trong `OnModelCreating`: `builder.ApplyConfiguration(new HrDepartmentTypeConfiguration());`
  - **Lý do**: Cần apply configuration để convert enum sang string ("active"/"deactive")

### 3.6 HttpApi Layer (`modules/hr/src/iOne.Hr.HttpApi/`)
**Files cần tạo:**

#### 3.6.1 Controller: `HrDepartmentTypeController.cs`
- Kế thừa `iOneHrController` (hoặc `AbpControllerBase`)
- Inject `IHrDepartmentTypeAppService`
- Routes: `/api/hr/department-types`
- Methods:
  - `GET /api/hr/department-types` - GetListAsync
  - `GET /api/hr/department-types/{id}` - GetAsync
  - `POST /api/hr/department-types` - CreateAsync
  - `PUT /api/hr/department-types/{id}` - UpdateAsync
  - `DELETE /api/hr/department-types/{id}` - DeleteAsync

## 4. Validation Rules

### 4.1 Code Validation
- **Format**: Chỉ cho phép A-Z (uppercase), 0-9, và dấu gạch dưới (_)
- **Regex**: `^[A-Z0-9_]+$`
- **Unique**: Code không được trùng nhau trong database
- **Required**: Bắt buộc nhập
- **Max Length**: 50 ký tự

### 4.2 Name Validation
- **Required**: Bắt buộc nhập
- **Max Length**: 250 ký tự

### 4.3 Status Validation
- **Required**: Bắt buộc chọn
- **Enum**: Chỉ cho phép Active hoặc Deactive

### 4.4 Update Restrictions
- **Code**: KHÔNG được phép sửa (chỉ set khi create)
- **Name**: Được phép sửa
- **Status**: Được phép sửa

## 5. Audit Logging
- Sử dụng `FullAuditedAggregateRoot<Guid>` để tự động ghi log:
  - CreationTime, CreatorId (khi create)
  - LastModificationTime, LastModifierId (khi update)
- ABP Framework tự động tích hợp với AuditLogging module

## 6. Permissions
- `HrDepartmentTypePermissions.Default` - Base permission
- `HrDepartmentTypePermissions.Create` - Tạo mới
- `HrDepartmentTypePermissions.Edit` - Sửa
- `HrDepartmentTypePermissions.Delete` - Xóa
- `HrDepartmentTypePermissions.View` - Xem danh sách và chi tiết

## 7. Migration
- Tạo EF Core Migration sau khi hoàn thành Entity Configuration
- Migration sẽ tạo bảng `HrDepartmentType` với đúng cấu trúc
- Status column sẽ lưu dưới dạng VARCHAR(10) với values "active" hoặc "deactive"
- Table và column comments sẽ được tạo trong migration:
  - Table comment: "Loại đơn vị: - CT: công ty - CN: chi nhánh - DV: đơn vị - PB: phòng ban"
  - Status column comment: "Trạng thái: - active: hoạt động - deactive: không hoạt động"

## 8. Testing Checklist
- [ ] Entity validation (code format)
- [ ] Code uniqueness check
- [ ] Create operation
- [ ] Update operation (chỉ Name và Status)
- [ ] Update không cho phép sửa Code
- [ ] Delete operation
- [ ] Get list với paging, sorting, filtering
- [ ] Permissions check
- [ ] Audit log được ghi đúng

## 9. File Structure Summary
```
src/common/
├── domain/
│   ├── iOne.Domain/
│   │   └── HrDepartmentTypes/ (mới)
│   │       ├── HrDepartmentType.cs
│   │       ├── IHrDepartmentTypeRepository.cs
│   │       └── HrDepartmentTypeManager.cs
│   └── iOne.Domain.Shared/
│       └── HrDepartmentTypes/ (mới)
│           ├── HrDepartmentTypeStatus.cs
│           └── HrDepartmentTypePermissions.cs
│       └── Permissions/ (cập nhật)
│           └── HrDepartmentTypePermissionDefinitionProvider.cs
└── infra/
    └── iOne.EntityFrameworkCore/
        └── HrDepartmentTypes/ (mới)
            ├── HrDepartmentTypeConfiguration.cs (cần thiết cho enum -> string conversion)
            └── EfCoreHrDepartmentTypeRepository.cs

modules/hr/src/
├── iOne.Hr.Application.Contracts/
│   └── HrDepartmentTypes/
│       ├── HrDepartmentTypeDto.cs
│       ├── CreateHrDepartmentTypeDto.cs
│       ├── UpdateHrDepartmentTypeDto.cs
│       └── IHrDepartmentTypeAppService.cs
├── iOne.Hr.Application/
│   └── HrDepartmentTypes/
│       └── HrDepartmentTypeAppService.cs
└── iOne.Hr.HttpApi/
    └── Controllers/
        └── HrDepartmentTypeController.cs
```

## 10. Notes
- **Domain và Domain.Shared**: Dùng chung trong `src/common/domain/` (không tạo riêng cho HR module)
- **EntityFrameworkCore**: Dùng chung trong `src/common/infra/` (không tạo riêng cho HR module)
- **Application layers**: Riêng biệt trong `modules/hr/src/` (Application, Application.Contracts, HttpApi)
- **UUID**: Tất cả các trường ID sử dụng PostgreSQL UUID type, trong C# dùng `Guid` (EF Core tự động map)

## 11. Cấu trúc cuối cùng (Đã xác nhận)
- **Domain**: `src/common/domain/iOne.Domain/HrDepartmentTypes/`
- **Domain.Shared**: `src/common/domain/iOne.Domain.Shared/HrDepartmentTypes/`
- **Application.Contracts**: `modules/hr/src/iOne.Hr.Application.Contracts/HrDepartmentTypes/`
- **Application**: `modules/hr/src/iOne.Hr.Application/HrDepartmentTypes/`
- **EntityFrameworkCore**: `src/common/infra/iOne.EntityFrameworkCore/HrDepartmentTypes/`
- **HttpApi**: `modules/hr/src/iOne.Hr.HttpApi/Controllers/HrDepartmentTypeController.cs`

## 12. UUID Mapping
- **C#**: Sử dụng `Guid` type
- **PostgreSQL**: Sử dụng `UUID` type
- **EF Core**: Tự động map `Guid` <-> `UUID` khi dùng Npgsql.EntityFrameworkCore.PostgreSQL
- **Tất cả ID fields**: Id, CreatorId, LastModifierId đều là UUID trong database

## 13. Data Annotations vs Fluent API
- **Ưu tiên**: Sử dụng Data Annotations trên Entity class
  - `[Table("HrDepartmentType")]` - Table name (PascalCase)
  - `[Comment("...")]` - Table comment (trên class) và column comment (trên property)
  - **Không cần `[Column]`** - Column names tự động map theo property name (PascalCase)
  - `[MaxLength(n)]` - Max length
  - `[Required]` - NOT NULL constraint
  - `[Index("IndexName", IsUnique = true)]` - Unique index
- **Dùng Fluent API Configuration cho**:
  - **Enum -> String conversion** (bắt buộc): `HasConversion<string>()` để convert enum sang "active"/"deactive"
  - Các trường hợp khác không thể dùng annotation (ví dụ: composite index, computed columns, complex relationships)
- **Lợi ích của Data Annotations**:
  - Code gọn gàng, dễ đọc
  - Cấu hình ngay trên Entity class
  - Giảm số lượng file cần maintain
- **Lưu ý**: Status enum cần converter vì database lưu dưới dạng string "active"/"deactive" chứ không phải numeric

