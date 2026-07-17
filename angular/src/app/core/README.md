# Core Module

Các hệ thống core tương tự ABP Framework cho Angular project.

## Modules

- **Permission System** - Hệ thống phân quyền
- **Localization System** - Hệ thống đa ngôn ngữ (i18n)

Xem chi tiết:
- [Permission System Documentation](./README.md#permission-system)
- [Localization System Documentation](./README-LOCALIZATION.md)

---

## Permission System

## Cấu trúc

- **Services**: `PermissionService` - Service chính để kiểm tra permissions
- **Guards**: `PermissionGuard` - Route guard để bảo vệ routes
- **Directives**: `PermissionDirective` - Directive để ẩn/hiện elements
- **Pipes**: `PermissionPipe` - Pipe để sử dụng trong templates
- **Utils**: Menu permission utilities - Helper functions cho menu

## Cách sử dụng

### 1. PermissionService

```typescript
import { PermissionService } from '@core/services/permission.service';

// Inject service
constructor(private permissionService: PermissionService) {}

// Kiểm tra một permission
const hasPermission = this.permissionService.getGrantedPolicy('Admin.Users');

// Kiểm tra nhiều permissions (chỉ cần một)
const hasAnyPermission = this.permissionService.isGranted(['Admin.Users', 'Admin.Roles']);

// Kiểm tra nhiều permissions (cần tất cả)
const hasAllPermissions = this.permissionService.isGranted(['Admin.Users', 'Admin.Roles'], true);

// Observable
this.permissionService.isGranted$('Admin.Users').subscribe(hasPermission => {
  // ...
});
```

### 2. PermissionGuard (Route Protection)

```typescript
import { permissionGuard } from '@core/guards/permission.guard';

// Trong app.routes.ts hoặc feature routes
{
  path: 'admin',
  component: AdminComponent,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'Admin.Users' // Một permission
  }
}

// Hoặc nhiều permissions
{
  path: 'admin',
  component: AdminComponent,
  canActivate: [permissionGuard],
  data: {
    requiredPermissions: ['Admin.Users', 'Admin.Roles'],
    requireAll: false // false = chỉ cần một, true = cần tất cả
  }
}
```

### 3. PermissionDirective (Template)

```html
<!-- Ẩn/hiện element dựa trên permission -->
<div *appPermission="'Admin.Users'">
  Content for users with Admin.Users permission
</div>

<!-- Nhiều permissions (chỉ cần một) -->
<div *appPermission="['Admin.Users', 'Admin.Roles']">
  Content for users with either permission
</div>

<!-- Nhiều permissions (cần tất cả) -->
<div *appPermission="['Admin.Users', 'Admin.Roles']; requireAll: true">
  Content for users with both permissions
</div>
```

### 4. PermissionPipe (Template)

```html
<!-- Sử dụng với *ngIf -->
<div *ngIf="'Admin.Users' | appPermission">
  Content
</div>

<!-- Nhiều permissions -->
<div *ngIf="['Admin.Users', 'Admin.Roles'] | appPermission:false">
  Content (chỉ cần một)
</div>

<div *ngIf="['Admin.Users', 'Admin.Roles'] | appPermission:true">
  Content (cần tất cả)
</div>
```

### 5. Menu với Permission

```typescript
import { MenuItemWithPermission } from '@core/utils/menu-permission.util';

// Trong app.menu.ts
model: MenuItemWithPermission[] = [
  {
    label: 'Admin',
    icon: 'pi pi-fw pi-shield',
    permission: 'Admin', // Menu chỉ hiển thị nếu có permission này
    items: [
      {
        label: 'Users',
        icon: 'pi pi-fw pi-users',
        routerLink: ['/admin/users'],
        permission: 'Admin.Users' // Sub-menu item cũng có permission
      },
      {
        label: 'Roles',
        icon: 'pi pi-fw pi-key',
        routerLink: ['/admin/roles'],
        permission: 'Admin.Roles'
      }
    ]
  }
];
```

Menu sẽ tự động filter dựa trên permissions của user.

## Permission Name Convention

Format: `{Module}.{Feature}.{Action}`

Examples:
- `Admin.Users` - Access to Users module
- `Admin.Users.Create` - Create user permission
- `Admin.Users.Edit` - Edit user permission
- `Admin.Users.Delete` - Delete user permission
- `Admin.Roles` - Access to Roles module

## Lưu ý

1. Permissions được lấy từ `ConfigStateService` (ABP Core)
2. Permissions được cache trong `grantedPolicies` object
3. Nếu không có permission requirement, mặc định cho phép (return true)
4. PermissionGuard sẽ redirect đến `/auth/access` nếu không có permission

## Integration với ABP

Hệ thống này tương thích hoàn toàn với ABP Framework:
- Sử dụng `ConfigStateService` từ `@abp/ng.core`
- Permissions được lấy từ `ApplicationConfigurationDto.grantedPolicies`
- Tương thích với ABP permission system trên backend
