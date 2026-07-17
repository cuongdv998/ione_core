# Permissions - Audit Logs

## 🔐 Cấu hình Permissions

### Backend Permissions

Được định nghĩa trong: `AuditLoggingPermissions.cs`

```csharp
public static class AuditLoggingPermissions
{
    public const string GroupName = "AbpAuditLogging";

    public static class AuditLogs
    {
        public const string Default = "AbpAuditLogging.AuditLogs";
        public const string View = "AbpAuditLogging.AuditLogs.View";
        public const string ViewDetails = "AbpAuditLogging.AuditLogs.ViewDetails";
        public const string ViewEntityChanges = "AbpAuditLogging.AuditLogs.ViewEntityChanges";
    }
}
```

#### Chi tiết từng permission:

| Permission | Mô tả | Apply tại |
|-----------|-------|-----------|
| `AbpAuditLogging.AuditLogs` | Truy cập trang | Route guard, Menu |
| `AbpAuditLogging.AuditLogs.View` | Xem danh sách, tìm kiếm | Button Tìm kiếm, Button Reset |
| `AbpAuditLogging.AuditLogs.ViewDetails` | Xem chi tiết audit log | Button "Xem chi tiết", Tab Actions |
| `AbpAuditLogging.AuditLogs.ViewEntityChanges` | Xem thay đổi entity | Tab "Thay đổi thực thể" |

### Frontend Implementation

#### Tổng quan UI Elements & Permissions

```
Trang Audit Logs
│
├── Route Access (AbpAuditLogging.AuditLogs)
│
├── Search Panel
│   ├── Button "Tìm kiếm" (AbpAuditLogging.AuditLogs.View)
│   └── Button "Đặt lại" (AbpAuditLogging.AuditLogs.View)
│
├── Data Table
│   └── Action "Xem chi tiết" (AbpAuditLogging.AuditLogs.ViewDetails)
│
└── Detail Dialog
    ├── Tab "Thông tin chung" (Luôn hiển thị)
    ├── Tab "Thay đổi thực thể" (AbpAuditLogging.AuditLogs.ViewEntityChanges)
    └── Tab "Actions" (AbpAuditLogging.AuditLogs.ViewDetails)
```

#### 1. Route Guard
File: `pages.routes.ts`

```typescript
{
    path: 'audit-logs', 
    component: AuditLogsComponent,
    canActivate: [permissionGuard],
    data: {
        requiredPermission: 'AbpAuditLogging.AuditLogs'
    }
}
```

**Chức năng:**
- Kiểm tra permission trước khi cho phép truy cập route
- Nếu không có permission → redirect đến `/auth/access`
- Hiển thị return URL để quay lại sau khi có quyền

#### 2. Menu Item
File: `app.menu.ts`

```typescript
{
    label: 'Tra cứu tác động',
    icon: 'pi pi-fw pi-history',
    routerLink: ['/pages/audit-logs'],
    requiredPermission: 'AbpAuditLogging.AuditLogs'
}
```

**Chức năng:**
- Menu item chỉ hiển thị khi user có permission
- Tự động ẩn nếu không có quyền
- Sử dụng `filterMenuItemsByPermission` utility

## 🔧 Cách hoạt động

### Permission Flow

```
User Login
    ↓
Backend trả về grantedPolicies trong auth config
    ↓
ConfigStateService lưu vào state
    ↓
PermissionService kiểm tra từ state
    ↓
Guard/Directive/Pipe sử dụng PermissionService
    ↓
Allow/Deny access
```

### Permission Service

```typescript
// Check single permission
permissionService.isGranted('AbpAuditLogging.AuditLogs')

// Check multiple permissions (OR)
permissionService.isGranted(['Permission1', 'Permission2'], false)

// Check multiple permissions (AND)
permissionService.isGranted(['Permission1', 'Permission2'], true)

// Observable
permissionService.isGranted$('AbpAuditLogging.AuditLogs')
```

## 📝 Permission Implementation Details

### Component Code

```typescript
// audit-logs.component.ts
export class AuditLogsComponent {
  // Define permissions as constants
  readonly PERMISSIONS = {
    VIEW: 'AbpAuditLogging.AuditLogs.View',
    VIEW_DETAILS: 'AbpAuditLogging.AuditLogs.ViewDetails',
    VIEW_ENTITY_CHANGES: 'AbpAuditLogging.AuditLogs.ViewEntityChanges'
  };

  constructor(
    private permissionService: PermissionService
  ) {
    this.initializeActions();
  }

  // Initialize table actions based on permissions
  private initializeActions(): void {
    if (this.permissionService.isGranted(this.PERMISSIONS.VIEW_DETAILS)) {
      this.actions.push({
        label: 'Xem chi tiết',
        icon: 'pi pi-eye',
        command: (row) => this.viewDetail(row)
      });
    }
  }
}
```

### Template Code

```html
<!-- Search buttons với permission check -->
<p-button
  *ngIf="PERMISSIONS.VIEW | appPermission"
  label="Tìm kiếm"
  (onClick)="search()"
/>

<!-- Tabs với permission check -->
<p-tab *ngIf="PERMISSIONS.VIEW_ENTITY_CHANGES | appPermission" value="1">
  Thay đổi thực thể
</p-tab>
```

## 📝 Các cách sử dụng Permission

### 1. Route Guard (Đã áp dụng)
```typescript
{
    path: 'audit-logs',
    canActivate: [permissionGuard],
    data: { requiredPermission: 'AbpAuditLogging.AuditLogs' }
}
```

### 2. Directive (Trong template)
```html
<div *appPermission="'AbpAuditLogging.AuditLogs'">
    Nội dung chỉ hiển thị khi có permission
</div>

<!-- Multiple permissions (OR) -->
<div *appPermission="['Permission1', 'Permission2']">
    Content
</div>

<!-- Multiple permissions (AND) -->
<div *appPermission="['Permission1', 'Permission2']; requireAll: true">
    Content
</div>
```

### 3. Pipe (Trong template)
```html
<button *ngIf="'AbpAuditLogging.AuditLogs' | appPermission">
    Button chỉ hiển thị khi có permission
</button>
```

### 4. Service (Trong component)
```typescript
export class MyComponent {
    constructor(private permissionService: PermissionService) {}

    ngOnInit() {
        if (this.permissionService.isGranted('AbpAuditLogging.AuditLogs')) {
            // Do something
        }
    }
}
```

### 5. Menu (Đã áp dụng)
```typescript
{
    label: 'Menu Item',
    routerLink: ['/path'],
    requiredPermission: 'Permission.Name'
}
```

## 🎯 Best Practices

### ✅ Nên làm
1. **Luôn protect routes với guard**
   ```typescript
   canActivate: [permissionGuard]
   ```

2. **Ẩn UI elements không có quyền**
   ```html
   <button *appPermission="'Permission'">Action</button>
   ```

3. **Check permission trong component khi cần**
   ```typescript
   if (this.permissionService.isGranted('Permission')) { ... }
   ```

4. **Sử dụng menu permission để tự động ẩn menu**
   ```typescript
   requiredPermission: 'Permission.Name'
   ```

### ❌ Không nên
1. **Chỉ dựa vào UI để bảo vệ**
   - Backend phải validate permission
   - Frontend chỉ là UX enhancement

2. **Hard-code permission strings**
   - Nên tạo constants file
   ```typescript
   export const PERMISSIONS = {
       AUDIT_LOGS: 'AbpAuditLogging.AuditLogs'
   };
   ```

3. **Quên handle access denied**
   - Luôn có fallback UI
   - Hiển thị message rõ ràng

## 🔄 Testing Permissions

### Test Matrix

| Scenario | Permission | Kết quả mong đợi |
|----------|-----------|------------------|
| **Access Route** | `AbpAuditLogging.AuditLogs` | ✅ Vào được trang / ❌ Redirect to access denied |
| **View Menu** | `AbpAuditLogging.AuditLogs` | ✅ Menu hiển thị / ❌ Menu ẩn |
| **Search Button** | `AbpAuditLogging.AuditLogs.View` | ✅ Button hiển thị / ❌ Button ẩn |
| **Reset Button** | `AbpAuditLogging.AuditLogs.View` | ✅ Button hiển thị / ❌ Button ẩn |
| **Detail Action** | `AbpAuditLogging.AuditLogs.ViewDetails` | ✅ Action hiển thị / ❌ Action ẩn |
| **Entity Changes Tab** | `AbpAuditLogging.AuditLogs.ViewEntityChanges` | ✅ Tab hiển thị / ❌ Tab ẩn |
| **Actions Tab** | `AbpAuditLogging.AuditLogs.ViewDetails` | ✅ Tab hiển thị / ❌ Tab ẩn |

### 1. Test với Admin (Full permissions)
```
✅ Menu "Tra cứu tác động" hiển thị
✅ Truy cập trang OK
✅ Button "Tìm kiếm" hiển thị
✅ Button "Đặt lại" hiển thị
✅ Button "Xem chi tiết" hiển thị trong table
✅ Tab "Thay đổi thực thể" hiển thị
✅ Tab "Actions" hiển thị
```

### 2. Test với User chỉ có View
```
Permission: AbpAuditLogging.AuditLogs + AbpAuditLogging.AuditLogs.View

✅ Menu hiển thị
✅ Truy cập trang OK
✅ Button "Tìm kiếm" hiển thị
✅ Button "Đặt lại" hiển thị
❌ Button "Xem chi tiết" KHÔNG hiển thị
❌ Không xem được chi tiết
```

### 3. Test với User không có permission
```
Permission: None

❌ Menu KHÔNG hiển thị
❌ Truy cập URL → Redirect to /auth/access
```

### 4. Test khi bỏ từng permission

#### Scenario 4.1: Không có View permission
```
Permission: AbpAuditLogging.AuditLogs only

✅ Vào được trang
❌ Button "Tìm kiếm" ẨN
❌ Button "Đặt lại" ẨN
❌ Không search được
```

#### Scenario 4.2: Không có ViewDetails
```
Permission: AbpAuditLogging.AuditLogs + .View

✅ Vào được trang
✅ Tìm kiếm OK
❌ Button "Xem chi tiết" ẨN
❌ Tab "Actions" ẨN (trong dialog nếu có cách khác vào)
```

#### Scenario 4.3: Không có ViewEntityChanges
```
Permission: AbpAuditLogging.AuditLogs + .View + .ViewDetails

✅ Tất cả hoạt động
❌ Tab "Thay đổi thực thể" ẨN
```

## 🐛 Troubleshooting

### Menu không hiển thị
**Kiểm tra:**
1. User có permission không?
2. `requiredPermission` đúng chưa?
3. `filterMenuItemsByPermission` được gọi chưa?

### Route vẫn truy cập được
**Kiểm tra:**
1. Guard được thêm vào route chưa?
2. Permission name đúng chưa?
3. Backend có grant permission chưa?

### Permission luôn false
**Kiểm tra:**
1. User đã login chưa?
2. Backend có trả về grantedPolicies chưa?
3. Permission name có typo không?

## 📚 Related Files

- `core/guards/permission.guard.ts` - Route guard
- `core/services/permission.service.ts` - Permission service
- `core/directives/permission.directive.ts` - Directive
- `core/pipes/permission.pipe.ts` - Pipe
- `core/utils/menu-permission.util.ts` - Menu utilities

## 🎓 Tài liệu tham khảo

- [ABP Permission System](https://docs.abp.io/en/abp/latest/Authorization)
- [Angular Route Guards](https://angular.dev/guide/routing/common-router-tasks#preventing-unauthorized-access)

