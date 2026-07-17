# Quản lý vai trò (Role Management)

## Tổng quan
Module quản lý vai trò (roles) trong hệ thống Identity.

## 🔐 Permissions
Chức năng này yêu cầu permissions:
- `AbpIdentity.Roles` - Truy cập trang
- `AbpIdentity.Roles.Create` - Tạo vai trò mới
- `AbpIdentity.Roles.Update` - Cập nhật vai trò
- `AbpIdentity.Roles.Delete` - Xóa vai trò

## 🎯 Tính năng

### Tìm kiếm
- ✅ Filter theo tên vai trò
- ✅ Reset & Search buttons

### Bảng dữ liệu
- ✅ 3 columns (Name, IsDefault, IsPublic)
- ✅ Server-side pagination
- ✅ Column sorting
- ✅ Actions: Edit, Delete (theo permissions)

### Create/Edit Dialog
- ✅ Tạo vai trò mới
- ✅ Cập nhật vai trò
- ✅ Checkbox IsDefault, IsPublic
- ✅ Không cho phép đổi tên vai trò Static
- ✅ Không cho phép xóa vai trò Static

## 🔧 Tech Stack
- Angular 20
- PrimeNG 20
- TypeScript 5.8
- Tailwind CSS
- ABP Framework (Permission system + Localization)

## 🌐 Localization
Sử dụng resource: `AbpIdentity`

Các keys chính:
- `Roles`, `NewRole`, `RoleName`
- `DisplayName:IsDefault`, `DisplayName:IsPublic`
- `RoleCreatedMessage`, `RoleUpdatedMessage`, `RoleDeletedMessage`
- `StaticRolesDeletionErrorMessage`

## 📁 Cấu trúc Files

```
roles/
├── roles.component.ts       # Component logic với permission checks
├── roles.component.html     # Template với localization
├── roles.component.scss     # Styles
├── roles.models.ts          # Type definitions
├── index.ts                 # Barrel export
└── README.md                # This file
```

## 🚀 Sử dụng

### Import
```typescript
import { RolesComponent } from '@/pages/roles';
```

### Route
```typescript
{
  path: 'roles',
  component: RolesComponent,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'AbpIdentity.Roles'
  }
}
```

## 📝 API Endpoints

- `GET /api/identity/roles` - Danh sách vai trò (paged)
- `GET /api/identity/roles/all` - Tất cả vai trò
- `GET /api/identity/roles/{id}` - Chi tiết vai trò
- `POST /api/identity/roles` - Tạo vai trò
- `PUT /api/identity/roles/{id}` - Cập nhật vai trò
- `DELETE /api/identity/roles/{id}` - Xóa vai trò

## ⚠️ Lưu ý

### Static Roles
- Vai trò Static không thể đổi tên
- Vai trò Static không thể xóa
- System sẽ hiển thị warning khi cố gắng xóa

### IsDefault
- Vai trò Default sẽ tự động được gán cho user mới

### IsPublic
- Vai trò Public có thể được xem bởi các user khác

## 🐛 Troubleshooting

### Không thấy menu "Quản lý vai trò"
**Kiểm tra:** User có permission `AbpIdentity.Roles`?

### Không thấy button "New Role"
**Kiểm tra:** User có permission `AbpIdentity.Roles.Create`?

### Không thấy action Edit/Delete
**Kiểm tra:** User có permissions `AbpIdentity.Roles.Update` và `AbpIdentity.Roles.Delete`?

---

**Version:** 1.0.0  
**Status:** ✅ Production Ready

