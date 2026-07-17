# V-Table Component

Component table tùy biến dựa trên PrimeNG Table, được tối ưu hóa cho ABP Framework với các tính năng nâng cao.

## 📋 Mục lục

- [Tính năng](#tính-năng)
- [Cài đặt](#cài-đặt)
- [Sử dụng cơ bản](#sử-dụng-cơ-bản)
- [API Reference](#api-reference)
- [Ví dụ nâng cao](#ví-dụ-nâng-cao)
- [Tích hợp với ABP](#tích-hợp-với-abp)

## ✨ Tính năng

- ✅ **Lazy Loading** - Tải dữ liệu theo trang
- ✅ **Pagination** - Phân trang với tùy chọn số dòng (10, 20, 50, 100)
- ✅ **Sorting** - Sắp xếp theo cột
- ✅ **Column Toggle** - Ẩn/hiện cột tùy chọn
- ✅ **Column Resize** - Kéo thả thay đổi độ rộng cột
- ✅ **Frozen Columns** - Cố định cột trái/phải
- ✅ **Scrollable** - Cuộn ngang/dọc
- ✅ **Row Actions** - Menu hành động cho từng dòng
- ✅ **Auto Index** - Cột STT tự động
- ✅ **Responsive** - Tương thích mobile
- ✅ **Custom Styling** - Giao diện hiện đại

## 🚀 Cài đặt

### 1. Import Component

```typescript
import { VTable } from '@/shared/components/v-table/v-table';
import { TableColumn } from '@/shared/models/table-column.model';
import { TableAction } from '@/shared/models/table-action.model';

@Component({
  selector: 'app-your-component',
  imports: [VTable],
  // ...
})
export class YourComponent {
  // ...
}
```

### 2. Định nghĩa Models

#### TableColumn Model

```typescript
export interface TableColumn {
  field: string;           // Tên field từ data
  header: string;          // Tiêu đề cột
  width?: string;          // Độ rộng (vd: '150px', '20%')
  type?: 'text' | 'number' | 'date';  // Kiểu dữ liệu
  sortable?: boolean;      // Cho phép sort
  freeze?: 'left' | 'right';  // Cố định cột
  align?: 'left' | 'center' | 'right';  // Căn chỉnh
}
```

#### TableAction Model

```typescript
export interface TableAction {
  label: string;           // Nhãn hiển thị
  icon?: string;           // Icon PI (vd: 'pi pi-pencil')
  command: (row: any) => void;  // Hàm xử lý khi click
  visible?: (row: any) => boolean;  // Điều kiện hiển thị
  disabled?: (row: any) => boolean; // Điều kiện disable
}
```

## 📖 Sử dụng cơ bản

### 1. Component Template

```html
<app-v-table
  [columns]="columns"
  [data]="data"
  [totalCount]="totalCount"
  [loading]="loading"
  [rows]="pageSize"
  [actions]="actions"
  (lazyLoad)="loadData($event)">
</app-v-table>
```

### 2. Component TypeScript

```typescript
import { Component, OnInit } from '@angular/core';
import { TableLazyLoadEvent } from 'primeng/table';
import { PagedResultDto } from '@abp/ng.core';
import { YourService } from './your.service';

@Component({
  selector: 'app-your-list',
  templateUrl: './your-list.component.html'
})
export class YourListComponent implements OnInit {
  // Data
  data: any[] = [];
  totalCount = 0;
  loading = false;
  pageSize = 10;

  // Columns
  columns: TableColumn[] = [
    { 
      field: 'name', 
      header: 'Tên', 
      width: '200px', 
      sortable: true 
    },
    { 
      field: 'email', 
      header: 'Email', 
      width: '250px', 
      sortable: true 
    },
    { 
      field: 'createdDate', 
      header: 'Ngày tạo', 
      width: '150px', 
      type: 'date',
      sortable: true,
      align: 'center'
    },
    { 
      field: 'totalAmount', 
      header: 'Tổng tiền', 
      width: '120px', 
      type: 'number',
      align: 'right'
    }
  ];

  // Actions
  actions: TableAction[] = [
    {
      label: 'Xem chi tiết',
      icon: 'pi pi-eye',
      command: (row) => this.viewDetail(row)
    },
    {
      label: 'Chỉnh sửa',
      icon: 'pi pi-pencil',
      command: (row) => this.edit(row),
      visible: (row) => row.canEdit
    },
    {
      label: 'Xóa',
      icon: 'pi pi-trash',
      command: (row) => this.delete(row),
      visible: (row) => row.canDelete
    }
  ];

  constructor(private yourService: YourService) {}

  ngOnInit() {
    // Data sẽ được load tự động qua event lazyLoad
  }

  loadData(event: TableLazyLoadEvent) {
    this.loading = true;
    
    const input = {
      skipCount: event.first || 0,
      maxResultCount: event.rows || this.pageSize,
      sorting: this.getSorting(event)
    };

    this.yourService.getList(input).subscribe({
      next: (result: PagedResultDto<any>) => {
        this.data = result.items;
        this.totalCount = result.totalCount;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  private getSorting(event: TableLazyLoadEvent): string {
    if (!event.sortField) return '';
    
    const direction = event.sortOrder === 1 ? 'ASC' : 'DESC';
    return `${event.sortField} ${direction}`;
  }

  viewDetail(row: any) {
    // Xử lý xem chi tiết
  }

  edit(row: any) {
    // Xử lý chỉnh sửa
  }

  delete(row: any) {
    // Xử lý xóa
  }
}
```

## 🔧 API Reference

### Inputs

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `columns` | `TableColumn[]` | `[]` | Định nghĩa các cột |
| `data` | `any[]` | `[]` | Dữ liệu hiển thị |
| `actions` | `TableAction[]` | `null` | Menu hành động |
| `totalCount` | `number` | `0` | Tổng số bản ghi |
| `loading` | `boolean` | `true` | Trạng thái loading |
| `rows` | `number` | `10` | Số dòng/trang |
| `rowsPerPageOptions` | `number[]` | `[10, 20, 50, 100]` | Tùy chọn số dòng/trang |
| `showCurrentPageReport` | `boolean` | `true` | Hiển thị thông tin phân trang |
| `currentPageReportTemplate` | `string` | `'Hiển thị {first} đến {last}...'` | Template thông tin phân trang |
| `showGridlines` | `boolean` | `true` | Hiển thị đường kẻ |
| `stripedRows` | `boolean` | `true` | Dòng kẻ sọc |
| `rowHover` | `boolean` | `true` | Hiệu ứng hover |
| `size` | `'small' \| 'normal' \| 'large'` | `'small'` | Kích thước table |
| `scrollable` | `boolean` | `true` | Cho phép cuộn |
| `scrollHeight` | `string` | `undefined` | Chiều cao vùng cuộn |
| `tableMinWidth` | `string` | `'1200px'` | Độ rộng tối thiểu |
| `showIndex` | `boolean` | `true` | Hiển thị cột STT |
| `indexHeader` | `string` | `'STT'` | Tiêu đề cột STT |
| `indexWidth` | `string` | `'60px'` | Độ rộng cột STT |
| `showColumnToggle` | `boolean` | `true` | Hiển thị nút ẩn/hiện cột |
| `columnTogglePlaceholder` | `string` | `'Chọn cột hiển thị'` | Placeholder column toggle |
| `columnToggleTooltip` | `string` | `'Tùy chỉnh hiển thị cột'` | Tooltip column toggle |
| `resizableColumns` | `boolean` | `true` | Cho phép resize cột |
| `columnResizeMode` | `'fit' \| 'expand'` | `'expand'` | Chế độ resize |
| `actionLabelKey` | `string` | `'iOne::Actions'` | Localization key cho nhãn action |

### Outputs

| Event | Payload | Description |
|-------|---------|-------------|
| `lazyLoad` | `TableLazyLoadEvent` | Sự kiện load dữ liệu (sort, page, filter) |

### TableLazyLoadEvent

```typescript
interface TableLazyLoadEvent {
  first?: number;        // Index bản ghi đầu tiên
  rows?: number;         // Số bản ghi/trang
  sortField?: string;    // Field được sort
  sortOrder?: number;    // 1: ASC, -1: DESC
}
```

## 🎯 Ví dụ nâng cao

### 1. Frozen Columns (Cố định cột)

```typescript
columns: TableColumn[] = [
  { 
    field: 'id', 
    header: 'ID', 
    width: '80px',
    freeze: 'left'  // Cố định bên trái
  },
  { field: 'name', header: 'Tên', width: '200px' },
  { field: 'description', header: 'Mô tả', width: '300px' },
  { 
    field: 'status', 
    header: 'Trạng thái', 
    width: '120px',
    freeze: 'right'  // Cố định bên phải
  }
];
```

### 2. Custom Column Alignment

```typescript
columns: TableColumn[] = [
  { field: 'name', header: 'Tên', align: 'left' },
  { field: 'quantity', header: 'Số lượng', align: 'center' },
  { field: 'amount', header: 'Thành tiền', align: 'right', type: 'number' }
];
```

### 3. Conditional Actions

```typescript
actions: TableAction[] = [
  {
    label: 'Duyệt',
    icon: 'pi pi-check',
    command: (row) => this.approve(row),
    visible: (row) => row.status === 'Pending',  // Chỉ hiện khi status là Pending
  },
  {
    label: 'Từ chối',
    icon: 'pi pi-times',
    command: (row) => this.reject(row),
    visible: (row) => row.status === 'Pending',
    disabled: (row) => !row.canReject  // Disable nếu không có quyền
  }
];
```

### 4. Scrollable với chiều cao cố định

```html
<app-v-table
  [columns]="columns"
  [data]="data"
  [scrollable]="true"
  scrollHeight="500px"
  (lazyLoad)="loadData($event)">
</app-v-table>
```

### 5. Tắt các tính năng

```html
<app-v-table
  [columns]="columns"
  [data]="data"
  [showIndex]="false"
  [showColumnToggle]="false"
  [resizableColumns]="false"
  [stripedRows]="false"
  (lazyLoad)="loadData($event)">
</app-v-table>
```

### 6. Custom Page Size Options

```html
<app-v-table
  [columns]="columns"
  [data]="data"
  [rows]="25"
  [rowsPerPageOptions]="[25, 50, 100, 200]"
  (lazyLoad)="loadData($event)">
</app-v-table>
```

## 🔗 Tích hợp với ABP

### 1. Sử dụng với ABP Service

```typescript
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';

export class YourListComponent implements OnInit {
  data: any[] = [];
  
  constructor(
    public readonly list: ListService,
    private yourService: YourService,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit() {
    // Hook vào ABP List Service
    const dataStreamCreator = (query) => this.yourService.getList({
      ...query,
      skipCount: query.maxResultCount * query.page,
      maxResultCount: query.maxResultCount
    });

    this.list.hookToQuery(dataStreamCreator).subscribe(
      (response: PagedResultDto<any>) => {
        this.data = response.items;
      }
    );
  }

  loadData(event: TableLazyLoadEvent) {
    this.list.page = Math.floor((event.first || 0) / (event.rows || 10));
    this.list.maxResultCount = event.rows || 10;
    
    if (event.sortField) {
      const direction = event.sortOrder === 1 ? 'ASC' : 'DESC';
      this.list.sortKey = event.sortField;
      this.list.sortOrder = direction;
    }
    
    this.list.get();
  }

  delete(row: any) {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure')
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.yourService.delete(row.id).subscribe(() => {
            this.list.get();
          });
        }
      });
  }
}
```

### 2. Sử dụng với ABP Localization

```typescript
import { LocalizationService } from '@abp/ng.core';

export class YourListComponent {
  columns: TableColumn[] = [];

  constructor(private localization: LocalizationService) {
    this.columns = [
      { 
        field: 'name', 
        header: this.localization.instant('::Name'),
        width: '200px'
      },
      { 
        field: 'email', 
        header: this.localization.instant('::Email'),
        width: '250px'
      }
    ];
  }

  actions: TableAction[] = [
    {
      label: this.localization.instant('::Edit'),
      icon: 'pi pi-pencil',
      command: (row) => this.edit(row)
    }
  ];
}
```

### 3. Sử dụng với ABP Permission

```typescript
import { PermissionService } from '@/core/services/permission.service';

export class YourListComponent {
  actions: TableAction[] = [];

  constructor(private permissionService: PermissionService) {
    this.actions = [
      {
        label: 'Chỉnh sửa',
        icon: 'pi pi-pencil',
        command: (row) => this.edit(row),
        visible: (row) => this.permissionService.getGrantedPolicy('YourModule.Edit')
      },
      {
        label: 'Xóa',
        icon: 'pi pi-trash',
        command: (row) => this.delete(row),
        visible: (row) => this.permissionService.getGrantedPolicy('YourModule.Delete')
      }
    ];
  }
}
```

## 🎨 Custom Styling

### Override Colors

```scss
// your-component.scss
::ng-deep {
  // Header background
  .p-datatable .p-datatable-thead > tr > th {
    background-color: #your-color !important;
  }

  // Row colors
  .p-datatable .p-datatable-tbody > tr:nth-child(even) {
    background-color: #your-color !important;
  }

  // Hover color
  .p-datatable .p-datatable-tbody > tr:hover {
    background-color: #your-color !important;
  }
}
```

## 📝 Best Practices

### 1. Performance

- Luôn sử dụng `lazy loading` với data lớn
- Set `scrollHeight` khi có nhiều dòng để tránh render toàn bộ
- Giới hạn số column không cần thiết

### 2. UX

- Đặt width phù hợp cho từng column
- Sử dụng `freeze` cho các column quan trọng (ID, Name)
- Thêm tooltip cho actions phức tạp
- Sử dụng icon rõ ràng cho actions

### 3. ABP Integration

- Kết hợp với `ListService` cho pagination tự động
- Sử dụng `ConfirmationService` cho delete actions
- Dùng `PermissionService` để kiểm soát actions
- Localize tất cả text hiển thị

## 🐛 Troubleshooting

### Table không hiển thị data

- Kiểm tra `totalCount` có được set đúng không
- Verify `data` array có dữ liệu
- Check console để xem có lỗi API không

### Sorting không hoạt động

- Đảm bảo `sortable: true` trong column config
- Xử lý `sortField` và `sortOrder` trong `loadData()`
- Backend phải hỗ trợ sorting

### Actions không hiện

- Check `actions` array có được truyền vào không
- Verify `visible()` function return true
- Kiểm tra permissions nếu dùng

## 📚 Tài liệu tham khảo

- [PrimeNG Table Documentation](https://primeng.org/table)
- [ABP Angular Documentation](https://docs.abp.io/en/abp/latest/UI/Angular)

## 📞 Hỗ trợ

Nếu gặp vấn đề hoặc có câu hỏi, vui lòng liên hệ team phát triển.

---

**Version:** 1.0.0  
**Last Updated:** December 2025

