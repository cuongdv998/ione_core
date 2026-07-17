# Permissions

ThÆ° má»¥c nÃ y chá»©a cÃ¡c permission definitions cho module Payment.

## Cáº¥u trÃºc

Khi táº¡o permissions cho má»™t Entity, táº¡o 2 files:

1. `{EntityName}Permissions.cs` - Chá»©a constants cho permissions
2. `{EntityName}PermissionDefinitionProvider.cs` - Äá»‹nh nghÄ©a permissions

## VÃ­ dá»¥

Xem module HR Ä‘á»ƒ tham kháº£o:
- `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrDepartmentTypePermissions.cs`
- `modules/hr/src/iOne.Hr.Application.Contracts/Permissions/HrDepartmentTypePermissionDefinitionProvider.cs`

## Quy táº¯c

- `GroupName` = `"{ModuleName}{EntityName}"` (vÃ­ dá»¥: `"PaymentPaymentType"`)
- `Default` = `GroupName` (khÃ´ng pháº£i `"{GroupName}.Default"`)
- CÃ¡c permission names = `"{GroupName}.{Action}"` (vÃ­ dá»¥: `"PaymentPaymentType.Create"`)

Xem thÃªm: `dev_note/RULES_CREATE_NEW_MODULE.md`

