# Product Module

Module quản lý sản phẩm (Product).

## Cấu trúc

```
product/
├── README.md              # File này
└── [feature-name]/        # Các feature sẽ được thêm vào đây
    ├── [feature].component.ts
    ├── [feature].component.html
    ├── [feature].component.scss
    ├── [feature].models.ts
    └── index.ts
```

## Localization

Sử dụng resource: `Product`

Các keys chính:
- `Menu:Product` - Tên module trong menu

## Permissions

Khi tạo permissions cho Entity, sử dụng format:
- `Product{EntityName}` - GroupName
- `Product{EntityName}.Create` - Create permission
- `Product{EntityName}.Edit` - Edit permission
- `Product{EntityName}.Delete` - Delete permission
- `Product{EntityName}.View` - View permission

## Routes

Thêm routes vào `pages.routes.ts`:

```typescript
{
  path: 'product/[feature-name]',
  component: [Feature]Component,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'Product[EntityName]',
    breadcrumb: 'Product::Menu:[EntityName]'
  }
}
```

## Proxy Services

Proxy services sẽ được generate vào:
- `app/proxy/product/`

Xem thêm: `dev_note/RULES_FRONTEND_DEVELOPMENT.md`

