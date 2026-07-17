# Cursor Usage Rules for ABP Framework + Angular Project

This document defines strict rules for using Cursor AI when developing an ABP Framework (Open Source) backend and Angular frontend. These rules exist to prevent structural violations, ensure module boundaries, and maintain long-term project health.

---
## 1. General Principles

1. **Cursor must NEVER modify ABP template auto-generated boilerplate** unless explicitly instructed.
2. **Every instruction to Cursor must specify module + layer** (e.g., `Product: Domain`, `Procurement: Application.Contracts`).
3. **Always ask Cursor for a diff (preview) before applying large changes.**
4. **All tasks must be incremental, small, and isolated.**
5. **No code is written without following DDD + ABP conventions.**

---
## 2. Backend (ABP) Rules

### 2.1 Module Structure
Cursor must enforce:
- Domain
- Domain.Shared
- Application
- Application.Contracts
- EntityFrameworkCore
- HttpApi

No new folders outside this structure.

### 2.2 Domain Layer Rules
- All business logic stays here.
- Entities must:
  - Be aggregate roots unless otherwise stated.
  - Use **private/protected setters**.
  - Validate input via constructor or domain methods.
- Use domain events when appropriate.

Cursor must avoid:
- Adding application logic in domain.
- Adding infrastructure (EF) code here.

### 2.3 Application Layer Rules
- Only orchestration and calling domain logic.
- No business logic.
- DTOs in Application.Contracts.
- Mapping using `ObjectMapper` only.

Cursor must avoid:
- Adding DbContext usage except via repository.
- Writing domain logic.

### 2.4 Infrastructure (EFCore) Rules
- Add entity configurations
- Add repository implementations
- Add DbContext extensions

Cursor must avoid:
- Putting business rules in EF layer.

### 2.5 HttpApi Rules
- Add controllers and API definitions only.
- Should not include business logic.

Cursor must avoid:
- Adding DTOs or entities here.

---
## 3. Angular Rules

### 3.1 Folder Structure Enforcement
- `core/` for services (auth, layout), guards, interceptors.
- `shared/` for utilities, directives, reusable components.
- `features/<module>/` for business features.
- `proxy/` must be auto-generated only.

Cursor must avoid:
- Editing files in `proxy/` (generated only).
- Creating new folders outside the defined structure.

### 3.2 Component Rules
- Use Angular standalone components.
- Keep components dumb.
- Use services for logic.
- Import DTO types from proxy.

Cursor must avoid:
- Hardcoding API URLs.
- Creating duplicate services when proxy already provides one.

### 3.3 State Management Rules
- If using Signals: maintain them in `/state` folder.
- If using NGXS: separate state, selectors, and actions.

Cursor must avoid:
- Putting state in components.

---
## 4. Naming Conventions

### Backend
- Classes: `PascalCase`
- Methods: `PascalCase`
- Private fields: `_camelCase`
- DTOs must end with `Dto`
- Application service interfaces end with `AppService`

### Angular
- Components: `xxx.component.ts`
- Services: `xxx.service.ts`
- State: `xxx.state.ts`
- Folders: kebab-case

---
## 5. Git & Branching Rules

### Branching
- `main`: stable release
- `develop`: integration
- `feature/<name>`: per module or task

### Commit Message Rules
Format:
```
<type>(<scope>): <message>
```
Examples:
```
feat(product): add product creation flow
fix(procurement): correct dto mapping
refactor(core): extract base table component
```

Allowed commit types:
- `feat`
- `fix`
- `refactor`
- `chore`
- `test`
- `docs`

---
## 6. Cursor Prompt Templates

### 6.1 Backend creation prompt
```
You are editing an ABP Framework (Open Source) modular DDD project.
Follow conventions strictly.
Task: <describe module + layer task>
Do NOT modify other layers.
```

### 6.2 Angular feature prompt
```
You are editing an Angular app structured by features.
Use proxy-generated services.
Use standalone components.
Task: <describe UI task>
```

### 6.3 Refactor prompt
```
Show me the diff only.
Do not modify architectural structure.
Task: <describe change>
```

---
## 7. Restricted Actions (Cursor Must NEVER Do)
1. Rewrite the solution structure.
2. Modify auto-generated ABP files unless asked.
3. Add business logic to Application layer or HttpApi.
4. Hardcode URLs in Angular.
5. Mix domain & infrastructure code.
6. Remove constructor validation.
7. Create new Angular API services when proxy already exists.

---
## 8. Safety Checks Before Accepting Cursor Output
Before applying changes from Cursor, verify:
- Does it break DDD boundaries?
- Does it modify unrelated files?
- Does it break ABP naming conventions?
- Does it add logic in wrong layers?
- Does it create duplicate code?
- Does it follow file structure?

Only accept if all checks pass.

---
## 9. Checklist for New Module Creation
When generating a new module using Cursor:
1. Create Domain + Entities + Value Objects first.
2. Create Domain.Shared (enums, constants).
3. Create Application.Contracts (DTO + Service Interface).
4. Create Application (implement AppService).
5. Create EntityFrameworkCore (repositories, mappings).
6. Create HttpApi (controller).
7. Run ABP CLI to generate Angular proxy.
8. Create Angular UI module.

---
## 10. How to Extend This File
You can later append:
- API performance rules
- Logging standards
- Error-handling conventions
- UI accessibility rules

## 11. How to create Proxy for Angular
You can run command:
- abp generate-proxy -t ng -m <module-name> -a default -s <project-name> -url https://localhost:44360