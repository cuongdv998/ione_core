# Quy Tắc Chung Khi Tạo Frontend

## 1. Routing và Navigation

### 1.1. Route Configuration

**Location**: `angular/src/app/pages/pages.routes.ts`

**Quy tắc**:
- Path: `"{module-name}/{entity-name-plural}"` (ví dụ: `"hr/department-types"`)
- `requiredPermission`: Sử dụng `GroupName` từ backend (ví dụ: `"HrDepartmentType"`), **KHÔNG** dùng `"{GroupName}.Default"`
- `breadcrumb`: Format `"{ModuleName}::Menu:{EntityName}"` (ví dụ: `"Hr::Menu:DepartmentTypes"`)

**Ví dụ**:
```typescript
{
    path: 'hr/department-types',
    component: DepartmentTypesComponent,
    canActivate: [permissionGuard],
    data: {
        requiredPermission: 'HrDepartmentType', // ✅ ĐÚNG
        // requiredPermission: 'HrDepartmentType.Default', // ❌ SAI
        breadcrumb: 'Hr::Menu:DepartmentTypes'
    }
}
```

### 1.2. Menu Configuration

**Location**: `angular/src/app/layout/component/app.menu.ts`

**Quy tắc QUAN TRỌNG**:
- **KHÔNG ĐƯỢC** cập nhật file `app.menu.ts` thủ công
- Menu được load tự động từ backend thông qua API endpoint `/api/app/menu`
- Backend đã tự động filter menu theo permissions của user hiện tại
- Menu được định nghĩa trong **backend** thông qua `MenuContributor` (ví dụ: `HrMenuContributor`)
- Location backend: `modules/{ModuleName}/src/iOne.{ModuleName}.Application/Navigation/{ModuleName}MenuContributor.cs`

**Lưu ý**:
- File `app.menu.ts` chỉ chứa logic load menu từ backend, không chứa menu items hardcode
- Nếu cần thêm menu item mới, phải thêm vào **backend MenuContributor**, không phải frontend
- Fallback menu trong `app.menu.ts` chỉ dùng cho development/testing, không nên sử dụng trong production

**Ví dụ Backend MenuContributor**:
```csharp
// Location: modules/hr/src/iOne.Hr.Application/Navigation/HrMenuContributor.cs
hrMenuItem.AddItem(new ApplicationMenuItem(
    "HR.EmployeeLevels",
    hrL["Menu:EmployeeLevels"],
    url: "~/pages/hr/employee-levels",
    icon: "pi pi-fw pi-sort-amount-up"
).RequirePermissions(HrEmployeeLevelPermissions.Default));
```

**Ví dụ Frontend (KHÔNG NÊN LÀM)**:
```typescript
// ❌ SAI: Không được thêm menu items vào app.menu.ts
{
    label: this.localizationService.localize('Hr::Menu:EmployeeLevels'),
    icon: 'pi pi-fw pi-sort-amount-up',
    routerLink: ['/pages/hr/employee-levels'],
    visible: this.permissionService.isGranted('HrEmployeeLevel')
}
```

## 2. Localization

### 2.1. Localization Key Format

**Quy tắc QUAN TRỌNG**:
- Format: `"{ResourceName}::{EntityName}:{Key}"` (ví dụ: `"Hr::HrDepartmentType:Code"`)
- Resource name phải match với `LocalizationResourceName` từ backend
- Sử dụng `::` (double colon) để phân tách resource name và key

**Ví dụ**:
```typescript
// ✅ ĐÚNG
this.localizationService.localize('Hr::HrDepartmentType:Code')
'{{ "Hr::HrDepartmentType:Code" | translate }}'

// ❌ SAI
this.localizationService.localize('HrDepartmentType:Code')
'{{ "HrDepartmentType:Code" | translate }}'
```

### 2.2. Common UI Keys

**Quy tắc**:
- Các key UI chung (All, Success, Error, Cancel, Save, Edit, Delete) nên được thêm vào module localization files
- Hoặc sử dụng `AbpUiResource` như base type (đã config ở backend)
- Nếu dùng `AbpUiResource`, format: `"AbpUi::{Key}"` (ví dụ: `"AbpUi::All"`)
- Nếu không, thêm vào module localization: `"Hr::{Key}"` (ví dụ: `"Hr::All"`)

## 3. Component Structure

### 3.1. Component Files

**Location**: `angular/src/app/pages/{module-name}/{entity-name-plural}/`

**Files**:
- `{entity-name-plural}.component.ts`
- `{entity-name-plural}.component.html`
- `{entity-name-plural}.component.scss`
- `{entity-name-plural}.models.ts`
- `index.ts`

### 3.2. Component Class

**Quy tắc**:
- Inject services: `LocalizationService`, `PermissionService`, Entity Service
- Define permissions constants
- Define form data models
- Implement CRUD operations với error handling

**Ví dụ**:
```typescript
export class DepartmentTypesComponent implements OnInit {
    private readonly permissions = {
        CREATE: 'HrDepartmentType.Create',
        UPDATE: 'HrDepartmentType.Edit',
        DELETE: 'HrDepartmentType.Delete',
        VIEW: 'HrDepartmentType.View'
    };

    formData: DepartmentTypeFormData = {
        code: '',
        name: '',
        status: HrDepartmentTypeStatus.Active
    };

    constructor(
        private departmentTypeService: HrDepartmentTypeService,
        private localizationService: LocalizationService,
        private permissionService: PermissionService
    ) {}
}
```

## 4. Table Component

### 4.1. Table Column Definition

**Quy tắc**:
- Sử dụng `TableColumn` interface từ `shared/models/table-column.model.ts`
- Columns phải có `field`, `header`, `sortable`, `type`, `width`
- Sử dụng `formatter` để format giá trị hiển thị
- Sử dụng `cellClass` để apply dynamic styling
- Sử dụng `freeze` và `align` cho column positioning

**Ví dụ**:
```typescript
{
    field: 'status',
    header: this.localizationService.localize('Hr::HrDepartmentType:Status'),
    sortable: true,
    type: 'text',
    width: '120px',
    freeze: 'right',
    align: 'center',
    formatter: (value: any) => this.formatStatus(value),
    cellClass: (value: any) => {
        const statusValue = typeof value === 'number' ? value : 
            (value === HrDepartmentTypeStatus.Active ? 0 : 1);
        return statusValue === 0
            ? 'px-2 py-1 rounded text-xs font-semibold bg-green-100 text-green-800'
            : 'px-2 py-1 rounded text-xs font-semibold bg-red-100 text-red-800';
    }
}
```

### 4.2. Status Formatting

**Quy tắc**:
- Enum values có thể là number (0, 1) hoặc string ("Active", "Deactive")
- Cần handle cả 2 cases trong formatter function
- Luôn return localized string

**Ví dụ**:
```typescript
formatStatus(status: HrDepartmentTypeStatus | number | string | undefined | null): string {
    if (status === undefined || status === null) {
        return '';
    }

    let statusValue: number;

    if (typeof status === 'number') {
        statusValue = status;
    } else if (status === HrDepartmentTypeStatus.Active || status === 'Active' || status === 'active' || status === '0') {
        statusValue = 0;
    } else if (status === HrDepartmentTypeStatus.Deactive || status === 'Deactive' || status === 'deactive' || status === '1') {
        statusValue = 1;
    } else {
        statusValue = 1; // Default
    }

    return statusValue === 0
        ? this.localizationService.localize('Hr::HrDepartmentType:Active')
        : this.localizationService.localize('Hr::HrDepartmentType:Deactive');
}
```

## 5. Dialog và Form

### 5.1. PrimeNG Dialog

**Quy tắc**:
- Sử dụng `p-dialog` với `[modal]="true"`
- Set `[contentStyle]="{ overflow: 'visible' }"` để tránh scrollbar issues
- Set `[draggable]="false"` và `[resizable]="false"` nếu không cần

**Ví dụ**:
```html
<p-dialog
    [(visible)]="dialogVisible"
    [header]="(dialogMode === 'create' ? 'Hr::HrDepartmentType:New' : 'Hr::HrDepartmentType:Edit') | translate"
    [modal]="true"
    [style]="{ width: '600px' }"
    [contentStyle]="{ overflow: 'visible' }"
    [draggable]="false"
    [resizable]="false"
>
```

### 5.2. PrimeNG Select/Dropdown

**Quy tắc QUAN TRỌNG**:
- **LUÔN** thêm `appendTo="body"` để tránh scrollbar issues trong dialog
- Điều này đảm bảo dropdown render outside dialog và không bị clip

**Ví dụ**:
```html
<p-select
    id="formStatus"
    [(ngModel)]="formData.status"
    [options]="statusOptions"
    [placeholder]="'Hr::HrDepartmentType:Status' | translate"
    optionLabel="label"
    optionValue="value"
    styleClass="w-full"
    appendTo="body"
/>
```

### 5.3. Form Validation

**Quy tắc**:
- Validate ở cả client-side (HTML attributes) và component logic
- Hiển thị error messages với localization keys
- Disable submit button khi form invalid

## 6. Styling

### 6.1. Component SCSS

**Quy tắc**:
- Sử dụng `:host ::ng-deep` để style PrimeNG components
- Set `z-index` cao cho dropdown overlays trong dialog
- Set `overflow: visible` cho dialog content

**Ví dụ**:
```scss
:host ::ng-deep {
    .p-dialog-content {
        overflow: visible;
    }
    .p-select-overlay {
        z-index: 10000 !important; // Ensure dropdown is above dialog
    }
}
```

## 7. Breadcrumb

### 7.1. Breadcrumb Logic

**Location**: `angular/src/app/layout/component/app.breadcrumb.ts`

**Quy tắc**:
- Bỏ qua segment `pages` cho HR routes (hoặc các routes đặc biệt)
- Thêm breadcrumb cho parent segments (ví dụ: "Nhân sự" cho `hr`)
- Xử lý nested routes đúng cách

**Ví dụ**:
```typescript
// Check if current route is an HR route
const isHrRoute = this.router.url.includes('/hr/') || this.router.url === '/pages/hr';

// Skip "pages" breadcrumb for HR routes
if ((routeUrl === 'pages' || routeSnapshot.routeConfig?.path === 'pages') && 
    routeData['breadcrumb'] === 'iOne::Menu:Administration') {
    if (!isHrRoute && !pagesBreadcrumbAdded) {
        // Add "Quản trị" breadcrumb
    }
}
```

### 7.2. Path Mappings

**Quy tắc**:
- Map các path segments đặc biệt đến localization keys
- Đảm bảo breadcrumb hiển thị đúng label

**Ví dụ**:
```typescript
const pathMappings: Record<string, string> = {
    'hr': 'Hr::Menu:HR',
    'audit-logs': 'iOne::Menu:AuditLogs',
    'roles': 'iOne::Menu:Roles',
    'users': 'iOne::Menu:Users'
};
```

## 8. Error Handling

### 8.1. API Error Handling

**Quy tắc**:
- Sử dụng try-catch cho tất cả API calls
- Hiển thị error messages với localization
- Log errors để debug

**Ví dụ**:
```typescript
try {
    const result = await this.departmentTypeService.create(input).toPromise();
    this.messageService.add({
        severity: 'success',
        summary: this.localizationService.localize('Hr::Success'),
        detail: this.localizationService.localize('Hr::HrDepartmentType:CreatedSuccessfully')
    });
} catch (error) {
    this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Hr::Error'),
        detail: this.localizationService.localize('Hr::InternalServerErrorMessage')
    });
}
```

## 9. Debug và Logging

### 9.1. Console Logs

**Quy tắc**:
- **XÓA TẤT CẢ** console.log trước khi commit
- Chỉ giữ lại logs cần thiết cho production (nếu có)
- Sử dụng proper logging service nếu cần

## 10. Checklist Trước Khi Hoàn Thành

- [ ] Route đã được config đúng với permission
- [ ] **Menu item đã được thêm vào backend MenuContributor (KHÔNG thêm vào app.menu.ts)**
- [ ] Tất cả localization keys đã sử dụng format đúng (`ResourceName::Key`)
- [ ] Table columns đã có formatter và cellClass nếu cần
- [ ] Dialog đã có `contentStyle` và `appendTo="body"` cho selects
- [ ] Form validation đã đầy đủ
- [ ] Error handling đã được implement
- [ ] Breadcrumb hiển thị đúng
- [ ] Tất cả console.log đã được xóa
- [ ] Build và test thành công

## 11. Lưu Ý Quan Trọng

1. **Menu Configuration**: 
   - **QUAN TRỌNG**: **KHÔNG ĐƯỢC** cập nhật file `app.menu.ts` thủ công
   - Menu được load tự động từ backend thông qua API `/api/app/menu`
   - Menu items phải được thêm vào **backend MenuContributor**, không phải frontend
   - Location backend: `modules/{ModuleName}/src/iOne.{ModuleName}.Application/Navigation/{ModuleName}MenuContributor.cs`
2. **Permission Name**: Sử dụng `GroupName` (ví dụ: `"HrDepartmentType"`), không phải `"{GroupName}.Default"`
3. **Localization Format**: Luôn dùng `ResourceName::Key` (double colon)
4. **Dialog Dropdowns**: LUÔN thêm `appendTo="body"` để tránh scrollbar issues
5. **Table Formatting**: Sử dụng `formatter` và `cellClass` cho dynamic content
6. **Breadcrumb**: Xử lý đúng nested routes và skip segments không cần thiết
7. **Console Logs**: Xóa tất cả trước khi commit

