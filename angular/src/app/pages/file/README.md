# File Module

Module quản lý tệp tin (File).

## Cấu trúc

```
file/
├── README.md              # File này
└── [feature-name]/        # Các feature sẽ được thêm vào đây
    ├── [feature].component.ts
    ├── [feature].component.html
    ├── [feature].component.scss
    ├── [feature].models.ts
    └── index.ts
```

## Localization

Sử dụng resource: `File`

Các keys chính:
- `Menu:File` - Tên module trong menu

## Permissions

Khi tạo permissions cho Entity, sử dụng format:
- `File{EntityName}` - GroupName
- `File{EntityName}.Create` - Create permission
- `File{EntityName}.Edit` - Edit permission
- `File{EntityName}.Delete` - Delete permission
- `File{EntityName}.View` - View permission

## Routes

Thêm routes vào `pages.routes.ts`:

```typescript
{
  path: 'file/[feature-name]',
  component: [Feature]Component,
  canActivate: [permissionGuard],
  data: {
    requiredPermission: 'File[EntityName]',
    breadcrumb: 'File::Menu:[EntityName]'
  }
}
```

## Proxy Services

Proxy services sẽ được generate vào:
- `app/proxy/file/`

Xem thêm: `dev_note/RULES_FRONTEND_DEVELOPMENT.md`

