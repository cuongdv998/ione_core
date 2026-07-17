# Plan: Backend Danh mục Cấu hình mã tự sinh (ResSequence)

## Tổng Quan

Tạo backend cho **Danh mục Cấu hình mã tự sinh** trong module **Master** với các tính năng:
- Xem danh sách (có filter/search)
- Thêm mới
- Sửa (không được sửa mã cấu hình)
- Xóa (soft delete với status = deactive, đảm bảo audit log)

## Yêu Cầu Nghiệp Vụ

1. **Mã cấu hình (Code)**:
   - Chỉ cho phép ký tự: A-Z, _, 0-9 (uppercase)
   - Mã là duy nhất (unique)
   - Không được phép sửa khi update (disable trên UI)

2. **Xóa (Delete)**:
   - Soft delete: Gọi `Repository.DeleteAsync()` trước để trigger ABP audit log
   - Sau đó cập nhật `Status = Deactive`
   - Đảm bảo có 2 audit log entries: 1 cho Delete, 1 cho Update Status

3. **Validation**:
   - Code: Required, MaxLength(25), Regex `^[A-Z0-9_]+$`
   - Name: Required, MaxLength(250)
   - Prefix: Optional, MaxLength(150)
   - Suffix: Optional, MaxLength(150)
   - Type: Required (enum: Normal, NoGap)
   - Padding: Optional, Range(0-99)
   - NumberNext: Required, Range(0-9999999999)
   - NumberIncrement: Required, Range(1-9)
   - UseDateRange: Required (enum: Yes, No), Default = No
   - DateRangeType: Optional (enum: Week, Month, Quarter, Half, Year) - chỉ có khi UseDateRange = Yes
   - Status: Required (enum: Active, Deactive)

## Cấu Trúc Database

### Table: `res_sequence` (snake_case - PostgreSQL convention)

**Columns** (snake_case):
- `id` (uuid, PK)
- `code` (varchar(25), NOT NULL, UNIQUE)
- `name` (varchar(250), NOT NULL)
- `prefix` (varchar(150), NULL)
- `suffix` (varchar(150), NULL)
- `type` (varchar(10), NOT NULL) - "normal" hoặc "no_gap"
- `padding` (numeric(2), NULL) - 0-99
- `number_next` (numeric(10), NOT NULL) - 0-9999999999
- `number_increment` (numeric(1), NOT NULL) - 1-9
- `use_date_range` (varchar(10), NOT NULL, default 'N') - "Y" hoặc "N"
- `date_range_type` (varchar(10), NULL) - "week", "month", "quater", "half", "year"
- `status` (varchar(10), NOT NULL) - "active" hoặc "deactive"
- Audit columns: `creation_time`, `creator_id`, `last_modification_time`, `last_modifier_id`, `is_deleted`, `deletion_time`, `deleter_id`, `concurrency_stamp`, `tenant_id`

**Indexes**:
- `ix_res_sequence_code` (unique) trên `code`
- `pk_res_sequence` (primary key) trên `id`

## Cấu Trúc Files Cần Tạo

### 1. Domain Layer

#### 1.1. Entity
**Location**: `src/common/domain/iOne.Domain/ResSequences/ResSequence.cs`
- Kế thừa `FullAuditedAggregateRoot<Guid>`
- Table name: `[Table("res_sequence")]` (snake_case)
- Properties với validation
- Code là `private set` và không có method `UpdateCode()`
- Methods: `SetCode()`, `SetName()`, `UpdateName()`, `UpdateStatus()`, etc.

#### 1.2. Enums
**Location**: `src/common/domain/iOne.Domain.Shared/ResSequences/`

- `ResSequenceType.cs`: Normal = 0, NoGap = 1
- `ResSequenceUseDateRange.cs`: No = 0, Yes = 1
- `ResSequenceDateRangeType.cs`: Week = 0, Month = 1, Quarter = 2, Half = 3, Year = 4
- `ResSequenceStatus.cs`: Active = 0, Deactive = 1

#### 1.3. Repository Interface
**Location**: `src/common/domain/iOne.Domain/ResSequences/IResSequenceRepository.cs`
- Kế thừa `IRepository<ResSequence, Guid>`
- Method: `IsCodeExistsAsync(string code)`

#### 1.4. Manager
**Location**: `src/common/domain/iOne.Domain/ResSequences/ResSequenceManager.cs`
- Kế thừa `DomainService`
- Methods:
  - `CreateAsync(ResSequence entity)`: Validate code uniqueness
  - `UpdateAsync(...)`: Update các fields (không có Code)
  - `DeleteAsync(ResSequence entity)`: Soft delete → Update status

### 2. EF Core Layer

#### 2.1. Entity Configuration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResSequences/ResSequenceConfiguration.cs`
- Table name: `"res_sequence"` (snake_case)
- Tất cả column names: snake_case
- Index: `ix_res_sequence_code` (unique)
- Enum conversions: string (lowercase)

#### 2.2. Repository Implementation
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResSequences/EfCoreResSequenceRepository.cs`
- Implement `IResSequenceRepository`
- Method `IsCodeExistsAsync()`

#### 2.3. Migration
**Location**: `src/common/infra/iOne.EntityFrameworkCore/Migrations/`
- Sử dụng `IF EXISTS`/`IF NOT EXISTS`
- Table và column names: snake_case
- Index names: snake_case với prefix

### 3. Application Layer

#### 3.1. DTOs
**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResSequences/`

- `CreateResSequenceDto.cs`:
  - Code (Required, MaxLength(25), Regex `^[A-Z0-9_]+$`)
  - Name (Required, MaxLength(250))
  - Prefix (Optional, MaxLength(150))
  - Suffix (Optional, MaxLength(150))
  - Type (Required)
  - Padding (Optional, Range(0-99))
  - NumberNext (Required, Range(0-9999999999))
  - NumberIncrement (Required, Range(1-9))
  - UseDateRange (Required, Default = No)
  - DateRangeType (Optional, chỉ validate khi UseDateRange = Yes)
  - Status (Required, Default = Active)

- `UpdateResSequenceDto.cs`:
  - **KHÔNG có Code** (Code không được phép sửa)
  - Các fields khác tương tự CreateDto

- `ResSequenceDto.cs`:
  - Kế thừa `FullAuditedEntityDto<Guid>`
  - Tất cả fields từ Entity

- `GetResSequencesInput.cs`:
  - Kế thừa `PagedAndSortedResultRequestDto`
  - Filter: Code, Name, Type, Status

#### 3.2. Application Service Interface
**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResSequences/IResSequenceAppService.cs`
- Kế thừa `ICrudAppService<ResSequenceDto, Guid, GetResSequencesInput, CreateResSequenceDto, UpdateResSequenceDto>`
- Methods có `[Authorize]` attribute

#### 3.3. Application Service Implementation
**Location**: `modules/master/src/iOne.Master.Application/ResSequences/ResSequenceAppService.cs`
- Kế thừa `CrudAppService<...>`
- Inject `IResSequenceRepository` và `ResSequenceManager`
- Override `CreateAsync()`, `UpdateAsync()`, `DeleteAsync()`, `CreateFilteredQueryAsync()`
- **DeleteAsync()**: Gọi `Repository.DeleteAsync()` trước → `UpdateStatus(Deactive)` sau

#### 3.4. AutoMapper Configuration
**Location**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`
- Thêm mappings:
  - `ResSequence` → `ResSequenceDto`
  - `CreateResSequenceDto` → `ResSequence`
  - `UpdateResSequenceDto` → `ResSequence`

### 4. Permissions

#### 4.1. Permission Constants
**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResSequencePermissions.cs`
- `GroupName = "MasterResSequence"`
- `Default = GroupName`
- `Create = Default + ".Create"`
- `Edit = Default + ".Edit"`
- `Delete = Default + ".Delete"`
- `View = Default + ".View"`

#### 4.2. Permission Definition Provider
**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResSequencePermissionDefinitionProvider.cs`
- Kế thừa `PermissionDefinitionProvider`
- Tạo permission group và child permissions
- Sử dụng localization từ `MasterResource`

### 5. Localization

#### 5.1. Localization Files
**Location**: `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/`

**vi-VN.json** và **en.json**:
- Menu keys: `"Menu:ResSequence"`
- Permission keys: `"Permission:ResSequence"`, `"Permission:Create"`, etc.
- Field labels: `"ResSequence:Code"`, `"ResSequence:Name"`, etc.
- Validation messages: `"ResSequence:CodeRequired"`, `"ResSequence:CodeInvalid"`, etc.
- Success messages: `"ResSequence:CreatedSuccessfully"`, etc.
- Enum values: `"ResSequence:Type:Normal"`, `"ResSequence:Type:NoGap"`, etc.

### 6. HTTP API

#### 6.1. Controller
**Location**: `modules/master/src/iOne.Master.HttpApi/Controllers/ResSequenceController.cs`
- Kế thừa `AbpControllerBase`
- Route: `"api/master/res-sequences"`
- `[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]`
- `[Area(MasterRemoteServiceConsts.ModuleName)]`
- Methods: `GetListAsync()`, `GetAsync()`, `CreateAsync()`, `UpdateAsync()`, `DeleteAsync()`
- Tất cả methods có `[Authorize]` với permission tương ứng

#### 6.2. Exclude từ Conventional Controllers
**Location**: `modules/master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`
- Nếu có manual controller, exclude `ResSequenceAppService` khỏi conventional controller generation

### 7. Menu Configuration

#### 7.1. Menu Contributor
**Location**: `modules/master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`
- Thêm menu item `"Master.ResSequence"` vào menu `"Master"`
- URL: `"~/pages/master/res-sequences"`
- Icon: `"pi pi-fw pi-cog"` (hoặc icon phù hợp)
- Permission: `ResSequencePermissions.Default`

#### 7.2. Đăng Ký Menu Contributor
**Location**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationModule.cs`
- Đảm bảo đã đăng ký `MasterMenuContributor` trong `ConfigureServices()`

## Chi Tiết Implementation

### 1. Entity: ResSequence

```csharp
[Table("res_sequence")]
public class ResSequence : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(150)]
    public virtual string? Prefix { get; private set; }

    [MaxLength(150)]
    public virtual string? Suffix { get; private set; }

    [Required]
    public virtual ResSequenceType Type { get; private set; }

    [Range(0, 99)]
    public virtual int? Padding { get; private set; }

    [Required]
    [Range(0, 9999999999)]
    public virtual long NumberNext { get; private set; }

    [Required]
    [Range(1, 9)]
    public virtual int NumberIncrement { get; private set; }

    [Required]
    public virtual ResSequenceUseDateRange UseDateRange { get; private set; } = ResSequenceUseDateRange.No;

    public virtual ResSequenceDateRangeType? DateRangeType { get; private set; }

    [Required]
    public virtual ResSequenceStatus Status { get; private set; }

    // Constructor và methods...
    // SetCode() với validation A-Z, 0-9, _
    // KHÔNG có UpdateCode()
}
```

### 2. Enums

```csharp
// ResSequenceType.cs
public enum ResSequenceType
{
    Normal = 0,    // Thông thường
    NoGap = 1      // Không có khoảng trống
}

// ResSequenceUseDateRange.cs
public enum ResSequenceUseDateRange
{
    No = 0,    // Không
    Yes = 1    // Có
}

// ResSequenceDateRangeType.cs
public enum ResSequenceDateRangeType
{
    Week = 0,      // Tuần
    Month = 1,     // Tháng
    Quarter = 2,   // Quý
    Half = 3,      // Nửa năm
    Year = 4      // Năm
}

// ResSequenceStatus.cs
public enum ResSequenceStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

### 3. Manager: ResSequenceManager

```csharp
public class ResSequenceManager : DomainService
{
    protected IResSequenceRepository Repository { get; }

    public virtual async Task CreateAsync(ResSequence entity)
    {
        // Validate code uniqueness
        if (await Repository.IsCodeExistsAsync(entity.Code))
        {
            throw new BusinessException("Master:ResSequence:CodeExists")
                .WithData("Code", entity.Code);
        }

        await Repository.InsertAsync(entity);
    }

    public virtual async Task UpdateAsync(
        ResSequence entity,
        string name,
        string? prefix,
        string? suffix,
        ResSequenceType type,
        int? padding,
        long numberNext,
        int numberIncrement,
        ResSequenceUseDateRange useDateRange,
        ResSequenceDateRangeType? dateRangeType,
        ResSequenceStatus status)
    {
        // ⚠️ QUAN TRỌNG: KHÔNG có UpdateCode - Code là immutable

        entity.UpdateName(name);
        entity.UpdatePrefix(prefix);
        entity.UpdateSuffix(suffix);
        entity.UpdateType(type);
        entity.UpdatePadding(padding);
        entity.UpdateNumberNext(numberNext);
        entity.UpdateNumberIncrement(numberIncrement);
        entity.UpdateUseDateRange(useDateRange);
        entity.UpdateDateRangeType(dateRangeType);
        entity.UpdateStatus(status);

        await Repository.UpdateAsync(entity);
    }

    public virtual async Task DeleteAsync(ResSequence entity)
    {
        // ⚠️ QUAN TRỌNG: Soft delete - xóa trước (ABP audit log) → cập nhật status về Deactive sau
        // 1. Delete để trigger ABP audit log (set IsDeleted = true, DeletionTime, DeleterId)
        await Repository.DeleteAsync(entity);
        
        // 2. Cập nhật status về Deactive
        entity.UpdateStatus(ResSequenceStatus.Deactive);
        await Repository.UpdateAsync(entity);
    }
}
```

### 4. AppService: ResSequenceAppService

```csharp
public override async Task DeleteAsync(Guid id)
{
    var entity = await Repository.GetAsync(id);
    
    // ⚠️ QUAN TRỌNG: Soft delete - xóa trước (ABP audit log) → cập nhật status về Deactive sau
    // 1. Delete để trigger ABP audit log (set IsDeleted = true, DeletionTime, DeleterId)
    await Manager.DeleteAsync(entity);
    await CurrentUnitOfWork.SaveChangesAsync();
}
```

### 5. EF Core Configuration

```csharp
public class ResSequenceConfiguration : IEntityTypeConfiguration<ResSequence>
{
    public void Configure(EntityTypeBuilder<ResSequence> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_sequence", t =>
        {
            t.HasComment("Bảng cấu hình các mã tự sinh của hệ thống");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (BẮT BUỘC override)
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Prefix).HasColumnName("prefix");
        builder.Property(x => x.Suffix).HasColumnName("suffix");
        builder.Property(x => x.Type).HasColumnName("type")
            .HasConversion<string>(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<ResSequenceType>(v, true));
        builder.Property(x => x.Padding).HasColumnName("padding");
        builder.Property(x => x.NumberNext).HasColumnName("number_next");
        builder.Property(x => x.NumberIncrement).HasColumnName("number_increment");
        builder.Property(x => x.UseDateRange).HasColumnName("use_date_range")
            .HasConversion<string>(v => v == ResSequenceUseDateRange.Yes ? "Y" : "N", v => v == "Y" ? ResSequenceUseDateRange.Yes : ResSequenceUseDateRange.No);
        builder.Property(x => x.DateRangeType).HasColumnName("date_range_type")
            .HasConversion<string>(v => v.HasValue ? v.Value.ToString().ToLowerInvariant() : null, v => string.IsNullOrEmpty(v) ? null : Enum.Parse<ResSequenceDateRangeType>(v, true));
        builder.Property(x => x.Status).HasColumnName("status")
            .HasConversion<string>(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<ResSequenceStatus>(v, true));
        
        // ✅ Audit columns: snake_case
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");

        // ✅ Index name: snake_case với prefix
        builder.HasIndex(e => e.Code, "ix_res_sequence_code")
            .IsUnique();
    }
}
```

## Checklist Trước Khi Hoàn Thành

### Domain Layer
- [ ] Entity `ResSequence` với validation đầy đủ
  - [ ] Table name: `[Table("res_sequence")]` (snake_case)
  - [ ] Code là `private set` và không có `UpdateCode()`
  - [ ] Validation Code: A-Z, 0-9, _ (uppercase)
- [ ] Enums: `ResSequenceType`, `ResSequenceUseDateRange`, `ResSequenceDateRangeType`, `ResSequenceStatus`
- [ ] Repository interface `IResSequenceRepository` với method `IsCodeExistsAsync()`
- [ ] Manager `ResSequenceManager` với `CreateAsync()`, `UpdateAsync()`, `DeleteAsync()`

### EF Core Layer
- [ ] Entity Configuration `ResSequenceConfiguration`
  - [ ] Table name: `"res_sequence"` (snake_case)
  - [ ] Tất cả column names: snake_case
  - [ ] Index: `ix_res_sequence_code` (unique)
  - [ ] Enum conversions: string (lowercase hoặc "Y"/"N" cho UseDateRange)
- [ ] Repository implementation `EfCoreResSequenceRepository`
- [ ] Migration đã được tạo và test
  - [ ] Sử dụng `IF EXISTS`/`IF NOT EXISTS`
  - [ ] Table và column names: snake_case
  - [ ] Index names: snake_case với prefix

### Application Layer
- [ ] DTOs: `CreateResSequenceDto`, `UpdateResSequenceDto` (không có Code), `ResSequenceDto`, `GetResSequencesInput`
- [ ] Application Service Interface `IResSequenceAppService`
- [ ] Application Service Implementation `ResSequenceAppService`
  - [ ] Override `CreateAsync()`, `UpdateAsync()`, `DeleteAsync()`, `CreateFilteredQueryAsync()`
  - [ ] `DeleteAsync()`: Gọi `Repository.DeleteAsync()` trước → `UpdateStatus(Deactive)` sau
- [ ] **AutoMapper configuration đã được thêm vào AutoMapper Profile**
  - [ ] Mapping `ResSequence` -> `ResSequenceDto`
  - [ ] Mapping `CreateResSequenceDto` -> `ResSequence`
  - [ ] Mapping `UpdateResSequenceDto` -> `ResSequence`

### Permissions
- [ ] Permission Constants `ResSequencePermissions`
- [ ] Permission Definition Provider `ResSequencePermissionDefinitionProvider`
- [ ] Permissions được sử dụng trong AppService và Controller

### Localization
- [ ] Localization keys đã đầy đủ (vi-VN và en)
  - [ ] Menu keys
  - [ ] Permission keys
  - [ ] Field labels
  - [ ] Validation messages
  - [ ] Success messages
  - [ ] Enum values

### HTTP API
- [ ] Controller `ResSequenceController` với đầy đủ endpoints
- [ ] Authorization với permissions tương ứng
- [ ] **Conventional Controllers đã được config để tránh duplicate API (nếu có manual controller)**

### Menu Configuration
- [ ] Menu Contributor đã được cập nhật với menu item `ResSequence`
- [ ] Menu Contributor đã được đăng ký trong Application Module
- [ ] Package reference `Volo.Abp.UI.Navigation` đã được thêm vào `.csproj`

### Testing
- [ ] Build solution thành công
- [ ] Test API endpoints thành công
- [ ] Test validation: Code chỉ A-Z, 0-9, _
- [ ] Test Code uniqueness
- [ ] Test Update: Code không được phép sửa
- [ ] Test Delete: Soft delete với status = deactive, có 2 audit log entries
- [ ] Test filter/search functionality
- [ ] Menu hiển thị đúng trên client

## Lưu Ý Quan Trọng

1. **Code Validation**: 
   - Code chỉ cho phép A-Z, 0-9, _ (uppercase)
   - Validation ở cả Entity (SetCode) và DTO (RegularExpression attribute)

2. **Code Immutability**: 
   - Code là `private set` trong Entity
   - Không có method `UpdateCode()` trong Entity
   - `UpdateResSequenceDto` không có Code property
   - Manager `UpdateAsync` không có parameter Code

3. **Soft Delete với Status Deactive**:
   - Khi Delete: Gọi `Repository.DeleteAsync()` để trigger audit log (soft delete)
   - Sau đó set `Status = Deactive` để đảm bảo business logic
   - Entity vẫn tồn tại trong database (IsDeleted = true)
   - Đảm bảo có 2 audit log entries: 1 cho Delete, 1 cho Update Status

4. **Table và Column Naming (PostgreSQL Style)**:
   - Table: `"res_sequence"` (snake_case)
   - Columns: `"code"`, `"name"`, `"prefix"`, `"suffix"`, `"type"`, `"padding"`, `"number_next"`, `"number_increment"`, `"use_date_range"`, `"date_range_type"`, `"status"` (snake_case)
   - Index: `"ix_res_sequence_code"` (snake_case với prefix)
   - Primary key: `"pk_res_sequence"` (snake_case với prefix)
   - **BẮT BUỘC**: Tất cả database objects phải sử dụng snake_case

5. **Enum Conversions**:
   - `ResSequenceType`: "normal", "no_gap" (lowercase)
   - `ResSequenceUseDateRange`: "Y", "N" (theo DLL)
   - `ResSequenceDateRangeType`: "week", "month", "quater", "half", "year" (lowercase, lưu ý "quater" không phải "quarter")
   - `ResSequenceStatus`: "active", "deactive" (lowercase)

6. **DateRangeType Validation**:
   - `DateRangeType` chỉ có giá trị khi `UseDateRange = Yes`
   - Validation trong DTO: Nếu `UseDateRange = Yes` thì `DateRangeType` là Required

7. **Audit Log**:
   - ABP Framework tự động tạo audit log thông qua `FullAuditedAggregateRoot<Guid>`
   - Khi Delete, audit log sẽ có ChangeType = Deleted
   - Khi Update Status, audit log sẽ có ChangeType = Updated

8. **Module Location**:
   - Entity và Domain layer: `src/common/domain/`
   - Application layer: `modules/master/src/`
   - Permissions: `modules/master/src/iOne.Master.Application.Contracts/Permissions/`

## Thứ Tự Thực Hiện

1. **Domain Layer** (Entity, Enums, Repository Interface, Manager)
2. **EF Core Layer** (Configuration, Repository Implementation, Migration)
3. **Application Layer** (DTOs, Application Service, AutoMapper)
4. **Permissions** (Constants, Definition Provider)
5. **Localization** (vi-VN và en)
6. **HTTP API** (Controller)
7. **Menu Configuration** (Menu Contributor)
8. **Testing** (Build, Test API, Test validation)

## Tài Liệu Tham Khảo

- `RULES_BACKEND_DEVELOPMENT.md`: Quy tắc chung khi tạo backend
- `ResBank`: Implementation mẫu trong module Master (tương tự về Code immutability và soft delete)
- `ResIndustry`: Implementation mẫu về soft delete với status deactive
- `HrEmployeeRole`: Implementation mẫu trong module HR

