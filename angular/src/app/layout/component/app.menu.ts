import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { AppMenuitem } from './app.menuitem';
import { PermissionService } from '../../core/services/permission.service';
import { NavigationService, ApplicationMenuItem } from '../../core/services/navigation.service';
import { ConfigStateService } from '@abp/ng.core';
import { LocalizationService } from '../../core/services/localization.service';
import { filter, take } from 'rxjs/operators';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [CommonModule, AppMenuitem, RouterModule],
    template: `<ul class="layout-menu">
        <ng-container *ngIf="!isLoading">
            <ng-container *ngFor="let item of filteredModel; let i = index">
                <li app-menuitem *ngIf="!item.separator" [item]="item" [index]="i" [root]="true"></li>
                <li *ngIf="item.separator" class="menu-separator"></li>
            </ng-container>
        </ng-container>
        <li *ngIf="isLoading" class="menu-loading">
            <i class="pi pi-spin pi-spinner"></i>
            <span>{{ localizationService.localize('iOne::Menu:Loading') }}</span>
        </li>
    </ul> `
})
export class AppMenu implements OnInit {
    private permissionService = inject(PermissionService);
    private navigationService = inject(NavigationService);
    private configState = inject(ConfigStateService);
    public localizationService = inject(LocalizationService);
    
    model: MenuItem[] = [];
    filteredModel: MenuItem[] = [];
    isLoading = true;

    ngOnInit() {
        let isFirstLoad = true;
        
        // Đợi auth state được load xong và reload menu khi permissions thay đổi
        this.configState.getOne$('auth').pipe(
            // Đợi auth state có giá trị và có grantedPolicies
            filter((auth) => {
                // Nếu auth là null/undefined, tiếp tục đợi
                if (!auth) {
                    return false;
                }
                // Nếu đã có grantedPolicies (có thể là empty object), cho phép load menu
                // Nếu chưa có grantedPolicies, tiếp tục đợi
                return auth.grantedPolicies !== undefined;
            })
        ).subscribe(() => {
            // Load menu lần đầu hoặc reload khi permissions thay đổi
            if (isFirstLoad) {
                isFirstLoad = false;
                this.loadMenuFromBackend();
            } else {
                // Chỉ reload nếu đã load menu lần đầu và không đang loading
                if (!this.isLoading) {
                    this.loadMenuFromBackend();
                }
            }
        });
    }

    private loadMenuFromBackend() {
        this.isLoading = true;
        // Lấy menu từ API endpoint /api/app/menu
        // Backend đã tự động filter menu theo permissions của user hiện tại
        this.navigationService.getMenuItems().subscribe({
            next: (items) => {
                console.log('Menu items from backend:', items);
                // Debug: Log để kiểm tra menu items
                if (items && items.length > 0) {
                    // Tìm menu "Master" và log sub-items
                    const masterMenu = items.find(item => item.name === 'Master');
                    if (masterMenu) {
                        console.log('Master menu found:', masterMenu);
                        console.log('Master menu items:', masterMenu.items);
                        // Tìm ResEvent menu item
                        const resEventMenu = masterMenu.items?.find(item => item.name === 'Master.ResEvent');
                        if (resEventMenu) {
                            console.log('ResEvent menu item found:', resEventMenu);
                        } else {
                            console.warn('ResEvent menu item NOT found in Master menu');
                            console.log('Available Master menu items:', masterMenu.items?.map(item => item.name));
                        }
                    } else {
                        console.warn('Master menu NOT found');
                        console.log('Available menu items:', items.map(item => item.name));
                    }
                    // Backend đã trả về menu và đã filter theo permissions
                    // Chỉ cần convert sang PrimeNG MenuItem format
                    this.filteredModel = this.navigationService.convertToPrimeNGMenu(items);
                } else {
                    // Fallback: Sử dụng menu hardcode nếu backend chưa có menu
                    // Tạm thời comment để test cơ chế load menu động
                    // this.loadFallbackMenu();
                    console.warn('No menu items returned from backend');
                }
                this.isLoading = false;
            },
            error: (error) => {
                console.error('Error loading menu from backend:', error);
                // Fallback: Sử dụng menu hardcode khi có lỗi
                // Tạm thời comment để test cơ chế load menu động
                // this.loadFallbackMenu();
                this.isLoading = false;
            }
        });
    }

    private loadFallbackMenu() {
        // Menu hardcode với localization (fallback)
        // Thêm permission checking cho từng item
        const auth = this.configState.getOne('auth');
        const grantedPermissions = auth?.grantedPolicies || {};
        
        this.model = [
            {
                label: this.localizationService.localize('iOne::Menu:Administration'),
                icon: 'pi pi-fw pi-cog',
                items: [
                    {
                        label: this.localizationService.localize('iOne::Menu:Users'),
                        icon: 'pi pi-fw pi-user',
                        routerLink: ['/pages/users'],
                        // Thêm permission checking
                        visible: this.permissionService.isGranted('AbpIdentity.Users'),
                    },
                    {
                        label: this.localizationService.localize('iOne::Menu:Roles'),
                        icon: 'pi pi-fw pi-users',
                        routerLink: ['/pages/roles'],
                        // Thêm permission checking
                        visible: this.permissionService.isGranted('AbpIdentity.Roles'),
                    },
                    {
                        label: this.localizationService.localize('iOne::Menu:AuditLogs'),
                        icon: 'pi pi-fw pi-history',
                        routerLink: ['/pages/audit-logs'],
                        // Thêm permission checking
                        visible: this.permissionService.isGranted('AbpAuditLogging.AuditLogs'),
                    }
                ]
            },
            {
                label: this.localizationService.localize('Hr::Menu:HR'),
                icon: 'pi pi-fw pi-briefcase',
                items: [
                    {
                        label: this.localizationService.localize('Hr::Menu:DepartmentTypes'),
                        icon: 'pi pi-fw pi-building',
                        routerLink: ['/pages/hr/department-types'],
                        // Permission name là "HrDepartmentType" (vì Default = GroupName)
                        visible: this.permissionService.isGranted('HrDepartmentType') || this.permissionService.isGranted('HrDepartmentType.View'),
                    },
                    {
                        label: this.localizationService.localize('Hr::Menu:EmployeeRoles'),
                        icon: 'pi pi-fw pi-user-edit',
                        routerLink: ['/pages/hr/employee-roles'],
                        // Permission name là "HrEmployeeRole" (vì Default = GroupName)
                        visible: this.permissionService.isGranted('HrEmployeeRole') || this.permissionService.isGranted('HrEmployeeRole.View'),
                    }
                ]
            }
        ];
        
        // Filter menu theo permissions
        this.filteredModel = this.filterMenuByPermissions(this.model);
    }

    private filterMenuByPermissions(items: MenuItem[]): MenuItem[] {
        const auth = this.configState.getOne('auth');
        const grantedPermissions = auth?.grantedPolicies || {};
        
        return items
            .map(item => {
                // Kiểm tra visible property
                // PrimeNG MenuItem.visible có type true | undefined
                // Nhưng chúng ta có thể set visible: false trong code
                // Sử dụng type assertion để check
                const isVisible = (item.visible as boolean | undefined) !== false;
                
                // Nếu visible là false, bỏ qua
                if (!isVisible) {
                    return null;
                }
                
                // Filter sub-items đệ quy
                let filteredSubItems: MenuItem[] | undefined;
                if (item.items && item.items.length > 0) {
                    filteredSubItems = this.filterMenuByPermissions(item.items);
                }
                
                // Nếu có sub-items, chỉ giữ lại item nếu còn sub-items sau khi filter
                if (filteredSubItems && filteredSubItems.length > 0) {
                    return {
                        ...item,
                        items: filteredSubItems
                    };
                }
                
                // Nếu không còn sub-items sau khi filter, chỉ giữ lại item nếu có routerLink và visible
                if (item.routerLink && isVisible) {
                    return {
                        ...item,
                        items: undefined
                    };
                }
                
                // Nếu không có routerLink hoặc không visible, bỏ qua
                return null;
            })
            .filter((item): item is MenuItem => item !== null);
    }
}
