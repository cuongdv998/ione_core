import { Pipe, PipeTransform, inject, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { PermissionService } from '../services/permission.service';
import { Observable, Subscription } from 'rxjs';

/**
 * Pipe để kiểm tra permission trong templates
 * 
 * Usage:
 * <div *ngIf="'Admin.Users' | appPermission">Content</div>
 * <div *ngIf="['Admin.Users', 'Admin.Roles'] | appPermission:false">Content</div>
 */
@Pipe({
  name: 'appPermission',
  standalone: true,
  pure: false, // Cần pure: false để pipe reactive với changes
})
export class PermissionPipe implements PipeTransform, OnDestroy {
  private permissionService = inject(PermissionService);
  private cdr = inject(ChangeDetectorRef);
  private subscription?: Subscription;
  private lastValue: string | string[] | null = null;
  private lastRequireAll: boolean = false;
  private result: boolean = false;

  transform(permissions: string | string[], requireAll: boolean = false): boolean {
    // Nếu giá trị không thay đổi, trả về kết quả cached
    if (
      this.lastValue === permissions &&
      this.lastRequireAll === requireAll &&
      this.subscription
    ) {
      return this.result;
    }

    // Unsubscribe subscription cũ
    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    this.lastValue = permissions;
    this.lastRequireAll = requireAll;

    // Subscribe để lắng nghe changes
    this.subscription = this.permissionService
      .isGranted$(permissions, requireAll)
      .subscribe((hasPermission) => {
        this.result = hasPermission;
        this.cdr.markForCheck();
      });

    return this.result;
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
