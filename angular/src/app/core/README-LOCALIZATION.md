# Core Module - Localization System

Hệ thống đa ngôn ngữ (i18n) tương tự ABP Framework cho Angular project.

## Cấu trúc

- **Services**: `LocalizationService` - Service chính để translate
- **Pipes**: `TranslatePipe` - Pipe để sử dụng trong templates
- **Directives**: `TranslateDirective` - Directive để translate element content
- **Components**: `LanguageSelectorComponent` - Component để chọn ngôn ngữ

## Cách sử dụng

### 1. LocalizationService

```typescript
import { LocalizationService } from '@/core/services/ocalization.service';

// Inject service
constructor(private localizationService: LocalizationService) {}

// Translate một key (sử dụng default resource)
const text = this.localizationService.localize('Welcome');

// Translate với resource cụ thể
const text = this.localizationService.localize('AbpAccount::Welcome');

// Translate với defaultValue
const text = this.localizationService.localize('Welcome', 'Hello');

// Observable
this.localizationService.localize$('Welcome').subscribe(text => {
  // ...
});

// Lấy current culture
const culture = this.localizationService.getCurrentCulture(); // 'en', 'vi', etc.

// Lấy danh sách languages
const languages = this.localizationService.getLanguages();
```

### 2. TranslatePipe (Template)

```html
<!-- Sử dụng default resource -->
<span>{{ 'Welcome' | translate }}</span>

<!-- Sử dụng resource cụ thể -->
<span>{{ 'AbpAccount::Welcome' | translate }}</span>

<!-- Với defaultValue -->
<span>{{ 'Welcome' | translate:'Hello' }}</span>

<!-- Trong attributes -->
<input [placeholder]="'Search' | translate" />
```

### 3. TranslateDirective (Template)

```html
<!-- Translate text content -->
<span [appTranslate]="'Welcome'"></span>

<!-- Với defaultValue -->
<span [appTranslate]="'Welcome'" [defaultValue]="'Hello'"></span>

<!-- Với resource cụ thể -->
<span [appTranslate]="'AbpAccount::Welcome'"></span>
```

### 4. LanguageSelectorComponent

```html
<!-- Sử dụng trong template -->
<app-language-selector></app-language-selector>
```

Component này sẽ:
- Hiển thị dropdown với danh sách languages
- Cho phép user chọn ngôn ngữ
- Tự động load translations mới khi culture thay đổi

### 5. Translation Key Format

ABP sử dụng format: `ResourceName::Key`

- **Với resource**: `AbpAccount::Welcome` - tìm trong resource "AbpAccount"
- **Không có resource**: `Welcome` - tìm trong default resource

Default resource được định nghĩa trong `ApplicationLocalizationConfigurationDto.defaultResourceName`.

## Integration với ABP

Hệ thống này tương thích hoàn toàn với ABP Framework:
- Sử dụng `ConfigStateService` từ `@abp/ng.core`
- Translations được lấy từ `ApplicationLocalizationConfigurationDto.values`
- Tương thích với ABP localization system trên backend
- Hỗ trợ multiple resources
- Tự động reactive khi culture thay đổi

## Cấu trúc Translations trong ABP

ABP lưu translations trong cấu trúc:

```typescript
{
  values: {
    "AbpAccount": {
      "Welcome": "Welcome",
      "Login": "Login",
      // ...
    },
    "Default": {
      "Save": "Save",
      "Cancel": "Cancel",
      // ...
    }
  },
  resources: {
    "AbpAccount": {
      texts: { /* translations */ },
      baseResources: []
    }
  },
  currentCulture: {
    cultureName: "en",
    displayName: "English",
    // ...
  },
  languages: [
    { cultureName: "en", displayName: "English" },
    { cultureName: "vi", displayName: "Tiếng Việt" }
  ],
  defaultResourceName: "Default"
}
```

## Lưu ý

1. Translations được cache trong `ConfigStateService`
2. Khi culture thay đổi, cần reload translations từ API
3. `LanguageSelectorComponent` tự động xử lý việc reload translations
4. Nếu không tìm thấy translation, sẽ trả về `defaultValue` hoặc key gốc
5. Pipe và Directive đều reactive, tự động update khi culture thay đổi

## Best Practices

1. **Sử dụng resource names rõ ràng**: `AbpAccount::Welcome` thay vì chỉ `Welcome`
2. **Luôn có defaultValue**: Để tránh hiển thị key khi không có translation
3. **Sử dụng Pipe trong templates**: Dễ đọc và maintain hơn
4. **Cache translations**: ABP tự động cache, không cần làm gì thêm
5. **Test với nhiều languages**: Đảm bảo UI không bị break với text dài
