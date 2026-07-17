# Tra cứu tác động (Audit Logs)

## Tổng quan
Module tra cứu và xem chi tiết các audit logs của hệ thống.

## 🔐 Permissions

### Permission Requirements

| UI Element | Required Permission |
|-----------|---------------------|
| Truy cập trang | `AbpAuditLogging.AuditLogs` |
| Menu item | `AbpAuditLogging.AuditLogs` |
| Button Tìm kiếm/Đặt lại | `AbpAuditLogging.AuditLogs.View` |
| Action "Xem chi tiết" | `AbpAuditLogging.AuditLogs.ViewDetails` |
| Tab "Thay đổi thực thể" | `AbpAuditLogging.AuditLogs.ViewEntityChanges` |
| Tab "Actions" | `AbpAuditLogging.AuditLogs.ViewDetails` |

📖 **Chi tiết:** Xem [PERMISSIONS.md](./PERMISSIONS.md)

## 🌐 Localization (Đa ngôn ngữ)

Component hỗ trợ đa ngôn ngữ sử dụng ABP Localization system.

### Ngôn ngữ hỗ trợ
- ✅ Tiếng Việt (vi)
- ✅ English (en)
- ✅ 25+ ngôn ngữ khác từ ABP Framework

### Resource Name
`AbpAuditLogging` - Tất cả localization keys sử dụng resource này

### Cách sử dụng
```html
<!-- Trong template -->
{{ 'AbpAuditLogging::Search' | translate }}
{{ 'AbpAuditLogging::UserName' | translate }}

<!-- Trong component -->
this.localizationService.localize('AbpAuditLogging::Search')
```

### Thêm/Sửa translations
Backend: `modules/Volo.Abp.AuditLogging/src/Volo.Abp.AuditLogging.Domain.Shared/Volo/Abp/AuditLogging/Localization/`
- `en.json` - English
- `vi.json` - Tiếng Việt

## Tính năng

### 🔍 Tìm kiếm
- Lọc theo thời gian (từ ngày - đến ngày)
- Lọc theo HTTP Method (GET, POST, PUT, DELETE, etc.)
- Lọc theo HTTP Status Code
- Tìm kiếm theo URL, User, IP, Correlation ID
- Lọc theo thời gian thực thi (min/max ms)
- Lọc các request có lỗi

### 📊 Bảng dữ liệu
- Hiển thị: User, Method, URL, Status, Time, Duration, IP, Application
- Phân trang server-side
- Sắp xếp theo cột
- Tùy chỉnh hiển thị cột

### 👁️ Xem chi tiết
Dialog với 3 tabs:
- **Thông tin chung**: ID, User, Time, Status, URL, IP, Browser, Exceptions
- **Thay đổi thực thể**: Entity changes với property changes
- **Actions**: Service calls với parameters

## Cấu trúc Files

```
audit-logs/
├── audit-logs.component.ts      # Component logic với permission checks
├── audit-logs.component.html    # Template với permission directives
├── audit-logs.component.scss    # Styles
├── audit-logs.models.ts         # Types & Interfaces
├── index.ts                     # Barrel export
├── README.md                    # This file
└── PERMISSIONS.md               # Chi tiết permission system
```

## Sử dụng

### Import
```typescript
import { AuditLogsComponent } from '@/pages/audit-logs';
```

### Route
```typescript
{ path: 'audit-logs', component: AuditLogsComponent }
```

### Access
```
http://localhost:4200/pages/audit-logs
```

## Lưu ý kỹ thuật

### API Response Format
API trả về **camelCase** (userName, httpMethod, url...) nên:
- Column fields sử dụng camelCase
- Template bindings sử dụng camelCase
- Component sử dụng `any` type thay vì `AuditLogDto` (do DTO định nghĩa PascalCase)

### Backend
- Endpoint: `/api/audit-logging/audit-logs`
- Service: `AuditLogService`
- Module: `Volo.Abp.AuditLogging`

## Tech Stack
- Angular 20
- PrimeNG 20
- TypeScript 5.8
- Tailwind CSS
- ABP Framework (Permission system + Localization)
