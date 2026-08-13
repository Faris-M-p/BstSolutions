# Task Management System

**Candidate:** Muhammed Faris  

ASP.NET Core MVC Task Management System developed as a technical machine test.

The application supports employee management, work-task assignment and lifecycle management, search/filter/sort, a dashboard, cookie authentication, AJAX form interactions, optimistic concurrency, centralized error handling, and service-layer unit tests.

---

## 1. Project overview

This solution demonstrates a layered ASP.NET Core MVC application:

- Employee create / list / edit (including active/inactive)
- Work task create / edit / details / delete / mark completed
- Task assignment to employees
- Task search, filtering, and sorting
- Dashboard metrics and upcoming tasks
- Server-side validation and service-layer business rules
- Cookie-based authentication and authorization
- AJAX (`fetch`) for login, employee/task save, and mark completed
- Optimistic concurrency via SQL Server `ROWVERSION`
- xUnit + Moq unit tests for service business rules

---

## 2. Key features

| Feature | Status |
|---|---|
| Employee list | Implemented |
| Create employee | Implemented |
| Edit employee | Implemented |
| Employee delete | Not implemented (deactivate via `IsActive`) |
| Duplicate email validation | Implemented |
| Task create / edit / details | Implemented |
| Task delete | Implemented (form POST + redirect) |
| Task assignment | Implemented |
| Task search | Implemented |
| Task filtering | Implemented |
| Task sorting | Implemented |
| Dashboard | Implemented |
| AJAX mark completed | Implemented |
| AJAX login / employee save / task save | Implemented |
| Business validation (service layer) | Implemented |
| Authentication / authorization | Implemented |
| Anti-forgery | Implemented |
| Global exception handling | Implemented |
| Request logging | Implemented |
| Optimistic concurrency (`RowVersion`) | Implemented |
| Unit tests (service layer) | Implemented |
| EF Core migrations | Not used (SQL master/patch scripts) |

---

## 3. Technology stack

Verified from project files (`TargetFramework` / package references):

| Area | Technology |
|---|---|
| Runtime | **.NET 10** (`net10.0`) |
| Web | ASP.NET Core MVC, Razor Views |
| Language | C# |
| ORM | Entity Framework Core (`Microsoft.EntityFrameworkCore.SqlServer` 10.0.0) |
| Database | SQL Server / LocalDB |
| UI | Bootstrap (static assets under `wwwroot`) |
| Client AJAX | JavaScript Fetch API |
| Auth | Cookie authentication + `PasswordHasher<T>` |
| Tests | xUnit, Moq, Microsoft.NET.Test.Sdk, coverlet.collector |

> Note: The assessment brief may mention .NET 8; this repository targets **`net10.0`**.

---

## 4. Architecture

```
Browser (Razor Views + JavaScript)
        ↓
Controllers
        ↓
Services (business rules)
        ↓
Repositories
        ↓
Entity Framework Core
        ↓
SQL Server
```

| Layer | Responsibility |
|---|---|
| **Controller** | HTTP, model binding, anti-forgery, authorization, ModelState / JSON responses |
| **ViewModel** | UI-specific input/output shapes; reduces over-posting vs entities |
| **Service** | Application/business rules and orchestration |
| **Repository** | Data access via EF Core / LINQ |
| **EF Core** | ORM mapping, change tracking, concurrency tokens |
| **Razor Views** | Server-rendered UI |
| **JavaScript** | AJAX submit / mark completed without full reload where implemented |
| **Middleware** | Request logging + global exception handling |

There is no generic repository, Unit of Work, MediatR, CQRS, AutoMapper, or Dapper in this solution.

---

## 5. Project structure

```
BstSolutions/
├── BstSolutions/                 # ASP.NET Core MVC web application
│   ├── Controllers/
│   ├── Models/                   # EF entities
│   ├── ViewModels/
│   ├── Services/ (+ Interfaces/)
│   ├── Repositories/ (+ Interfaces/)
│   ├── Data/                     # ApplicationDbContext
│   ├── Middleware/
│   ├── Common/                   # Enums, exceptions, validation, ApiResponse
│   ├── Views/
│   ├── wwwroot/
│   ├── Database/                 # SQL create/patch scripts
│   ├── Program.cs
│   ├── appsettings.json
│   └── BstSolutions.slnx
└── BstSolutions.Tests/           # xUnit tests
    ├── Services/
    └── Helpers/
```

| Folder | Purpose |
|---|---|
| `Controllers` | MVC endpoints |
| `Models` | Database entities |
| `ViewModels` | Screen/API-shaped models |
| `Services` | Business logic |
| `Repositories` | Persistence |
| `Data` | `DbContext` configuration |
| `Middleware` | Logging and exception handling |
| `Common` | Shared enums, exceptions, attributes |
| `Database` | Schema create/patch SQL |
| `BstSolutions.Tests` | Unit tests |

Solution file: `BstSolutions/BstSolutions.slnx` (includes web + test projects).

---

## 6. Database design

### Tables

**`ApplicationUsers`** (login only — separate from employees)

| Column | Notes |
|---|---|
| `ID_ApplicationUser` | PK, identity |
| `Email` | Unique |
| `PasswordHash` | ASP.NET `PasswordHasher` hash |
| `IsActive` | Bit |
| `Role` | e.g. `Admin` |
| `CreatedDate` | UTC default |

**`Employees`**

| Column | Notes |
|---|---|
| `ID_Employee` | PK, identity |
| `FirstName` / `LastName` | Required |
| `Email` | Unique |
| `IsActive` | Default `1` |
| `CreatedDate` | UTC default |

**`WorkTasks`**

| Column | Notes |
|---|---|
| `ID_WorkTask` | PK, identity |
| `Title` / `Description` | Description optional |
| `FK_Employee` | FK → `Employees.ID_Employee` |
| `Priority` | INT 1–4 (CHECK) |
| `Status` | INT 1–4 (CHECK) |
| `DueDate` | `DATE` |
| `CreatedDate` | UTC |
| `CompletedDate` | Nullable |
| `RowVersion` | `ROWVERSION` concurrency token |

### Relationship

`Employee` **1 → many** `WorkTask` (`FK_Employee`), delete behavior **Restrict**.

### Indexes

- `IX_WorkTasks_FK_Employee`
- `IX_WorkTasks_Status`
- `IX_WorkTasks_Priority`
- `IX_WorkTasks_DueDate`

### Enums (C# / DB INT)

| Priority | Status |
|---|---|
| 1 Low, 2 Medium, 3 High, 4 Critical | 1 New, 2 InProgress, 3 Completed, 4 Cancelled |

---

## 7. Database setup

This project uses **SQL master/patch scripts**, not EF Core migrations.

### New database

```
Database.sql
   ↓
Creates TaskManagementSystem (if missing)
   ↓
01_Tables → 02_Indexes → 03_SeedData
```

**Recommended (Windows / LocalDB):**

1. Ensure SQL Server LocalDB is available: `(localdb)\MSSQLLocalDB`
2. Double-click `BstSolutions/Database/Database-Create.bat`  
   (runs `sqlcmd` against `Database.sql`)

### Existing database upgrades

```
Database-Patch.sql
   ↓
Runs folder Patch.sql files only
```

Use `Database-Patch.bat` for upgrades. Master `.sql` files remain the latest full definition for new environments.

---

## 8. Configuration

Connection string key: **`DefaultConnection`**

Example (LocalDB / Integrated Security — adjust for your environment):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TaskManagementSystem;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Do not commit production passwords or secrets. Use Integrated Security or user secrets / environment configuration for sensitive environments.

---

## 9. How to run the application

1. Clone or extract the repository.
2. Open `BstSolutions/BstSolutions.slnx` in Visual Studio (or open the folder in your IDE).
3. Restore NuGet packages.
4. Create the database (`Database-Create.bat` / `Database.sql`).
5. Confirm `DefaultConnection` in `appsettings.json` / `appsettings.Development.json`.
6. Set `BstSolutions` as the startup project.
7. Build and run (F5 / `dotnet run --project BstSolutions`).

**SDK:** .NET 10 (`net10.0`).

Default route starts at **Account/Login**.

---

## 10. Login / authentication

| Item | Detail |
|---|---|
| Mechanism | Cookie authentication (`TaskManagement.Auth`) |
| Login path | `/Account/Login` |
| Access denied | `/Account/AccessDenied` |
| Password storage | `PasswordHasher<ApplicationUser>` (never plain text) |
| Protected areas | `[Authorize]` on Dashboard, Employee, Task controllers |
| Logout | POST form in layout with anti-forgery |
| Return URL | Supported on login (local URLs only) |

### Seed / demo account

From `Database/03_SeedData/SeedData.sql` (idempotent):

| Field | Value |
|---|---|
| Email | `admin@gmail.com` |
| Password | `Admin@123` |
| Role | `Admin` |

Login is submitted via **AJAX** (`fetch`). Invalid credentials return HTTP **409 Conflict** with `{ message }`.

> `RememberMe` exists on the login ViewModel / sign-in options, but the current Login view does not render a Remember Me checkbox.

---

## 11. Employee management

| Action | Behavior |
|---|---|
| List | All employees |
| Create | AJAX POST; sets `IsActive = true` |
| Edit | AJAX POST; can toggle `IsActive` |
| Delete | Not implemented |

**Validation / rules**

- Required first name, last name, email (DataAnnotations + custom attributes)
- Unique email (service → `EMPLOYEE_EMAIL_EXISTS`)
- Email trimmed before persistence

---

## 12. Task management

| Action | Behavior |
|---|---|
| Create | AJAX; status forced to **New** |
| Edit | AJAX; supports status/priority/assignment/`RowVersion` |
| Details | Server-rendered view |
| Delete | Classic form POST → redirect + TempData (not AJAX) |
| Mark completed | AJAX on Task Index |

**`CompletedDate` rule** (`TaskService.ApplyStatusChange`):

- Status → **Completed**: `CompletedDate = UtcNow`
- Status → any other value: `CompletedDate = null`

---

## 13. Business validation

Business rules live in the **Service** layer (not only in controllers).

| Rule | Error code |
|---|---|
| Duplicate employee email | `EMPLOYEE_EMAIL_EXISTS` |
| Employee not found (update) | `EMPLOYEE_NOT_FOUND` |
| Assigned employee missing | `TASK_EMPLOYEE_NOT_FOUND` |
| Inactive employee on create / reassignment | `TASK_EMPLOYEE_INACTIVE` |
| Task not found | `TASK_NOT_FOUND` |
| Complete cancelled task | `TASK_INVALID_STATUS` |
| Complete already completed task | `TASK_ALREADY_COMPLETED` |
| Delete completed task | `TASK_COMPLETED_CANNOT_DELETE` |
| Concurrent edit conflict | `CONCURRENCY_CONFLICT` |
| Invalid login | `INVALID_CREDENTIALS` |

Additional ViewModel rules include required title, max lengths, due date not in the past on create (`DateNotInPast`), and greater-than-zero IDs.

---

## 14. Search / filter / sort

Implemented on Task Index via GET filter model and **composable `IQueryable`** in `TaskService` (database-side filtering/sorting).

**Filters:** Employee, Status, Priority  

**Search matches:** Title, Description, Employee First Name, Employee Last Name  

**Sort by:** Created Date (default), Due Date, Priority, Employee Name  

**Direction:** Asc / Desc  

---

## 15. Dashboard

`DashboardService` aggregates with EF Core queries (`AsNoTracking`, counts, limited upcoming list):

| Metric | Definition |
|---|---|
| Total Tasks | All tasks |
| New / In Progress / Completed | Count by status |
| Overdue | `DueDate < today` AND status not Completed/Cancelled |
| Upcoming | Next **5** tasks with `DueDate >= today`, excluding Completed/Cancelled, ordered by DueDate |

---

## 16. AJAX / JavaScript

AJAX (`fetch`) is used for:

- Login
- Employee Create / Edit
- Task Create / Edit
- Mark Completed

### Mark Completed flow

```
Click Mark Completed
 → fetch POST /Task/Complete
 → TaskController → TaskService → TaskRepository → SQL Server
 → JSON response
 → UI updates status cell (no full reload)
```

### Typical controller JSON shapes

| Outcome | Response |
|---|---|
| Validation failure | `400 BadRequest(ModelState)` |
| Business failure | `409 Conflict({ message })` |
| Success (save/login) | `200 Ok({ message, redirectUrl })` |
| Success (complete) | `200 Ok({ message })` |

Client displays ModelState errors generically into `#generalError` (no hard-coded field names). Anti-forgery token is sent in header `RequestVerificationToken`. AJAX requests also send `X-Requested-With: XMLHttpRequest`.

Task **Delete** and the Task Index **filter form** remain classic full-page posts (not AJAX).

---

## 17. Validation

| Layer | Implementation |
|---|---|
| ViewModels | DataAnnotations + custom attributes (`NoScriptTags`, `DateNotInPast`, `GreaterThanZero`) |
| Controllers | `ModelState.IsValid`; invalid AJAX posts return `BadRequest(ModelState)` |
| Services | Domain/business rules and exceptions |
| UI | `#generalError` for AJAX errors; Task filter uses `asp-validation-summary` |

Server-side validation always applies for posted AJAX actions. Unobtrusive jQuery validation scripts are not wired on the current AJAX forms.

---

## 18. Error handling

| Component | Behavior |
|---|---|
| Custom exceptions | `BusinessException`, `NotFoundException`, `ConflictException`, `UnauthorizedException` |
| Controllers | Many business failures returned as `Conflict({ message })` |
| `AppExceptionHandler` | Maps uncaught exceptions to 401/404/409/400/500 |
| AJAX uncaught errors | JSON `ApiResponse` (`success`, `userMessage`, `errorCode`) |
| Non-AJAX uncaught errors | Redirect to `/Account/Error` |
| Unexpected errors | Generic user message; details logged (no stack traces to client) |
| `RequestLoggingMiddleware` | Logs method, path, status, duration, user, TraceId (no request bodies) |

`AddProblemDetails()` is registered in `Program.cs`.

---

## 19. Security

| Measure | Implementation |
|---|---|
| Authentication | Cookie auth |
| Authorization | `[Authorize]` on protected controllers; `[AllowAnonymous]` on login/error |
| Anti-forgery | `[ValidateAntiForgeryToken]` + header `RequestVerificationToken` |
| Over-posting reduction | ViewModels for writes |
| SQL injection mitigation | EF Core parameterized queries |
| XSS mitigation | Razor encoding; `NoScriptTags` attribute on text inputs |
| Passwords | Hashed only |
| Cookies | HttpOnly; SecurePolicy SameAsRequest; 8-hour sliding expiration |

---

## 20. Concurrency

Optimistic concurrency is implemented for task updates:

1. `WorkTasks.RowVersion` mapped as EF concurrency token (`IsRowVersion()`).
2. Edit form posts `RowVersion`.
3. Service calls `SetOriginalRowVersion` before save.
4. On `DbUpdateConcurrencyException`, service throws `ConflictException` (`CONCURRENCY_CONFLICT`).
5. Controller returns **HTTP 409** with `{ message }` for the AJAX edit path.

If two users edit the same task, the second save receives a conflict message and must refresh.

---

## 21. Async programming

Database and service operations use async APIs, for example:

- `ToListAsync`, `FirstOrDefaultAsync` / repository async methods
- `SaveChangesAsync`
- `CancellationToken` parameters threaded through controllers → services → repositories

`CancellationToken` allows cooperative cancellation when a request is aborted.

---

## 22. Unit testing

Project: **`BstSolutions.Tests`**

| Tool | Role |
|---|---|
| xUnit | Test framework |
| Moq | Mock repository interfaces |
| Pattern | Arrange → Act → Assert |
| Focus | Service-layer business rules (no real SQL Server) |

```
Test → Service (real) → Mock Repository
```

### `EmployeeServiceTests`

- Get employees / active employees / by id
- Create success and duplicate email
- Update success, not found, duplicate email

### `TaskServiceTests`

- Create with active / missing / inactive employee
- Update success, not found, inactive reassignment, concurrency conflict
- Complete success / missing / cancelled / already completed
- Delete success / completed blocked / missing

Helper: `Helpers/TestDataFactory.cs`

### Run tests

**Visual Studio:** Test → Test Explorer → Run All  

**CLI:**

```bash
dotnet test BstSolutions.Tests/BstSolutions.Tests.csproj
```

---

## 23. Design decisions

### Why MVC?
Matches the machine-test requirement for ASP.NET Core MVC and Razor Views.

### Why ViewModels?
Separate UI contracts from entities and limit over-posting.

### Why Service layer?
Keep business rules out of controllers and reusable across actions.

### Why Repository?
Isolate EF Core data access behind interfaces (also enables Moq unit tests).

### Why EF Core + LINQ?
Readable composable queries for list/filter/dashboard without raw ADO.NET.

### Why AJAX?
Login/save and Mark Completed update without full page reload; JSON error display.

### Why SQL master/patch instead of migrations?
Explicit, reviewable schema scripts for create vs upgrade (`Database.sql` / `Database-Patch.sql`).

### Why unit tests at service layer?
Verify business exceptions and state transitions independently of SQL Server.

---

## 24. Assessment requirement mapping

| Assessment area | Implementation |
|---|---|
| Project structure | Controllers, Models, ViewModels, Services, Repositories, Data, Middleware, Views, Database |
| Employee management | `EmployeeController` + `EmployeeService` + `EmployeeRepository` |
| Task management | `TaskController` + `TaskService` + `TaskRepository` |
| Business validation | Service exceptions + ViewModel annotations |
| Search & filtering | `IQueryable` in `TaskService` |
| Sorting | LINQ `OrderBy` / `OrderByDescending` |
| Dashboard | `DashboardController` + `DashboardService` |
| AJAX | Fetch on login, employee/task save, mark completed |
| Repository / service | Implemented and registered in DI |
| Async programming | Async EF / service / repository APIs |
| Error handling | Custom exceptions + `AppExceptionHandler` + logging middleware |
| Concurrency | `RowVersion` + `ConflictException` / HTTP 409 |
| Security | Cookie auth, `[Authorize]`, anti-forgery, ViewModels, hashed passwords |
| Unit testing | xUnit + Moq service tests |
| Database deployment | SQL master/patch (not EF migrations) |

---

## 25. Manual testing checklist

### Authentication
- Open a protected URL while logged out → redirect to Login
- Login with `admin@gmail.com` / `Admin@123`
- Login with invalid credentials → error under the form
- Logout

### Employee
- Create a valid employee
- Create with duplicate email → business error
- Edit name/email
- Set employee inactive

### Task
- Create task for an active employee
- Try assign inactive employee on create → error
- Search / filter / sort on Task Index
- Edit task (including status changes) and confirm `CompletedDate` behavior
- Mark Completed via AJAX (row updates without reload)
- Try delete a completed task → blocked
- Delete a non-completed task
- (Optional) Two-browser concurrency edit to see conflict message

### Dashboard
- Confirm counts and upcoming list after creating/completing tasks

---

## 26. Project limitations / notes

| Item | Note |
|---|---|
| Employee hard delete | Not implemented; use `IsActive` |
| Task delete AJAX | Not implemented; classic POST + redirect |
| Remember Me UI | Property/sign-in support exists; checkbox not shown on Login view |
| EF migrations | Not used |
| Client unobtrusive validation | Not attached on current AJAX forms; server validation + JSON errors used |
| Users vs employees | Separate tables by design |

No other known core assessment gaps were identified beyond the items above.

---

## 27. Submission notes

This solution was developed as an ASP.NET Core MVC technical machine test with emphasis on:

- Clean separation of Controllers / Services / Repositories
- Maintainable structure and ViewModels
- Service-layer business validation
- EF Core + LINQ query composition
- Async data access
- Razor UI + selective AJAX (`fetch`)
- Centralized error handling and request logging
- Cookie security and anti-forgery
- Optimistic concurrency
- Focused service-layer unit tests

**Candidate:** Muhammed Faris
