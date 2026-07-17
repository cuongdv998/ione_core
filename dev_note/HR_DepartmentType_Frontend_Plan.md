# Plan: Frontend - Danh mục Loại đơn vị (HR Department Type)

## 📋 Tổng quan

Tạo frontend module quản lý "Danh mục Loại đơn vị" (HR Department Type) trong Angular application, thuộc module HR.

## 🎯 Yêu cầu chức năng

### 1. Danh sách (List)
- ✅ Hiển thị bảng dữ liệu với các cột: Code, Name, Status, CreationTime
- ✅ Server-side pagination
- ✅ Column sorting (theo Code, Name, Status, CreationTime)
- ✅ Tìm kiếm riêng biệt:
  - **Tìm theo Code**: Input riêng, tìm tương đối (contains), không phân biệt hoa thường
  - **Tìm theo Name**: Input riêng, tìm tương đối (contains), không phân biệt hoa thường
- ✅ Filter theo Status (Active/Deactive)
- ✅ Actions: View, Edit, Delete (theo permissions)

### 2. Tạo mới (Create)
- ✅ Dialog form với các trường:
  - **Code**: Text input, required, max 50 chars
  - **Validation**: Chỉ cho phép A-Z, 0-9, và `_` (uppercase)
  - **Name**: Text input, required, max 250 chars
  - **Status**: Dropdown (Active/Deactive), required
- ✅ Validate code format trước khi submit
- ✅ Kiểm tra code trùng lặp (từ backend error)
- ✅ Hiển thị thông báo lỗi từ backend

### 3. Chỉnh sửa (Edit)
- ✅ Dialog form với các trường:
  - **Code**: Read-only (không cho phép sửa)
  - **Name**: Text input, required, max 250 chars
  - **Status**: Dropdown (Active/Deactive), required
- ✅ Load dữ liệu hiện tại vào form
- ✅ Validate trước khi submit

### 4. Xóa (Delete)
- ✅ Confirmation dialog trước khi xóa
- ✅ Hiển thị thông báo thành công/lỗi

## 🔐 Permissions

Sử dụng permissions từ HR module:
- `HrDepartmentType.Default` - Truy cập trang
- `HrDepartmentType.Create` - Tạo mới
- `HrDepartmentType.Edit` - Chỉnh sửa
- `HrDepartmentType.Delete` - Xóa
- `HrDepartmentType.View` - Xem chi tiết

## 🌐 Localization

Sử dụng localization resource: `Hr`

Các keys cần sử dụng:
- `HrDepartmentType:Code` - "Mã loại đơn vị"
- `HrDepartmentType:Name` - "Tên loại đơn vị"
- `HrDepartmentType:Status` - "Trạng thái"
- `HrDepartmentType:CodeRequired` - "Mã loại đơn vị là bắt buộc"
- `HrDepartmentType:CodeMaxLength` - "Mã loại đơn vị không được vượt quá 50 ký tự"
- `HrDepartmentType:CodeInvalidFormat` - "Mã chỉ được phép chứa chữ in hoa (A-Z), số (0-9) và dấu gạch dưới (_)"
- `HrDepartmentType:NameRequired` - "Tên loại đơn vị là bắt buộc"
- `HrDepartmentType:NameMaxLength` - "Tên loại đơn vị không được vượt quá 250 ký tự"
- `HrDepartmentType:StatusRequired` - "Trạng thái là bắt buộc"
- `HrDepartmentType:CodeExists` - "Mã loại đơn vị đã tồn tại"
- `HrDepartmentType:Active` - "Hoạt động"
- `HrDepartmentType:Deactive` - "Không hoạt động"
- `HrDepartmentType:New` - "Thêm mới loại đơn vị"
- `HrDepartmentType:Edit` - "Chỉnh sửa loại đơn vị"
- `HrDepartmentType:Delete` - "Xóa loại đơn vị"
- `HrDepartmentType:DeleteConfirm` - "Bạn có chắc chắn muốn xóa loại đơn vị này?"
- `HrDepartmentType:CreatedSuccessfully` - "Tạo loại đơn vị thành công"
- `HrDepartmentType:UpdatedSuccessfully` - "Cập nhật loại đơn vị thành công"
- `HrDepartmentType:DeletedSuccessfully` - "Xóa loại đơn vị thành công"

## 📁 Cấu trúc Files

```
angular/src/app/pages/hr/
└── department-types/
    ├── department-types.component.ts       # Component logic
    ├── department-types.component.html     # Template
    ├── department-types.component.scss     # Styles
    ├── department-types.models.ts          # Type definitions
    ├── index.ts                            # Barrel export
    ├── README.md                            # Documentation
    ├── PERMISSIONS.md                       # Permission documentation
    └── LOCALIZATION.md                      # Localization documentation
```

## 🔧 Tech Stack

- **Angular 20** - Framework
- **PrimeNG 20** - UI Components
- **TypeScript 5.8** - Language
- **Tailwind CSS** - Styling
- **ABP Framework** - Permission system + Localization
- **RxJS** - Reactive programming

## 📝 Chi tiết Implementation

### 1. Component TypeScript (`department-types.component.ts`)

#### Imports
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TableLazyLoadEvent } from 'primeng/table';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';
import { HrDepartmentTypeService } from '@/proxy/hr/controllers/hr-department-type.service';
import { HrDepartmentTypeDto, CreateHrDepartmentTypeDto, UpdateHrDepartmentTypeDto } from '@/proxy/hr/hr-department-types/models';
import { HrDepartmentTypeStatus } from '@/proxy/hr-department-types/hr-department-type-status.enum';
import { DepartmentTypeSearchForm, DepartmentTypeFormData } from './department-types.models';
import { PermissionPipe } from '@/core/pipes/permission.pipe';
import { PermissionService } from '@/core/services/permission.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
```

#### Properties
```typescript
// Permissions
readonly PERMISSIONS = {
  CREATE: 'HrDepartmentType.Create',
  UPDATE: 'HrDepartmentType.Edit',
  DELETE: 'HrDepartmentType.Delete',
  VIEW: 'HrDepartmentType.View'
};

// Data
departmentTypes: HrDepartmentTypeDto[] = [];
totalCount = 0;
loading = true;
pageSize = 10;

// Dialog
dialogVisible = false;
dialogMode: 'create' | 'edit' = 'create';
formData: DepartmentTypeFormData = this.getEmptyForm();
selectedDepartmentType?: HrDepartmentTypeDto;

// Search form
searchForm: DepartmentTypeSearchForm = {
  code: null,
  name: null,
  status: null
};

// Status options
statusOptions = [
  { label: 'HrDepartmentType:Active', value: HrDepartmentTypeStatus.Active },
  { label: 'HrDepartmentType:Deactive', value: HrDepartmentTypeStatus.Deactive }
];

// Table
columns: TableColumn[] = [];
actions: TableAction<HrDepartmentTypeDto>[] = [];
currentLazyLoadEvent?: TableLazyLoadEvent;
```

#### Methods
- `ngOnInit()`: Initialize columns, actions, load data
- `initializeColumns()`: Setup table columns
- `initializeActions()`: Setup table actions
- `loadData(event?: TableLazyLoadEvent)`: Load paginated data
  - Gửi `code` và `name` trong request parameters (riêng biệt)
  - Backend sẽ xử lý tìm kiếm tương đối (contains) và không phân biệt hoa thường (case-insensitive)
  - Có thể tìm theo Code, theo Name, hoặc cả hai cùng lúc
- `search()`: Apply search filters (reset to first page, reload data)
- `resetSearch()`: Reset search form (clear code, name, status)
- `openCreateDialog()`: Open create dialog
- `openEditDialog(departmentType: HrDepartmentTypeDto)`: Open edit dialog
- `save()`: Save (create/update)
- `delete(departmentType: HrDepartmentTypeDto)`: Delete with confirmation
- `validateCode(code: string)`: Validate code format (A-Z, 0-9, _)
- `getEmptyForm()`: Get empty form data
- `formatStatus(status: HrDepartmentTypeStatus)`: Format status for display

### 2. Template HTML (`department-types.component.html`)

#### Structure
- **Search Panel**: 
  - Input "Tìm theo Code" (riêng biệt)
  - Input "Tìm theo Name" (riêng biệt)
  - Dropdown "Filter theo Status"
  - Reset/Search buttons
  - Create button
- **Data Panel**: VTable component with columns, actions, pagination
- **Create/Edit Dialog**: Form with Code (read-only in edit), Name, Status fields
- **Delete Confirmation Dialog**: PrimeNG ConfirmDialog

#### Key Features
- Use `TranslatePipe` for all text
- Use `PermissionPipe` for permission checks
- Form validation with error messages
- Code input with uppercase conversion and format validation
- Status dropdown with localization

### 3. Models (`department-types.models.ts`)

export interface DepartmentTypeSearchForm {
  code: string | null;
  name: string | null;
  status: HrDepartmentTypeStatus | null;
}

export interface DepartmentTypeFormData {
  code: string;
  name: string;
  status: HrDepartmentTypeStatus;
}
```

### 4. Routes (`pages.routes.ts`)

```typescript
{
  path: 'hr/department-types',
  component: HrDepartmentTypesComponent,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'HrDepartmentType.Default',
    breadcrumb: 'Hr::Menu:DepartmentTypes'
  }
}
```

### 5. Table Columns

- **Code**: Text, sortable
- **Name**: Text, sortable
- **Status**: Badge (Active/Deactive), sortable
- **CreationTime**: Date, sortable

### 6. Table Actions

- **Edit**: Icon button, requires `HrDepartmentType.Edit` permission
- **Delete**: Icon button, requires `HrDepartmentType.Delete` permission

## ✅ Validation Rules

### Code Field
- Required
- Max length: 50 characters
- Format: Only A-Z, 0-9, and `_` (uppercase)
- Real-time validation on input
- Check uniqueness (from backend error)

### Name Field
- Required
- Max length: 250 characters

### Status Field
- Required
- Options: Active, Deactive

## 🎨 UI/UX Features

1. **Loading States**: Show loading indicator during API calls
2. **Error Handling**: Display user-friendly error messages
3. **Success Messages**: Toast notifications for successful operations
4. **Confirmation Dialogs**: Confirm before delete
5. **Form Validation**: Real-time validation with error messages
6. **Responsive Design**: Mobile-friendly layout
7. **Accessibility**: Proper labels and ARIA attributes

## 📊 API Integration

### Service Methods
- `getList(input: PagedAndSortedResultRequestDto)`: Get paginated list
- `get(id: string)`: Get single item
- `create(input: CreateHrDepartmentTypeDto)`: Create new
- `update(id: string, input: UpdateHrDepartmentTypeDto)`: Update existing
- `delete(id: string)`: Delete item

### Error Handling
- Handle validation errors from backend
- Display localized error messages
- Handle network errors gracefully

## 🔄 State Management

- Use component state for:
  - Data list
  - Loading states
  - Dialog visibility
  - Form data
  - Search filters

## 📚 Documentation Files

### README.md
- Overview
- Features
- Usage
- API endpoints

### PERMISSIONS.md
- Required permissions
- Permission checks in component

### LOCALIZATION.md
- Localization keys used
- How to add new translations

## 🚀 Implementation Steps

1. ✅ Create folder structure
2. ✅ Create models file
3. ✅ Create component TypeScript file
4. ✅ Create component HTML template
5. ✅ Create component SCSS file
6. ✅ Create index.ts barrel export
7. ✅ Add route to pages.routes.ts
8. ✅ Test all CRUD operations
9. ✅ Test permissions
10. ✅ Test localization
11. ✅ Test validation
12. ✅ Create documentation files

## 🧪 Testing Checklist

- [ ] Load list with pagination
- [ ] Search by Code (tương đối, không phân biệt hoa thường)
- [ ] Search by Name (tương đối, không phân biệt hoa thường)
- [ ] Search by both Code and Name (kết hợp)
- [ ] Filter by Status
- [ ] Sort columns
- [ ] Create new item (valid data)
- [ ] Create new item (invalid code format)
- [ ] Create new item (duplicate code)
- [ ] Edit item (change Name and Status)
- [ ] Edit item (Code is read-only)
- [ ] Delete item with confirmation
- [ ] Permission checks (hide buttons if no permission)
- [ ] Localization (all text translated)
- [ ] Error handling (network errors, validation errors)
- [ ] Responsive design

## 📝 Notes

- Follow existing patterns from `roles` and `users` components
- Use VTable component for consistent table UI
- Use PrimeNG components for dialogs and forms
- All text must be localized using `Hr` resource
- All actions must check permissions
- Code validation must be done on frontend before submit
- Backend will also validate, so handle backend errors gracefully
- **Search Implementation**:
  - Code và Name là 2 input riêng biệt trong search form
  - Tìm kiếm tương đối (contains) và không phân biệt hoa thường (case-insensitive)
  - Backend cần hỗ trợ filter parameters cho `code` và `name` riêng biệt
  - Frontend gửi cả 2 parameters trong query string khi search
  - Có thể tìm theo Code, theo Name, hoặc cả hai cùng lúc
  - Khi reset search, clear cả 2 trường Code và Name

