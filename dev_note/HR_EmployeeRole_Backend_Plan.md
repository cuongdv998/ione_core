# Plan: Xây Dựng Backend Danh Mục Vai Trò Nhân Viên (HrEmployeeRole)

## Tổng Quan

Xây dựng backend cho module **Danh mục Vai trò nhân viên** trong module HR với các tính năng:
- CRUD: Xem, Thêm, Sửa, Xóa
- Validation: Mã vai trò chỉ chứa A-Z, _, 0-9
- Business Rules: Code không được sửa khi update, Code là unique
- Soft Delete: Khi xóa thì set Status = Deactive (không hard delete)
- Audit Log: Tự động từ ABP Framework
- Đa ngôn ngữ: vi-VN và en
- Phân quyền: View, Create, Edit, Delete

## 1. Domain Layer

### 1.1. Enum: HrEmployeeRoleStatus

**Location**: `src/common/domain/iOne.Domain.Shared/HrEmployeeRoles/HrEmployeeRoleStatus.cs`

**Nội dung**:
```csharp
namespace iOne.HrEmployeeRoles;

public enum HrEmployeeRoleStatus
{
    Active,    // Hoạt động
    Deactive   // Không hoạt động
}
```

**Lưu ý**: 
- Enum values sẽ được convert sang string trong database: "active", "deactive"
- MaxLength(10) trong EF Core configuration

---

### 1.2. Entity: HrEmployeeRole

**Location**: `src/common/domain/iOne.Domain/HrEmployeeRoles/HrEmployeeRole.cs`

**Nội dung**:
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.HrEmployeeRoles;

[Table("HrEmployeeRole")]
public class HrEmployeeRole : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual HrEmployeeRoleStatus Status { get; private set; }

    protected HrEmployeeRole()
    {
        // For ORM
    }

    public HrEmployeeRole(Guid id, string code, string name, HrEmployeeRoleStatus status)
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

    private void SetStatus(HrEmployeeRoleStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(HrEmployeeRoleStatus status)
    {
        SetStatus(status);
    }
}
```

**Đặc điểm**:
- Kế thừa `FullAuditedAggregateRoot<Guid>` (tự động có audit fields)
- Code là `private set` và chỉ có thể set qua constructor
- Không có method `UpdateCode()` - đảm bảo Code không thể sửa
- Validation Code format: `^[A-Z0-9_]+$` (chỉ A-Z, 0-9, _)

---

### 1.3. Repository Interface

**Location**: `src/common/domain/iOne.Domain/HrEmployeeRoles/IHrEmployeeRoleRepository.cs`

**Nội dung**:
```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.HrEmployeeRoles;

public interface IHrEmployeeRoleRepository : IRepository<HrEmployeeRole, Guid>
{
    Task<bool> IsCodeExistsAsync(string code);
}
```

**Lưu ý**: 
- Method `IsCodeExistsAsync` để check Code uniqueness

---

### 1.4. Manager: HrEmployeeRoleManager

**Location**: `src/common/domain/iOne.Domain/HrEmployeeRoles/HrEmployeeRoleManager.cs`

**Nội dung**:
```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.HrEmployeeRoles;

public class HrEmployeeRoleManager : DomainService
{
    protected IHrEmployeeRoleRepository Repository { get; }

    public HrEmployeeRoleManager(IHrEmployeeRoleRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(HrEmployeeRole employeeRole)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(employeeRole.Code))
        {
            throw new BusinessException("HrEmployeeRole:CodeExists")
                .WithData("Code", employeeRole.Code);
        }

        await Repository.InsertAsync(employeeRole);
    }

    public virtual async Task UpdateAsync(HrEmployeeRole employeeRole, string name, HrEmployeeRoleStatus status)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa
        employeeRole.UpdateName(name);
        employeeRole.UpdateStatus(status);
        await Repository.UpdateAsync(employeeRole);
    }
}
```

**Đặc điểm**:
- `CreateAsync`: Validate Code uniqueness
- `UpdateAsync`: Chỉ cho phép update Name và Status, không có Code parameter

---

## 2. Application Layer

### 2.1. DTOs

#### 2.1.1. HrEmployeeRoleDto

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeeRoles/HrEmployeeRoleDto.cs`

**Nội dung**:
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeeRoles;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeeRoles;

public class HrEmployeeRoleDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "HrEmployeeRole:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "HrEmployeeRole:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "HrEmployeeRole:Status")]
    public HrEmployeeRoleStatus Status { get; set; }
}
```

---

#### 2.1.2. CreateHrEmployeeRoleDto

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeeRoles/CreateHrEmployeeRoleDto.cs`

**Nội dung**:
```csharp
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeeRoles;

namespace iOne.Hr.HrEmployeeRoles;

public class CreateHrEmployeeRoleDto
{
    [Required(ErrorMessage = "HrEmployeeRole:CodeRequired")]
    [StringLength(50, ErrorMessage = "HrEmployeeRole:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "HrEmployeeRole:CodeInvalidFormat")]
    [Display(Name = "HrEmployeeRole:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeeRole:NameRequired")]
    [StringLength(250, ErrorMessage = "HrEmployeeRole:NameMaxLength")]
    [Display(Name = "HrEmployeeRole:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeeRole:StatusRequired")]
    [Display(Name = "HrEmployeeRole:Status")]
    public HrEmployeeRoleStatus Status { get; set; }
}
```

**Lưu ý**: 
- Có validation `RegularExpression` cho Code format

---

#### 2.1.3. UpdateHrEmployeeRoleDto

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeeRoles/UpdateHrEmployeeRoleDto.cs`

**Nội dung**:
```csharp
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeeRoles;

namespace iOne.Hr.HrEmployeeRoles;

public class UpdateHrEmployeeRoleDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "HrEmployeeRole:NameRequired")]
    [StringLength(250, ErrorMessage = "HrEmployeeRole:NameMaxLength")]
    [Display(Name = "HrEmployeeRole:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeeRole:StatusRequired")]
    [Display(Name = "HrEmployeeRole:Status")]
    public HrEmployeeRoleStatus Status { get; set; }
}
```

**Đặc điểm**: 
- **KHÔNG có Code property** - đảm bảo Code không thể sửa

---

#### 2.1.4. GetHrEmployeeRolesInput

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeeRoles/GetHrEmployeeRolesInput.cs`

**Nội dung**:
```csharp
using iOne.HrEmployeeRoles;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeeRoles;

public class GetHrEmployeeRolesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    
    public string? Name { get; set; }
    
    public HrEmployeeRoleStatus? Status { get; set; }
}
```

---

### 2.2. Application Service Interface

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeeRoles/IHrEmployeeRoleAppService.cs`

**Nội dung**:
```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace iOne.Hr.HrEmployeeRoles;

public interface IHrEmployeeRoleAppService : ICrudAppService<
    HrEmployeeRoleDto,
    Guid,
    GetHrEmployeeRolesInput,
    CreateHrEmployeeRoleDto,
    UpdateHrEmployeeRoleDto>
{
}
```

---

### 2.3. Application Service Implementation

**Location**: `modules/hr/src/iOne.Hr.Application/HrEmployeeRoles/HrEmployeeRoleAppService.cs`

**Nội dung**:
```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrEmployeeRoles;
using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using iOne.HrEmployeeRoles; // For Entity, Manager, Repository
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Hr.HrEmployeeRoles;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Authorize(HrEmployeeRolePermissions.Default)]
public class HrEmployeeRoleAppService : CrudAppService<
    HrEmployeeRole,
    HrEmployeeRoleDto,
    Guid,
    GetHrEmployeeRolesInput,
    CreateHrEmployeeRoleDto,
    UpdateHrEmployeeRoleDto>, IHrEmployeeRoleAppService
{
    protected HrEmployeeRoleManager Manager { get; }

    public HrEmployeeRoleAppService(
        IHrEmployeeRoleRepository repository,
        HrEmployeeRoleManager manager)
        : base(repository)
    {
        Manager = manager;
        LocalizationResource = typeof(HrResource);
        GetPolicyName = HrEmployeeRolePermissions.View;
        GetListPolicyName = HrEmployeeRolePermissions.View;
        CreatePolicyName = HrEmployeeRolePermissions.Create;
        UpdatePolicyName = HrEmployeeRolePermissions.Edit;
        DeletePolicyName = HrEmployeeRolePermissions.Delete;
    }

    public override async Task<HrEmployeeRoleDto> CreateAsync(CreateHrEmployeeRoleDto input)
    {
        var entity = new HrEmployeeRole(
            GuidGenerator.Create(),
            input.Code,
            input.Name,
            input.Status
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<HrEmployeeRole, HrEmployeeRoleDto>(entity);
    }

    public override async Task<HrEmployeeRoleDto> UpdateAsync(Guid id, UpdateHrEmployeeRoleDto input)
    {
        // Load entity with tracking to ensure EF Core can detect changes
        var entity = await Repository.GetAsync(id);

        // ⚠️ QUAN TRỌNG: Chỉ update Name và Status, không update Code
        await Manager.UpdateAsync(entity, input.Name, input.Status);

        // Ensure changes are saved (Repository.UpdateAsync should handle this, but explicitly save to ensure audit logging)
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<HrEmployeeRole, HrEmployeeRoleDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        // ⚠️ QUAN TRỌNG: Soft delete - set Status to Deactive instead of hard delete
        var entity = await Repository.GetAsync(id);
        entity.UpdateStatus(HrEmployeeRoleStatus.Deactive);
        await Repository.UpdateAsync(entity);
        
        // Ensure changes are saved to trigger audit logging
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<HrEmployeeRole>> CreateFilteredQueryAsync(GetHrEmployeeRolesInput input)
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
- `DeleteAsync`: **Soft delete** - set Status = Deactive (không hard delete)
- `CreateFilteredQueryAsync`: Filter theo Code, Name, Status

---

## 3. Permissions

### 3.1. Permission Constants

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrEmployeeRolePermissions.cs`

**Nội dung**:
```csharp
namespace iOne.Hr.Permissions;

public static class HrEmployeeRolePermissions
{
    public const string GroupName = "HrEmployeeRole";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

**Lưu ý**: 
- `GroupName` = `"HrEmployeeRole"` (không có prefix "Hr" vì đã nằm trong module Hr)
- `Default` = `GroupName` (không phải `"{GroupName}.Default"`)

---

### 3.2. Permission Definition Provider

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrEmployeeRolePermissionDefinitionProvider.cs`

**Nội dung**:
```csharp
using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Hr.Permissions;

public class HrEmployeeRolePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var employeeRoleGroup = context.AddGroup(
            HrEmployeeRolePermissions.GroupName,
            L("Permission:HrEmployeeRole")
        );

        var employeeRolePermission = employeeRoleGroup.AddPermission(
            HrEmployeeRolePermissions.Default,
            L("Permission:HrEmployeeRole")
        );

        employeeRolePermission.AddChild(
            HrEmployeeRolePermissions.Create,
            L("Permission:Create")
        );

        employeeRolePermission.AddChild(
            HrEmployeeRolePermissions.Edit,
            L("Permission:Edit")
        );

        employeeRolePermission.AddChild(
            HrEmployeeRolePermissions.Delete,
            L("Permission:Delete")
        );

        employeeRolePermission.AddChild(
            HrEmployeeRolePermissions.View,
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

## 4. Entity Framework Core

### 4.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrEmployeeRoles/HrEmployeeRoleConfiguration.cs`

**Nội dung**:
```csharp
using iOne.HrEmployeeRoles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.HrEmployeeRoles;

public class HrEmployeeRoleConfiguration : IEntityTypeConfiguration<HrEmployeeRole>
{
    public void Configure(EntityTypeBuilder<HrEmployeeRole> builder)
    {
        builder.ToTable("HrEmployeeRole", t =>
        {
            t.HasComment("Bảng lưu vai trò của nhân viên: CTV, NV, CV, ...");
        });

        builder.ConfigureByConvention();

        // Configure Code with unique index
        builder.HasIndex(e => e.Code, "IX_HrEmployeeRole_Code")
            .IsUnique();

        // Configure Status enum -> string conversion
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasComment("Trạng thái: - active: hoạt động - deactive: không hoạt động");
    }
}
```

**Lưu ý**: 
- Table name: `"HrEmployeeRole"` (PascalCase)
- Unique index trên Code
- Status convert sang string với MaxLength(10)

---

### 4.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrEmployeeRoles/EfCoreHrEmployeeRoleRepository.cs`

**Nội dung**:
```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.EntityFrameworkCore;
using iOne.HrEmployeeRoles;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.HrEmployeeRoles;

public class EfCoreHrEmployeeRoleRepository : EfCoreRepository<iOneDbContext, HrEmployeeRole, Guid>,
    IHrEmployeeRoleRepository
{
    public EfCoreHrEmployeeRoleRepository(IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeExistsAsync(string code)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => x.Code == code);
    }
}
```

---

### 4.3. Register Repository

**Location**: `src/common/infra/iOne.EntityFrameworkCore/iOneDbContext.cs`

**Cần thêm**:
```csharp
public DbSet<HrEmployeeRole> HrEmployeeRoles { get; set; }
```

**Trong `OnModelCreating`**:
```csharp
builder.ConfigureHrEmployeeRoles();
```

**Location**: `src/common/infra/iOne.EntityFrameworkCore/iOneDbContextModelCreatingExtensions.cs`

**Cần thêm**:
```csharp
public static void ConfigureHrEmployeeRoles(this ModelBuilder builder)
{
    builder.ApplyConfiguration(new HrEmployeeRoleConfiguration());
}
```

---

### 4.4. Database Migration

**Quy tắc QUAN TRỌNG**:
- **LUÔN** sử dụng `IF EXISTS` khi DROP TABLE/COLUMN/INDEX
- **LUÔN** sử dụng `IF NOT EXISTS` khi CREATE TABLE/COLUMN/INDEX

**Migration sẽ tạo**:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "HrEmployeeRole",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
            Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
            Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, 
                comment: "Trạng thái: - active: hoạt động - deactive: không hoạt động"),
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
            table.PrimaryKey("PK_HrEmployeeRole", x => x.Id);
        },
        comment: "Bảng lưu vai trò của nhân viên: CTV, NV, CV, ...");

    migrationBuilder.CreateIndex(
        name: "IX_HrEmployeeRole_Code",
        table: "HrEmployeeRole",
        column: "Code",
        unique: true);
}
```

---

## 5. Localization

### 5.1. Localization Keys (vi-VN)

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Localization/Hr/vi-VN.json`

**Cần thêm các keys sau**:
```json
{
  "Menu:EmployeeRoles": "Danh mục Vai trò nhân viên",
  "Permission:HrEmployeeRole": "Danh mục Vai trò nhân viên",
  "HrEmployeeRole:Code": "Mã vai trò",
  "HrEmployeeRole:Name": "Tên vai trò",
  "HrEmployeeRole:Status": "Trạng thái",
  "HrEmployeeRole:New": "Thêm mới",
  "HrEmployeeRole:Edit": "Chỉnh sửa",
  "HrEmployeeRole:Delete": "Xóa",
  "HrEmployeeRole:SearchByCode": "Tìm theo mã",
  "HrEmployeeRole:SearchByName": "Tìm theo tên",
  "HrEmployeeRole:Active": "Hoạt động",
  "HrEmployeeRole:Deactive": "Không hoạt động",
  "HrEmployeeRole:CodeRequired": "Mã vai trò không được để trống",
  "HrEmployeeRole:CodeMaxLength": "Mã vai trò không được vượt quá {0} ký tự",
  "HrEmployeeRole:CodeInvalidFormat": "Mã vai trò chỉ được chứa chữ cái in hoa (A-Z), số (0-9) và dấu gạch dưới (_)",
  "HrEmployeeRole:CodeExists": "Mã vai trò '{Code}' đã tồn tại",
  "HrEmployeeRole:NameRequired": "Tên vai trò không được để trống",
  "HrEmployeeRole:NameMaxLength": "Tên vai trò không được vượt quá {0} ký tự",
  "HrEmployeeRole:StatusRequired": "Trạng thái không được để trống",
  "HrEmployeeRole:Status:Active": "Hoạt động",
  "HrEmployeeRole:Status:Deactive": "Không hoạt động",
  "HrEmployeeRole:CreatedSuccessfully": "Tạo mới thành công",
  "HrEmployeeRole:UpdatedSuccessfully": "Cập nhật thành công",
  "HrEmployeeRole:DeletedSuccessfully": "Xóa thành công",
  "HrEmployeeRole:DeleteConfirm": "Bạn có chắc chắn muốn xóa bản ghi này?"
}
```

---

### 5.2. Localization Keys (en)

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Localization/Hr/en.json`

**Cần thêm các keys sau**:
```json
{
  "Menu:EmployeeRoles": "Employee Roles",
  "Permission:HrEmployeeRole": "Employee Role",
  "HrEmployeeRole:Code": "Code",
  "HrEmployeeRole:Name": "Name",
  "HrEmployeeRole:Status": "Status",
  "HrEmployeeRole:New": "New",
  "HrEmployeeRole:Edit": "Edit",
  "HrEmployeeRole:Delete": "Delete",
  "HrEmployeeRole:SearchByCode": "Search by code",
  "HrEmployeeRole:SearchByName": "Search by name",
  "HrEmployeeRole:Active": "Active",
  "HrEmployeeRole:Deactive": "Deactive",
  "HrEmployeeRole:CodeRequired": "Code cannot be empty",
  "HrEmployeeRole:CodeMaxLength": "Code cannot exceed {0} characters",
  "HrEmployeeRole:CodeInvalidFormat": "Code can only contain uppercase letters (A-Z), numbers (0-9), and underscore (_)",
  "HrEmployeeRole:CodeExists": "Code '{Code}' already exists",
  "HrEmployeeRole:NameRequired": "Name cannot be empty",
  "HrEmployeeRole:NameMaxLength": "Name cannot exceed {0} characters",
  "HrEmployeeRole:StatusRequired": "Status cannot be empty",
  "HrEmployeeRole:Status:Active": "Active",
  "HrEmployeeRole:Status:Deactive": "Deactive",
  "HrEmployeeRole:CreatedSuccessfully": "Created successfully",
  "HrEmployeeRole:UpdatedSuccessfully": "Updated successfully",
  "HrEmployeeRole:DeletedSuccessfully": "Deleted successfully",
  "HrEmployeeRole:DeleteConfirm": "Are you sure you want to delete this record?"
}
```

---

## 6. HTTP API Controller

**Location**: `modules/hr/src/iOne.Hr.HttpApi/Controllers/HrEmployeeRoleController.cs`

**Nội dung**:
```csharp
using System;
using System.Threading.Tasks;
using iOne.Hr;
using iOne.Hr.HrEmployeeRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Hr.Controllers;

[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]
[Area(HrRemoteServiceConsts.ModuleName)]
[Route("api/hr/employee-roles")]
[Authorize]
public class HrEmployeeRoleController : AbpControllerBase
{
    protected IHrEmployeeRoleAppService AppService { get; }

    public HrEmployeeRoleController(IHrEmployeeRoleAppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<HrEmployeeRoleDto>> GetListAsync(GetHrEmployeeRolesInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<HrEmployeeRoleDto> GetAsync(Guid id)
    {
        return AppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<HrEmployeeRoleDto> CreateAsync(CreateHrEmployeeRoleDto input)
    {
        return AppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<HrEmployeeRoleDto> UpdateAsync(Guid id, UpdateHrEmployeeRoleDto input)
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

**Lưu ý**: 
- Route: `"api/hr/employee-roles"`
- Tất cả methods đều có `[Authorize]` (từ base class và AppService)

---

## 7. Module Registration

### 7.1. Domain Module

**Location**: `src/common/domain/iOne.Domain/iOneDomainModule.cs`

**Cần đảm bảo**:
- Module đã được cấu hình đúng
- Không cần thay đổi gì (vì entity nằm trong namespace riêng)

---

### 7.2. Application Contracts Module

**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/iOneHrApplicationContractsModule.cs`

**Cần đảm bảo**:
- PermissionDefinitionProvider đã được register:
```csharp
Configure<PermissionOptions>(options =>
{
    options.DefinitionProviders.Add<HrEmployeeRolePermissionDefinitionProvider>();
});
```

---

### 7.3. Application Module

**Location**: `modules/hr/src/iOne.Hr.Application/iOneHrApplicationModule.cs`

**Cần đảm bảo**:
- AutoMapper profile đã được cấu hình (nếu cần custom mapping)

---

### 7.4. EntityFrameworkCore Module

**Location**: `src/common/infra/iOne.EntityFrameworkCore/iOneEntityFrameworkCoreModule.cs`

**Cần đảm bảo**:
- DbContext đã được register đúng

---

## 8. Checklist Trước Khi Hoàn Thành

### 8.1. Domain Layer
- [ ] Enum `HrEmployeeRoleStatus` đã tạo
- [ ] Entity `HrEmployeeRole` đã tạo với:
  - [ ] Code là `private set`
  - [ ] Không có method `UpdateCode()`
  - [ ] Validation Code format: `^[A-Z0-9_]+$`
- [ ] Repository interface `IHrEmployeeRoleRepository` đã tạo với method `IsCodeExistsAsync`
- [ ] Manager `HrEmployeeRoleManager` đã tạo với:
  - [ ] `CreateAsync` validate Code uniqueness
  - [ ] `UpdateAsync` không có Code parameter

### 8.2. Application Layer
- [ ] DTOs đã tạo:
  - [ ] `HrEmployeeRoleDto`
  - [ ] `CreateHrEmployeeRoleDto` (có Code với validation)
  - [ ] `UpdateHrEmployeeRoleDto` (**KHÔNG có Code**)
  - [ ] `GetHrEmployeeRolesInput`
- [ ] Application Service Interface `IHrEmployeeRoleAppService` đã tạo
- [ ] Application Service `HrEmployeeRoleAppService` đã implement với:
  - [ ] `CreateAsync` tạo entity mới
  - [ ] `UpdateAsync` chỉ update Name và Status
  - [ ] `DeleteAsync` **soft delete** (set Status = Deactive)
  - [ ] `CreateFilteredQueryAsync` filter theo Code, Name, Status

### 8.3. Permissions
- [ ] Permission Constants `HrEmployeeRolePermissions` đã tạo
- [ ] Permission Definition Provider `HrEmployeeRolePermissionDefinitionProvider` đã tạo
- [ ] Permission Provider đã được register trong Application Contracts Module

### 8.4. Entity Framework Core
- [ ] Entity Configuration `HrEmployeeRoleConfiguration` đã tạo:
  - [ ] Table name: `"HrEmployeeRole"`
  - [ ] Unique index trên Code
  - [ ] Status convert sang string
- [ ] Repository Implementation `EfCoreHrEmployeeRoleRepository` đã tạo
- [ ] DbContext đã thêm `DbSet<HrEmployeeRole>`
- [ ] DbContext Model Creating Extensions đã thêm `ConfigureHrEmployeeRoles()`
- [ ] Migration đã được tạo với `IF EXISTS`/`IF NOT EXISTS`

### 8.5. Localization
- [ ] Localization keys đã thêm vào `vi-VN.json`
- [ ] Localization keys đã thêm vào `en.json`
- [ ] Tất cả keys đều match nhau giữa 2 file

### 8.6. HTTP API
- [ ] Controller `HrEmployeeRoleController` đã tạo với:
  - [ ] Route: `"api/hr/employee-roles"`
  - [ ] Tất cả CRUD endpoints
  - [ ] Authorization attributes

### 8.7. Testing
- [ ] Build solution thành công
- [ ] Migration chạy thành công
- [ ] Test API endpoints:
  - [ ] GET `/api/hr/employee-roles` - List với filter
  - [ ] GET `/api/hr/employee-roles/{id}` - Get by id
  - [ ] POST `/api/hr/employee-roles` - Create (validate Code format, uniqueness)
  - [ ] PUT `/api/hr/employee-roles/{id}` - Update (không cho sửa Code)
  - [ ] DELETE `/api/hr/employee-roles/{id}` - Delete (soft delete, set Status = Deactive)
- [ ] Test permissions:
  - [ ] View permission
  - [ ] Create permission
  - [ ] Edit permission
  - [ ] Delete permission
- [ ] Test audit log:
  - [ ] Tạo mới có audit log
  - [ ] Update có audit log
  - [ ] Delete (soft delete) có audit log

---

## 9. Lưu Ý Quan Trọng

1. **Code không được sửa**: 
   - Entity không có method `UpdateCode()`
   - `UpdateHrEmployeeRoleDto` không có Code property
   - `UpdateAsync` trong Manager không có Code parameter

2. **Soft Delete**: 
   - `DeleteAsync` không hard delete, chỉ set Status = Deactive
   - Đảm bảo gọi `SaveChangesAsync()` để trigger audit log

3. **Code Validation**: 
   - Format: `^[A-Z0-9_]+$` (chỉ A-Z, 0-9, _)
   - Unique constraint trong database
   - Validation ở cả Entity và DTO

4. **Audit Log**: 
   - Tự động từ `FullAuditedAggregateRoot<Guid>`
   - Đảm bảo gọi `SaveChangesAsync()` sau mỗi thao tác

5. **Permissions**: 
   - Đặt trong module HR, không phải common/domain
   - `GroupName` = `"HrEmployeeRole"`
   - `Default` = `GroupName` (không phải `"{GroupName}.Default"`)

6. **Table Name**: 
   - Table name: `"HrEmployeeRole"` (PascalCase)

7. **Migration Safety**: 
   - LUÔN dùng `IF EXISTS`/`IF NOT EXISTS` trong migration

---

## 10. Thứ Tự Thực Hiện

1. **Domain Layer** (bước 1):
   - Tạo Enum
   - Tạo Entity
   - Tạo Repository Interface
   - Tạo Manager

2. **EF Core** (bước 2):
   - Tạo Entity Configuration
   - Tạo Repository Implementation
   - Register trong DbContext
   - Tạo Migration

3. **Application Layer** (bước 3):
   - Tạo DTOs
   - Tạo Application Service Interface
   - Tạo Application Service Implementation

4. **Permissions** (bước 4):
   - Tạo Permission Constants
   - Tạo Permission Definition Provider
   - Register Permission Provider

5. **Localization** (bước 5):
   - Thêm keys vào vi-VN.json
   - Thêm keys vào en.json

6. **HTTP API** (bước 6):
   - Tạo Controller

7. **Testing** (bước 7):
   - Build và test

---

## Kết Luận

Plan này tuân thủ đầy đủ các quy tắc trong `RULES_BACKEND_DEVELOPMENT.md` và đáp ứng tất cả yêu cầu:
- ✅ CRUD đầy đủ
- ✅ Code validation: A-Z, _, 0-9
- ✅ Code không được sửa khi update
- ✅ Code unique
- ✅ Soft delete (Status = Deactive)
- ✅ Audit log tự động
- ✅ Đa ngôn ngữ (vi-VN, en)
- ✅ Phân quyền đầy đủ

Sau khi review và approve plan này, có thể bắt đầu implement theo thứ tự đã nêu.

