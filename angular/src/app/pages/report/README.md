# Report Module

Module quản lý báo cáo (Report).

## Cấu trúc

```
report/
├── README.md              # File này
└── [feature-name]/        # Các feature sẽ được thêm vào đây
    ├── [feature].component.ts
    ├── [feature].component.html
    ├── [feature].component.scss
    ├── [feature].models.ts
    └── index.ts
```

## Localization

Sử dụng resource: `Report`

Các keys chính:
- `Menu:Report` - Tên module trong menu

## Permissions

Khi tạo permissions cho Entity, sử dụng format:
- `Report{EntityName}` - GroupName
- `Report{EntityName}.Create` - Create permission
- `Report{EntityName}.Edit` - Edit permission
- `Report{EntityName}.Delete` - Delete permission
- `Report{EntityName}.View` - View permission

## Routes

Thêm routes vào `pages.routes.ts`:

```typescript
{
  path: 'report/[feature-name]',
  component: [Feature]Component,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'Report[EntityName]',
    breadcrumb: 'Report::Menu:[EntityName]'
  }
}
```

## Proxy Services

Proxy services sẽ được generate vào:
- `app/proxy/report/`

Xem thêm: `dev_note/RULES_FRONTEND_DEVELOPMENT.md`

