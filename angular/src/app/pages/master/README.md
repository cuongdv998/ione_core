# Master Module

Module quản lý danh mục (Master).

## Cấu trúc

```
master/
├── README.md              # File này
└── [feature-name]/        # Các feature sẽ được thêm vào đây
    ├── [feature].component.ts
    ├── [feature].component.html
    ├── [feature].component.scss
    ├── [feature].models.ts
    └── index.ts
```

## Localization

Sử dụng resource: `Master`

Các keys chính:
- `Menu:Master` - Tên module trong menu

## Permissions

Khi tạo permissions cho Entity, sử dụng format:
- `Master{EntityName}` - GroupName
- `Master{EntityName}.Create` - Create permission
- `Master{EntityName}.Edit` - Edit permission
- `Master{EntityName}.Delete` - Delete permission
- `Master{EntityName}.View` - View permission

## Routes

Thêm routes vào `pages.routes.ts`:

```typescript
{
  path: 'master/[feature-name]',
  component: [Feature]Component,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'Master[EntityName]',
    breadcrumb: 'Master::Menu:[EntityName]'
  }
}
```

## Proxy Services

Proxy services sẽ được generate vào:
- `app/proxy/master/`

Xem thêm: `dev_note/RULES_FRONTEND_DEVELOPMENT.md`

