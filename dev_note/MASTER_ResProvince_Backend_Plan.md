# Plan: Backend Danh mục Tỉnh/Thành (ResProvince) - Module Master

## 1. Tổng Quan

**Entity Name**: `ResProvince`  
**Module**: `Master`  
**Table Name**: `res_province` (snake_case - PostgreSQL convention)  
**Namespace**: `iOne.ResProvinces`

**Chức năng**: Danh mục Tỉnh/Thành
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
- **Foreign Key**: `CountryId` (Guid) → `res_country.id` (on delete restrict)
- **Description**: Optional field, max length 500

**Lưu ý Database**:
- Table name trong DDL: `RESPROVINCE` → sẽ đổi thành `res_province` (snake_case)
- Column names trong DDL: UPPERCASE → sẽ đổi thành snake_case
- ID và audit IDs: UUID type (PostgreSQL) - EF Core tự động map `Guid` sang `UUID`
- Foreign key: `country_id` (snake_case) → `res_country.id`

---

## 2. Domain Layer

### 2.1. Enum: ResProvinceStatus

**Location**: `src/common/domain/iOne.Domain.Shared/ResProvinces/ResProvinceStatus.cs`

```csharp
namespace iOne.ResProvinces;

public enum ResProvinceStatus
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

### 2.2. Entity: ResProvince

**Location**: `src/common/domain/iOne.Domain/ResProvinces/ResProvince.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResProvinces;

[Table("res_province")] // ✅ snake_case (PostgreSQL convention)
public class ResProvince : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid CountryId { get; private set; }

    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResProvinceStatus Status { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    // Navigation property
    public virtual ResCountry? Country { get; set; }

    protected ResProvince()
    {
        // For ORM
    }

    public ResProvince(
        Guid id,
        Guid countryId,
        string code,
        string name,
        ResProvinceStatus status,
        string? description = null)
        : base(id)
    {
        SetCountryId(countryId);
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetDescription(description);
    }

    private void SetCountryId(Guid countryId)
    {
        if (countryId == Guid.Empty)
        {
            throw new ArgumentException("CountryId cannot be empty.", nameof(countryId));
        }

        CountryId = countryId;
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

    private void SetStatus(ResProvinceStatus status)
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
    public virtual void UpdateCountryId(Guid countryId)
    {
        SetCountryId(countryId);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResProvinceStatus status)
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
- Kế thừa từ `FullAuditedAggregateRoot<Guid>` (có audit fields và soft delete)
- Code: private setter, chỉ set trong constructor
- Không có method `UpdateCode()` - Code không được phép sửa
- Validation: Code chỉ cho phép A-Z, 0-9, _ (tự động convert sang uppercase)
- CountryId: Required, foreign key đến ResCountry
- Description: Optional, max length 500

---

### 2.3. Repository Interface

**Location**: `src/common/domain/iOne.Domain/ResProvinces/IResProvinceRepository.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResProvinces;

public interface IResProvinceRepository : IRepository<ResProvince, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
```

**Đặc điểm**:
- Kế thừa từ `IRepository<ResProvince, Guid>`
- Method `IsCodeExistsAsync`: Check code uniqueness (có thể exclude một ID khi update)

---

### 2.4. Manager (Domain Service)

**Location**: `src/common/domain/iOne.Domain/ResProvinces/ResProvinceManager.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResProvinces;

public class ResProvinceManager : DomainService
{
    protected IResProvinceRepository Repository { get; }
    protected IResCountryRepository CountryRepository { get; }

    public ResProvinceManager(
        IResProvinceRepository repository,
        IResCountryRepository countryRepository)
    {
        Repository = repository;
        CountryRepository = countryRepository;
    }

    public virtual async Task CreateAsync(ResProvince province)
    {
        // Check country exists
        if (!await CountryRepository.AnyAsync(x => x.Id == province.CountryId))
        {
            throw new BusinessException("Master:ResProvince:CountryNotFound")
                .WithData("CountryId", province.CountryId);
        }

        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(province.Code))
        {
            throw new BusinessException("Master:ResProvince:CodeExists")
                .WithData("Code", province.Code);
        }

        await Repository.InsertAsync(province);
    }

    public virtual async Task UpdateAsync(
        ResProvince province,
        Guid countryId,
        string name,
        ResProvinceStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        // Check country exists (if changed)
        if (province.CountryId != countryId)
        {
            if (!await CountryRepository.AnyAsync(x => x.Id == countryId))
            {
                throw new BusinessException("Master:ResProvince:CountryNotFound")
                    .WithData("CountryId", countryId);
            }
        }

        province.UpdateCountryId(countryId);
        province.UpdateName(name);
        province.UpdateStatus(status);
        province.UpdateDescription(description);
        await Repository.UpdateAsync(province);
    }
}
```

**Đặc điểm**:
- Business logic validation: Check code uniqueness, check country exists
- Update method không có parameter code - Code không được phép sửa

---

## 3. Application Layer

### 3.1. DTOs

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResProvinces/`

#### 3.1.1. ResProvinceDto

```csharp
using System;
using Volo.Abp.Application.Dtos;
using iOne.ResProvinces;

namespace iOne.Master.ResProvinces;

public class ResProvinceDto : FullAuditedEntityDto<Guid>
{
    public Guid CountryId { get; set; }
    public string? CountryName { get; set; } // For display
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ResProvinceStatus Status { get; set; }
    public string? Description { get; set; }
}
```

#### 3.1.2. CreateResProvinceDto

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResProvinces;

namespace iOne.Master.ResProvinces;

public class CreateResProvinceDto
{
    [Required]
    public Guid CountryId { get; set; }

    [Required]
    [MaxLength(25)]
    public string Code { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = null!;

    [Required]
    public ResProvinceStatus Status { get; set; } = ResProvinceStatus.Active;

    [MaxLength(500)]
    public string? Description { get; set; }
}
```

#### 3.1.3. UpdateResProvinceDto

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResProvinces;

namespace iOne.Master.ResProvinces;

public class UpdateResProvinceDto
{
    // ⚠️ QUAN TRỌNG: Không có Code - Code không được phép sửa
    [Required]
    public Guid CountryId { get; set; }

    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = null!;

    [Required]
    public ResProvinceStatus Status { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
```

#### 3.1.4. GetResProvincesInput

```csharp
using System;
using Volo.Abp.Application.Dtos;
using iOne.ResProvinces;

namespace iOne.Master.ResProvinces;

public class GetResProvincesInput : PagedAndSortedResultRequestDto
{
    public Guid? CountryId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResProvinceStatus? Status { get; set; }
}
```

---

### 3.2. Application Service Interface

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResProvinces/IResProvinceAppService.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResProvinces;

public interface IResProvinceAppService : ICrudAppService<
    ResProvinceDto,
    Guid,
    GetResProvincesInput,
    CreateResProvinceDto,
    UpdateResProvinceDto>
{
}
```

---

### 3.3. Application Service Implementation

**Location**: `modules/master/src/iOne.Master.Application/ResProvinces/ResProvinceAppService.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.ResCountries;
using iOne.ResProvinces;

namespace iOne.Master.ResProvinces;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResProvincePermissions.Default)]
public class ResProvinceAppService : CrudAppService<
    ResProvince,
    ResProvinceDto,
    Guid,
    GetResProvincesInput,
    CreateResProvinceDto,
    UpdateResProvinceDto>,
    IResProvinceAppService
{
    protected ResProvinceManager Manager { get; }
    protected IRepository<ResCountry, Guid> CountryRepository { get; }

    public ResProvinceAppService(
        IResProvinceRepository repository,
        ResProvinceManager manager,
        IRepository<ResCountry, Guid> countryRepository)
        : base(repository)
    {
        Manager = manager;
        CountryRepository = countryRepository;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = ResProvincePermissions.View;
        GetListPolicyName = ResProvincePermissions.View;
        CreatePolicyName = ResProvincePermissions.Create;
        UpdatePolicyName = ResProvincePermissions.Edit;
        DeletePolicyName = ResProvincePermissions.Delete;
    }

    public override async Task<ResProvinceDto> CreateAsync(CreateResProvinceDto input)
    {
        var entity = new ResProvince(
            GuidGenerator.Create(),
            input.CountryId,
            input.Code,
            input.Name,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return await MapToDtoAsync(entity);
    }

    public override async Task<ResProvinceDto> UpdateAsync(Guid id, UpdateResProvinceDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update CountryId, Name, Status, Description - không update Code
        await Manager.UpdateAsync(
            entity,
            input.CountryId,
            input.Name,
            input.Status,
            input.Description
        );

        // Ensure changes are saved
        await CurrentUnitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        
        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResProvinceStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<ResProvinceDto> MapToDtoAsync(ResProvince entity)
    {
        var dto = ObjectMapper.Map<ResProvince, ResProvinceDto>(entity);
        
        // Load country name if needed
        if (entity.CountryId != Guid.Empty)
        {
            var country = await CountryRepository.GetAsync(entity.CountryId);
            dto.CountryName = country.Name;
        }
        
        return dto;
    }

    protected override async Task<IQueryable<ResProvince>> CreateFilteredQueryAsync(GetResProvincesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        // Filter by CountryId
        if (input.CountryId.HasValue)
        {
            query = query.Where(x => x.CountryId == input.CountryId.Value);
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
- Delete: Gọi `DeleteAsync()` trước → SaveChanges → Update Status → SaveChanges
- Update: Chỉ update CountryId, Name, Status, Description - không update Code
- Filter: Hỗ trợ filter theo CountryId, Code, Name, Status
- MapToDtoAsync: Load CountryName để hiển thị

---

### 3.4. AutoMapper Configuration

**Location**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

**Cần thêm**:
```csharp
using iOne.ResProvinces;

// ResProvince mappings
CreateMap<ResProvince, ResProvinceDto>();
CreateMap<CreateResProvinceDto, ResProvince>();
CreateMap<UpdateResProvinceDto, ResProvince>();
```

---

## 4. Permissions

### 4.1. Permission Constants

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResProvincePermissions.cs`

```csharp
namespace iOne.Master.Permissions;

public static class ResProvincePermissions
{
    public const string GroupName = "MasterResProvince";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

---

### 4.2. Permission Definition Provider

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResProvincePermissionDefinitionProvider.cs`

```csharp
using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResProvincePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resProvinceGroup = context.AddGroup(
            ResProvincePermissions.GroupName,
            L("Permission:ResProvince")
        );

        var resProvincePermission = resProvinceGroup.AddPermission(
            ResProvincePermissions.Default,
            L("Permission:ResProvince")
        );

        resProvincePermission.AddChild(
            ResProvincePermissions.Create,
            L("Permission:Create")
        );

        resProvincePermission.AddChild(
            ResProvincePermissions.Edit,
            L("Permission:Edit")
        );

        resProvincePermission.AddChild(
            ResProvincePermissions.Delete,
            L("Permission:Delete")
        );

        resProvincePermission.AddChild(
            ResProvincePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MasterResource>(name);
    }
}
```

**Đăng ký trong**: `modules/master/src/iOne.Master.Application.Contracts/iOneMasterApplicationContractsModule.cs`

---

## 5. Entity Framework Core

### 5.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResProvinces/ResProvinceConfiguration.cs`

```csharp
using System;
using iOne.ResCountries;
using iOne.ResProvinces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResProvinces;

public class ResProvinceConfiguration : IEntityTypeConfiguration<ResProvince>
{
    public void Configure(EntityTypeBuilder<ResProvince> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_province", t =>
        {
            t.HasComment("Tỉnh/Thành");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        // Note: EF Core với Npgsql tự động map Guid sang UUID, không cần specify HasColumnType
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.CountryId)
            .HasColumnName("country_id")
            .IsRequired();

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
                v => Enum.Parse<ResProvinceStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);
        
        // ✅ ExtraProperties: snake_case
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");
        
        // ✅ Audit columns: snake_case
        // Note: EF Core với Npgsql tự động map Guid sang UUID cho các audit ID columns
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");

        // ✅ Foreign Key: country_id → res_country.id
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // ✅ Index: Unique constraint on Code
        builder.HasIndex(e => e.Code, "ix_res_province_code")
            .IsUnique();

        // ✅ Index: Foreign key index for better query performance
        builder.HasIndex(e => e.CountryId, "ix_res_province_country_id");
    }
}
```

**Đặc điểm**:
- Table name: `res_province` (snake_case)
- Column names: snake_case
- ID và audit IDs: EF Core với Npgsql tự động map `Guid` sang `UUID` (PostgreSQL)
- **Status**: Enum `Active`/`Deactive` (C#) → DB lưu `"active"`/`"deactive"` (lowercase) với max length 10
- Foreign key: `country_id` → `res_country.id` (on delete restrict)
- Unique index trên Code
- Index trên CountryId để tối ưu query performance

---

### 5.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResProvinces/EfCoreResProvinceRepository.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResProvinces;

namespace iOne.EntityFrameworkCore.ResProvinces;

public class EfCoreResProvinceRepository : EfCoreRepository<iOneDbContext, ResProvince, Guid>, IResProvinceRepository
{
    public EfCoreResProvinceRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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
}
```

---

### 5.3. Register Repository trong DbContext

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`

**Cần thêm**:
```csharp
public DbSet<ResProvince> ResProvinces { get; set; }
```

---

### 5.4. Database Migration

**Quy tắc**:
- **LUÔN** sử dụng `IF EXISTS` khi DROP TABLE/COLUMN/INDEX
- **LUÔN** sử dụng `IF NOT EXISTS` khi CREATE TABLE/COLUMN/INDEX
- Table và Column Names: snake_case
- ID và audit IDs: UUID (PostgreSQL) - EF Core tự động map từ Guid

**Migration Script** (ví dụ):
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_province"";");
    
    migrationBuilder.CreateTable(
        name: "res_province",
        columns: table => new
        {
            id = table.Column<Guid>(type: "uuid", nullable: false),
            country_id = table.Column<Guid>(type: "uuid", nullable: false),
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
            deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
            tenant_id = table.Column<Guid>(type: "uuid", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("pk_res_province", x => x.id);
            table.ForeignKey(
                name: "fk_res_province_country_id",
                column: x => x.country_id,
                principalTable: "res_country",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        },
        comment: "Tỉnh/Thành");

    migrationBuilder.CreateIndex(
        name: "ix_res_province_code",
        table: "res_province",
        column: "code",
        unique: true);

    migrationBuilder.CreateIndex(
        name: "ix_res_province_country_id",
        table: "res_province",
        column: "country_id");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_province_country_id"";");
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_province_code"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_province"";");
}
```

---

## 6. HTTP API Controllers

### 6.1. Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/Controllers/ResProvinceController.cs`

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResProvinces;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/provinces")]
[Authorize]
public class ResProvinceController : AbpControllerBase
{
    protected IResProvinceAppService AppService { get; }

    public ResProvinceController(IResProvinceAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResProvincePermissions.View)]
    public virtual Task<PagedResultDto<ResProvinceDto>> GetListAsync(GetResProvincesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResProvincePermissions.View)]
    public virtual Task<ResProvinceDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResProvincePermissions.Create)]
    public virtual Task<ResProvinceDto> CreateAsync(CreateResProvinceDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResProvincePermissions.Edit)]
    public virtual Task<ResProvinceDto> UpdateAsync(Guid id, UpdateResProvinceDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResProvincePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
```

---

### 6.2. Exclude từ Conventional Controllers (nếu cần)

**Location**: `modules/master/src/iOne.Master.HttpApi/iOneMasterHttpApiModule.cs`

Nếu có manual controller, cần exclude AppService khỏi conventional controller generation:

```csharp
Configure<AbpAspNetCoreMvcOptions>(options =>
{
    options.ConventionalControllers.Create(
        typeof(iOneMasterApplicationModule).Assembly,
        opts =>
        {
            opts.TypePredicate = type =>
                type.Name != "ResCountryAppService" &&
                type.Name != "ResProvinceAppService"; // Exclude AppService có manual controller
        });
});
```

---

## 7. Localization

### 7.1. Localization Keys

**Location**: 
- `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/vi-VN.json`
- `modules/master/src/iOne.Master.Application.Contracts/Localization/Master/en.json`

**Cần thêm**:

**vi-VN.json**:
```json
{
  "Culture": "vi-VN",
  "Texts": {
    "Menu:Master": "Danh mục",
    "Menu:ResProvince": "Tỉnh/Thành",
    "Permission:Create": "Tạo mới",
    "Permission:Edit": "Sửa",
    "Permission:Delete": "Xóa",
    "Permission:View": "Xem",
    "Permission:ResProvince": "Tỉnh/Thành",
    "ResProvince:CountryId": "Quốc gia",
    "ResProvince:Code": "Mã tỉnh/thành",
    "ResProvince:Name": "Tên tỉnh/thành",
    "ResProvince:Status": "Trạng thái",
    "ResProvince:Description": "Mô tả",
    "ResProvince:CodeExists": "Mã tỉnh/thành '{Code}' đã tồn tại",
    "ResProvince:CodeInvalid": "Mã tỉnh/thành chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
    "ResProvince:CountryNotFound": "Quốc gia không tồn tại",
    "ResProvince:CreatedSuccessfully": "Tạo tỉnh/thành thành công",
    "ResProvince:UpdatedSuccessfully": "Cập nhật tỉnh/thành thành công",
    "ResProvince:DeletedSuccessfully": "Xóa tỉnh/thành thành công"
  }
}
```

**en.json**:
```json
{
  "Culture": "en",
  "Texts": {
    "Menu:Master": "Master",
    "Menu:ResProvince": "Province",
    "Permission:Create": "Create",
    "Permission:Edit": "Edit",
    "Permission:Delete": "Delete",
    "Permission:View": "View",
    "Permission:ResProvince": "Province",
    "ResProvince:CountryId": "Country",
    "ResProvince:Code": "Province Code",
    "ResProvince:Name": "Province Name",
    "ResProvince:Status": "Status",
    "ResProvince:Description": "Description",
    "ResProvince:CodeExists": "Province code '{Code}' already exists",
    "ResProvince:CodeInvalid": "Province code can only contain letters (A-Z), numbers (0-9) and underscore (_)",
    "ResProvince:CountryNotFound": "Country not found",
    "ResProvince:CreatedSuccessfully": "Province created successfully",
    "ResProvince:UpdatedSuccessfully": "Province updated successfully",
    "ResProvince:DeletedSuccessfully": "Province deleted successfully"
  }
}
```

---

## 8. Menu Configuration (Navigation)

### 8.1. Menu Contributor

**Location**: `modules/master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

**Cần thêm menu item**:
```csharp
masterMenuItem.AddItem(new ApplicationMenuItem(
    "Master.ResProvince",
    masterL["Menu:ResProvince"],
    url: "~/pages/master/provinces",
    icon: "pi pi-fw pi-map"
).RequirePermissions(ResProvincePermissions.Default));
```

**Đã đăng ký trong**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationModule.cs` (đã có sẵn)

---

## 9. Checklist Trước Khi Hoàn Thành

- [ ] Entity có đầy đủ validation attributes
  - [ ] Table name sử dụng `[Table("res_province")]` (snake_case)
  - [ ] Code validation: chỉ A-Z, 0-9, _ (uppercase)
  - [ ] Không có method `UpdateCode()` - Code không được phép sửa
  - [ ] CountryId: Required, foreign key validation
  - [ ] Description: Optional, max length 500
- [ ] Enum `ResProvinceStatus` đã tạo (Active, Deactive)
  - [ ] Status trong DB lưu dưới dạng "active"/"deactive" (lowercase) - đã có `ToLowerInvariant()` trong EF Core config
- [ ] Repository interface và implementation đã tạo
  - [ ] Method `IsCodeExistsAsync` để check code uniqueness
- [ ] Manager (Domain Service) đã tạo
  - [ ] Business logic validation: Check code uniqueness, check country exists
  - [ ] Update method không có parameter code
- [ ] Application service có đầy đủ CRUD operations
  - [ ] Delete: Delete trước → SaveChanges → Update Status → SaveChanges
  - [ ] Update: Chỉ update CountryId, Name, Status, Description - không update Code
  - [ ] MapToDtoAsync: Load CountryName để hiển thị
- [ ] **AutoMapper configuration đã được thêm vào AutoMapper Profile**
  - [ ] Mapping `ResProvince` -> `ResProvinceDto`
  - [ ] Mapping `CreateResProvinceDto` -> `ResProvince`
  - [ ] Mapping `UpdateResProvinceDto` -> `ResProvince`
- [ ] Permissions đã được định nghĩa và sử dụng đúng
  - [ ] Permission Constants: `ResProvincePermissions`
  - [ ] Permission Definition Provider: `ResProvincePermissionDefinitionProvider`
  - [ ] Đã đăng ký trong Application Contracts Module
- [ ] EF Core configuration đã đúng
  - [ ] Table name: `res_province` (snake_case)
  - [ ] Column names: snake_case (bao gồm audit columns)
  - [ ] ID và audit IDs: EF Core tự động map `Guid` sang `UUID` (PostgreSQL)
  - [ ] Status: Enum `Active`/`Deactive` (C#) → DB lưu `"active"`/`"deactive"` (lowercase) với max length 10
  - [ ] Đã sử dụng `ToLowerInvariant()` trong `HasConversion<string>()` để đảm bảo lưu lowercase
  - [ ] Foreign key: `country_id` → `res_country.id` (on delete restrict)
  - [ ] Unique index trên Code: `ix_res_province_code`
  - [ ] Index trên CountryId: `ix_res_province_country_id`
- [ ] Repository implementation đã tạo
  - [ ] Method `IsCodeExistsAsync` đã implement
- [ ] DbContext đã register `DbSet<ResProvince>`
- [ ] Migration đã được tạo và test (với IF EXISTS/IF NOT EXISTS)
  - [ ] Table name trong migration: `res_province` (snake_case)
  - [ ] Column names trong migration: snake_case
  - [ ] ID và audit IDs: UUID (PostgreSQL)
  - [ ] Foreign key constraint: `fk_res_province_country_id`
  - [ ] Index names: `ix_res_province_code`, `ix_res_province_country_id` (snake_case)
- [ ] Localization keys đã đầy đủ (vi-VN và en)
- [ ] Controller đã có đầy đủ endpoints với authorization
- [ ] **Menu Contributor đã được cập nhật với menu item ResProvince**
- [ ] **Conventional Controllers đã được config để tránh duplicate API (nếu có manual controller)**
- [ ] Build solution thành công
- [ ] Test API endpoints thành công
- [ ] **Menu hiển thị đúng trên client**

---

## 10. Notes

### 10.1. Database ID Mapping

- **C#**: Sử dụng `Guid` type
- **PostgreSQL**: Sử dụng `UUID` type
- **EF Core**: Tự động map `Guid` → `UUID` khi dùng Npgsql.EntityFrameworkCore.PostgreSQL (không cần specify `HasColumnType`)

### 10.2. Code Validation

- Code chỉ cho phép: A-Z, 0-9, _ (uppercase)
- Tự động convert sang uppercase trong `SetCode()`
- Validation bằng Regex: `^[A-Z0-9_]+$`

### 10.3. Soft Delete với Status Update

- **Quy trình**:
  1. Gọi `Repository.DeleteAsync(entity)` → Trigger audit log với `ChangeType = Deleted`
  2. `SaveChangesAsync()` → Lưu audit log
  3. `entity.UpdateStatus(ResProvinceStatus.Deactive)` → Update status
  4. `Repository.UpdateAsync(entity)` → Update entity
  5. `SaveChangesAsync()` → Lưu status update

### 10.4. Foreign Key Relationship

- **CountryId**: Required, foreign key đến `res_country.id`
- **On Delete**: Restrict (không cho phép xóa country nếu có province đang sử dụng)
- **Navigation Property**: `Country` (optional, lazy loading)

### 10.5. Cấu trúc File

- **Domain**: `src/common/domain/iOne.Domain/ResProvinces/`
- **Domain.Shared**: `src/common/domain/iOne.Domain.Shared/ResProvinces/`
- **Application.Contracts**: `modules/master/src/iOne.Master.Application.Contracts/ResProvinces/`
- **Application**: `modules/master/src/iOne.Master.Application/ResProvinces/`
- **EntityFrameworkCore**: `src/common/infra/iOne.EntityFrameworkCore/ResProvinces/`
- **HttpApi**: `modules/master/src/iOne.Master.HttpApi/Controllers/ResProvinceController.cs`

---

## 11. API Endpoints

Sau khi hoàn thành, các API endpoints sẽ là:

- `GET /api/master/provinces` - Lấy danh sách (có filter/search)
- `GET /api/master/provinces/{id}` - Lấy chi tiết
- `POST /api/master/provinces` - Tạo mới
- `PUT /api/master/provinces/{id}` - Cập nhật (Code không được phép sửa)
- `DELETE /api/master/provinces/{id}` - Xóa (Soft delete + Set Status = Deactive)

---

## 12. Frontend Integration

**Route**: `~/pages/master/provinces`  
**Permission**: `MasterResProvince` (GroupName)  
**Menu**: "Tỉnh/Thành" trong menu "Danh mục"

**Lưu ý cho Frontend**:
- Khi update: Disable trường Code (readonly hoặc disabled)
- Validation: Code chỉ cho phép A-Z, 0-9, _ (uppercase)
- Delete: Hiển thị confirm dialog trước khi xóa
- CountryId: Dropdown/Select để chọn quốc gia
- Description: Optional textarea field

---

**Plan này đã được review và sẵn sàng để implement. Vui lòng xác nhận trước khi bắt đầu code.**

