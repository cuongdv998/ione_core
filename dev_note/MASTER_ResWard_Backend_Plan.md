# Plan: Backend Danh mục Phường/Xã (ResWard) - Module Master

## 1. Tổng Quan

**Entity Name**: `ResWard`  
**Module**: `Master`  
**Table Name**: `res_ward` (snake_case - PostgreSQL convention)  
**Namespace**: `iOne.ResWards`

**Chức năng**: Danh mục Phường/Xã
- ✅ Xem danh sách (có filter/search)
- ✅ Thêm mới
- ✅ Sửa (Code không được phép sửa - disable trên UI)
- ✅ Xóa (Soft delete: Delete trước → Set Status = Deactive sau)

**Yêu cầu đặc biệt**:
- Code chỉ cho phép: A-Z, 0-9, _ (uppercase)
- Code là duy nhất (unique)
- Code không được phép sửa khi update (disable trên UI)
- Xóa: Gọi `DeleteAsync()` trước để trigger audit log, sau đó set `Status = Deactive`
- **Status trong DB**: Lưu dưới dạng `"active"` hoặc `"deactive"` (lowercase), không phải "Active" hay "Deactive"
- **Foreign Key**: `ProvinceId` (Guid) → `res_province.id` (on delete restrict)
- **Description**: Optional field, max length 500

**Lưu ý Database**:
- Table name trong DDL: `RESWARD` → sẽ đổi thành `res_ward` (snake_case)
- Column names trong DDL: UPPERCASE → sẽ đổi thành snake_case
- ID và audit IDs: UUID type (PostgreSQL) - EF Core tự động map `Guid` sang `UUID`
- Foreign key: `province_id` (snake_case) → `res_province.id`
- **QUAN TRỌNG**: DDL có `CREATIONTIME DATE` và `CREATORID NUMERIC(10)` - cần chuyển sang:
  - `creation_time`: `timestamp without time zone` (DateTime)
  - `creator_id`: `uuid` (Guid)
  - Tương tự cho các audit columns khác

---

## 2. Domain Layer

### 2.1. Enum: ResWardStatus

**Location**: `src/common/domain/iOne.Domain.Shared/ResWards/ResWardStatus.cs`

```csharp
namespace iOne.ResWards;

public enum ResWardStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

**Đặc điểm**:
- Enum values trong C#: `Active = 0`, `Deactive = 1` (PascalCase)
- **QUAN TRỌNG**: Trong database lưu dưới dạng string `"active"`, `"deactive"` (lowercase, không phải "Active" hay "Deactive")
- Sử dụng `HasConversion<string>()` với `ToLowerInvariant()` trong EF Core configuration để tự động convert

---

### 2.2. Entity: ResWard

**Location**: `src/common/domain/iOne.Domain/ResWards/ResWard.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResProvinces;

namespace iOne.ResWards;

[Table("res_ward")] // ✅ snake_case (PostgreSQL convention)
public class ResWard : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ProvinceId { get; private set; }

    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResWardStatus Status { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    // Navigation property
    public virtual ResProvince? Province { get; set; }

    protected ResWard()
    {
        // For ORM
    }

    public ResWard(
        Guid id,
        Guid provinceId,
        string code,
        string name,
        ResWardStatus status,
        string? description = null)
        : base(id)
    {
        SetProvinceId(provinceId);
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetDescription(description);
    }

    private void SetProvinceId(Guid provinceId)
    {
        if (provinceId == Guid.Empty)
        {
            throw new ArgumentException("ProvinceId cannot be empty.", nameof(provinceId));
        }

        ProvinceId = provinceId;
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

    private void SetStatus(ResWardStatus status)
    {
        Status = status;
    }

    private void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }

        Description = description;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateProvinceId(Guid provinceId)
    {
        SetProvinceId(provinceId);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResWardStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }
}
```

**Đặc điểm**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>` (có đầy đủ audit fields)
- `Code` có private setter và không có `UpdateCode()` method → Code không được phép sửa
- Validation trong constructor và private setters
- Navigation property `Province` để eager load khi cần

---

### 2.3. Repository Interface: IResWardRepository

**Location**: `src/common/domain/iOne.Domain/ResWards/IResWardRepository.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResWards;

public interface IResWardRepository : IRepository<ResWard, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<ResWard?> FindByCodeAsync(string code);
}
```

**Methods**:
- `IsCodeExistsAsync`: Check code uniqueness (có thể exclude một ID khi update)
- `FindByCodeAsync`: Tìm ward theo code

---

### 2.4. Domain Service: ResWardManager

**Location**: `src/common/domain/iOne.Domain/ResWards/ResWardManager.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using iOne.ResProvinces;

namespace iOne.ResWards;

public class ResWardManager : DomainService
{
    protected IResWardRepository Repository { get; }
    protected IResProvinceRepository ProvinceRepository { get; }

    public ResWardManager(
        IResWardRepository repository,
        IResProvinceRepository provinceRepository)
    {
        Repository = repository;
        ProvinceRepository = provinceRepository;
    }

    public virtual async Task CreateAsync(ResWard ward)
    {
        // Check province exists
        var province = await ProvinceRepository.FindAsync(ward.ProvinceId);
        if (province == null)
        {
            throw new BusinessException("Master:ResWard:ProvinceNotFound")
                .WithData("ProvinceId", ward.ProvinceId);
        }

        // Check code uniqueness
        if (await Repository.FindByCodeAsync(ward.Code) != null)
        {
            throw new BusinessException("Master:ResWard:CodeExists")
                .WithData("Code", ward.Code);
        }

        await Repository.InsertAsync(ward);
    }

    public virtual async Task UpdateAsync(
        ResWard ward,
        Guid provinceId,
        string name,
        ResWardStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        // Check province exists (if changed)
        if (ward.ProvinceId != provinceId)
        {
            var province = await ProvinceRepository.FindAsync(provinceId);
            if (province == null)
            {
                throw new BusinessException("Master:ResWard:ProvinceNotFound")
                    .WithData("ProvinceId", provinceId);
            }
        }

        ward.UpdateProvinceId(provinceId);
        ward.UpdateName(name);
        ward.UpdateStatus(status);
        ward.UpdateDescription(description);
        await Repository.UpdateAsync(ward);
    }
}
```

**Đặc điểm**:
- Validate `ProvinceId` tồn tại
- Validate `Code` uniqueness
- **QUAN TRỌNG**: `UpdateAsync` không có parameter `code` → Code không được phép sửa

---

## 3. Application Layer

### 3.1. DTOs

#### 3.1.1. ResWardDto

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResWards/ResWardDto.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResWards;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResWards;

public class ResWardDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Master::ResWard:Province")]
    public Guid ProvinceId { get; set; }

    [Display(Name = "Master::ResWard:ProvinceName")]
    public string ProvinceName { get; set; } = null!; // For display purposes

    [Display(Name = "Master::ResWard:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Master::ResWard:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Master::ResWard:Status")]
    public ResWardStatus Status { get; set; }

    [Display(Name = "Master::ResWard:Description")]
    public string? Description { get; set; }
}
```

**Đặc điểm**:
- Kế thừa từ `FullAuditedEntityDto<Guid>` (có đầy đủ audit fields)
- Có `ProvinceName` để hiển thị tên tỉnh/thành (không phải chỉ ID)

---

#### 3.1.2. CreateResWardDto

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResWards/CreateResWardDto.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResWards;

namespace iOne.Master.ResWards;

public class CreateResWardDto
{
    [Required(ErrorMessage = "Master::ResWard:ProvinceRequired")]
    [Display(Name = "Master::ResWard:Province")]
    public Guid ProvinceId { get; set; }

    [Required(ErrorMessage = "Master::ResWard:CodeRequired")]
    [StringLength(25, ErrorMessage = "Master::ResWard:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Master::ResWard:CodeInvalid")]
    [Display(Name = "Master::ResWard:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Master::ResWard:NameRequired")]
    [StringLength(250, ErrorMessage = "Master::ResWard:NameMaxLength")]
    [Display(Name = "Master::ResWard:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Master::ResWard:StatusRequired")]
    [Display(Name = "Master::ResWard:Status")]
    public ResWardStatus Status { get; set; } = ResWardStatus.Active;

    [StringLength(500, ErrorMessage = "Master::ResWard:DescriptionMaxLength")]
    [Display(Name = "Master::ResWard:Description")]
    public string? Description { get; set; }
}
```

**Đặc điểm**:
- Validation attributes đầy đủ
- `Code` có `RegularExpression` để validate format: `^[A-Z0-9_]+$`
- `Status` default = `Active`

---

#### 3.1.3. UpdateResWardDto

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResWards/UpdateResWardDto.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResWards;

namespace iOne.Master.ResWards;

public class UpdateResWardDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "Master::ResWard:ProvinceRequired")]
    [Display(Name = "Master::ResWard:Province")]
    public Guid ProvinceId { get; set; }

    [Required(ErrorMessage = "Master::ResWard:NameRequired")]
    [StringLength(250, ErrorMessage = "Master::ResWard:NameMaxLength")]
    [Display(Name = "Master::ResWard:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Master::ResWard:StatusRequired")]
    [Display(Name = "Master::ResWard:Status")]
    public ResWardStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "Master::ResWard:DescriptionMaxLength")]
    [Display(Name = "Master::ResWard:Description")]
    public string? Description { get; set; }
}
```

**Đặc điểm**:
- **KHÔNG có** `Code` property → Code không được phép sửa
- Chỉ có các fields có thể update: `ProvinceId`, `Name`, `Status`, `Description`

---

#### 3.1.4. GetResWardsInput

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResWards/GetResWardsInput.cs`

```csharp
using System;
using iOne.ResWards;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResWards;

public class GetResWardsInput : PagedAndSortedResultRequestDto
{
    public Guid? ProvinceId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResWardStatus? Status { get; set; }
}
```

**Đặc điểm**:
- Kế thừa từ `PagedAndSortedResultRequestDto` (có pagination và sorting)
- Filter theo: `ProvinceId`, `Code`, `Name`, `Status`

---

### 3.2. Application Service Interface

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResWards/IResWardAppService.cs`

```csharp
using System;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResWards;

public interface IResWardAppService : ICrudAppService<
    ResWardDto,
    Guid,
    GetResWardsInput,
    CreateResWardDto,
    UpdateResWardDto>
{
}
```

**Đặc điểm**:
- Kế thừa từ `ICrudAppService` → có sẵn CRUD operations
- Generic types: `ResWardDto`, `Guid`, `GetResWardsInput`, `CreateResWardDto`, `UpdateResWardDto`

---

### 3.3. Application Service Implementation

**Location**: `modules/master/src/iOne.Master.Application/ResWards/ResWardAppService.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResWards;
using iOne.ResProvinces;
using iOne.ResWards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResWards;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResWardPermissions.Default)]
public class ResWardAppService : CrudAppService<
    ResWard,
    ResWardDto,
    Guid,
    GetResWardsInput,
    CreateResWardDto,
    UpdateResWardDto>,
    IResWardAppService
{
    protected ResWardManager Manager { get; }
    protected IResProvinceRepository ProvinceRepository { get; }

    public ResWardAppService(
        IResWardRepository repository,
        ResWardManager manager,
        IResProvinceRepository provinceRepository)
        : base(repository)
    {
        Manager = manager;
        ProvinceRepository = provinceRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResWardPermissions.View;
        GetListPolicyName = ResWardPermissions.View;
        CreatePolicyName = ResWardPermissions.Create;
        UpdatePolicyName = ResWardPermissions.Edit;
        DeletePolicyName = ResWardPermissions.Delete;
    }

    public override async Task<ResWardDto> CreateAsync(CreateResWardDto input)
    {
        var entity = new ResWard(
            GuidGenerator.Create(),
            input.ProvinceId,
            input.Code,
            input.Name,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return await MapToDtoWithProvinceNameAsync(entity);
    }

    public override async Task<ResWardDto> UpdateAsync(Guid id, UpdateResWardDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update ProvinceId, Name, Status, Description - không update Code
        await Manager.UpdateAsync(
            entity,
            input.ProvinceId,
            input.Name,
            input.Status,
            input.Description
        );

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return await MapToDtoWithProvinceNameAsync(entity);
    }

    public override async Task<ResWardDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        return await MapToDtoWithProvinceNameAsync(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResWardStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public override async Task<PagedResultDto<ResWardDto>> GetListAsync(GetResWardsInput input)
    {
        // Get filtered query with Province included
        var query = await CreateFilteredQueryAsync(input);
        
        // Get total count
        var totalCount = await AsyncExecuter.CountAsync(query);

        // Apply sorting and paging
        query = ApplySorting(query, input);
        query = ApplyPaging(query, input);

        // Execute query and get entities with Province navigation property loaded
        var entities = await AsyncExecuter.ToListAsync(query);

        // Map all entities to DTOs at once
        var dtos = ObjectMapper.Map<List<ResWard>, List<ResWardDto>>(entities);

        // Set ProvinceName from loaded navigation property (no additional queries needed)
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i].Province != null)
            {
                dtos[i].ProvinceName = entities[i].Province.Name;
            }
        }

        return new PagedResultDto<ResWardDto>(totalCount, dtos);
    }

    protected async Task<ResWardDto> MapToDtoWithProvinceNameAsync(ResWard entity)
    {
        var dto = ObjectMapper.Map<ResWard, ResWardDto>(entity);

        // Load province name if needed
        if (entity.ProvinceId != Guid.Empty)
        {
            // If Province navigation property is already loaded, use it
            if (entity.Province != null)
            {
                dto.ProvinceName = entity.Province.Name;
            }
            else
            {
                // Otherwise, load from repository
                var province = await ProvinceRepository.GetAsync(entity.ProvinceId);
                dto.ProvinceName = province.Name;
            }
        }

        return dto;
    }

    protected override async Task<IQueryable<ResWard>> CreateFilteredQueryAsync(GetResWardsInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Include Province navigation property to avoid N+1 queries
        query = query.Include(x => x.Province);

        // Filter by ProvinceId
        if (input.ProvinceId.HasValue)
        {
            query = query.Where(x => x.ProvinceId == input.ProvinceId.Value);
        }

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

**Đặc điểm**:
- Override `GetListAsync` để load `ProvinceName` cho tất cả items (tối ưu với Include)
- Override `DeleteAsync` để: Delete trước → Set Status = Deactive sau
- `CreateFilteredQueryAsync` Include `Province` để tránh N+1 queries

---

### 3.4. AutoMapper Configuration

**Location**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

**Cần thêm**:
```csharp
// ResWard mappings
CreateMap<ResWard, ResWardDto>();
CreateMap<CreateResWardDto, ResWard>();
CreateMap<UpdateResWardDto, ResWard>();
```

---

## 4. Permissions

### 4.1. Permission Constants

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResWardPermissions.cs`

```csharp
namespace iOne.Master.Permissions;

public static class ResWardPermissions
{
    public const string GroupName = "MasterResWard";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

**Đặc điểm**:
- `GroupName` = `"MasterResWard"` (không có dấu chấm)
- `Default` = `GroupName` (không phải `"{GroupName}.Default"`)
- Frontend sử dụng `GroupName` để check permission

---

### 4.2. Permission Definition Provider

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResWardPermissionDefinitionProvider.cs`

```csharp
using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResWardPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resWardGroup = context.AddGroup(
            ResWardPermissions.GroupName,
            L("Permission:ResWard")
        );

        var resWardPermission = resWardGroup.AddPermission(
            ResWardPermissions.Default,
            L("Permission:ResWard")
        );

        resWardPermission.AddChild(
            ResWardPermissions.Create,
            L("Permission:Create")
        );

        resWardPermission.AddChild(
            ResWardPermissions.Edit,
            L("Permission:Edit")
        );

        resWardPermission.AddChild(
            ResWardPermissions.Delete,
            L("Permission:Delete")
        );

        resWardPermission.AddChild(
            ResWardPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
```

---

## 5. Entity Framework Core

### 5.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResWards/ResWardConfiguration.cs`

```csharp
using System;
using iOne.ResProvinces;
using iOne.ResWards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResWards;

public class ResWardConfiguration : IEntityTypeConfiguration<ResWard>
{
    public void Configure(EntityTypeBuilder<ResWard> builder)
    {
        builder.ToTable("res_ward", t =>
        {
            t.HasComment("Phường/Xã");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProvinceId)
            .HasColumnName("province_id")
            .IsRequired();

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
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResWardStatus>(v, true)
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        // TenantId is not configured here as ResProvinceConfiguration also doesn't have it.

        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(e => e.Code, "ix_res_ward_code")
            .IsUnique();

        builder.HasIndex(e => e.ProvinceId, "ix_res_ward_province_id");
    }
}
```

**Đặc điểm**:
- Table name: `res_ward` (snake_case)
- Column names: snake_case (bao gồm audit columns)
- Status enum → string conversion (lowercase)
- Foreign key: `province_id` → `res_province.id` (on delete restrict)
- Indexes: unique trên `code`, index trên `province_id`

---

### 5.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResWards/EfCoreResWardRepository.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResWards;

namespace iOne.EntityFrameworkCore.ResWards;

public class EfCoreResWardRepository : EfCoreRepository<iOneDbContext, ResWard, Guid>, IResWardRepository
{
    public EfCoreResWardRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public async Task<ResWard?> FindByCodeAsync(string code)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code);
    }
}
```

---

### 5.3. Register Repository và Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`

**Cần thêm**:
```csharp
using iOne.ResWards; // Added
using iOne.EntityFrameworkCore.ResWards; // Added

public class iOneDbContext : ...
{
    public DbSet<ResWard> ResWards { get; set; } // Added

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // ... existing configurations ...
        builder.ApplyConfiguration(new ResWardConfiguration()); // Added
    }
}
```

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneEntityFrameworkCoreModule.cs`

**Cần thêm**:
```csharp
using iOne.ResWards; // Added
using iOne.EntityFrameworkCore.ResWards; // Added

public override void ConfigureServices(ServiceConfigurationContext context)
{
    context.Services.AddAbpDbContext<iOneDbContext>(options =>
    {
        options.AddDefaultRepositories(includeAllEntities: true);
        // ... existing custom repositories ...
        options.AddRepository<ResWard, EfCoreResWardRepository>(); // Added
    });
}
```

---

### 5.4. Database Migration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/Migrations/{Timestamp}_AddResWard.cs`

**Cần tạo migration**:
```csharp
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    public partial class AddResWard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP (nếu table đã tồn tại)
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_ward"";");

            migrationBuilder.CreateTable(
                name: "res_ward",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    province_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động"),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("pk_res_ward", x => x.id);
                    // ✅ ĐÚNG: Foreign key name theo snake_case với prefix
                    table.ForeignKey(
                        name: "fk_res_ward_province_id",
                        column: x => x.province_id,
                        principalTable: "res_province",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Phường/Xã");

            // ✅ ĐÚNG: Index name theo snake_case với prefix
            migrationBuilder.CreateIndex(
                name: "ix_res_ward_code",
                table: "res_ward",
                column: "code",
                unique: true);

            // ✅ ĐÚNG: Index trên foreign key để tối ưu query performance
            migrationBuilder.CreateIndex(
                name: "ix_res_ward_province_id",
                table: "res_ward",
                column: "province_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_ward_province_id"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_ward_code"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_ward"";");
        }
    }
}
```

**Đặc điểm**:
- Table name: `res_ward` (snake_case)
- Column names: snake_case
- Primary key: `pk_res_ward`
- Foreign key: `fk_res_ward_province_id` → `res_province.id`
- Indexes: `ix_res_ward_code` (unique), `ix_res_ward_province_id`
- Sử dụng `IF EXISTS` trong Down method

---

## 6. HTTP API Controllers

### 6.1. Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/Controllers/ResWardController.cs`

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResWards;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/wards")]
[Authorize]
public class ResWardController : AbpControllerBase
{
    protected IResWardAppService AppService { get; }

    public ResWardController(IResWardAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResWardPermissions.View)]
    public virtual Task<PagedResultDto<ResWardDto>> GetListAsync(GetResWardsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResWardPermissions.View)]
    public virtual Task<ResWardDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResWardPermissions.Create)]
    public virtual Task<ResWardDto> CreateAsync(CreateResWardDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResWardPermissions.Edit)]
    public virtual Task<ResWardDto> UpdateAsync(Guid id, UpdateResWardDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResWardPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
```

**Đặc điểm**:
- Route: `api/master/wards`
- Có đầy đủ CRUD endpoints với authorization

---

### 6.2. Exclude từ Conventional Controllers

**Location**: `modules/master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`

**Cần thêm**:
```csharp
Configure<AbpAspNetCoreMvcOptions>(options =>
{
    options.ConventionalControllers.Create(
        typeof(iOneMasterApplicationModule).Assembly,
        opts =>
        {
            opts.TypePredicate = type =>
                type.Name != "ResCountryAppService" &&
                type.Name != "ResProvinceAppService" &&
                type.Name != "ResWardAppService"; // Exclude ResWardAppService
        });
});
```

---

## 7. Localization

### 7.1. Vietnamese (vi-VN.json)

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/vi-VN.json`

**Cần thêm**:
```json
{
  "Menu:ResWard": "Danh mục Phường/Xã",
  "Permission:ResWard": "Phường/Xã",
  "ResWard:Province": "Tỉnh/Thành",
  "ResWard:ProvinceName": "Tên tỉnh/thành",
  "ResWard:Code": "Mã phường/xã",
  "ResWard:Name": "Tên phường/xã",
  "ResWard:Status": "Trạng thái",
  "ResWard:Description": "Mô tả",
  "ResWard:ProvinceRequired": "Tỉnh/thành là bắt buộc",
  "ResWard:CodeRequired": "Mã phường/xã là bắt buộc",
  "ResWard:CodeMaxLength": "Mã phường/xã không được vượt quá 25 ký tự",
  "ResWard:CodeInvalid": "Mã phường/xã chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
  "ResWard:NameRequired": "Tên phường/xã là bắt buộc",
  "ResWard:NameMaxLength": "Tên phường/xã không được vượt quá 250 ký tự",
  "ResWard:StatusRequired": "Trạng thái là bắt buộc",
  "ResWard:DescriptionMaxLength": "Mô tả không được vượt quá 500 ký tự",
  "ResWard:CodeExists": "Mã phường/xã '{Code}' đã tồn tại",
  "ResWard:ProvinceNotFound": "Tỉnh/thành không tồn tại",
  "ResWard:CreatedSuccessfully": "Tạo phường/xã thành công",
  "ResWard:UpdatedSuccessfully": "Cập nhật phường/xã thành công",
  "ResWard:DeletedSuccessfully": "Xóa phường/xã thành công",
  "ResWard:New": "Thêm mới phường/xã",
  "ResWard:Edit": "Sửa phường/xã",
  "ResWard:Delete": "Xóa phường/xã",
  "ResWard:DeleteConfirm": "Bạn có chắc chắn muốn xóa phường/xã này?",
  "ResWard:CodeCannotBeChanged": "Mã phường/xã không được phép thay đổi",
  "ResWard:Active": "Hoạt động",
  "ResWard:Deactive": "Không hoạt động",
  "ResWard:SearchByProvince": "Tìm theo tỉnh/thành",
  "ResWard:SearchByCode": "Tìm theo mã phường/xã",
  "ResWard:SearchByName": "Tìm theo tên phường/xã"
}
```

---

### 7.2. English (en.json)

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/en.json`

**Cần thêm**:
```json
{
  "Menu:ResWard": "Ward Master",
  "Permission:ResWard": "Ward",
  "ResWard:Province": "Province",
  "ResWard:ProvinceName": "Province Name",
  "ResWard:Code": "Ward Code",
  "ResWard:Name": "Ward Name",
  "ResWard:Status": "Status",
  "ResWard:Description": "Description",
  "ResWard:ProvinceRequired": "Province is required",
  "ResWard:CodeRequired": "Ward code is required",
  "ResWard:CodeMaxLength": "Ward code cannot exceed 25 characters",
  "ResWard:CodeInvalid": "Ward code can only contain letters (A-Z), numbers (0-9) and underscore (_)",
  "ResWard:NameRequired": "Ward name is required",
  "ResWard:NameMaxLength": "Ward name cannot exceed 250 characters",
  "ResWard:StatusRequired": "Status is required",
  "ResWard:DescriptionMaxLength": "Description cannot exceed 500 characters",
  "ResWard:CodeExists": "Ward code '{Code}' already exists",
  "ResWard:ProvinceNotFound": "Province not found",
  "ResWard:CreatedSuccessfully": "Ward created successfully",
  "ResWard:UpdatedSuccessfully": "Ward updated successfully",
  "ResWard:DeletedSuccessfully": "Ward deleted successfully",
  "ResWard:New": "New Ward",
  "ResWard:Edit": "Edit Ward",
  "ResWard:Delete": "Delete Ward",
  "ResWard:DeleteConfirm": "Are you sure you want to delete this ward?",
  "ResWard:CodeCannotBeChanged": "Ward code cannot be changed",
  "ResWard:Active": "Active",
  "ResWard:Deactive": "Deactive",
  "ResWard:SearchByProvince": "Search by province",
  "ResWard:SearchByCode": "Search by ward code",
  "ResWard:SearchByName": "Search by ward name"
}
```

---

## 8. Menu Configuration

### 8.1. Menu Contributor

**Location**: `modules/master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

**Cần thêm**:
```csharp
masterMenuItem.AddItem(new ApplicationMenuItem(
    "Master.ResWard",
    masterL["Menu:ResWard"],
    url: "~/pages/master/wards",
    icon: "pi pi-fw pi-map-marker"
).RequirePermissions(ResWardPermissions.Default));
```

---

## 9. Checklist Implementation

### 9.1. Domain Layer
- [ ] Enum `ResWardStatus` (Active, Deactive)
- [ ] Entity `ResWard` với private setters và validation
- [ ] Repository interface `IResWardRepository` với `IsCodeExistsAsync`, `FindByCodeAsync`
- [ ] Domain service `ResWardManager` với validation logic

### 9.2. Application Layer
- [ ] DTOs: `ResWardDto`, `CreateResWardDto`, `UpdateResWardDto`, `GetResWardsInput`
- [ ] Application service interface `IResWardAppService`
- [ ] Application service implementation `ResWardAppService` với:
  - [ ] Override `GetListAsync` để load `ProvinceName` (tối ưu với Include)
  - [ ] Override `DeleteAsync` để: Delete trước → Set Status = Deactive sau
- [ ] AutoMapper configuration

### 9.3. Permissions
- [ ] Permission constants `ResWardPermissions`
- [ ] Permission definition provider `ResWardPermissionDefinitionProvider`

### 9.4. Entity Framework Core
- [ ] Entity configuration `ResWardConfiguration` (snake_case naming)
- [ ] Repository implementation `EfCoreResWardRepository`
- [ ] Register trong `iOneDbContext` và `iOneEntityFrameworkCoreModule`
- [ ] Migration script với IF EXISTS/IF NOT EXISTS

### 9.5. HTTP API
- [ ] Controller `ResWardController` với CRUD endpoints
- [ ] Exclude `ResWardAppService` khỏi conventional controllers

### 9.6. Localization
- [ ] Vietnamese keys (vi-VN.json)
- [ ] English keys (en.json)

### 9.7. Menu
- [ ] Menu item trong `MasterMenuContributor`

### 9.8. Testing
- [ ] Build solution thành công
- [ ] Run migration thành công
- [ ] Test API endpoints
- [ ] Test permissions
- [ ] Test localization

---

## 10. Lưu Ý Quan Trọng

1. **Code Immutability**: Code không được phép sửa → `UpdateResWardDto` không có `Code` property, UI disable field này khi edit
2. **Soft Delete**: Delete trước để trigger audit log, sau đó set Status = Deactive
3. **Foreign Key**: `ProvinceId` → `res_province.id` (on delete restrict)
4. **Status Storage**: Lưu trong DB dưới dạng `"active"`/`"deactive"` (lowercase), không phải enum values
5. **Naming Convention**: Tất cả table/column/index names phải theo snake_case (PostgreSQL convention)
6. **Performance**: Sử dụng Include để load Province navigation property, tránh N+1 queries trong GetListAsync
7. **Validation**: Code chỉ cho phép A-Z, 0-9, _ (uppercase)
8. **Uniqueness**: Code phải unique (có unique index)

---

## 11. So Sánh với ResProvince

| Aspect | ResProvince | ResWard |
|--------|-------------|---------|
| Foreign Key | `CountryId` → `res_country.id` | `ProvinceId` → `res_province.id` |
| Navigation Property | `Country` | `Province` |
| Display Name in DTO | `CountryName` | `ProvinceName` |
| Table Name | `res_province` | `res_ward` |
| Code Validation | A-Z, 0-9, _ | A-Z, 0-9, _ |
| Code Immutability | ✅ | ✅ |
| Soft Delete | ✅ | ✅ |

**Tương tự**: Cấu trúc và logic giống ResProvince, chỉ khác foreign key và navigation property.

---

**END OF PLAN**

