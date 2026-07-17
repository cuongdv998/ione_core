import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { MenuItem } from 'primeng/api';
import { RestService } from '@abp/ng.core';
import type { ApplicationMenuItem } from '@/proxy/volo/abp/ui/navigation/models';

export type { ApplicationMenuItem };

export interface ApplicationMenuDto {
  name: string;
  items: ApplicationMenuItemDto[];
}

export interface ApplicationMenuItemDto {
  name: string;
  displayName: string;
  url?: string;
  icon?: string;
  order: number;
  requiredPermissionName?: string;
  items?: ApplicationMenuItemDto[];
}

/**
 * Service để quản lý menu động từ ABP backend
 */
@Injectable({
  providedIn: 'root',
})
export class NavigationService {
  private restService = inject(RestService);

  /**
   * Lấy menu items từ backend API
   * Menu được lấy từ API endpoint /api/app/menu
   */
  getMenuItems(menuName: string = 'Main'): Observable<ApplicationMenuItem[]> {
    // Load menu từ API endpoint mới
    return this.loadMenuFromApi(menuName);
  }

  /**
   * Load menu từ API endpoint /api/app/menu
   */
  private loadMenuFromApi(menuName: string = 'Main'): Observable<ApplicationMenuItem[]> {
    return this.restService.request<ApplicationMenuDto, ApplicationMenuDto>({
      method: 'GET',
      url: '/api/app/menu',
      params: { menuName },
    }, { apiName: 'default' }).pipe(
      map((menu) => {
        if (menu && menu.items && menu.items.length > 0) {
          // Convert ApplicationMenuItemDto[] sang ApplicationMenuItem[]
          const items = this.convertToApplicationMenuItem(menu.items);
          // Sắp xếp menu items theo order
          return this.sortMenuItems(items);
        }
        return [];
      }),
      catchError((error) => {
        console.error('Error loading menu from API:', error);
        return of([]);
      })
    );
  }

  /**
   * Convert ApplicationMenuItemDto[] sang ApplicationMenuItem[]
   */
  private convertToApplicationMenuItem(items: ApplicationMenuItemDto[]): ApplicationMenuItem[] {
    return items.map(item => ({
      name: item.name,
      displayName: item.displayName,
      url: item.url,
      icon: item.icon,
      order: item.order,
      requiredPermissionName: item.requiredPermissionName,
      items: item.items ? this.convertToApplicationMenuItem(item.items) : undefined,
    }));
  }


  /**
   * Sắp xếp menu items theo order (đệ quy cho sub-items)
   */
  private sortMenuItems(items: ApplicationMenuItem[]): ApplicationMenuItem[] {
    return items
      .sort((a, b) => (a.order || 0) - (b.order || 0))
      .map(item => {
        if (item.items && item.items.length > 0) {
          return {
            ...item,
            items: this.sortMenuItems(item.items)
          };
        }
        return item;
      });
  }

  /**
   * Convert ApplicationMenuItem sang PrimeNG MenuItem
   */
  convertToPrimeNGMenu(items: ApplicationMenuItem[]): MenuItem[] {
    return items.map(item => this.mapMenuItem(item));
  }

  private mapMenuItem(item: ApplicationMenuItem): MenuItem {
    // Normalize URL: loại bỏ ~/ prefix và xử lý URL
    const normalizedUrl = this.normalizeUrl(item.url);
    
    const menuItem: MenuItem = {
      label: item.displayName,
      icon: item.icon,
      routerLink: normalizedUrl ? [normalizedUrl] : undefined,
      visible: true,
    };

    if (item.items && item.items.length > 0) {
      menuItem.items = item.items.map(subItem => this.mapMenuItem(subItem));
    }

    return menuItem;
  }

  /**
   * Normalize URL từ backend (loại bỏ ~/ prefix và xử lý các trường hợp đặc biệt)
   */
  private normalizeUrl(url?: string): string | undefined {
    if (!url) {
      return undefined;
    }

    // Loại bỏ ~/ prefix (ABP convention)
    let normalized = url.replace(/^~\//, '/');
    
    // Đảm bảo bắt đầu bằng /
    if (!normalized.startsWith('/')) {
      normalized = '/' + normalized;
    }

    // Loại bỏ trailing slash (trừ root)
    if (normalized.length > 1 && normalized.endsWith('/')) {
      normalized = normalized.slice(0, -1);
    }

    return normalized;
  }

  /**
   * Filter menu items theo permissions
   * (Permission checking được handle bởi backend, nhưng có thể double-check ở frontend)
   */
  filterMenuByPermissions(items: ApplicationMenuItem[], grantedPermissions: Record<string, boolean>): ApplicationMenuItem[] {
    return items
      .map(item => {
        // Kiểm tra permission cho item hiện tại
        let hasPermission = true;
        if (item.requiredPermissionName) {
          hasPermission = grantedPermissions[item.requiredPermissionName] === true;
          // Debug log
          if (!hasPermission) {
            console.log(`Menu item "${item.name}" (${item.displayName}) requires permission "${item.requiredPermissionName}" but it's not granted.`);
          }
        }

        // Nếu item không có permission, bỏ qua (không return null vì sẽ filter sau)
        if (!hasPermission) {
          return null;
        }

        // Filter sub-items đệ quy
        let filteredSubItems: ApplicationMenuItem[] | undefined;
        if (item.items && item.items.length > 0) {
          filteredSubItems = this.filterMenuByPermissions(item.items, grantedPermissions);
        }

        // Nếu có sub-items, chỉ giữ lại item nếu còn sub-items sau khi filter
        if (filteredSubItems && filteredSubItems.length > 0) {
          return {
            ...item,
            items: filteredSubItems
          };
        }

        // Nếu không có sub-items hoặc sub-items đã bị filter hết
        // Nếu item có URL, giữ lại (có thể navigate được)
        if (item.url) {
          return {
            ...item,
            items: undefined // Xóa sub-items nếu đã bị filter hết
          };
        }

        // Nếu item không có URL và không còn sub-items sau khi filter
        // Nếu item không có requiredPermissionName (menu cha không yêu cầu permission)
        // và có sub-items ban đầu, vẫn giữ lại menu cha ngay cả khi sub-items bị filter hết
        // Vì menu cha có thể có sub-items khác được thêm sau này
        // Nhưng trong trường hợp này, nếu không còn sub-items nào, nên ẩn menu cha
        // Vì không có gì để hiển thị
        return null; // Ẩn menu cha nếu không còn sub-items
      })
      .filter((item): item is ApplicationMenuItem => item !== null);
  }
}

