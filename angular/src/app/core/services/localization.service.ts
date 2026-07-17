import { Injectable, inject } from '@angular/core';
import { ConfigStateService } from '@abp/ng.core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

/**
 * Service để quản lý và sử dụng translations
 * Tương tự ABP LocalizationService
 */
@Injectable({
  providedIn: 'root',
})
export class LocalizationService {
  private configState = inject(ConfigStateService);

  /**
   * Lấy translation text
   * @param key Key của translation (format: "ResourceName::Key" hoặc "Key" nếu dùng default resource)
   * @param defaultValue Giá trị mặc định nếu không tìm thấy translation
   * @returns Translated text hoặc defaultValue
   */
  localize(key: string, defaultValue?: string): string {
    if (!key) {
      return defaultValue || key;
    }

    const localization = this.configState.getOne('localization');
    if (!localization) {
      return defaultValue || key;
    }

    // Parse key: "ResourceName::Key" hoặc "Key"
    const [resourceName, translationKey] = key.includes('::') 
      ? key.split('::', 2)
      : [localization.defaultResourceName || 'Default', key];

    // ABP lưu translations trong values: Record<resourceName, Record<key, value>>
    let resourceTranslations: Record<string, string> | undefined;
    
    if (localization.values && localization.values[resourceName]) {
      resourceTranslations = localization.values[resourceName];
    } else if (localization.resources && localization.resources[resourceName]) {
      // Fallback: lấy từ resources.texts
      resourceTranslations = localization.resources[resourceName].texts;
    }

    // Debug: Log khi không tìm thấy resource
    if (!resourceTranslations && key.includes('::')) {
      console.warn(`[Localization] Resource "${resourceName}" not found. Available resources:`, 
        Object.keys(localization.values || {}), 
        Object.keys(localization.resources || {}));
      console.warn(`[Localization] Looking for key: "${translationKey}" in resource: "${resourceName}"`);
    }

    if (!resourceTranslations) {
      return defaultValue || key;
    }

    // Lấy translation
    const translation = resourceTranslations[translationKey];
    
    // Debug: Log khi không tìm thấy translation
    if (!translation && key.includes('::')) {
      console.warn(`[Localization] Key "${translationKey}" not found in resource "${resourceName}". Available keys:`, 
        Object.keys(resourceTranslations).slice(0, 10));
    }
    
    return translation || defaultValue || key;
  }

  /**
   * Lấy translation text dưới dạng Observable
   * @param key Key của translation
   * @param defaultValue Giá trị mặc định
   * @returns Observable<string>
   */
  localize$(key: string, defaultValue?: string): Observable<string> {
    if (!key) {
      return of(defaultValue || key);
    }

    return this.configState.getOne$('localization').pipe(
      map((localization) => {
        if (!localization) {
          return defaultValue || key;
        }

        const [resourceName, translationKey] = key.includes('::') 
          ? key.split('::', 2)
          : [localization.defaultResourceName || 'Default', key];

        let resourceTranslations: Record<string, string> | undefined;
        
        if (localization.values && localization.values[resourceName]) {
          resourceTranslations = localization.values[resourceName];
        } else if (localization.resources && localization.resources[resourceName]) {
          resourceTranslations = localization.resources[resourceName].texts;
        }

        if (!resourceTranslations) {
          return defaultValue || key;
        }

        const translation = resourceTranslations[translationKey];
        return translation || defaultValue || key;
      })
    );
  }

  /**
   * Lấy current culture
   * @returns Current culture name (e.g., 'en', 'vi')
   */
  getCurrentCulture(): string {
    const localization = this.configState.getOne('localization');
    return localization?.currentCulture?.cultureName || '';
  }

  /**
   * Lấy current culture dưới dạng Observable
   * @returns Observable<string>
   */
  getCurrentCulture$(): Observable<string> {
    return this.configState.getOne$('localization').pipe(
      map((localization) => localization?.currentCulture?.cultureName || '')
    );
  }

  /**
   * Lấy danh sách languages
   * @returns Array of LanguageInfo
   */
  getLanguages() {
    const localization = this.configState.getOne('localization');
    return localization?.languages || [];
  }

  /**
   * Lấy danh sách languages dưới dạng Observable
   * @returns Observable<LanguageInfo[]>
   */
  getLanguages$() {
    return this.configState.getOne$('localization').pipe(
      map((localization) => localization?.languages || [])
    );
  }

  /**
   * Lấy default resource name
   * @returns Default resource name
   */
  getDefaultResourceName(): string | undefined {
    const localization = this.configState.getOne('localization');
    return localization?.defaultResourceName;
  }

  /**
   * Kiểm tra xem có translation cho key không
   * @param key Key của translation
   * @returns true nếu có translation
   */
  hasTranslation(key: string): boolean {
    if (!key) {
      return false;
    }

    const localization = this.configState.getOne('localization');
    if (!localization) {
      return false;
    }

    const [resourceName, translationKey] = key.includes('::') 
      ? key.split('::', 2)
      : [localization.defaultResourceName || 'Default', key];

    let resourceTranslations: Record<string, string> | undefined;
    
    if (localization.values && localization.values[resourceName]) {
      resourceTranslations = localization.values[resourceName];
    } else if (localization.resources && localization.resources[resourceName]) {
      resourceTranslations = localization.resources[resourceName].texts;
    }

    if (!resourceTranslations) {
      return false;
    }

    return translationKey in resourceTranslations;
  }

  /**
   * Lấy tất cả translations cho một resource
   * @param resourceName Tên resource
   * @returns Record<string, string> hoặc null
   */
  getResourceTranslations(resourceName: string): Record<string, string> | null {
    const localization = this.configState.getOne('localization');
    if (!localization) {
      return null;
    }

    if (localization.values && localization.values[resourceName]) {
      return localization.values[resourceName];
    }

    if (localization.resources && localization.resources[resourceName]) {
      return localization.resources[resourceName].texts;
    }

    return null;
  }
}
