import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot } from '@angular/router';
import { PermissionService } from '../services/permission.service';
import { ConfigStateService } from '@abp/ng.core';
import { Observable, of } from 'rxjs';
import { map, catchError, filter, timeout, take } from 'rxjs/operators';

/**
 * Guard để bảo vệ routes dựa trên permissions
 * 
 * Usage trong routes:
 * {
 *   path: 'admin',
 *   component: AdminComponent,
 *   canActivate: [permissionGuard],
 *   data: { 
 *     requiredPermission: 'Admin.Users' 
 *     // hoặc
 *     requiredPermissions: ['Admin.Users', 'Admin.Roles'],
 *     requireAll: false // true nếu cần tất cả, false nếu chỉ cần một
 *   }
 * }
 */
export const permissionGuard: CanActivateFn = (route: ActivatedRouteSnapshot): Observable<boolean> => {
  const permissionService = inject(PermissionService);
  const router = inject(Router);
  const configState = inject(ConfigStateService);

  // Lấy thông tin permission từ route data
  const requiredPermission = route.data['requiredPermission'] as string | undefined;
  const requiredPermissions = route.data['requiredPermissions'] as string[] | undefined;
  const requireAll = route.data['requireAll'] as boolean | undefined ?? false;

  // Nếu không có yêu cầu permission nào, cho phép truy cập
  if (!requiredPermission && !requiredPermissions) {
    return of(true);
  }

  // Kiểm tra permission
  const permissions = requiredPermission 
    ? [requiredPermission] 
    : (requiredPermissions || []);

  // Đợi config state load xong (đặc biệt quan trọng khi refresh trang)
  // Đợi auth state có grantedPolicies
  return configState.getOne$('auth').pipe(
    // Đợi auth state có giá trị và có grantedPolicies
    filter((auth) => {
      // Nếu auth là null/undefined, tiếp tục đợi
      if (!auth) {
        return false;
      }
      // Nếu đã có grantedPolicies (có thể là empty object), cho phép check
      // Nếu chưa có grantedPolicies, tiếp tục đợi
      return auth.grantedPolicies !== undefined;
    }),
    // Timeout sau 5 giây để tránh đợi vô hạn
    timeout(5000),
    // Chỉ lấy giá trị đầu tiên
    take(1),
    map((auth) => {
      // Kiểm tra permission
      const hasPermission = permissionService.isGranted(permissions, requireAll);

      if (hasPermission) {
        return true;
      }

      // Nếu không có permission, redirect đến trang access denied
      router.navigate(['/auth/access'], {
        queryParams: { returnUrl: route.url.join('/') }
      });
      
      return false;
    }),
    catchError(() => {
      // Nếu có lỗi hoặc timeout, thử check một lần nữa với giá trị hiện tại
      const hasPermission = permissionService.isGranted(permissions, requireAll);
      
      if (hasPermission) {
        return of(true);
      }

      // Nếu không có permission, redirect đến trang access denied
      router.navigate(['/auth/access'], {
        queryParams: { returnUrl: route.url.join('/') }
      });
      
      return of(false);
    })
  );
};
