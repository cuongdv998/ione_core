import { Injectable, inject } from '@angular/core';
import { ConfigStateService } from '@abp/ng.core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

/**
 * Service để quản lý và kiểm tra permissions
 * Tương tự ABP PermissionService
 */
@Injectable({
  providedIn: 'root',
})
export class PermissionService {
  private configState = inject(ConfigStateService);

  /**
   * Kiểm tra xem user có permission cụ thể không
   * @param permissionName Tên permission cần kiểm tra
   * @returns true nếu có permission, false nếu không
   */
  getGrantedPolicy(permissionName: string): boolean {
    const auth = this.configState.getOne('auth');
    if (!auth || !auth.grantedPolicies) {
      return false;
    }
    return auth.grantedPolicies[permissionName] === true;
  }

  /**
   * Kiểm tra permission dưới dạng Observable
   * @param permissionName Tên permission cần kiểm tra
   * @returns Observable<boolean>
   */
  getGrantedPolicy$(permissionName: string): Observable<boolean> {
    return this.configState.getOne$('auth').pipe(
      map((auth) => {
        if (!auth || !auth.grantedPolicies) {
          return false;
        }
        return auth.grantedPolicies[permissionName] === true;
      })
    );
  }

  /**
   * Kiểm tra xem user có ít nhất một trong các permissions không
   * @param permissionNames Mảng các permission names
   * @param requireAll Nếu true, cần có tất cả permissions. Nếu false, chỉ cần một trong số đó
   * @returns true nếu thỏa mãn điều kiện
   */
  isGranted(permissionNames: string | string[], requireAll: boolean = false): boolean {
    if (!permissionNames) {
      return true; // Nếu không có permission nào được yêu cầu, cho phép
    }

    const permissions = Array.isArray(permissionNames) ? permissionNames : [permissionNames];
    
    if (permissions.length === 0) {
      return true;
    }

    if (requireAll) {
      // Cần có tất cả permissions
      return permissions.every((permission) => this.getGrantedPolicy(permission));
    } else {
      // Chỉ cần một trong các permissions
      return permissions.some((permission) => this.getGrantedPolicy(permission));
    }
  }

  /**
   * Kiểm tra permission dưới dạng Observable
   * @param permissionNames Mảng các permission names
   * @param requireAll Nếu true, cần có tất cả permissions. Nếu false, chỉ cần một trong số đó
   * @returns Observable<boolean>
   */
  isGranted$(permissionNames: string | string[], requireAll: boolean = false): Observable<boolean> {
    if (!permissionNames) {
      return of(true);
    }

    const permissions = Array.isArray(permissionNames) ? permissionNames : [permissionNames];
    
    if (permissions.length === 0) {
      return of(true);
    }

    return this.configState.getOne$('auth').pipe(
      map((auth) => {
        if (!auth || !auth.grantedPolicies) {
          return false;
        }

        if (requireAll) {
          return permissions.every((permission) => auth.grantedPolicies[permission] === true);
        } else {
          return permissions.some((permission) => auth.grantedPolicies[permission] === true);
        }
      })
    );
  }

  /**
   * Lấy tất cả granted policies
   * @returns Record<string, boolean> hoặc null
   */
  getAllGrantedPolicies(): Record<string, boolean> | null {
    const auth = this.configState.getOne('auth');
    return auth?.grantedPolicies || null;
  }

  /**
   * Lấy tất cả granted policies dưới dạng Observable
   * @returns Observable<Record<string, boolean> | null>
   */
  getAllGrantedPolicies$(): Observable<Record<string, boolean> | null> {
    return this.configState.getOne$('auth').pipe(
      map((auth) => auth?.grantedPolicies || null)
    );
  }
}
