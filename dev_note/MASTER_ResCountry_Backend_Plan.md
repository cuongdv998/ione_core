# Plan: Backend Danh mục Quốc gia (ResCountry) - Module Master

## 1. Tổng Quan

**Entity Name**: `ResCountry`  
**Module**: `Master`  
**Table Name**: `res_country` (snake_case - PostgreSQL convention)  
**Namespace**: `iOne.ResCountries`

**Chức năng**: Danh mục Quốc gia
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
- Database: ID là UUID (PostgreSQL) - EF Core tự động map `Guid` sang `UUID`

**Lưu ý Database**:
- Table name trong DDL: `RESCOUNTRY` → sẽ đổi thành `res_country` (snake_case)
- Column names trong DDL: UPPERCASE → sẽ đổi thành snake_case
- ID và audit IDs: UUID type (PostgreSQL)

---

## 2. Domain Layer

### 2.1. Enum: ResCountryStatus

**Location**: `src/common/domain/iOne.Domain.Shared/ResCountries/ResCountryStatus.cs`

```csharp
namespace iOne.ResCountries;

public enum ResCountryStatus
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

### 2.2. Entity: ResCountry

**Location**: `src/common/domain/iOne.Domain/ResCountries/ResCountry.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResCountries;

[Table("res_country")] // ✅ snake_case (PostgreSQL convention)
public class ResCountry : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResCountryStatus Status { get; private set; }

    protected ResCountry()
    {
        // For ORM
    }

    public ResCountry(Guid id, string code, string name, ResCountryStatus status)
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

    private void SetStatus(ResCountryStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResCountryStatus status)
    {
        SetStatus(status);
    }
}
```

**Đặc điểm**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>` (có audit fields và soft delete)
- Code: private setter, chỉ set trong constructor
- Không có method `UpdateCode()` - Code không được phép sửa
- Validation: Code chỉ cho phép A-Z, 0-9, _ (tự động convert sang uppercase)

---

### 2.3. Repository Interface

**Location**: `src/common/domain/iOne.Domain/ResCountries/IResCountryRepository.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResCountries;

public interface IResCountryRepository : IRepository<ResCountry, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
}
```

**Đặc điểm**:
- Kế thừa từ `IRepository<ResCountry, Guid>`
- Method `IsCodeExistsAsync`: Check code uniqueness (có thể exclude một ID khi update)

---

### 2.4. Manager (Domain Service)

**Location**: `src/common/domain/iOne.Domain/ResCountries/ResCountryManager.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResCountries;

public class ResCountryManager : DomainService
{
    protected IResCountryRepository Repository { get; }

    public ResCountryManager(IResCountryRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ResCountry country)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(country.Code))
        {
            throw new BusinessException("Master:ResCountry:CodeExists")
                .WithData("Code", country.Code);
        }

        await Repository.InsertAsync(country);
    }

    public virtual async Task UpdateAsync(ResCountry country, string name, ResCountryStatus status)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        country.UpdateName(name);
        country.UpdateStatus(status);
        await Repository.UpdateAsync(country);
    }
}
```

**Đặc điểm**:
- Business logic validation: Check code uniqueness
- Update method không có parameter code - Code không được phép sửa

---

## 3. Application Layer

### 3.1. DTOs

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResCountries/`

#### 3.1.1. ResCountryDto

```csharp
using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCountries;

public class ResCountryDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ResCountryStatus Status { get; set; }
}
```

#### 3.1.2. CreateResCountryDto

```csharp
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.ResCountries;

public class CreateResCountryDto
{
    [Required]
    [MaxLength(25)]
    public string Code { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = null!;

    [Required]
    public ResCountryStatus Status { get; set; } = ResCountryStatus.Active;
}
```

#### 3.1.3. UpdateResCountryDto

```csharp
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.ResCountries;

public class UpdateResCountryDto
{
    // ⚠️ QUAN TRỌNG: Không có Code - Code không được phép sửa
    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = null!;

    [Required]
    public ResCountryStatus Status { get; set; }
}
```

#### 3.1.4. GetResCountriesInput

```csharp
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCountries;

public class GetResCountriesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResCountryStatus? Status { get; set; }
}
```

---

### 3.2. Application Service Interface

**Location**: `modules/master/src/iOne.Master.Application.Contracts/ResCountries/IResCountryAppService.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Master.ResCountries;

public interface IResCountryAppService : ICrudAppService<
    ResCountryDto,
    Guid,
    GetResCountriesInput,
    CreateResCountryDto,
    UpdateResCountryDto>
{
}
```

---

### 3.3. Application Service Implementation

**Location**: `modules/master/src/iOne.Master.Application/ResCountries/ResCountryAppService.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using iOne.ResCountries;

namespace iOne.Master.ResCountries;

public class ResCountryAppService : CrudAppService<
    ResCountry,
    ResCountryDto,
    Guid,
    GetResCountriesInput,
    CreateResCountryDto,
    UpdateResCountryDto>,
    IResCountryAppService
{
    protected ResCountryManager Manager { get; }

    public ResCountryAppService(
        IRepository<ResCountry, Guid> repository,
        ResCountryManager manager)
        : base(repository)
    {
        Manager = manager;
    }

    public override async Task<ResCountryDto> CreateAsync(CreateResCountryDto input)
    {
        var entity = new ResCountry(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResCountry, ResCountryDto>(entity);
    }

    public override async Task<ResCountryDto> UpdateAsync(Guid id, UpdateResCountryDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name và Status, không update Code
        await Manager.UpdateAsync(entity, input.Name, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResCountry, ResCountryDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        
        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResCountryStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResCountry>> CreateFilteredQueryAsync(GetResCountriesInput input)
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
- Delete: Gọi `DeleteAsync()` trước → SaveChanges → Update Status → SaveChanges
- Update: Chỉ update Name và Status, không update Code
- Filter: Hỗ trợ filter theo Code, Name, Status

---

### 3.4. AutoMapper Configuration

**Location**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationAutoMapperProfile.cs`

**Cần thêm**:
```csharp
// ResCountry mappings
CreateMap<ResCountry, ResCountryDto>();
CreateMap<CreateResCountryDto, ResCountry>();
CreateMap<UpdateResCountryDto, ResCountry>();
```

---

## 4. Permissions

### 4.1. Permission Constants

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResCountryPermissions.cs`

```csharp
namespace iOne.Master.Permissions;

public static class ResCountryPermissions
{
    public const string GroupName = "MasterResCountry";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

---

### 4.2. Permission Definition Provider

**Location**: `modules/master/src/iOne.Master.Application.Contracts/Permissions/ResCountryPermissionDefinitionProvider.cs`

```csharp
using iOne.Master.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Master.Permissions;

public class ResCountryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resCountryGroup = context.AddGroup(
            ResCountryPermissions.GroupName,
            L("Permission:ResCountry")
        );

        var resCountryPermission = resCountryGroup.AddPermission(
            ResCountryPermissions.Default,
            L("Permission:ResCountry")
        );

        resCountryPermission.AddChild(
            ResCountryPermissions.Create,
            L("Permission:Create")
        );

        resCountryPermission.AddChild(
            ResCountryPermissions.Edit,
            L("Permission:Edit")
        );

        resCountryPermission.AddChild(
            ResCountryPermissions.Delete,
            L("Permission:Delete")
        );

        resCountryPermission.AddChild(
            ResCountryPermissions.View,
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

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResCountries/ResCountryConfiguration.cs`

```csharp
using System;
using iOne.ResCountries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResCountries;

public class ResCountryConfiguration : IEntityTypeConfiguration<ResCountry>
{
    public void Configure(EntityTypeBuilder<ResCountry> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_country", t =>
        {
            t.HasComment("Quốc gia (tự insert dữ liệu không có form nhập liệu)");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        // Note: EF Core với Npgsql tự động map Guid sang UUID, không cần specify HasColumnType
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
                v => Enum.Parse<ResCountryStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");
        
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

        // ✅ Index: Unique constraint on Code
        builder.HasIndex(e => e.Code, "ix_res_country_code")
            .IsUnique();
    }
}
```

**Đặc điểm**:
- Table name: `res_country` (snake_case)
- Column names: snake_case
- ID và audit IDs: EF Core với Npgsql tự động map `Guid` sang `UUID` (PostgreSQL)
- **Status**: Enum `Active`/`Deactive` (C#) → DB lưu `"active"`/`"deactive"` (lowercase) với max length 10
- Unique index trên Code

---

### 5.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResCountries/EfCoreResCountryRepository.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResCountries;

namespace iOne.EntityFrameworkCore.ResCountries;

public class EfCoreResCountryRepository : EfCoreRepository<iOneDbContext, ResCountry, Guid>, IResCountryRepository
{
    public EfCoreResCountryRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
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
public DbSet<ResCountry> ResCountries { get; set; }
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
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_country"";");
    
    migrationBuilder.CreateTable(
        name: "res_country",
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
            tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
            extra_properties = table.Column<string>(type: "text", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("pk_res_country", x => x.id);
        },
        comment: "Quốc gia (tự insert dữ liệu không có form nhập liệu)");

    migrationBuilder.CreateIndex(
        name: "ix_res_country_code",
        table: "res_country",
        column: "code",
        unique: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_country_code"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_country"";");
}
```

---

## 6. HTTP API Controllers

### 6.1. Controller

**Location**: `modules/master/src/iOne.Master.HttpApi/Controllers/ResCountryController.cs`

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using iOne.Master.Permissions;
using iOne.Master.ResCountries;

namespace iOne.Master.Controllers;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Area(MasterRemoteServiceConsts.ModuleName)]
[Route("api/master/countries")]
[Authorize]
public class ResCountryController : AbpControllerBase
{
    protected IResCountryAppService AppService { get; }

    public ResCountryController(IResCountryAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResCountryPermissions.View)]
    public virtual Task<PagedResultDto<ResCountryDto>> GetListAsync(GetResCountriesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResCountryPermissions.View)]
    public virtual Task<ResCountryDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResCountryPermissions.Create)]
    public virtual Task<ResCountryDto> CreateAsync(CreateResCountryDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResCountryPermissions.Edit)]
    public virtual Task<ResCountryDto> UpdateAsync(Guid id, UpdateResCountryDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResCountryPermissions.Delete)]
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
                type.Name != "ResCountryAppService"; // Exclude AppService có manual controller
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
    "Menu:ResCountry": "Quốc gia",
    "Permission:Create": "Tạo mới",
    "Permission:Edit": "Sửa",
    "Permission:Delete": "Xóa",
    "Permission:View": "Xem",
    "Permission:ResCountry": "Quốc gia",
    "ResCountry:Code": "Mã quốc gia",
    "ResCountry:Name": "Tên quốc gia",
    "ResCountry:Status": "Trạng thái",
    "ResCountry:CodeExists": "Mã quốc gia '{Code}' đã tồn tại",
    "ResCountry:CodeInvalid": "Mã quốc gia chỉ được chứa chữ cái (A-Z), số (0-9) và dấu gạch dưới (_)",
    "ResCountry:CreatedSuccessfully": "Tạo quốc gia thành công",
    "ResCountry:UpdatedSuccessfully": "Cập nhật quốc gia thành công",
    "ResCountry:DeletedSuccessfully": "Xóa quốc gia thành công"
  }
}
```

**en.json**:
```json
{
  "Culture": "en",
  "Texts": {
    "Menu:Master": "Master",
    "Menu:ResCountry": "Country",
    "Permission:Create": "Create",
    "Permission:Edit": "Edit",
    "Permission:Delete": "Delete",
    "Permission:View": "View",
    "Permission:ResCountry": "Country",
    "ResCountry:Code": "Country Code",
    "ResCountry:Name": "Country Name",
    "ResCountry:Status": "Status",
    "ResCountry:CodeExists": "Country code '{Code}' already exists",
    "ResCountry:CodeInvalid": "Country code can only contain letters (A-Z), numbers (0-9) and underscore (_)",
    "ResCountry:CreatedSuccessfully": "Country created successfully",
    "ResCountry:UpdatedSuccessfully": "Country updated successfully",
    "ResCountry:DeletedSuccessfully": "Country deleted successfully"
  }
}
```

---

## 8. Menu Configuration (Navigation)

### 8.1. Menu Contributor

**Location**: `modules/master/src/iOne.Master.Application/Navigation/MasterMenuContributor.cs`

**Cần thêm menu item**:
```csharp
resCountryMenuItem.AddItem(new ApplicationMenuItem(
    "Master.ResCountry",
    masterL["Menu:ResCountry"],
    url: "~/pages/master/countries",
    icon: "pi pi-fw pi-globe"
).RequirePermissions(ResCountryPermissions.Default));
```

**Đã đăng ký trong**: `modules/master/src/iOne.Master.Application/iOneMasterApplicationModule.cs` (đã có sẵn)

---

## 9. Checklist Trước Khi Hoàn Thành

- [ ] Entity có đầy đủ validation attributes
  - [ ] Table name sử dụng `[Table("res_country")]` (snake_case)
  - [ ] Code validation: chỉ A-Z, 0-9, _ (uppercase)
  - [ ] Không có method `UpdateCode()` - Code không được phép sửa
- [ ] Enum `ResCountryStatus` đã tạo (Active, Deactive)
  - [ ] Status trong DB lưu dưới dạng "active"/"deactive" (lowercase) - đã có `ToLowerInvariant()` trong EF Core config
- [ ] Repository interface và implementation đã tạo
  - [ ] Method `IsCodeExistsAsync` để check code uniqueness
- [ ] Manager (Domain Service) đã tạo
  - [ ] Business logic validation: Check code uniqueness
  - [ ] Update method không có parameter code
- [ ] Application service có đầy đủ CRUD operations
  - [ ] Delete: Delete trước → SaveChanges → Update Status → SaveChanges
  - [ ] Update: Chỉ update Name và Status, không update Code
- [ ] **AutoMapper configuration đã được thêm vào AutoMapper Profile**
  - [ ] Mapping `ResCountry` -> `ResCountryDto`
  - [ ] Mapping `CreateResCountryDto` -> `ResCountry`
  - [ ] Mapping `UpdateResCountryDto` -> `ResCountry`
- [ ] Permissions đã được định nghĩa và sử dụng đúng
  - [ ] Permission Constants: `ResCountryPermissions`
  - [ ] Permission Definition Provider: `ResCountryPermissionDefinitionProvider`
  - [ ] Đã đăng ký trong Application Contracts Module
- [ ] EF Core configuration đã đúng
  - [ ] Table name: `res_country` (snake_case)
  - [ ] Column names: snake_case (bao gồm audit columns)
  - [ ] ID và audit IDs: EF Core tự động map `Guid` sang `UUID` (PostgreSQL)
  - [ ] Status: Enum `Active`/`Deactive` (C#) → DB lưu `"active"`/`"deactive"` (lowercase) với max length 10
  - [ ] Đã sử dụng `ToLowerInvariant()` trong `HasConversion<string>()` để đảm bảo lưu lowercase
  - [ ] Unique index trên Code: `ix_res_country_code`
- [ ] Repository implementation đã tạo
  - [ ] Method `IsCodeExistsAsync` đã implement
- [ ] DbContext đã register `DbSet<ResCountry>`
- [ ] Migration đã được tạo và test (với IF EXISTS/IF NOT EXISTS)
  - [ ] Table name trong migration: `res_country` (snake_case)
  - [ ] Column names trong migration: snake_case
  - [ ] ID và audit IDs: UUID (PostgreSQL)
  - [ ] Index name: `ix_res_country_code` (snake_case)
- [ ] Localization keys đã đầy đủ (vi-VN và en)
- [ ] Controller đã có đầy đủ endpoints với authorization
- [ ] **Menu Contributor đã được cập nhật với menu item ResCountry**
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
- **Lưu ý**: EF Core với Npgsql provider tự động nhận diện `Guid` và map sang PostgreSQL `UUID` type

### 10.2. Code Validation

- Code chỉ cho phép: A-Z, 0-9, _ (uppercase)
- Tự động convert sang uppercase trong `SetCode()`
- Validation bằng Regex: `^[A-Z0-9_]+$`

### 10.3. Soft Delete với Status Update

- **Quy trình**:
  1. Gọi `Repository.DeleteAsync(entity)` → Trigger audit log với `ChangeType = Deleted`
  2. `SaveChangesAsync()` → Lưu audit log
  3. `entity.UpdateStatus(ResCountryStatus.Deactive)` → Update status
  4. `Repository.UpdateAsync(entity)` → Update entity
  5. `SaveChangesAsync()` → Lưu status update

### 10.4. Cấu trúc File

- **Domain**: `src/common/domain/iOne.Domain/ResCountries/`
- **Domain.Shared**: `src/common/domain/iOne.Domain.Shared/ResCountries/`
- **Application.Contracts**: `modules/master/src/iOne.Master.Application.Contracts/ResCountries/`
- **Application**: `modules/master/src/iOne.Master.Application/ResCountries/`
- **EntityFrameworkCore**: `src/common/infra/iOne.EntityFrameworkCore/ResCountries/`
- **HttpApi**: `modules/master/src/iOne.Master.HttpApi/Controllers/ResCountryController.cs`

---

## 11. API Endpoints

Sau khi hoàn thành, các API endpoints sẽ là:

- `GET /api/master/countries` - Lấy danh sách (có filter/search)
- `GET /api/master/countries/{id}` - Lấy chi tiết
- `POST /api/master/countries` - Tạo mới
- `PUT /api/master/countries/{id}` - Cập nhật (Code không được phép sửa)
- `DELETE /api/master/countries/{id}` - Xóa (Soft delete + Set Status = Deactive)

---

## 12. Frontend Integration

**Route**: `~/pages/master/countries`  
**Permission**: `MasterResCountry` (GroupName)  
**Menu**: "Quốc gia" trong menu "Danh mục"

**Lưu ý cho Frontend**:
- Khi update: Disable trường Code (readonly hoặc disabled)
- Validation: Code chỉ cho phép A-Z, 0-9, _ (uppercase)
- Delete: Hiển thị confirm dialog trước khi xóa

---

**Plan này đã được review và sẵn sàng để implement. Vui lòng xác nhận trước khi bắt đầu code.**

