# Script Generate Missing DTOs - Tài liệu

## Tổng quan

Script `generate-missing-dtos.js` tự động generate các DTOs còn thiếu từ `generate-proxy.json` sau khi chạy `abp generate-proxy`. Script này xử lý:

1. **Tự động phát hiện DTOs còn thiếu** từ return types và parameters
2. **Tự động xác định module path** dựa trên namespace
3. **Generate TypeScript interfaces** với đúng naming convention (camelCase)
4. **Tự động merge imports** để tránh duplicate
5. **Xử lý các base types** phổ biến trong ABP

## Các tính năng chính

### 1. Base Types được hỗ trợ

#### EntityDto Types:
- ✅ `EntityDto<TKey>` → `EntityDto<string>`
- ✅ `FullAuditedEntityDto<TKey>` → `FullAuditedEntityDto<string>`
- ✅ `AuditedEntityDto<TKey>` → `AuditedEntityDto<string>`
- ✅ `CreationAuditedEntityDto<TKey>` → `CreationAuditedEntityDto<string>`
- ✅ `ExtensibleFullAuditedEntityDto<TKey>` → `FullAuditedEntityDto<string>` (mapped to non-Extensible)
- ✅ `ExtensibleAuditedEntityDto<TKey>` → `AuditedEntityDto<string>` (mapped to non-Extensible)
- ✅ `ExtensibleCreationAuditedEntityDto<TKey>` → `CreationAuditedEntityDto<string>` (mapped to non-Extensible)
- ✅ `ExtensibleEntityDto<TKey>` → `EntityDto<string>` (mapped to non-Extensible)

#### Request DTO Types:
- ✅ `PagedAndSortedResultRequestDto`
- ✅ `ExtensiblePagedAndSortedResultRequestDto` → `PagedAndSortedResultRequestDto`
- ✅ `PagedResultRequestDto`
- ✅ `ExtensiblePagedResultRequestDto` → `PagedResultRequestDto`
- ✅ `SortedResultRequestDto`
- ✅ `ExtensibleSortedResultRequestDto` → `SortedResultRequestDto`
- ✅ `LimitedResultRequestDto`
- ✅ `ExtensibleLimitedResultRequestDto` → `LimitedResultRequestDto`

#### ExtensibleObject:
- ✅ `ExtensibleObject` - Không extend trong TypeScript, nhưng tự động thêm `extraProperties?: Record<string, any>`
- ✅ Tất cả Extensible variants (ExtensibleEntityDto, ExtensibleFullAuditedEntityDto, etc.) tự động thêm `extraProperties`

### 2. Type Mapping

#### System Types:
- ✅ `string` / `string?` → `string`
- ✅ `number` / `number?` → `number` (Int32, Int64, Double, Decimal, etc.)
- ✅ `boolean` / `boolean?` → `boolean`
- ✅ `DateTime` / `DateTime?` → `string`
- ✅ `Guid` / `Guid?` → `string`

#### Complex Types:
- ✅ Arrays: `[System.String]` → `string[]`, `[DtoType]` → `DtoType[]`
- ✅ Dictionaries: `{System.String:System.String}` → `Record<string, any>`
- ✅ Generic types: `PagedResultDto<AuditLogDto>` → Extracts `AuditLogDto`
- ✅ Nested generics: Recursively extracts inner types

#### Enum Types:
- ✅ Regular enums: `EntityChangeType` → `EntityChangeType` (with import)
- ✅ Special enums:
  - `System.Net.HttpStatusCode` → `number`
  - `LoginResultType` → `number` (if enum file doesn't exist) or `LoginResultType` (if exists)

### 3. Property Naming

- ✅ **PascalCase → camelCase**: `Name` → `name`, `IsActive` → `isActive`
- ✅ **jsonName support**: Sử dụng `jsonName` nếu có (thường đã là camelCase)
- ✅ **Nullable handling**: `isRequired: false` → property có `?` marker

### 4. Module Path Resolution

Script tự động xác định module path dựa trên:

1. **Namespace patterns** (ưu tiên):
   - `Volo.Abp.AuditLogging` → `volo/abp/audit-logging`
   - `Volo.Abp.Identity` → `identity` (nếu có custom rootPath) hoặc `volo/abp/identity`
   - `Volo.Abp.PermissionManagement` → `permissionManagement` (nếu có custom rootPath) hoặc `volo/abp/permission-management`
   - `Volo.Abp.Ui.Navigation` → `volo/abp/ui/navigation`
   - `Volo.Abp.Auditing` → `volo/abp/auditing`
   - `iOne.Hr.*` → `hr/*` (với subfolder nếu có)

2. **Controller-based matching**: Tìm module chứa controller sử dụng type này

3. **Fallback patterns**: Các namespace patterns khác

### 5. Import Management

- ✅ **Auto-merge @abp/ng.core imports**: Tất cả imports từ `@abp/ng.core` được merge thành một dòng
- ✅ **Auto-merge enum imports**: Các enum imports từ cùng một path được merge
- ✅ **Remove duplicates**: Tự động loại bỏ duplicate imports
- ✅ **Sort imports**: Imports được sắp xếp alphabetically

### 6. Edge Cases được xử lý

- ✅ **Nested generics**: `PagedResultDto<ListResultDto<IdentityRoleDto>>` → Extracts cả 3 DTOs
- ✅ **Dictionary types**: `{System.String:System.String}` → Filtered out (không generate)
- ✅ **System types**: `System.*` → Filtered out
- ✅ **Primitive types**: `string`, `number`, `boolean` → Filtered out
- ✅ **Generic wrapper types**: `PagedResultDto`, `ListResultDto` → Không generate (chỉ extract inner types)
- ✅ **Enum nullable**: `enum?` → Xử lý đúng (nullable ở property, không ở type)
- ✅ **Missing enum files**: Fallback to `number` nếu enum file không tồn tại

## Các trường hợp chưa được xử lý

### 1. ExtensibleObject
- ✅ **Đã xử lý**: Tự động thêm `extraProperties?: Record<string, any>` cho các DTOs có baseType là ExtensibleObject hoặc Extensible variants
- ✅ **Kiểm tra**: Script tự động kiểm tra xem property `extraProperties` đã tồn tại chưa trước khi thêm

### 2. Custom base types
- **Vấn đề**: Các base types tùy chỉnh không có trong danh sách
- **Giải pháp**: Cần thêm vào script nếu gặp

### 3. Union types
- **Vấn đề**: TypeScript union types (`string | number`)
- **Giải pháp**: Chưa được xử lý (hiếm gặp trong ABP DTOs)

## Cách sử dụng

```bash
# Sau khi generate proxy
npm run fix-proxy-dtos

# Hoặc chạy trực tiếp
node scripts/generate-missing-dtos.js
```

## Lưu ý

1. **Không sửa file models.ts thủ công**: Các file này sẽ bị overwrite khi chạy lại script
2. **Chạy sau khi generate proxy**: Script cần `generate-proxy.json` mới nhất
3. **Service files**: Script không generate service files, cần chạy `generate-missing-services.js` riêng

## Cải thiện trong tương lai

- [ ] Hỗ trợ ExtensibleObject với extraProperties
- [ ] Hỗ trợ union types
- [ ] Hỗ trợ custom base types từ config
- [ ] Better error handling và logging
- [ ] Support for type aliases

