# 🌐 Localization - Audit Logs

## Tổng quan

Component Audit Logs đã được tích hợp đầy đủ với ABP Localization system, hỗ trợ đa ngôn ngữ.

## 📋 Localization Keys

### Resource Name
```
AbpAuditLogging
```

### Danh sách Keys đã sử dụng

| Key | English | Tiếng Việt | Sử dụng tại |
|-----|---------|------------|-------------|
| `SearchInformation` | Search information | Thông tin tìm kiếm | Panel header |
| `DataList` | Data list | Danh sách dữ liệu | Panel header |
| `Search` | Search | Tìm kiếm | Button |
| `Reset` | Reset | Đặt lại | Button |
| `ViewDetail` | View detail | Xem chi tiết | Action button |
| `FromDate` | From date | Từ ngày | Label |
| `ToDate` | To date | Đến ngày | Label |
| `SelectStartDate` | Select start date | Chọn ngày bắt đầu | Placeholder |
| `SelectEndDate` | Select end date | Chọn ngày kết thúc | Placeholder |
| `HttpMethod` | HTTP method | Phương thức HTTP | Label |
| `SelectHttpMethod` | Select HTTP method | Chọn phương thức | Placeholder |
| `HttpStatusCode` | HTTP status code | Mã trạng thái HTTP | Label |
| `EnterStatusCode` | Enter status code | Nhập mã trạng thái | Placeholder |
| `Url` | URL | URL | Label |
| `EnterUrl` | Enter URL | Nhập URL | Placeholder |
| `UserName` | User name | Tên tài khoản | Label, Column |
| `EnterUserName` | Enter user name | Nhập tên người dùng | Placeholder |
| `ApplicationName` | Application name | Tên ứng dụng | Label, Column |
| `EnterApplicationName` | Enter application name | Nhập tên ứng dụng | Placeholder |
| `IpAddress` | IP address | Địa chỉ IP | Label, Column |
| `EnterIpAddress` | Enter IP address | Nhập địa chỉ IP | Placeholder |
| `CorrelationId` | Correlation Id | Id tương quan | Label |
| `EnterCorrelationId` | Enter correlation ID | Nhập Correlation ID | Placeholder |
| `MinExecutionDuration` | Min duration (ms) | Thời gian tối thiểu (ms) | Label |
| `MaxExecutionDuration` | Max duration (ms) | Thời gian tối đa (ms) | Label |
| `EnterDuration` | Enter duration | Nhập thời gian | Placeholder |
| `HasException` | Has exception | Có ngoại lệ | Label |
| `Status` | Status | Trạng thái | Column |
| `ExecutionTime` | Time | Thời gian | Column |
| `DurationMs` | Duration (ms) | Thời lượng (ms) | Column |
| `AuditLogDetailTitle` | Audit Log Detail | Chi tiết Audit Log | Dialog header |
| `GeneralInformation` | General information | Thông tin chung | Tab |
| `EntityChanges` | Entity changes | Thay đổi thực thể | Tab |
| `Actions` | Actions | hành động | Tab |
| `NoEntityChanges` | No entity changes | Không có thay đổi thực thể nào | Message |
| `NoActions` | No actions | Không có action nào | Message |
| `PropertyChanges` | Property changes | Thay đổi thuộc tính | Label |
| `PropertyName` | Property name | Tên tài sản | Table header |
| `OriginalValue` | Old value | Giá trị gốc | Table header |
| `NewValue` | New value | Giá trị mới | Table header |

## 🔧 Implementation

### Component (TypeScript)

```typescript
import { LocalizationService } from '@/core/services/localization.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';

@Component({
  imports: [TranslatePipe, ...],
  ...
})
export class AuditLogsComponent {
  constructor(private localizationService: LocalizationService) {
    this.initializeColumns();
  }

  private initializeColumns(): void {
    this.columns = [
      {
        field: 'userName',
        header: this.localizationService.localize('AbpAuditLogging::UserName'),
        ...
      },
      ...
    ];
  }
}
```

### Template (HTML)

```html
<!-- Using translate pipe -->
<label>{{ 'AbpAuditLogging::UserName' | translate }}</label>

<!-- In attributes -->
<input [placeholder]="'AbpAuditLogging::EnterUserName' | translate" />

<!-- In panel headers -->
<p-panel [header]="'AbpAuditLogging::SearchInformation' | translate">
```

## 📝 Thêm Keys mới

### 1. Backend (JSON files)

**File:** `modules/Volo.Abp.AuditLogging/src/Volo.Abp.AuditLogging.Domain.Shared/Volo/Abp/AuditLogging/Localization/en.json`

```json
{
  "culture": "en",
  "texts": {
    "YourNewKey": "Your translation here"
  }
}
```

**File:** `modules/Volo.Abp.AuditLogging/src/Volo.Abp.AuditLogging.Domain.Shared/Volo/Abp/AuditLogging/Localization/vi.json`

```json
{
  "culture": "vi",
  "texts": {
    "YourNewKey": "Bản dịch tiếng Việt"
  }
}
```

### 2. Frontend (Sử dụng)

```html
{{ 'AbpAuditLogging::YourNewKey' | translate }}
```

hoặc

```typescript
this.localizationService.localize('AbpAuditLogging::YourNewKey')
```

### 3. Rebuild Backend

```bash
cd src/iOne.HttpApi.Host
dotnet build
```

## 🌍 Ngôn ngữ hỗ trợ

ABP AuditLogging module hỗ trợ 27 ngôn ngữ:

- 🇻🇳 Tiếng Việt (vi)
- 🇬🇧 English (en)
- 🇨🇳 简体中文 (zh-Hans)
- 🇹🇼 繁體中文 (zh-Hant)
- 🇹🇷 Türkçe (tr)
- 🇸🇪 Svenska (sv)
- 🇸🇮 Slovenščina (sl)
- 🇸🇰 Slovenčina (sk)
- 🇷🇺 Русский (ru)
- 🇷🇴 Română (ro-RO)
- 🇧🇷 Português (pt-BR)
- 🇵🇱 Polski (pl-PL)
- 🇳🇱 Nederlands (nl)
- 🇮🇹 Italiano (it)
- 🇮🇸 Íslenska (is)
- 🇭🇺 Magyar (hu)
- 🇭🇷 Hrvatski (hr)
- 🇮🇳 हिन्दी (hi)
- 🇫🇷 Français (fr)
- 🇫🇮 Suomi (fi)
- 🇪🇸 Español (es)
- 🇩🇪 Deutsch (de, de-DE)
- 🇨🇿 Čeština (cs)
- 🇸🇦 العربية (ar)

## 🔄 Đổi ngôn ngữ

### Trong UI

Sử dụng language selector component (thường ở header/menu)

### Programmatically

```typescript
// Thông qua ConfigStateService
this.configState.refreshAppState().subscribe();
```

### Lưu preference

Language preference được lưu tự động khi user chọn ngôn ngữ.

## 🧪 Testing Localization

### Test với Tiếng Việt

1. Chọn ngôn ngữ "Tiếng Việt" trong language selector
2. Kiểm tra:
   - ✅ Panel headers hiển thị tiếng Việt
   - ✅ Labels hiển thị tiếng Việt
   - ✅ Buttons hiển thị tiếng Việt
   - ✅ Placeholders hiển thị tiếng Việt
   - ✅ Table columns hiển thị tiếng Việt
   - ✅ Dialog/Tabs hiển thị tiếng Việt

### Test với English

1. Chọn ngôn ngữ "English" trong language selector
2. Kiểm tra tất cả UI elements hiển thị English

### Test missing keys

Nếu key không tồn tại, sẽ hiển thị key name (e.g., `AbpAuditLogging::MissingKey`)

## 📚 Best Practices

### ✅ Nên làm

1. **Luôn sử dụng localization keys**
   ```html
   <!-- Good -->
   <label>{{ 'AbpAuditLogging::UserName' | translate }}</label>
   
   <!-- Bad -->
   <label>Tên người dùng</label>
   ```

2. **Sử dụng resource name đầy đủ**
   ```typescript
   // Good
   'AbpAuditLogging::Search'
   
   // Bad (chỉ dùng khi có default resource)
   'Search'
   ```

3. **Thêm keys vào TẤT CẢ ngôn ngữ**
   - Ít nhất: en.json và vi.json
   - Tốt nhất: Tất cả language files

4. **Test với nhiều ngôn ngữ**
   - Test ít nhất 2 ngôn ngữ (en, vi)
   - Kiểm tra UI không bị vỡ layout

### ❌ Không nên

1. **Hard-code text**
   ```html
   <!-- Bad -->
   <button>Tìm kiếm</button>
   ```

2. **Mix languages**
   ```html
   <!-- Bad -->
   <label>User name: {{ userName }}</label>
   ```

3. **Quên defaultValue**
   ```typescript
   // Nếu key có thể missing, cung cấp defaultValue
   this.localizationService.localize('Key', 'Default Text')
   ```

## 🐛 Troubleshooting

### Key không hiển thị translation

**Nguyên nhân:**
- Key không tồn tại trong JSON
- Resource name sai
- Backend chưa rebuild

**Fix:**
1. Kiểm tra key trong `en.json` và `vi.json`
2. Verify resource name: `AbpAuditLogging`
3. Rebuild backend
4. Refresh browser (Ctrl+F5)

### UI vỡ layout khi đổi ngôn ngữ

**Nguyên nhân:**
- Text dài hơn expected
- Fixed width không đủ

**Fix:**
- Sử dụng responsive width
- Test với text dài nhất
- Dùng ellipsis cho text quá dài

---

**Version:** 1.3.0  
**Last Updated:** 2024-12-22  
**Localization:** ✅ Fully Integrated

