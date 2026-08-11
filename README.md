# Task Management System (BstSolutions)

ASP.NET Core MVC application for managing employees and work tasks.

This repository contains a working Task Management System for the ASP.NET Core MVC machine test.

Project / assembly name: `BstSolutions`.

---

## 1. Project overview

The application will support:

- Employee list / create / edit (including active/inactive and unique email)
- Task create / view / edit / delete / mark completed
- Filtering and sorting across tasks
- Dashboard summary counts and upcoming tasks
- Optimistic concurrency with SQL Server `RowVersion`
- AJAX mark-complete without full page reload

---

## 2. Architecture

Layered, interview-friendly MVC architecture:

| Layer | Responsibility |
|---|---|
| Middleware | Cross-cutting HTTP concerns (request logging, global exception handling) |
| Controller | HTTP, model binding, ModelState, call services, return View/JSON/Redirect |
| Service | Business operations, business validation/rules, coordinate repositories |
| Repository | Database access only via EF Core + LINQ |
| EF Core | ORM, mapping, LINQ translation, persistence |
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

Rules:

1. Controllers depend on services only (not repositories / DbContext).
2. Services own business rules.
3. Repositories own data access.
4. Middleware does not contain business logic.

---

## 4. Technology stack

- .NET 8 or later (this machine uses .NET 10)
- ASP.NET Core MVC
- C#
- Entity Framework Core (SQL Server provider)
- SQL Server
- LINQ
- Razor Views
- Dependency Injection
- Async programming
- SQL-based database deployment (master + patch scripts)

Fetch API will be used later for AJAX complete-task calls.

---

## 5. Database Deployment

**EF Core Migrations are intentionally not used.**

Schema is owned by SQL scripts under `BstSolutions/Database/`.

### NEW DATABASE

1. Double-click `Database/Database-Create.bat`
2. It runs `sqlcmd` → `Database.sql`
3. `Database.sql` creates `TaskManagementSystem` (if needed) and executes the latest **master** SQL files:
   - `01_Tables/ApplicationUsers.sql`
   - `01_Tables/Employees.sql`
   - `01_Tables/WorkTasks.sql`
   - `02_Indexes/Indexes.sql`
   - `03_SeedData/SeedData.sql`
4. Demo admin (`admin@gmail.com`) is seeded by `03_SeedData/SeedData.sql` with a **PasswordHasher** hash (not plain text).

### EXISTING DATABASE

1. Double-click `Database/Database-Patch.bat`
2. It runs `sqlcmd` → `Database-Patch.sql` against the existing database
3. Only active folder `Patch.sql` files are executed
4. It never recreates the database and never runs `Database.sql`

### Master vs Patch

| Type | Purpose |
|---|---|
| **Master SQL** | Latest **complete** definition. Used for a **new** database. |
| **Patch SQL** | **Only changes** needed to upgrade an **existing** database. |

Lifecycle:

```text
Update master SQL
    ↓
Add/uncomment upgrade entry in Patch.sql
    ↓
Double-click Database-Patch.bat
    ↓
After deploy: remove/comment completed patch
    ↓
Master remains the latest definition
```

### Requirements

- **sqlcmd** must be installed and available in PATH (`sqlcmd -?`)
- Windows Integrated Authentication is used by default
- Default instance in the `.bat` files: `(localdb)\MSSQLLocalDB` (matches `appsettings.json`)
- Edit `SERVER` / `DATABASE` at the top of the `.bat` files if needed

### Entry points

- `Database.sql` — master creation entry point (no Patch.sql includes)
- `Database-Patch.sql` — patch entry point (no master SQL includes)

---

## 6. How to create a new database

Preferred:

1. Open `BstSolutions/Database/`
2. Double-click `Database-Create.bat`

Manual equivalent:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -i Database.sql
```

---

## 7. How to patch an existing database

Preferred:

1. Update the related master SQL file(s)
2. Uncomment the matching `:r` line(s) in the folder `Patch.sql`
3. Double-click `Database-Patch.bat`

Manual equivalent:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -d TaskManagementSystem -b -i Database-Patch.sql
```

Example `01_Tables/Patch.sql`:

```sql
-- Uncomment only the required changes.
-- :r .\Employees.sql
-- :r .\WorkTasks.sql
```

---

## 8. Why EF Core Migrations are not used

- Schema ownership stays explicit in SQL (easy to review in interviews / PRs).
- New environments always get the latest master definition.
- Existing environments get precise patches.
- Avoids migration history drift and EF-generated SQL surprises.
- Matches the required SQL master + patch deployment model for this assessment.

EF Core is still used as the ORM for application data access.

---

## 9. Enum representation (app ↔ database)

Enums are stored as **INT** in SQL Server and converted in EF Core.

### Priority (`BstSolutions.Common.Enums.Priority`)

| Value | Name |
|---|---|
| 1 | Low |
| 2 | Medium |
| 3 | High |
| 4 | Critical |

### Status (`BstSolutions.Common.Enums.WorkTaskStatus`)

| Value | Name |
|---|---|
| 1 | New |
| 2 | InProgress |
| 3 | Completed |
| 4 | Cancelled |

Named `WorkTaskStatus` (not `TaskStatus`) to avoid clashing with `System.Threading.Tasks.TaskStatus`.

CHECK constraints in SQL mirror these values.

---

## 10. SQL naming conventions

Simple and consistent for the machine test:

| Rule | Example |
|---|---|
| Primary key | `ID_Employee`, `ID_WorkTask` |
| Foreign key column | `FK_Employee` |
| FK constraint name | `FK_WorkTasks_Employee` |

C# entities keep clean interview-friendly names (`Id`, `EmployeeId`) and map to SQL columns with Fluent API:

- `Employee.Id` → `ID_Employee`
- `WorkTask.Id` → `ID_WorkTask`
- `WorkTask.EmployeeId` → `FK_Employee`
- `WorkTask.RowVersion` → SQL Server `ROWVERSION`

---

## 11. Concurrency approach (Task 12)

I used optimistic concurrency with SQL Server RowVersion.

1. The original `RowVersion` is loaded with the task and sent back with the update (hidden field on the Edit form).
2. EF Core includes that value in the update condition.
3. If another user has already modified the record, the `RowVersion` does not match.
4. EF Core detects the concurrency conflict (`DbUpdateConcurrencyException`).
5. We show the user a conflict message instead of silently overwriting the other user's changes.

Configured with:

```csharp
entity.Property(t => t.RowVersion).IsRowVersion();
```

Schema is created through SQL scripts (`Database.sql`), not EF migrations.

---

## 12. Validation approach

### Request / UI validation (DataAnnotations on ViewModels)

- Employee: FirstName/LastName/Email required; valid email format
- Task: Title required, max 150; Description max 2000

### Business validation (Service layer — later)

- Duplicate employee email
- Employee must be active when assigning a new task
- Task must have an employee
- DueDate cannot be before today
- Completed task cannot be deleted
- CompletedDate set/cleared with status changes

`Validators/Employee` and `Validators/Task` folders are placeholders for custom validation only where genuinely required. Custom attributes are **not** created for every rule.

---

## 13. Security approach

| Concern | Approach |
|---|---|
| Authentication | Database-backed users + cookie authentication + ClaimsPrincipal |
| Authorization | `[Authorize]` on create/edit/delete/complete actions |
| Anti-forgery | `[ValidateAntiForgeryToken]` on POST actions |
| Over-posting | ViewModels instead of binding entities directly |
| Model validation | DataAnnotations + ModelState checks |
| SQL injection | EF Core parameterized queries / LINQ |
| XSS | Razor output encoding by default |
| Passwords | ASP.NET Core `PasswordHasher<ApplicationUser>` (never plain text) |

### Authentication

Full ASP.NET Core Identity is intentionally **not** used (outside the required scope of the technical test).

Database-backed cookie authentication is used:

```text
Controller
    ↓
AuthenticationService
    ↓
UserRepository
    ↓
EF Core
    ↓
ApplicationUsers

AuthenticationService
    ↓
ClaimsPrincipal
    ↓
Cookie Authentication
    ↓
[Authorize]
```

**Technical-test / demo admin account** (seeded by `Database-Create.bat` → `SeedData.sql` with a password hash only):

| Setting | Value |
|---|---|
| Email | `admin@gmail.com` |
| Password | `Admin@123` |
| Role | `Admin` |

Do **not** use this pattern for production credentials.

Claims issued on login:

- `ClaimTypes.NameIdentifier` → user Id
- `ClaimTypes.Name` → email
- `ClaimTypes.Email` → email
- `ClaimTypes.Role` → role (e.g. `Admin`)

`PasswordHash` is never returned to views, never added to claims, and never logged.

Protected with `[Authorize]`:

- Task: Create, Edit, Delete, Mark Completed
- Employee: Create, Edit

Default unauthenticated entry: `/Account/Login`

JWT is **not** used (MVC/Razor app).

Schema + demo admin seed are created by `Database-Create.bat` / `Database.sql` / `SeedData.sql`.
The seed stores only an ASP.NET Core `PasswordHasher` hash for `Admin@123` — never the plain password.

### Rate limiting (optional, cross-cutting)

If added later, use ASP.NET Core rate limiting middleware/configuration only.

- Not a service concern
- Not a controller concern
- No `RateLimitService`

---

## 14. Response and Error Handling

### ApiResponse (AJAX)

AJAX endpoints return `ApiResponse` without exposing technical details.

**Success (200)**
```json
{
  "success": true,
  "userMessage": "Task completed successfully.",
  "errorCode": null
}
```

**Business failure (400)**
```json
{
  "success": false,
  "userMessage": "An employee with this email already exists.",
  "errorCode": "EMPLOYEE_EMAIL_EXISTS"
}
```

**Unexpected (500)** via `GlobalExceptionMiddleware`
```json
{
  "success": false,
  "userMessage": "Something went wrong. Please try again later. Reference: ABC12345.",
  "errorCode": "INTERNAL_SERVER_ERROR"
}
```

### Rules

- Frontend shows **UserMessage only** (`result.userMessage`).
- Never show `DeveloperMessage`, stack traces, SQL, or connection strings.
- Expected business failures use `BusinessException` (UserMessage + DeveloperMessage + ErrorCode).
- Unexpected exceptions are logged by `GlobalExceptionMiddleware`.
- MVC Razor actions use `ModelState` / `TempData` + Views.

### Flow

```text
Expected business failure
    → BusinessException
    → Controller (ModelState / ApiResponse)
    → Frontend UserMessage

Unexpected exception
    → GlobalExceptionMiddleware
    → ILogger (technical details)
    → Safe ApiResponse / Error page
    → Frontend UserMessage only
```

---

## 15. Request Logging Middleware

Every HTTP request passes through `Middleware/RequestLoggingMiddleware`.

It records:

- HTTP method
- Request path
- Authenticated user (or `Anonymous`)
- Response status code
- Duration in milliseconds
- `TraceIdentifier` (ASP.NET Core request id)

Sensitive information is intentionally **not** logged (passwords, tokens, cookies, Authorization headers, request/response bodies, connection strings).

Uses `ILogger<RequestLoggingMiddleware>` so the existing ASP.NET Core / Serilog logging configuration receives the logs. Request logging is separate from `GlobalExceptionMiddleware` (which handles unexpected exceptions as `Error`).

Example:

```text
HTTP POST /Task/Complete responded 200 in 145 ms for user admin@gmail.com, TraceId: 0HMK...
```

Unauthenticated example:

```text
HTTP GET /Account/Login responded 200 in 35 ms for user Anonymous, TraceId: 0HMK...
```

Pipeline order:

```text
RequestLoggingMiddleware → GlobalExceptionMiddleware → app pipeline
```

---

## 16. Error handling approach

`Middleware/GlobalExceptionMiddleware`:

- Catches unhandled exceptions
- Logs with `ILogger` (full details server-side)
- Never exposes stack traces, SQL, connection strings, or internal details
- Returns JSON-friendly safe payload for AJAX/JSON requests
- Redirects MVC requests to `/Home/Error`

Business-specific exceptions are handled in services/controllers; middleware stays generic for unexpected failures.

---

## 17. Solution structure

```text
BstSolutions/
├── Controllers/
├── Middleware/
│   ├── RequestLoggingMiddleware.cs
│   └── GlobalExceptionMiddleware.cs
├── Data/
├── Models/
├── ViewModels/
│   ├── Employee/
│   ├── Task/
│   └── Dashboard/
├── Repositories/
│   └── Interfaces/
├── Services/
│   └── Interfaces/
├── Validators/
│   ├── Employee/
│   └── Task/
├── Common/
│   ├── Constants/
│   ├── Enums/
│   └── Extensions/
├── Views/
│   ├── Employee/
│   ├── Task/
│   ├── Dashboard/
│   ├── Home/
│   └── Shared/
├── wwwroot/
├── Database/
│   ├── Database.sql
│   ├── Database-Patch.sql
│   ├── Database-Create.bat
│   ├── Database-Patch.bat
│   ├── 01_Tables/
│   ├── 02_Indexes/
│   └── 03_SeedData/
├── Program.cs
├── appsettings.json
└── README.md (repo root)
```

---

## 18. Future optional enhancements

1. Replace demo cookie auth with ASP.NET Core Identity or company SSO.
2. Optionally enable ASP.NET Core rate limiting middleware.
3. Add unit tests for service business rules.

---

## 19. Local run (after database is created)

```bash
cd BstSolutions
dotnet restore
dotnet build
dotnet run
```

Default route: `Dashboard/Index`.
