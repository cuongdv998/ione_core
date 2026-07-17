/**
 * Types và interfaces cho localization system
 */

import type { LanguageInfo } from '../../proxy/volo/abp/localization/models';

/**
 * Translation key format
 * - "Key" - sử dụng default resource
 * - "ResourceName::Key" - chỉ định resource cụ thể
 */
export type TranslationKey = string;

/**
 * Resource name
 */
export type ResourceName = string;

/**
 * Translation value
 */
export type TranslationValue = string;

/**
 * Translations map cho một resource
 * Key: translation key, Value: translated text
 */
export type ResourceTranslations = Record<string, string>;

/**
 * All resources translations
 * Key: resource name, Value: ResourceTranslations
 */
export type AllTranslations = Record<ResourceName, ResourceTranslations>;

/**
 * Language info (re-export từ ABP models)
 */
export type { LanguageInfo };
