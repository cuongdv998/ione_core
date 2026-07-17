# Survey Module

Module quản lý khảo sát (Survey).

## Cấu trúc

```
survey/
├── README.md              # File này
└── [feature-name]/        # Các feature sẽ được thêm vào đây
    ├── [feature].component.ts
    ├── [feature].component.html
    ├── [feature].component.scss
    ├── [feature].models.ts
    └── index.ts
```

## Localization

Sử dụng resource: `Survey`

Các keys chính:
- `Menu:Survey` - Tên module trong menu

## Permissions

Khi tạo permissions cho Entity, sử dụng format:
- `Survey{EntityName}` - GroupName
- `Survey{EntityName}.Create` - Create permission
- `Survey{EntityName}.Edit` - Edit permission
- `Survey{EntityName}.Delete` - Delete permission
- `Survey{EntityName}.View` - View permission

## Routes

Thêm routes vào `pages.routes.ts`:

```typescript
{
  path: 'survey/[feature-name]',
  component: [Feature]Component,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'Survey[EntityName]',
    breadcrumb: 'Survey::Menu:[EntityName]'
  }
}
```

## Proxy Services

Proxy services sẽ được generate vào:
- `app/proxy/survey/`

Xem thêm: `dev_note/RULES_FRONTEND_DEVELOPMENT.md`

