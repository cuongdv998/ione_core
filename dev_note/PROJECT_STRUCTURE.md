# Cấu Trúc Project iOne-Core2

## Tổng Quan

Project sử dụng **ABP Framework** (ASP.NET Core) cho backend và **Angular** cho frontend, theo kiến trúc **Modular Monolith**.

## Cấu Trúc Thư Mục

```
iOne-Core2/
├── src/                          # Backend code chính
│   ├── common/                   # Code dùng chung
│   │   ├── domain/               # Domain layer (entities, domain services)
│   │   │   ├── iOne.Domain/      # Domain entities và business logic
│   │   │   └── iOne.Domain.Shared/ # Shared constants, enums, localization
│   │   └── infra/                # Infrastructure layer
│   │       ├── iOne.EntityFrameworkCore/ # EF Core DbContext, repositories
│   │       └── iOne.DbMigrator/  # Database migration tool
│   └── web/                      # Application layer chính
│       ├── iOne.Application/     # Application services
│       ├── iOne.Application.Contracts/ # DTOs, interfaces
│       ├── iOne.HttpApi/         # API controllers
│       ├── iOne.HttpApi.Client/  # Client proxy
│       └── iOne.HttpApi.Host/    # Web host (startup project)
│
├── modules/                      # Các module riêng biệt
│   ├── hr/                       # HR Module (ví dụ)
│   │   └── src/
│   │       ├── iOne.Hr.Application/
│   │       ├── iOne.Hr.Application.Contracts/
│   │       ├── iOne.Hr.HttpApi/
│   │       ├── iOne.Hr.HttpApi.Client/
│   │       └── iOne.Hr.HttpApi.Host/
│   └── Volo.Abp.*/              # ABP Framework modules
│
├── angular/                     # Frontend Angular
│   ├── src/app/
│   │   ├── core/                # Core services, guards, pipes
│   │   ├── layout/              # Layout components
│   │   ├── pages/               # Page components
│   │   ├── proxy/               # Generated API proxies
│   │   └── shared/              # Shared components
│   └── scripts/                 # Build scripts (generate proxy, etc.)
│
├── test/                        # Test projects
└── dev_note/                    # Documentation và quy tắc phát triển
```

## Backend Architecture

### 1. Common Layer (`src/common/`)

**Domain Layer:**
- `iOne.Domain`: Entities, Domain Services, Domain Logic
- `iOne.Domain.Shared`: Constants, Enums, Localization, Permissions (chung)

**Infrastructure Layer:**
- `iOne.EntityFrameworkCore`: DbContext, Repository implementations, Migrations
- `iOne.DbMigrator`: Tool để chạy database migrations

### 2. Web Layer (`src/web/`)

**Application Layer:**
- `iOne.Application`: Application Services (business logic)
- `iOne.Application.Contracts`: DTOs, Interfaces, Contracts

**API Layer:**
- `iOne.HttpApi`: Controllers (REST API endpoints)
- `iOne.HttpApi.Client`: Client proxy để gọi API từ module khác
- `iOne.HttpApi.Host`: Startup project (Program.cs, appsettings.json)

### 3. Module Structure (`modules/`)

Mỗi module độc lập có cấu trúc tương tự:

```
modules/{ModuleName}/src/
├── iOne.{ModuleName}.Application/        # Application services
├── iOne.{ModuleName}.Application.Contracts/ # DTOs, interfaces, permissions
├── iOne.{ModuleName}.HttpApi/           # Controllers
├── iOne.{ModuleName}.HttpApi.Client/     # Client proxy
└── iOne.{ModuleName}.HttpApi.Host/      # Module host (có thể chạy riêng)
```

**Lưu ý quan trọng:**
- Permissions phải đặt trong module, không đặt trong `common/domain`
- Mỗi module có thể chạy độc lập hoặc tích hợp vào main host

## Frontend Architecture

### 1. Core (`angular/src/app/core/`)

- **Services**: `localization.service.ts`, `navigation.service.ts`, `permission.service.ts`
- **Guards**: `auth.guard.ts`, `permission.guard.ts`
- **Pipes**: `translate.pipe.ts`, `permission.pipe.ts`
- **Directives**: `permission.directive.ts`, `translate.directive.ts`

### 2. Layout (`angular/src/app/layout/`)

- Layout components: `app.layout.ts`, `app.menu.ts`, `app.breadcrumb.ts`, etc.
- Menu động được load từ backend API `/api/app/menu`

### 3. Pages (`angular/src/app/pages/`)

- Các page components theo module: `hr/`, `audit-logs/`, `users/`, `roles/`
- Routes được định nghĩa trong `pages.routes.ts`

### 4. Proxy (`angular/src/app/proxy/`)

- Generated TypeScript proxies từ backend DTOs
- Sử dụng script `generate-proxy-all.js` để generate

## Quy Tắc Quan Trọng

### 1. Permissions

- **KHÔNG** đặt permissions trong `src/common/domain`
- **PHẢI** đặt permissions trong module: `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/Permissions/`
- Permission `Default` = `GroupName` (không phải `"{GroupName}.Default"`)

### 2. Localization

- Module localization: `modules/{ModuleName}/src/iOne.{ModuleName}.Application.Contracts/Localization/{ModuleName}/`
- Common localization: `src/common/domain/iOne.Domain.Shared/Localization/iOne/`
- Format key: `{ResourceName}::{Key}` (ví dụ: `Hr::HrDepartmentType:Code`)

### 3. Entity & Domain

- Entities đặt trong `src/common/domain/iOne.Domain/` (nếu dùng chung)
- Hoặc trong module nếu chỉ module đó dùng
- Entity phải kế thừa `FullAuditedAggregateRoot<Guid>` để có audit logging

### 4. Database Migrations

- Migrations được tạo trong `src/common/infra/iOne.EntityFrameworkCore/Migrations/`
- Chạy migration bằng `iOne.DbMigrator` project

## Các File Quan Trọng

### Backend

- `src/web/iOne.HttpApi.Host/Program.cs`: Entry point
- `src/common/infra/iOne.EntityFrameworkCore/EntityFrameworkCore/iOneDbContext.cs`: DbContext chính
- `src/web/iOne.Application/Navigation/iOneMenuContributor.cs`: Định nghĩa menu động

### Frontend

- `angular/src/app/app.routes.ts`: Main routes
- `angular/src/app/pages/pages.routes.ts`: Page routes
- `angular/src/app/core/services/navigation.service.ts`: Service load menu từ backend
- `angular/scripts/generate-proxy-all.js`: Script generate TypeScript proxies

## Development Workflow

1. **Tạo Module Mới**: Xem `dev_note/RULES_CREATE_NEW_MODULE.md`
2. **Backend Development**: Xem `dev_note/RULES_BACKEND_DEVELOPMENT.md`
3. **Frontend Development**: Xem `dev_note/RULES_FRONTEND_DEVELOPMENT.md`
4. **Generate Proxy**: Chạy `node angular/scripts/generate-proxy-all.js` sau khi thay đổi backend DTOs

## Startup Projects

- **Main Backend**: `src/web/iOne.HttpApi.Host/iOne.HttpApi.Host.csproj`
- **Module Backend** (nếu chạy riêng): `modules/{ModuleName}/src/iOne.{ModuleName}.HttpApi.Host/iOne.{ModuleName}.HttpApi.Host.csproj`
- **Frontend**: `angular/` (chạy `npm start` hoặc `ng serve`)

## Database

- **Provider**: PostgreSQL
- **Connection String**: Trong `appsettings.json` của host project
- **Migrations**: Tự động chạy khi start `DbMigrator` hoặc `HttpApi.Host`

## Technology Stack

- **Backend**: .NET 9.0, ABP Framework, Entity Framework Core, PostgreSQL
- **Frontend**: Angular, PrimeNG, Tailwind CSS
- **Architecture**: Modular Monolith, DDD (Domain-Driven Design)





















