import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { RestService } from '@abp/ng.core';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { LocalizationService } from '../../core/services/localization.service';

@Component({
    selector: 'app-reset-password',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule, ButtonModule, PasswordModule, ToastModule, TranslatePipe],
    providers: [MessageService],
    styles: [
        `
            :host ::ng-deep .reset-submit-btn.p-button {
                background: #da441d;
                border-color: #da441d;
            }
            :host ::ng-deep .reset-submit-btn.p-button:hover:not(:disabled) {
                background: #c93d1a;
                border-color: #c93d1a;
            }
            :host ::ng-deep .reset-submit-btn.p-button:enabled:active {
                background: #b63617;
                border-color: #b63617;
            }
        `
    ],
    template: `
        <p-toast />
        <div class="flex items-center justify-center min-h-screen w-full overflow-hidden bg-cover bg-left md:bg-center bg-no-repeat" style="background-image: url('/assets/images/background-login.png')">
            <div class="flex flex-col items-center justify-center">
                <div class="w-full h-full md:w-120" style="border-radius: 14px; padding: 2px; background: linear-gradient(180deg, #007BFF 0%, rgba(41, 124, 241, 0) 100%)">
                    <div class="flex flex-col gap-6 w-full bg-surface-0 dark:bg-surface-900 pt-8 pr-8 pb-6 pl-8 sm:px-10" style="border-radius: 12px">
                        <div class="text-center">
                            <img src="/assets/images/ione-logo-login.png" alt="iOne Logo" class="shrink-0 mx-auto" style="height: 6rem; width: auto;" />
                        </div>

                        <h2 class="text-surface-900 dark:text-surface-0 text-2xl font-semibold text-center m-0">
                            {{ 'AbpAccount::ResetPassword' | translate }}
                        </h2>

                        @if (isTokenValid === false) {
                            <p class="text-center text-red-500 m-0">
                                {{ 'AbpAccount::InvalidPasswordResetToken' | translate }}
                            </p>
                            <p-button
                                [label]="'AbpAccount::Login' | translate"
                                styleClass="w-full"
                                (onClick)="goLogin()"
                            />
                        } @else {
                            <div class="flex flex-col gap-4">
                                <div>
                                    <label class="block text-surface-900 dark:text-surface-0 font-medium text-lg mb-2">
                                        {{ 'AbpAccount::Password' | translate }}
                                    </label>
                                    <p-password
                                        [(ngModel)]="password"
                                        [feedback]="true"
                                        [toggleMask]="true"
                                        [fluid]="true"
                                    />
                                </div>

                                <div>
                                    <label class="block text-surface-900 dark:text-surface-0 font-medium text-lg mb-2">
                                        {{ 'AbpAccount::ConfirmPassword' | translate }}
                                    </label>
                                    <p-password
                                        [(ngModel)]="confirmPassword"
                                        [feedback]="false"
                                        [toggleMask]="true"
                                        [fluid]="true"
                                    />
                                </div>
                            </div>

                            <div class="flex flex-col gap-2">
                                <p-button
                                    [label]="'iOne::ResetPassword:Submit' | translate"
                                    styleClass="w-full h-[32px] px-3 rounded-[6px] reset-submit-btn"
                                    [loading]="isSubmitting"
                                    [disabled]="!canSubmit()"
                                    (onClick)="resetPassword()"
                                />
                            </div>
                        }
                    </div>
                </div>
            </div>
        </div>
    `
})
export class ResetPassword implements OnInit {
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private restService = inject(RestService);
    private messageService = inject(MessageService);
    private localizationService = inject(LocalizationService);

    userId = '';
    resetToken = '';
    password = '';
    confirmPassword = '';

    isSubmitting = false;
    isTokenValid: boolean | null = null;

    ngOnInit(): void {
        this.userId = this.route.snapshot.queryParamMap.get('userId') || '';
        this.resetToken = this.route.snapshot.queryParamMap.get('resetToken') || '';

        if (!this.userId || !this.resetToken) {
            this.isTokenValid = false;
            return;
        }

        this.verifyToken();
    }

    canSubmit(): boolean {
        return !!this.password && !!this.confirmPassword && this.password === this.confirmPassword && !this.isSubmitting && this.isTokenValid !== false;
    }

    private verifyToken(): void {
        this.restService
            .request<any, boolean>(
                {
                    method: 'POST',
                    url: '/api/account/verify-password-reset-token',
                    body: { userId: this.userId, resetToken: this.resetToken }
                },
                { apiName: 'Default' }
            )
            .subscribe({
                next: (ok) => {
                    this.isTokenValid = !!ok;
                },
                error: () => {
                    this.isTokenValid = false;
                }
            });
    }

    resetPassword(): void {
        if (!this.canSubmit()) {
            return;
        }

        if (this.password !== this.confirmPassword) {
            this.messageService.add({
                severity: 'warn',
                detail: this.localizationService.localize('AbpIdentity::PasswordsShouldMatch')
            });
            return;
        }

        this.isSubmitting = true;
        this.restService
            .request<any, void>(
                {
                    method: 'POST',
                    url: '/api/account/reset-password',
                    body: {
                        userId: this.userId,
                        resetToken: this.resetToken,
                        password: this.password
                    }
                },
                { apiName: 'Default' }
            )
            .subscribe({
                next: () => {
                    this.isSubmitting = false;
                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpUi::Success'),
                        detail: this.localizationService.localize('AbpAccount::PasswordReset')
                    });
                    this.goLogin();
                },
                error: (error) => {
                    this.isSubmitting = false;
                    const detail =
                        error?.error?.error?.message ||
                        error?.error?.message ||
                        this.localizationService.localize('AbpUi::InternalServerErrorMessage');
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail
                    });
                }
            });
    }

    goLogin(): void {
        this.router.navigate(['/auth/login']);
    }
}

