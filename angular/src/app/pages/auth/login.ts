import { Component, computed, DestroyRef, inject, OnInit, AfterViewInit, signal, ViewChild, ElementRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { filter, take } from 'rxjs/operators';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { RippleModule } from 'primeng/ripple';
import { AppFloatingConfigurator } from '../../layout/component/app.floatingconfigurator';
import { AuthService, ConfigStateService, RestService } from '@abp/ng.core';
import { AbpOAuthService } from '@abp/ng.oauth';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { environment } from '@environments/environment';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { ApplicationLanguageSwitchService } from '../../core/services/application-language-switch.service';
import { LocalizationService } from '../../core/services/localization.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [ButtonModule, InputTextModule, PasswordModule, FormsModule, RouterModule, RippleModule, AppFloatingConfigurator, ToastModule, DialogModule, TranslatePipe],
    providers: [MessageService],
    styles: [
        `
            .login-lang {
                margin-top: 1.35rem;
                width: 100%;
            }
            .login-lang__track {
                display: flex;
                width: 100%;
                border-bottom: 1px solid var(--surface-border);
            }
            .login-lang__btn {
                flex: 1;
                margin: 0;
                border-radius: 0;
                border-top: none;
                border-right: none;
                border-left: none;
                border-bottom: 2px solid transparent;
                margin-bottom: -1px;
                padding: 0.55rem 0.5rem 0.65rem;
                font-size: 0.8125rem;
                font-weight: 500;
                letter-spacing: 0.02em;
                cursor: pointer;
                color: color-mix(in srgb, var(--text-color-secondary) 72%, var(--surface-0, #fff) 28%);
                background: transparent;
                transition: color 0.2s ease, border-color 0.2s ease;
            }
            :host-context(.app-dark) .login-lang__btn:not(.login-lang__btn--on) {
                color: color-mix(in srgb, var(--text-color-secondary) 65%, transparent);
            }
            .login-lang__btn:hover:not(.login-lang__btn--on) {
                color: color-mix(in srgb, var(--text-color-secondary) 88%, var(--text-color) 12%);
            }
            .login-lang__btn--on {
                color: var(--text-color);
                border-bottom-color: color-mix(in srgb, var(--text-color) 38%, var(--surface-border) 62%);
                font-weight: 500;
            }
            .login-lang__btn:focus-visible {
                outline: 2px solid color-mix(in srgb, var(--text-color) 45%, var(--surface-border) 55%);
                outline-offset: 1px;
            }
        `
    ],
    template: `
        <app-floating-configurator [hideUi]="true" />
        <p-toast />
        <div class="flex items-center justify-center min-h-screen w-full overflow-hidden bg-cover bg-left md:bg-center bg-no-repeat" style="background-image: url('/assets/images/background-login.png')">
            <div class="flex flex-col items-center justify-center">
                <div class="w-full h-full md:w-120" style="border-radius: 14px; padding: 2px; background: linear-gradient(180deg, #007BFF 0%, rgba(41, 124, 241, 0) 100%)">
                    <div class="flex flex-col gap-[24px] w-full bg-surface-0 dark:bg-surface-900 pt-8 pr-8 pb-4 pl-8 sm:px-10" style="border-radius: 12px">
                        <div class="text-center">
                            <img src="/assets/images/ione-logo-login.png" alt="iOne Logo" class="shrink-0 mx-auto" style="height: 6rem; width: auto;" />
                        </div>

                        <form (ngSubmit)="onLogin(); $event.preventDefault()">
                            <div>
                                <label for="email1" class="block text-surface-900 dark:text-surface-0 text-xl font-medium mb-2">{{ 'AbpAccount::EmailAddress' | translate }}</label>
                                <input #emailInput pInputText id="email1" type="text" [placeholder]="'AbpAccount::EmailAddress' | translate" class="w-full mb-8" [(ngModel)]="email" name="email" (keydown.enter)="onLogin(); $event.preventDefault()" />

                                <label for="password1" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">{{ 'AbpAccount::Password' | translate }}</label>
                                <p-password id="password1" [(ngModel)]="password" [placeholder]="'AbpAccount::Password' | translate" [toggleMask]="true" styleClass="mb-4" [fluid]="true" [feedback]="false" (onKeyDown)="onPasswordKeyDown($event)" [ngModelOptions]="{standalone: true}"></p-password>

                            <div class="flex items-center justify-end mb-8">
                                <button
                                    type="button"
                                    class="font-medium no-underline cursor-pointer text-primary bg-transparent border-0 p-0"
                                    (click)="openForgotPasswordDialog()"
                                >
                                    {{ 'AbpAccount::ForgotPassword' | translate }}
                                </button>
                            </div>
                                <p-button 
                                    type="submit"
                                    [label]="'AbpAccount::SignIn' | translate" 
                                    styleClass="w-full h-[32px] px-3 rounded-[6px]" 
                                    [loading]="isLoading"
                                    [disabled]="!email || !password || isLoading">
                                </p-button>
                                <div class="login-lang" role="group" aria-label="Ngôn ngữ">
                                    <div class="login-lang__track">
                                        <button
                                            type="button"
                                            class="login-lang__btn"
                                            [class.login-lang__btn--on]="isEnglishActive()"
                                            (click)="selectCultureForLogin('en')"
                                        >
                                            English
                                        </button>
                                        <button
                                            type="button"
                                            class="login-lang__btn"
                                            [class.login-lang__btn--on]="isVietnameseActive()"
                                            (click)="selectCultureForLogin('vi')"
                                        >
                                            Tiếng Việt
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </form>
                        <p class="font-inter font-normal text-[12px] leading-[100%] tracking-normal align-middle text-center text-[#606060]">
                            {{ 'iOne::AppSlogan' | translate }}
                        </p>
                    </div>
                </div>
            </div>
        </div>

        <p-dialog
            [(visible)]="forgotPasswordDialogVisible"
            [modal]="true"
            [dismissableMask]="true"
            [closable]="!isSendingResetEmail"
            [draggable]="false"
            [resizable]="false"
            [style]="{ width: '28rem', maxWidth: '95vw' }"
            [header]="'AbpAccount::ForgotPassword' | translate"
        >
            <div class="flex flex-col gap-3">
                <p class="m-0 text-surface-600 dark:text-surface-300">
                    {{ 'AbpAccount::ForgotPassword' | translate }}
                </p>
                <div>
                    <label for="forgotPasswordEmail" class="block text-surface-900 dark:text-surface-0 font-medium mb-2">
                        {{ 'AbpAccount::EmailAddress' | translate }}
                    </label>
                    <input
                        id="forgotPasswordEmail"
                        pInputText
                        type="email"
                        class="w-full"
                        [(ngModel)]="forgotPasswordEmail"
                        [disabled]="isSendingResetEmail"
                        autocomplete="email"
                    />
                </div>
                <small class="text-surface-500 dark:text-surface-400">
                    {{ 'AbpAccount::PasswordResetMailSentMessage' | translate }}
                </small>
            </div>

            <ng-template pTemplate="footer">
                <div class="flex justify-end gap-2">
                    <p-button
                        type="button"
                        severity="secondary"
                        [outlined]="true"
                        [label]="'AbpUi::Cancel' | translate"
                        [disabled]="isSendingResetEmail"
                        (onClick)="forgotPasswordDialogVisible = false"
                    />
                    <p-button
                        type="button"
                        [label]="'AbpUi::Submit' | translate"
                        [loading]="isSendingResetEmail"
                        [disabled]="!forgotPasswordEmail || isSendingResetEmail"
                        (onClick)="sendPasswordResetEmail()"
                    />
                </div>
            </ng-template>
        </p-dialog>
    `
})
export class Login implements OnInit, AfterViewInit {
    @ViewChild('emailInput') emailInput!: ElementRef<HTMLInputElement>;

    private authService = inject(AuthService);
    private abpOAuthService = inject(AbpOAuthService);
    private configState = inject(ConfigStateService);
    private router = inject(Router);
    private messageService = inject(MessageService);
    private restService = inject(RestService);
    private localizationService = inject(LocalizationService);
    private languageSwitch = inject(ApplicationLanguageSwitchService);
    private destroyRef = inject(DestroyRef);

    readonly currentCulture = signal('');
    readonly enCultureName = signal('en');
    readonly viCultureName = signal('vi');

    readonly isEnglishActive = computed(() => this.langPrimary(this.currentCulture()) === 'en');
    readonly isVietnameseActive = computed(() => this.langPrimary(this.currentCulture()) === 'vi');

    email: string = '';
    password: string = '';
    isLoading: boolean = false;
    forgotPasswordDialogVisible = false;
    forgotPasswordEmail = '';
    isSendingResetEmail = false;

    ngOnInit() {
        // Redirect to home if already authenticated
        if (this.authService.isAuthenticated) {
            this.router.navigate(['/']);
            return;
        }

        this.localizationService
            .getCurrentCulture$()
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe((c) => this.currentCulture.set(c || ''));

        this.localizationService
            .getLanguages$()
            .pipe(filter((l) => l.length > 0), take(1), takeUntilDestroyed(this.destroyRef))
            .subscribe((langs) => {
                const en = langs.find((l) => this.langPrimary(l.cultureName || '') === 'en')?.cultureName ?? 'en';
                const vi = langs.find((l) => this.langPrimary(l.cultureName || '') === 'vi')?.cultureName ?? 'vi';
                this.enCultureName.set(en);
                this.viCultureName.set(vi);

                // Ngôn ngữ mặc định (Abp.Localization.DefaultLanguage) do AppComponent xử lý sau khi load application-configuration.
            });
    }

    selectCultureForLogin(kind: 'en' | 'vi'): void {
        const target = kind === 'en' ? this.enCultureName() : this.viCultureName();
        if (this.langPrimary(this.localizationService.getCurrentCulture()) === kind) {
            return;
        }
        this.languageSwitch.switchToCulture(target, true);
    }

    private langPrimary(culture: string | undefined | null): string {
        return (culture || '').toLowerCase().split(/[-_]/)[0] || '';
    }

    ngAfterViewInit() {
        // Auto focus on email input (only if no element is already focused)
        if (this.emailInput?.nativeElement && document.activeElement === document.body) {
            setTimeout(() => {
                this.emailInput.nativeElement.focus();
            }, 100);
        }
    }

    onLogin() {
        // Prevent double submission
        if (this.isLoading) {
            return;
        }

        if (!this.email || !this.password) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpAccount::Validation'),
                detail: this.localizationService.localize('AbpAccount::PleaseEnterEmailAndPassword')
            });
            return;
        }

        this.isLoading = true;

        // Use AbpOAuthService.loginUsingGrant for password grant type
        // This properly handles token storage and authentication state
        const oAuthConfig = environment.oAuthConfig;
        if (!oAuthConfig) {
            this.isLoading = false;
            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpAccount::ConfigurationError'),
                detail: this.localizationService.localize('AbpAccount::OAuthConfigurationMissing'),
                life: 5000
            });
            return;
        }

        // Use loginUsingGrant which properly stores tokens and sets authentication state
        this.abpOAuthService.loginUsingGrant(
            'password',
            {
                username: this.email,
                password: this.password,
                client_id: oAuthConfig.clientId || '',
                scope: oAuthConfig.scope || 'offline_access iOne'
            }
        ).then((response) => {
            // Verify authentication was successful
            if (!this.authService.isAuthenticated) {
                this.isLoading = false;
                this.messageService.add({
                    severity: 'error',
                    summary: this.localizationService.localize('AbpAccount::LoginFailed'),
                    detail: this.localizationService.localize('AbpAccount::AuthenticationFailed'),
                    life: 5000
                });
                return;
            }

            // Refresh application state to get user information
            this.configState.refreshAppState().subscribe({
                next: () => {
                    this.isLoading = false;
                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpUi::Success'),
                        detail: this.localizationService.localize('AbpAccount::LoginSuccessful'),
                        life: 3000
                    });
                    this.router.navigate(['/']);
                },
                error: () => {
                    // Even if refresh fails, still navigate (user info will load on next page)
                    this.isLoading = false;
                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpUi::Success'),
                        detail: this.localizationService.localize('AbpAccount::LoginSuccessful'),
                        life: 3000
                    });
                    this.router.navigate(['/']);
                }
            });
        }).catch((error) => {
            this.isLoading = false;
            console.error('Login error:', error);

            let errorMessage = this.localizationService.localize('AbpAccount::InvalidCredentials');

            if (error?.error) {
                if (typeof error.error === 'string') {
                    errorMessage = error.error;
                } else {
                    errorMessage = error.error.error_description ||
                        error.error.error?.message ||
                        error.error.message ||
                        error.error.error ||
                        errorMessage;
                }
            } else if (error?.message) {
                errorMessage = error.message;
            }

            // Check if it's a 400 Bad Request
            if (error?.status === 400) {
                const errorStr = JSON.stringify(error?.error || '').toLowerCase();
                if (errorStr.includes('invalid_grant') ||
                    errorStr.includes('invalid_username_or_password') ||
                    errorStr.includes('invalid_credentials')) {
                    errorMessage = this.localizationService.localize('AbpAccount::InvalidUserNameOrPassword');
                } else if (errorStr.includes('unsupported_grant_type')) {
                    errorMessage = this.localizationService.localize('AbpAccount::OAuthServerDoesNotSupportPassword');
                }
            }

            this.messageService.add({
                severity: 'error',
                summary: this.localizationService.localize('AbpAccount::LoginFailed'),
                detail: errorMessage,
                life: 5000
            });
        });
    }

    onPasswordKeyDown(event: Event) {
        const keyboardEvent = event as KeyboardEvent;
        if (keyboardEvent.key === 'Enter' || keyboardEvent.keyCode === 13) {
            keyboardEvent.preventDefault();
            this.onLogin();
        }
    }

    openForgotPasswordDialog(): void {
        this.forgotPasswordEmail = this.email || '';
        this.forgotPasswordDialogVisible = true;
    }

    sendPasswordResetEmail(): void {
        const email = this.forgotPasswordEmail?.trim();
        if (!email) {
            this.messageService.add({
                severity: 'warn',
                summary: this.localizationService.localize('AbpAccount::Validation'),
                detail: this.localizationService.localize('AbpAccount::InvalidEmailAddress', 'Email không hợp lệ')
            });
            return;
        }

        this.isSendingResetEmail = true;
        this.restService
            .request<any, void>(
                {
                    method: 'POST',
                    url: '/api/account/send-password-reset-code',
                    body: {
                        email,
                        appName: 'Angular',
                        returnUrl: `${window.location.origin}/auth/login`,
                        returnUrlHash: ''
                    }
                },
                { apiName: 'Default' }
            )
            .subscribe({
                next: () => {
                    this.isSendingResetEmail = false;
                    this.forgotPasswordDialogVisible = false;
                    this.messageService.add({
                        severity: 'success',
                        summary: this.localizationService.localize('AbpUi::Success'),
                        detail: this.localizationService.localize(
                            'AbpAccount::PasswordResetMailSentMessage',
                            'Nếu email tồn tại trong hệ thống, liên kết đặt lại mật khẩu đã được gửi.'
                        ),
                        life: 5000
                    });
                },
                error: (error) => {
                    this.isSendingResetEmail = false;
                    const detail =
                        error?.error?.error?.message ||
                        error?.error?.message ||
                        this.localizationService.localize(
                            'AbpAccount::InvalidEmailAddress',
                            'Không thể gửi email đặt lại mật khẩu.'
                        );
                    this.messageService.add({
                        severity: 'error',
                        summary: this.localizationService.localize('AbpUi::Error'),
                        detail,
                        life: 5000
                    });
                }
            });
    }
}
