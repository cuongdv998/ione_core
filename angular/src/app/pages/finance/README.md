# Finance Module

Module quản lý tài chính (Finance).

## Cấu trúc

```
finance/
├── README.md              # File này
└── [feature-name]/        # Các feature sẽ được thêm vào đây
    ├── [feature].component.ts
    ├── [feature].component.html
    ├── [feature].component.scss
    ├── [feature].models.ts
    └── index.ts
```

## Localization

Sử dụng resource: `Finance`

Các keys chính:
- `Menu:Finance` - Tên module trong menu

## Permissions

Khi tạo permissions cho Entity, sử dụng format:
- `Finance{EntityName}` - GroupName
- `Finance{EntityName}.Create` - Create permission
- `Finance{EntityName}.Edit` - Edit permission
- `Finance{EntityName}.Delete` - Delete permission
- `Finance{EntityName}.View` - View permission

## Routes

Thêm routes vào `pages.routes.ts`:

```typescript
{
  path: 'finance/[feature-name]',
  component: [Feature]Component,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'Finance[EntityName]',
    breadcrumb: 'Finance::Menu:[EntityName]'
  }
}
```

## Proxy Services

Proxy services sẽ được generate vào:
- `app/proxy/finance/`

Xem thêm: `dev_note/RULES_FRONTEND_DEVELOPMENT.md`

