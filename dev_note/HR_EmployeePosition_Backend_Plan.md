# Plan: Backend Danh mục Chức danh nhân viên (HR Employee Position)

## 1. Tổng quan
Tạo module backend cho quản lý Danh mục Chức danh nhân viên với các tính năng CRUD đầy đủ, validation nghiêm ngặt và audit logging.

## 2. Cấu trúc Database
**Bảng: HrEmployeePosition** (PascalCase, không phải HREMPLOYEEPOSITION)
- **Table Comment**: "Bảng lưu chức danh công tác"
- Id (UUID) - Primary Key (PostgreSQL UUID type)
- Code (VARCHAR(50)) - NOT NULL, UNIQUE
  - **Validation**: Chỉ chứa A-Z, _, 0-9
  - **Không được sửa** khi update
- Name (VARCHAR(250)) - NOT NULL
- Type (VARCHAR(10)) - NOT NULL
  - **Column Comment**: "Phân loại vị trí/vai trò: - profess: chuyên môn - manage: quản lý"
  - **Enum values**: `Profess`, `Manage`
- Status (VARCHAR(10)) - NOT NULL
  - **Column Comment**: "Trạng thái: - active: hoạt động - deactive: không hoạt động"
  - **Enum values**: `Active`, `Deactive`
- CreationTime (TIMESTAMP) - NOT NULL (từ FullAuditedAggregateRoot)
- CreatorId (UUID) - NULL (từ FullAuditedAggregateRoot)
- LastModificationTime (TIMESTAMP) - NULL (từ FullAuditedAggregateRoot)
- LastModifierId (UUID) - NULL (từ FullAuditedAggregateRoot)
- IsDeleted (BOOLEAN) - NOT NULL, DEFAULT FALSE (từ FullAuditedAggregateRoot - soft delete)
- DeleterId (UUID) - NULL (từ FullAuditedAggregateRoot)
- DeletionTime (TIMESTAMP) - NULL (từ FullAuditedAggregateRoot)

**Lưu ý**: 
- Table name phải là **PascalCase**: `HrEmployeePosition` (không phải `HREMPLOYEEPOSITION`)
- Column names phải là **PascalCase**: `Code`, `Name`, `Type`, `Status` (không phải `CODE`, `NAME`, `TYPE`, `STATUS`)

## 3. Chi tiết từng Layer

### 3.1 Domain Layer (`src/common/domain/iOne.Domain/HrEmployeePositions/`)

#### 3.1.1 Enum: `HrEmployeePositionType.cs`
**Location**: `src/common/domain/iOne.Domain.Shared/HrEmployeePositions/HrEmployeePositionType.cs`

```csharp
namespace iOne.HrEmployeePositions;

public enum HrEmployeePositionType
{
    Profess = 0,    // Chuyên môn
    Manage = 1      // Quản lý
}
```

#### 3.1.2 Enum: `HrEmployeePositionStatus.cs`
**Location**: `src/common/domain/iOne.Domain.Shared/HrEmployeePositions/HrEmployeePositionStatus.cs`

```csharp
namespace iOne.HrEmployeePositions;

public enum HrEmployeePositionStatus
{
    Active = 0,     // Hoạt động
    Deactive = 1    // Không hoạt động
}
```

#### 3.1.3 Entity: `HrEmployeePosition.cs`
**Location**: `src/common/domain/iOne.Domain/HrEmployeePositions/HrEmployeePosition.cs`

**Quy tắc**:
- Kế thừa `FullAuditedAggregateRoot<Guid>`
- `[Table("HrEmployeePosition")]` - Table name (PascalCase)
- Properties:
  - `Code` (string) - private setter, với validation: `[MaxLength(50)]`, `[Required]`, regex `^[A-Z0-9_]+$`
  - `Name` (string) - private setter, với validation: `[MaxLength(250)]`, `[Required]`
  - `Type` (HrEmployeePositionType) - private setter, với validation: `[Required]`
  - `Status` (HrEmployeePositionStatus) - private setter, với validation: `[Required]`
- Methods:
  - Constructor: `HrEmployeePosition(Guid id, string code, string name, HrEmployeePositionType type, HrEmployeePositionStatus status)`
  - `SetCode(string code)` - private method với validation regex `^[A-Z0-9_]+$`
  - `SetName(string name)` - private method
  - `SetType(HrEmployeePositionType type)` - private method
  - `UpdateName(string name)` - public method để update Name
  - `UpdateType(HrEmployeePositionType type)` - public method để update Type
  - `UpdateStatus(HrEmployeePositionStatus status)` - public method để update Status
  - **KHÔNG có method `UpdateCode`** - Code không được sửa sau khi tạo

**Lưu ý**: Code không được sửa sau khi tạo, nên không có method `UpdateCode`.

#### 3.1.4 Repository Interface: `IHrEmployeePositionRepository.cs`
**Location**: `src/common/domain/iOne.Domain/HrEmployeePositions/IHrEmployeePositionRepository.cs`

```csharp
using Volo.Abp.Domain.Repositories;

namespace iOne.HrEmployeePositions;

public interface IHrEmployeePositionRepository : IRepository<HrEmployeePosition, Guid>
{
}
```

#### 3.1.5 Manager: `HrEmployeePositionManager.cs`
**Location**: `src/common/domain/iOne.Domain/HrEmployeePositions/HrEmployeePositionManager.cs`

**Quy tắc**:
- Inject `IHrEmployeePositionRepository`
- Methods:
  - `CreateAsync(HrEmployeePosition entity)` - Validate Code unique trước khi tạo
  - `UpdateAsync(HrEmployeePosition entity, string name, HrEmployeePositionType type, HrEmployeePositionStatus status)` - Validate (không validate Code vì không được sửa)

### 3.2 Application Layer (`modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeePositions/`)

#### 3.2.1 DTO: `HrEmployeePositionDto.cs`
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeePositions/HrEmployeePositionDto.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeePositions;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeePositions;

public class HrEmployeePositionDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "HrEmployeePosition:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "HrEmployeePosition:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "HrEmployeePosition:Type")]
    public HrEmployeePositionType Type { get; set; }

    [Display(Name = "HrEmployeePosition:Status")]
    public HrEmployeePositionStatus Status { get; set; }
}
```

#### 3.2.2 DTO: `CreateHrEmployeePositionDto.cs`
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeePositions/CreateHrEmployeePositionDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeePositions;

namespace iOne.Hr.HrEmployeePositions;

public class CreateHrEmployeePositionDto
{
    [Required(ErrorMessage = "HrEmployeePosition:CodeRequired")]
    [StringLength(50, ErrorMessage = "HrEmployeePosition:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "HrEmployeePosition:CodeInvalidFormat")]
    [Display(Name = "HrEmployeePosition:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeePosition:NameRequired")]
    [StringLength(250, ErrorMessage = "HrEmployeePosition:NameMaxLength")]
    [Display(Name = "HrEmployeePosition:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeePosition:TypeRequired")]
    [Display(Name = "HrEmployeePosition:Type")]
    public HrEmployeePositionType Type { get; set; }

    [Required(ErrorMessage = "HrEmployeePosition:StatusRequired")]
    [Display(Name = "HrEmployeePosition:Status")]
    public HrEmployeePositionStatus Status { get; set; }
}
```

#### 3.2.3 DTO: `UpdateHrEmployeePositionDto.cs`
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeePositions/UpdateHrEmployeePositionDto.cs`

**Lưu ý**: **KHÔNG có field Code** vì Code không được sửa.

```csharp
using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeePositions;

namespace iOne.Hr.HrEmployeePositions;

public class UpdateHrEmployeePositionDto
{
    [Required(ErrorMessage = "HrEmployeePosition:NameRequired")]
    [StringLength(250, ErrorMessage = "HrEmployeePosition:NameMaxLength")]
    [Display(Name = "HrEmployeePosition:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeePosition:TypeRequired")]
    [Display(Name = "HrEmployeePosition:Type")]
    public HrEmployeePositionType Type { get; set; }

    [Required(ErrorMessage = "HrEmployeePosition:StatusRequired")]
    [Display(Name = "HrEmployeePosition:Status")]
    public HrEmployeePositionStatus Status { get; set; }
}
```

#### 3.2.4 DTO: `GetHrEmployeePositionsInput.cs`
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeePositions/GetHrEmployeePositionsInput.cs`

```csharp
using iOne.HrEmployeePositions;
using Volo.Abp.Application.Dtos;

namespace iOne.Hr.HrEmployeePositions;

public class GetHrEmployeePositionsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public HrEmployeePositionType? Type { get; set; }
    public HrEmployeePositionStatus? Status { get; set; }
}
```

#### 3.2.5 AppService Interface: `IHrEmployeePositionAppService.cs`
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/HrEmployeePositions/IHrEmployeePositionAppService.cs`

```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Hr.HrEmployeePositions;

public interface IHrEmployeePositionAppService : ICrudAppService<
    HrEmployeePositionDto,
    Guid,
    GetHrEmployeePositionsInput,
    CreateHrEmployeePositionDto,
    UpdateHrEmployeePositionDto>
{
}
```

#### 3.2.6 AppService Implementation: `HrEmployeePositionAppService.cs`
**Location**: `modules/hr/src/iOne.Hr.Application/HrEmployeePositions/HrEmployeePositionAppService.cs`

**Quy tắc**:
- Kế thừa `CrudAppService<HrEmployeePosition, HrEmployeePositionDto, Guid, GetHrEmployeePositionsInput, CreateHrEmployeePositionDto, UpdateHrEmployeePositionDto>`
- Inject `IHrEmployeePositionRepository` và `HrEmployeePositionManager`
- Override methods:
  - `CreateAsync(CreateHrEmployeePositionDto input)` - Validate Code unique, tạo entity
  - `UpdateAsync(Guid id, UpdateHrEmployeePositionDto input)` - **KHÔNG update Code**, chỉ update Name, Type, Status
  - `DeleteAsync(Guid id)` - **QUAN TRỌNG**: 
    1. Load entity
    2. **Xóa trước** (soft delete) để trigger audit log với ChangeType = Deleted
    3. Save changes
    4. **Cập nhật status = Deactive** sau
    5. Save changes
  - `CreateFilteredQueryAsync(GetHrEmployeePositionsInput input)` - Filter theo Code, Name, Type, Status

**Lưu ý về Delete**:
```csharp
public override async Task DeleteAsync(Guid id)
{
    var entity = await Repository.GetAsync(id);
    
    // 1. Delete trước để trigger audit logging với ChangeType = Deleted
    await Repository.DeleteAsync(entity);
    await CurrentUnitOfWork.SaveChangesAsync();
    
    // 2. Update status về Deactive sau
    entity.UpdateStatus(HrEmployeePositionStatus.Deactive);
    await Repository.UpdateAsync(entity);
    await CurrentUnitOfWork.SaveChangesAsync();
}
```

### 3.3 Permissions (`modules/hr/src/iOne.Hr.Application.Contracts/Permissions/`)

#### 3.3.1 Permission Constants: `HrEmployeePositionPermissions.cs`
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrEmployeePositionPermissions.cs`

```csharp
namespace iOne.Hr.Permissions;

public static class HrEmployeePositionPermissions
{
    public const string GroupName = "HrEmployeePosition";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

#### 3.3.2 Permission Definition Provider: `HrEmployeePositionPermissionDefinitionProvider.cs`
**Location**: `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrEmployeePositionPermissionDefinitionProvider.cs`

```csharp
using iOne.Hr.Localization;
using iOne.Hr.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.Hr.Permissions;

public class HrEmployeePositionPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var hrEmployeePositionGroup = context.AddGroup(
            HrEmployeePositionPermissions.GroupName,
            L("Permission:HrEmployeePosition")
        );

        var hrEmployeePositionPermission = hrEmployeePositionGroup.AddPermission(
            HrEmployeePositionPermissions.Default,
            L("Permission:HrEmployeePosition")
        );

        hrEmployeePositionPermission.AddChild(
            HrEmployeePositionPermissions.Create,
            L("Permission:Create")
        );

        hrEmployeePositionPermission.AddChild(
            HrEmployeePositionPermissions.Edit,
            L("Permission:Edit")
        );

        hrEmployeePositionPermission.AddChild(
            HrEmployeePositionPermissions.Delete,
            L("Permission:Delete")
        );

        hrEmployeePositionPermission.AddChild(
            HrEmployeePositionPermissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HrResource>(name);
    }
}
```

### 3.4 Entity Framework Core (`src/common/infra/iOne.EntityFrameworkCore/HrEmployeePositions/`)

#### 3.4.1 Entity Configuration: `HrEmployeePositionConfiguration.cs`
**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrEmployeePositions/HrEmployeePositionConfiguration.cs`

**Quy tắc**:
- Table name: `"HrEmployeePosition"` (PascalCase)
- Table comment: `"Bảng lưu chức danh công tác"`
- Unique index trên Code: `"IX_HrEmployeePosition_Code"`
- Configure enum conversions:
  - `Type` -> string (maxLength: 10) với comment
  - `Status` -> string (maxLength: 10) với comment

```csharp
public class HrEmployeePositionConfiguration : IEntityTypeConfiguration<HrEmployeePosition>
{
    public void Configure(EntityTypeBuilder<HrEmployeePosition> builder)
    {
        builder.ToTable("HrEmployeePosition", t =>
        {
            t.HasComment("Bảng lưu chức danh công tác");
        });

        builder.ConfigureByConvention();

        // Unique index trên Code
        builder.HasIndex(e => e.Code, "IX_HrEmployeePosition_Code")
            .IsUnique();

        // Configure Type enum -> string conversion
        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasComment("Phân loại vị trí/vai trò: - profess: chuyên môn - manage: quản lý");

        // Configure Status enum -> string conversion
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasComment("Trạng thái: - active: hoạt động - deactive: không hoạt động");
    }
}
```

#### 3.4.2 Repository Implementation: `EfCoreHrEmployeePositionRepository.cs`
**Location**: `src/common/infra/iOne.EntityFrameworkCore/HrEmployeePositions/EfCoreHrEmployeePositionRepository.cs`

```csharp
using iOne.HrEmployeePositions;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.HrEmployeePositions;

public class EfCoreHrEmployeePositionRepository
    : EfCoreRepository<iOneDbContext, HrEmployeePosition, Guid>,
      IHrEmployeePositionRepository
{
    public EfCoreHrEmployeePositionRepository(
        IDbContextProvider<iOneDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
```

#### 3.4.3 Database Migration
**Quy tắc QUAN TRỌNG**:
- **LUÔN** sử dụng `IF EXISTS` khi DROP TABLE/COLUMN/INDEX
- **LUÔN** sử dụng `IF NOT EXISTS` khi CREATE TABLE/COLUMN/INDEX
- Table name: `"HrEmployeePosition"` (PascalCase)
- Column names: `"Code"`, `"Name"`, `"Type"`, `"Status"` (PascalCase)
- Index name: `"IX_HrEmployeePosition_Code"` (PascalCase với prefix)
- Primary key name: `"PK_HrEmployeePosition"` (PascalCase với prefix)

**Migration Example**:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "HrEmployeePosition",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
            Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
            Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Phân loại vị trí/vai trò: - profess: chuyên môn - manage: quản lý"),
            Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Trạng thái: - active: hoạt động - deactive: không hoạt động"),
            // ... audit fields từ FullAuditedAggregateRoot
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_HrEmployeePosition", x => x.Id);
        },
        comment: "Bảng lưu chức danh công tác");

    migrationBuilder.CreateIndex(
        name: "IX_HrEmployeePosition_Code",
        table: "HrEmployeePosition",
        column: "Code",
        unique: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_HrEmployeePosition_Code"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""HrEmployeePosition"";");
}
```

### 3.5 Localization (`modules/hr/src/iOne.Hr.Application.Contracts/Localization/Hr/`)

#### 3.5.1 Localization Keys (vi-VN.json và en.json)
**Location**: 
- `modules/hr/src/iOne.Hr.Application.Contracts/Localization/Hr/vi-VN.json`
- `modules/hr/src/iOne.Hr.Application.Contracts/Localization/Hr/en.json`

**Keys cần thêm**:
```json
{
  "Menu:EmployeePositions": "Danh mục Chức danh nhân viên",
  "Permission:HrEmployeePosition": "Danh mục Chức danh nhân viên",
  "HrEmployeePosition:Code": "Mã chức danh",
  "HrEmployeePosition:Name": "Tên chức danh",
  "HrEmployeePosition:Type": "Phân loại",
  "HrEmployeePosition:Status": "Trạng thái",
  "HrEmployeePosition:New": "Thêm mới",
  "HrEmployeePosition:Edit": "Chỉnh sửa",
  "HrEmployeePosition:Delete": "Xóa",
  "HrEmployeePosition:SearchByCode": "Tìm theo mã",
  "HrEmployeePosition:SearchByName": "Tìm theo tên",
  "HrEmployeePosition:SearchByType": "Tìm theo phân loại",
  "HrEmployeePosition:Profess": "Chuyên môn",
  "HrEmployeePosition:Manage": "Quản lý",
  "HrEmployeePosition:Active": "Hoạt động",
  "HrEmployeePosition:Deactive": "Không hoạt động",
  "HrEmployeePosition:CodeRequired": "Mã chức danh không được để trống",
  "HrEmployeePosition:CodeMaxLength": "Mã chức danh không được vượt quá {0} ký tự",
  "HrEmployeePosition:CodeInvalidFormat": "Mã chức danh chỉ được chứa chữ cái in hoa (A-Z), số (0-9) và dấu gạch dưới (_)",
  "HrEmployeePosition:CodeExists": "Mã chức danh '{Code}' đã tồn tại",
  "HrEmployeePosition:CodeCannotBeChanged": "Mã chức danh không thể thay đổi sau khi tạo",
  "HrEmployeePosition:NameRequired": "Tên chức danh không được để trống",
  "HrEmployeePosition:NameMaxLength": "Tên chức danh không được vượt quá {0} ký tự",
  "HrEmployeePosition:TypeRequired": "Phân loại không được để trống",
  "HrEmployeePosition:Type:Profess": "Chuyên môn",
  "HrEmployeePosition:Type:Manage": "Quản lý",
  "HrEmployeePosition:StatusRequired": "Trạng thái không được để trống",
  "HrEmployeePosition:Status:Active": "Hoạt động",
  "HrEmployeePosition:Status:Deactive": "Không hoạt động",
  "HrEmployeePosition:CreatedSuccessfully": "Tạo mới thành công",
  "HrEmployeePosition:UpdatedSuccessfully": "Cập nhật thành công",
  "HrEmployeePosition:DeletedSuccessfully": "Xóa thành công",
  "HrEmployeePosition:DeleteConfirm": "Bạn có chắc chắn muốn xóa bản ghi này?"
}
```

### 3.6 HTTP API Controllers (`modules/hr/src/iOne.Hr.HttpApi/Controllers/`)

#### 3.6.1 Controller: `HrEmployeePositionController.cs`
**Location**: `modules/hr/src/iOne.Hr.HttpApi/Controllers/HrEmployeePositionController.cs`

**Quy tắc**:
- Kế thừa `AbpControllerBase`
- Route: `"api/hr/employee-positions"`
- `[RemoteService(Name = HrRemoteServiceConsts.RemoteServiceName)]`
- `[Area(HrRemoteServiceConsts.ModuleName)]`
- `[Authorize]` trên class
- Methods:
  - `GetListAsync(GetHrEmployeePositionsInput input)` - `[HttpGet]`, `[Authorize(HrEmployeePositionPermissions.View)]`
  - `GetAsync(Guid id)` - `[HttpGet("{id}")]`, `[Authorize(HrEmployeePositionPermissions.View)]`
  - `CreateAsync(CreateHrEmployeePositionDto input)` - `[HttpPost]`, `[Authorize(HrEmployeePositionPermissions.Create)]`
  - `UpdateAsync(Guid id, UpdateHrEmployeePositionDto input)` - `[HttpPut("{id}")]`, `[Authorize(HrEmployeePositionPermissions.Edit)]`
  - `DeleteAsync(Guid id)` - `[HttpDelete("{id}")]`, `[Authorize(HrEmployeePositionPermissions.Delete)]`

#### 3.6.2 Exclude từ Conventional Controllers
**Location**: `modules/hr/src/iOne.Hr.HttpApi/iOneHrHttpApiModule.cs`

**Quy tắc**: Nếu có manual controller, phải exclude AppService khỏi conventional controller generation.

```csharp
Configure<AbpAspNetCoreMvcOptions>(options =>
{
    options.ConventionalControllers.Create(
        typeof(iOneHrApplicationModule).Assembly,
        opts =>
        {
            opts.TypePredicate = type =>
                type.Name != "HrDepartmentTypeAppService" &&
                type.Name != "HrEmployeeRoleAppService" &&
                type.Name != "HrEmployeeLevelAppService" &&
                type.Name != "HrEmployeePositionAppService"; // Exclude AppService có manual controller
        });
});
```

### 3.7 Menu Configuration (`modules/hr/src/iOne.Hr.Application/Navigation/`)

#### 3.7.1 Update Menu Contributor: `HrMenuContributor.cs`
**Location**: `modules/hr/src/iOne.Hr.Application/Navigation/HrMenuContributor.cs`

**Quy tắc**: Thêm menu item cho HrEmployeePosition vào menu HR.

```csharp
hrMenuItem.AddItem(new ApplicationMenuItem(
    "HR.EmployeePositions",
    hrL["Menu:EmployeePositions"],
    url: "~/pages/hr/employee-positions",
    icon: "pi pi-fw pi-briefcase"
).RequirePermissions(HrEmployeePositionPermissions.Default));
```

## 4. AutoMapper Configuration

#### 4.1 AutoMapper Profile
**Location**: `modules/hr/src/iOne.Hr.Application/iOneHrApplicationAutoMapperProfile.cs`

**Quy tắc**: Thêm mapping configuration nếu cần (thường ABP tự động map).

## 5. Checklist Trước Khi Hoàn Thành

### Domain Layer
- [ ] Enum `HrEmployeePositionType` đã tạo với values: `Profess`, `Manage`
- [ ] Enum `HrEmployeePositionStatus` đã tạo với values: `Active`, `Deactive`
- [ ] Entity `HrEmployeePosition` đã tạo với:
  - [ ] Kế thừa `FullAuditedAggregateRoot<Guid>`
  - [ ] `[Table("HrEmployeePosition")]` (PascalCase)
  - [ ] Properties: `Code`, `Name`, `Type`, `Status` với validation
  - [ ] Method `SetCode` với regex validation `^[A-Z0-9_]+$`
  - [ ] Methods: `UpdateName`, `UpdateType`, `UpdateStatus`
  - [ ] **KHÔNG có method `UpdateCode`**
- [ ] Repository interface `IHrEmployeePositionRepository` đã tạo
- [ ] Manager `HrEmployeePositionManager` đã tạo với validation Code unique

### Application Layer
- [ ] DTO `HrEmployeePositionDto` đã tạo
- [ ] DTO `CreateHrEmployeePositionDto` đã tạo với:
  - [ ] Field `Code` với regex validation `^[A-Z0-9_]+$`
  - [ ] Fields: `Name`, `Type`, `Status`
- [ ] DTO `UpdateHrEmployeePositionDto` đã tạo với:
  - [ ] **KHÔNG có field `Code`**
  - [ ] Fields: `Name`, `Type`, `Status`
- [ ] DTO `GetHrEmployeePositionsInput` đã tạo với filters: `Code`, `Name`, `Type`, `Status`
- [ ] AppService interface `IHrEmployeePositionAppService` đã tạo
- [ ] AppService implementation `HrEmployeePositionAppService` đã tạo với:
  - [ ] `CreateAsync` - validate Code unique
  - [ ] `UpdateAsync` - **KHÔNG update Code**
  - [ ] `DeleteAsync` - **Xóa trước, cập nhật status = Deactive sau**
  - [ ] `CreateFilteredQueryAsync` - filter theo Code, Name, Type, Status

### Permissions
- [ ] Permission constants `HrEmployeePositionPermissions` đã tạo
- [ ] Permission definition provider `HrEmployeePositionPermissionDefinitionProvider` đã tạo
- [ ] Permissions đã được sử dụng trong AppService và Controller

### Entity Framework Core
- [ ] Entity configuration `HrEmployeePositionConfiguration` đã tạo với:
  - [ ] Table name: `"HrEmployeePosition"` (PascalCase)
  - [ ] Table comment: `"Bảng lưu chức danh công tác"`
  - [ ] Unique index: `"IX_HrEmployeePosition_Code"`
  - [ ] Enum conversions cho `Type` và `Status` với comments
- [ ] Repository implementation `EfCoreHrEmployeePositionRepository` đã tạo
- [ ] Migration đã được tạo với:
  - [ ] Table name: `"HrEmployeePosition"` (PascalCase)
  - [ ] Column names: `"Code"`, `"Name"`, `"Type"`, `"Status"` (PascalCase)
  - [ ] Index name: `"IX_HrEmployeePosition_Code"` (PascalCase)
  - [ ] Primary key name: `"PK_HrEmployeePosition"` (PascalCase)
  - [ ] **Sử dụng `IF EXISTS`/`IF NOT EXISTS` trong migration**

### Localization
- [ ] Localization keys đã được thêm vào `vi-VN.json`
- [ ] Localization keys đã được thêm vào `en.json`
- [ ] Keys phải match giữa 2 file

### HTTP API
- [ ] Controller `HrEmployeePositionController` đã tạo với:
  - [ ] Route: `"api/hr/employee-positions"`
  - [ ] `[RemoteService]` và `[Area]` attributes
  - [ ] All CRUD endpoints với authorization
- [ ] AppService đã được exclude khỏi conventional controllers (nếu có manual controller)

### Menu Configuration
- [ ] Menu item đã được thêm vào `HrMenuContributor`
- [ ] Menu item có permission check: `RequirePermissions(HrEmployeePositionPermissions.Default)`

### Testing
- [ ] Build solution thành công
- [ ] Migration chạy thành công
- [ ] Test API endpoints:
  - [ ] GET `/api/hr/employee-positions` - List với filters
  - [ ] GET `/api/hr/employee-positions/{id}` - Get by id
  - [ ] POST `/api/hr/employee-positions` - Create với Code validation
  - [ ] PUT `/api/hr/employee-positions/{id}` - Update (không được update Code)
  - [ ] DELETE `/api/hr/employee-positions/{id}` - Delete (soft delete + status = Deactive)
- [ ] Test validation:
  - [ ] Code chỉ chấp nhận A-Z, _, 0-9
  - [ ] Code unique
  - [ ] Code không được sửa khi update
- [ ] Test audit log: Delete phải có 2 records (Deleted + Updated)
- [ ] Test permissions: Tất cả endpoints phải có authorization
- [ ] Test menu: Menu hiển thị đúng trên client

## 6. Lưu Ý Quan Trọng

1. **Table và Column Naming**: 
   - **QUAN TRỌNG**: Table name và column name phải theo **PascalCase**
   - Table: `"HrEmployeePosition"` (không phải `"HREMPLOYEEPOSITION"`)
   - Columns: `"Code"`, `"Name"`, `"Type"`, `"Status"` (không phải `"CODE"`, `"NAME"`, `"TYPE"`, `"STATUS"`)

2. **Code Validation**: 
   - Chỉ chấp nhận A-Z, _, 0-9 (regex: `^[A-Z0-9_]+$`)
   - Code là unique
   - **Code không được sửa** sau khi tạo (không có trong `UpdateHrEmployeePositionDto`)

3. **Soft Delete với Status Update**:
   - **Xóa trước** (soft delete) để trigger audit log với ChangeType = Deleted
   - **Cập nhật status = Deactive sau** để đảm bảo audit log có 2 records

4. **Migration Safety**: 
   - **LUÔN** dùng `IF EXISTS`/`IF NOT EXISTS` để tránh lỗi khi migration

5. **Permissions**: 
   - Permissions phải được đặt trong **module**, không phải trong `common/domain`
   - Location: `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/`

6. **Menu Configuration**: 
   - Menu contributor phải được đặt trong **module Application**
   - Location: `modules/hr/src/iOne.Hr.Application/Navigation/HrMenuContributor.cs`

7. **Tránh Duplicate API**: 
   - Nếu có manual controller, phải exclude AppService khỏi conventional controller generation
   - Configuration trong `iOneHrHttpApiModule.cs`

