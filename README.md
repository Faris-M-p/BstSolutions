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
| Middleware | Cross-cutting HTTP concerns (global exception handling) |
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
Middleware (GlobalExceptionMiddleware)
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

- .NET 8
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

## 5. Database deployment approach

**EF Core Migrations are not used.**

Schema is owned by SQL scripts under `BstSolutions/Database/`.

### Master SQL vs Patch SQL

| Type | Purpose |
|---|---|
| **Master SQL** | Latest **complete** definition. Used to create a **new** database. |
| **Patch SQL** | **Only changes** needed to upgrade an **existing/old** database. |

Concept:

```text
New Database:
    Database.sql
        ↓
    Latest master definitions (01_Tables, 02_Indexes, 03_SeedData)
        ↓
    Latest database

Existing Database:
    Database-Patch.sql
        ↓
    Active patch files
        ↓
    Latest database
```

Rules:

1. Master files always contain the latest complete definition.
2. `Database.sql` creates a completely new database from masters.
3. Folder `Patch.sql` files only list which scripts to run (commented `:r` includes).
4. `Database-Patch.sql` executes those patch entry points.
5. Masters are never historical migration files.
6. No timestamp-based migration files.
7. No EF Core migration commands.

---

## 6. How to create a new database

From `BstSolutions/Database/` using **sqlcmd** (or SSMS with SQLCMD Mode):

```bash
sqlcmd -S .\SQLEXPRESS -E -i Database.sql
```

This creates database `TaskManagementSystem` (override with sqlcmd variable if needed) and applies:

- `01_Tables/Employees.sql`
- `01_Tables/WorkTasks.sql`
- `02_Indexes/WorkTasks.sql`
- `03_SeedData/Seed.sql`

Update `appsettings.json` / `appsettings.Development.json` connection string if your SQL Server instance differs.

---

## 7. How to patch an existing database

```bash
sqlcmd -S .\SQLEXPRESS -E -d TaskManagementSystem -i Database-Patch.sql
```

`Database-Patch.sql` only orchestrates folder patch entry points:

```text
=== Patch 01_Tables ===
=== Patch 02_Indexes ===
=== Patch 03_SeedData ===
```

Each folder `Patch.sql` lists which master files to run — **commented by default**. Uncomment only what you need:

```sql
-- Uncomment only the required changes.
-- :r .\Employees.sql
-- :r .\WorkTasks.sql
```

When schema changes:

1. Update the related **master** file(s) (latest complete definition).
2. Uncomment the matching `:r` line(s) inside the folder `Patch.sql`.
3. Run `Database-Patch.sql`.

Patch files contain **no change logic** — only which scripts to include.

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

Prepared / to be applied during feature implementation:

| Concern | Approach |
|---|---|
| Authorization | `[Authorize]` on controllers/actions when auth is introduced |
| Anti-forgery | `[ValidateAntiForgeryToken]` on POST actions (already on skeletons) |
| Over-posting | ViewModels instead of binding entities directly |
| Model validation | DataAnnotations + ModelState checks |
| SQL injection | EF Core parameterized queries / LINQ |
| XSS | Razor output encoding by default |
| Secrets | Connection strings in config (not committed secrets for production) |

Full ASP.NET Core Identity is **not** required at this stage.

### Where authorization will be applied

- Prefer controller/action `[Authorize]` on Employee, Task, and Dashboard once authentication exists.
- Keep anonymous access only for error/public pages if needed.
- Do not push authorization checks into repositories.

### Rate limiting (optional, cross-cutting)

If added later, use ASP.NET Core rate limiting middleware/configuration only.

- Not a service concern
- Not a controller concern
- No `RateLimitService`

---

## 14. Error handling approach

`Middleware/GlobalExceptionMiddleware`:

- Catches unhandled exceptions
- Logs with `ILogger` (full details server-side)
- Never exposes stack traces, SQL, connection strings, or internal details
- Returns JSON-friendly safe payload for AJAX/JSON requests
- Redirects MVC requests to `/Home/Error`

Business-specific exceptions will be handled in services/controllers later; middleware stays generic.

---

## 15. Solution structure

```text
BstSolutions/
├── Controllers/
├── Middleware/
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
│   ├── 01_Tables/
│   ├── 02_Indexes/
│   └── 03_SeedData/
├── Program.cs
├── appsettings.json
└── README.md (repo root)
```

---

## 16. Future optional enhancements

1. Add ASP.NET Core Identity and enable `[Authorize]` on Employee/Task/Dashboard.
2. Optionally enable ASP.NET Core rate limiting middleware.
3. Add unit tests for service business rules.

---

## 17. Local run (after database is created)

```bash
cd BstSolutions
dotnet restore
dotnet build
dotnet run
```

Default route: `Dashboard/Index`.
