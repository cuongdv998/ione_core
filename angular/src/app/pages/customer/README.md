# Customer Module

Module quản lý khách hàng (Customer).

## Cấu trúc

```
customer/
├── README.md              # File này
└── [feature-name]/        # Các feature sẽ được thêm vào đây
    ├── [feature].component.ts
    ├── [feature].component.html
    ├── [feature].component.scss
    ├── [feature].models.ts
    └── index.ts
```

## Localization

Sử dụng resource: `Customer`

Các keys chính:
- `Menu:Customer` - Tên module trong menu

## Permissions

Khi tạo permissions cho Entity, sử dụng format:
- `Customer{EntityName}` - GroupName
- `Customer{EntityName}.Create` - Create permission
- `Customer{EntityName}.Edit` - Edit permission
- `Customer{EntityName}.Delete` - Delete permission
- `Customer{EntityName}.View` - View permission

## Routes

Thêm routes vào `pages.routes.ts`:

```typescript
{
  path: 'customer/[feature-name]',
  component: [Feature]Component,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'Customer[EntityName]',
    breadcrumb: 'Customer::Menu:[EntityName]'
  }
}
```

## Proxy Services

Proxy services sẽ được generate vào:
- `app/proxy/customer/`

Xem thêm: `dev_note/RULES_FRONTEND_DEVELOPMENT.md`

