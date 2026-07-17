# Plan Xây Dựng Backend: Danh mục Cấp bậc nhân viên (HrEmployeeLevel)

## Tổng Quan

**Module**: HR  
**Entity**: HrEmployeeLevel  
**Table**: HREMPLOYEELEVEL  
**Chức năng**: CRUD (Create, Read, Update, Delete) với các ràng buộc đặc biệt

## Yêu Cầu Đặc Biệt

1. ✅ **Mã cấp bậc (Code)**: Chỉ cho phép ký tự A-Z, _, 0-9
2. ✅ **Mã cấp bậc là duy nhất**: Không được trùng
3. ✅ **Khi sửa không được sửa Code**: Code chỉ set khi tạo mới
4. ✅ **Khi xóa**: Set status = Deactive (soft delete) và đảm bảo audit log
5. ✅ **Đa ngôn ngữ**: Hỗ trợ vi-VN và en
6. ✅ **Phân quyền**: View, Create, Edit, Delete

## Cấu Trúc Database

```sql
CREATE TABLE HREMPLOYEELEVEL (
   ID                   CHAR(36)             NOT NULL,
   CODE                 VARCHAR(50)          NOT NULL,
   NAME                 VARCHAR(250)         NOT NULL,
   STATUS               VARCHAR(10)          NOT NULL,
   DESCRIPTION          VARCHAR(500)         NULL,
   CREATIONTIME         DATE                 NOT NULL,
   CREATORID            VARCHAR(50)          NOT NULL,
   LASTMODIFICATIONTIME DATE                 NULL,
   LASTMODIFIERID       VARCHAR(50)          NULL,
   CONSTRAINT PK_HREMPLOYEELEVEL PRIMARY KEY (ID)
);

COMMENT ON TABLE HREMPLOYEELEVEL IS 'Bảng lưu phân bậc nhân viên';
COMMENT ON COLUMN HREMPLOYEELEVEL.STATUS IS 'Trạng thái: - active: hoạt động - deactive: không hoạt động';
```

**Lưu ý**: ABP Framework sẽ tự động thêm các trường audit (IsDeleted, DeleterId, DeletionTime, ExtraProperties, ConcurrencyStamp) thông qua `FullAuditedAggregateRoot<Guid>`.

---

## 1. Domain Layer

### 1.1. Enum: HrEmployeeLevelStatus

**Location**: `src/common/domain/iOne.Domain.Shared/HrEmployeeLevels/HrEmployeeLevelStatus.cs`

**Nội dung**:
```csharp
namespace iOne.HrEmployeeLevels;

public enum HrEmployeeLevelStatus
{
    Active,    // Hoạt động
    Deactive   // Không hoạt động
}
```

**Lưu ý**: Enum sẽ được convert sang string ("active", "deactive") trong database.

---

### 1.2. Entity: HrEmployeeLevel

**Location**: `src/common/domain/iOne.Domain/HrEmployeeLevels/HrEmployeeLevel.cs`

**Nội dung**:
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.HrEmployeeLevels;

[Table("HrEmployeeLevel")]
public class HrEmployeeLevel : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual HrEmployeeLevelStatus Status { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    protected HrEmployeeLevel()
    {
        // For ORM
    }

    public HrEmployeeLevel(Guid id, string code, string name, HrEmployeeLevelStatus status, string? description = null)
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

    private void SetStatus(HrEmployeeLevelStatus status)
    {
        Status = status;
    }

    private void SetDescription(string? description)
    {
        if (description != null && description.Length > 500)
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

    public virtual void UpdateStatus(HrEmployeeLevelStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }
}
```

**Điểm quan trọng**:
- Code là `private set` và chỉ có thể set trong constructor
- Không có method `UpdateCode()` để đảm bảo Code không thể sửa sau khi tạo
- Description là optional (nullable)

---

### 1.3. Repository Interface: IHrEmployeeLevelRepository

**Location**: `src/common/domain/iOne.Domain/HrEmployeeLevels/IHrEmployeeLevelRepository.cs`

**Nội dung**:
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.HrEmployeeLevels;

public interface IHrEmployeeLevelRepository : IRepository<HrEmployeeLevel, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<HrEmployeeLevel?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}
```

---

### 1.4. Manager: HrEmployeeLevelManager

**Location**: `src/common/domain/iOne.Domain/HrEmployeeLevels/HrEmployeeLevelManager.cs`

**Nội dung**:
```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.HrEmployeeLevels;

public class HrEmployeeLevelManager : DomainService
{
    protected IHrEmployeeLevelRepository Repository { get; }

    public HrEmployeeLevelManager(IHrEmployeeLevelRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(HrEmployeeLevel employeeLevel)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(employeeLevel.Code))
        {
            throw new BusinessException("HrEmployeeLevel:CodeExists")
                .WithData("Code", employeeLevel.Code);
        }

        await Repository.InsertAsync(employeeLevel);
    }

    public virtual async Task UpdateAsync(
        HrEmployeeLevel employeeLevel, 
        string name, 
        HrEmployeeLevelStatus status,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        employeeLevel.UpdateName(name);
        employeeLevel.UpdateStatus(status);
        employeeLevel.UpdateDescription(description);
        await Repository.UpdateAsync(employeeLevel);
    }
}
```

---

### 1.5. Đăng ký trong Domain Module

**Location**: `src/common/domain/iOne.Domain/iOneDomainModule.cs`

**Cần thêm**:
```csharp
// Trong ConfigureServices method
context.Services.AddTransient<HrEmployeeLevelManager>();
```

---

### 1.6. Đăng ký trong Domain Shared Module

**Location**: `src/common/domain/iOne.Domain.Shared/iOneDomainSharedModule.cs`

**Cần thêm**: Enum sẽ tự động được expose qua namespace.

---

## 2. Entity Framework Core Layer

### 2.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrEmployeeLevels/HrEmployeeLevelConfiguration.cs`

**Nội dung**:
```csharp
using iOne.HrEmployeeLevels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.HrEmployeeLevels;

public class HrEmployeeLevelConfiguration : IEntityTypeConfiguration<HrEmployeeLevel>
{
    public void Configure(EntityTypeBuilder<HrEmployeeLevel> builder)
    {
        builder.ToTable("HrEmployeeLevel", t =>
        {
            t.HasComment("Bảng lưu phân bậc nhân viên");
        });

        builder.ConfigureByConvention();

        // Configure Code with unique index
        builder.HasIndex(e => e.Code, "IX_HrEmployeeLevel_Code")
            .IsUnique();

        // Configure Status enum -> string conversion
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasComment("Trạng thái: - active: hoạt động - deactive: không hoạt động");

        // Configure Description
        builder.Property(e => e.Description)
            .HasMaxLength(500);
    }
}
```

---

### 2.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrEmployeeLevels/EfCoreHrEmployeeLevelRepository.cs`

**Nội dung**:
```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iOne.HrEmployeeLevels;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.HrEmployeeLevels;

public class EfCoreHrEmployeeLevelRepository : EfCoreRepository<iOneDbContext, HrEmployeeLevel, Guid>,
    IHrEmployeeLevelRepository
{
    public EfCoreHrEmployeeLevelRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        query = query.Where(x => x.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public virtual async Task<HrEmployeeLevel?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }
}
```

---

### 2.3. Đăng ký trong DbContext

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`

**Cần thêm**:
```csharp
public DbSet<HrEmployeeLevel> HrEmployeeLevels { get; set; }
```

---

### 2.4. Đăng ký Repository trong EF Core Module

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneEntityFrameworkCoreModule.cs`

**Cần thêm trong ConfigureServices**:
```csharp
options.AddRepository<HrEmployeeLevel, EfCoreHrEmployeeLevelRepository>();
```

---

### 2.5. Đăng ký Configuration trong DbContext

**Location**: `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContextModelBuilderExtensions.cs` (hoặc trong OnModelCreating của DbContext)

**Cần thêm**:
```csharp
builder.ApplyConfiguration(new HrEmployeeLevelConfiguration());
```

---

### 2.6. Migration

**Lệnh tạo migration**:
```powershell
cd src/common/infra/iOne.EntityFrameworkCore
dotnet ef migrations add AddHrEmployeeLevel --startup-project ../../web/iOne.HttpApi.Host
```

**Lưu ý QUAN TRỌNG**: 
- Migration phải sử dụng `IF EXISTS` khi DROP và `IF NOT EXISTS` khi CREATE
- Table name: `HrEmployeeLevel` (PascalCase)
- Column names: PascalCase (Code, Name, Status, Description)
- Status column: VARCHAR(10) với comment

**Ví dụ migration**:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "HrEmployeeLevel",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
            Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
            Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái: - active: hoạt động - deactive: không hoạt động"),
            Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
            ExtraProperties = table.Column<string>(type: "text", nullable: false),
            ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
            CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
            CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
            LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
            LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
            IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
            DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
            DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_HrEmployeeLevel", x => x.Id);
        },
        comment: "Bảng lưu phân bậc nhân viên");

    migrationBuilder.CreateIndex(
        name: "IX_HrEmployeeLevel_Code",
        table: "HrEmployeeLevel",
        column: "Code",
        unique: true);
}
```

---

## 3. Application Layer

### 3.1. DTOs

#### 3.1.1. HrEmployeeLevelDto

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeeLevels/HrEmployeeLevelDto.cs`

**Nội dung**:
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeeLevels;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeeLevels;

public class HrEmployeeLevelDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "HrEmployeeLevel:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "HrEmployeeLevel:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "HrEmployeeLevel:Status")]
    public HrEmployeeLevelStatus Status { get; set; }

    [Display(Name = "HrEmployeeLevel:Description")]
    public string? Description { get; set; }
}
```

---

#### 3.1.2. CreateHrEmployeeLevelDto

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeeLevels/CreateHrEmployeeLevelDto.cs`

**Nội dung**:
```csharp
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeeLevels;

namespace iOne.Hr.HrEmployeeLevels;

public class CreateHrEmployeeLevelDto
{
    [Required(ErrorMessage = "HrEmployeeLevel:CodeRequired")]
    [StringLength(50, ErrorMessage = "HrEmployeeLevel:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "HrEmployeeLevel:CodeInvalidFormat")]
    [Display(Name = "HrEmployeeLevel:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeeLevel:NameRequired")]
    [StringLength(250, ErrorMessage = "HrEmployeeLevel:NameMaxLength")]
    [Display(Name = "HrEmployeeLevel:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeeLevel:StatusRequired")]
    [Display(Name = "HrEmployeeLevel:Status")]
    public HrEmployeeLevelStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "HrEmployeeLevel:DescriptionMaxLength")]
    [Display(Name = "HrEmployeeLevel:Description")]
    public string? Description { get; set; }
}
```

---

#### 3.1.3. UpdateHrEmployeeLevelDto

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeeLevels/UpdateHrEmployeeLevelDto.cs`

**Nội dung**:
```csharp
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeeLevels;

namespace iOne.Hr.HrEmployeeLevels;

public class UpdateHrEmployeeLevelDto
{
    // ⚠️ QUAN TRỌNG: Không có Code - Code không được phép sửa

    [Required(ErrorMessage = "HrEmployeeLevel:NameRequired")]
    [StringLength(250, ErrorMessage = "HrEmployeeLevel:NameMaxLength")]
    [Display(Name = "HrEmployeeLevel:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeeLevel:StatusRequired")]
    [Display(Name = "HrEmployeeLevel:Status")]
    public HrEmployeeLevelStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "HrEmployeeLevel:DescriptionMaxLength")]
    [Display(Name = "HrEmployeeLevel:Description")]
    public string? Description { get; set; }
}
```

---

#### 3.1.4. GetHrEmployeeLevelsInput

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeeLevels/GetHrEmployeeLevelsInput.cs`

**Nội dung**:
```csharp
using iOne.HrEmployeeLevels;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeeLevels;

public class GetHrEmployeeLevelsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public HrEmployeeLevelStatus? Status { get; set; }
}
```

---

### 3.2. Application Service Interface

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeeLevels/IHrEmployeeLevelAppService.cs`

**Nội dung**:
```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Hr.HrEmployeeLevels;

public interface IHrEmployeeLevelAppService : ICrudAppService<
    HrEmployeeLevelDto,
    Guid,
    GetHrEmployeeLevelsInput,
    CreateHrEmployeeLevelDto,
    UpdateHrEmployeeLevelDto>
{
}
```

---

### 3.3. Application Service Implementation

**Location**: `modules/hr/src/iOne.Hr.Application/HrEmployeeLevels/HrEmployeeLevelAppService.cs`

**Nội dung**:
```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrEmployeeLevels;
using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using iOne.HrEmployeeLevels; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp;

namespace iOne.Hr.HrEmployeeLevels;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Authorize(HrEmployeeLevelPermissions.Default)]
public class HrEmployeeLevelAppService : CrudAppService<
    HrEmployeeLevel,
    HrEmployeeLevelDto,
    Guid,
    GetHrEmployeeLevelsInput,
    CreateHrEmployeeLevelDto,
    UpdateHrEmployeeLevelDto>, IHrEmployeeLevelAppService
{
    protected HrEmployeeLevelManager Manager { get; }

    public HrEmployeeLevelAppService(
        IHrEmployeeLevelRepository repository,
        HrEmployeeLevelManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(HrResource);
        GetPolicyName = HrEmployeeLevelPermissions.View;
        GetListPolicyName = HrEmployeeLevelPermissions.View;
        CreatePolicyName = HrEmployeeLevelPermissions.Create;
        UpdatePolicyName = HrEmployeeLevelPermissions.Edit;
        DeletePolicyName = HrEmployeeLevelPermissions.Delete;
    }

    public override async Task<HrEmployeeLevelDto> CreateAsync(CreateHrEmployeeLevelDto input)
    {
        var entity = new HrEmployeeLevel(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status,
            input.Description
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<HrEmployeeLevel, HrEmployeeLevelDto>(entity);
    }

    public override async Task<HrEmployeeLevelDto> UpdateAsync(Guid id, UpdateHrEmployeeLevelDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        await Manager.UpdateAsync(entity, input.Name, input.Status, input.Description);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<HrEmployeeLevel, HrEmployeeLevelDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        
        // Delete to trigger audit logging with ChangeType = Deleted
        // FullAuditedAggregateRoot uses soft delete, so entity is not actually removed from database
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
        
        // Update status to Deactive (entity still exists in database due to soft delete)
        entity.UpdateStatus(HrEmployeeLevelStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<HrEmployeeLevel>> CreateFilteredQueryAsync(GetHrEmployeeLevelsInput input)
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

**Điểm quan trọng**:
- `DeleteAsync`: Thực hiện soft delete (set IsDeleted = true) và sau đó set Status = Deactive để đảm bảo audit log và business logic
- `UpdateAsync`: Không có Code trong input, đảm bảo Code không thể sửa

---

### 3.4. AutoMapper Configuration

**Location**: `modules/hr/src/iOne.Hr.Application/iOneHrApplicationAutoMapperProfile.cs`

**Cần thêm**:
```csharp
CreateMap<HrEmployeeLevel, HrEmployeeLevelDto>();
CreateMap<CreateHrEmployeeLevelDto, HrEmployeeLevel>();
CreateMap<UpdateHrEmployeeLevelDto, HrEmployeeLevel>();
```

---

## 4. Permissions

### 4.1. Permission Constants

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrEmployeeLevelPermissions.cs`

**Nội dung**:
```csharp
namespace iOne.Hr.Permissions;

public static class HrEmployeeLevelPermissions
{
    public const string GroupName = "HrEmployeeLevel";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

---

### 4.2. Permission Definition Provider

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrEmployeeLevelPermissionDefinitionProvider.cs`

**Nội dung**:
```csharp
using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Hr.Permissions;

public class HrEmployeeLevelPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var employeeLevelGroup = context.AddGroup(
            HrEmployeeLevelPermissions.GroupName,
            L("Permission:HrEmployeeLevel")
        );

        var employeeLevelPermission = employeeLevelGroup.AddPermission(
            HrEmployeeLevelPermissions.Default,
            L("Permission:HrEmployeeLevel")
        );

        employeeLevelPermission.AddChild(
            HrEmployeeLevelPermissions.Create,
            L("Permission:Create")
        );

        employeeLevelPermission.AddChild(
            HrEmployeeLevelPermissions.Edit,
            L("Permission:Edit")
        );

        employeeLevelPermission.AddChild(
            HrEmployeeLevelPermissions.Delete,
            L("Permission:Delete")
        );

        employeeLevelPermission.AddChild(
            HrEmployeeLevelPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HrResource>(name);
    }
}
```

---

### 4.3. Đăng ký Permission Provider

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/iOneHrApplicationContractsModule.cs`

**Cần thêm trong ConfigureServices**:
```csharp
context.Services.Configure<PermissionOptions>(options =>
{
    options.DefinitionProviders.Add<HrEmployeeLevelPermissionDefinitionProvider>();
});
```

---

## 5. Localization

### 5.1. Localization File (vi-VN)

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Localization/Hr/vi-VN.json`

**Cần thêm các keys sau**:
```json
{
  "Culture": "vi-VN",
  "Texts": {
    "Menu:EmployeeLevels": "Danh mục Cấp bậc nhân viên",
    "Permission:HrEmployeeLevel": "Danh mục Cấp bậc nhân viên",
    "HrEmployeeLevel:Code": "Mã cấp bậc",
    "HrEmployeeLevel:Name": "Tên cấp bậc",
    "HrEmployeeLevel:Status": "Trạng thái",
    "HrEmployeeLevel:Description": "Mô tả",
    "HrEmployeeLevel:New": "Thêm mới",
    "HrEmployeeLevel:Edit": "Chỉnh sửa",
    "HrEmployeeLevel:Delete": "Xóa",
    "HrEmployeeLevel:SearchByCode": "Tìm theo mã",
    "HrEmployeeLevel:SearchByName": "Tìm theo tên",
    "HrEmployeeLevel:Active": "Hoạt động",
    "HrEmployeeLevel:Deactive": "Không hoạt động",
    "HrEmployeeLevel:CodeRequired": "Mã cấp bậc không được để trống",
    "HrEmployeeLevel:CodeMaxLength": "Mã cấp bậc không được vượt quá {0} ký tự",
    "HrEmployeeLevel:CodeInvalidFormat": "Mã cấp bậc chỉ được chứa chữ cái in hoa (A-Z), số (0-9) và dấu gạch dưới (_)",
    "HrEmployeeLevel:CodeCannotBeChanged": "Mã cấp bậc không thể thay đổi sau khi tạo",
    "HrEmployeeLevel:CodeExists": "Mã cấp bậc '{Code}' đã tồn tại",
    "HrEmployeeLevel:NameRequired": "Tên cấp bậc không được để trống",
    "HrEmployeeLevel:NameMaxLength": "Tên cấp bậc không được vượt quá {0} ký tự",
    "HrEmployeeLevel:StatusRequired": "Trạng thái không được để trống",
    "HrEmployeeLevel:Status:Active": "Hoạt động",
    "HrEmployeeLevel:Status:Deactive": "Không hoạt động",
    "HrEmployeeLevel:DescriptionMaxLength": "Mô tả không được vượt quá {0} ký tự",
    "HrEmployeeLevel:CreatedSuccessfully": "Tạo mới thành công",
    "HrEmployeeLevel:UpdatedSuccessfully": "Cập nhật thành công",
    "HrEmployeeLevel:DeletedSuccessfully": "Xóa thành công",
    "HrEmployeeLevel:DeleteConfirm": "Bạn có chắc chắn muốn xóa bản ghi này?"
  }
}
```

---

### 5.2. Localization File (en)

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Localization/Hr/en.json`

**Cần thêm các keys sau**:
```json
{
  "Culture": "en",
  "Texts": {
    "Menu:EmployeeLevels": "Employee Level",
    "Permission:HrEmployeeLevel": "Employee Level",
    "HrEmployeeLevel:Code": "Code",
    "HrEmployeeLevel:Name": "Name",
    "HrEmployeeLevel:Status": "Status",
    "HrEmployeeLevel:Description": "Description",
    "HrEmployeeLevel:New": "New",
    "HrEmployeeLevel:Edit": "Edit",
    "HrEmployeeLevel:Delete": "Delete",
    "HrEmployeeLevel:SearchByCode": "Search by code",
    "HrEmployeeLevel:SearchByName": "Search by name",
    "HrEmployeeLevel:Active": "Active",
    "HrEmployeeLevel:Deactive": "Inactive",
    "HrEmployeeLevel:CodeRequired": "Code is required",
    "HrEmployeeLevel:CodeMaxLength": "Code cannot exceed {0} characters",
    "HrEmployeeLevel:CodeInvalidFormat": "Code can only contain uppercase letters (A-Z), numbers (0-9) and underscore (_)",
    "HrEmployeeLevel:CodeCannotBeChanged": "Code cannot be changed after creation",
    "HrEmployeeLevel:CodeExists": "Code '{Code}' already exists",
    "HrEmployeeLevel:NameRequired": "Name is required",
    "HrEmployeeLevel:NameMaxLength": "Name cannot exceed {0} characters",
    "HrEmployeeLevel:StatusRequired": "Status is required",
    "HrEmployeeLevel:Status:Active": "Active",
    "HrEmployeeLevel:Status:Deactive": "Inactive",
    "HrEmployeeLevel:DescriptionMaxLength": "Description cannot exceed {0} characters",
    "HrEmployeeLevel:CreatedSuccessfully": "Created successfully",
    "HrEmployeeLevel:UpdatedSuccessfully": "Updated successfully",
    "HrEmployeeLevel:DeletedSuccessfully": "Deleted successfully",
    "HrEmployeeLevel:DeleteConfirm": "Are you sure you want to delete this record?"
  }
}
```

---

## 6. HTTP API Controllers

### 6.1. Controller

**Location**: `modules/hr/src/iOne.Hr.HttpApi/Controllers/HrEmployeeLevelController.cs`

**Nội dung**:
```csharp
using System;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrEmployeeLevels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Hr.Controllers;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Area(HrRemoteServiceConsts.ModuleName)]
[Route("api/hr/employee-levels")]
[Authorize]
public class HrEmployeeLevelController : AbpControllerBase
{
    protected IHrEmployeeLevelAppService AppService { get; }

    public HrEmployeeLevelController(IHrEmployeeLevelAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<HrEmployeeLevelDto>> GetListAsync(GetHrEmployeeLevelsInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<HrEmployeeLevelDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<HrEmployeeLevelDto> CreateAsync(CreateHrEmployeeLevelDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<HrEmployeeLevelDto> UpdateAsync(Guid id, UpdateHrEmployeeLevelDto input)
    {
        return AppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return AppService.DeleteAsync(id);
    }
}
```

---

## 7. Checklist Trước Khi Hoàn Thành

### Domain Layer
- [ ] Enum `HrEmployeeLevelStatus` đã tạo
- [ ] Entity `HrEmployeeLevel` đã tạo với validation Code (A-Z, 0-9, _)
- [ ] Entity không có method `UpdateCode()` (Code không thể sửa)
- [ ] Repository interface `IHrEmployeeLevelRepository` đã tạo
- [ ] Manager `HrEmployeeLevelManager` đã tạo với validation Code uniqueness
- [ ] Manager `UpdateAsync` không có parameter Code
- [ ] Đăng ký Manager trong Domain Module

### Entity Framework Core Layer
- [ ] Entity Configuration đã tạo với unique index cho Code
- [ ] Repository Implementation đã tạo
- [ ] Đăng ký Repository trong EF Core Module
- [ ] Đăng ký DbSet trong DbContext
- [ ] Đăng ký Configuration trong DbContext
- [ ] Migration đã tạo với IF EXISTS/IF NOT EXISTS
- [ ] Migration đã test trên database mới và database có dữ liệu

### Application Layer
- [ ] DTOs đã tạo đầy đủ (Dto, CreateDto, UpdateDto, GetInput)
- [ ] `UpdateHrEmployeeLevelDto` không có Code
- [ ] Application Service Interface đã tạo
- [ ] Application Service Implementation đã tạo
- [ ] `DeleteAsync` thực hiện soft delete và set Status = Deactive
- [ ] `UpdateAsync` không cho phép sửa Code
- [ ] AutoMapper configuration đã thêm

### Permissions
- [ ] Permission Constants đã tạo với GroupName = "HrEmployeeLevel"
- [ ] Permission Definition Provider đã tạo
- [ ] Đăng ký Permission Provider trong Application Contracts Module

### Localization
- [ ] Localization keys đã thêm vào vi-VN.json
- [ ] Localization keys đã thêm vào en.json
- [ ] Keys phải match nhau giữa 2 file

### HTTP API
- [ ] Controller đã tạo với route `api/hr/employee-levels`
- [ ] Controller có đầy đủ CRUD endpoints
- [ ] Controller có authorization attributes

### Testing
- [ ] Build solution thành công
- [ ] Test API endpoints thành công
- [ ] Test validation Code format (A-Z, 0-9, _)
- [ ] Test Code uniqueness
- [ ] Test không thể sửa Code khi Update
- [ ] Test Delete set Status = Deactive và có audit log
- [ ] Test filter/search functionality

---

## 8. Lưu Ý Quan Trọng

1. **Code Validation**: 
   - Code chỉ cho phép A-Z, 0-9, _ (uppercase)
   - Validation ở cả Entity (SetCode) và DTO (RegularExpression attribute)

2. **Code Immutability**: 
   - Code là `private set` trong Entity
   - Không có method `UpdateCode()` trong Entity
   - `UpdateHrEmployeeLevelDto` không có Code property
   - Manager `UpdateAsync` không có parameter Code

3. **Soft Delete với Status Deactive**:
   - Khi Delete: Gọi `Repository.DeleteAsync()` để trigger audit log (soft delete)
   - Sau đó set `Status = Deactive` để đảm bảo business logic
   - Entity vẫn tồn tại trong database (IsDeleted = true)

4. **Table và Column Naming**:
   - Table: `HrEmployeeLevel` (PascalCase)
   - Columns: `Code`, `Name`, `Status`, `Description` (PascalCase)
   - ABP sẽ tự động map sang database convention nếu cần

5. **Status Enum**:
   - Enum trong code: `Active`, `Deactive`
   - String trong DB: `"active"`, `"deactive"` (lowercase)
   - Sử dụng `HasConversion<string>()` trong EF Core configuration

6. **Audit Log**:
   - ABP Framework tự động tạo audit log thông qua `FullAuditedAggregateRoot<Guid>`
   - Khi Delete, audit log sẽ có ChangeType = Deleted
   - Khi Update Status, audit log sẽ có ChangeType = Updated

---

## 9. Thứ Tự Thực Hiện

1. **Domain Layer** (Entity, Enum, Repository Interface, Manager)
2. **EF Core Layer** (Configuration, Repository Implementation, Migration)
3. **Application Layer** (DTOs, Application Service)
4. **Permissions** (Constants, Definition Provider)
5. **Localization** (vi-VN và en)
6. **HTTP API** (Controller)
7. **Testing** (Build, Test API, Test validation)

---

## 10. Tài Liệu Tham Khảo

- `RULES_BACKEND_DEVELOPMENT.md`: Quy tắc chung khi tạo backend
- `HrDepartmentType`: Implementation mẫu trong module HR
- `HrEmployeeRole`: Implementation mẫu tương tự trong module HR

