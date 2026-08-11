# Task Management System (BstSolutions)

ASP.NET Core MVC application for managing employees and work tasks.

---

## 1. Project overview

- Employee list / create / edit (active/inactive, unique email)
- Task create / view / edit / delete / mark completed
- Filtering and sorting across tasks
- Dashboard summary counts and upcoming tasks
- Optimistic concurrency with SQL Server `RowVersion`
- AJAX mark-complete without full page reload

---

## 2. Architecture

| Layer | Responsibility |
|---|---|
| Middleware | Request logging, global exception handling |
| Controller | HTTP, model binding, ModelState, call services, return View/JSON/Redirect |
| Service | Business rules, coordinate repositories |
| Repository | Database access via EF Core + LINQ |
| EF Core | ORM / persistence |
| SQL Server | Data store |

No Generic Repository, Unit of Work, MediatR, CQRS, AutoMapper, or Dapper.

---

## 3. Dependency flow

```text
HTTP Request
    ↓
RequestLoggingMiddleware
    ↓
GlobalExceptionMiddleware
    ↓
Routing / Authentication / Authorization
    ↓
Controller
    ↓
Service
    ↓
Repository
    ↓
EF Core + LINQ
    ↓
SQL Server
```

1. Controllers depend on services only (not repositories / DbContext).
2. Services own business rules.
3. Repositories own data access.
4. Middleware does not contain business logic.

---

## 4. Technology stack

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core (SQL Server)
- SQL Server / LocalDB
- LINQ, Razor Views, Dependency Injection
- SQL-based database deployment (master + patch scripts)

---

## 5. Database Deployment

**EF Core Migrations are not used.** Schema is owned by SQL scripts under `BstSolutions/Database/`.

### New database

1. Double-click `Database/Database-Create.bat`
2. Runs `Database.sql` (creates `TaskManagementSystem` and applies master scripts)
3. Demo admin (`admin@gmail.com`) is seeded with a PasswordHasher hash

### Existing database

1. Double-click `Database/Database-Patch.bat`
2. Runs `Database-Patch.sql` against the existing database
3. Only active folder `Patch.sql` entries are executed

### Master vs Patch

| Type | Purpose |
|---|---|
| **Master SQL** | Latest complete definition for a **new** database |
| **Patch SQL** | Changes only, to upgrade an **existing** database |

```text
Update master SQL
    ↓
Uncomment upgrade entry in Patch.sql
    ↓
Run Database-Patch.bat
    ↓
After deploy: comment/remove completed patch
```

### Requirements

- `sqlcmd` available in PATH
- Default server: `(localdb)\MSSQLLocalDB` (matches `appsettings.json`)

---

## 6. Enum representation

Enums are stored as **INT** in SQL Server.

### Priority

| Value | Name |
|---|---|
| 1 | Low |
| 2 | Medium |
| 3 | High |
| 4 | Critical |

### WorkTaskStatus

| Value | Name |
|---|---|
| 1 | New |
| 2 | InProgress |
| 3 | Completed |
| 4 | Cancelled |

Named `WorkTaskStatus` to avoid clashing with `System.Threading.Tasks.TaskStatus`.

---

## 7. SQL naming conventions

| Rule | Example |
|---|---|
| Primary key | `ID_Employee`, `ID_WorkTask` |
| Foreign key column | `FK_Employee` |
| FK constraint | `FK_WorkTasks_Employee` |

C# entity properties map via Fluent API (`Id` → `ID_Employee`, etc.).

---

## 8. Concurrency

Optimistic concurrency with SQL Server `RowVersion`:

1. Original `RowVersion` is sent with the Edit form (hidden field).
2. EF Core includes it in the update condition.
3. If another user changed the row, `DbUpdateConcurrencyException` is raised.
4. User sees a conflict message instead of a silent overwrite.

```csharp
entity.Property(t => t.RowVersion).IsRowVersion();
```

---

## 9. Validation

### UI / ViewModel (DataAnnotations)

- Employee: FirstName, LastName, Email required; valid email
- Task: Title required (max 150); Description max 2000

### Business rules (Service layer)

- Duplicate employee email
- Employee must be active when assigning a new task
- Task must have an employee
- DueDate cannot be before today
- Completed task cannot be deleted
- CompletedDate set/cleared with status changes

---

## 10. Security

| Concern | Approach |
|---|---|
| Authentication | DB users + cookie auth + ClaimsPrincipal |
| Authorization | `[Authorize]` on create/edit/delete/complete |
| Anti-forgery | `[ValidateAntiForgeryToken]` on POSTs |
| Over-posting | ViewModels (not entities) |
| SQL injection | EF Core parameterized LINQ |
| Passwords | `PasswordHasher` (never plain text) |

### Demo login (seeded)

| Setting | Value |
|---|---|
| Email | `admin@gmail.com` |
| Password | `Admin@123` |
| Role | `Admin` |

JWT and full ASP.NET Core Identity are not used.

---

## 11. Response and error handling

### ApiResponse (AJAX)

**Success**
```json
{ "success": true, "userMessage": "Task completed successfully.", "errorCode": null }
```

**Business failure**
```json
{ "success": false, "userMessage": "An employee with this email already exists.", "errorCode": "EMPLOYEE_EMAIL_EXISTS" }
```

**Unexpected (500)**
```json
{ "success": false, "userMessage": "Something went wrong. Please try again later." }
```

### Flow

```text
BusinessException → Controller (ModelState / ApiResponse) → UserMessage
Unhandled exception → GlobalExceptionMiddleware → safe JSON or /Home/Error
```

Frontend shows **UserMessage only**. Stack traces are never returned to the client.

---

## 12. Request Logging Middleware

`RequestLoggingMiddleware` logs each request:

- Method, path, status code, duration, user (`Anonymous` if not authenticated), TraceId
- Does **not** log passwords, tokens, cookies, Authorization headers, or bodies

Example:
```text
HTTP POST /Task/Complete responded 200 in 145 ms for user admin@gmail.com, TraceId: 0HMK...
```

Order: `RequestLoggingMiddleware` → `GlobalExceptionMiddleware` → app pipeline

---

## 13. Global exception middleware

1. `try` / `await _next` / `catch`
2. Log exception with `ILogger`
3. AJAX → safe JSON
4. MVC → redirect `/Home/Error`

`BusinessException` is handled in controllers; middleware covers unexpected failures only.

---

## 14. Solution structure

```text
BstSolutions/
├── Controllers/
├── Middleware/
│   ├── RequestLoggingMiddleware.cs
│   └── GlobalExceptionMiddleware.cs
├── Data/
├── Models/
├── ViewModels/
├── Repositories/
├── Services/
├── Common/
├── Views/
├── wwwroot/
├── Database/
├── Program.cs
└── appsettings.json
```

---

## 15. How to run

```bash
# 1. Create database
#    Double-click BstSolutions/Database/Database-Create.bat

# 2. Run app
cd BstSolutions
dotnet restore
dotnet build
dotnet run
```

Default route: `/Account/Login`  
After login: Dashboard
