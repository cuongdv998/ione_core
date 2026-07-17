# Plan: Backend - Quản lý cấu hình chung (AdminConfig)

## 1. Tổng Quan

**Module**: Master  
**Entity**: AdminConfig  
**Table Name**: `admin_config` (snake_case - PostgreSQL convention)  
**Mô tả**: Quản lý các cấu hình chung của hệ thống

## 2. Database Schema

### 2.1. Table Structure

```sql
CREATE TABLE admin_config (
    id uuid NOT NULL,
    code character varying(25) NOT NULL,
    name character varying(250) NOT NULL,
    sub_code character varying(25) NOT NULL,
    value character varying(250) NOT NULL,
    description character varying(250) NULL,
    status character varying(10) NOT NULL,
    extra_properties text NOT NULL,
    concurrency_stamp character varying(40) NOT NULL,
    creation_time timestamp without time zone NOT NULL,
    creator_id uuid NULL,
    last_modification_time timestamp without time zone NULL,
    last_modifier_id uuid NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleter_id uuid NULL,
    deletion_time timestamp without time zone NULL,
    CONSTRAINT pk_admin_config PRIMARY KEY (id)
);

COMMENT ON TABLE admin_config IS 'Bảng lưu trữ các cấu hình chung của hệ thống';

COMMENT ON COLUMN admin_config.sub_code IS 'Mã con (nếu một cấu hình lớn cần chia nhiều cấu hình con)';

COMMENT ON COLUMN admin_config.value IS 'Giá trị cấu hình';

COMMENT ON COLUMN admin_config.status IS 'Trạng thái:
- active: Hoạt động
- deactive: Không hoạt động';

-- ⚠️ QUAN TRỌNG: Composite unique index trên (code, sub_code)
CREATE UNIQUE INDEX ix_admin_config_code_sub_code ON admin_config (code, sub_code);
```

### 2.2. Naming Conventions (PostgreSQL Style)

- **Table name**: `admin_config` (snake_case)
- **Column names**: `id`, `code`, `name`, `sub_code`, `value`, `description`, `status`, `creation_time`, `creator_id`, `last_modification_time`, `last_modifier_id`, `is_deleted`, `deleter_id`, `deletion_time`, `concurrency_stamp`, `extra_properties` (snake_case)
- **Primary key**: `pk_admin_config` (snake_case với prefix `pk_`)
- **Composite unique index**: `ix_admin_config_code_sub_code` (snake_case với prefix `ix_`)

### 2.3. Composite Unique Key

**QUAN TRỌNG**: 
- `(Code, SubCode)` tạo thành khóa duy nhất
- Trong cùng 1 Code, các SubCode không được trùng nhau
- Cần validate uniqueness trong Manager khi create/update

## 3. Domain Layer

### 3.1. Enum: AdminConfigStatus

**Location**: `src/common/domain/iOne.Domain.Shared/AdminConfigs/AdminConfigStatus.cs`

```csharp
namespace iOne.AdminConfigs;

public enum AdminConfigStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

**Lưu ý**: Enum sẽ được convert sang string lowercase (`"active"`, `"deactive"`) khi lưu vào database.

### 3.2. Entity: AdminConfig

**Location**: `src/common/domain/iOne.Domain/AdminConfigs/AdminConfig.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>`
- Table name: `admin_config` (snake_case)
- Properties:
  - `Id`: `Guid` (từ base class)
  - `Code`: `string`, MaxLength(25), Required, **Immutable** (chỉ set trong constructor)
  - `Name`: `string`, MaxLength(250), Required
  - `SubCode`: `string`, MaxLength(25), Required, **Immutable** (chỉ set trong constructor)
  - `Value`: `string`, MaxLength(250), Required
  - `Description`: `string?`, MaxLength(250), Optional
  - `Status`: `AdminConfigStatus`, Required
- Code và SubCode validation: Chỉ cho phép A-Z, 0-9, _ (uppercase)
- Private setters cho tất cả properties
- Public methods: `UpdateName()`, `UpdateValue()`, `UpdateDescription()`, `UpdateStatus()`
- **KHÔNG có** `UpdateCode()` và `UpdateSubCode()` methods

**Code Structure**:
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.AdminConfigs;

[Table("admin_config")]
public class AdminConfig : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    [MaxLength(25)]
    public virtual string SubCode { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Value { get; private set; } = null!;

    [MaxLength(250)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual AdminConfigStatus Status { get; private set; }

    protected AdminConfig()
    {
        // For ORM
    }

    public AdminConfig(
        Guid id,
        string code,
        string name,
        string subCode,
        string value,
        AdminConfigStatus status,
        string? description = null)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetSubCode(subCode);
        SetValue(value);
        SetStatus(status);
        SetDescription(description);
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

    private void SetSubCode(string subCode)
    {
        if (string.IsNullOrWhiteSpace(subCode))
        {
            throw new ArgumentException("SubCode cannot be null or empty.", nameof(subCode));
        }

        if (subCode.Length > 25)
        {
            throw new ArgumentException("SubCode cannot exceed 25 characters.", nameof(subCode));
        }

        // Convert to uppercase and validate format: only A-Z, 0-9, and underscore
        var upperSubCode = subCode.ToUpperInvariant();
        if (!Regex.IsMatch(upperSubCode, @"^[A-Z0-9_]+$"))
        {
            throw new ArgumentException("SubCode can only contain letters (A-Z), numbers (0-9), and underscore (_).", nameof(subCode));
        }

        SubCode = upperSubCode;
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

    private void SetValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        }

        if (value.Length > 250)
        {
            throw new ArgumentException("Value cannot exceed 250 characters.", nameof(value));
        }

        Value = value;
    }

    private void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 250)
        {
            throw new ArgumentException("Description cannot exceed 250 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetStatus(AdminConfigStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() và UpdateSubCode() - Code và SubCode không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateValue(string value)
    {
        SetValue(value);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateStatus(AdminConfigStatus status)
    {
        SetStatus(status);
    }
}
```

### 3.3. Repository Interface: IAdminConfigRepository

**Location**: `src/common/domain/iOne.Domain/AdminConfigs/IAdminConfigRepository.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.AdminConfigs;

public interface IAdminConfigRepository : IRepository<AdminConfig, Guid>
{
    Task<bool> IsCodeSubCodeExistsAsync(string code, string subCode, Guid? excludeId = null);
    Task<AdminConfig?> FindByCodeSubCodeAsync(string code, string subCode);
}
```

### 3.4. Manager: AdminConfigManager

**Location**: `src/common/domain/iOne.Domain/AdminConfigs/AdminConfigManager.cs`

**Yêu cầu**:
- Business logic validation
- Check composite unique key (Code, SubCode) khi create
- Validate Code và SubCode format

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.AdminConfigs;

public class AdminConfigManager : DomainService
{
    protected IAdminConfigRepository Repository { get; }

    public AdminConfigManager(IAdminConfigRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(AdminConfig adminConfig)
    {
        // Check composite unique key (Code, SubCode)
        if (await Repository.FindByCodeSubCodeAsync(adminConfig.Code, adminConfig.SubCode) != null)
        {
            throw new BusinessException("Master:AdminConfig:CodeSubCodeExists")
                .WithData("Code", adminConfig.Code)
                .WithData("SubCode", adminConfig.SubCode);
        }

        await Repository.InsertAsync(adminConfig);
    }

    public virtual async Task UpdateAsync(
        AdminConfig adminConfig,
        string name,
        string value,
        AdminConfigStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code và subCode - Code và SubCode không được phép sửa

        adminConfig.UpdateName(name);
        adminConfig.UpdateValue(value);
        adminConfig.UpdateStatus(status);
        adminConfig.UpdateDescription(description);
        await Repository.UpdateAsync(adminConfig);
    }
}
```

## 4. Application Layer

### 4.1. DTOs

#### 4.1.1. AdminConfigDto

**Location**: `modules/master/src/iOne.Master.Application.Contracts/AdminConfigs/AdminConfigDto.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.AdminConfigs;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.AdminConfigs;

public class AdminConfigDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Master::AdminConfig:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Master::AdminConfig:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Master::AdminConfig:SubCode")]
    public string SubCode { get; set; } = null!;

    [Display(Name = "Master::AdminConfig:Value")]
    public string Value { get; set; } = null!;

    [Display(Name = "Master::AdminConfig:Description")]
    public string? Description { get; set; }

    [Display(Name = "Master::AdminConfig:Status")]
    public AdminConfigStatus Status { get; set; }
}
```

#### 4.1.2. CreateAdminConfigDto

**Location**: `modules/master/src/iOne.Master.Application.Contracts/AdminConfigs/CreateAdminConfigDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.AdminConfigs;

namespace iOne.Master.AdminConfigs;

public class CreateAdminConfigDto
{
    [Required(ErrorMessage = "Master::AdminConfig:CodeRequired")]
    [StringLength(25, ErrorMessage = "Master::AdminConfig:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Master::AdminConfig:CodeInvalid")]
    [Display(Name = "Master::AdminConfig:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Master::AdminConfig:NameRequired")]
    [StringLength(250, ErrorMessage = "Master::AdminConfig:NameMaxLength")]
    [Display(Name = "Master::AdminConfig:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Master::AdminConfig:SubCodeRequired")]
    [StringLength(25, ErrorMessage = "Master::AdminConfig:SubCodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Master::AdminConfig:SubCodeInvalid")]
    [Display(Name = "Master::AdminConfig:SubCode")]
    public string SubCode { get; set; } = null!;

    [Required(ErrorMessage = "Master::AdminConfig:ValueRequired")]
    [StringLength(250, ErrorMessage = "Master::AdminConfig:ValueMaxLength")]
    [Display(Name = "Master::AdminConfig:Value")]
    public string Value { get; set; } = null!;

    [StringLength(250, ErrorMessage = "Master::AdminConfig:DescriptionMaxLength")]
    [Display(Name = "Master::AdminConfig:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Master::AdminConfig:StatusRequired")]
    [Display(Name = "Master::AdminConfig:Status")]
    public AdminConfigStatus Status { get; set; } = AdminConfigStatus.Active;
}
```

#### 4.1.3. UpdateAdminConfigDto

**Location**: `modules/master/src/iOne.Master.Application.Contracts/AdminConfigs/UpdateAdminConfigDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.AdminConfigs;

namespace iOne.Master.AdminConfigs;

public class UpdateAdminConfigDto
{
    // ⚠️ QUAN TRỌNG: Không có Code và SubCode properties - Code và SubCode không được phép sửa

    [Required(ErrorMessage = "Master::AdminConfig:NameRequired")]
    [StringLength(250, ErrorMessage = "Master::AdminConfig:NameMaxLength")]
    [Display(Name = "Master::AdminConfig:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Master::AdminConfig:ValueRequired")]
    [StringLength(250, ErrorMessage = "Master::AdminConfig:ValueMaxLength")]
    [Display(Name = "Master::AdminConfig:Value")]
    public string Value { get; set; } = null!;

    [StringLength(250, ErrorMessage = "Master::AdminConfig:DescriptionMaxLength")]
    [Display(Name = "Master::AdminConfig:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Master::AdminConfig:StatusRequired")]
    [Display(Name = "Master::AdminConfig:Status")]
    public AdminConfigStatus Status { get; set; }
}
```

#### 4.1.4. GetAdminConfigsInput

**Location**: `modules/master/src/iOne.Master.Application.Contracts/AdminConfigs/GetAdminConfigsInput.cs`

```csharp
using iOne.AdminConfigs;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.AdminConfigs;

public class GetAdminConfigsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? SubCode { get; set; }
    public string? Name { get; set; }
    public AdminConfigStatus? Status { get; set; }
}
```

### 4.2. Application Service Interface

**Location**: `modules/master/src/iOne.Master.Application.Contracts/AdminConfigs/IAdminConfigAppService.cs`

```csharp
using System;
using Volo.Abp.Application.Services;

namespace iOne.Master.AdminConfigs;

public interface IAdminConfigAppService : ICrudAppService<
    AdminConfigDto,
    Guid,
    GetAdminConfigsInput,
    CreateAdminConfigDto,
    UpdateAdminConfigDto>
{
}
```

### 4.3. Application Service Implementation

**Location**: `modules/master/src/iOne.Master.Application/AdminConfigs/AdminConfigAppService.cs`

**Yêu cầu**:
- Override `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetAsync`, `GetListAsync`
- `DeleteAsync`: Delete trước → Set Status = Deactive sau (để trigger audit log)
- Filter theo Code, SubCode, Name, Status
- Sử dụng Manager cho business logic

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.AdminConfigs;
using iOne.AdminConfigs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.AdminConfigs;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(AdminConfigPermissions.Default)]
public class AdminConfigAppService : CrudAppService<
    AdminConfig,
    AdminConfigDto,
    Guid,
    GetAdminConfigsInput,
    CreateAdminConfigDto,
    UpdateAdminConfigDto>,
    IAdminConfigAppService
{
    protected AdminConfigManager Manager { get; }

    public AdminConfigAppService(
        IAdminConfigRepository repository,
        AdminConfigManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = AdminConfigPermissions.View;
        GetListPolicyName = AdminConfigPermissions.View;
        CreatePolicyName = AdminConfigPermissions.Create;
        UpdatePolicyName = AdminConfigPermissions.Edit;
        DeletePolicyName = AdminConfigPermissions.Delete;
    }

    public override async Task<AdminConfigDto> CreateAsync(CreateAdminConfigDto input)
    {
        var entity = new AdminConfig(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.SubCode,
            input.Value,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<AdminConfig, AdminConfigDto>(entity);
    }

    public override async Task<AdminConfigDto> UpdateAsync(Guid id, UpdateAdminConfigDto input)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Value, Description, Status - không update Code và SubCode
        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Value,
            input.Status,
            input.Description
        );

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<AdminConfig, AdminConfigDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(AdminConfigStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<AdminConfig>> CreateFilteredQueryAsync(GetAdminConfigsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by Code (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.Code}%"));
        }

        // Filter by SubCode (case-insensitive, contains)
        if (!string.IsNullOrWhiteSpace(input.SubCode))
        {
            query = query.Where(x => EF.Functions.ILike(x.SubCode, $"%{input.SubCode}%"));
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
// AdminConfig mappings
CreateMap<AdminConfig, AdminConfigDto>();
CreateMap<CreateAdminConfigDto, AdminConfig>();
CreateMap<UpdateAdminConfigDto, AdminConfig>();
```

## 5. Permissions

### 5.1. Permission Constants

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/AdminConfigPermissions.cs`

```csharp
namespace iOne.Master.Permissions;

public static class AdminConfigPermissions
{
    public const string GroupName = "MasterAdminConfig";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

### 5.2. Permission Definition Provider

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/AdminConfigPermissionDefinitionProvider.cs`

```csharp
using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class AdminConfigPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var adminConfigGroup = context.AddGroup(
            AdminConfigPermissions.GroupName,
            L("Permission:AdminConfig")
        );

        var adminConfigPermission = adminConfigGroup.AddPermission(
            AdminConfigPermissions.Default,
            L("Permission:AdminConfig")
        );

        adminConfigPermission.AddChild(
            AdminConfigPermissions.Create,
            L("Permission:Create")
        );

        adminConfigPermission.AddChild(
            AdminConfigPermissions.Edit,
            L("Permission:Edit")
        );

        adminConfigPermission.AddChild(
            AdminConfigPermissions.Delete,
            L("Permission:Delete")
        );

        adminConfigPermission.AddChild(
            AdminConfigPermissions.View,
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

**Location**: `src/common/infra/iOne.EntityFrameworkCore/AdminConfigs/AdminConfigConfiguration.cs`

**Yêu cầu**:
- Table name: `admin_config` (snake_case)
- Column names: snake_case
- **Composite unique index**: `ix_admin_config_code_sub_code` trên `(code, sub_code)`
- Status enum conversion: `Active`/`Deactive` → `"active"`/`"deactive"` (lowercase)

```csharp
using System;
using iOne.AdminConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.AdminConfigs;

public class AdminConfigConfiguration : IEntityTypeConfiguration<AdminConfig>
{
    public void Configure(EntityTypeBuilder<AdminConfig> builder)
    {
        builder.ToTable("admin_config", t =>
        {
            t.HasComment("Bảng lưu trữ các cấu hình chung của hệ thống");
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

        builder.Property(x => x.SubCode)
            .HasColumnName("sub_code")
            .HasMaxLength(25)
            .IsRequired()
            .HasComment("Mã con (nếu một cấu hình lớn cần chia nhiều cấu hình con)");

        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasMaxLength(250)
            .IsRequired()
            .HasComment("Giá trị cấu hình");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(250);

        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<AdminConfigStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive (case-insensitive)
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");

        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // ⚠️ QUAN TRỌNG: Composite unique index trên (code, sub_code)
        builder.HasIndex(e => new { e.Code, e.SubCode }, "ix_admin_config_code_sub_code")
            .IsUnique();
    }
}
```

### 6.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/AdminConfigs/EfCoreAdminConfigRepository.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.AdminConfigs;

namespace iOne.EntityFrameworkCore.AdminConfigs;

public class EfCoreAdminConfigRepository : EfCoreRepository<iOneDbContext, AdminConfig, Guid>, IAdminConfigRepository
{
    public EfCoreAdminConfigRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeSubCodeExistsAsync(string code, string subCode, Guid? excludeId = null)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => x.Code == code && x.SubCode == subCode);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<AdminConfig?> FindByCodeSubCodeAsync(string code, string subCode)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code && x.SubCode == subCode);
    }
}
```

### 6.3. Register trong DbContext

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`

- Thêm `DbSet<AdminConfig> AdminConfigs { get; set; }`
- Thêm `builder.ApplyConfiguration(new AdminConfigConfiguration());`

### 6.4. Register trong Module

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneEntityFrameworkCoreModule.cs`

- Thêm `using iOne.AdminConfigs;`
- Thêm `using iOne.EntityFrameworkCore.AdminConfigs;`
- Thêm `options.AddRepository<AdminConfig, EfCoreAdminConfigRepository>();`

## 7. HTTP API

### 7.1. Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/Controllers/AdminConfigController.cs`

```csharp
using System;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Permissions;
using iOne.Master.AdminConfigs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/admin-configs")]
[Authorize]
public class AdminConfigController : AbpControllerBase
{
    protected IAdminConfigAppService AppService { get; }

    public AdminConfigController(IAdminConfigAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(AdminConfigPermissions.View)]
    public virtual Task<PagedResultDto<AdminConfigDto>> GetListAsync(GetAdminConfigsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(AdminConfigPermissions.View)]
    public virtual Task<AdminConfigDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(AdminConfigPermissions.Create)]
    public virtual Task<AdminConfigDto> CreateAsync(CreateAdminConfigDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(AdminConfigPermissions.Edit)]
    public virtual Task<AdminConfigDto> UpdateAsync(Guid id, UpdateAdminConfigDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(AdminConfigPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
```

### 7.2. Exclude khỏi Conventional Controllers

**Location**: `modules/master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`

Thêm `AdminConfigAppService` vào exclude list:

```csharp
opts.TypePredicate = type =>
    type.Name != "ResCountryAppService" &&
    type.Name != "ResProvinceAppService" &&
    type.Name != "ResWardAppService" &&
    type.Name != "ResDocumentTypeAppService" &&
    type.Name != "AdminConfigAppService"; // Exclude AdminConfigAppService
```

## 8. Localization

### 8.1. Vietnamese (vi-VN.json)

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/vi-VN.json`

```json
{
  "Menu:AdminConfig": "Quản lý cấu hình chung",
  "Permission:AdminConfig": "Cấu hình chung",
  "AdminConfig:Code": "Mã cấu hình",
  "AdminConfig:Name": "Tên cấu hình",
  "AdminConfig:SubCode": "Mã con",
  "AdminConfig:Value": "Giá trị",
  "AdminConfig:Description": "Mô tả",
  "AdminConfig:Status": "Trạng thái",
  "AdminConfig:CodeRequired": "Mã cấu hình là bắt buộc",
  "AdminConfig:CodeMaxLength": "Mã cấu hình không được vượt quá 25 ký tự",
  "AdminConfig:CodeInvalid": "Mã cấu hình chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
  "AdminConfig:SubCodeRequired": "Mã con là bắt buộc",
  "AdminConfig:SubCodeMaxLength": "Mã con không được vượt quá 25 ký tự",
  "AdminConfig:SubCodeInvalid": "Mã con chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
  "AdminConfig:NameRequired": "Tên cấu hình là bắt buộc",
  "AdminConfig:NameMaxLength": "Tên cấu hình không được vượt quá 250 ký tự",
  "AdminConfig:ValueRequired": "Giá trị là bắt buộc",
  "AdminConfig:ValueMaxLength": "Giá trị không được vượt quá 250 ký tự",
  "AdminConfig:DescriptionMaxLength": "Mô tả không được vượt quá 250 ký tự",
  "AdminConfig:StatusRequired": "Trạng thái là bắt buộc",
  "AdminConfig:CodeSubCodeExists": "Cặp mã cấu hình '{Code}' và mã con '{SubCode}' đã tồn tại",
  "AdminConfig:CreatedSuccessfully": "Tạo cấu hình thành công",
  "AdminConfig:UpdatedSuccessfully": "Cập nhật cấu hình thành công",
  "AdminConfig:DeletedSuccessfully": "Xóa cấu hình thành công",
  "AdminConfig:New": "Thêm mới cấu hình",
  "AdminConfig:Edit": "Sửa cấu hình",
  "AdminConfig:Delete": "Xóa cấu hình",
  "AdminConfig:DeleteConfirm": "Bạn có chắc chắn muốn xóa cấu hình này?",
  "AdminConfig:CodeCannotBeChanged": "Mã cấu hình không được phép thay đổi",
  "AdminConfig:SubCodeCannotBeChanged": "Mã con không được phép thay đổi",
  "AdminConfig:Active": "Hoạt động",
  "AdminConfig:Deactive": "Không hoạt động",
  "AdminConfig:SearchByCode": "Tìm theo mã cấu hình",
  "AdminConfig:SearchBySubCode": "Tìm theo mã con",
  "AdminConfig:SearchByName": "Tìm theo tên cấu hình"
}
```

### 8.2. English (en.json)

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/en.json`

```json
{
  "Menu:AdminConfig": "Admin Config Management",
  "Permission:AdminConfig": "Admin Config",
  "AdminConfig:Code": "Config Code",
  "AdminConfig:Name": "Config Name",
  "AdminConfig:SubCode": "Sub Code",
  "AdminConfig:Value": "Value",
  "AdminConfig:Description": "Description",
  "AdminConfig:Status": "Status",
  "AdminConfig:CodeRequired": "Config code is required",
  "AdminConfig:CodeMaxLength": "Config code cannot exceed 25 characters",
  "AdminConfig:CodeInvalid": "Config code can only contain letters (A-Z), numbers (0-9) and underscore (_)",
  "AdminConfig:SubCodeRequired": "Sub code is required",
  "AdminConfig:SubCodeMaxLength": "Sub code cannot exceed 25 characters",
  "AdminConfig:SubCodeInvalid": "Sub code can only contain letters (A-Z), numbers (0-9) and underscore (_)",
  "AdminConfig:NameRequired": "Config name is required",
  "AdminConfig:NameMaxLength": "Config name cannot exceed 250 characters",
  "AdminConfig:ValueRequired": "Value is required",
  "AdminConfig:ValueMaxLength": "Value cannot exceed 250 characters",
  "AdminConfig:DescriptionMaxLength": "Description cannot exceed 250 characters",
  "AdminConfig:StatusRequired": "Status is required",
  "AdminConfig:CodeSubCodeExists": "Config code '{Code}' and sub code '{SubCode}' combination already exists",
  "AdminConfig:CreatedSuccessfully": "Config created successfully",
  "AdminConfig:UpdatedSuccessfully": "Config updated successfully",
  "AdminConfig:DeletedSuccessfully": "Config deleted successfully",
  "AdminConfig:New": "New Config",
  "AdminConfig:Edit": "Edit Config",
  "AdminConfig:Delete": "Delete Config",
  "AdminConfig:DeleteConfirm": "Are you sure you want to delete this config?",
  "AdminConfig:CodeCannotBeChanged": "Config code cannot be changed",
  "AdminConfig:SubCodeCannotBeChanged": "Sub code cannot be changed",
  "AdminConfig:Active": "Active",
  "AdminConfig:Deactive": "Deactive",
  "AdminConfig:SearchByCode": "Search by config code",
  "AdminConfig:SearchBySubCode": "Search by sub code",
  "AdminConfig:SearchByName": "Search by config name"
}
```

## 9. Menu Configuration

**Location**: `modules/master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

Thêm menu item:

```csharp
// AdminConfig Menu Item
masterMenuItem.AddItem(new ApplicationMenuItem(
    "Master.AdminConfig",
    masterL["Menu:AdminConfig"],
    url: "~/pages/master/admin-configs",
    icon: "pi pi-fw pi-cog"
).RequirePermissions(AdminConfigPermissions.Default));
```

## 10. Migration

### 10.1. Tạo Migration

```bash
cd src/common/infra/iOne.EntityFrameworkCore
dotnet ef migrations add AddAdminConfig --startup-project ../../iOne.DbMigrator/iOne.DbMigrator.csproj --context iOneDbContext
```

### 10.2. Migration Script

**Location**: `src/common/infra/iOne.EntityFrameworkCore/Migrations/YYYYMMDDHHMMSS_AddAdminConfig.cs`

**QUAN TRỌNG**: Sử dụng `IF EXISTS`/`IF NOT EXISTS` và snake_case naming:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP (nếu table đã tồn tại)
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""admin_config"";");

    migrationBuilder.CreateTable(
        name: "admin_config",
        columns: table => new
        {
            id = table.Column<Guid>(type: "uuid", nullable: false),
            code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
            name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
            sub_code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, comment: "Mã con (nếu một cấu hình lớn cần chia nhiều cấu hình con)"),
            value = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, comment: "Giá trị cấu hình"),
            description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
            status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động"),
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
            table.PrimaryKey("pk_admin_config", x => x.id);
        },
        comment: "Bảng lưu trữ các cấu hình chung của hệ thống");

    // ⚠️ QUAN TRỌNG: Composite unique index trên (code, sub_code)
    migrationBuilder.CreateIndex(
        name: "ix_admin_config_code_sub_code",
        table: "admin_config",
        columns: new[] { "code", "sub_code" },
        unique: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_admin_config_code_sub_code"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""admin_config"";");
}
```

## 11. Checklist

- [ ] Domain Layer:
  - [ ] Enum `AdminConfigStatus` đã tạo
  - [ ] Entity `AdminConfig` với private setters và validation (Code, Name, SubCode, Value, Description, Status)
  - [ ] Repository interface `IAdminConfigRepository` với `IsCodeSubCodeExistsAsync`, `FindByCodeSubCodeAsync`
  - [ ] Manager `AdminConfigManager` với business logic (composite unique key validation)
- [ ] Application Layer:
  - [ ] DTOs: `AdminConfigDto`, `CreateAdminConfigDto`, `UpdateAdminConfigDto` (không có Code và SubCode), `GetAdminConfigsInput`
  - [ ] Interface `IAdminConfigAppService`
  - [ ] Implementation `AdminConfigAppService` với override `DeleteAsync` (soft delete logic)
  - [ ] **AutoMapper configuration đã được thêm vào AutoMapper Profile**
- [ ] Permissions:
  - [ ] `AdminConfigPermissions` constants
  - [ ] `AdminConfigPermissionDefinitionProvider`
- [ ] EF Core:
  - [ ] `AdminConfigConfiguration` với snake_case naming, enum conversion (lowercase), và **composite unique index**
  - [ ] `EfCoreAdminConfigRepository` implementation
  - [ ] Register trong `iOneDbContext` và `iOneEntityFrameworkCoreModule`
- [ ] HTTP API:
  - [ ] `AdminConfigController` với CRUD endpoints
  - [ ] Exclude `AdminConfigAppService` khỏi conventional controllers
- [ ] Localization:
  - [ ] Keys đã thêm vào `vi-VN.json` và `en.json`
- [ ] Menu:
  - [ ] Menu item đã thêm vào `MasterMenuContributor`
- [ ] Migration:
  - [ ] Migration đã tạo với snake_case naming, composite unique index, và `IF EXISTS`/`IF NOT EXISTS`
  - [ ] Migration đã chạy thành công

## 12. Lưu Ý Quan Trọng

1. **Composite Unique Key**: 
   - `(Code, SubCode)` tạo thành khóa duy nhất
   - Trong cùng 1 Code, các SubCode không được trùng nhau
   - Cần validate uniqueness trong Manager khi create
   - Cần tạo composite unique index trong EF Core configuration

2. **Code và SubCode Immutability**: 
   - Code và SubCode chỉ được set trong constructor, không có `UpdateCode()` và `UpdateSubCode()` methods
   - Update DTO không có Code và SubCode properties

3. **Status Storage**: 
   - Enum `Active`/`Deactive` được lưu trong DB dưới dạng `"active"`/`"deactive"` (lowercase)

4. **Soft Delete**: 
   - Delete trước → Set Status = Deactive sau để trigger audit log

5. **PostgreSQL Naming**: 
   - Tất cả database objects (tables, columns, indexes) phải theo snake_case convention

6. **AutoMapper**: 
   - **BẮT BUỘC** phải thêm mapping configuration trong AutoMapper Profile

7. **Permissions**: 
   - GroupName = `"MasterAdminConfig"`, Default = GroupName

8. **Localization**: 
   - Format `Master::AdminConfig:{Key}`

9. **Menu**: 
   - URL = `"~/pages/master/admin-configs"`

10. **Validation**:
    - Code và SubCode: Chỉ cho phép A-Z, 0-9, _ (uppercase)
    - Name, Value: MaxLength 250
    - Description: MaxLength 250, optional

