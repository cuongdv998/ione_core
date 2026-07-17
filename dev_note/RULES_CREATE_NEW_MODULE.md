# Quy Tắc Tạo Module Rỗng Mới

## 1. Cấu Trúc Thư Mục Module

```
modules/
└── {ModuleName}/
    └── src/
        ├── iOne.{ModuleName}.Application/
        ├── iOne.{ModuleName}.Application.Contracts/
        ├── iOne.{ModuleName}.HttpApi/
        ├── iOne.{ModuleName}.HttpApi.Client/
        └── iOne.{ModuleName}.HttpApi.Host/
```

## 2. Đặt Tên Module

- **Quy tắc**: PascalCase, không có khoảng trắng, không có ký tự đặc biệt
- **Ví dụ**: `Hr`, `Finance`, `Inventory`
- **Lưu ý**: Tên module sẽ được dùng trong namespace, class name, và folder structure

## 3. Cấu Hình Module

### 3.1. Application.Contracts Module

**File**: `iOne.{ModuleName}.Application.Contracts/iOne{ModuleName}ApplicationContractsModule.cs`

```csharp
[DependsOn(
    typeof(iOneDomainSharedModule)
)]
public class iOne{ModuleName}ApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Localization configuration
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<{ModuleName}Resource>("vi-VN")
                .AddBaseTypes(typeof(AbpValidationResource), typeof(AbpUiResource))
                .AddVirtualJson("/Localization/{ModuleName}");
        });
    }
}
```

### 3.2. Localization Resource

**File**: `iOne.{ModuleName}.Application.Contracts/Localization/{ModuleName}/{ModuleName}Resource.cs`

```csharp
[LocalizationResourceName("{ModuleName}")]
public class {ModuleName}Resource
{
}
```

### 3.3. Localization Files

**Files**: 
- `Localization/{ModuleName}/vi-VN.json`
- `Localization/{ModuleName}/en.json`

**Cấu trúc tối thiểu**:
```json
{
  "Culture": "vi-VN",
  "Texts": {
    "Menu:{ModuleName}": "Tên Module",
    "Permission:{EntityName}": "Tên Entity",
    "Permission:Create": "Tạo mới",
    "Permission:Edit": "Sửa",
    "Permission:Delete": "Xóa",
    "Permission:View": "Xem"
  }
}
```

## 4. Project File Configuration

**File**: `iOne.{ModuleName}.Application.Contracts/iOne.{ModuleName}.Application.Contracts.csproj`

```xml
<PropertyGroup>
  <GenerateEmbeddedFilesManifest>true</GenerateEmbeddedFilesManifest>
</PropertyGroup>
```

## 5. Permissions

### 5.1. Permission Constants

**Location**: `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/Permissions/{EntityName}Permissions.cs`

**Quy tắc QUAN TRỌNG**:
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

**Lưu ý**: 
- Permission Constants phải được đặt trong **module**, không phải trong `common/domain`
- Namespace: `iOne.{ModuleName}.Permissions`
- File location: `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/Permissions/`

### 5.2. Permission Definition Provider

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

### 5.3. Sử dụng Permissions

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
public virtual Task<{EntityName}Dto> CreateAsync(Create{EntityName}Dto input)
{
    return AppService.CreateAsync(input);
}
```

**Trong Frontend (Angular Route)**:
```typescript
{
    path: '{module-name}/{entity-name-plural}',
    component: {EntityName}Component,
    canActivate: [permissionGuard],
    data: {
        requiredPermission: '{ModuleName}{EntityName}', // ✅ Sử dụng GroupName
        // ❌ KHÔNG dùng '{ModuleName}{EntityName}.Default'
        breadcrumb: '{ModuleName}::Menu:{EntityName}'
    }
}
```

**Trong Frontend (Menu)**:
```typescript
{
    label: this.localizationService.localize('{ModuleName}::Menu:{EntityName}'),
    routerLink: ['/pages/{module-name}/{entity-name-plural}'],
    visible: this.permissionService.isGranted('{ModuleName}{EntityName}') || 
             this.permissionService.isGranted('{ModuleName}{EntityName}.View')
}
```

## 6. HttpApi.Host Module

**File**: `iOne.{ModuleName}.HttpApi.Host/iOne{ModuleName}HttpApiHostModule.cs`

- Đảm bảo có `ConfigureVirtualFileSystem` để load localization files trong development mode
- Đảm bảo có `ConfigureConventionalControllers` để register controllers

## 7. Kiểm Tra Sau Khi Tạo

- [ ] Build solution thành công
- [ ] Localization files được load đúng
- [ ] Module có thể được reference từ các project khác
- [ ] Không có circular dependencies

## 8. Lưu Ý Quan Trọng

1. **Namespace**: Sử dụng `iOne.{ModuleName}` cho tất cả namespace
2. **Resource Name**: Sử dụng `{ModuleName}` cho LocalizationResourceName (không có prefix "iOne")
3. **Base Types**: Luôn thêm `AbpUiResource` để inherit các key UI chung (All, Success, Error, etc.)
4. **Embedded Files**: Luôn set `GenerateEmbeddedFilesManifest` để đảm bảo JSON files được embed đúng
5. **Permission Location**: 
   - **QUAN TRỌNG**: Permission Constants và PermissionDefinitionProvider phải được đặt trong **module**, không phải trong `common/domain`
   - Location: `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/Permissions/`
   - Namespace: `iOne.{ModuleName}.Permissions`
6. **Permission GroupName**: 
   - `GroupName` phải match với permission name được dùng trong frontend
   - Default permission = `GroupName` (không phải `"{GroupName}.Default"`)
   - Frontend sử dụng `GroupName` cho `requiredPermission` trong route guard
7. **Permission Structure**: 
   - Default permission là parent permission
   - Create, Edit, Delete, View là child permissions của Default
   - Khi user có Default permission, họ tự động có tất cả child permissions

