# Claim Module

Module quản lý khiếu nại (Claim).

## Cấu trúc

```
claim/
├── README.md              # File này
└── [feature-name]/        # Các feature sẽ được thêm vào đây
    ├── [feature].component.ts
    ├── [feature].component.html
    ├── [feature].component.scss
    ├── [feature].models.ts
    └── index.ts
```

## Localization

Sử dụng resource: `Claim`

Các keys chính:
- `Menu:Claim` - Tên module trong menu

## Permissions

Khi tạo permissions cho Entity, sử dụng format:
- `Claim{EntityName}` - GroupName
- `Claim{EntityName}.Create` - Create permission
- `Claim{EntityName}.Edit` - Edit permission
- `Claim{EntityName}.Delete` - Delete permission
- `Claim{EntityName}.View` - View permission

## Routes

Thêm routes vào `pages.routes.ts`:

```typescript
{
  path: 'claim/[feature-name]',
  component: [Feature]Component,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'Claim[EntityName]',
    breadcrumb: 'Claim::Menu:[EntityName]'
  }
}
```

## Proxy Services

Proxy services sẽ được generate vào:
- `app/proxy/claim/`

Xem thêm: `dev_note/RULES_FRONTEND_DEVELOPMENT.md`

