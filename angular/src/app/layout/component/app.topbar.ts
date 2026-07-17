import { Component, inject, OnInit, computed, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StyleClassModule } from 'primeng/styleclass';
import { AvatarModule } from 'primeng/avatar';
import { DividerModule } from 'primeng/divider';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { AppConfigurator } from './app.configurator';
import { LayoutService } from '../service/layout.service';
import { AuthService, ConfigStateService } from '@abp/ng.core';
import { LanguageSelectorMenuComponent } from '../../core/components/language-selector-menu/language-selector-menu.component';
import { SystemEventNotifyService } from '@/proxy/master/controllers/system-event-notify.service';
import type { SystemEventNotifyDto } from '@/proxy/master/system-event-notifies/models';
import { NotificationSignalrService } from '../../core/services/notification-signalr.service';
import { MyProfilePasswordService } from '@/core/services/my-profile-password.service';
import { LocalizationService } from '@/core/services/localization.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { finalize } from 'rxjs';

function getRelativeTime(dateInput: string | Date): string {
    const date = typeof dateInput === 'string' ? new Date(dateInput) : dateInput;
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    if (diffMin < 1) return 'Vừa xong';
    if (diffMin < 60) return `${diffMin} phút trước`;
    const diffHour = Math.floor(diffMin / 60);
    if (diffHour < 24) return `${diffHour} giờ trước`;
    const diffDay = Math.floor(diffHour / 24);
    if (diffDay < 30) return `${diffDay} ngày trước`;
    return date.toLocaleDateString('vi-VN');
}

@Component({
    selector: 'app-topbar',
    standalone: true,
    imports: [
        RouterModule,
        CommonModule,
        FormsModule,
        StyleClassModule,
        AppConfigurator,
        AvatarModule,
        DividerModule,
        DialogModule,
        ButtonModule,
        PasswordModule,
        ToastModule,
        LanguageSelectorMenuComponent,
        TranslatePipe
    ],
    providers: [MessageService],
    template: ` <div class="layout-topbar">
        <div class="layout-topbar-logo-container">
            <button class="layout-menu-button layout-topbar-action" (click)="layoutService.onMenuToggle()">
                <i class="pi pi-bars"></i>
            </button>
            <a class="layout-topbar-logo" routerLink="/">
                <img src="/assets/images/ione-logo-login.png" alt="iOne Logo" />
            </a>
        </div>
        <div class="layout-topbar-actions">
            <div class="layout-config-menu">
                <button type="button" class="layout-topbar-action layout-topbar-action-highlight" (click)="toggleDarkMode()">
                    <i [ngClass]="{ 'pi ': true, 'pi-moon': layoutService.isDarkTheme(), 'pi-sun': !layoutService.isDarkTheme() }"></i>
                </button>
                <div class="relative">
                    <button
                        class="layout-topbar-action layout-topbar-action-highlight"
                        pStyleClass="@next"
                        enterFromClass="hidden"
                        enterActiveClass="animate-scalein"
                        leaveToClass="hidden"
                        leaveActiveClass="animate-fadeout"
                        [hideOnOutsideClick]="true"
                    >
                        <i class="pi pi-palette"></i>
                    </button>
                    <app-configurator />
                </div>
            </div>

            <button class="layout-topbar-menu-button layout-topbar-action" pStyleClass="@next" enterFromClass="hidden" enterActiveClass="animate-scalein" leaveToClass="hidden" leaveActiveClass="animate-fadeout" [hideOnOutsideClick]="true">
                <i class="pi pi-ellipsis-v"></i>
            </button>

            <div class="layout-topbar-menu hidden lg:block">
                <div class="layout-topbar-menu-content">
                    <!-- Notification Bell -->
                    <div class="notification-container">
                        <button
                            type="button"
                            class="layout-topbar-action notification-bell-btn"
                            pStyleClass="@next"
                            enterFromClass="hidden"
                            enterActiveClass="animate-scalein"
                            leaveToClass="hidden"
                            leaveActiveClass="animate-fadeout"
                            [hideOnOutsideClick]="true"
                        >
                            <i class="pi pi-bell"></i>
                            <span class="notification-badge" *ngIf="unreadCount() > 0">{{ unreadCount() > 9 ? '9+' : unreadCount() }}</span>
                        </button>
                        <div class="notification-panel hidden">
                            <div class="notification-panel-header">
                                <div class="notification-panel-title">
                                    Thông báo
                                    <span class="notification-count-chip" *ngIf="unreadCount() > 0">{{ unreadCount() }}</span>
                                </div>
                                <button type="button" class="notification-mark-all-btn" *ngIf="unreadCount() > 0" (click)="markAllAsRead(); $event.stopPropagation()">Đánh dấu đã đọc</button>
                            </div>
                            <div class="notification-panel-list" *ngIf="notifications().length > 0">
                                <div *ngFor="let item of notifications()" class="notification-item" [class.unread]="!item.readAt" (click)="markAsRead($any(item.id))">
                                    <div class="notification-item-icon icon-info">
                                        <i class="pi pi-bell"></i>
                                    </div>
                                    <div class="notification-item-content">
                                        <div class="notification-item-title">{{ item.title }}</div>
                                        <div class="notification-item-desc">{{ item.body }}</div>
                                        <div class="notification-item-time">{{ formatTime(item.creationTime) }}</div>
                                    </div>
                                    <div class="notification-unread-dot" *ngIf="!item.readAt"></div>
                                </div>
                            </div>
                            <div class="notification-empty" *ngIf="notifications().length === 0">
                                <i class="pi pi-bell-slash"></i>
                                <span>Không có thông báo mới</span>
                            </div>
                            <div class="notification-panel-footer" *ngIf="notificationHasMore()">
                                <button type="button" class="notification-view-all-btn" (click)="viewAllNotifications($event)" [disabled]="notificationLoading()">
                                    {{ notificationLoading() ? 'Đang tải...' : 'Xem thêm thông báo' }}
                                    <i class="pi pi-arrow-down" *ngIf="!notificationLoading()"></i>
                                    <i class="pi pi-spinner pi-spin" *ngIf="notificationLoading()"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                    <!-- User Menu -->
                    <div class="user-menu-container" *ngIf="authService.isAuthenticated">
                        <button type="button" class="layout-topbar-action user-menu-trigger" pStyleClass="@next" enterFromClass="hidden" enterActiveClass="animate-scalein" leaveToClass="hidden" leaveActiveClass="animate-fadeout" [hideOnOutsideClick]="true">
                            <i class="pi pi-user"></i>
                            <span>Profile</span>
                        </button>
                        <div class="user-menu-panel hidden">
                            <div class="user-menu-header">
                                <p-avatar
                                    [label]="userInitials()"
                                    [style]="{ 'background-color': 'var(--primary-color)', color: 'var(--primary-contrast-color)', width: '3.5rem', height: '3.5rem', 'font-size': '1.25rem' }"
                                    shape="circle"
                                    size="large"
                                ></p-avatar>
                                <div class="user-menu-info">
                                    <div class="user-menu-name">{{ userName() }}</div>
                                    <div class="user-menu-email" *ngIf="userEmail()">{{ userEmail() }}</div>
                                </div>
                            </div>
                            <p-divider *ngIf="userOrganization()"></p-divider>
                            <div class="user-menu-organization" *ngIf="userOrganization()">
                                <i class="pi pi-building text-surface-500"></i>
                                <span>{{ userOrganization() }}</span>
                            </div>
                            <p-divider></p-divider>
                            <div class="user-menu-actions">
                                <div class="user-menu-language-selector">
                                    <app-language-selector-menu [reloadFullConfig]="true"></app-language-selector-menu>
                                </div>
                                <p-divider></p-divider>
                                <button type="button" class="user-menu-action-item" (click)="openChangePasswordDialog(); $event.stopPropagation()">
                                    <i class="pi pi-key"></i>
                                    <span>{{ 'AbpAccount::ProfileTab:Password' | translate }}</span>
                                </button>
                                <p-divider></p-divider>
                                <button type="button" class="user-menu-action-item user-menu-logout" (click)="logout()">
                                    <i class="pi pi-sign-out"></i>
                                    <span>Đăng xuất</span>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <p-toast position="top-right"></p-toast>
        <p-dialog
            [(visible)]="passwordDialogVisible"
            [header]="'AbpAccount::ProfileTab:Password' | translate"
            [modal]="true"
            [draggable]="false"
            [resizable]="false"
            [closable]="true"
            [style]="{ width: '460px', maxWidth: '95vw' }"
            (onHide)="resetPasswordDialogForm()"
            [focusOnShow]="false"
        >
            <div class="flex flex-col gap-4 py-2">
                <div class="flex flex-col gap-2">
                    <label for="topbarPwdCurrent" class="text-sm font-medium">{{ 'AbpAccount::DisplayName:CurrentPassword' | translate }}</label>
                    <p-password
                        inputId="topbarPwdCurrent"
                        [(ngModel)]="passwordForm.currentPassword"
                        [toggleMask]="true"
                        [feedback]="false"
                        [fluid]="true"
                        autocomplete="current-password"
                        [ngModelOptions]="{standalone: true}"
                        (onKeyDown)="onPasswordFieldKeyDown($event)"
                    ></p-password>
                </div>
                <div class="flex flex-col gap-2">
                    <label for="topbarPwdNew" class="text-sm font-medium">{{ 'AbpAccount::DisplayName:NewPassword' | translate }}</label>
                    <p-password
                        inputId="topbarPwdNew"
                        [(ngModel)]="passwordForm.newPassword"
                        [toggleMask]="true"
                        [feedback]="false"
                        [fluid]="true"
                        autocomplete="new-password"
                        [ngModelOptions]="{standalone: true}"
                        (onKeyDown)="onPasswordFieldKeyDown($event)"
                    ></p-password>
                </div>
                <div class="flex flex-col gap-2">
                    <label for="topbarPwdConfirm" class="text-sm font-medium">{{ 'AbpAccount::DisplayName:NewPasswordConfirm' | translate }}</label>
                    <p-password
                        inputId="topbarPwdConfirm"
                        [(ngModel)]="passwordForm.confirmPassword"
                        [toggleMask]="true"
                        [feedback]="false"
                        [fluid]="true"
                        autocomplete="new-password"
                        [ngModelOptions]="{standalone: true}"
                        (onKeyDown)="onPasswordFieldKeyDown($event)"
                    ></p-password>
                </div>
            </div>
            <ng-template pTemplate="footer">
                <p-button [label]="'AbpUi::Cancel' | translate" severity="secondary" [text]="true" (onClick)="closePasswordDialog()" [disabled]="passwordSubmitting"></p-button>
                <p-button [label]="'AbpUi::Save' | translate" icon="pi pi-check" (onClick)="submitPasswordChange()" [loading]="passwordSubmitting" [disabled]="passwordSubmitting"></p-button>
            </ng-template>
        </p-dialog>
    </div>`
})
export class AppTopbar implements OnInit {
    authService = inject(AuthService);
    private configState = inject(ConfigStateService);
    private router = inject(Router);
    private notifyService = inject(SystemEventNotifyService);
    private signalrService = inject(NotificationSignalrService);
    private myProfilePasswordService = inject(MyProfilePasswordService);
    private messageService = inject(MessageService);
    private localizationService = inject(LocalizationService);

    passwordDialogVisible = false;
    passwordSubmitting = false;
    passwordForm = {
        currentPassword: '',
        newPassword: '',
        confirmPassword: '',
    };

    userName = signal<string>('');
    userEmail = signal<string>('');
    userOrganization = signal<string>('');
    // Notification data
    notifications = signal<SystemEventNotifyDto[]>([]);
    unreadCount = computed(() => this.notifications().filter((n) => !n.readAt).length);
    notificationSkipCount = signal<number>(0);
    notificationHasMore = signal<boolean>(true);
    notificationLoading = signal<boolean>(false);

    constructor(public layoutService: LayoutService) {
    }

    ngOnInit() {
        console.log('ngOnInit');
        this.loadUserInfo();
        this.loadNotifications();

        // Subscribe to real-time notifications via SignalR
        this.signalrService.notificationReceived$.subscribe((notification: any) => {
            if (notification) {
                this.loadNotifications();
            }
        });

        // Subscribe to config state changes to update user info
        this.configState.getOne$('currentUser').subscribe((currentUser) => {
            if (currentUser) {
                const name = currentUser.userName || currentUser.name || 'User';
                const email = currentUser.email || '';
                // Try to get organization from currentUser (may not exist in CurrentUserDto)
                const organization = (currentUser as any).organizationUnit || (currentUser as any).department || (currentUser as any).organizationName || '';

                // Only update if values changed to avoid unnecessary updates
                if (this.userName() !== name || this.userEmail() !== email || this.userOrganization() !== organization) {
                    this.userName.set(name);
                    this.userEmail.set(email);
                    this.userOrganization.set(organization);
                }
            } else {
                // If no user, reset to default
                if (this.userName() !== 'User') {
                    this.userName.set('User');
                    this.userEmail.set('');
                    this.userOrganization.set('');
                }
            }
        });
    }

    userInitials = computed(() => {
        const name = this.userName();
        if (!name || name === 'User') return 'U';
        const parts = name.split(' ');
        if (parts.length >= 2) {
            return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
        }
        return name.substring(0, 2).toUpperCase();
    });

    loadUserInfo() {
        const currentUser = this.configState.getOne('currentUser');
        if (currentUser) {
            const name = currentUser.userName || currentUser.name || 'User';
            const email = currentUser.email || '';
            // Try to get organization from currentUser (may not exist in CurrentUserDto)
            const organization = (currentUser as any).organizationUnit || (currentUser as any).department || (currentUser as any).organizationName || '';
            this.userName.set(name);
            this.userEmail.set(email);
            this.userOrganization.set(organization);
        } else {
            // Fallback: set default values
            this.userName.set('User');
            this.userEmail.set('');
            this.userOrganization.set('');
        }
    }

    // Notification methods
    loadNotifications(isLoadMore = false) {
        if (this.notificationLoading()) return;

        if (!isLoadMore) {
            this.notificationSkipCount.set(0);
        }

        this.notificationLoading.set(true);

        this.notifyService
            .getListWebNotify({
                skipCount: this.notificationSkipCount(),
                maxResultCount: 10
            })
            .subscribe({
                next: (result) => {
                    const newItems = result.items ?? [];
                    if (isLoadMore) {
                        this.notifications.update((items) => [...items, ...newItems]);
                    } else {
                        this.notifications.set(newItems);
                    }
                    this.notificationSkipCount.update((count) => count + 10);
                    this.notificationHasMore.set(this.notifications().length < (result.totalCount || 0));
                    this.notificationLoading.set(false);
                },
                error: (err) => {
                    console.error('Failed to load notifications', err);
                    this.notificationLoading.set(false);
                }
            });
    }

    markAllAsRead() {
        const unreadItems = this.notifications().filter((n) => !n.readAt);
        if (unreadItems.length === 0) return;

        // Optimistic update
        const now = new Date().toISOString();
        this.notifications.update((items) => items.map((item) => (!item.readAt ? { ...item, readAt: now } : item)));

        // Call API for each unread item
        unreadItems.forEach((item) => {
            if (!item.id) return;
            this.notifyService.update(item.id, { status: 3 }).subscribe({
                error: (err) => console.error(`Failed to mark ${item.id} as read`, err)
            });
        });
    }

    markAsRead(id: string) {
        const item = this.notifications().find((n) => n.id === id);
        if (!item || item.readAt) return;

        // Optimistic update
        const now = new Date().toISOString();
        this.notifications.update((items) => items.map((n) => (n.id === id ? { ...n, readAt: now } : n)));

        this.notifyService.update(id, { status: 3, readAt: now }).subscribe({
            error: (err) => console.error(`Failed to mark ${id} as read`, err)
        });
    }

    formatTime(dateInput?: string | Date): string {
        if (!dateInput) return '';
        return getRelativeTime(dateInput);
    }

    viewAllNotifications(event?: Event) {
        if (event) {
            event.stopPropagation();
        }
        this.loadNotifications(true);
    }

    onPasswordFieldKeyDown(event: Event): void {
        const ke = event as KeyboardEvent;
        if (ke.key !== 'Enter') return;
        ke.preventDefault();
        this.submitPasswordChange();
    }

    openChangePasswordDialog(): void {
        this.resetPasswordDialogForm();
        this.passwordDialogVisible = true;
    }

    closePasswordDialog(): void {
        this.passwordDialogVisible = false;
        this.resetPasswordDialogForm();
    }

    resetPasswordDialogForm(): void {
        this.passwordForm = {
            currentPassword: '',
            newPassword: '',
            confirmPassword: '',
        };
    }

    submitPasswordChange(): void {
        const { currentPassword, newPassword, confirmPassword } = this.passwordForm;
        if (!currentPassword?.trim()) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning', 'Chú ý'),
                detail: `${this.localizationService.localize('AbpAccount::DisplayName:CurrentPassword', 'Mật khẩu hiện tại')} — ${this.localizationService.localize('AbpValidation:ThisFieldIsRequired', 'Trường này là bắt buộc')}`,
            });
            return;
        }
        if (!newPassword?.trim()) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning', 'Chú ý'),
                detail: `${this.localizationService.localize('AbpAccount::DisplayName:NewPassword', 'Mật khẩu mới')} — ${this.localizationService.localize('AbpValidation:ThisFieldIsRequired', 'Trường này là bắt buộc')}`,
            });
            return;
        }
        if (newPassword !== confirmPassword) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpUi::Warning', 'Chú ý'),
                detail: this.localizationService.localize('AbpAccount::NewPasswordConfirmFailed', 'Xác nhận mật khẩu mới không trùng khớp.'),
            });
            return;
        }

        this.passwordSubmitting = true;
        this.myProfilePasswordService
            .changePassword({ currentPassword, newPassword })
            .pipe(finalize(() => (this.passwordSubmitting = false)))
            .subscribe({
                next: () => {
                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpAccount::PasswordChanged', 'Đổi mật khẩu'),
                        detail: this.localizationService.localize('AbpAccount::PasswordChangedMessage', 'Đã đổi mật khẩu thành công.'),
                    });
                    this.closePasswordDialog();
                },
                error: (err: unknown) => {
                    const apiMsg =
                        typeof err === 'object' &&
                        err !== null &&
                        'error' in err &&
                        typeof (err as { error?: { error?: { message?: string } } }).error?.error?.message === 'string'
                            ? (err as { error: { error: { message: string } } }).error.error.message
                            : null;
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error', 'Lỗi'),
                        detail:
                            apiMsg ||
                            this.localizationService.localize(
                                'AbpUi::InternalServerErrorMessage',
                                'Không đổi được mật khẩu. Vui lòng thử lại.'
                            ),
                    });
                },
            });
    }

    logout() {
        /* this.authService.logout().subscribe(() => {
               this.router.navigate(['/auth/login']); */

        this.router.navigate(['/auth/login']).then(() => {
            this.authService.logout().subscribe();
        });
    }

    toggleDarkMode() {
        this.layoutService.layoutConfig.update((state) => ({ ...state, darkTheme: !state.darkTheme }));
    }
}
