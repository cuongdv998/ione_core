# Partner Module

Module quản lý đối tác (Partner).

## Cấu trúc

```
partner/
├── README.md              # File này
└── [feature-name]/        # Các feature sẽ được thêm vào đây
    ├── [feature].component.ts
    ├── [feature].component.html
    ├── [feature].component.scss
    ├── [feature].models.ts
    └── index.ts
```

## Localization

Sử dụng resource: `Partner`

Các keys chính:
- `Menu:Partner` - Tên module trong menu

## Permissions

Khi tạo permissions cho Entity, sử dụng format:
- `Partner{EntityName}` - GroupName
- `Partner{EntityName}.Create` - Create permission
- `Partner{EntityName}.Edit` - Edit permission
- `Partner{EntityName}.Delete` - Delete permission
- `Partner{EntityName}.View` - View permission

## Routes

Thêm routes vào `pages.routes.ts`:

```typescript
{
  path: 'partner/[feature-name]',
  component: [Feature]Component,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'Partner[EntityName]',
    breadcrumb: 'Partner::Menu:[EntityName]'
  }
}
```

## Proxy Services

Proxy services sẽ được generate vào:
- `app/proxy/partner/`

Xem thêm: `dev_note/RULES_FRONTEND_DEVELOPMENT.md`

