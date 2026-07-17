import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { PanelModule } from 'primeng/panel';
import { PasswordModule } from 'primeng/password';
import { TabsModule } from 'primeng/tabs';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { AppSettingService } from '@/proxy/setting/app-setting.service';
import { PermissionService } from '@/core/services/permission.service';
import { LocalizationService } from '@/core/services/localization.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import {
  ACCOUNT_TAB_KEYS,
  EMAIL_TAB_KEYS_GET,
  EMAIL_TAB_KEYS_SAVE_BASE,
  EmailSettingKeys,
  IdentitySettingKeys,
  PASSWORD_TAB_KEYS,
} from './system-settings.keys';

@Component({
  selector: 'app-system-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TabsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    CheckboxModule,
    PasswordModule,
    ToastModule,
    TranslatePipe,
  ],
  templateUrl: './system-settings.component.html',
  styleUrl: './system-settings.component.scss',
  providers: [MessageService],
})
export class SystemSettingsComponent implements OnInit {
  private readonly appSettingService = inject(AppSettingService);
  readonly permissionService = inject(PermissionService);
  private readonly localization = inject(LocalizationService);
  private readonly messageService = inject(MessageService);

  readonly VIEW = 'iOne.AppSetting.View';
  readonly MANAGE = 'iOne.AppSetting.Manage';

  /** PrimeNG Tabs dùng value dạng chuỗi */
  activeTabIndex: string | number = '0';
  loading = false;
  saving = false;

  loadedAccount = false;
  loadedPassword = false;
  loadedEmail = false;

  accountValues: Record<string, string> = {};
  passwordValues: Record<string, string> = {};
  emailValues: Record<string, string> = {};

  smtpPasswordInput = '';
  testEmailTo = '';
  sendingTestEmail = false;

  ngOnInit(): void {
    if (this.permissionService.isGranted(this.VIEW)) {
      this.loadTab('account');
    }
  }

  get canManage(): boolean {
    return this.permissionService.isGranted(this.MANAGE);
  }

  readonly IdentitySettingKeys = IdentitySettingKeys;
  readonly EmailSettingKeys = EmailSettingKeys;

  onTabChange(index: number | string | undefined): void {
    if (index === undefined || index === null) return;
    const i = typeof index === 'number' ? index : Number(index);
    if (Number.isNaN(i)) return;
    if (!this.permissionService.isGranted(this.VIEW)) return;
    if (i === 0 && !this.loadedAccount) {
      this.loadTab('account');
    }
    if (i === 1 && !this.loadedPassword) {
      this.loadTab('password');
    }
    if (i === 2 && !this.loadedEmail) {
      this.loadTab('email');
    }
  }

  parseBool(raw: string | undefined): boolean {
    if (raw == null || raw === '') return false;
    return raw.toLowerCase() === 'true' || raw === '1';
  }

  bool(map: Record<string, string>, name: string): boolean {
    return this.parseBool(map[name]);
  }

  setBool(map: Record<string, string>, name: string, v: boolean): void {
    this.patchValues(map, name, v ? 'True' : 'False');
  }

  num(map: Record<string, string>, name: string, fallback: number): number {
    const raw = map[name];
    if (raw == null || raw === '') return fallback;
    const n = Number(raw);
    return Number.isFinite(n) ? n : fallback;
  }

  setNum(map: Record<string, string>, name: string, v: number | null): void {
    let s = '0';
    if (v !== null && v !== undefined && !Number.isNaN(v)) {
      s = String(Math.trunc(v));
    }
    this.patchValues(map, name, s);
  }

  text(map: Record<string, string>, name: string): string {
    return map[name] ?? '';
  }

  setText(map: Record<string, string>, name: string, v: string): void {
    this.patchValues(map, name, v);
  }

  private patchValues(which: Record<string, string>, name: string, value: string): void {
    const next = { ...which, [name]: value };
    if (which === this.accountValues) this.accountValues = next;
    else if (which === this.passwordValues) this.passwordValues = next;
    else this.emailValues = next;
  }

  loadTab(tab: 'account' | 'password' | 'email'): void {
    const keys =
      tab === 'account'
        ? ACCOUNT_TAB_KEYS
        : tab === 'password'
          ? PASSWORD_TAB_KEYS
          : EMAIL_TAB_KEYS_GET;

    let targetSetter: (m: Record<string, string>) => void;
    let loadedFlagSetter: () => void;
    if (tab === 'account') {
      targetSetter = (m) => (this.accountValues = m);
      loadedFlagSetter = () => (this.loadedAccount = true);
    } else if (tab === 'password') {
      targetSetter = (m) => (this.passwordValues = m);
      loadedFlagSetter = () => (this.loadedPassword = true);
    } else {
      targetSetter = (m) => (this.emailValues = m);
      loadedFlagSetter = () => {
        this.loadedEmail = true;
        this.smtpPasswordInput = '';
      };
    }

    this.loading = true;
    forkJoin(
      keys.map((name) =>
        this.appSettingService.get(name).pipe(
          catchError(() => of(null)),
        ),
      ),
    )
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (values) => {
          const merged: Record<string, string> = {};
          keys.forEach((name, i) => {
            const val = values[i];
            merged[name] = val != null ? String(val) : '';
          });
          targetSetter(merged);
          loadedFlagSetter();
        },
      });
  }

  saveAccount(): void {
    this.persistKeys(this.accountValues, [...ACCOUNT_TAB_KEYS]);
  }

  savePassword(): void {
    this.persistKeys(this.passwordValues, [...PASSWORD_TAB_KEYS]);
  }

  saveEmail(): void {
    const payload: { name: string; value: string }[] = EMAIL_TAB_KEYS_SAVE_BASE.map((name) => ({
      name,
      value: this.emailValues[name] ?? '',
    }));
    if (this.smtpPasswordInput.trim() !== '') {
      payload.push({ name: EmailSettingKeys.SmtpPassword, value: this.smtpPasswordInput });
    }
    this.persist(payload);
  }

  sendTestEmail(): void {
    if (!this.canManage) {
      return;
    }
    const to = this.testEmailTo?.trim();
    if (!to) {
      this.messageService.add({
        severity: 'warn',
        detail: this.localization.localize(
          'iOne::SystemSettings:TestEmailRecipientRequired',
          'Vui lòng nhập địa chỉ email người nhận.',
        ),
      });
      return;
    }
    this.sendingTestEmail = true;
    this.appSettingService
      .sendTestEmail({ to })
      .pipe(finalize(() => (this.sendingTestEmail = false)))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            detail: this.localization.localize(
              'iOne::SystemSettings:TestEmailSent',
              'Đã gửi email thử.',
            ),
          });
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            detail: this.localization.localize(
              'iOne::SystemSettings:TestEmailFailed',
              'Không gửi được email thử.',
            ),
          });
        },
      });
  }

  private persistKeys(record: Record<string, string>, names: readonly string[]): void {
    const payload = names.map((name) => ({ name, value: record[name] ?? '' }));
    this.persist(payload);
  }

  private persist(payload: { name: string; value: string }[]): void {
    if (!this.canManage) return;
    this.saving = true;
    forkJoin(payload.map(({ name, value }) => this.appSettingService.setGlobal(name, value)))
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: () => {
          const msg = this.localization.localize(
            'iOne::SystemSettings:SaveSuccess',
            'Đã lưu thiết lập.',
          );
          this.messageService.add({ severity: 'success', detail: msg });
          if (this.tabIndexNumeric() === 2 && this.smtpPasswordInput.trim() !== '') {
            this.smtpPasswordInput = '';
          }
        },
        error: () => {
          const msg = this.localization.localize(
            'iOne::SystemSettings:SaveError',
            'Không thể lưu thiết lập.',
          );
          this.messageService.add({ severity: 'error', detail: msg });
        },
      });
  }

  private tabIndexNumeric(): number {
    const v = this.activeTabIndex;
    const n = typeof v === 'number' ? v : Number(v);
    return Number.isNaN(n) ? 0 : n;
  }
}
