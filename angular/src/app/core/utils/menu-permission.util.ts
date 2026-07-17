import { MenuItem } from 'primeng/api';
import { PermissionService } from '../services/permission.service';

/**
 * Interface mở rộng MenuItem để hỗ trợ permission
 */
export interface MenuItemWithPermission extends MenuItem {
  /**
   * Permission name hoặc array of permission names
   * Nếu có permission này, menu item sẽ được hiển thị
   */
  permission?: string | string[];
  
  /**
   * Nếu true, cần có tất cả permissions. Nếu false, chỉ cần một trong số đó
   * Mặc định: false
   */
  requireAll?: boolean;
  
  /**
   * Menu items con (recursive)
   */
  items?: MenuItemWithPermission[];
}

/**
 * Filter menu items dựa trên permissions
 * @param items Menu items cần filter
 * @param permissionService PermissionService instance
 * @returns Menu items đã được filter
 */
export function filterMenuItemsByPermission(
  items: MenuItemWithPermission[],
  permissionService: PermissionService
): MenuItem[] {
  return items
    .map((item) => {
      // Kiểm tra permission cho item hiện tại
      if (item.permission) {
        const hasPermission = permissionService.isGranted(
          item.permission,
          item.requireAll ?? false
        );
        
        if (!hasPermission) {
          return null; // Ẩn item này
        }
      }

      // Recursive filter cho sub-items
      if (item.items && item.items.length > 0) {
        const filteredItems = filterMenuItemsByPermission(item.items, permissionService);
        
        // Nếu không còn sub-item nào, có thể ẩn parent item
        if (filteredItems.length === 0 && item.permission) {
          return null;
        }
        
        return {
          ...item,
          items: filteredItems,
        };
      }

      return item;
    })
    .filter((item): item is MenuItem => item !== null);
}

/**
 * Kiểm tra xem menu item có visible không dựa trên permission
 * @param item Menu item cần kiểm tra
 * @param permissionService PermissionService instance
 * @returns true nếu visible, false nếu không
 */
export function isMenuItemVisible(
  item: MenuItemWithPermission,
  permissionService: PermissionService
): boolean {
  if (!item.permission) {
    return item.visible !== false; // Nếu không có permission requirement, dùng visible property
  }

  const hasPermission = permissionService.isGranted(
    item.permission,
    item.requireAll ?? false
  );

  // Nếu có sub-items, kiểm tra xem có sub-item nào visible không
  if (item.items && item.items.length > 0) {
    const hasVisibleSubItem = item.items.some((subItem) =>
      isMenuItemVisible(subItem, permissionService)
    );
    return hasPermission && hasVisibleSubItem;
  }

  return hasPermission && item.visible !== false;
}
