import { Injectable, inject } from '@angular/core';
import { AbpLocalStorageService, ConfigStateService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

/**
 * Service để quản lý language preference của user
 * Lưu vào localStorage và tự động sử dụng default từ backend nếu chưa có
 */
@Injectable({
  providedIn: 'root',
})
export class LanguagePreferenceService {
  // ABP sử dụng key này để lưu language preference
  // Phải dùng đúng key để ABP tự động set Accept-Language header
  private readonly LANGUAGE_PREFERENCE_KEY = 'Abp.Localization.CultureName';
  /** Trùng setting hệ thống từ backend (ApplicationConfiguration.setting.values). */
  private readonly DEFAULT_LANGUAGE_SETTING_KEY = 'Abp.Localization.DefaultLanguage';
  private readonly ABP_SESSION_KEY = 'abpSession';
  private localStorage = inject(AbpLocalStorageService);
  private configState = inject(ConfigStateService);

  /**
   * Lấy language preference từ localStorage (chỉ từ localStorage, không fallback)
   * @returns Culture name từ localStorage hoặc null nếu chưa có
   */
  getSavedLanguagePreference(): string | null {
    return this.localStorage.getItem(this.LANGUAGE_PREFERENCE_KEY);
  }

  /**
   * Lấy language preference từ localStorage hoặc default từ backend
   * @returns Culture name (e.g., 'en', 'vi')
   */
  getLanguagePreference(): string | null {
    // Ưu tiên lấy từ localStorage (user đã chọn trước đó)
    const savedLanguage = this.getSavedLanguagePreference();
    if (savedLanguage) {
      return savedLanguage;
    }

    // Nếu chưa có, lấy từ backend configuration (default)
    return this.getDefaultLanguage();
  }

  /**
   * Lưu language preference vào localStorage
   * @param cultureName Culture name cần lưu
   */
  setLanguagePreference(cultureName: string): void {
    if (cultureName) {
      this.localStorage.setItem(this.LANGUAGE_PREFERENCE_KEY, cultureName);
      
      // Cập nhật abpSession để đồng bộ language
      // ABP có thể ưu tiên lấy từ abpSession
      const abpSession = this.localStorage.getItem(this.ABP_SESSION_KEY);
      if (abpSession) {
        try {
          const session = JSON.parse(abpSession);
          session.language = cultureName;
          this.localStorage.setItem(this.ABP_SESSION_KEY, JSON.stringify(session));
        } catch (error) {
          console.error('Error updating abpSession:', error);
        }
      }
    }
  }

  /**
   * Xóa language preference (sẽ dùng default từ backend)
   */
  clearLanguagePreference(): void {
    this.localStorage.removeItem(this.LANGUAGE_PREFERENCE_KEY);
    
    // Xóa language trong abpSession
    const abpSession = this.localStorage.getItem(this.ABP_SESSION_KEY);
    if (abpSession) {
      try {
        const session = JSON.parse(abpSession);
        delete session.language;
        this.localStorage.setItem(this.ABP_SESSION_KEY, JSON.stringify(session));
      } catch (error) {
        console.error('Error clearing language from abpSession:', error);
      }
    }
  }

  /**
   * Lấy default language từ backend configuration
   * @returns Default culture name hoặc null
   */
  getDefaultLanguage(): string | null {
    const setting = this.configState.getOne('setting') as { values?: Record<string, string> } | undefined;
    const fromSetting = setting?.values?.[this.DEFAULT_LANGUAGE_SETTING_KEY]?.trim();
    if (fromSetting) {
      return fromSetting;
    }
    const localization = this.configState.getOne('localization');
    return localization?.currentCulture?.cultureName || null;
  }

  /**
   * Lấy default language dưới dạng Observable
   * @returns Observable<string | null>
   */
  getDefaultLanguage$() {
    return this.configState.getOne$('localization').pipe(
      map(() => this.getDefaultLanguage())
    );
  }
}
