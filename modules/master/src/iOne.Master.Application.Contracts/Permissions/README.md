# Permissions

Thư mục này chứa các permission definitions cho module Master.

## Cấu trúc

Khi tạo permissions cho một Entity, tạo 2 files:

1. `{EntityName}Permissions.cs` - Chứa constants cho permissions
2. `{EntityName}PermissionDefinitionProvider.cs` - Định nghĩa permissions

## Ví dụ

Xem module HR để tham khảo:
- `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrDepartmentTypePermissions.cs`
- `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrDepartmentTypePermissionDefinitionProvider.cs`

## Quy tắc

- `GroupName` = `"{ModuleName}{EntityName}"` (ví dụ: `"MasterMasterType"`)
- `Default` = `GroupName` (không phải `"{GroupName}.Default"`)
- Các permission names = `"{GroupName}.{Action}"` (ví dụ: `"MasterMasterType.Create"`)

Xem thêm: `dev_note/RULES_CREATE_NEW_MODULE.md`

