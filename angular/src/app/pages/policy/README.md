# Policy Module

Module quản lý bảo hiểm (Policy).

## Cấu trúc

```
policy/
├── README.md              # File này
└── [feature-name]/        # Các feature sẽ được thêm vào đây
    ├── [feature].component.ts
    ├── [feature].component.html
    ├── [feature].component.scss
    ├── [feature].models.ts
    └── index.ts
```

## Localization

Sử dụng resource: `Policy`

Các keys chính:
- `Menu:Policy` - Tên module trong menu

## Permissions

Khi tạo permissions cho Entity, sử dụng format:
- `Policy{EntityName}` - GroupName
- `Policy{EntityName}.Create` - Create permission
- `Policy{EntityName}.Edit` - Edit permission
- `Policy{EntityName}.Delete` - Delete permission
- `Policy{EntityName}.View` - View permission

## Routes

Thêm routes vào `pages.routes.ts`:

```typescript
{
  path: 'policy/[feature-name]',
  component: [Feature]Component,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'Policy[EntityName]',
    breadcrumb: 'Policy::Menu:[EntityName]'
  }
}
```

## Proxy Services

Proxy services sẽ được generate vào:
- `app/proxy/policy/`

Xem thêm: `dev_note/RULES_FRONTEND_DEVELOPMENT.md`

