# Plan: Backend - Danh mục Loại tài liệu (ResDocumentType)

## 1. Tổng Quan

**Module**: Master  
**Entity**: ResDocumentType  
**Table Name**: `res_document_type` (snake_case - PostgreSQL convention)  
**Mô tả**: Quản lý loại chứng từ

## 2. Database Schema

### 2.1. Table Structure

```sql
CREATE TABLE res_document_type (
    id uuid NOT NULL,
    code character varying(25) NOT NULL,
    name character varying(250) NOT NULL,
    description character varying(500) NULL,
    status character varying(10) NOT NULL,
    bucket character varying(50) NULL,
    extra_properties text NOT NULL,
    concurrency_stamp character varying(40) NOT NULL,
    creation_time timestamp without time zone NOT NULL,
    creator_id uuid NULL,
    last_modification_time timestamp without time zone NULL,
    last_modifier_id uuid NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleter_id uuid NULL,
    deletion_time timestamp without time zone NULL,
    CONSTRAINT pk_res_document_type PRIMARY KEY (id)
);

COMMENT ON TABLE res_document_type IS 'Loại chứng từ';

COMMENT ON COLUMN res_document_type.status IS 'Trạng thái:
- active: hoạt động
- deactive: không hoạt động';

COMMENT ON COLUMN res_document_type.bucket IS 'Thông tin bucket sẽ lưu tài liệu';

CREATE UNIQUE INDEX ix_res_document_type_code ON res_document_type (code);
```

### 2.2. Naming Conventions (PostgreSQL Style)

- **Table name**: `res_document_type` (snake_case)
- **Column names**: `id`, `code`, `name`, `description`, `status`, `bucket`, `creation_time`, `creator_id`, `last_modification_time`, `last_modifier_id`, `is_deleted`, `deleter_id`, `deletion_time`, `concurrency_stamp`, `extra_properties` (snake_case)
- **Primary key**: `pk_res_document_type` (snake_case với prefix `pk_`)
- **Index**: `ix_res_document_type_code` (snake_case với prefix `ix_`)

## 3. Domain Layer

### 3.1. Enum: ResDocumentTypeStatus

**Location**: `src/common/domain/iOne.Domain.Shared/ResDocumentTypes/ResDocumentTypeStatus.cs`

```csharp
namespace iOne.ResDocumentTypes;

public enum ResDocumentTypeStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

**Lưu ý**: Enum sẽ được convert sang string lowercase (`"active"`, `"deactive"`) khi lưu vào database.

### 3.2. Entity: ResDocumentType

**Location**: `src/common/domain/iOne.Domain/ResDocumentTypes/ResDocumentType.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>`
- Table name: `res_document_type` (snake_case)
- Properties:
  - `Id`: `Guid` (từ base class)
  - `Code`: `string`, MaxLength(25), Required, **Immutable** (chỉ set trong constructor)
  - `Name`: `string`, MaxLength(250), Required
  - `Description`: `string?`, MaxLength(500), Optional
  - `Status`: `ResDocumentTypeStatus`, Required
  - `Bucket`: `string?`, MaxLength(50), Optional
- Code validation: Chỉ cho phép A-Z, 0-9, _ (uppercase)
- Private setters cho tất cả properties
- Public methods: `UpdateName()`, `UpdateDescription()`, `UpdateStatus()`, `UpdateBucket()`
- **KHÔNG có** `UpdateCode()` method

**Code Structure**:
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResDocumentTypes;

[Table("res_document_type")]
public class ResDocumentType : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ResDocumentTypeStatus Status { get; private set; }

    [MaxLength(50)]
    public virtual string? Bucket { get; private set; }

    protected ResDocumentType()
    {
        // For ORM
    }

    public ResDocumentType(
        Guid id,
        string code,
        string name,
        ResDocumentTypeStatus status,
        string? description = null,
        string? bucket = null)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetDescription(description);
        SetBucket(bucket);
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }

        if (code.Length > 25)
        {
            throw new ArgumentException("Code cannot exceed 25 characters.", nameof(code));
        }

        // Convert to uppercase and validate format: only A-Z, 0-9, and underscore
        var upperCode = code.ToUpperInvariant();
        if (!Regex.IsMatch(upperCode, @"^[A-Z0-9_]+$"))
        {
            throw new ArgumentException("Code can only contain letters (A-Z), numbers (0-9), and underscore (_).", nameof(code));
        }

        Code = upperCode;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if (name.Length > 250)
        {
            throw new ArgumentException("Name cannot exceed 250 characters.", nameof(name));
        }

        Name = name;
    }

    private void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetStatus(ResDocumentTypeStatus status)
    {
        Status = status;
    }

    private void SetBucket(string? bucket)
    {
        if (!string.IsNullOrWhiteSpace(bucket) && bucket.Length > 50)
        {
            throw new ArgumentException("Bucket cannot exceed 50 characters.", nameof(bucket));
        }

        Bucket = bucket;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateStatus(ResDocumentTypeStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateBucket(string? bucket)
    {
        SetBucket(bucket);
    }
}
```

### 3.3. Repository Interface: IResDocumentTypeRepository

**Location**: `src/common/domain/iOne.Domain/ResDocumentTypes/IResDocumentTypeRepository.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResDocumentTypes;

public interface IResDocumentTypeRepository : IRepository<ResDocumentType, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<ResDocumentType?> FindByCodeAsync(string code);
}
```

### 3.4. Manager: ResDocumentTypeManager

**Location**: `src/common/domain/iOne.Domain/ResDocumentTypes/ResDocumentTypeManager.cs`

**Yêu cầu**:
- Business logic validation
- Check code uniqueness khi create
- Validate code format

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResDocumentTypes;

public class ResDocumentTypeManager : DomainService
{
    protected IResDocumentTypeRepository Repository { get; }

    public ResDocumentTypeManager(IResDocumentTypeRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResDocumentType documentType)
    {
        // Check code uniqueness
        if (await Repository.FindByCodeAsync(documentType.Code) != null)
        {
            throw new BusinessException("Master:ResDocumentType:CodeExists")
                .WithData("Code", documentType.Code);
        }

        await Repository.InsertAsync(documentType);
    }

    public virtual async Task UpdateAsync(
        ResDocumentType documentType,
        string name,
        ResDocumentTypeStatus status,
        string? description = null,
        string? bucket = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        documentType.UpdateName(name);
        documentType.UpdateStatus(status);
        documentType.UpdateDescription(description);
        documentType.UpdateBucket(bucket);
        await Repository.UpdateAsync(documentType);
    }
}
```

## 4. Application Layer

### 4.1. DTOs

#### 4.1.1. ResDocumentTypeDto

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResDocumentTypes/ResDocumentTypeDto.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResDocumentTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResDocumentTypes;

public class ResDocumentTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Master::ResDocumentType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Master::ResDocumentType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Master::ResDocumentType:Description")]
    public string? Description { get; set; }

    [Display(Name = "Master::ResDocumentType:Status")]
    public ResDocumentTypeStatus Status { get; set; }

    [Display(Name = "Master::ResDocumentType:Bucket")]
    public string? Bucket { get; set; }
}
```

#### 4.1.2. CreateResDocumentTypeDto

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResDocumentTypes/CreateResDocumentTypeDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.ResDocumentTypes;

namespace iOne.Master.ResDocumentTypes;

public class CreateResDocumentTypeDto
{
    [Required(ErrorMessage = "Master::ResDocumentType:CodeRequired")]
    [StringLength(25, ErrorMessage = "Master::ResDocumentType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Master::ResDocumentType:CodeInvalid")]
    [Display(Name = "Master::ResDocumentType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Master::ResDocumentType:NameRequired")]
    [StringLength(250, ErrorMessage = "Master::ResDocumentType:NameMaxLength")]
    [Display(Name = "Master::ResDocumentType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Master::ResDocumentType:DescriptionMaxLength")]
    [Display(Name = "Master::ResDocumentType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Master::ResDocumentType:StatusRequired")]
    [Display(Name = "Master::ResDocumentType:Status")]
    public ResDocumentTypeStatus Status { get; set; } = ResDocumentTypeStatus.Active;

    [StringLength(50, ErrorMessage = "Master::ResDocumentType:BucketMaxLength")]
    [Display(Name = "Master::ResDocumentType:Bucket")]
    public string? Bucket { get; set; }
}
```

#### 4.1.3. UpdateResDocumentTypeDto

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResDocumentTypes/UpdateResDocumentTypeDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.ResDocumentTypes;

namespace iOne.Master.ResDocumentTypes;

public class UpdateResDocumentTypeDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "Master::ResDocumentType:NameRequired")]
    [StringLength(250, ErrorMessage = "Master::ResDocumentType:NameMaxLength")]
    [Display(Name = "Master::ResDocumentType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Master::ResDocumentType:DescriptionMaxLength")]
    [Display(Name = "Master::ResDocumentType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Master::ResDocumentType:StatusRequired")]
    [Display(Name = "Master::ResDocumentType:Status")]
    public ResDocumentTypeStatus Status { get; set; }

    [StringLength(50, ErrorMessage = "Master::ResDocumentType:BucketMaxLength")]
    [Display(Name = "Master::ResDocumentType:Bucket")]
    public string? Bucket { get; set; }
}
```

#### 4.1.4. GetResDocumentTypesInput

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResDocumentTypes/GetResDocumentTypesInput.cs`

```csharp
using iOne.ResDocumentTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResDocumentTypes;

public class GetResDocumentTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResDocumentTypeStatus? Status { get; set; }
}
```

### 4.2. Application Service Interface

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResDocumentTypes/IResDocumentTypeAppService.cs`

```csharp
using System;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResDocumentTypes;

public interface IResDocumentTypeAppService : ICrudAppService<
    ResDocumentTypeDto,
    Guid,
    GetResDocumentTypesInput,
    CreateResDocumentTypeDto,
    UpdateResDocumentTypeDto>
{
}
```

### 4.3. Application Service Implementation

**Location**: `modules/master/src/iOne.Master.Application/ResDocumentTypes/ResDocumentTypeAppService.cs`

**Yêu cầu**:
- Override `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetAsync`, `GetListAsync`
- `DeleteAsync`: Delete trước → Set Status = Deactive sau (để trigger audit log)
- Filter theo Code, Name, Status
- Sử dụng Manager cho business logic

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResDocumentTypes;
using iOne.ResDocumentTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResDocumentTypes;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResDocumentTypePermissions.Default)]
public class ResDocumentTypeAppService : CrudAppService<
    ResDocumentType,
    ResDocumentTypeDto,
    Guid,
    GetResDocumentTypesInput,
    CreateResDocumentTypeDto,
    UpdateResDocumentTypeDto>,
    IResDocumentTypeAppService
{
    protected ResDocumentTypeManager Manager { get; }

    public ResDocumentTypeAppService(
        IResDocumentTypeRepository repository,
        ResDocumentTypeManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResDocumentTypePermissions.View;
        GetListPolicyName = ResDocumentTypePermissions.View;
        CreatePolicyName = ResDocumentTypePermissions.Create;
        UpdatePolicyName = ResDocumentTypePermissions.Edit;
        DeletePolicyName = ResDocumentTypePermissions.Delete;
    }

    public override async Task<ResDocumentTypeDto> CreateAsync(CreateResDocumentTypeDto input)
    {
        var entity = new ResDocumentType(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status,
            input.Description,
            input.Bucket
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResDocumentType, ResDocumentTypeDto>(entity);
    }

    public override async Task<ResDocumentTypeDto> UpdateAsync(Guid id, UpdateResDocumentTypeDto input)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Description, Status, Bucket - không update Code
        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Status,
            input.Description,
            input.Bucket
        );

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResDocumentType, ResDocumentTypeDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResDocumentTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResDocumentType>> CreateFilteredQueryAsync(GetResDocumentTypesInput input)
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
}
```

### 4.4. AutoMapper Configuration

**Location**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

**QUAN TRỌNG**: Phải thêm mapping configuration:

```csharp
// ResDocumentType mappings
CreateMap<ResDocumentType, ResDocumentTypeDto>();
CreateMap<CreateResDocumentTypeDto, ResDocumentType>();
CreateMap<UpdateResDocumentTypeDto, ResDocumentType>();
```

## 5. Permissions

### 5.1. Permission Constants

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResDocumentTypePermissions.cs`

```csharp
namespace iOne.Master.Permissions;

public static class ResDocumentTypePermissions
{
    public const string GroupName = "MasterResDocumentType";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

### 5.2. Permission Definition Provider

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResDocumentTypePermissionDefinitionProvider.cs`

```csharp
using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResDocumentTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resDocumentTypeGroup = context.AddGroup(
            ResDocumentTypePermissions.GroupName,
            L("Permission:ResDocumentType")
        );

        var resDocumentTypePermission = resDocumentTypeGroup.AddPermission(
            ResDocumentTypePermissions.Default,
            L("Permission:ResDocumentType")
        );

        resDocumentTypePermission.AddChild(
            ResDocumentTypePermissions.Create,
            L("Permission:Create")
        );

        resDocumentTypePermission.AddChild(
            ResDocumentTypePermissions.Edit,
            L("Permission:Edit")
        );

        resDocumentTypePermission.AddChild(
            ResDocumentTypePermissions.Delete,
            L("Permission:Delete")
        );

        resDocumentTypePermission.AddChild(
            ResDocumentTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
```

## 6. Entity Framework Core

### 6.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResDocumentTypes/ResDocumentTypeConfiguration.cs`

**Yêu cầu**:
- Table name: `res_document_type` (snake_case)
- Column names: snake_case
- Index: `ix_res_document_type_code` (unique)
- Status enum conversion: `Active`/`Deactive` → `"active"`/`"deactive"` (lowercase)

```csharp
using System;
using iOne.ResDocumentTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResDocumentTypes;

public class ResDocumentTypeConfiguration : IEntityTypeConfiguration<ResDocumentType>
{
    public void Configure(EntityTypeBuilder<ResDocumentType> builder)
    {
        builder.ToTable("res_document_type", t =>
        {
            t.HasComment("Loại chứng từ");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ResDocumentTypeStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive (case-insensitive)
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động");

        builder.Property(x => x.Bucket)
            .HasColumnName("bucket")
            .HasMaxLength(50)
            .HasComment("Thông tin bucket sẽ lưu tài liệu");

        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        builder.HasIndex(e => e.Code, "ix_res_document_type_code")
            .IsUnique();
    }
}
```

### 6.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResDocumentTypes/EfCoreResDocumentTypeRepository.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResDocumentTypes;

namespace iOne.EntityFrameworkCore.ResDocumentTypes;

public class EfCoreResDocumentTypeRepository : EfCoreRepository<iOneDbContext, ResDocumentType, Guid>, IResDocumentTypeRepository
{
    public EfCoreResDocumentTypeRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => x.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<ResDocumentType?> FindByCodeAsync(string code)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code);
    }
}
```

### 6.3. Register trong DbContext

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`

- Thêm `DbSet<ResDocumentType> ResDocumentTypes { get; set; }`
- Thêm `builder.ApplyConfiguration(new ResDocumentTypeConfiguration());`

### 6.4. Register trong Module

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneEntityFrameworkCoreModule.cs`

- Thêm `using iOne.ResDocumentTypes;`
- Thêm `using iOne.EntityFrameworkCore.ResDocumentTypes;`
- Thêm `options.AddRepository<ResDocumentType, EfCoreResDocumentTypeRepository>();`

## 7. HTTP API

### 7.1. Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/Controllers/ResDocumentTypeController.cs`

```csharp
using System;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.ResDocumentTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/document-types")]
[Authorize]
public class ResDocumentTypeController : AbpControllerBase
{
    protected IResDocumentTypeAppService AppService { get; }

    public ResDocumentTypeController(IResDocumentTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResDocumentTypePermissions.View)]
    public virtual Task<PagedResultDto<ResDocumentTypeDto>> GetListAsync(GetResDocumentTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResDocumentTypePermissions.View)]
    public virtual Task<ResDocumentTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResDocumentTypePermissions.Create)]
    public virtual Task<ResDocumentTypeDto> CreateAsync(CreateResDocumentTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResDocumentTypePermissions.Edit)]
    public virtual Task<ResDocumentTypeDto> UpdateAsync(Guid id, UpdateResDocumentTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResDocumentTypePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
```

### 7.2. Exclude khỏi Conventional Controllers

**Location**: `modules/master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`

Thêm `ResDocumentTypeAppService` vào exclude list:

```csharp
opts.TypePredicate = type =>
    type.Name != "ResCountryAppService" &&
    type.Name != "ResProvinceAppService" &&
    type.Name != "ResWardAppService" &&
    type.Name != "ResDocumentTypeAppService"; // Exclude ResDocumentTypeAppService
```

## 8. Localization

### 8.1. Vietnamese (vi-VN.json)

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/vi-VN.json`

```json
{
  "Menu:ResDocumentType": "Danh mục Loại tài liệu",
  "Permission:ResDocumentType": "Loại tài liệu",
  "ResDocumentType:Code": "Mã loại tài liệu",
  "ResDocumentType:Name": "Tên loại tài liệu",
  "ResDocumentType:Description": "Mô tả",
  "ResDocumentType:Status": "Trạng thái",
  "ResDocumentType:Bucket": "Bucket",
  "ResDocumentType:CodeRequired": "Mã loại tài liệu là bắt buộc",
  "ResDocumentType:CodeMaxLength": "Mã loại tài liệu không được vượt quá 25 ký tự",
  "ResDocumentType:CodeInvalid": "Mã loại tài liệu chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
  "ResDocumentType:NameRequired": "Tên loại tài liệu là bắt buộc",
  "ResDocumentType:NameMaxLength": "Tên loại tài liệu không được vượt quá 250 ký tự",
  "ResDocumentType:DescriptionMaxLength": "Mô tả không được vượt quá 500 ký tự",
  "ResDocumentType:StatusRequired": "Trạng thái là bắt buộc",
  "ResDocumentType:BucketMaxLength": "Bucket không được vượt quá 50 ký tự",
  "ResDocumentType:CodeExists": "Mã loại tài liệu '{Code}' đã tồn tại",
  "ResDocumentType:CreatedSuccessfully": "Tạo loại tài liệu thành công",
  "ResDocumentType:UpdatedSuccessfully": "Cập nhật loại tài liệu thành công",
  "ResDocumentType:DeletedSuccessfully": "Xóa loại tài liệu thành công",
  "ResDocumentType:New": "Thêm mới loại tài liệu",
  "ResDocumentType:Edit": "Sửa loại tài liệu",
  "ResDocumentType:Delete": "Xóa loại tài liệu",
  "ResDocumentType:DeleteConfirm": "Bạn có chắc chắn muốn xóa loại tài liệu này?",
  "ResDocumentType:CodeCannotBeChanged": "Mã loại tài liệu không được phép thay đổi",
  "ResDocumentType:Active": "Hoạt động",
  "ResDocumentType:Deactive": "Không hoạt động",
  "ResDocumentType:SearchByCode": "Tìm theo mã loại tài liệu",
  "ResDocumentType:SearchByName": "Tìm theo tên loại tài liệu"
}
```

### 8.2. English (en.json)

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/en.json`

```json
{
  "Menu:ResDocumentType": "Document Type Master",
  "Permission:ResDocumentType": "Document Type",
  "ResDocumentType:Code": "Document Type Code",
  "ResDocumentType:Name": "Document Type Name",
  "ResDocumentType:Description": "Description",
  "ResDocumentType:Status": "Status",
  "ResDocumentType:Bucket": "Bucket",
  "ResDocumentType:CodeRequired": "Document type code is required",
  "ResDocumentType:CodeMaxLength": "Document type code cannot exceed 25 characters",
  "ResDocumentType:CodeInvalid": "Document type code can only contain letters (A-Z), numbers (0-9) and underscore (_)",
  "ResDocumentType:NameRequired": "Document type name is required",
  "ResDocumentType:NameMaxLength": "Document type name cannot exceed 250 characters",
  "ResDocumentType:DescriptionMaxLength": "Description cannot exceed 500 characters",
  "ResDocumentType:StatusRequired": "Status is required",
  "ResDocumentType:BucketMaxLength": "Bucket cannot exceed 50 characters",
  "ResDocumentType:CodeExists": "Document type code '{Code}' already exists",
  "ResDocumentType:CreatedSuccessfully": "Document type created successfully",
  "ResDocumentType:UpdatedSuccessfully": "Document type updated successfully",
  "ResDocumentType:DeletedSuccessfully": "Document type deleted successfully",
  "ResDocumentType:New": "New Document Type",
  "ResDocumentType:Edit": "Edit Document Type",
  "ResDocumentType:Delete": "Delete Document Type",
  "ResDocumentType:DeleteConfirm": "Are you sure you want to delete this document type?",
  "ResDocumentType:CodeCannotBeChanged": "Document type code cannot be changed",
  "ResDocumentType:Active": "Active",
  "ResDocumentType:Deactive": "Deactive",
  "ResDocumentType:SearchByCode": "Search by document type code",
  "ResDocumentType:SearchByName": "Search by document type name"
}
```

## 9. Menu Configuration

**Location**: `modules/master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

Thêm menu item:

```csharp
// ResDocumentType Menu Item
masterMenuItem.AddItem(new ApplicationMenuItem(
    "Master.ResDocumentType",
    masterL["Menu:ResDocumentType"],
    url: "~/pages/master/document-types",
    icon: "pi pi-fw pi-file"
).RequirePermissions(ResDocumentTypePermissions.Default));
```

## 10. Migration

### 10.1. Tạo Migration

```bash
cd src/common/infra/iOne.EntityFrameworkCore
dotnet ef migrations add AddResDocumentType --startup-project ../../iOne.DbMigrator/iOne.DbMigrator.csproj --context iOneDbContext
```

### 10.2. Migration Script

**Location**: `src/common/infra/iOne.EntityFrameworkCore/Migrations/YYYYMMDDHHMMSS_AddResDocumentType.cs`

**QUAN TRỌNG**: Sử dụng `IF EXISTS`/`IF NOT EXISTS` và snake_case naming:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP (nếu table đã tồn tại)
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_document_type"";");

    migrationBuilder.CreateTable(
        name: "res_document_type",
        columns: table => new
        {
            id = table.Column<Guid>(type: "uuid", nullable: false),
            code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
            name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
            description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
            status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động"),
            bucket = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Thông tin bucket sẽ lưu tài liệu"),
            extra_properties = table.Column<string>(type: "text", nullable: false),
            concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
            creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
            creator_id = table.Column<Guid>(type: "uuid", nullable: true),
            last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
            last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true),
            is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
            deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
            deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
        },
        constraints: table =>
        {
            // ✅ ĐÚNG: Primary key name theo snake_case với prefix
            table.PrimaryKey("pk_res_document_type", x => x.id);
        },
        comment: "Loại chứng từ");

    // ✅ ĐÚNG: Index name theo snake_case với prefix
    migrationBuilder.CreateIndex(
        name: "ix_res_document_type_code",
        table: "res_document_type",
        column: "code",
        unique: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_document_type_code"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_document_type"";");
}
```

## 11. Checklist

- [ ] Domain Layer:
  - [ ] Enum `ResDocumentTypeStatus` đã tạo
  - [ ] Entity `ResDocumentType` với private setters và validation (Code, Name, Description, Status, Bucket)
  - [ ] Repository interface `IResDocumentTypeRepository` với `IsCodeExistsAsync`, `FindByCodeAsync`
  - [ ] Manager `ResDocumentTypeManager` với business logic
- [ ] Application Layer:
  - [ ] DTOs: `ResDocumentTypeDto`, `CreateResDocumentTypeDto`, `UpdateResDocumentTypeDto` (không có Code), `GetResDocumentTypesInput`
  - [ ] Interface `IResDocumentTypeAppService`
  - [ ] Implementation `ResDocumentTypeAppService` với override `DeleteAsync` (soft delete logic)
  - [ ] **AutoMapper configuration đã được thêm vào AutoMapper Profile**
- [ ] Permissions:
  - [ ] `ResDocumentTypePermissions` constants
  - [ ] `ResDocumentTypePermissionDefinitionProvider`
- [ ] EF Core:
  - [ ] `ResDocumentTypeConfiguration` với snake_case naming và enum conversion (lowercase)
  - [ ] `EfCoreResDocumentTypeRepository` implementation
  - [ ] Register trong `iOneDbContext` và `iOneEntityFrameworkCoreModule`
- [ ] HTTP API:
  - [ ] `ResDocumentTypeController` với CRUD endpoints
  - [ ] Exclude `ResDocumentTypeAppService` khỏi conventional controllers
- [ ] Localization:
  - [ ] Keys đã thêm vào `vi-VN.json` và `en.json`
- [ ] Menu:
  - [ ] Menu item đã thêm vào `MasterMenuContributor`
- [ ] Migration:
  - [ ] Migration đã tạo với snake_case naming và `IF EXISTS`/`IF NOT EXISTS`
  - [ ] Migration đã chạy thành công

## 12. Lưu Ý Quan Trọng

1. **Code Immutability**: Code chỉ được set trong constructor, không có `UpdateCode()` method
2. **Status Storage**: Enum `Active`/`Deactive` được lưu trong DB dưới dạng `"active"`/`"deactive"` (lowercase)
3. **Soft Delete**: Delete trước → Set Status = Deactive sau để trigger audit log
4. **PostgreSQL Naming**: Tất cả database objects (tables, columns, indexes) phải theo snake_case convention
5. **AutoMapper**: **BẮT BUỘC** phải thêm mapping configuration trong AutoMapper Profile
6. **Permissions**: GroupName = `"MasterResDocumentType"`, Default = GroupName
7. **Localization**: Format `Master::ResDocumentType:{Key}`
8. **Menu**: URL = `"~/pages/master/document-types"`
9. **Additional Fields**: 
   - `Description`: Optional, MaxLength 500
   - `Bucket`: Optional, MaxLength 50 (Thông tin bucket sẽ lưu tài liệu)

