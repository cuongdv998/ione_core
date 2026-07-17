import { Directive, Input, TemplateRef, ViewContainerRef, OnInit, OnDestroy, inject } from '@angular/core';
import { Subscription } from 'rxjs';
import { PermissionService } from '../services/permission.service';

/**
 * Directive để ẩn/hiện elements dựa trên permission
 * 
 * Usage:
 * <div *appPermission="'Admin.Users'">Content for users with Admin.Users permission</div>
 * <div *appPermission="['Admin.Users', 'Admin.Roles']; requireAll: true">Content requiring both permissions</div>
 * <div *appPermission="['Admin.Users', 'Admin.Roles']; requireAll: false">Content requiring one of the permissions</div>
 */
@Directive({
  selector: '[appPermission]',
  standalone: true,
})
export class PermissionDirective implements OnInit, OnDestroy {
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);
  private permissionService = inject(PermissionService);
  
  private subscription?: Subscription;
  private permissions: string | string[] = [];
  private requireAll = false;

  @Input() set appPermission(permissions: string | string[]) {
    this.permissions = permissions;
    this.updateView();
  }

  @Input() set appPermissionRequireAll(requireAll: boolean) {
    this.requireAll = requireAll;
    this.updateView();
  }

  ngOnInit() {
    this.updateView();
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  private updateView() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    if (!this.permissions) {
      this.viewContainer.clear();
      return;
    }

    this.subscription = this.permissionService
      .isGranted$(this.permissions, this.requireAll)
      .subscribe((hasPermission) => {
        if (hasPermission) {
          this.viewContainer.createEmbeddedView(this.templateRef);
        } else {
          this.viewContainer.clear();
        }
      });
  }
}
