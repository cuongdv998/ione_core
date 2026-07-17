# Plan: Backend - Danh mục Điều khoản hợp tác (ResAgreementTerm)

## 1. Tổng Quan

**Module**: Partner  
**Entity**: ResAgreementTerm  
**Table Name**: `res_agreement_term` (snake_case - PostgreSQL convention)  
**Mô tả**: Quản lý các điều khoản thỏa thuận (ví dụ có tham gia vai trò TPA không, có tham gia với vai trò tự quản lý khách hàng không, ...)

## 2. Database Schema

### 2.1. Table Structure

```sql
CREATE TABLE res_agreement_term (
    id uuid NOT NULL,
    code character varying(25) NOT NULL,
    name character varying(250) NOT NULL,
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
    CONSTRAINT pk_res_agreement_term PRIMARY KEY (id)
);

COMMENT ON TABLE res_agreement_term IS 'Các điều khoản thỏa thuận (ví dụ có tham gia vai trò TPA không, có tham gia với vai trò tự quản lý khách hàng không, ...)';

COMMENT ON COLUMN res_agreement_term.code IS 'Các mã điều khoản:
- IS_CUSTOMER_OF_BROKER: khách hàng là của Broker
- CDR_INFO: thông tin đẩy CDR file
- CAR_TPA: có ký kết TPA cho loại hình bảo hiểm xe ô tô hay không
- CAR_TPA_PAYMENT: có ký kết điều khoản thanh toán bồi thường hay không';

COMMENT ON COLUMN res_agreement_term.status IS 'Trạng thái:
- active: Hoạt động
- deactive: Không hoạt động';

CREATE UNIQUE INDEX ix_res_agreement_term_code ON res_agreement_term (code);
```

### 2.2. Naming Conventions (PostgreSQL Style)

- **Table name**: `res_agreement_term` (snake_case)
- **Column names**: `id`, `code`, `name`, `status`, `creation_time`, `creator_id`, `last_modification_time`, `last_modifier_id`, `is_deleted`, `deleter_id`, `deletion_time`, `concurrency_stamp`, `extra_properties` (snake_case)
- **Primary key**: `pk_res_agreement_term` (snake_case với prefix `pk_`)
- **Index**: `ix_res_agreement_term_code` (snake_case với prefix `ix_`)

## 3. Domain Layer

### 3.1. Enum: ResAgreementTermStatus

**Location**: `src/common/domain/iOne.Domain.Shared/ResAgreementTerms/ResAgreementTermStatus.cs`

```csharp
namespace iOne.ResAgreementTerms;

public enum ResAgreementTermStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

**Lưu ý**: Enum sẽ được convert sang string lowercase (`"active"`, `"deactive"`) khi lưu vào database.

### 3.2. Entity: ResAgreementTerm

**Location**: `src/common/domain/iOne.Domain/ResAgreementTerms/ResAgreementTerm.cs`

**Yêu cầu**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>`
- Table name: `res_agreement_term` (snake_case)
- Properties:
  - `Id`: `Guid` (từ base class)
  - `Code`: `string`, MaxLength(25), Required, **Immutable** (chỉ set trong constructor)
  - `Name`: `string`, MaxLength(250), Required
  - `Status`: `ResAgreementTermStatus`, Required
- Code validation: Chỉ cho phép A-Z, 0-9, _ (uppercase)
- Private setters cho tất cả properties
- Public methods: `UpdateName()`, `UpdateStatus()`
- **KHÔNG có** `UpdateCode()` method

**Code Structure**:
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResAgreementTerms;

[Table("res_agreement_term")]
public class ResAgreementTerm : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResAgreementTermStatus Status { get; private set; }

    protected ResAgreementTerm()
    {
        // For ORM
    }

    public ResAgreementTerm(
        Guid id,
        string code,
        string name,
        ResAgreementTermStatus status)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetStatus(status);
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

    private void SetStatus(ResAgreementTermStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResAgreementTermStatus status)
    {
        SetStatus(status);
    }
}
```

### 3.3. Repository Interface: IResAgreementTermRepository

**Location**: `src/common/domain/iOne.Domain/ResAgreementTerms/IResAgreementTermRepository.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResAgreementTerms;

public interface IResAgreementTermRepository : IRepository<ResAgreementTerm, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<ResAgreementTerm?> FindByCodeAsync(string code);
}
```

### 3.4. Manager: ResAgreementTermManager

**Location**: `src/common/domain/iOne.Domain/ResAgreementTerms/ResAgreementTermManager.cs`

**Yêu cầu**:
- Business logic validation
- Check code uniqueness khi create
- Validate code format

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResAgreementTerms;

public class ResAgreementTermManager : DomainService
{
    protected IResAgreementTermRepository Repository { get; }

    public ResAgreementTermManager(IResAgreementTermRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResAgreementTerm term)
    {
        // Check code uniqueness
        if (await Repository.FindByCodeAsync(term.Code) != null)
        {
            throw new BusinessException("Partner:ResAgreementTerm:CodeExists")
                .WithData("Code", term.Code);
        }

        await Repository.InsertAsync(term);
    }

    public virtual async Task UpdateAsync(
        ResAgreementTerm term,
        string name,
        ResAgreementTermStatus status)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        term.UpdateName(name);
        term.UpdateStatus(status);
        await Repository.UpdateAsync(term);
    }
}
```

## 4. Application Layer

### 4.1. DTOs

#### 4.1.1. ResAgreementTermDto

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResAgreementTerms/ResAgreementTermDto.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResAgreementTerms;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResAgreementTerms;

public class ResAgreementTermDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Partner::ResAgreementTerm:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Partner::ResAgreementTerm:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Partner::ResAgreementTerm:Status")]
    public ResAgreementTermStatus Status { get; set; }
}
```

#### 4.1.2. CreateResAgreementTermDto

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResAgreementTerms/CreateResAgreementTermDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.ResAgreementTerms;

namespace iOne.Partner.ResAgreementTerms;

public class CreateResAgreementTermDto
{
    [Required(ErrorMessage = "Partner::ResAgreementTerm:CodeRequired")]
    [StringLength(25, ErrorMessage = "Partner::ResAgreementTerm:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Partner::ResAgreementTerm:CodeInvalid")]
    [Display(Name = "Partner::ResAgreementTerm:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResAgreementTerm:NameRequired")]
    [StringLength(250, ErrorMessage = "Partner::ResAgreementTerm:NameMaxLength")]
    [Display(Name = "Partner::ResAgreementTerm:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResAgreementTerm:StatusRequired")]
    [Display(Name = "Partner::ResAgreementTerm:Status")]
    public ResAgreementTermStatus Status { get; set; } = ResAgreementTermStatus.Active;
}
```

#### 4.1.3. UpdateResAgreementTermDto

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResAgreementTerms/UpdateResAgreementTermDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.ResAgreementTerms;

namespace iOne.Partner.ResAgreementTerms;

public class UpdateResAgreementTermDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "Partner::ResAgreementTerm:NameRequired")]
    [StringLength(250, ErrorMessage = "Partner::ResAgreementTerm:NameMaxLength")]
    [Display(Name = "Partner::ResAgreementTerm:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResAgreementTerm:StatusRequired")]
    [Display(Name = "Partner::ResAgreementTerm:Status")]
    public ResAgreementTermStatus Status { get; set; }
}
```

#### 4.1.4. GetResAgreementTermsInput

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResAgreementTerms/GetResAgreementTermsInput.cs`

```csharp
using iOne.ResAgreementTerms;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResAgreementTerms;

public class GetResAgreementTermsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResAgreementTermStatus? Status { get; set; }
}
```

### 4.2. Application Service Interface

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResAgreementTerms/IResAgreementTermAppService.cs`

```csharp
using System;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResAgreementTerms;

public interface IResAgreementTermAppService : ICrudAppService<
    ResAgreementTermDto,
    Guid,
    GetResAgreementTermsInput,
    CreateResAgreementTermDto,
    UpdateResAgreementTermDto>
{
}
```

### 4.3. Application Service Implementation

**Location**: `modules/partner/src/iOne.Partner.Application/ResAgreementTerms/ResAgreementTermAppService.cs`

**Yêu cầu**:
- Override `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetAsync`, `GetListAsync`
- `DeleteAsync`: Delete trước → Set Status = Deactive sau (để trigger audit log)
- Filter theo Code, Name, Status
- Sử dụng Manager cho business logic

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Partner.Localization;
using iOne.Partner.Permissions;
using iOne.Partner.ResAgreementTerms;
using iOne.ResAgreementTerms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResAgreementTerms;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResAgreementTermPermissions.Default)]
public class ResAgreementTermAppService : CrudAppService<
    ResAgreementTerm,
    ResAgreementTermDto,
    Guid,
    GetResAgreementTermsInput,
    CreateResAgreementTermDto,
    UpdateResAgreementTermDto>,
    IResAgreementTermAppService
{
    protected ResAgreementTermManager Manager { get; }

    public ResAgreementTermAppService(
        IResAgreementTermRepository repository,
        ResAgreementTermManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(PartnerResource);
        GetPolicyName = ResAgreementTermPermissions.View;
        GetListPolicyName = ResAgreementTermPermissions.View;
        CreatePolicyName = ResAgreementTermPermissions.Create;
        UpdatePolicyName = ResAgreementTermPermissions.Edit;
        DeletePolicyName = ResAgreementTermPermissions.Delete;
    }

    public override async Task<ResAgreementTermDto> CreateAsync(CreateResAgreementTermDto input)
    {
        var entity = new ResAgreementTerm(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResAgreementTerm, ResAgreementTermDto>(entity);
    }

    public override async Task<ResAgreementTermDto> UpdateAsync(Guid id, UpdateResAgreementTermDto input)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name và Status - không update Code
        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Status
        );

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResAgreementTerm, ResAgreementTermDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResAgreementTermStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResAgreementTerm>> CreateFilteredQueryAsync(GetResAgreementTermsInput input)
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

**Location**: `modules/partner/src/iOne.Partner.Application/iOnePartnerApplicationAutoMapperProfile.cs`

**QUAN TRỌNG**: Phải thêm mapping configuration:

```csharp
// ResAgreementTerm mappings
CreateMap<ResAgreementTerm, ResAgreementTermDto>();
CreateMap<CreateResAgreementTermDto, ResAgreementTerm>();
CreateMap<UpdateResAgreementTermDto, ResAgreementTerm>();
```

## 5. Permissions

### 5.1. Permission Constants

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Permissions/ResAgreementTermPermissions.cs`

```csharp
namespace iOne.Partner.Permissions;

public static class ResAgreementTermPermissions
{
    public const string GroupName = "PartnerResAgreementTerm";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

### 5.2. Permission Definition Provider

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Permissions/ResAgreementTermPermissionDefinitionProvider.cs`

```csharp
using iOne.Partner.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Partner.Permissions;

public class ResAgreementTermPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resAgreementTermGroup = context.AddGroup(
            ResAgreementTermPermissions.GroupName,
            L("Permission:ResAgreementTerm")
        );

        var resAgreementTermPermission = resAgreementTermGroup.AddPermission(
            ResAgreementTermPermissions.Default,
            L("Permission:ResAgreementTerm")
        );

        resAgreementTermPermission.AddChild(
            ResAgreementTermPermissions.Create,
            L("Permission:Create")
        );

        resAgreementTermPermission.AddChild(
            ResAgreementTermPermissions.Edit,
            L("Permission:Edit")
        );

        resAgreementTermPermission.AddChild(
            ResAgreementTermPermissions.Delete,
            L("Permission:Delete")
        );

        resAgreementTermPermission.AddChild(
            ResAgreementTermPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PartnerResource>(name);
    }
}
```

## 6. Entity Framework Core

### 6.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResAgreementTerms/ResAgreementTermConfiguration.cs`

**Yêu cầu**:
- Table name: `res_agreement_term` (snake_case)
- Column names: snake_case
- Index: `ix_res_agreement_term_code` (unique)
- Status enum conversion: `Active`/`Deactive` → `"active"`/`"deactive"` (lowercase)

```csharp
using System;
using iOne.ResAgreementTerms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResAgreementTerms;

public class ResAgreementTermConfiguration : IEntityTypeConfiguration<ResAgreementTerm>
{
    public void Configure(EntityTypeBuilder<ResAgreementTerm> builder)
    {
        builder.ToTable("res_agreement_term", t =>
        {
            t.HasComment("Các điều khoản thỏa thuận (ví dụ có tham gia vai trò TPA không, có tham gia với vai trò tự quản lý khách hàng không, ...)");
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

        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ResAgreementTermStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive (case-insensitive)
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

        builder.HasIndex(e => e.Code, "ix_res_agreement_term_code")
            .IsUnique();
    }
}
```

### 6.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResAgreementTerms/EfCoreResAgreementTermRepository.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResAgreementTerms;

namespace iOne.EntityFrameworkCore.ResAgreementTerms;

public class EfCoreResAgreementTermRepository : EfCoreRepository<iOneDbContext, ResAgreementTerm, Guid>, IResAgreementTermRepository
{
    public EfCoreResAgreementTermRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public async Task<ResAgreementTerm?> FindByCodeAsync(string code)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code);
    }
}
```

### 6.3. Register trong DbContext

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`

- Thêm `DbSet<ResAgreementTerm> ResAgreementTerms { get; set; }`
- Thêm `builder.ApplyConfiguration(new ResAgreementTermConfiguration());`

### 6.4. Register trong Module

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneEntityFrameworkCoreModule.cs`

- Thêm `using iOne.ResAgreementTerms;`
- Thêm `using iOne.EntityFrameworkCore.ResAgreementTerms;`
- Thêm `options.AddRepository<ResAgreementTerm, EfCoreResAgreementTermRepository>();`

## 7. HTTP API

### 7.1. Controller

**Location**: `modules/partner/src/iOne.Partner.HttpApi/Controllers/ResAgreementTermController.cs`

```csharp
using System;
using System.Threading.Tasks;
using iOne.Partner;
using iOne.Partner.Permissions;
using iOne.Partner.ResAgreementTerms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Partner.Controllers;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Area(PartnerRemoteServiceConsts.ModuleName)]
[Route("api/partner/agreement-terms")]
[Authorize]
public class ResAgreementTermController : AbpControllerBase
{
    protected IResAgreementTermAppService AppService { get; }

    public ResAgreementTermController(IResAgreementTermAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResAgreementTermPermissions.View)]
    public virtual Task<PagedResultDto<ResAgreementTermDto>> GetListAsync(GetResAgreementTermsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResAgreementTermPermissions.View)]
    public virtual Task<ResAgreementTermDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResAgreementTermPermissions.Create)]
    public virtual Task<ResAgreementTermDto> CreateAsync(CreateResAgreementTermDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResAgreementTermPermissions.Edit)]
    public virtual Task<ResAgreementTermDto> UpdateAsync(Guid id, UpdateResAgreementTermDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResAgreementTermPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
```

### 7.2. Exclude khỏi Conventional Controllers

**Location**: `modules/partner/src/iOne.Partner.HttpApi/iOnePartnerHttpApiModule.cs`

Thêm `ResAgreementTermAppService` vào exclude list:

```csharp
opts.TypePredicate = type =>
    type.Name != "ResPartnerTypeAppService" &&
    type.Name != "ResOrganizationTypeAppService" &&
    type.Name != "ResChannelAppService" &&
    type.Name != "ResAgreementTermAppService"; // Exclude ResAgreementTermAppService
```

## 8. Localization

### 8.1. Vietnamese (vi-VN.json)

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Localization/Partner/vi-VN.json`

```json
{
  "Menu:ResAgreementTerm": "Danh mục Điều khoản hợp tác",
  "Permission:ResAgreementTerm": "Điều khoản hợp tác",
  "ResAgreementTerm:Code": "Mã điều khoản",
  "ResAgreementTerm:Name": "Tên điều khoản",
  "ResAgreementTerm:Status": "Trạng thái",
  "ResAgreementTerm:CodeRequired": "Mã điều khoản là bắt buộc",
  "ResAgreementTerm:CodeMaxLength": "Mã điều khoản không được vượt quá 25 ký tự",
  "ResAgreementTerm:CodeInvalid": "Mã điều khoản chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
  "ResAgreementTerm:NameRequired": "Tên điều khoản là bắt buộc",
  "ResAgreementTerm:NameMaxLength": "Tên điều khoản không được vượt quá 250 ký tự",
  "ResAgreementTerm:StatusRequired": "Trạng thái là bắt buộc",
  "ResAgreementTerm:CodeExists": "Mã điều khoản '{Code}' đã tồn tại",
  "ResAgreementTerm:CreatedSuccessfully": "Tạo điều khoản hợp tác thành công",
  "ResAgreementTerm:UpdatedSuccessfully": "Cập nhật điều khoản hợp tác thành công",
  "ResAgreementTerm:DeletedSuccessfully": "Xóa điều khoản hợp tác thành công",
  "ResAgreementTerm:New": "Thêm mới điều khoản hợp tác",
  "ResAgreementTerm:Edit": "Sửa điều khoản hợp tác",
  "ResAgreementTerm:Delete": "Xóa điều khoản hợp tác",
  "ResAgreementTerm:DeleteConfirm": "Bạn có chắc chắn muốn xóa điều khoản hợp tác này?",
  "ResAgreementTerm:CodeCannotBeChanged": "Mã điều khoản không được phép thay đổi",
  "ResAgreementTerm:Active": "Hoạt động",
  "ResAgreementTerm:Deactive": "Không hoạt động",
  "ResAgreementTerm:SearchByCode": "Tìm theo mã điều khoản",
  "ResAgreementTerm:SearchByName": "Tìm theo tên điều khoản"
}
```

### 8.2. English (en.json)

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Localization/Partner/en.json`

```json
{
  "Menu:ResAgreementTerm": "Agreement Term Master",
  "Permission:ResAgreementTerm": "Agreement Term",
  "ResAgreementTerm:Code": "Agreement Term Code",
  "ResAgreementTerm:Name": "Agreement Term Name",
  "ResAgreementTerm:Status": "Status",
  "ResAgreementTerm:CodeRequired": "Agreement term code is required",
  "ResAgreementTerm:CodeMaxLength": "Agreement term code cannot exceed 25 characters",
  "ResAgreementTerm:CodeInvalid": "Agreement term code can only contain letters (A-Z), numbers (0-9) and underscore (_)",
  "ResAgreementTerm:NameRequired": "Agreement term name is required",
  "ResAgreementTerm:NameMaxLength": "Agreement term name cannot exceed 250 characters",
  "ResAgreementTerm:StatusRequired": "Status is required",
  "ResAgreementTerm:CodeExists": "Agreement term code '{Code}' already exists",
  "ResAgreementTerm:CreatedSuccessfully": "Agreement term created successfully",
  "ResAgreementTerm:UpdatedSuccessfully": "Agreement term updated successfully",
  "ResAgreementTerm:DeletedSuccessfully": "Agreement term deleted successfully",
  "ResAgreementTerm:New": "New Agreement Term",
  "ResAgreementTerm:Edit": "Edit Agreement Term",
  "ResAgreementTerm:Delete": "Delete Agreement Term",
  "ResAgreementTerm:DeleteConfirm": "Are you sure you want to delete this agreement term?",
  "ResAgreementTerm:CodeCannotBeChanged": "Agreement term code cannot be changed",
  "ResAgreementTerm:Active": "Active",
  "ResAgreementTerm:Deactive": "Deactive",
  "ResAgreementTerm:SearchByCode": "Search by agreement term code",
  "ResAgreementTerm:SearchByName": "Search by agreement term name"
}
```

## 9. Menu Configuration

**Location**: `modules/partner/src/iOne.Partner.Application/Navigation/PartnerMenuContributor.cs`

Thêm menu item:

```csharp
// ResAgreementTerm Menu Item
partnerMenuItem.AddItem(new ApplicationMenuItem(
    "Partner.ResAgreementTerm",
    partnerL["Menu:ResAgreementTerm"],
    url: "~/pages/partner/agreement-terms",
    icon: "pi pi-fw pi-file-edit"
).RequirePermissions(ResAgreementTermPermissions.Default));
```

## 10. Migration

### 10.1. Tạo Migration

```bash
cd src/common/infra/iOne.EntityFrameworkCore
dotnet ef migrations add AddResAgreementTerm --startup-project ../../iOne.DbMigrator/iOne.DbMigrator.csproj --context iOneDbContext
```

### 10.2. Migration Script

**Location**: `src/common/infra/iOne.EntityFrameworkCore/Migrations/YYYYMMDDHHMMSS_AddResAgreementTerm.cs`

**QUAN TRỌNG**: Sử dụng `IF EXISTS`/`IF NOT EXISTS` và snake_case naming:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP (nếu table đã tồn tại)
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_agreement_term"";");

    migrationBuilder.CreateTable(
        name: "res_agreement_term",
        columns: table => new
        {
            id = table.Column<Guid>(type: "uuid", nullable: false),
            code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
            name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
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
            table.PrimaryKey("pk_res_agreement_term", x => x.id);
        },
        comment: "Các điều khoản thỏa thuận (ví dụ có tham gia vai trò TPA không, có tham gia với vai trò tự quản lý khách hàng không, ...)");

    // ✅ ĐÚNG: Index name theo snake_case với prefix
    migrationBuilder.CreateIndex(
        name: "ix_res_agreement_term_code",
        table: "res_agreement_term",
        column: "code",
        unique: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_agreement_term_code"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_agreement_term"";");
}
```

## 11. Checklist

- [ ] Domain Layer:
  - [ ] Enum `ResAgreementTermStatus` đã tạo
  - [ ] Entity `ResAgreementTerm` với private setters và validation
  - [ ] Repository interface `IResAgreementTermRepository` với `IsCodeExistsAsync`, `FindByCodeAsync`
  - [ ] Manager `ResAgreementTermManager` với business logic
- [ ] Application Layer:
  - [ ] DTOs: `ResAgreementTermDto`, `CreateResAgreementTermDto`, `UpdateResAgreementTermDto` (không có Code), `GetResAgreementTermsInput`
  - [ ] Interface `IResAgreementTermAppService`
  - [ ] Implementation `ResAgreementTermAppService` với override `DeleteAsync` (soft delete logic)
  - [ ] **AutoMapper configuration đã được thêm vào AutoMapper Profile**
- [ ] Permissions:
  - [ ] `ResAgreementTermPermissions` constants
  - [ ] `ResAgreementTermPermissionDefinitionProvider`
- [ ] EF Core:
  - [ ] `ResAgreementTermConfiguration` với snake_case naming và enum conversion (lowercase)
  - [ ] `EfCoreResAgreementTermRepository` implementation
  - [ ] Register trong `iOneDbContext` và `iOneEntityFrameworkCoreModule`
- [ ] HTTP API:
  - [ ] `ResAgreementTermController` với CRUD endpoints
  - [ ] Exclude `ResAgreementTermAppService` khỏi conventional controllers
- [ ] Localization:
  - [ ] Keys đã thêm vào `vi-VN.json` và `en.json`
- [ ] Menu:
  - [ ] Menu item đã thêm vào `PartnerMenuContributor`
- [ ] Migration:
  - [ ] Migration đã tạo với snake_case naming và `IF EXISTS`/`IF NOT EXISTS`
  - [ ] Migration đã chạy thành công

## 12. Lưu Ý Quan Trọng

1. **Code Immutability**: Code chỉ được set trong constructor, không có `UpdateCode()` method
2. **Status Storage**: Enum `Active`/`Deactive` được lưu trong DB dưới dạng `"active"`/`"deactive"` (lowercase)
3. **Soft Delete**: Delete trước → Set Status = Deactive sau để trigger audit log
4. **PostgreSQL Naming**: Tất cả database objects (tables, columns, indexes) phải theo snake_case convention
5. **AutoMapper**: **BẮT BUỘC** phải thêm mapping configuration trong AutoMapper Profile
6. **Permissions**: GroupName = `"PartnerResAgreementTerm"`, Default = GroupName
7. **Localization**: Format `Partner::ResAgreementTerm:{Key}`
8. **Menu**: URL = `"~/pages/partner/agreement-terms"`

