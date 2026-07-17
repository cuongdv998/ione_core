# Cơ Chế Sinh Menu Động trong Sidebar

## Tổng Quan

Sidebar của ứng dụng Angular sử dụng cơ chế menu động từ ABP Framework backend. Menu được định nghĩa ở backend và được load động về frontend, sau đó được render bằng PrimeNG components.

## Kiến Trúc

```
AppSidebar → AppMenu → NavigationService → NavigationApiService → Backend API
                ↓
         AppMenuitem (render từng item)
```

## Luồng Hoạt Động

### 1. Component Sidebar (`app.sidebar.ts`)
- Component wrapper đơn giản chứa `AppMenu`
- Không có logic phức tạp

### 2. Component Menu (`app.menu.ts`)
- **Khởi tạo**: Trong `ngOnInit()`, gọi `loadMenuFromBackend()`
- **Load menu**: Sử dụng `NavigationService` để lấy menu từ backend
- **Xử lý permissions**: Lọc menu theo permissions của user
- **Convert format**: Chuyển đổi từ `ApplicationMenuItem` (ABP) sang `MenuItem` (PrimeNG)
- **Fallback**: Nếu backend không có menu, sử dụng menu hardcode
- **Loading state**: Hiển thị loading indicator khi đang tải menu
- **Auto refresh**: Tự động reload menu khi permissions thay đổi

### 3. NavigationService (`core/services/navigation.service.ts`)
- **getMenuItems()**: Gọi API backend để lấy menu, tự động sắp xếp theo `order`
- **convertToPrimeNGMenu()**: Chuyển đổi `ApplicationMenuItem[]` → `MenuItem[]`
- **filterMenuByPermissions()**: Lọc menu theo permissions (đệ quy cho sub-items)
- **normalizeUrl()**: Xử lý URL từ backend (loại bỏ `~/` prefix, normalize path)
- **sortMenuItems()**: Sắp xếp menu items theo `order` (đệ quy)

### 4. NavigationApiService (`proxy/volo/abp/ui/navigation/navigation.service.ts`)
- Gọi endpoint: `GET /api/abp/application-configuration/navigation/{menuName}`
- Mặc định `menuName = 'Main'`
- Sử dụng ABP `RestService` để gọi API

### 5. Backend (ABP Framework)
- Menu được định nghĩa qua các `IMenuContributor` classes
- Backend tự động filter theo permissions
- Trả về `ApplicationMenu` với cấu trúc:
  ```typescript
  {
    name: string;
    items: ApplicationMenuItem[];
  }
  ```

## Cấu Trúc Dữ Liệu

### ApplicationMenuItem (từ backend)
```typescript
{
  name: string;                    // Tên unique của menu item
  displayName: string;             // Tên hiển thị (có thể là localization key)
  url?: string;                    // URL (có thể có ~/ prefix)
  icon?: string;                   // Icon class (ví dụ: "fa fa-cog")
  order: number;                   // Thứ tự sắp xếp
  requiredPermissionName?: string; // Permission cần thiết
  items?: ApplicationMenuItem[];  // Sub-menu items (đệ quy)
}
```

### MenuItem (PrimeNG - dùng để render)
```typescript
{
  label: string;                   // Tên hiển thị
  icon: string;                    // Icon class
  routerLink?: string[];           // Angular route
  items?: MenuItem[];              // Sub-menu items
  visible?: boolean;               // Hiển thị hay không
  // ... các thuộc tính khác của PrimeNG
}
```

## Xử Lý Permissions

1. **Backend**: ABP Framework tự động filter menu theo permissions của user
2. **Frontend**: Double-check bằng `filterMenuByPermissions()`:
   - Lấy `grantedPolicies` từ `ConfigStateService`
   - Kiểm tra `requiredPermissionName` của mỗi item
   - Ẩn item nếu user không có permission
   - Đệ quy cho sub-items
   - Loại bỏ parent items nếu không còn sub-items nào

## Xử Lý URL

- **Normalize URL**: Loại bỏ `~/` prefix (ABP convention)
- **Path normalization**: Đảm bảo URL bắt đầu bằng `/`
- **Trailing slash**: Loại bỏ trailing slash (trừ root path)

## Sắp Xếp Menu

- Menu items được sắp xếp theo `order` property
- Sắp xếp đệ quy cho cả sub-items
- Items không có `order` sẽ được xếp cuối cùng (order = 0)

## Fallback Mechanism

Nếu backend không trả về menu hoặc có lỗi:
- Sử dụng menu hardcode trong `loadFallbackMenu()`
- Menu fallback có localization support
- Menu fallback bao gồm:
  - Administration
    - Users
    - Roles
    - Audit Logs

## Render Menu Items

`AppMenuitem` component:
- Render từng menu item với animation
- Hỗ trợ sub-menu (đệ quy)
- Xử lý routing và active state
- Sử dụng PrimeNG Ripple effect
- Tự động highlight menu item khi route active

## API Endpoint

```
GET /api/abp/application-configuration/navigation/{menuName}
```

**Response:**
```json
{
  "name": "Main",
  "items": [
    {
      "name": "Administration",
      "displayName": "Administration",
      "icon": "fa fa-cog",
      "order": 1,
      "items": [
        {
          "name": "IdentityManagement",
          "displayName": "Identity Management",
          "icon": "fa fa-id-card",
          "order": 1,
          "requiredPermissionName": "AbpIdentity.Users",
          "items": [...]
        }
      ]
    }
  ]
}
```

## Cách Sử Dụng

### Thêm Menu Item ở Backend

1. Tạo `IMenuContributor` class:
```csharp
public class MyMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var menuItem = new ApplicationMenuItem(
            "MyMenu",
            "My Menu",
            icon: "fa fa-star"
        );
        
        menuItem.AddItem(new ApplicationMenuItem(
            "MySubMenu",
            "My Sub Menu",
            url: "~/pages/my-page"
        ).RequirePermissions("MyPermission"));

        context.Menu.AddItem(menuItem);
    }
}
```

2. Đăng ký trong module:
```csharp
context.Services.Configure<AbpNavigationOptions>(options =>
{
    options.MenuContributors.Add(new MyMenuContributor());
});
```

### Customize Menu ở Frontend

1. **Thay đổi menu name**: 
   ```typescript
   this.navigationService.getMenuItems('CustomMenu')
   ```

2. **Thêm menu item programmatically**:
   ```typescript
   this.filteredModel.push({
     label: 'Custom Item',
     icon: 'pi pi-star',
     routerLink: ['/custom']
   });
   ```

## Troubleshooting

### Menu không hiển thị
1. Kiểm tra backend có trả về menu không (check Network tab)
2. Kiểm tra permissions của user
3. Kiểm tra console logs để xem có lỗi không
4. Kiểm tra URL normalization có đúng không

### Menu không sắp xếp đúng
- Đảm bảo backend trả về `order` property
- Kiểm tra `sortMenuItems()` có được gọi không

### URL không hoạt động
- Kiểm tra URL có được normalize đúng không
- Kiểm tra route có được định nghĩa trong `app.routes.ts` không
- Kiểm tra URL có `~/` prefix không (sẽ được tự động loại bỏ)

## Tóm Tắt

1. ✅ Backend định nghĩa menu qua `IMenuContributor`
2. ✅ Frontend gọi API để lấy menu động
3. ✅ Convert từ ABP format sang PrimeNG format
4. ✅ Filter theo permissions (backend + frontend)
5. ✅ Sắp xếp theo `order`
6. ✅ Normalize URL (loại bỏ `~/` prefix)
7. ✅ Render bằng PrimeNG components với animation
8. ✅ Fallback nếu backend không có menu
9. ✅ Auto refresh khi permissions thay đổi
10. ✅ Loading state khi đang tải menu

