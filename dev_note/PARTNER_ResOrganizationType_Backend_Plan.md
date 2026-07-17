# Plan: Backend Danh mục Loại tổ chức (Partner ResOrganizationType)

## 1. Tổng quan
Tạo module backend cho quản lý Danh mục Loại tổ chức trong module Partner với các tính năng CRUD đầy đủ, validation nghiêm ngặt và audit logging.

**Entity Name**: `ResOrganizationType`  
**Module**: `Partner`  
**Table Name**: `res_organization_type` (snake_case - PostgreSQL convention)

## 2. Yêu cầu nghiệp vụ

### 2.1. Chức năng
- ✅ Xem danh sách (có phân trang, sắp xếp, filter)
- ✅ Xem chi tiết
- ✅ Thêm mới
- ✅ Sửa (chỉ được sửa Name và Status, **KHÔNG được sửa Code**)
- ✅ Xóa (soft delete: cập nhật status về Deactive)

### 2.2. Validation Rules
- **Code**: 
  - Chỉ cho phép ký tự: `A-Z`, `_`, `0-9` (không phân biệt hoa thường, nhưng lưu uppercase)
  - Độ dài tối đa: 25 ký tự
  - **Bắt buộc** (Required)
  - **Duy nhất** (Unique)
  - **Không được sửa** sau khi tạo (disable trên UI khi edit)
- **Name**: 
  - Độ dài tối đa: 250 ký tự
  - **Bắt buộc** (Required)
- **Status**: 
  - Enum: `Active`, `Deactive`
  - **Bắt buộc** (Required)
  - Mặc định: `Active`

### 2.3. Xóa (Soft Delete)
- **QUAN TRỌNG**: Khi xóa, phải đảm bảo audit log của ABP:
  1. Gọi `Repository.DeleteAsync(entity)` trước → trigger audit log với `ChangeType = Deleted`
  2. Gọi `CurrentUnitOfWork.SaveChangesAsync()` để lưu audit log
  3. Sau đó update `Status = Deactive`
  4. Gọi `Repository.UpdateAsync(entity)` và `SaveChangesAsync()` lần 2
- Entity không bị xóa khỏi database (soft delete), chỉ đánh dấu `IsDeleted = true` và `Status = Deactive`

## 3. Cấu trúc Database

### 3.1. Table Schema
**Table Name**: `res_organization_type` (snake_case - PostgreSQL convention)

| Column Name (snake_case) | Type | Constraints | Description |
|-------------------------|------|-------------|-------------|
| `id` | UUID | PRIMARY KEY | ID của loại tổ chức |
| `code` | VARCHAR(25) | NOT NULL, UNIQUE | Mã loại tổ chức (A-Z, _, 0-9) |
| `name` | VARCHAR(250) | NOT NULL | Tên loại tổ chức |
| `status` | VARCHAR(10) | NOT NULL | Trạng thái: "active" hoặc "deactive" |
| `creation_time` | TIMESTAMP | NOT NULL | Thời gian tạo |
| `creator_id` | UUID | NULL | ID người tạo |
| `last_modification_time` | TIMESTAMP | NULL | Thời gian sửa cuối |
| `last_modifier_id` | UUID | NULL | ID người sửa cuối |
| `is_deleted` | BOOLEAN | NOT NULL, DEFAULT false | Đánh dấu xóa (soft delete) |
| `deletion_time` | TIMESTAMP | NULL | Thời gian xóa |
| `deleter_id` | UUID | NULL | ID người xóa |
| `concurrency_stamp` | VARCHAR(40) | NULL | Concurrency stamp |
| `tenant_id` | UUID | NULL | Tenant ID (multi-tenancy) |

### 3.2. Table Comment
```sql
COMMENT ON TABLE res_organization_type IS 'Loại tổ chức, định nghĩa các loại như: cá nhân, doanh nghiệp, tổ chức khác ...';
```

### 3.3. Column Comment
```sql
COMMENT ON COLUMN res_organization_type.status IS 'Trạng thái:
- active: Hoạt động
- deactive: Không hoạt động';
```

### 3.4. Indexes
- **Primary Key**: `pk_res_organization_type` (on `id`)
- **Unique Index**: `ix_res_organization_type_code` (on `code`, unique)

## 4. Chi tiết từng Layer

### 4.1. Domain Layer (`src/common/domain/iOne.Domain/ResOrganizationTypes/`)

#### 4.1.1. Entity: `ResOrganizationType.cs`
**Location**: `src/common/domain/iOne.Domain/ResOrganizationTypes/ResOrganizationType.cs`

**Đặc điểm**:
- Kế thừa `FullAuditedAggregateRoot<Guid>` (tự động có audit fields)
- Sử dụng `Guid` cho Id (map với PostgreSQL UUID)
- Table name: `[Table("res_organization_type")]` (snake_case)
- Properties có `private setter` để đảm bảo encapsulation
- Validation trong constructor và setter methods

**Properties**:
```csharp
[Required]
[MaxLength(25)]
public virtual string Code { get; private set; } = null!;

[Required]
[MaxLength(250)]
public virtual string Name { get; private set; } = null!;

[Required]
public virtual ResOrganizationTypeStatus Status { get; private set; }
```

**Constructor**:
```csharp
public ResOrganizationType(Guid id, string code, string name, ResOrganizationTypeStatus status)
    : base(id)
{
    SetCode(code);
    SetName(name);
    SetStatus(status);
}
```

**Methods**:
- `private void SetCode(string code)` - Validate code format (A-Z, _, 0-9), length (max 25), không null/empty
- `private void SetName(string name)` - Validate name length (max 250), không null/empty
- `private void SetStatus(ResOrganizationTypeStatus status)` - Set status
- `public virtual void UpdateName(string name)` - Update name (dùng khi edit)
- `public virtual void UpdateStatus(ResOrganizationTypeStatus status)` - Update status (dùng khi edit hoặc delete)

**Validation Code**:
- Regex: `^[A-Z0-9_]+$` (chỉ A-Z, 0-9, _)
- Convert input về uppercase trước khi validate và lưu

#### 4.1.2. Repository Interface: `IResOrganizationTypeRepository.cs`
**Location**: `src/common/domain/iOne.Domain/ResOrganizationTypes/IResOrganizationTypeRepository.cs`

**Đặc điểm**:
- Kế thừa `IRepository<ResOrganizationType, Guid>`

**Methods** (nếu cần custom):
- `Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)` - Check code unique (exclude current entity khi update)

#### 4.1.3. Domain Service: `ResOrganizationTypeManager.cs` (Optional)
**Location**: `src/common/domain/iOne.Domain/ResOrganizationTypes/ResOrganizationTypeManager.cs`

**Đặc điểm**:
- Kế thừa `DomainService`
- Chứa business logic validation

**Methods**:
- `Task CreateAsync(ResOrganizationType organizationType)` - Validate code unique trước khi create
- `Task UpdateAsync(ResOrganizationType organizationType, string name, ResOrganizationTypeStatus status)` - Validate và update (không update Code)

### 4.2. Domain.Shared Layer (`src/common/domain/iOne.Domain.Shared/ResOrganizationTypes/`)

#### 4.2.1. Enum: `ResOrganizationTypeStatus.cs`
**Location**: `src/common/domain/iOne.Domain.Shared/ResOrganizationTypes/ResOrganizationTypeStatus.cs`

```csharp
namespace iOne.ResOrganizationTypes;

public enum ResOrganizationTypeStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

**Lưu ý**: Enum values sẽ được convert sang string "active" và "deactive" khi lưu vào database (thông qua EF Core configuration).

### 4.3. Application Layer (`modules/partner/src/iOne.Partner.Application/ResOrganizationTypes/`)

#### 4.3.1. DTOs
**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResOrganizationTypes/`

##### 4.3.1.1. `ResOrganizationTypeDto.cs`
```csharp
public class ResOrganizationTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResOrganizationType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResOrganizationType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResOrganizationType:Status")]
    public ResOrganizationTypeStatus Status { get; set; }
}
```

##### 4.3.1.2. `CreateResOrganizationTypeDto.cs`
```csharp
public class CreateResOrganizationTypeDto
{
    [Required]
    [MaxLength(25)]
    [Display(Name = "ResOrganizationType:Code")]
    public string Code { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    [Display(Name = "ResOrganizationType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResOrganizationType:Status")]
    public ResOrganizationTypeStatus Status { get; set; } = ResOrganizationTypeStatus.Active;
}
```

##### 4.3.1.3. `UpdateResOrganizationTypeDto.cs`
```csharp
public class UpdateResOrganizationTypeDto
{
    // ⚠️ QUAN TRỌNG: KHÔNG có Code field - không được sửa Code

    [Required]
    [MaxLength(250)]
    [Display(Name = "ResOrganizationType:Name")]
    public string Name { get; set; } = null!;

    [Required]
    [Display(Name = "ResOrganizationType:Status")]
    public ResOrganizationTypeStatus Status { get; set; }
}
```

##### 4.3.1.4. `GetResOrganizationTypesInput.cs`
```csharp
public class GetResOrganizationTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResOrganizationTypeStatus? Status { get; set; }
}
```

#### 4.3.2. Application Service Interface: `IResOrganizationTypeAppService.cs`
**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResOrganizationTypes/IResOrganizationTypeAppService.cs`

```csharp
public interface IResOrganizationTypeAppService : ICrudAppService<
    ResOrganizationTypeDto,
    Guid,
    GetResOrganizationTypesInput,
    CreateResOrganizationTypeDto,
    UpdateResOrganizationTypeDto>
{
}
```

**Lưu ý**: Kế thừa từ `ICrudAppService` để có sẵn các methods: `GetAsync`, `GetListAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`.

#### 4.3.3. Application Service Implementation: `ResOrganizationTypeAppService.cs`
**Location**: `modules/partner/src/iOne.Partner.Application/ResOrganizationTypes/ResOrganizationTypeAppService.cs`

**Đặc điểm**:
- Kế thừa `CrudAppService<ResOrganizationType, ResOrganizationTypeDto, Guid, GetResOrganizationTypesInput, CreateResOrganizationTypeDto, UpdateResOrganizationTypeDto>`
- Inject `IResOrganizationTypeRepository` và `ResOrganizationTypeManager` (nếu có)

**Methods**:

##### `CreateAsync(CreateResOrganizationTypeDto input)`
```csharp
[Authorize(ResOrganizationTypePermissions.Create)]
public override async Task<ResOrganizationTypeDto> CreateAsync(CreateResOrganizationTypeDto input)
{
    // 1. Validate Code unique
    if (await Repository.AnyAsync(x => x.Code == input.Code.ToUpperInvariant()))
    {
        throw new UserFriendlyException(
            _localizer["ResOrganizationType:CodeExists", new { Code = input.Code }]
        );
    }

    // 2. Create entity (convert Code to uppercase)
    var entity = new ResOrganizationType(
        GuidGenerator.Create(),
        input.Code.ToUpperInvariant(),
        input.Name,
        input.Status
    );

    // 3. Insert và return DTO
    await Repository.InsertAsync(entity);
    return ObjectMapper.Map<ResOrganizationType, ResOrganizationTypeDto>(entity);
}
```

##### `UpdateAsync(Guid id, UpdateResOrganizationTypeDto input)`
```csharp
[Authorize(ResOrganizationTypePermissions.Edit)]
public override async Task<ResOrganizationTypeDto> UpdateAsync(Guid id, UpdateResOrganizationTypeDto input)
{
    // 1. Load entity
    var entity = await Repository.GetAsync(id);

    // 2. ⚠️ QUAN TRỌNG: Chỉ update Name và Status, KHÔNG update Code
    entity.UpdateName(input.Name);
    entity.UpdateStatus(input.Status);

    // 3. Update và return DTO
    await Repository.UpdateAsync(entity);
    await CurrentUnitOfWork.SaveChangesAsync();

    return ObjectMapper.Map<ResOrganizationType, ResOrganizationTypeDto>(entity);
}
```

##### `DeleteAsync(Guid id)`
```csharp
[Authorize(ResOrganizationTypePermissions.Delete)]
public override async Task DeleteAsync(Guid id)
{
    // 1. Load entity
    var entity = await Repository.GetAsync(id);

    // 2. ⚠️ QUAN TRỌNG: Delete trước để trigger audit log với ChangeType = Deleted
    await Repository.DeleteAsync(entity);
    await CurrentUnitOfWork.SaveChangesAsync();

    // 3. Update status to Deactive (entity still exists due to soft delete)
    entity.UpdateStatus(ResOrganizationTypeStatus.Deactive);
    await Repository.UpdateAsync(entity);
    await CurrentUnitOfWork.SaveChangesAsync();
}
```

##### `CreateFilteredQueryAsync(GetResOrganizationTypesInput input)`
```csharp
protected override async Task<IQueryable<ResOrganizationType>> CreateFilteredQueryAsync(GetResOrganizationTypesInput input)
{
    var query = await ReadOnlyRepository.GetQueryableAsync();

    // Filter by Code (case-insensitive, contains)
    if (!string.IsNullOrWhiteSpace(input.Code))
    {
        query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
    }

    // Filter by Name (case-insensitive, contains)
    if (!string.IsNullOrWhiteSpace(input.Name))
    {
        query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.Name}%"));
    }

    // Filter by Status
    if (input.Status.HasValue)
    {
        query = query.Where(x => x.Status == input.Status.Value);
    }

    return query;
}
```

#### 4.3.4. AutoMapper Configuration
**Location**: `modules/partner/src/iOne.Partner.Application/iOnePartnerApplicationAutoMapperProfile.cs`

**QUAN TRỌNG**: Phải thêm mapping configuration:

```csharp
// ResOrganizationType mappings
CreateMap<ResOrganizationType, ResOrganizationTypeDto>();
CreateMap<CreateResOrganizationTypeDto, ResOrganizationType>(); // Có thể không dùng nếu dùng constructor
CreateMap<UpdateResOrganizationTypeDto, ResOrganizationType>(); // Có thể không dùng nếu dùng methods
```

### 4.4. Permissions (`modules/partner/src/iOne.Partner.Application.Contracts/Permissions/`)

#### 4.4.1. Permission Constants: `ResOrganizationTypePermissions.cs`
**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Permissions/ResOrganizationTypePermissions.cs`

```csharp
namespace iOne.Partner.Permissions;

public static class ResOrganizationTypePermissions
{
    public const string GroupName = "PartnerResOrganizationType";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

**Lưu ý**: 
- `GroupName` = `"PartnerResOrganizationType"` (Module + Entity)
- `Default` = `GroupName` (không phải `"{GroupName}.Default"`)

#### 4.4.2. Permission Definition Provider: `ResOrganizationTypePermissionDefinitionProvider.cs`
**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Permissions/ResOrganizationTypePermissionDefinitionProvider.cs`

```csharp
using iOne.Partner.Localization;
using iOne.Partner.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Partner.Permissions;

public class ResOrganizationTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resOrganizationTypeGroup = context.AddGroup(
            ResOrganizationTypePermissions.GroupName,
            L("Permission:ResOrganizationType")
        );

        var resOrganizationTypePermission = resOrganizationTypeGroup.AddPermission(
            ResOrganizationTypePermissions.Default,
            L("Permission:ResOrganizationType")
        );

        resOrganizationTypePermission.AddChild(
            ResOrganizationTypePermissions.Create,
            L("Permission:Create")
        );

        resOrganizationTypePermission.AddChild(
            ResOrganizationTypePermissions.Edit,
            L("Permission:Edit")
        );

        resOrganizationTypePermission.AddChild(
            ResOrganizationTypePermissions.Delete,
            L("Permission:Delete")
        );

        resOrganizationTypePermission.AddChild(
            ResOrganizationTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PartnerResource>(name);
    }
}
```

**Lưu ý**: Phải đăng ký provider này trong `iOnePartnerApplicationContractsModule.ConfigureServices()` (nếu chưa có).

### 4.5. Entity Framework Core (`src/common/infra/iOne.EntityFrameworkCore/ResOrganizationTypes/`)

#### 4.5.1. Entity Configuration: `ResOrganizationTypeConfiguration.cs`
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResOrganizationTypes/ResOrganizationTypeConfiguration.cs`

**QUAN TRỌNG**: Phải sử dụng **snake_case** cho table name và column names (PostgreSQL convention).

```csharp
public class ResOrganizationTypeConfiguration : IEntityTypeConfiguration<ResOrganizationType>
{
    public void Configure(EntityTypeBuilder<ResOrganizationType> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_organization_type", t =>
        {
            t.HasComment("Loại tổ chức, định nghĩa các loại như: cá nhân, doanh nghiệp, tổ chức khác ...");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (BẮT BUỘC override)
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");

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
        builder.HasIndex(e => e.Code, "ix_res_organization_type_code")
            .IsUnique();
    }
}
```

**Lưu ý**: 
- Enum `ResOrganizationTypeStatus` sẽ được convert sang string "active"/"deactive" tự động bởi EF Core (có thể cần converter nếu cần).

#### 4.5.2. Repository Implementation: `EfCoreResOrganizationTypeRepository.cs`
**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResOrganizationTypes/EfCoreResOrganizationTypeRepository.cs`

```csharp
public class EfCoreResOrganizationTypeRepository : EfCoreRepository<iOneDbContext, ResOrganizationType, Guid>,
    IResOrganizationTypeRepository
{
    public EfCoreResOrganizationTypeRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
```

**Lưu ý**: Nếu không có custom methods trong interface, có thể không cần implement riêng (ABP sẽ tự động tạo).

#### 4.5.3. Register Configuration trong DbContext
**Location**: `src/common/infra/iOne.EntityFrameworkCore/iOneDbContext.cs`

Thêm vào `OnModelCreating`:
```csharp
builder.ApplyConfiguration(new ResOrganizationTypeConfiguration());
```

### 4.6. HTTP API Controllers (`modules/partner/src/iOne.Partner.HttpApi/Controllers/`)

#### 4.6.1. Controller: `ResOrganizationTypeController.cs`
**Location**: `modules/partner/src/iOne.Partner.HttpApi/Controllers/ResOrganizationTypeController.cs`

```csharp
using iOne.Partner;
using iOne.Partner.Permissions;
using iOne.Partner.ResOrganizationTypes;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Partner.Controllers;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Area(PartnerRemoteServiceConsts.ModuleName)]
[Route("api/partner/res-organization-types")]
[Authorize]
public class ResOrganizationTypeController : AbpControllerBase
{
    protected IResOrganizationTypeAppService AppService { get; }

    public ResOrganizationTypeController(IResOrganizationTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResOrganizationTypePermissions.View)]
    public virtual Task<PagedResultDto<ResOrganizationTypeDto>> GetListAsync(GetResOrganizationTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResOrganizationTypePermissions.View)]
    public virtual Task<ResOrganizationTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResOrganizationTypePermissions.Create)]
    public virtual Task<ResOrganizationTypeDto> CreateAsync(CreateResOrganizationTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResOrganizationTypePermissions.Edit)]
    public virtual Task<ResOrganizationTypeDto> UpdateAsync(Guid id, UpdateResOrganizationTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResOrganizationTypePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
```

#### 4.6.2. Exclude AppService khỏi Conventional Controllers
**Location**: `modules/partner/src/iOne.Partner.HttpApi/iOnePartnerHttpApiModule.cs`

**QUAN TRỌNG**: Nếu có manual controller, phải exclude AppService khỏi conventional controller generation:

```csharp
Configure<AbpAspNetCoreMvcOptions>(options =>
{
    options.ConventionalControllers.Create(
        typeof(iOnePartnerApplicationModule).Assembly,
        opts =>
        {
            opts.TypePredicate = type =>
                type.Name != "ResOrganizationTypeAppService"; // Exclude AppService có manual controller
        });
});
```

**Lưu ý**: Phải thêm `ProjectReference` đến `iOne.Partner.Application.csproj` trong `iOne.Partner.HttpApi.csproj`.

### 4.7. Localization (`modules/partner/src/iOne.Partner.Application.Contracts/Localization/Partner/`)

#### 4.7.1. Vietnamese (`vi-VN.json`)
```json
{
  "Culture": "vi-VN",
  "Texts": {
    "Menu:Partner": "Đối tác",
    "Menu:ResOrganizationType": "Loại tổ chức",
    
    "Permission:ResOrganizationType": "Loại tổ chức",
    "Permission:Create": "Tạo mới",
    "Permission:Edit": "Sửa",
    "Permission:Delete": "Xóa",
    "Permission:View": "Xem",
    
    "ResOrganizationType:Code": "Mã loại tổ chức",
    "ResOrganizationType:Name": "Tên loại tổ chức",
    "ResOrganizationType:Status": "Trạng thái",
    
    "ResOrganizationType:CodeRequired": "Mã loại tổ chức là bắt buộc",
    "ResOrganizationType:CodeMaxLength": "Mã loại tổ chức không được vượt quá {0} ký tự",
    "ResOrganizationType:CodeInvalidFormat": "Mã loại tổ chức chỉ được chứa các ký tự A-Z, 0-9 và dấu gạch dưới (_)",
    "ResOrganizationType:CodeExists": "Mã loại tổ chức '{0}' đã tồn tại",
    
    "ResOrganizationType:NameRequired": "Tên loại tổ chức là bắt buộc",
    "ResOrganizationType:NameMaxLength": "Tên loại tổ chức không được vượt quá {0} ký tự",
    
    "ResOrganizationType:StatusRequired": "Trạng thái là bắt buộc",
    
    "ResOrganizationType:CreatedSuccessfully": "Tạo loại tổ chức thành công",
    "ResOrganizationType:UpdatedSuccessfully": "Cập nhật loại tổ chức thành công",
    "ResOrganizationType:DeletedSuccessfully": "Xóa loại tổ chức thành công"
  }
}
```

#### 4.7.2. English (`en.json`)
```json
{
  "Culture": "en",
  "Texts": {
    "Menu:Partner": "Partner",
    "Menu:ResOrganizationType": "Organization Type",
    
    "Permission:ResOrganizationType": "Organization Type",
    "Permission:Create": "Create",
    "Permission:Edit": "Edit",
    "Permission:Delete": "Delete",
    "Permission:View": "View",
    
    "ResOrganizationType:Code": "Organization Type Code",
    "ResOrganizationType:Name": "Organization Type Name",
    "ResOrganizationType:Status": "Status",
    
    "ResOrganizationType:CodeRequired": "Organization type code is required",
    "ResOrganizationType:CodeMaxLength": "Organization type code cannot exceed {0} characters",
    "ResOrganizationType:CodeInvalidFormat": "Organization type code can only contain A-Z, 0-9 and underscore (_)",
    "ResOrganizationType:CodeExists": "Organization type code '{0}' already exists",
    
    "ResOrganizationType:NameRequired": "Organization type name is required",
    "ResOrganizationType:NameMaxLength": "Organization type name cannot exceed {0} characters",
    
    "ResOrganizationType:StatusRequired": "Status is required",
    
    "ResOrganizationType:CreatedSuccessfully": "Organization type created successfully",
    "ResOrganizationType:UpdatedSuccessfully": "Organization type updated successfully",
    "ResOrganizationType:DeletedSuccessfully": "Organization type deleted successfully"
  }
}
```

### 4.8. Menu Configuration (`modules/partner/src/iOne.Partner.Application/Navigation/`)

#### 4.8.1. Update Menu Contributor
**Location**: `modules/partner/src/iOne.Partner.Application/Navigation/PartnerMenuContributor.cs`

Thêm menu item cho ResOrganizationType:

```csharp
resOrganizationTypeMenuItem.AddItem(new ApplicationMenuItem(
    "Partner.ResOrganizationType",
    partnerL["Menu:ResOrganizationType"],
    url: "~/pages/partner/res-organization-types",
    icon: "pi pi-fw pi-building"
).RequirePermissions(ResOrganizationTypePermissions.Default));
```

**Lưu ý**: 
- Menu contributor đã có sẵn trong module, chỉ cần thêm menu item mới.
- Phải import `iOne.Partner.Permissions` namespace.

### 4.9. Database Migration

#### 4.9.1. Migration Script
**Location**: `src/common/infra/iOne.EntityFrameworkCore/Migrations/YYYYMMDDHHMMSS_AddResOrganizationType.cs`

**QUAN TRỌNG**: 
- Sử dụng `IF EXISTS` khi DROP
- Sử dụng `IF NOT EXISTS` khi CREATE
- Tất cả names phải theo **snake_case** (PostgreSQL convention)

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "res_organization_type",
        columns: table => new
        {
            id = table.Column<Guid>(type: "uuid", nullable: false),
            code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
            name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
            status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
            creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
            creator_id = table.Column<Guid>(type: "uuid", nullable: true),
            last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
            last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
            is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
            deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
            deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
            concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
            tenant_id = table.Column<Guid>(type: "uuid", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("pk_res_organization_type", x => x.id);
        },
        comment: "Loại tổ chức, định nghĩa các loại như: cá nhân, doanh nghiệp, tổ chức khác ...");

    migrationBuilder.CreateIndex(
        name: "ix_res_organization_type_code",
        table: "res_organization_type",
        column: "code",
        unique: true);

    migrationBuilder.Sql(@"
        COMMENT ON COLUMN res_organization_type.status IS 'Trạng thái:
- active: Hoạt động
- deactive: Không hoạt động';
    ");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_organization_type_code"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_organization_type"";");
}
```

## 5. Checklist Trước Khi Hoàn Thành

### 5.1. Domain Layer
- [ ] Entity `ResOrganizationType` đã tạo với đầy đủ validation
  - [ ] Table name sử dụng `[Table("res_organization_type")]` (snake_case)
  - [ ] Code validation: A-Z, _, 0-9, max 25 ký tự
  - [ ] Name validation: max 250 ký tự
  - [ ] Status enum: Active, Deactive
  - [ ] Private setters và public update methods
- [ ] Enum `ResOrganizationTypeStatus` đã tạo
- [ ] Repository interface `IResOrganizationTypeRepository` đã tạo (nếu cần custom methods)
- [ ] Domain Service `ResOrganizationTypeManager` đã tạo (nếu cần business logic)

### 5.2. Application Layer
- [ ] DTOs đã tạo đầy đủ:
  - [ ] `ResOrganizationTypeDto`
  - [ ] `CreateResOrganizationTypeDto`
  - [ ] `UpdateResOrganizationTypeDto` (KHÔNG có Code field)
  - [ ] `GetResOrganizationTypesInput`
- [ ] Application Service Interface `IResOrganizationTypeAppService` đã tạo
- [ ] Application Service Implementation `ResOrganizationTypeAppService` đã tạo:
  - [ ] `CreateAsync`: Validate code unique
  - [ ] `UpdateAsync`: Chỉ update Name và Status, KHÔNG update Code
  - [ ] `DeleteAsync`: Soft delete (DeleteAsync trước, sau đó update Status = Deactive)
  - [ ] `CreateFilteredQueryAsync`: Filter theo Code, Name, Status
- [ ] **AutoMapper configuration đã được thêm vào AutoMapper Profile**
  - [ ] Mapping `ResOrganizationType` -> `ResOrganizationTypeDto`
  - [ ] Mapping `CreateResOrganizationTypeDto` -> `ResOrganizationType` (nếu dùng)
  - [ ] Mapping `UpdateResOrganizationTypeDto` -> `ResOrganizationType` (nếu dùng)

### 5.3. Permissions
- [ ] Permission Constants `ResOrganizationTypePermissions` đã tạo
- [ ] Permission Definition Provider `ResOrganizationTypePermissionDefinitionProvider` đã tạo
- [ ] Permission Provider đã được đăng ký trong Application Contracts Module
- [ ] Permissions đã được sử dụng trong AppService và Controller

### 5.4. Entity Framework Core
- [ ] Entity Configuration `ResOrganizationTypeConfiguration` đã tạo:
  - [ ] Table name: `"res_organization_type"` (snake_case)
  - [ ] Tất cả column names: snake_case (code, name, status, creation_time, etc.)
  - [ ] Index: `"ix_res_organization_type_code"` (unique)
  - [ ] Primary key: `"pk_res_organization_type"`
- [ ] Repository Implementation `EfCoreResOrganizationTypeRepository` đã tạo (nếu cần)
- [ ] Configuration đã được register trong DbContext

### 5.5. HTTP API
- [ ] Controller `ResOrganizationTypeController` đã tạo với đầy đủ endpoints
- [ ] Controller có `[Authorize]` attributes với permissions tương ứng
- [ ] **Conventional Controllers đã được config để exclude AppService (nếu có manual controller)**
- [ ] Route: `"api/partner/res-organization-types"`

### 5.6. Localization
- [ ] Localization keys đã đầy đủ trong `vi-VN.json`:
  - [ ] Menu keys
  - [ ] Permission keys
  - [ ] Field labels
  - [ ] Validation messages
  - [ ] Success messages
- [ ] Localization keys đã đầy đủ trong `en.json` (tương tự vi-VN)
- [ ] Keys phải match nhau giữa 2 file

### 5.7. Menu Configuration
- [ ] Menu item đã được thêm vào `PartnerMenuContributor`
- [ ] Menu item có permission check
- [ ] Menu item có icon và URL đúng

### 5.8. Database Migration
- [ ] Migration script đã được tạo:
  - [ ] Table name: `"res_organization_type"` (snake_case)
  - [ ] Column names: snake_case
  - [ ] Index names: snake_case với prefix
  - [ ] Sử dụng `IF EXISTS`/`IF NOT EXISTS`
  - [ ] Table và column comments đã được thêm
- [ ] Migration đã được test trên database mới
- [ ] Migration đã được test trên database đã có dữ liệu

### 5.9. Testing
- [ ] Build solution thành công
- [ ] Test API endpoints thành công:
  - [ ] GET `/api/partner/res-organization-types` (list)
  - [ ] GET `/api/partner/res-organization-types/{id}` (detail)
  - [ ] POST `/api/partner/res-organization-types` (create)
  - [ ] PUT `/api/partner/res-organization-types/{id}` (update - không được sửa Code)
  - [ ] DELETE `/api/partner/res-organization-types/{id}` (soft delete)
- [ ] Test validation:
  - [ ] Code format: chỉ A-Z, _, 0-9
  - [ ] Code unique
  - [ ] Code không được sửa khi update
  - [ ] Name required, max length
  - [ ] Status required
- [ ] Test soft delete:
  - [ ] Entity không bị xóa khỏi database
  - [ ] `IsDeleted = true` sau khi delete
  - [ ] `Status = Deactive` sau khi delete
  - [ ] Audit log có `ChangeType = Deleted`
- [ ] Test permissions:
  - [ ] User không có permission không thể access
  - [ ] User có permission có thể access
- [ ] Test localization:
  - [ ] Menu hiển thị đúng ngôn ngữ
  - [ ] Error messages hiển thị đúng ngôn ngữ
- [ ] Test menu:
  - [ ] Menu item hiển thị đúng
  - [ ] Menu item có permission check

## 6. Lưu Ý Quan Trọng

### 6.1. Database Naming Convention
- **BẮT BUỘC**: Tất cả database objects (tables, columns, indexes) phải theo **snake_case** (PostgreSQL convention)
- Table name: `"res_organization_type"` (không phải `"RESORGANIZATIONTYPE"` hay `"ResOrganizationType"`)
- Column names: `"code"`, `"name"`, `"status"`, `"creation_time"`, etc. (không phải `"CODE"`, `"Code"`, etc.)
- Index names: `"ix_res_organization_type_code"` (không phải `"IX_ResOrganizationType_Code"`)

### 6.2. Code Validation
- Code chỉ cho phép: `A-Z`, `_`, `0-9`
- Code phải được convert về uppercase trước khi lưu
- Code là unique
- Code không được sửa sau khi tạo (disable trên UI khi edit)

### 6.3. Soft Delete
- **QUAN TRỌNG**: Khi xóa, phải:
  1. Gọi `Repository.DeleteAsync(entity)` trước → trigger audit log với `ChangeType = Deleted`
  2. Gọi `CurrentUnitOfWork.SaveChangesAsync()` để lưu audit log
  3. Sau đó update `Status = Deactive`
  4. Gọi `Repository.UpdateAsync(entity)` và `SaveChangesAsync()` lần 2
- Entity không bị xóa khỏi database (soft delete), chỉ đánh dấu `IsDeleted = true`

### 6.4. Update Restriction
- **QUAN TRỌNG**: Khi update, chỉ được sửa `Name` và `Status`
- **KHÔNG được sửa `Code`** (không có Code field trong `UpdateResOrganizationTypeDto`)
- UI phải disable Code field khi edit

### 6.5. Permissions
- Permissions phải được đặt trong **module Partner**, không phải trong `common/domain`
- Location: `modules/partner/src/iOne.Partner.Application.Contracts/Permissions/`
- `GroupName` = `"PartnerResOrganizationType"` (Module + Entity)
- `Default` = `GroupName` (không phải `"{GroupName}.Default"`)

### 6.6. AutoMapper
- **BẮT BUỘC**: Phải thêm mapping configuration trong AutoMapper Profile
- Nếu thiếu mapping, sẽ gặp lỗi: `"Missing type map configuration or unsupported mapping"`

### 6.7. Migration Safety
- **LUÔN** sử dụng `IF EXISTS` khi DROP
- **LUÔN** sử dụng `IF NOT EXISTS` khi CREATE
- Đảm bảo migration có thể chạy trên database mới hoặc đã có dữ liệu

## 7. Thứ Tự Thực Hiện

1. **Domain Layer** (Entity, Enum, Repository Interface)
2. **Domain.Shared Layer** (Enum)
3. **EF Core Configuration** (Entity Configuration, Repository Implementation)
4. **Application Layer** (DTOs, AppService Interface, AppService Implementation)
5. **AutoMapper Configuration**
6. **Permissions** (Constants, Definition Provider)
7. **Localization** (vi-VN.json, en.json)
8. **HTTP API** (Controller, Exclude AppService)
9. **Menu Configuration** (Update Menu Contributor)
10. **Database Migration** (Create và test migration)
11. **Testing** (Build, test API, test validation, test permissions)

---

**Tài liệu này sẽ được cập nhật sau khi review và thực hiện.**

