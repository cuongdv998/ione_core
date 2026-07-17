# Plan: Backend Danh mục Kênh phân phối (ResChannel) - Module Partner

## 1. Tổng Quan

**Entity Name**: `ResChannel`  
**Module**: `Partner`  
**Table Name**: `res_channel` (snake_case - PostgreSQL convention)  
**Namespace**: `iOne.ResChannels`

**Chức năng**: Danh mục Kênh phân phối (Định nghĩa kênh khai thác)
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
- **Description**: Optional field, max length 500
- **Không có Foreign Key** - Entity độc lập

**Lưu ý Database**:
- Table name trong DDL: `RESCHANNEL` → sẽ đổi thành `res_channel` (snake_case)
- Column names trong DDL: UPPERCASE → sẽ đổi thành snake_case
- ID và audit IDs: UUID type (PostgreSQL) - EF Core tự động map `Guid` sang `UUID`
- **QUAN TRỌNG**: DDL có `CREATIONTIME DATE` và `CREATORID CHAR(36)` - cần chuyển sang:
  - `creation_time`: `timestamp without time zone` (DateTime)
  - `creator_id`: `uuid` (Guid)
  - Tương tự cho các audit columns khác
- **QUAN TRỌNG**: DDL có `CODE VARCHAR(50)` và `NAME VARCHAR(50)` - cần giữ nguyên max length

---

## 2. Domain Layer

### 2.1. Enum: ResChannelStatus

**Location**: `src/common/domain/iOne.Domain.Shared/ResChannels/ResChannelStatus.cs`

```csharp
namespace iOne.ResChannels;

public enum ResChannelStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

**Đặc điểm**:
- Enum values trong C#: `Active = 0`, `Deactive = 1` (PascalCase)
- **QUAN TRỌNG**: Trong database lưu dưới dạng string `"active"`, `"deactive"` (lowercase, không phải "Active" hay "Deactive")
- Sử dụng `HasConversion<string>()` với `ToLowerInvariant()` trong EF Core configuration để tự động convert
- **LƯU Ý**: Khi đọc từ database, EF Core sẽ parse `"active"` → `Active`, `"deactive"` → `Deactive` (case-insensitive)

---

### 2.2. Entity: ResChannel

**Location**: `src/common/domain/iOne.Domain/ResChannels/ResChannel.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResChannels;

[Table("res_channel")] // ✅ snake_case (PostgreSQL convention)
public class ResChannel : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(50)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResChannelStatus Status { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    protected ResChannel()
    {
        // For ORM
    }

    public ResChannel(
        Guid id,
        string code,
        string name,
        ResChannelStatus status,
        string? description = null)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetDescription(description);
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }

        if (code.Length > 50)
        {
            throw new ArgumentException("Code cannot exceed 50 characters.", nameof(code));
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

        if (name.Length > 50)
        {
            throw new ArgumentException("Name cannot exceed 50 characters.", nameof(name));
        }

        Name = name;
    }

    private void SetStatus(ResChannelStatus status)
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
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResChannelStatus status)
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
- **Không có navigation property** - Entity độc lập

---

### 2.3. Repository Interface: IResChannelRepository

**Location**: `src/common/domain/iOne.Domain/ResChannels/IResChannelRepository.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResChannels;

public interface IResChannelRepository : IRepository<ResChannel, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<ResChannel?> FindByCodeAsync(string code);
}
```

**Methods**:
- `IsCodeExistsAsync`: Check code uniqueness (có thể exclude một ID khi update)
- `FindByCodeAsync`: Tìm channel theo code

---

### 2.4. Domain Service: ResChannelManager

**Location**: `src/common/domain/iOne.Domain/ResChannels/ResChannelManager.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResChannels;

public class ResChannelManager : DomainService
{
    protected IResChannelRepository Repository { get; }

    public ResChannelManager(IResChannelRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResChannel channel)
    {
        // Check code uniqueness
        if (await Repository.FindByCodeAsync(channel.Code) != null)
        {
            throw new BusinessException("Partner:ResChannel:CodeExists")
                .WithData("Code", channel.Code);
        }

        await Repository.InsertAsync(channel);
    }

    public virtual async Task UpdateAsync(
        ResChannel channel,
        string name,
        ResChannelStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        channel.UpdateName(name);
        channel.UpdateStatus(status);
        channel.UpdateDescription(description);
        await Repository.UpdateAsync(channel);
    }
}
```

**Đặc điểm**:
- Validate `Code` uniqueness
- **QUAN TRỌNG**: `UpdateAsync` không có parameter `code` → Code không được phép sửa
- **Không có foreign key validation** - Entity độc lập

---

## 3. Application Layer

### 3.1. DTOs

#### 3.1.1. ResChannelDto

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResChannels/ResChannelDto.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResChannels;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResChannels;

public class ResChannelDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Partner::ResChannel:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Partner::ResChannel:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Partner::ResChannel:Status")]
    public ResChannelStatus Status { get; set; }

    [Display(Name = "Partner::ResChannel:Description")]
    public string? Description { get; set; }
}
```

**Đặc điểm**:
- Kế thừa từ `FullAuditedEntityDto<Guid>` (có đầy đủ audit fields)
- Không có navigation property (entity độc lập)

---

#### 3.1.2. CreateResChannelDto

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResChannels/CreateResChannelDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.ResChannels;

namespace iOne.Partner.ResChannels;

public class CreateResChannelDto
{
    [Required(ErrorMessage = "Partner::ResChannel:CodeRequired")]
    [StringLength(50, ErrorMessage = "Partner::ResChannel:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Partner::ResChannel:CodeInvalid")]
    [Display(Name = "Partner::ResChannel:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResChannel:NameRequired")]
    [StringLength(50, ErrorMessage = "Partner::ResChannel:NameMaxLength")]
    [Display(Name = "Partner::ResChannel:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResChannel:StatusRequired")]
    [Display(Name = "Partner::ResChannel:Status")]
    public ResChannelStatus Status { get; set; } = ResChannelStatus.Active;

    [StringLength(500, ErrorMessage = "Partner::ResChannel:DescriptionMaxLength")]
    [Display(Name = "Partner::ResChannel:Description")]
    public string? Description { get; set; }
}
```

**Đặc điểm**:
- Validation attributes đầy đủ
- `Code` có `RegularExpression` để validate format: `^[A-Z0-9_]+$`
- `Status` default = `Active`
- Max length: Code = 50, Name = 50, Description = 500

---

#### 3.1.3. UpdateResChannelDto

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResChannels/UpdateResChannelDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.ResChannels;

namespace iOne.Partner.ResChannels;

public class UpdateResChannelDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "Partner::ResChannel:NameRequired")]
    [StringLength(50, ErrorMessage = "Partner::ResChannel:NameMaxLength")]
    [Display(Name = "Partner::ResChannel:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Partner::ResChannel:StatusRequired")]
    [Display(Name = "Partner::ResChannel:Status")]
    public ResChannelStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "Partner::ResChannel:DescriptionMaxLength")]
    [Display(Name = "Partner::ResChannel:Description")]
    public string? Description { get; set; }
}
```

**Đặc điểm**:
- **KHÔNG có** `Code` property → Code không được phép sửa
- Chỉ có các fields có thể update: `Name`, `Status`, `Description`

---

#### 3.1.4. GetResChannelsInput

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResChannels/GetResChannelsInput.cs`

```csharp
using iOne.ResChannels;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResChannels;

public class GetResChannelsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResChannelStatus? Status { get; set; }
}
```

**Đặc điểm**:
- Kế thừa từ `PagedAndSortedResultRequestDto` (có pagination và sorting)
- Filter theo: `Code`, `Name`, `Status`
- **Không có foreign key filter** - Entity độc lập

---

### 3.2. Application Service Interface

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResChannels/IResChannelAppService.cs`

```csharp
using System;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResChannels;

public interface IResChannelAppService : ICrudAppService<
    ResChannelDto,
    Guid,
    GetResChannelsInput,
    CreateResChannelDto,
    UpdateResChannelDto>
{
}
```

**Đặc điểm**:
- Kế thừa từ `ICrudAppService` → có sẵn CRUD operations
- Generic types: `ResChannelDto`, `Guid`, `GetResChannelsInput`, `CreateResChannelDto`, `UpdateResChannelDto`

---

### 3.3. Application Service Implementation

**Location**: `modules/partner/src/iOne.Partner.Application/ResChannels/ResChannelAppService.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Partner.Localization;
using iOne.Partner.Permissions;
using iOne.Partner.ResChannels;
using iOne.ResChannels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResChannels;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResChannelPermissions.Default)]
public class ResChannelAppService : CrudAppService<
    ResChannel,
    ResChannelDto,
    Guid,
    GetResChannelsInput,
    CreateResChannelDto,
    UpdateResChannelDto>,
    IResChannelAppService
{
    protected ResChannelManager Manager { get; }

    public ResChannelAppService(
        IResChannelRepository repository,
        ResChannelManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(PartnerResource);
        GetPolicyName = ResChannelPermissions.View;
        GetListPolicyName = ResChannelPermissions.View;
        CreatePolicyName = ResChannelPermissions.Create;
        UpdatePolicyName = ResChannelPermissions.Edit;
        DeletePolicyName = ResChannelPermissions.Delete;
    }

    public override async Task<ResChannelDto> CreateAsync(CreateResChannelDto input)
    {
        var entity = new ResChannel(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResChannel, ResChannelDto>(entity);
    }

    public override async Task<ResChannelDto> UpdateAsync(Guid id, UpdateResChannelDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name, Status, Description - không update Code
        await Manager.UpdateAsync(
            entity,
            input.Name,
            input.Status,
            input.Description
        );

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResChannel, ResChannelDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResChannelStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResChannel>> CreateFilteredQueryAsync(GetResChannelsInput input)
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

**Đặc điểm**:
- Override `DeleteAsync` để: Delete trước → Set Status = Deactive sau
- `CreateFilteredQueryAsync` filter theo Code, Name, Status
- **Không có Include navigation property** - Entity độc lập

---

### 3.4. AutoMapper Configuration

**Location**: `modules/partner/src/iOne.Partner.Application/iOnePartnerApplicationAutoMapperProfile.cs`

**Cần thêm**:
```csharp
// ResChannel mappings
CreateMap<ResChannel, ResChannelDto>();
CreateMap<CreateResChannelDto, ResChannel>();
CreateMap<UpdateResChannelDto, ResChannel>();
```

---

## 4. Permissions

### 4.1. Permission Constants

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Permissions/ResChannelPermissions.cs`

```csharp
namespace iOne.Partner.Permissions;

public static class ResChannelPermissions
{
    public const string GroupName = "PartnerResChannel";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

**Đặc điểm**:
- `GroupName` = `"PartnerResChannel"` (không có dấu chấm)
- `Default` = `GroupName` (không phải `"{GroupName}.Default"`)
- Frontend sử dụng `GroupName` để check permission

---

### 4.2. Permission Definition Provider

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Permissions/ResChannelPermissionDefinitionProvider.cs`

```csharp
using iOne.Partner.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Partner.Permissions;

public class ResChannelPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resChannelGroup = context.AddGroup(
            ResChannelPermissions.GroupName,
            L("Permission:ResChannel")
        );

        var resChannelPermission = resChannelGroup.AddPermission(
            ResChannelPermissions.Default,
            L("Permission:ResChannel")
        );

        resChannelPermission.AddChild(
            ResChannelPermissions.Create,
            L("Permission:Create")
        );

        resChannelPermission.AddChild(
            ResChannelPermissions.Edit,
            L("Permission:Edit")
        );

        resChannelPermission.AddChild(
            ResChannelPermissions.Delete,
            L("Permission:Delete")
        );

        resChannelPermission.AddChild(
            ResChannelPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PartnerResource>(name);
    }
}
```

---

## 5. Entity Framework Core

### 5.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResChannels/ResChannelConfiguration.cs`

```csharp
using System;
using iOne.ResChannels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResChannels;

public class ResChannelConfiguration : IEntityTypeConfiguration<ResChannel>
{
    public void Configure(EntityTypeBuilder<ResChannel> builder)
    {
        builder.ToTable("res_channel", t =>
        {
            t.HasComment("Định nghĩa kênh khai thác");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        // ✅ Status: Enum to string conversion (lowercase)
        // QUAN TRỌNG: Enum Active/Deactive → DB lưu "active"/"deactive" (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ResChannelStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive (case-insensitive)
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");

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
        // TenantId is not configured here as ResPartnerTypeConfiguration also doesn't have it.

        builder.HasIndex(e => e.Code, "ix_res_channel_code")
            .IsUnique();
    }
}
```

**Đặc điểm**:
- Table name: `res_channel` (snake_case)
- Column names: snake_case (bao gồm audit columns)
- Status enum → string conversion (lowercase)
- **Không có foreign key** - Entity độc lập
- Index: unique trên `code`
- Max length: Code = 50, Name = 50, Description = 500

---

### 5.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResChannels/EfCoreResChannelRepository.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResChannels;

namespace iOne.EntityFrameworkCore.ResChannels;

public class EfCoreResChannelRepository : EfCoreRepository<iOneDbContext, ResChannel, Guid>, IResChannelRepository
{
    public EfCoreResChannelRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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

    public async Task<ResChannel?> FindByCodeAsync(string code)
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
using iOne.ResChannels; // Added
using iOne.EntityFrameworkCore.ResChannels; // Added

public class iOneDbContext : ...
{
    public DbSet<ResChannel> ResChannels { get; set; } // Added

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // ... existing configurations ...
        builder.ApplyConfiguration(new ResChannelConfiguration()); // Added
    }
}
```

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneEntityFrameworkCoreModule.cs`

**Cần thêm**:
```csharp
using iOne.ResChannels; // Added
using iOne.EntityFrameworkCore.ResChannels; // Added

public override void ConfigureServices(ServiceConfigurationContext context)
{
    context.Services.AddAbpDbContext<iOneDbContext>(options =>
    {
        options.AddDefaultRepositories(includeAllEntities: true);
        // ... existing custom repositories ...
        options.AddRepository<ResChannel, EfCoreResChannelRepository>(); // Added
    });
}
```

---

### 5.4. Database Migration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/Migrations/{Timestamp}_AddResChannel.cs`

**Cần tạo migration**:
```csharp
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iOne.Migrations
{
    public partial class AddResChannel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP (nếu table đã tồn tại)
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_channel"";");

            migrationBuilder.CreateTable(
                name: "res_channel",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động"),
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
                    table.PrimaryKey("pk_res_channel", x => x.id);
                },
                comment: "Định nghĩa kênh khai thác");

            // ✅ ĐÚNG: Index name theo snake_case với prefix
            migrationBuilder.CreateIndex(
                name: "ix_res_channel_code",
                table: "res_channel",
                column: "code",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_channel_code"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_channel"";");
        }
    }
}
```

**Đặc điểm**:
- Table name: `res_channel` (snake_case)
- Column names: snake_case
- Primary key: `pk_res_channel`
- **Không có foreign key** - Entity độc lập
- Index: `ix_res_channel_code` (unique)
- Sử dụng `IF EXISTS` trong Down method
- Max length: Code = 50, Name = 50, Description = 500

---

## 6. HTTP API Controllers

### 6.1. Controller

**Location**: `modules/partner/src/iOne.Partner.HttpApi/Controllers/ResChannelController.cs`

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Partner.Permissions;
using iOne.Partner.ResChannels;

namespace iOne.Partner.Controllers;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Area(PartnerRemoteServiceConsts.ModuleName)]
[Route("api/partner/channels")]
[Authorize]
public class ResChannelController : AbpControllerBase
{
    protected IResChannelAppService AppService { get; }

    public ResChannelController(IResChannelAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResChannelPermissions.View)]
    public virtual Task<PagedResultDto<ResChannelDto>> GetListAsync(GetResChannelsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResChannelPermissions.View)]
    public virtual Task<ResChannelDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResChannelPermissions.Create)]
    public virtual Task<ResChannelDto> CreateAsync(CreateResChannelDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResChannelPermissions.Edit)]
    public virtual Task<ResChannelDto> UpdateAsync(Guid id, UpdateResChannelDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResChannelPermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
```

**Đặc điểm**:
- Route: `api/partner/channels`
- Có đầy đủ CRUD endpoints với authorization

---

### 6.2. Exclude từ Conventional Controllers

**Location**: `modules/partner/src/iOne.Partner.HttpApi/iOnePartnerHttpApiModule.cs`

**Cần thêm**:
```csharp
Configure<AbpAspNetCoreMvcOptions>(options =>
{
    options.ConventionalControllers.Create(
        typeof(iOnePartnerApplicationModule).Assembly,
        opts =>
        {
            opts.TypePredicate = type =>
                type.Name != "ResPartnerTypeAppService" &&
                type.Name != "ResOrganizationTypeAppService" &&
                type.Name != "ResChannelAppService"; // Exclude ResChannelAppService
        });
});
```

---

## 7. Localization

### 7.1. Vietnamese (vi-VN.json)

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Localization/Partner/vi-VN.json`

**Cần thêm**:
```json
{
  "Menu:ResChannel": "Danh mục Kênh phân phối",
  "Permission:ResChannel": "Kênh phân phối",
  "ResChannel:Code": "Mã kênh phân phối",
  "ResChannel:Name": "Tên kênh phân phối",
  "ResChannel:Status": "Trạng thái",
  "ResChannel:Description": "Mô tả",
  "ResChannel:CodeRequired": "Mã kênh phân phối là bắt buộc",
  "ResChannel:CodeMaxLength": "Mã kênh phân phối không được vượt quá 50 ký tự",
  "ResChannel:CodeInvalid": "Mã kênh phân phối chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
  "ResChannel:NameRequired": "Tên kênh phân phối là bắt buộc",
  "ResChannel:NameMaxLength": "Tên kênh phân phối không được vượt quá 50 ký tự",
  "ResChannel:StatusRequired": "Trạng thái là bắt buộc",
  "ResChannel:DescriptionMaxLength": "Mô tả không được vượt quá 500 ký tự",
  "ResChannel:CodeExists": "Mã kênh phân phối '{Code}' đã tồn tại",
  "ResChannel:CreatedSuccessfully": "Tạo kênh phân phối thành công",
  "ResChannel:UpdatedSuccessfully": "Cập nhật kênh phân phối thành công",
  "ResChannel:DeletedSuccessfully": "Xóa kênh phân phối thành công",
  "ResChannel:New": "Thêm mới kênh phân phối",
  "ResChannel:Edit": "Sửa kênh phân phối",
  "ResChannel:Delete": "Xóa kênh phân phối",
  "ResChannel:DeleteConfirm": "Bạn có chắc chắn muốn xóa kênh phân phối này?",
  "ResChannel:CodeCannotBeChanged": "Mã kênh phân phối không được phép thay đổi",
  "ResChannel:Active": "Hoạt động",
  "ResChannel:Deactive": "Không hoạt động",
  "ResChannel:SearchByCode": "Tìm theo mã kênh phân phối",
  "ResChannel:SearchByName": "Tìm theo tên kênh phân phối"
}
```

---

### 7.2. English (en.json)

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Localization/Partner/en.json`

**Cần thêm**:
```json
{
  "Menu:ResChannel": "Channel Master",
  "Permission:ResChannel": "Channel",
  "ResChannel:Code": "Channel Code",
  "ResChannel:Name": "Channel Name",
  "ResChannel:Status": "Status",
  "ResChannel:Description": "Description",
  "ResChannel:CodeRequired": "Channel code is required",
  "ResChannel:CodeMaxLength": "Channel code cannot exceed 50 characters",
  "ResChannel:CodeInvalid": "Channel code can only contain letters (A-Z), numbers (0-9) and underscore (_)",
  "ResChannel:NameRequired": "Channel name is required",
  "ResChannel:NameMaxLength": "Channel name cannot exceed 50 characters",
  "ResChannel:StatusRequired": "Status is required",
  "ResChannel:DescriptionMaxLength": "Description cannot exceed 500 characters",
  "ResChannel:CodeExists": "Channel code '{Code}' already exists",
  "ResChannel:CreatedSuccessfully": "Channel created successfully",
  "ResChannel:UpdatedSuccessfully": "Channel updated successfully",
  "ResChannel:DeletedSuccessfully": "Channel deleted successfully",
  "ResChannel:New": "New Channel",
  "ResChannel:Edit": "Edit Channel",
  "ResChannel:Delete": "Delete Channel",
  "ResChannel:DeleteConfirm": "Are you sure you want to delete this channel?",
  "ResChannel:CodeCannotBeChanged": "Channel code cannot be changed",
  "ResChannel:Active": "Active",
  "ResChannel:Deactive": "Deactive",
  "ResChannel:SearchByCode": "Search by channel code",
  "ResChannel:SearchByName": "Search by channel name"
}
```

---

## 8. Menu Configuration

### 8.1. Menu Contributor

**Location**: `modules/partner/src/iOne.Partner.Application/Navigation/PartnerMenuContributor.cs`

**Cần thêm**:
```csharp
partnerMenuItem.AddItem(new ApplicationMenuItem(
    "Partner.ResChannel",
    partnerL["Menu:ResChannel"],
    url: "~/pages/partner/channels",
    icon: "pi pi-fw pi-sitemap"
).RequirePermissions(ResChannelPermissions.Default));
```

---

## 9. Checklist Implementation

### 9.1. Domain Layer
- [ ] Enum `ResChannelStatus` (Active, Deactive)
- [ ] Entity `ResChannel` với private setters và validation
- [ ] Repository interface `IResChannelRepository` với `IsCodeExistsAsync`, `FindByCodeAsync`
- [ ] Domain service `ResChannelManager` với validation logic

### 9.2. Application Layer
- [ ] DTOs: `ResChannelDto`, `CreateResChannelDto`, `UpdateResChannelDto` (không có Code), `GetResChannelsInput`
- [ ] Application service interface `IResChannelAppService`
- [ ] Application service implementation `ResChannelAppService` với:
  - [ ] Override `DeleteAsync` để: Delete trước → Set Status = Deactive sau
- [ ] AutoMapper configuration

### 9.3. Permissions
- [ ] Permission constants `ResChannelPermissions`
- [ ] Permission definition provider `ResChannelPermissionDefinitionProvider`

### 9.4. Entity Framework Core
- [ ] Entity configuration `ResChannelConfiguration` (snake_case naming)
- [ ] Repository implementation `EfCoreResChannelRepository`
- [ ] Register trong `iOneDbContext` và `iOneEntityFrameworkCoreModule`
- [ ] Migration script với IF EXISTS/IF NOT EXISTS

### 9.5. HTTP API
- [ ] Controller `ResChannelController` với CRUD endpoints
- [ ] Exclude `ResChannelAppService` khỏi conventional controllers

### 9.6. Localization
- [ ] Vietnamese keys (vi-VN.json)
- [ ] English keys (en.json)

### 9.7. Menu
- [ ] Menu item trong `PartnerMenuContributor`

### 9.8. Testing
- [ ] Build solution thành công
- [ ] Run migration thành công
- [ ] Test API endpoints
- [ ] Test permissions
- [ ] Test localization

---

## 10. Lưu Ý Quan Trọng

1. **Code Immutability**: Code không được phép sửa → `UpdateResChannelDto` không có `Code` property, UI disable field này khi edit
2. **Soft Delete**: Delete trước để trigger audit log, sau đó set Status = Deactive
3. **No Foreign Key**: Entity độc lập, không có foreign key
4. **Status Storage**: 
   - **QUAN TRỌNG**: Lưu trong DB dưới dạng `"active"`/`"deactive"` (lowercase), không phải `"Active"`/`"Deactive"` (PascalCase)
   - EF Core configuration sử dụng `ToLowerInvariant()` để convert enum → lowercase string
   - Khi đọc từ DB, EF Core parse case-insensitive: `"active"` → `Active`, `"deactive"` → `Deactive`
5. **Naming Convention**: Tất cả table/column/index names phải theo snake_case (PostgreSQL convention)
6. **Max Length**: Code = 50, Name = 50, Description = 500 (theo DDL)
7. **Validation**: Code chỉ cho phép A-Z, 0-9, _ (uppercase)
8. **Uniqueness**: Code phải unique (có unique index)

---

## 11. So Sánh với ResPartnerType

| Aspect | ResPartnerType | ResChannel |
|--------|----------------|------------|
| Foreign Key | ❌ Không có | ❌ Không có |
| Navigation Property | ❌ Không có | ❌ Không có |
| Description Field | ❌ Không có | ✅ Có (max 500) |
| Code Max Length | 25 | 50 |
| Name Max Length | 250 | 50 |
| Table Name | `res_partner_type` | `res_channel` |
| Code Validation | A-Z, 0-9, _ | A-Z, 0-9, _ |
| Code Immutability | ✅ | ✅ |
| Soft Delete | ✅ | ✅ |

**Tương tự**: Cấu trúc giống ResPartnerType, nhưng có thêm Description field và max length khác (Code/Name = 50 thay vì 25/250).

---

**END OF PLAN**

