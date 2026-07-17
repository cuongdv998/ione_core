# Quy Tắc Chung Khi Tạo Backend

## 1. Entity và Domain Layer

### 1.1. Tạo Entity

**Location**: `src/common/domain/iOne.Domain/{ModuleName}/{EntityName}.cs`

**Quy tắc**:
- Kế thừa từ `FullAuditedAggregateRoot<Guid>` hoặc `FullAuditedEntity<Guid>`
- Sử dụng `Guid` cho Id (không dùng `string` trừ khi có lý do đặc biệt)
- Đặt tên class theo PascalCase
- Properties phải có validation attributes
- **Table Name**: Sử dụng `[Table]` attribute với **snake_case** (PostgreSQL convention) (ví dụ: `[Table("hr_employee_role")]`)
  - **KHÔNG** dùng UPPERCASE (ví dụ: `"HREMPLOYEEROLE"`)
  - **KHÔNG** dùng PascalCase (ví dụ: `"HrEmployeeRole"`)
  - **KHÔNG** dùng camelCase (ví dụ: `"hrEmployeeRole"`)
  - **BẮT BUỘC** dùng snake_case để phù hợp với PostgreSQL database style

**Ví dụ**:
```csharp
using System.ComponentModel.DataAnnotations.Schema;

[Table("hr_employee_role")] // ✅ ĐÚNG: snake_case (PostgreSQL convention)
public class HrEmployeeRole : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
    
    [Required]
    public HrEmployeeRoleStatus Status { get; set; }
}

// ❌ SAI: Table name không đúng convention
// [Table("HREMPLOYEEROLE")] // UPPERCASE
// [Table("HrEmployeeRole")] // PascalCase
// [Table("hrEmployeeRole")] // camelCase
```

### 1.2. Tạo Enum

**Location**: `src/common/domain/iOne.Domain.Shared/{ModuleName}/{EntityName}Status.cs`

**Quy tắc**:
- Đặt tên enum theo PascalCase với suffix phù hợp (Status, Type, etc.)
- Values phải có comment mô tả
- Nếu cần localization, thêm key tương ứng trong localization files

**Ví dụ**:
```csharp
public enum HrDepartmentTypeStatus
{
    Active = 0,    // Hoạt động
    Deactive = 1   // Không hoạt động
}
```

### 1.3. Tạo Repository Interface

**Location**: `src/common/domain/iOne.Domain/{ModuleName}/I{EntityName}Repository.cs`

**Quy tắc**:
- Kế thừa từ `IRepository<{EntityName}, Guid>`
- Thêm các method custom nếu cần

### 1.4. Tạo Manager (nếu cần business logic)

**Location**: `src/common/domain/iOne.Domain/{ModuleName}/{EntityName}Manager.cs`

**Quy tắc**:
- Chứa business logic validation
- Inject repository và các service cần thiết

## 2. Application Layer

### 2.1. DTOs

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/{EntityName}s/models.ts` (cho frontend) hoặc `.cs` (cho backend)

**Quy tắc**:
- `Create{EntityName}Dto`: Chỉ chứa fields cần thiết để tạo mới
- `Update{EntityName}Dto`: Chỉ chứa fields có thể update
- `{EntityName}Dto`: Chứa tất cả fields, kế thừa từ `FullAuditedEntityDto<Guid>`
- `Get{EntityName}sInput`: Kế thừa từ `PagedAndSortedResultRequestDto`

### 2.2. Application Service Interface

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/{EntityName}s/I{EntityName}AppService.cs`

**Quy tắc**:
- Methods phải có `[Authorize]` attribute
- Sử dụng `Task<T>` cho async methods
- Return types phải là DTOs, không phải Entity

### 2.3. Application Service Implementation

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.Application/{EntityName}s/{EntityName}AppService.cs`

**Quy tắc**:
- Inject repository và mapper
- Sử dụng `ObjectMapper` để convert giữa Entity và DTO
- Validation phải được thực hiện trước khi save
- Throw `UserFriendlyException` với localization key khi có lỗi

**Ví dụ**:
```csharp
public async Task<HrDepartmentTypeDto> CreateAsync(CreateHrDepartmentTypeDto input)
{
    // Validation
    if (await _repository.AnyAsync(x => x.Code == input.Code))
    {
        throw new UserFriendlyException(
            _localizer["HrDepartmentType:CodeExists", new { Code = input.Code }]
        );
    }
    
    // Create entity
    var entity = ObjectMapper.Map<CreateHrDepartmentTypeDto, HrDepartmentType>(input);
    await _repository.InsertAsync(entity);
    
    return ObjectMapper.Map<HrDepartmentType, HrDepartmentTypeDto>(entity);
}
```

### 2.4. AutoMapper Configuration

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.Application/iOne{ModuleName}ApplicationAutoMapperProfile.cs`

**Quy tắc QUAN TRỌNG**:
- **BẮT BUỘC**: Phải thêm mapping configuration cho Entity và DTOs trong AutoMapper Profile
- Nếu không có mapping, sẽ gặp lỗi: `"Missing type map configuration or unsupported mapping"`
- Phải thêm mapping cho:
  - `Entity` -> `{EntityName}Dto`
  - `Create{EntityName}Dto` -> `Entity` (nếu sử dụng ObjectMapper để map từ DTO sang Entity)
  - `Update{EntityName}Dto` -> `Entity` (nếu sử dụng ObjectMapper để map từ DTO sang Entity)

**Lưu ý**:
- Nếu Entity có private setters và constructor riêng (như `HrEmployeePosition`), có thể không dùng AutoMapper để map từ DTO sang Entity
- Trong trường hợp này, vẫn cần mapping `Entity` -> `{EntityName}Dto` để map từ Entity sang DTO khi return
- Mapping `CreateDto`/`UpdateDto` -> `Entity` vẫn nên thêm để tránh lỗi, dù có thể không được sử dụng

**Ví dụ**:
```csharp
using AutoMapper;
using iOne.Hr.HrDepartmentTypes;
using iOne.Hr.HrEmployeePositions;
using iOne.HrDepartmentTypes;
using iOne.HrEmployeePositions;

namespace iOne.Hr;

public class iOneHrApplicationAutoMapperProfile : Profile
{
    public iOneHrApplicationAutoMapperProfile()
    {
        // HrDepartmentType mappings
        CreateMap<HrDepartmentType, HrDepartmentTypeDto>();
        CreateMap<CreateHrDepartmentTypeDto, HrDepartmentType>();
        CreateMap<UpdateHrDepartmentTypeDto, HrDepartmentType>();

        // HrEmployeePosition mappings
        CreateMap<HrEmployeePosition, HrEmployeePositionDto>();
        CreateMap<CreateHrEmployeePositionDto, HrEmployeePosition>();
        CreateMap<UpdateHrEmployeePositionDto, HrEmployeePosition>();
    }
}
```

**Checklist**:
- [ ] Đã thêm `using` statements cho Entity và DTOs
- [ ] Đã thêm mapping `Entity` -> `{EntityName}Dto`
- [ ] Đã thêm mapping `Create{EntityName}Dto` -> `Entity`
- [ ] Đã thêm mapping `Update{EntityName}Dto` -> `Entity`

## 3. Permissions

**Lưu ý QUAN TRỌNG**: Permissions phải được đặt trong **module**, không phải trong `common/domain`.

### 3.1. Permission Constants

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/Permissions/{EntityName}Permissions.cs`

**Quy tắc**:
- `GroupName` = `"{ModuleName}{EntityName}"` (ví dụ: `"HrDepartmentType"`)
- `Default` = `GroupName` (không phải `"{GroupName}.Default"`)
- Các permission names = `"{GroupName}.{Action}"` (ví dụ: `"HrDepartmentType.Create"`)

**Ví dụ**:
```csharp
namespace iOne.{ModuleName}.Permissions;

public static class {EntityName}Permissions
{
    public const string GroupName = "{ModuleName}{EntityName}";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";
}
```

### 3.2. Permission Definition Provider

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/Permissions/{EntityName}PermissionDefinitionProvider.cs`

**Quy tắc**:
- Kế thừa từ `PermissionDefinitionProvider`
- Tạo permission group với `GroupName`
- Tạo Default permission = `GroupName`
- Thêm các child permissions (Create, Edit, Delete, View) vào Default permission
- Sử dụng localization từ module resource

**Ví dụ**:
```csharp
using iOne.{ModuleName}.Localization;
using iOne.{ModuleName}.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace iOne.{ModuleName}.Permissions;

public class {EntityName}PermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var {entityName}Group = context.AddGroup(
            {EntityName}Permissions.GroupName,
            L("Permission:{EntityName}")
        );

        var {entityName}Permission = {entityName}Group.AddPermission(
            {EntityName}Permissions.Default,
            L("Permission:{EntityName}")
        );

        {entityName}Permission.AddChild(
            {EntityName}Permissions.Create,
            L("Permission:Create")
        );

        {entityName}Permission.AddChild(
            {EntityName}Permissions.Edit,
            L("Permission:Edit")
        );

        {entityName}Permission.AddChild(
            {EntityName}Permissions.Delete,
            L("Permission:Delete")
        );

        {entityName}Permission.AddChild(
            {EntityName}Permissions.View,
            L("Permission:View")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<{ModuleName}Resource>(name);
    }
}
```

### 3.3. Sử dụng Permissions

**Trong Application Service**:
```csharp
using iOne.{ModuleName}.Permissions;

[Authorize({EntityName}Permissions.Create)]
public async Task<{EntityName}Dto> CreateAsync(Create{EntityName}Dto input)
{
    // ...
}
```

**Trong Controller**:
```csharp
using iOne.{ModuleName}.Permissions;

[HttpPost]
[Authorize({EntityName}Permissions.Create)]
public async Task<{EntityName}Dto> CreateAsync(Create{EntityName}Dto input)
{
    return await _appService.CreateAsync(input);
}
```

## 4. Entity Framework Core

### 4.1. Entity Configuration

**Location**: `src/common/infra/iOne.EntityFrameworkCore/{ModuleName}/{EntityName}Configuration.cs`

**Quy tắc QUAN TRỌNG**:
- **Table Name**: Phải sử dụng **snake_case** (PostgreSQL convention) (ví dụ: `"hr_employee_role"`, `"hr_department_type"`)
  - **KHÔNG** dùng UPPERCASE (ví dụ: `"HREMPLOYEEROLE"`)
  - **KHÔNG** dùng PascalCase (ví dụ: `"HrEmployeeRole"`)
  - **KHÔNG** dùng camelCase (ví dụ: `"hrEmployeeRole"`)
  - **BẮT BUỘC** dùng snake_case để phù hợp với PostgreSQL database style
- **Column Names**: Phải sử dụng **snake_case** (PostgreSQL convention) (ví dụ: `"code"`, `"name"`, `"status"`, `"creation_time"`, `"creator_id"`)
  - Column names mặc định từ property names (PascalCase) cần được convert sang snake_case
  - **BẮT BUỘC** sử dụng `HasColumnName("snake_case_name")` để override column names
  - Audit columns: `"creation_time"`, `"creator_id"`, `"last_modification_time"`, `"last_modifier_id"`, `"deletion_time"`, `"deleter_id"`, `"is_deleted"`, `"concurrency_stamp"`, `"tenant_id"`
- **Index Names**: Sử dụng **snake_case** với prefix (ví dụ: `"ix_hr_employee_role_code"`, `"pk_hr_employee_role"`)
- Configure table name, column names, constraints
- Configure indexes nếu cần
- Configure relationships nếu có

**Ví dụ**:
```csharp
public class HrEmployeeRoleConfiguration : IEntityTypeConfiguration<HrEmployeeRole>
{
    public void Configure(EntityTypeBuilder<HrEmployeeRole> builder)
    {
        // ✅ ĐÚNG: Table name theo snake_case (PostgreSQL convention)
        builder.ToTable("hr_employee_role", t =>
        {
            t.HasComment("Bảng lưu vai trò của nhân viên: CTV, NV, CV, ...");
        });
        
        // ❌ SAI: Table name không đúng convention
        // builder.ToTable("HREMPLOYEEROLE"); // UPPERCASE
        // builder.ToTable("HrEmployeeRole"); // PascalCase
        // builder.ToTable("hrEmployeeRole"); // camelCase

        builder.ConfigureByConvention();

        // ✅ ĐÚNG: Column names theo snake_case
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Status).HasColumnName("status");
        
        // ✅ ĐÚNG: Audit columns theo snake_case
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");

        // ✅ ĐÚNG: Index name theo snake_case với prefix
        builder.HasIndex(e => e.Code, "ix_hr_employee_role_code")
            .IsUnique();
    }
}
```

### 4.2. Repository Implementation

**Location**: `src/common/infra/iOne.EntityFrameworkCore/{ModuleName}/EfCore{EntityName}Repository.cs`

**Quy tắc**:
- Kế thừa từ `EfCoreRepository<iOneDbContext, {EntityName}, Guid>`
- Implement interface từ Domain layer
- Thêm custom queries nếu cần

### 4.3. Database Migrations (Không được phép tự tạo Migration, sẽ chạy lệnh bên ngoài)

**Quy tắc QUAN TRỌNG**:
- **LUÔN** sử dụng `IF EXISTS` khi DROP TABLE/COLUMN/INDEX
- **LUÔN** sử dụng `IF NOT EXISTS` khi CREATE TABLE/COLUMN/INDEX
- Điều này đảm bảo migration có thể chạy trên database mới hoặc đã có dữ liệu
- **Table và Column Names**: Phải theo **snake_case** (PostgreSQL convention) trong migration
  - Table name: `"hr_employee_role"` (snake_case)
  - Column names: `"code"`, `"name"`, `"status"`, `"creation_time"`, `"creator_id"` (snake_case)
  - Index names: `"ix_hr_employee_role_code"` (snake_case với prefix)
  - Primary key names: `"pk_hr_employee_role"` (snake_case với prefix)
  - Foreign key names: `"fk_hr_employee_role_xxx"` (snake_case với prefix)

**Ví dụ**:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ✅ ĐÚNG: Sử dụng IF EXISTS
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""hr_employee_role"";");
    
    // ❌ SAI: Không có IF EXISTS
    // migrationBuilder.DropTable("hr_employee_role");
    
    // ✅ ĐÚNG: Table name theo snake_case (PostgreSQL convention)
    migrationBuilder.CreateTable(
        name: "hr_employee_role", // snake_case
        columns: table => new
        {
            id = table.Column<Guid>(type: "uuid", nullable: false),
            code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false), // snake_case
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
            // ... other columns
        },
        constraints: table =>
        {
            // ✅ ĐÚNG: Primary key name theo snake_case với prefix
            table.PrimaryKey("pk_hr_employee_role", x => x.id);
        },
        comment: "Bảng lưu vai trò của nhân viên: CTV, NV, CV, ...");

    // ✅ ĐÚNG: Index name theo snake_case với prefix
    migrationBuilder.CreateIndex(
        name: "ix_hr_employee_role_code",
        table: "hr_employee_role",
        column: "code",
        unique: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // ✅ ĐÚNG: Sử dụng IF EXISTS khi DROP
    migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""ix_hr_employee_role_code"";");
    migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""hr_employee_role"";");
}
```

## 5. Localization

### 5.1. Localization Keys

**Quy tắc**:
- Format: `"{EntityName}:{Key}"` (ví dụ: `"HrDepartmentType:Code"`)
- Menu keys: `"Menu:{ModuleName}"`, `"Menu:{EntityName}"`
- Permission keys: `"Permission:{EntityName}"`, `"Permission:{Action}"`
- Validation messages: `"{EntityName}:{Field}Required"`, `"{EntityName}:{Field}MaxLength"`
- Success messages: `"{EntityName}:CreatedSuccessfully"`, `"{EntityName}:UpdatedSuccessfully"`

### 5.2. Localization Files

**Location**: 
- `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/Localization/{ModuleName}/vi-VN.json`
- `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/Localization/{ModuleName}/en.json`

**Quy tắc**:
- Phải có cả 2 file (vi-VN và en)
- Keys phải giống nhau ở cả 2 file
- Sử dụng placeholders `{0}`, `{1}`, `{Field}` cho dynamic values

## 6. HTTP API Controllers

### 6.1. Controller

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.HttpApi/Controllers/{EntityName}Controller.cs`

**Quy tắc**:
- Kế thừa từ `AbpControllerBase`
- Route: `"api/{module-name}/{entity-name-plural}"` (ví dụ: `"api/hr/department-types"`)
- Methods phải có `[Authorize]` với permission tương ứng
- Sử dụng `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` attributes
- Có `[RemoteService]` attribute với `Name = {ModuleName}RemoteServiceConsts.RemoteServiceName`
- Có `[Area]` attribute với `{ModuleName}RemoteServiceConsts.ModuleName`

**Ví dụ**:
```csharp
using iOne.{ModuleName};
using iOne.{ModuleName}.Permissions;

[RemoteService(Name = {ModuleName}RemoteServiceConsts.RemoteServiceName)]
[Area({ModuleName}RemoteServiceConsts.ModuleName)]
[Route("api/{module-name}/{entity-name-plural}")]
[Authorize]
public class {EntityName}Controller : AbpControllerBase
{
    protected I{EntityName}AppService AppService { get; }

    public {EntityName}Controller(I{EntityName}AppService appService)
    {
        AppService = appService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<{EntityName}Dto>> GetListAsync(Get{EntityName}sInput input)
    {
        return AppService.GetListAsync(input);
    }

    [HttpPost]
    public virtual Task<{EntityName}Dto> CreateAsync(Create{EntityName}Dto input)
    {
        return AppService.CreateAsync(input);
    }
}
```

### 6.2. Tránh Duplicate API Endpoints

**QUAN TRỌNG**: Khi có manual controller, phải exclude AppService khỏi conventional controller generation để tránh duplicate API endpoints.

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.HttpApi/iOne{ModuleName}HttpApiModule.cs`

**Quy tắc**:
- Nếu có manual controller cho AppService, phải exclude AppService khỏi conventional controller generation
- Sử dụng `TypePredicate` để filter các AppService cần exclude
- Configuration phải được đặt trong **module HttpApi**, không phải trong host module

**Ví dụ**:
```csharp
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace iOne.{ModuleName};

[DependsOn(
    typeof(iOne{ModuleName}ApplicationContractsModule),
    typeof(iOne{ModuleName}ApplicationModule), // Cần reference để access Assembly
    // ... other dependencies
)]
public class iOne{ModuleName}HttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // Create conventional controllers for module, but exclude services that have manual controllers
            options.ConventionalControllers.Create(
                typeof(iOne{ModuleName}ApplicationModule).Assembly,
                opts =>
                {
                    opts.TypePredicate = type =>
                        type.Name != "{EntityName1}AppService" &&
                        type.Name != "{EntityName2}AppService"; // Exclude các AppService có manual controller
                });
        });
    }
}
```

**Lưu ý**:
- Phải thêm `ProjectReference` đến `iOne.{ModuleName}.Application.csproj` trong `iOne.{ModuleName}.HttpApi.csproj` để access Assembly
- Nếu không exclude, ABP sẽ tự động tạo conventional controller, dẫn đến duplicate endpoints

## 7. Menu Configuration (Navigation)

### 7.1. Tạo Menu Contributor

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.Application/Navigation/{ModuleName}MenuContributor.cs`

**Quy tắc**:
- Mỗi module phải có menu contributor riêng trong module của nó
- Menu contributor phải implement `IMenuContributor`
- Menu items phải có permission check bằng `RequirePermissions()`
- Sử dụng localization từ module resource

**Ví dụ**:
```csharp
using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Authorization.Permissions;
using iOne.{ModuleName}.Localization;
using iOne.{ModuleName}.Permissions;

namespace iOne.{ModuleName}.Navigation;

public class {ModuleName}MenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var {moduleName}L = context.GetLocalizer<{ModuleName}Resource>();

        // {ModuleName} Menu
        var {moduleName}MenuItem = new ApplicationMenuItem(
            "{ModuleName}",
            {moduleName}L["Menu:{ModuleName}"],
            icon: "pi pi-fw pi-{icon}"
        );
        context.Menu.AddItem({moduleName}MenuItem);

        {moduleName}MenuItem.AddItem(new ApplicationMenuItem(
            "{ModuleName}.{EntityName}",
            {moduleName}L["Menu:{EntityName}"],
            url: "~/pages/{module-name}/{entity-name-plural}",
            icon: "pi pi-fw pi-{icon}"
        ).RequirePermissions({EntityName}Permissions.Default));

        await Task.CompletedTask;
    }
}
```

### 7.2. Đăng Ký Menu Contributor

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.Application/iOne{ModuleName}ApplicationModule.cs`

**Quy tắc**:
- Phải đăng ký menu contributor trong `ConfigureServices` của Application Module
- Sử dụng `Configure<AbpNavigationOptions>` để đăng ký
- Phải thêm package reference `Volo.Abp.UI.Navigation` vào `.csproj`

**Ví dụ**:
```csharp
using iOne.{ModuleName}.Localization;
using iOne.{ModuleName}.Navigation;
using Volo.Abp.UI.Navigation;

namespace iOne.{ModuleName};

public class iOne{ModuleName}ApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // ... other configurations

        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new {ModuleName}MenuContributor());
        });
    }
}
```

**Package Reference**:
```xml
<PackageReference Include="Volo.Abp.UI.Navigation" Version="9.3.6" />
```

### 7.3. Localization Keys cho Menu

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/Localization/{ModuleName}/vi-VN.json` và `en.json`

**Quy tắc**:
- Menu keys: `"Menu:{ModuleName}"`, `"Menu:{EntityName}"`
- Keys phải có trong cả 2 file (vi-VN và en)

**Ví dụ**:
```json
{
  "Menu:{ModuleName}": "Tên Module",
  "Menu:{EntityName}": "Tên Entity"
}
```

## 8. Checklist Trước Khi Hoàn Thành

**Lưu ý**: Checklist này bao gồm cả PostgreSQL naming convention (snake_case).

- [ ] Entity có đầy đủ validation attributes
  - [ ] Table name sử dụng `[Table("snake_case_name")]` (PostgreSQL convention)
  - [ ] Column names sử dụng `[Column("snake_case_name")]` nếu cần override
- [ ] Repository interface và implementation đã tạo
- [ ] Application service có đầy đủ CRUD operations
- [ ] **AutoMapper configuration đã được thêm vào AutoMapper Profile**
  - [ ] Mapping `Entity` -> `{EntityName}Dto`
  - [ ] Mapping `Create{EntityName}Dto` -> `Entity`
  - [ ] Mapping `Update{EntityName}Dto` -> `Entity`
- [ ] Permissions đã được định nghĩa và sử dụng đúng
- [ ] EF Core configuration đã đúng
  - [ ] Table name sử dụng snake_case (PostgreSQL convention): `builder.ToTable("snake_case_name")`
  - [ ] Tất cả column names sử dụng snake_case (bao gồm audit columns): `builder.Property(x => x.Code).HasColumnName("code")`
  - [ ] Index names sử dụng snake_case với prefix phù hợp: `builder.HasIndex(..., "ix_snake_case_name")`
  - [ ] Primary key name sử dụng snake_case với prefix `pk_`
  - [ ] Foreign key names (nếu có) sử dụng snake_case với prefix `fk_`
- [ ] Migration đã được tạo và test (với IF EXISTS/IF NOT EXISTS)
  - [ ] Table name trong migration sử dụng snake_case
  - [ ] Column names trong migration sử dụng snake_case
  - [ ] Index names trong migration sử dụng snake_case với prefix
- [ ] Localization keys đã đầy đủ (vi-VN và en)
- [ ] Controller đã có đầy đủ endpoints với authorization
- [ ] **Menu Contributor đã được tạo và đăng ký trong Application Module**
- [ ] **Conventional Controllers đã được config để tránh duplicate API (nếu có manual controller)**
- [ ] **Package reference `Volo.Abp.UI.Navigation` đã được thêm vào `.csproj`**
- [ ] Build solution thành công
- [ ] Test API endpoints thành công
- [ ] **Menu hiển thị đúng trên client**

## 9. PostgreSQL Database Naming Convention

### 9.1. Tổng Quan

**QUAN TRỌNG**: Tất cả database objects (tables, columns, indexes, constraints) phải tuân theo **snake_case** convention để phù hợp với PostgreSQL database style.

### 9.2. Table Names

- **Format**: `snake_case` (chữ thường, phân cách bằng dấu gạch dưới)
- **Ví dụ đúng**: 
  - `"hr_employee_role"`
  - `"hr_department_type"`
  - `"procurement_purchase_order"`
- **Ví dụ sai**:
  - `"HrEmployeeRole"` (PascalCase)
  - `"HREMPLOYEEROLE"` (UPPERCASE)
  - `"hrEmployeeRole"` (camelCase)

### 9.3. Column Names

- **Format**: `snake_case` (chữ thường, phân cách bằng dấu gạch dưới)
- **Ví dụ đúng**:
  - `"code"`, `"name"`, `"status"`
  - `"creation_time"`, `"creator_id"`
  - `"last_modification_time"`, `"last_modifier_id"`
  - `"deletion_time"`, `"deleter_id"`
  - `"is_deleted"`, `"concurrency_stamp"`, `"tenant_id"`
- **Ví dụ sai**:
  - `"Code"`, `"Name"`, `"Status"` (PascalCase)
  - `"CreationTime"`, `"CreatorId"` (PascalCase)
  - `"CREATION_TIME"` (UPPERCASE)

### 9.4. Index Names

- **Format**: `snake_case` với prefix mô tả loại index
- **Prefixes**:
  - `"ix_"` cho indexes thông thường
  - `"pk_"` cho primary keys
  - `"fk_"` cho foreign keys
  - `"uq_"` cho unique constraints
- **Ví dụ đúng**:
  - `"ix_hr_employee_role_code"` (index trên column code)
  - `"pk_hr_employee_role"` (primary key)
  - `"fk_hr_employee_role_department_id"` (foreign key)
  - `"uq_hr_employee_role_code"` (unique constraint)
- **Ví dụ sai**:
  - `"IX_HrEmployeeRole_Code"` (PascalCase)
  - `"PK_HrEmployeeRole"` (PascalCase)

### 9.5. Implementation trong Entity Configuration

**BẮT BUỘC**: Khi tạo Entity Configuration, phải:
1. Set table name bằng `ToTable("snake_case_name")`
2. Override tất cả column names bằng `HasColumnName("snake_case_name")`
3. Set index names bằng `HasIndex(..., "ix_snake_case_name")`

**Ví dụ đầy đủ**:
```csharp
public class HrEmployeeRoleConfiguration : IEntityTypeConfiguration<HrEmployeeRole>
{
    public void Configure(EntityTypeBuilder<HrEmployeeRole> builder)
    {
        // ✅ Table name: snake_case
        builder.ToTable("hr_employee_role", t =>
        {
            t.HasComment("Bảng lưu vai trò của nhân viên: CTV, NV, CV, ...");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (BẮT BUỘC override)
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Status).HasColumnName("status");
        
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

        // ✅ Index name: snake_case với prefix
        builder.HasIndex(e => e.Code, "ix_hr_employee_role_code")
            .IsUnique();
    }
}
```

### 9.6. Implementation trong Entity (Data Annotations)

**BẮT BUỘC**: Khi sử dụng Data Annotations trên Entity class:
1. Set table name bằng `[Table("snake_case_name")]`
2. Override column names bằng `[Column("snake_case_name")]` (nếu cần)

**Ví dụ**:
```csharp
using System.ComponentModel.DataAnnotations.Schema;

[Table("hr_employee_role")] // ✅ snake_case
public class HrEmployeeRole : FullAuditedAggregateRoot<Guid>
{
    [Column("code")] // ✅ snake_case
    [Required]
    [MaxLength(50)]
    public string Code { get; set; }
    
    [Column("name")] // ✅ snake_case
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
    
    [Column("status")] // ✅ snake_case
    [Required]
    public HrEmployeeRoleStatus Status { get; set; }
}
```

### 9.7. Migration Scripts

**BẮT BUỘC**: Khi tạo migration scripts, phải sử dụng snake_case cho tất cả database objects:
- Table names: `"hr_employee_role"`
- Column names: `"code"`, `"name"`, `"status"`
- Index names: `"ix_hr_employee_role_code"`
- Primary key names: `"pk_hr_employee_role"`
- Foreign key names: `"fk_hr_employee_role_xxx"`

### 9.8. Checklist

Trước khi hoàn thành Entity Configuration, đảm bảo:
- [ ] Table name sử dụng snake_case
- [ ] Tất cả column names sử dụng snake_case (bao gồm audit columns)
- [ ] Index names sử dụng snake_case với prefix phù hợp
- [ ] Primary key name sử dụng snake_case với prefix `pk_`
- [ ] Foreign key names (nếu có) sử dụng snake_case với prefix `fk_`
- [ ] Migration scripts sử dụng snake_case cho tất cả database objects

## 10. Lưu Ý Quan Trọng

1. **Permission Location**: 
   - **QUAN TRỌNG**: Permission Constants và PermissionDefinitionProvider phải được đặt trong **module**, không phải trong `common/domain`
   - Location: `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/Permissions/`
   - Namespace: `iOne.{ModuleName}.Permissions`
2. **Permission Name**: Frontend sử dụng `GroupName` (ví dụ: `"HrDepartmentType"`), không phải `"{GroupName}.Default"`
3. **Permission Structure**: 
   - Default permission = `GroupName` (không phải `"{GroupName}.Default"`)
   - Default permission là parent permission
   - Create, Edit, Delete, View là child permissions của Default
4. **Database Naming Convention (PostgreSQL Style)**: 
   - **QUAN TRỌNG**: Table name và column name phải theo **snake_case** (PostgreSQL convention)
   - Table name: `"hr_employee_role"` (snake_case), không phải `"HREMPLOYEEROLE"` (UPPERCASE) hay `"HrEmployeeRole"` (PascalCase)
   - Column names: `"code"`, `"name"`, `"status"`, `"creation_time"`, `"creator_id"` (snake_case)
   - Index names: `"ix_hr_employee_role_code"` (snake_case với prefix)
   - Primary key names: `"pk_hr_employee_role"` (snake_case với prefix)
   - Foreign key names: `"fk_hr_employee_role_xxx"` (snake_case với prefix)
   - **BẮT BUỘC**: Tất cả database objects (tables, columns, indexes, constraints) phải sử dụng snake_case để phù hợp với PostgreSQL database style
5. **Migration Safety**: LUÔN dùng `IF EXISTS`/`IF NOT EXISTS` để tránh lỗi khi migration
6. **Localization**: Đảm bảo có cả vi-VN và en, và keys phải match
7. **DTOs**: Không expose Entity trực tiếp, luôn dùng DTOs
8. **Validation**: Validate ở cả Domain layer (attributes) và Application layer (business rules)
9. **Menu Configuration**:
   - **QUAN TRỌNG**: Menu contributor phải được đặt trong **module Application**, không phải trong module gốc
   - Location: `modules/{ModuleName}/src/iOne.{ModuleName}.Application/Navigation/{ModuleName}MenuContributor.cs`
   - Phải đăng ký trong `iOne{ModuleName}ApplicationModule.ConfigureServices()`
   - Phải thêm package reference `Volo.Abp.UI.Navigation` vào `.csproj`
10. **Tránh Duplicate API**:
   - **QUAN TRỌNG**: Nếu có manual controller cho AppService, phải exclude AppService khỏi conventional controller generation
   - Configuration phải được đặt trong **module HttpApi** và **module HttpApiHost** của module chức năng
   - Phải thêm `ProjectReference` đến `iOne.{ModuleName}.Application.csproj` trong `iOne.{ModuleName}.HttpApi.csproj`
   - Sử dụng `TypePredicate` để exclude các AppService có manual controller
11. **AutoMapper Configuration**:
   - **QUAN TRỌNG**: **BẮT BUỘC** phải thêm mapping configuration trong AutoMapper Profile
   - Location: `modules/{ModuleName}/src/iOne.{ModuleName}.Application/iOne{ModuleName}ApplicationAutoMapperProfile.cs`
   - Phải thêm mapping cho:
     - `Entity` -> `{EntityName}Dto` (BẮT BUỘC - dùng khi return từ AppService)
     - `Create{EntityName}Dto` -> `Entity` (Nên thêm để tránh lỗi, dù có thể không dùng nếu Entity có private setters)
     - `Update{EntityName}Dto` -> `Entity` (Nên thêm để tránh lỗi, dù có thể không dùng nếu Entity có private setters)
   - Nếu thiếu mapping, sẽ gặp lỗi: `"Missing type map configuration or unsupported mapping"`
   - Lỗi thường xảy ra khi gọi `ObjectMapper.Map<Entity, Dto>(entity)` trong AppService

