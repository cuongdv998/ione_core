# Plan: Backend Danh mục Loại đối tác (Partner Type) - Module Partner

## 1. Tổng Quan

**Entity Name**: `ResPartnerType`  
**Module**: `Partner`  
**Table Name**: `res_partner_type` (snake_case - PostgreSQL convention)  
**Namespace**: `iOne.ResPartnerTypes`

**Chức năng**:
- ✅ Xem danh sách (có filter/search)
- ✅ Thêm mới
- ✅ Sửa (Code không được phép sửa)
- ✅ Xóa (Soft delete: Delete trước → Set Status = Deactive sau)

**Yêu cầu đặc biệt**:
- Code chỉ cho phép: A-Z, 0-9, _ (uppercase)
- Code là duy nhất (unique)
- Code không được phép sửa khi update (disable trên UI)
- Xóa: Gọi `DeleteAsync()` trước để trigger audit log, sau đó set `Status = Deactive`

---

## 2. Domain Layer

### 2.1. Enum: ResPartnerTypeStatus

**Location**: `src/common/domain/iOne.Domain.Shared/ResPartnerTypes/ResPartnerTypeStatus.cs`

```csharp
namespace iOne.ResPartnerTypes;

public enum ResPartnerTypeStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

**Đặc điểm**:
- Enum values: `Active = 0`, `Deactive = 1`
- Trong database: lưu dưới dạng string `"active"`, `"deactive"` (lowercase)
- Sử dụng `HasConversion<string>()` trong EF Core configuration

---

### 2.2. Entity: ResPartnerType

**Location**: `src/common/domain/iOne.Domain/ResPartnerTypes/ResPartnerType.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResPartnerTypes;

[Table("res_partner_type")] // ✅ snake_case (PostgreSQL convention)
public class ResPartnerType : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResPartnerTypeStatus Status { get; private set; }

    protected ResPartnerType()
    {
        // For ORM
    }

    public ResPartnerType(Guid id, string code, string name, ResPartnerTypeStatus status)
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

        // Validate code format: only A-Z, 0-9, and underscore
        if (!Regex.IsMatch(code, @"^[A-Z0-9_]+$"))
        {
            throw new ArgumentException("Code can only contain uppercase letters (A-Z), numbers (0-9), and underscore (_).", nameof(code));
        }

        Code = code;
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

    private void SetStatus(ResPartnerTypeStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResPartnerTypeStatus status)
    {
        SetStatus(status);
    }
}
```

**Đặc điểm**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>` (có audit log tự động)
- Table name: `res_partner_type` (snake_case)
- Code: `private set`, chỉ set được qua constructor
- Code validation: Regex `^[A-Z0-9_]+$` (chỉ A-Z, 0-9, _)
- Code max length: 25
- Name max length: 250
- **Không có method `UpdateCode()`** - Code không được phép sửa

---

### 2.3. Repository Interface

**Location**: `src/common/domain/iOne.Domain/ResPartnerTypes/IResPartnerTypeRepository.cs`

```csharp
using Volo.Abp.Domain.Repositories;

namespace iOne.ResPartnerTypes;

public interface IResPartnerTypeRepository : IRepository<ResPartnerType, Guid>
{
}
```

**Đặc điểm**:
- Kế thừa từ `IRepository<ResPartnerType, Guid>`
- Có thể thêm custom methods nếu cần

---

### 2.4. Manager: ResPartnerTypeManager

**Location**: `src/common/domain/iOne.Domain/ResPartnerTypes/ResPartnerTypeManager.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ResPartnerTypes;

public class ResPartnerTypeManager : DomainService
{
    protected IResPartnerTypeRepository Repository { get; }

    public ResPartnerTypeManager(IResPartnerTypeRepository repository)
    {
        Repository = repository;
    }

    public async Task CreateAsync(ResPartnerType entity)
    {
        // Check Code uniqueness
        if (await Repository.AnyAsync(x => x.Code == entity.Code))
        {
            throw new UserFriendlyException($"Code '{entity.Code}' already exists.");
        }

        await Repository.InsertAsync(entity);
    }

    public async Task UpdateAsync(ResPartnerType entity, string name, ResPartnerTypeStatus status)
    {
        // ⚠️ QUAN TRỌNG: Không update Code
        entity.UpdateName(name);
        entity.UpdateStatus(status);

        await Repository.UpdateAsync(entity);
    }
}
```

**Đặc điểm**:
- `CreateAsync`: Validate Code uniqueness
- `UpdateAsync`: Chỉ update Name và Status, không update Code

---

## 3. Entity Framework Core Layer

### 3.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResPartnerTypes/ResPartnerTypeConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using iOne.ResPartnerTypes;

namespace iOne.EntityFrameworkCore.ResPartnerTypes;

public class ResPartnerTypeConfiguration : IEntityTypeConfiguration<ResPartnerType>
{
    public void Configure(EntityTypeBuilder<ResPartnerType> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_partner_type", t =>
        {
            t.HasComment("Loại đối tác");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        // ✅ Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(10)
            .IsRequired()
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResPartnerTypeStatus>(v, true)
            );

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

        // ✅ Index: Code unique (snake_case với prefix)
        builder.HasIndex(e => e.Code, "ix_res_partner_type_code")
            .IsUnique();

        // ✅ Primary key name: snake_case với prefix
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
    }
}
```

**Đặc điểm**:
- Table name: `res_partner_type` (snake_case)
- Column names: `code`, `name`, `status` (snake_case)
- Status conversion: Enum → string (lowercase: `"active"`, `"deactive"`)
- Index: `ix_res_partner_type_code` (unique)
- Primary key: `id` (Guid)

---

### 3.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/ResPartnerTypes/EfCoreResPartnerTypeRepository.cs`

```csharp
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using iOne.EntityFrameworkCore;
using iOne.ResPartnerTypes;

namespace iOne.EntityFrameworkCore.ResPartnerTypes;

public class EfCoreResPartnerTypeRepository
    : EfCoreRepository<iOneDbContext, ResPartnerType, Guid>,
      IResPartnerTypeRepository
{
    public EfCoreResPartnerTypeRepository(
        IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
```

**Đặc điểm**:
- Kế thừa từ `EfCoreRepository<iOneDbContext, ResPartnerType, Guid>`
- Implement `IResPartnerTypeRepository`

---

### 3.3. Register Repository trong DbContext

**Location**: `src/common/infra/iOne.EntityFrameworkCore/iOneDbContext.cs`

**Cần thêm**:
```csharp
public DbSet<ResPartnerType> ResPartnerTypes { get; set; }
```

**Trong `OnModelCreating`**:
```csharp
builder.ApplyConfiguration(new ResPartnerTypeConfiguration());
```

---

### 3.4. Migration

**Quy tắc QUAN TRỌNG**:
- **LUÔN** sử dụng `IF EXISTS` khi DROP
- **LUÔN** sử dụng `IF NOT EXISTS` khi CREATE
- Table và column names: **snake_case** (PostgreSQL convention)

**Migration Script** (ví dụ):
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ✅ ĐÚNG: Sử dụng IF EXISTS
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_partner_type"";");
    
    // ✅ ĐÚNG: Table name theo snake_case
    migrationBuilder.CreateTable(
        name: "res_partner_type", // snake_case
        columns: table => new
        {
            id = table.Column<Guid>(type: "uuid", nullable: false),
            code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false), // snake_case
            name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false), // snake_case
            status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false), // snake_case
            creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false), // snake_case
            creator_id = table.Column<Guid>(type: "uuid", nullable: true), // snake_case
            last_modification_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true), // snake_case
            last_modifier_id = table.Column<Guid>(type: "uuid", nullable: true), // snake_case
            is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false), // snake_case
            deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true), // snake_case
            deleter_id = table.Column<Guid>(type: "uuid", nullable: true), // snake_case
            concurrency_stamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true), // snake_case
            tenant_id = table.Column<Guid>(type: "uuid", nullable: true), // snake_case
        },
        constraints: table =>
        {
            // ✅ ĐÚNG: Primary key name theo snake_case với prefix
            table.PrimaryKey("pk_res_partner_type", x => x.id);
        },
        comment: "Loại đối tác");

    // ✅ ĐÚNG: Index name theo snake_case với prefix
    migrationBuilder.CreateIndex(
        name: "ix_res_partner_type_code",
        table: "res_partner_type",
        column: "code",
        unique: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_res_partner_type_code"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""res_partner_type"";");
}
```

---

## 4. Application Layer

### 4.1. DTOs

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResPartnerTypes/`

#### 4.1.1. ResPartnerTypeDto

```csharp
using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResPartnerTypes;

public class ResPartnerTypeDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ResPartnerTypeStatus Status { get; set; }
}
```

#### 4.1.2. CreateResPartnerTypeDto

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.ResPartnerTypes;

namespace iOne.Partner.ResPartnerTypes;

public class CreateResPartnerTypeDto
{
    [Required]
    [MaxLength(25)]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Code can only contain uppercase letters (A-Z), numbers (0-9), and underscore (_)")]
    public string Code { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = null!;

    [Required]
    public ResPartnerTypeStatus Status { get; set; }
}
```

#### 4.1.3. UpdateResPartnerTypeDto

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.ResPartnerTypes;

namespace iOne.Partner.ResPartnerTypes;

public class UpdateResPartnerTypeDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = null!;

    [Required]
    public ResPartnerTypeStatus Status { get; set; }
}
```

#### 4.1.4. GetResPartnerTypesInput

```csharp
using System;
using iOne.ResPartnerTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResPartnerTypes;

public class GetResPartnerTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResPartnerTypeStatus? Status { get; set; }
}
```

**Đặc điểm**:
- `CreateResPartnerTypeDto`: Có Code với validation (Regex)
- `UpdateResPartnerTypeDto`: **KHÔNG có Code** - Code không được phép sửa
- `GetResPartnerTypesInput`: Filter theo Code, Name, Status

---

### 4.2. Application Service Interface

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/ResPartnerTypes/IResPartnerTypeAppService.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResPartnerTypes;

public interface IResPartnerTypeAppService : ICrudAppService<
    ResPartnerTypeDto,
    Guid,
    GetResPartnerTypesInput,
    CreateResPartnerTypeDto,
    UpdateResPartnerTypeDto>
{
}
```

**Đặc điểm**:
- Kế thừa từ `ICrudAppService` với đầy đủ CRUD operations

---

### 4.3. Application Service Implementation

**Location**: `modules/partner/src/iOne.Partner.Application/ResPartnerTypes/ResPartnerTypeAppService.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Partner.Localization;
using iOne.Partner.ResPartnerTypes;
using iOne.Partner.Permissions;
using iOne.ResPartnerTypes; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Partner.ResPartnerTypes;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Authorize(ResPartnerTypePermissions.Default)]
public class ResPartnerTypeAppService : CrudAppService<
    ResPartnerType,
    ResPartnerTypeDto,
    Guid,
    GetResPartnerTypesInput,
    CreateResPartnerTypeDto,
    UpdateResPartnerTypeDto>, IResPartnerTypeAppService
{
    protected ResPartnerTypeManager Manager { get; }

    public ResPartnerTypeAppService(
        IResPartnerTypeRepository repository,
        ResPartnerTypeManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(PartnerResource);
        GetPolicyName = ResPartnerTypePermissions.View;
        GetListPolicyName = ResPartnerTypePermissions.View;
        CreatePolicyName = ResPartnerTypePermissions.Create;
        UpdatePolicyName = ResPartnerTypePermissions.Edit;
        DeletePolicyName = ResPartnerTypePermissions.Delete;
    }

    public override async Task<ResPartnerTypeDto> CreateAsync(CreateResPartnerTypeDto input)
    {
        var entity = new ResPartnerType(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<ResPartnerType, ResPartnerTypeDto>(entity);
    }

    public override async Task<ResPartnerTypeDto> UpdateAsync(Guid id, UpdateResPartnerTypeDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name và Status, không update Code
        await Manager.UpdateAsync(entity, input.Name, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ResPartnerType, ResPartnerTypeDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // Repository.GetAsync will throw exception if not found, never returns null
        var entity = await Repository.GetAsync(id);
        
        // ⚠️ QUAN TRỌNG: Delete trước để trigger audit logging với ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Sau đó update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(ResPartnerTypeStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<ResPartnerType>> CreateFilteredQueryAsync(GetResPartnerTypesInput input)
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
- `CreateAsync`: Tạo entity mới với Code validation
- `UpdateAsync`: Chỉ update Name và Status, không update Code
- `DeleteAsync`: **Delete trước** (trigger audit log) → **Sau đó set Status = Deactive**
- `CreateFilteredQueryAsync`: Filter theo Code, Name, Status

---

### 4.4. AutoMapper Configuration

**Location**: `modules/partner/src/iOne.Partner.Application/iOnePartnerApplicationAutoMapperProfile.cs`

**Cần thêm**:
```csharp
using AutoMapper;
using iOne.Partner.ResPartnerTypes;
using iOne.ResPartnerTypes;

namespace iOne.Partner;

public class iOnePartnerApplicationAutoMapperProfile : Profile
{
    public iOnePartnerApplicationAutoMapperProfile()
    {
        // ResPartnerType mappings
        CreateMap<ResPartnerType, ResPartnerTypeDto>();
        CreateMap<CreateResPartnerTypeDto, ResPartnerType>();
        CreateMap<UpdateResPartnerTypeDto, ResPartnerType>();
    }
}
```

**⚠️ QUAN TRỌNG**: Phải thêm mapping để tránh lỗi `"Missing type map configuration or unsupported mapping"`

---

## 5. Permissions

### 5.1. Permission Constants

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Permissions/ResPartnerTypePermissions.cs`

```csharp
namespace iOne.Partner.Permissions;

public static class ResPartnerTypePermissions
{
    public const string GroupName = "PartnerResPartnerType";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

**Đặc điểm**:
- `GroupName` = `"PartnerResPartnerType"`
- `Default` = `GroupName` (không phải `"{GroupName}.Default"`)
- Frontend sử dụng `GroupName` để check permission

---

### 5.2. Permission Definition Provider

**Location**: `modules/partner/src/iOne.Partner.Application.Contracts/Permissions/ResPartnerTypePermissionDefinitionProvider.cs`

```csharp
using iOne.Partner.Localization;
using iOne.Partner.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Partner.Permissions;

public class ResPartnerTypePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var resPartnerTypeGroup = context.AddGroup(
            ResPartnerTypePermissions.GroupName,
            L("Permission:ResPartnerType")
        );

        var resPartnerTypePermission = resPartnerTypeGroup.AddPermission(
            ResPartnerTypePermissions.Default,
            L("Permission:ResPartnerType")
        );

        resPartnerTypePermission.AddChild(
            ResPartnerTypePermissions.Create,
            L("Permission:Create")
        );

        resPartnerTypePermission.AddChild(
            ResPartnerTypePermissions.Edit,
            L("Permission:Edit")
        );

        resPartnerTypePermission.AddChild(
            ResPartnerTypePermissions.Delete,
            L("Permission:Delete")
        );

        resPartnerTypePermission.AddChild(
            ResPartnerTypePermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PartnerResource>(name);
    }
}
```

**Đặc điểm**:
- Tạo permission group với `GroupName`
- Tạo Default permission = `GroupName`
- Thêm child permissions (Create, Edit, Delete, View)

---

## 6. Localization

### 6.1. Localization Files

**Location**: 
- `modules/partner/src/iOne.Partner.Application.Contracts/Localization/Partner/vi-VN.json`
- `modules/partner/src/iOne.Partner.Application.Contracts/Localization/Partner/en.json`

#### vi-VN.json (cần thêm):

```json
{
  "Culture": "vi-VN",
  "Texts": {
    "Menu:Partner": "Đối tác",
    "Menu:ResPartnerType": "Danh mục Loại đối tác",
    "Permission:ResPartnerType": "Danh mục Loại đối tác",
    "Permission:Create": "Tạo mới",
    "Permission:Edit": "Sửa",
    "Permission:Delete": "Xóa",
    "Permission:View": "Xem",
    "ResPartnerType:Code": "Mã loại đối tác",
    "ResPartnerType:Name": "Tên loại đối tác",
    "ResPartnerType:Status": "Trạng thái",
    "ResPartnerType:New": "Thêm mới",
    "ResPartnerType:Edit": "Chỉnh sửa",
    "ResPartnerType:Delete": "Xóa",
    "ResPartnerType:SearchByCode": "Tìm theo mã",
    "ResPartnerType:SearchByName": "Tìm theo tên",
    "ResPartnerType:Active": "Hoạt động",
    "ResPartnerType:Deactive": "Không hoạt động",
    "ResPartnerType:CodeRequired": "Mã loại đối tác không được để trống",
    "ResPartnerType:CodeMaxLength": "Mã loại đối tác không được vượt quá {0} ký tự",
    "ResPartnerType:CodeInvalidFormat": "Mã loại đối tác chỉ được chứa chữ cái in hoa (A-Z), số (0-9) và dấu gạch dưới (_)",
    "ResPartnerType:CodeExists": "Mã loại đối tác '{Code}' đã tồn tại",
    "ResPartnerType:NameRequired": "Tên loại đối tác không được để trống",
    "ResPartnerType:NameMaxLength": "Tên loại đối tác không được vượt quá {0} ký tự",
    "ResPartnerType:StatusRequired": "Trạng thái không được để trống",
    "ResPartnerType:Status:Active": "Hoạt động",
    "ResPartnerType:Status:Deactive": "Không hoạt động",
    "ResPartnerType:CreatedSuccessfully": "Tạo mới thành công",
    "ResPartnerType:UpdatedSuccessfully": "Cập nhật thành công",
    "ResPartnerType:DeletedSuccessfully": "Xóa thành công",
    "ResPartnerType:DeleteConfirm": "Bạn có chắc chắn muốn xóa bản ghi này?"
  }
}
```

#### en.json (cần thêm):

```json
{
  "Culture": "en",
  "Texts": {
    "Menu:Partner": "Partner",
    "Menu:ResPartnerType": "Partner Types",
    "Permission:ResPartnerType": "Partner Type",
    "Permission:Create": "Create",
    "Permission:Edit": "Edit",
    "Permission:Delete": "Delete",
    "Permission:View": "View",
    "ResPartnerType:Code": "Code",
    "ResPartnerType:Name": "Name",
    "ResPartnerType:Status": "Status",
    "ResPartnerType:New": "New",
    "ResPartnerType:Edit": "Edit",
    "ResPartnerType:Delete": "Delete",
    "ResPartnerType:SearchByCode": "Search by code",
    "ResPartnerType:SearchByName": "Search by name",
    "ResPartnerType:Active": "Active",
    "ResPartnerType:Deactive": "Deactive",
    "ResPartnerType:CodeRequired": "Code cannot be empty",
    "ResPartnerType:CodeMaxLength": "Code cannot exceed {0} characters",
    "ResPartnerType:CodeInvalidFormat": "Code can only contain uppercase letters (A-Z), numbers (0-9), and underscore (_)",
    "ResPartnerType:CodeExists": "Code '{Code}' already exists",
    "ResPartnerType:NameRequired": "Name cannot be empty",
    "ResPartnerType:NameMaxLength": "Name cannot exceed {0} characters",
    "ResPartnerType:StatusRequired": "Status cannot be empty",
    "ResPartnerType:Status:Active": "Active",
    "ResPartnerType:Status:Deactive": "Deactive",
    "ResPartnerType:CreatedSuccessfully": "Created successfully",
    "ResPartnerType:UpdatedSuccessfully": "Updated successfully",
    "ResPartnerType:DeletedSuccessfully": "Deleted successfully",
    "ResPartnerType:DeleteConfirm": "Are you sure you want to delete this record?"
  }
}
```

**Đặc điểm**:
- Keys phải giống nhau ở cả 2 file (vi-VN và en)
- Sử dụng placeholders `{0}`, `{Code}` cho dynamic values

---

## 7. HTTP API Controllers

### 7.1. Controller

**Location**: `modules/partner/src/iOne.Partner.HttpApi/Controllers/ResPartnerTypeController.cs`

```csharp
using iOne.Partner;
using iOne.Partner.Permissions;
using iOne.Partner.ResPartnerTypes;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Partner.Controllers;

[RemoteService(Name = PartnerRemoteServiceConsts.RemoteServiceName)]
[Area(PartnerRemoteServiceConsts.ModuleName)]
[Route("api/partner/partner-types")]
[Authorize]
public class ResPartnerTypeController : AbpControllerBase
{
    protected IResPartnerTypeAppService AppService { get; }

    public ResPartnerTypeController(IResPartnerTypeAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    [Authorize(ResPartnerTypePermissions.View)]
    public virtual Task<PagedResultDto<ResPartnerTypeDto>> GetListAsync(GetResPartnerTypesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ResPartnerTypePermissions.View)]
    public virtual Task<ResPartnerTypeDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    [Authorize(ResPartnerTypePermissions.Create)]
    public virtual Task<ResPartnerTypeDto> CreateAsync(CreateResPartnerTypeDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(ResPartnerTypePermissions.Edit)]
    public virtual Task<ResPartnerTypeDto> UpdateAsync(Guid id, UpdateResPartnerTypeDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(ResPartnerTypePermissions.Delete)]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
```

**Đặc điểm**:
- Route: `"api/partner/partner-types"`
- Methods có `[Authorize]` với permission tương ứng
- Có `[RemoteService]` và `[Area]` attributes

---

### 7.2. Exclude từ Conventional Controllers

**Location**: `modules/partner/src/iOne.Partner.HttpApi/iOnePartnerHttpApiModule.cs`

**Cần thêm** (nếu chưa có):
```csharp
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace iOne.Partner;

[DependsOn(
    typeof(iOnePartnerApplicationContractsModule),
    typeof(iOnePartnerApplicationModule), // Cần reference để access Assembly
    // ... other dependencies
)]
public class iOnePartnerHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Create conventional controllers for module, but exclude services that have manual controllers
            options.ConventionalControllers.Create(
                typeof(iOnePartnerApplicationModule).Assembly,
                opts =>
                {
                    opts.TypePredicate = type =>
                        type.Name != "ResPartnerTypeAppService"; // Exclude AppService có manual controller
                });
        });
    }
}
```

**⚠️ QUAN TRỌNG**: Phải exclude để tránh duplicate API endpoints

**Cần thêm ProjectReference** trong `iOne.Partner.HttpApi.csproj`:
```xml
<ProjectReference Include="..\iOne.Partner.Application\iOne.Partner.Application.csproj" />
```

---

## 8. Menu Configuration

### 8.1. Update Menu Contributor

**Location**: `modules/partner/src/iOne.Partner.Application/Navigation/PartnerMenuContributor.cs`

**Cần thêm**:
```csharp
using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using iOne.Partner.Localization;
using iOne.Partner.Permissions;

namespace iOne.Partner.Navigation;

public class PartnerMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var partnerL = context.GetLocalizer<PartnerResource>();

        // Partner Menu
        var partnerMenuItem = new ApplicationMenuItem(
            "Partner",
            partnerL["Menu:Partner"],
            icon: "pi pi-fw pi-handshake"
        );
        context.Menu.AddItem(partnerMenuItem);

        // ResPartnerType Menu Item
        partnerMenuItem.AddItem(new ApplicationMenuItem(
            "Partner.ResPartnerType",
            partnerL["Menu:ResPartnerType"],
            url: "~/pages/partner/partner-types",
            icon: "pi pi-fw pi-tag"
        ).RequirePermissions(ResPartnerTypePermissions.Default));

        await Task.CompletedTask;
    }
}
```

**Đặc điểm**:
- Menu item có permission check bằng `RequirePermissions()`
- URL: `"~/pages/partner/partner-types"`

---

## 9. Checklist Trước Khi Hoàn Thành

### 9.1. Domain Layer
- [ ] Enum `ResPartnerTypeStatus` đã tạo (Active, Deactive)
- [ ] Entity `ResPartnerType` đã tạo với:
  - [ ] Table name: `res_partner_type` (snake_case)
  - [ ] Code: `private set`, validation Regex `^[A-Z0-9_]+$`
  - [ ] Code max length: 25
  - [ ] Name max length: 250
  - [ ] **Không có method `UpdateCode()`**
- [ ] Repository interface `IResPartnerTypeRepository` đã tạo
- [ ] Manager `ResPartnerTypeManager` đã tạo với:
  - [ ] `CreateAsync`: Validate Code uniqueness
  - [ ] `UpdateAsync`: Chỉ update Name và Status

### 9.2. EF Core Layer
- [ ] Entity Configuration đã tạo với:
  - [ ] Table name: `res_partner_type` (snake_case)
  - [ ] Column names: `code`, `name`, `status` (snake_case)
  - [ ] Status conversion: Enum → string (lowercase)
  - [ ] Index: `ix_res_partner_type_code` (unique)
  - [ ] Audit columns: snake_case
- [ ] Repository implementation `EfCoreResPartnerTypeRepository` đã tạo
- [ ] Đã register trong `iOneDbContext`
- [ ] Migration đã tạo và test (với IF EXISTS/IF NOT EXISTS)

### 9.3. Application Layer
- [ ] DTOs đã tạo:
  - [ ] `ResPartnerTypeDto`
  - [ ] `CreateResPartnerTypeDto` (có Code với Regex validation)
  - [ ] `UpdateResPartnerTypeDto` (**KHÔNG có Code**)
  - [ ] `GetResPartnerTypesInput` (filter theo Code, Name, Status)
- [ ] Application Service interface `IResPartnerTypeAppService` đã tạo
- [ ] Application Service implementation `ResPartnerTypeAppService` đã tạo với:
  - [ ] `CreateAsync`: Tạo entity mới
  - [ ] `UpdateAsync`: Chỉ update Name và Status
  - [ ] `DeleteAsync`: **Delete trước** → **Sau đó set Status = Deactive**
  - [ ] `CreateFilteredQueryAsync`: Filter theo Code, Name, Status
- [ ] **AutoMapper configuration đã được thêm** (Entity → DTO, CreateDto → Entity, UpdateDto → Entity)

### 9.4. Permissions
- [ ] Permission Constants `ResPartnerTypePermissions` đã tạo
- [ ] Permission Definition Provider `ResPartnerTypePermissionDefinitionProvider` đã tạo
- [ ] Permissions đã được sử dụng trong AppService và Controller

### 9.5. Localization
- [ ] Localization keys đã thêm vào `vi-VN.json`
- [ ] Localization keys đã thêm vào `en.json`
- [ ] Keys phải giống nhau ở cả 2 file

### 9.6. HTTP API
- [ ] Controller `ResPartnerTypeController` đã tạo với đầy đủ endpoints
- [ ] **Conventional Controllers đã được config để exclude `ResPartnerTypeAppService`**
- [ ] **ProjectReference đến `iOne.Partner.Application.csproj` đã được thêm vào `iOne.Partner.HttpApi.csproj`**

### 9.7. Menu
- [ ] Menu Contributor đã được update với menu item cho ResPartnerType
- [ ] Menu item có permission check

### 9.8. Testing
- [ ] Build solution thành công
- [ ] Test API endpoints thành công:
  - [ ] GET `/api/partner/partner-types` (list với filter)
  - [ ] GET `/api/partner/partner-types/{id}` (get by id)
  - [ ] POST `/api/partner/partner-types` (create)
  - [ ] PUT `/api/partner/partner-types/{id}` (update - Code không được sửa)
  - [ ] DELETE `/api/partner/partner-types/{id}` (delete - Status = Deactive)
- [ ] Test validation:
  - [ ] Code chỉ cho phép A-Z, 0-9, _
  - [ ] Code là unique
  - [ ] Code không được sửa khi update
- [ ] Test audit log:
  - [ ] Khi Delete: Có audit log với ChangeType = Deleted
  - [ ] Sau đó Status = Deactive
- [ ] Menu hiển thị đúng trên client

---

## 10. Lưu Ý Quan Trọng

### 10.1. Code Validation
- Code chỉ cho phép: **A-Z, 0-9, _** (uppercase)
- Validation ở cả Entity (`SetCode`) và DTO (`RegularExpression` attribute)
- Regex pattern: `^[A-Z0-9_]+$`

### 10.2. Code Immutability
- Code là `private set` trong Entity
- **Không có method `UpdateCode()`** trong Entity
- `UpdateResPartnerTypeDto` **không có Code property**
- Manager `UpdateAsync` **không có parameter Code**

### 10.3. Soft Delete với Status Deactive
- **QUAN TRỌNG**: Khi Delete:
  1. Gọi `Repository.DeleteAsync()` **trước** để trigger audit log (soft delete)
  2. Sau đó set `Status = Deactive` để đảm bảo business logic
- Entity vẫn tồn tại trong database (`IsDeleted = true`)
- Audit log sẽ có 2 entries:
  - Entry 1: ChangeType = Deleted (từ `DeleteAsync()`)
  - Entry 2: ChangeType = Updated (từ `UpdateStatus()`)

### 10.4. Table và Column Naming
- **QUAN TRỌNG**: Table và column names phải theo **snake_case** (PostgreSQL convention)
- Table: `res_partner_type` (snake_case)
- Columns: `code`, `name`, `status`, `creation_time`, `creator_id`, etc. (snake_case)
- Index: `ix_res_partner_type_code` (snake_case với prefix)
- Primary key: `pk_res_partner_type` (snake_case với prefix)

### 10.5. Status Enum
- Enum trong code: `Active`, `Deactive`
- String trong DB: `"active"`, `"deactive"` (lowercase)
- Sử dụng `HasConversion<string>()` trong EF Core configuration

### 10.6. Migration Safety
- **LUÔN** dùng `IF EXISTS` khi DROP
- **LUÔN** dùng `IF NOT EXISTS` khi CREATE
- Đảm bảo migration có thể chạy trên database mới hoặc đã có dữ liệu

### 10.7. AutoMapper Configuration
- **BẮT BUỘC**: Phải thêm mapping trong AutoMapper Profile
- Nếu thiếu mapping, sẽ gặp lỗi: `"Missing type map configuration or unsupported mapping"`

### 10.8. Tránh Duplicate API
- **QUAN TRỌNG**: Nếu có manual controller, phải exclude AppService khỏi conventional controller generation
- Configuration phải được đặt trong **module HttpApi**

---

## 11. Thứ Tự Thực Hiện

1. **Domain Layer** (Enum, Entity, Repository Interface, Manager)
2. **EF Core Layer** (Configuration, Repository Implementation, Migration)
3. **Application Layer** (DTOs, Application Service, AutoMapper)
4. **Permissions** (Constants, Definition Provider)
5. **Localization** (vi-VN và en)
6. **HTTP API** (Controller, Exclude từ Conventional Controllers)
7. **Menu** (Update Menu Contributor)
8. **Testing** (Build, Test API, Test validation, Test audit log)

---

## 12. Tài Liệu Tham Khảo

- `RULES_BACKEND_DEVELOPMENT.md`: Quy tắc chung khi tạo backend
- `HrEmployeeRole`: Implementation mẫu trong module HR (tương tự về Code immutability và soft delete)
- `HrDepartmentType`: Implementation mẫu trong module HR
- `HrEmployeePosition`: Implementation mẫu về soft delete với status deactive

---

## 13. So Sánh với DLL Database

**DLL Database**:
- Table: `RESPARTNERTYPE` (UPPERCASE)
- ID: `VARCHAR(36)`
- CODE: `VARCHAR(25)`
- NAME: `VARCHAR(250)`
- STATUS: `VARCHAR(10)` (values: `"active"`, `"deactive"`)

**Implementation**:
- Table: `res_partner_type` (snake_case - PostgreSQL convention)
- ID: `Guid` (uuid trong PostgreSQL)
- CODE: `VARCHAR(25)` → `character varying(25)`
- NAME: `VARCHAR(250)` → `character varying(250)`
- STATUS: `VARCHAR(10)` → `character varying(10)` (values: `"active"`, `"deactive"`)

**Lưu ý**: 
- Table và column names được chuyển sang snake_case để phù hợp với PostgreSQL convention
- ID được chuyển từ VARCHAR(36) sang Guid (uuid) để phù hợp với ABP Framework convention

