# Interview Preparation — BstSolutions Task Management System

This document is based **only** on the current implementation in this repository.  
If a concept is useful for interview talk but **not in the code**, it is labeled:

> **Not currently implemented — interview concept**

---

## 1. PROJECT OVERVIEW

### What it does
ASP.NET Core MVC app to manage **employees** and **work tasks**, with:
- Cookie authentication (login first)
- Dashboard counts + upcoming tasks
- Employee create/edit
- Task create/edit/details/delete/AJAX mark-complete
- Filtering, search, sorting
- Optimistic concurrency (`RowVersion`)

### Main modules
| Module | Controllers / Views |
|---|---|
| Account | Login, Logout, AccessDenied, Error |
| Dashboard | Summary counts + upcoming list |
| Employee | Index, Create, Edit |
| Task | Index, Create, Edit, Details, Delete, Complete (AJAX) |

### Architecture
Layered MVC:

```text
Browser → Razor View / AJAX
  → Controller
  → Service (business rules)
  → Repository (data access)
  → EF Core (ORM)
  → SQL Server / LocalDB
```

### Why ASP.NET Core MVC?
Server-rendered pages + form posts fit CRUD admin apps. Controllers return Views or JSON for one AJAX action.

### Why Razor Views?
HTML is generated on the server with Tag Helpers (`asp-for`, `asp-validation-for`). No separate SPA.

### Where JavaScript/AJAX is used
`Views/Task/Index.cshtml` — **Mark Completed** uses `fetch` → `TaskController.Complete` → updates DOM without full reload.

### Where EF Core is used
`ApplicationDbContext`, all repositories (`AddAsync`, `SaveChangesAsync`, LINQ queries).

### Where LINQ is used
Especially `TaskService.GetTasksAsync`, `DashboardService.GetDashboardAsync`, repository queries.

### Where SQL Server is used
Database `TaskManagementSystem` on `(localdb)\MSSQLLocalDB` via connection string. Schema from SQL scripts (not EF migrations).

### Full request flow
```text
Browser
  → Razor View (HTML form / button)
  → HTTP GET/POST
  → Middleware (RequestLogging → ExceptionHandler → Auth)
  → Routing
  → Controller
  → Service
  → Repository
  → EF Core → SQL Server
  → Response
  → Razor View (HTML) or AJAX JSON (ApiResponse)
```

### Layer responsibilities
| Layer | Responsibility |
|---|---|
| Middleware | Cross-cutting: logging, exception handling, auth cookie |
| Controller | HTTP, model binding, ModelState, call services, return View/JSON/Redirect |
| Service | Business rules, map Entity ↔ ViewModel, throw custom exceptions |
| Repository | EF Core queries/commands only |
| EF Core | Change tracking, SQL generation, mapping |
| SQL Server | Persistence, constraints, indexes, RowVersion |

---

## 2. PROJECT STRUCTURE

| Path | Why it exists | If removed |
|---|---|---|
| `Controllers/` | HTTP entry points | App has no routes/actions |
| `Data/ApplicationDbContext.cs` | EF Core model + Fluent API | No ORM access |
| `Models/` | Entities matching tables | Nothing to map to DB |
| `ViewModels/` | Form/API shapes + validation | Over-posting risk / no UI binding model |
| `Repositories/` + `Interfaces/` | Data access abstraction | Controllers/services talk to EF directly |
| `Services/` + `Interfaces/` | Business logic | Controllers become fat / duplicated rules |
| `Middleware/` | Request logging + `AppExceptionHandler` | No request logs / unhandled exceptions uncaught |
| `Common/Enums` | Priority, WorkTaskStatus | Magic ints everywhere |
| `Common/Exceptions` | Business/NotFound/Conflict/Unauthorized | No structured error codes |
| `Common/Responses/ApiResponse.cs` | AJAX JSON envelope | Complete AJAX has no contract |
| `Common/Validation` | Custom attributes | Weaker model validation |
| `Views/` | Razor UI | No UI |
| `wwwroot/` | CSS/JS/libs | Static assets missing |
| `Database/` | Master/patch SQL | No schema deployment scripts |
| `Program.cs` | DI + pipeline | App won’t start |
| `appsettings.json` | Connection string | EF can’t connect |

Default route in `Program.cs`: `{controller=Account}/{action=Login}/{id?}`.

---

## 3. MODELS / ENTITIES

### Why `WorkTask` not `Task`?
`System.Threading.Tasks.Task` already exists. Name clash. Project uses `WorkTask` + enum `WorkTaskStatus`.

### `ApplicationUser` (table `ApplicationUsers`)
| Property | Type | Meaning |
|---|---|---|
| `ID_ApplicationUser` | `int` | PK, identity |
| `Email` | `string` | Login email, unique |
| `PasswordHash` | `string` | ASP.NET Identity hasher output (never plain text) |
| `IsActive` | `bool` | Inactive users cannot login |
| `Role` | `string` | Claim role (e.g. Admin) |
| `CreatedDate` | `DateTime` | Created UTC |

**Not linked to Employee.** Auth users ≠ employees.

### `Employee` (table `Employees`)
| Property | Type | Meaning |
|---|---|---|
| `ID_Employee` | `int` | PK |
| `FirstName` / `LastName` | `string` | Required |
| `Email` | `string` | Unique |
| `IsActive` | `bool` | Inactive cannot get **new** task assignment |
| `CreatedDate` | `DateTime` | Created |
| `WorkTasks` | `ICollection<WorkTask>` | Navigation (1 → many) |

### `WorkTask` (table `WorkTasks`)
| Property | Type | Meaning |
|---|---|---|
| `ID_WorkTask` | `int` | PK |
| `Title` | `string` | Required |
| `Description` | `string?` | Optional |
| `FK_Employee` | `int` | FK to `Employees.ID_Employee` |
| `Priority` | `Priority` enum | Stored as INT |
| `Status` | `WorkTaskStatus` enum | Stored as INT |
| `DueDate` | `DateTime` | Date |
| `CreatedDate` | `DateTime` | Created |
| `CompletedDate` | `DateTime?` | Set when status = Completed |
| `RowVersion` | `byte[]` | SQL `ROWVERSION` concurrency token |
| `Employee` | `Employee` | Navigation |

### Relationship
```text
Employee (1) ──< WorkTask (many)
FK_Employee → ID_Employee
OnDelete: Restrict (cannot delete employee who has tasks)
```

### Naming split (important interview point)
- **Entity/DB:** `ID_Employee`, `FK_Employee`, …
- **ViewModels/UI:** `Id`, `EmployeeId`, …
- **Service layer maps** between them.

---

## 4. APPLICATIONDBCONTEXT — LINE BY LINE

```csharp
public class ApplicationDbContext : DbContext
```
**Duty:** Unit of work + change tracker for this app.  
**If removed:** No EF access.

```csharp
public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
```
**Duty:** Receives DI options (connection string, provider).  
**Who:** ASP.NET Core DI via `AddDbContext`.

```csharp
public DbSet<Employee> Employees => Set<Employee>();
```
**Duty:** Entry point for querying/adding `Employee`.  
**If removed:** Cannot `context.Employees...`.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
```
**Duty:** Fluent API configuration.  
**EF vs app:** EF Core feature; content is application-specific.

```csharp
entity.ToTable("Employees");
```
Maps entity → table name.

```csharp
entity.HasKey(e => e.ID_Employee);
entity.Property(e => e.ID_Employee).HasColumnName("ID_Employee");
```
Defines PK and column name (aligned with SQL scripts).

```csharp
entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
entity.HasIndex(e => e.Email).IsUnique();
```
Required + length + unique index (mirrors DB unique constraint).

```csharp
entity.HasMany(e => e.WorkTasks)
    .WithOne(t => t.Employee)
    .HasForeignKey(t => t.FK_Employee)
    .HasConstraintName("FK_WorkTasks_Employee")
    .OnDelete(DeleteBehavior.Restrict);
```
**Duty:** 1–many relationship; restrict delete.  
**If remove Restrict / use Cascade:** Deleting employee could cascade-delete tasks (not wanted here).

```csharp
entity.Property(t => t.Priority).HasConversion<int>()
```
Enum ↔ INT in SQL.

```csharp
entity.Property(t => t.RowVersion).IsRowVersion();
```
Optimistic concurrency token.  
**If removed:** Last write wins; no concurrency detection.

### Data Annotations vs Fluent API
- Annotations: on ViewModels for MVC validation.
- Fluent API: in `OnModelCreating` for EF mapping/relationships.  
This project configures EF mainly with **Fluent API**.

---

## 5. EF CORE

**What is EF Core?** ORM that maps C# entities to SQL and generates SQL from LINQ.

**What is DbContext?** Session with the DB: tracking + `SaveChangesAsync`.

**What is DbSet?** Typed collection for one entity set.

**Change tracking:** EF watches loaded entities; on `SaveChangesAsync` it emits INSERT/UPDATE/DELETE.

**`AddAsync`:** Marks entity Added (SQL not sent yet).

**`SaveChangesAsync`:** Sends pending changes in a transaction.

**How EF knows table/PK/relationships:** `OnModelCreating` + conventions.

**Enums:** `.HasConversion<int>()` → INT columns.

**Indexes:** Fluent `HasIndex` + SQL `02_Indexes/Indexes.sql`.

**`AsNoTracking()`:** Used for read-only lists (employees list, task list query, user by email). Faster, no tracking.

**Tracked vs untracked:** Tracked = can update later; untracked = read-only snapshot.

**Why async?** Don’t block thread pool during I/O.

---

## 6. LINQ — FROM THIS PROJECT

### Example: Task filtering (`TaskService.GetTasksAsync`)
```csharp
var query = _taskRepository.Query()
    .AsNoTracking()
    .Include(t => t.Employee)
    .AsQueryable();

if (filter.EmployeeId.HasValue)
    query = query.Where(t => t.FK_Employee == filter.EmployeeId.Value);
// Status, Priority, Search Contains...
query = ApplySort(...);
await query.Select(...).ToListAsync(cancellationToken);
```

| Call | Meaning |
|---|---|
| `Where` | SQL WHERE (deferred) |
| `Include` | JOIN/load navigation |
| `OrderBy` / `ThenBy` | ORDER BY |
| `Select` | Project to ViewModel |
| `ToListAsync` | **Executes** query |
| `CountAsync` | SELECT COUNT |
| `Take(5)` | TOP 5 (Dashboard upcoming) |
| `AnyAsync` | EXISTS-style check |
| `FirstOrDefaultAsync` | First row or null |

### Interview answers
- **Does `Where` execute immediately?** No — builds expression tree until materialization (`ToListAsync`, `CountAsync`, …).
- **Where does filtering run?** On SQL Server when using `IQueryable` against EF.
- **Why not `ToListAsync` before filters?** Would pull all rows into memory then filter in C#.
- **`IQueryable` vs `IEnumerable`:** IQueryable can translate to SQL; IEnumerable is in-memory.

---

## 7. VIEWMODELS

### Why not bind entities in forms?
Entities have navigation props, hashes, RowVersion internals, extra fields. ViewModels:
- Expose only form fields
- Carry DataAnnotations
- Prevent **over-posting** (attacker cannot set `IsActive`/`Role` on create if not on Create VM)

### Actual ViewModels
| ViewModel | Purpose |
|---|---|
| `LoginViewModel` | Email, Password, RememberMe |
| `CreateEmployeeViewModel` | First/Last/Email |
| `EditEmployeeViewModel` | Id + fields + IsActive |
| `CreateTaskViewModel` | Title, Description, EmployeeId, Priority, DueDate |
| `EditTaskViewModel` | + Status + RowVersion |
| `TaskFilterViewModel` | Filter/sort query string |
| `TaskListViewModel` | Tasks + Filter |
| `TaskListItemViewModel` / `TaskDetailsViewModel` | Display |
| `DashboardViewModel` | Counts + upcoming |
| `AuthenticatedUserInfo` | Safe user info (no PasswordHash) |

---

## 8. VALIDATION

### Attributes used in this project
| Attribute | Where | Duty |
|---|---|---|
| `[Required]` | Login, Employee, Task VMs | Must have value |
| `[StringLength(n)]` | Text fields | Max length |
| `[MinLength(6)]` | Login Password | Min length |
| `[EmailAddress]` | Email fields | Format |
| `[Display(Name=...)]` | Labels + `{0}` in messages |
| `[DataType(Password/Date)]` | Input type hints |
| `[EnumDataType]` | Priority/Status | Enum domain |
| `[HiddenInput]` | RowVersion | Hidden field |
| `[NoScriptTags]` | Custom | Blocks script-like content |
| `[GreaterThanZero]` | Custom | Id/EmployeeId > 0 |
| `[DateNotInPast]` | CreateTask DueDate | Due ≥ today |

### How `{0}` works
```csharp
[Required(ErrorMessage = "{0} is required.")]
[Display(Name = "Password")]
public string Password { get; set; }
```
ASP.NET Core replaces `{0}` with Display name → **"Password is required."**  
The Razor file does **not** hardcode that sentence.

### Responsibility chain
```text
ViewModel attribute
  → MVC validation during model binding
  → ModelState entries
  → Tag Helpers (asp-validation-for / summary)
  → HTML error spans
  (+ client-side jquery.validate.unobtrusive if scripts loaded)
```

### Login page flow
1. Empty Password → `[Required]` fails → `ModelState.IsValid == false`
2. Controller: `return View(model)` (clears password)
3. `asp-validation-summary="All"` shows model + property errors
4. `asp-validation-for="Password"` shows field error

### ModelState.AddModelError
```csharp
ModelState.AddModelError(string.Empty, ex.UserMessage); // general / summary
ModelState.AddModelError("Email", "...");              // field-specific
```
Login uses `string.Empty` for invalid credentials (business auth failure after validation passed).

### Client vs server
- `_ValidationScriptsPartial` loads jquery.validate + unobtrusive → **client UX**
- Server validation always runs — **required** because JS can be disabled/bypassed

### Interview Qs
- **Who produces "Password is required"?** Validation attribute + Display name, not the cshtml string.
- **JS or C#?** Both possible; C# is authoritative.
- **JS disabled?** Server still validates; form redisplays with errors.
- **Why `View(model)` not `BadRequest` for Razor form?** Users expect HTML form with errors, not API status page.

---

## 9. RAZOR VIEWS

| Syntax | Duty |
|---|---|
| `@model LoginViewModel` | Strongly typed view |
| `asp-for="Email"` | Name/id/value + data-val-* attributes |
| `asp-validation-for="Email"` | Field error span |
| `asp-validation-summary="All"` | All errors list |
| `asp-action` / `asp-controller` | URL generation |
| `@Html.AntiForgeryToken()` / layout CSRF meta | CSRF token |
| `@section Scripts` | Page scripts after layout jQuery |
| `<partial name="_ValidationScriptsPartial" />` | Client validation scripts |
| `TempData["Success"]` | Flash message after redirect (layout) |
| `ViewData["ReturnUrl"]` | Pass return URL to login |

**Tag Helper example:**  
`<input asp-for="Email" />` becomes `<input type="email" id="Email" name="Email" value="..." data-val="true" ... />` (server-generated HTML).

Processed on **server**; browser receives HTML/JS.

---

## 10. MVC REQUEST FLOW

### GET `/Employee/Create`
Routing → `[Authorize]` check → `Create()` GET → View with empty VM → HTML.

### POST `/Employee/Create`
Model binding → validation → `ModelState` → if invalid `View(model)` → if valid Service → Repository → `SaveChangesAsync` → `TempData["Success"]` → `RedirectToAction(Index)` → GET Index.

### Post/Redirect/Get (PRG)
After successful POST, redirect to GET. Refresh won’t resubmit the POST.

---

## 11. CONTROLLERS

### Attributes
| Attribute | Duty |
|---|---|
| `[HttpGet]` / `[HttpPost]` | Verb constraint |
| `[Authorize]` | Must be authenticated (Dashboard/Employee/Task) |
| `[AllowAnonymous]` | Login/Error/AccessDenied |
| `[ValidateAntiForgeryToken]` | CSRF check on POSTs |

### Patterns
- `IActionResult` / `Task<IActionResult>` — async actions
- `CancellationToken` — ASP.NET Core injects request abort token
- Catch `BusinessException` / `UnauthorizedException` on forms → ModelState / TempData
- `Complete` AJAX → `Ok(ApiResponse)` or `BadRequest(ApiResponse)`
- Uncaught `NotFoundException` / `ConflictException` → `AppExceptionHandler`

---

## 12. CANCELLATIONTOKEN

```csharp
public async Task<IActionResult> Create(..., CancellationToken cancellationToken)
```
- Provided automatically by ASP.NET Core from the HTTP request.
- Passed Controller → Service → Repository → `ToListAsync` / `SaveChangesAsync`.
- If browser disconnects, cooperative cancel can stop waiting I/O.
- **Does not undo** an already committed SQL transaction.

---

## 13. SERVICE LAYER

Business rules live here (not controllers):

| Rule | Where | Result |
|---|---|---|
| Duplicate employee email | EmployeeService | `BusinessException` |
| Employee not found | EmployeeService | `NotFoundException` |
| Assign inactive employee | TaskService | `BusinessException` |
| Employee missing for task | TaskService | `NotFoundException` |
| Completed task delete | TaskService | `BusinessException` |
| Complete cancelled/already done | TaskService | `BusinessException` |
| Concurrency conflict | TaskService | `ConflictException` |
| Invalid login | AuthenticationService | `UnauthorizedException` |
| Due date past | CreateTaskViewModel `[DateNotInPast]` | ModelState (validation) |
| CompletedDate set/clear | `ApplyStatusChange` | When status changes |

---

## 14. REPOSITORY LAYER

- Owns DbContext usage.
- Interfaces enable testing/swapping.
- Controllers never inject `ApplicationDbContext` directly.
- `TaskRepository.SetOriginalRowVersion` sets concurrency original value before save.

---

## 15. DEPENDENCY INJECTION (`Program.cs`)

```csharp
AddScoped<IEmployeeService, EmployeeService>();
AddDbContext<ApplicationDbContext>(...);
```

| Lifetime | Used for | Why |
|---|---|---|
| **Scoped** | Repos, Services, DbContext | One instance per HTTP request |
| Transient | — | Not used for app services here |
| Singleton | — | Not used for DbContext (unsafe) |

Constructor injection:
```csharp
public EmployeeController(IEmployeeService employeeService)
```
ASP.NET Core resolves `IEmployeeService` → `EmployeeService` → its `IEmployeeRepository` → `ApplicationDbContext`.

---

## 16. MIDDLEWARE

### Pipeline order (actual)
```text
RequestLoggingMiddleware
  → UseExceptionHandler (AppExceptionHandler)
  → HSTS (non-dev)
  → HTTPS / StaticFiles / Routing
  → Authentication / Authorization
  → Controllers
```

### `RequestLoggingMiddleware` (line duties)
| Code | Duty |
|---|---|
| `RequestDelegate _next` | Next middleware/endpoint |
| `Stopwatch.StartNew()` | Duration |
| `await _next(context)` | Run rest of pipeline |
| `finally` | Always log even if exception handled downstream |
| `context.Request.Method/Path` | What was called |
| `context.Response.StatusCode` | Final status |
| `context.User.Identity` | Authenticated name or Anonymous |
| `context.TraceIdentifier` | Correlation id |
| `LogInformation(...)` | Structured log — **no body/password** |

**Before `_next`:** start timer.  
**After `_next`:** log result.

### Built-in auth middleware
- `UseAuthentication` — reads cookie, builds `ClaimsPrincipal`
- `UseAuthorization` — enforces `[Authorize]`

---

## 17. NO REQUEST LOGGING ATTRIBUTE

> **Not currently implemented — interview concept**

On this branch there is **no** `NoRequestLoggingAttribute`.  
`RequestLoggingMiddleware` logs every request after completion (method/path/status/user/duration only — still **no password/body logging**).

### How it would work (interview talk)
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoRequestLoggingAttribute : Attribute { }

// In middleware finally, after routing:
var endpoint = context.GetEndpoint();
if (endpoint?.Metadata.GetMetadata<NoRequestLoggingAttribute>() != null)
    skip log;
```
Endpoint metadata is available **after** routing runs inside `_next`.

---

## 18. GLOBAL EXCEPTION HANDLING

### Exception classes (`Common/Exceptions/`)
All inherit **`Exception`** directly (not from each other):

```csharp
public class BusinessException : Exception
{
    public string UserMessage { get; }
    public string ErrorCode { get; }
}
```
Same shape: `NotFoundException`, `ConflictException`, `UnauthorizedException`.

### `AppExceptionHandler` (`IExceptionHandler`)
| Exception | Status |
|---|---|
| UnauthorizedException | 401 |
| NotFoundException | 404 |
| ConflictException | 409 |
| BusinessException | 400 |
| Anything else | 500 + safe message |

AJAX (`X-Requested-With: XMLHttpRequest`) → JSON `ApiResponse`.  
MVC → redirect `/Account/Error`.

### Registration
```csharp
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails(); // required so UseExceptionHandler() configures correctly
app.UseExceptionHandler();
```

**ProblemDetails:** ASP.NET Core standard error format support. This app’s handler writes **`ApiResponse` JSON** for AJAX, not a full ProblemDetails body. `AddProblemDetails()` still needed for middleware setup.

### Expected vs unexpected
- Expected: business/not found/conflict/unauthorized → user message + code  
- Unexpected: NullReference/SQL down → log Error, generic message, no stack to client

---

## 19. HTTP STATUS CODES (THIS APP)

| Code | When in this project |
|---|---|
| 200 | Normal views / `Ok(ApiResponse)` on Complete success |
| 400 | `BusinessException` (handler) or `BadRequest(ApiResponse)` from Complete catch |
| 401 | `UnauthorizedException` if uncaught; cookie auth redirects to Login for MVC |
| 403 | AccessDenied path configured |
| 404 | `NotFoundException`; controller `NotFound()` when edit/details null |
| 409 | `ConflictException` (concurrency) |
| 500 | Unexpected exceptions |

**Razor forms:** prefer `return View(model)` with ModelState errors (HTML), not always HTTP 400 page.  
**AJAX Complete:** JSON + status via controller/handler.

401 = not logged in. 403 = logged in but forbidden. 404 = missing resource. 409 = version conflict.

---

## 20. AUTHENTICATION / LOGIN

### Flow
1. GET Login → form  
2. POST Login → ModelState validation  
3. `AuthenticationService.AuthenticateAsync`  
   - Load user by email (`AsNoTracking`)  
   - Check active  
   - `PasswordHasher.VerifyHashedPassword`  
   - Fail → `UnauthorizedException`  
4. Controller catches → ModelState general error  
5. Success → claims + `SignInAsync` cookie  
6. Redirect Dashboard (or local returnUrl)

### Claims issued
`NameIdentifier`, `Name`, `Email`, `Role`

### `[Authorize]`
Unauthenticated user hitting `/Employee` → challenge → `/Account/Login`.

### Demo seed
`admin@gmail.com` / `Admin@123` (hash only in DB).

---

## 21. ANTI-FORGERY / SECURITY

- Layout stores token in `<meta name="csrf-token">`
- Forms: antiforgery via Tag Helpers / tokens
- POSTs: `[ValidateAntiForgeryToken]`
- AJAX Complete sends header `RequestVerificationToken` (configured in `Program.cs` Antiforgery.HeaderName)
- Missing/invalid token → 400 antiforgery failure
- ViewModels reduce over-posting
- Logger does not log request bodies/passwords

---

## 22. JAVASCRIPT / AJAX

Only significant app JS: **Mark Completed** in `Task/Index.cshtml`.

```text
Click → fetch POST /Task/Complete
  headers: RequestVerificationToken, X-Requested-With: XMLHttpRequest
  body: id=...
→ JSON { success, userMessage, errorCode }
→ Update status cell, remove buttons, alert message
```

`wwwroot/js/site.js` is empty/minimal; validation scripts come from lib via partial.

---

## 23. DASHBOARD

`DashboardService` uses `CountAsync` on filtered `IQueryable` (DB-side counts):
- Total, New, InProgress, Completed
- Overdue: DueDate < today AND not Completed/Cancelled
- Upcoming: DueDate ≥ today, not completed/cancelled, `OrderBy(DueDate).Take(5)`

Prefer counting in SQL (as done) over loading all tasks into memory.

---

## 24. TASK FILTERING / SEARCH / SORTING

`TaskFilterViewModel` + `TaskService.GetTasksAsync`:
- EmployeeId, Status, Priority (optional Where)
- Search: Title, Description, Employee First/Last `Contains`
- SortBy: createddate (default), duedate, priority, employee
- SortDirection: asc/desc

Filters combine with AND on one `IQueryable` → one SQL query at `ToListAsync`.

---

## 25. CONCURRENCY

1. Edit loads `RowVersion` into hidden field  
2. Update sets `OriginalValue` via `SetOriginalRowVersion`  
3. EF UPDATE includes RowVersion in WHERE  
4. If another user saved first → `DbUpdateConcurrencyException` → `ConflictException`  
5. User must refresh

**Optimistic concurrency** (no locks).  
Complete/Delete do not send client RowVersion in current code.

> Alternatives (interview concept): pessimistic locks, app version int — **not used**; project uses SQL `ROWVERSION`.

---

## 26. DATABASE / SQL SERVER

Tables: `ApplicationUsers`, `Employees`, `WorkTasks`  
- PKs `ID_*`, FK `FK_Employee`  
- Unique emails  
- CHECK Priority/Status 1–4  
- Indexes on FK/Status/Priority/DueDate  
- `ROWVERSION`  
- Seed admin user  

DB constraints + app validation both matter (defense in depth).

---

## 27. DATABASE DEPLOYMENT — NO EF MIGRATIONS

| File | Role |
|---|---|
| `Database-Create.bat` / `Database.sql` | New DB = master scripts |
| `Database-Patch.bat` / `Database-Patch.sql` | Existing DB = Patch.sql includes |
| Folder `Patch.sql` | Commented `:r` lines until needed |

`ApplicationDbContext` = runtime ORM mapping, **not** a migration.  
Schema ownership = SQL scripts.

---

## 28. ATTRIBUTES (WHO READS THEM)

| Attribute | Reader | Category |
|---|---|---|
| `[Required]` etc. | MVC validation | Validation |
| Custom validation attrs | MVC validation | Validation |
| `[HttpGet]/[HttpPost]` | Routing/action selector | Routing |
| `[Authorize]`/`[AllowAnonymous]` | Authorization middleware | Security |
| `[ValidateAntiForgeryToken]` | Antiforgery filter | Security |
| `[HiddenInput]` | Tag Helpers / MVC | UI metadata |
| `[Display]` | Labels + validation messages | UI metadata |

Attributes are **metadata**, not magic methods that “run” alone.

---

## 29. “WHAT IF I REMOVE THIS LINE?”

| Remove | Effect |
|---|---|
| `[HttpPost]` on Login POST | Wrong verb matching / ambiguity |
| `[Authorize]` on EmployeeController | Anonymous access to employees |
| `[ValidateAntiForgeryToken]` | CSRF possible on that POST |
| Anti-forgery token in form/AJAX | POST fails antiforgery |
| `[Required]` on Password | Empty password may reach service |
| `[StringLength(2000)]` on Description | Only DB length protects |
| `asp-validation-for` | No field error UI (ModelState still set) |
| `asp-validation-summary` | No summary list |
| `await _next(context)` in middleware | Pipeline stops; no controller |
| `CancellationToken` pass-through | Harder to cancel EF waits |
| `SaveChangesAsync` | Nothing persisted |
| `HasIndex` unique Email | Duplicates possible at EF level (DB unique still if SQL has it) |
| `HasForeignKey` / Restrict | Broken relationship config / wrong delete behavior |
| `IsRowVersion` | No optimistic concurrency |
| `AddScoped` registration | DI fails at runtime |
| `AddProblemDetails` | `UseExceptionHandler()` can throw at startup |
| `UseExceptionHandler` | Unhandled exceptions not mapped by AppExceptionHandler |
| `UseAuthentication` | Cookie not applied; User empty |
| `UseAuthorization` | `[Authorize]` not enforced |
| `AsNoTracking` | Still works but more tracking overhead |
| `ToListAsync` | Query never executes if nothing materializes |
| `RedirectToAction` after save | Risk of POST refresh resubmit |
| `TempData` success | No flash message after redirect |
| `ModelState.AddModelError` | User doesn’t see auth failure text |
| `return View(model)` on invalid | Wrong response type for form UX |

---

## 30. INTERVIEW SCENARIO QUESTIONS

1. **Two users edit same task?** First save wins; second gets concurrency conflict (`ConflictException` / 409 path).  
2. **Duplicate email?** `BusinessException` → ModelState on form.  
3. **JS disabled?** Server validation still works; Complete AJAX won’t run.  
4. **Close browser during SaveChangesAsync?** Token may cancel; completed commit isn’t rolled back by token alone.  
5. **Unauthenticated `/Employee`?** Challenge → Login.  
6. **Invalid antiforgery?** Request rejected.  
7. **Task missing?** `NotFound()` or `NotFoundException`.  
8. **SQL Server down?** Unexpected exception → 500 safe message / Error page.  
9. **Delete completed task?** `BusinessException` → TempData error.  
10. **Why not business logic in controller?** Reuse, testability, thin HTTP layer.  
11. **Why ViewModels?** Validation + over-posting protection + UI shape.  
12. **Why EF Core?** LINQ + mapping + change tracking.  
13. **Why LINQ?** Readable queries translated to SQL.  
14. **Why Repository?** Isolate data access.  
15. **Why Service?** Business rules ownership.  
16. **Why Middleware?** Cross-cutting before/after MVC.  
17. **Why attributes?** Declarative metadata for framework features.  
18. **Why ProblemDetails registration?** Required companion for exception handler setup; AJAX still uses ApiResponse.  
19. **Why CancellationToken?** Cooperative cancel on abort.  
20. **Why RowVersion?** Detect lost updates.  
21. **Why Razor not SPA?** Simpler full-stack MVC for this CRUD app.  
22. **Where is Password required generated?** `[Required]` + `[Display(Name="Password")]`.  
23. **Who displays ModelState?** Validation Tag Helpers in Razor.  
24. **Does StringLength run JS?** No by itself; unobtrusive adapters may mirror it client-side.  
25. **Bypass JS validation?** Server still validates.  
26. **View vs BadRequest for forms?** HTML UX vs API semantics.  
27. **401 vs 403?** Not authenticated vs authenticated but denied.  
28. **404?** Resource missing.  
29. **409?** Concurrency conflict.  
30. **SaveChangesAsync internals?** Detect changes → generate SQL → execute transaction → update tracking.

---

## 31. LINE-BY-LINE RAPID FIRE

### Program.cs
**CODE:** `builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(...));`  
**ANSWER:** Registers EF context with SQL Server provider and connection string.  
**IF REMOVED:** DI cannot create DbContext.  
**WHO:** ASP.NET Core DI + EF Core.

**CODE:** `app.UseAuthentication(); app.UseAuthorization();`  
**ANSWER:** Populate User from cookie; enforce `[Authorize]`.  
**IF REMOVED:** Auth/authorization broken.  
**WHO:** ASP.NET Core security middleware.

### ApplicationDbContext
**CODE:** `.OnDelete(DeleteBehavior.Restrict)`  
**ANSWER:** Prevent deleting employee that still has tasks at relationship level.  
**IF REMOVED / Cascade:** Risk of deleting dependent tasks automatically.  
**WHO:** EF Core model.

**CODE:** `.IsRowVersion()`  
**ANSWER:** Maps concurrency token.  
**IF REMOVED:** Silent overwrites possible.  
**WHO:** EF Core + SQL ROWVERSION.

### AccountController Login
**CODE:** `if (!ModelState.IsValid) return View(model);`  
**ANSWER:** Stop when ViewModel validation failed; redisplay form.  
**WHO:** ASP.NET Core MVC.

**CODE:** `catch (UnauthorizedException ex) { ModelState.AddModelError(string.Empty, ex.UserMessage); }`  
**ANSWER:** Show invalid login as summary error without 500.  
**WHO:** Application controller.

### RequestLoggingMiddleware
**CODE:** `await _next(context);`  
**ANSWER:** Continue pipeline to exception handler/routing/MVC.  
**IF REMOVED:** Nothing after logging middleware runs.  
**WHO:** ASP.NET Core middleware pipeline.

**CODE:** `_logger.LogInformation("HTTP {Method} {Path} ...")`  
**ANSWER:** Structured request summary after response.  
**WHO:** Application middleware + logging providers.

### Task Complete AJAX
**CODE:** `'X-Requested-With': 'XMLHttpRequest'`  
**ANSWER:** Lets `AppExceptionHandler` return JSON instead of redirect.  
**WHO:** Browser JS + application handler.

### LoginViewModel
**CODE:** `[Required(ErrorMessage = "{0} is required.")] [Display(Name = "Password")]`  
**ANSWER:** Produces message “Password is required.”  
**WHO:** DataAnnotations + MVC validation.

---

## 32. FINAL QUICK REVISION

1. MVC layers: Controller → Service → Repository → EF → SQL.  
2. Razor = server HTML; one AJAX Complete action.  
3. Entities use SQL names (`ID_Employee`, `FK_Employee`); ViewModels use `Id`/`EmployeeId`.  
4. `WorkTask` avoids clash with `System.Threading.Tasks.Task`.  
5. Fluent API configures tables, keys, indexes, FK Restrict, enum INT, RowVersion.  
6. No EF migrations — SQL master/patch scripts.  
7. LocalDB connection in appsettings.  
8. ViewModels prevent over-posting + hold validation.  
9. `{0}` in ErrorMessage = Display name.  
10. ModelState drives form errors.  
11. Client validation is UX; server validation is mandatory.  
12. `asp-validation-for` / `asp-validation-summary` render ModelState.  
13. PRG after successful POST.  
14. Cookie auth + claims; `[Authorize]` on app areas.  
15. PasswordHasher — never store plain passwords.  
16. Invalid login → `UnauthorizedException` → ModelState.  
17. Duplicate email → `BusinessException`.  
18. Missing entity → `NotFoundException`.  
19. Concurrency → `ConflictException`.  
20. Exceptions inherit `Exception` separately.  
21. `AppExceptionHandler` maps to 401/404/409/400/500.  
22. AJAX gets ApiResponse JSON; MVC gets Error redirect.  
23. `AddProblemDetails` needed with `UseExceptionHandler`.  
24. Request logging: method/path/status/duration/user/trace — no bodies.  
25. NoRequestLogging attribute: **not in current branch**.  
26. LINQ on IQueryable → SQL; `ToListAsync` executes.  
27. `AsNoTracking` for read lists.  
28. Dashboard counts via `CountAsync` in DB.  
29. Task filters AND together on one query.  
30. Search uses `Contains` on title/description/names.  
31. RowVersion hidden field + OriginalValue.  
32. Completed tasks cannot be deleted.  
33. Inactive employees cannot be assigned new/reassigned tasks.  
34. Create task DueDate: `[DateNotInPast]`.  
35. Edit DueDate: no past restriction attribute.  
36. CompletedDate set when status Completed.  
37. Antiforgery header name `RequestVerificationToken`.  
38. CSRF meta token used by AJAX.  
39. Scoped DI for services/repos/DbContext.  
40. CancellationToken flows to EF async calls.  
41. Controllers stay thin; services own rules.  
42. Repository isolates DbContext.  
43. Unique email in Fluent API + SQL.  
44. Indexes support filter/sort columns.  
45. Restrict delete protects tasks.  
46. 401 vs 403 vs 404 vs 409 meanings.  
47. Form errors → `View(model)`; API-ish → status + JSON.  
48. TempData for success after redirect.  
49. Seed admin in SQL, not app startup.  
50. ApplicationUsers ≠ Employees.  
51. `Include(Employee)` for names in lists.  
52. `SetOriginalRowVersion` critical for concurrency.  
53. Never log passwords/tokens/bodies.  
54. `novalidate` on login form disables browser native HTML5 validation; app still uses unobtrusive + server validation.  
55. Default route starts at Account/Login.  
56. RememberMe → persistent cookie.  
57. Sliding expiration 8 hours.  
58. HttpOnly cookie.  
59. Enum CHECK constraints in SQL match C# values.  
60. Interview mantra: point to a line → say who runs it (MVC / EF / SQL / Browser / Middleware).  
61. `asp-validation-summary="ModelOnly"` = general/model errors only; property errors use `asp-validation-for`.  
62. `All` repeats property errors in the summary; `None` hides the summary.  
63. Login auth failure is shown via `TempData["LoginError"]` (PRG); field rules still use ModelState + Tag Helpers.

---

## 33. RAZOR VALIDATION SUMMARY — IMPORTANT

### Where `asp-validation-summary` is used in this project

| View | Setting |
|---|---|
| `Views/Account/Login.cshtml` | `ModelOnly` |
| `Views/Employee/Create.cshtml` | `ModelOnly` |
| `Views/Employee/Edit.cshtml` | `ModelOnly` |
| `Views/Task/Create.cshtml` | `ModelOnly` |
| `Views/Task/Edit.cshtml` | `ModelOnly` |
| `Views/Task/Index.cshtml` (filter form) | `ModelOnly` |

**None is not used** in this project (interview concept only for the enum value).

---

### The three modes

| Value | What it displays |
|---|---|
| `None` | Summary Tag Helper does not render ModelState errors in that summary block. |
| `ModelOnly` | Only **model-level / general** errors (key `string.Empty` / `""`). |
| `All` | Model-level errors **plus** every **property-level** error (Email, Password, Title, …). |

---

### Model-level vs property-level

**Model-level** (general):

```csharp
ModelState.AddModelError(string.Empty, "An employee with this email already exists.");
```

Shown by:

```html
<div asp-validation-summary="ModelOnly" class="text-danger"></div>
```

**Actual project usage:** `EmployeeController` / `TaskController` catch `BusinessException` and call `ModelState.AddModelError(string.Empty, ex.UserMessage)` then `return View(model)`.

**Property-level** (field):

```csharp
// Comes from attributes on LoginViewModel, OR manually:
ModelState.AddModelError("Password", "Password is invalid.");
```

Shown by:

```html
<span asp-validation-for="Password" class="text-danger"></span>
```

---

### Login page — actual markup

```html
@if (TempData["LoginError"] is string loginError)
{
    <div class="text-danger mb-3">@loginError</div>
}

<form asp-action="Login" method="post" class="col-md-5" novalidate>
    <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

    <span asp-validation-for="Email" class="text-danger"></span>
    <span asp-validation-for="Password" class="text-danger"></span>
</form>
```

**Why `ModelOnly` on Login?**
- Property messages (`Email is required.`, `Password is required.`, …) already appear **under each field** via `asp-validation-for`.
- Using `All` would **duplicate** those messages: once under the field and again in the top summary.
- `ModelOnly` keeps the top summary for general ModelState errors only (if added with `string.Empty`).

**Invalid email/password in this project today:**
- Not added with `ModelState.AddModelError` anymore.
- `AccountController` sets `TempData["LoginError"] = ex.UserMessage` and redirects (PRG).
- Login view shows that message in a separate `TempData` div above the form.
- So the auth failure message is **not** produced by `asp-validation-summary`; field validation still is.

**Interview example (if using ModelState for auth failure):**

```csharp
ModelState.AddModelError(string.Empty, "Invalid email or password.");
```

That **would** appear in `asp-validation-summary="ModelOnly"`.

---

### Actual `LoginViewModel` attributes → browser messages

```csharp
[Required(ErrorMessage = "{0} is required.")]
[EmailAddress(ErrorMessage = "{0} must be a valid email address.")]
[StringLength(256, ErrorMessage = "{0} cannot exceed {1} characters.")]
[Display(Name = "Email")]
[NoScriptTags]
public string Email { get; set; }

[Required(ErrorMessage = "{0} is required.")]
[MinLength(6, ErrorMessage = "{0} must be at least {1} characters.")]
[StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
[DataType(DataType.Password)]
[Display(Name = "Password")]
public string Password { get; set; }
```

| Attribute | Example message after `{0}` / `{1}` replace |
|---|---|
| Email `[Required]` | `Email is required.` |
| Email `[EmailAddress]` | `Email must be a valid email address.` |
| Email `[StringLength(256)]` | `Email cannot exceed 256 characters.` |
| Password `[Required]` | `Password is required.` |
| Password `[MinLength(6)]` | `Password must be at least 6 characters.` |
| Password `[StringLength(100)]` | `Password cannot exceed 100 characters.` |

These strings are **not hard-coded in Login.cshtml**. They come from attributes + `[Display(Name=...)]`.

---

### Complete flow

```text
LoginViewModel validation attribute
        ↓
ASP.NET Core model validation (on POST binding)
        ↓
ModelState dictionary
        ↓
Validation Tag Helpers
  - asp-validation-summary="ModelOnly"
  - asp-validation-for="Email" / "Password"
        ↓
Razor generates HTML (error <ul>/<span>, data-val-* on inputs)
        ↓
Browser displays messages
(+ optional client-side unobtrusive JS before submit)
```

---

### `asp-validation-summary="ModelOnly"` vs `asp-validation-for="Password"`

| Tag Helper | Shows |
|---|---|
| `asp-validation-summary="ModelOnly"` | General errors (`ModelState[""]`) at the top |
| `asp-validation-for="Password"` | Only errors for the `Password` key, next to that field |

Same for Email.

---

### Server-side vs client-side validation

**Server-side (always authoritative in this app)**  
POST Login → MVC validates `LoginViewModel` → fills ModelState → if invalid, `return View(model)` → Tag Helpers render errors.

**Client-side (UX only)**  
`Login.cshtml` includes:

```html
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

Which loads:

```html
jquery.validate.min.js
jquery.validate.unobtrusive.min.js
```

**Unobtrusive validation:** reads `data-val-*` attributes that Tag Helpers put on inputs from DataAnnotations, then jquery.validate blocks submit and shows messages without a round-trip.

**`data-val-*` examples (generated by `asp-for`, not hand-written):**
- `data-val="true"`
- `data-val-required="Email is required."`
- `data-val-email="Email must be a valid email address."`
- `data-val-length` / `data-val-length-max` from `[StringLength]`
- `data-val-minlength` from `[MinLength]`

**Who generates `data-val-*`?** ASP.NET Core Tag Helpers from ViewModel attributes — **not** JavaScript inventing the rules.

**Form has `novalidate`:** turns off browser’s native HTML5 validation UI so unobtrusive/jquery handles client UX consistently.

If JavaScript is disabled / bypassed → client validation does nothing → **server-side still runs**.

---

### Interview Q&A (use LoginViewModel / Login.cshtml)

1. **What does `asp-validation-summary="ModelOnly"` mean?**  
   Show only model-level ModelState errors, not Email/Password field errors.

2. **Difference between ModelOnly and All?**  
   ModelOnly = general only. All = general + every property error (can duplicate field messages).

3. **What does None mean?**  
   Summary does not list ModelState errors.  
   > Not used in this project.

4. **Why ModelOnly on Login instead of All?**  
   Field errors already under Email/Password via `asp-validation-for`. All would duplicate them at the top.

5. **What is a model-level validation error?**  
   Error keyed to `string.Empty`, e.g. `ModelState.AddModelError(string.Empty, ex.UserMessage)` on Employee/Task forms.

6. **What is a property-level validation error?**  
   Error for a property key like `Password`, from `[Required]` on `LoginViewModel.Password` or `AddModelError("Password", ...)`.

7. **`AddModelError("", msg)` vs `AddModelError("Password", msg)`?**  
   First → summary (ModelOnly/All). Second → `asp-validation-for="Password"`.

8. **Where does `asp-validation-for` get its message?**  
   From ModelState entry for that property name after validation.

9. **Who generates `Password is required.`?**  
   `[Required(ErrorMessage = "{0} is required.")]` + `[Display(Name = "Password")]` on `LoginViewModel`.

10. **Is the message hard-coded in the Razor View?**  
    No. Login.cshtml has no that string; attributes do.

11. **Does JavaScript generate the validation rule?**  
    No. Rules come from C# attributes → Tag Helpers emit `data-val-*` → unobtrusive JS reads them.

12. **What if JavaScript is disabled?**  
    No client-side checks; form posts; server validates.

13. **Does server-side validation still happen?**  
    Yes. Always on POST when model binding/validation runs.

14. **If client-side is bypassed?**  
    Server ModelState still invalid for empty Password, etc.

15. **Remove `asp-validation-for`?**  
    ModelState still has errors; user may not see them next to the field (summary ModelOnly won’t show property errors).

16. **Remove `asp-validation-summary`?**  
    Model-level ModelState errors won’t show in that block (Login auth failure still can show via TempData div).

17. **Change ModelOnly to All?**  
    Top summary also lists Email/Password attribute errors (duplicate UI).

18. **Why do model-level errors appear at the top?**  
    That’s where the summary Tag Helper is placed in the form.

19. **Why does Password error appear below Password?**  
    `<span asp-validation-for="Password">` is under that input.

20. **Role of `_ValidationScriptsPartial`?**  
    Loads jquery.validate + unobtrusive for client-side validation.

21. **What is unobtrusive validation?**  
    JS that wires `data-val-*` to jquery.validate without inline onclick scripts.

22. **What are `data-val-*` attributes?**  
    HTML attributes encoding validation rules/messages for the client.

23. **Who generates `data-val-*`?**  
    Input Tag Helper from DataAnnotations on the ViewModel.

24. **How does `[Required]` become a browser message?**  
    Server: ModelState → validation-for span. Client: `data-val-required="Password is required."` → unobtrusive shows it.

25. **How does `[StringLength]` become a browser message?**  
    Same path with length `data-val-*` / ModelState message  
    e.g. `Password cannot exceed 100 characters.` from  
    `[StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]`.

---

*End of interview preparation document — aligned to the current BstSolutions codebase.*
