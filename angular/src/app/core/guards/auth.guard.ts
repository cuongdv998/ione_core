import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService, ConfigStateService } from '@abp/ng.core';
import { Observable, of } from 'rxjs';
import { map, catchError, filter, timeout, take } from 'rxjs/operators';

export const authGuard: CanActivateFn = (route, state): Observable<boolean> => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const configState = inject(ConfigStateService);

  // Nếu đã authenticated ngay lập tức, cho phép
  if (authService.isAuthenticated) {
    return of(true);
  }

  // Đợi auth state được restore từ token (khi refresh trang)
  // ABP tự động restore token từ storage, nhưng cần thời gian
  // Check cả auth (grantedPolicies) và currentUser (isAuthenticated)
  return configState.getOne$('currentUser').pipe(
    // Đợi currentUser có giá trị (không null/undefined)
    filter((currentUser) => currentUser !== null && currentUser !== undefined),
    // Timeout sau 5 giây để tránh đợi vô hạn
    timeout(5000),
    // Chỉ lấy giá trị đầu tiên
    take(1),
    map((currentUser) => {
      // Kiểm tra xem user có authenticated không
      // currentUser có isAuthenticated property
      if (authService.isAuthenticated || (currentUser && currentUser.isAuthenticated)) {
        return true;
      }
      
      // Nếu không authenticated, redirect đến login
      router.navigate(['/auth/login'], { 
        queryParams: { returnUrl: state.url } 
      });
      return false;
    }),
    catchError((error) => {
      // Nếu có lỗi hoặc timeout, kiểm tra lại một lần nữa
      if (authService.isAuthenticated) {
        return of(true);
      }
      
      // Redirect to login page
      router.navigate(['/auth/login'], { 
        queryParams: { returnUrl: state.url } 
      });
      return of(false);
    })
  );
};

