using BstSolutions.Data;
using BstSolutions.Middleware;
using BstSolutions.Repositories;
using BstSolutions.Repositories.Interfaces;
using BstSolutions.Services;
using BstSolutions.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "TaskManagement.Auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<BstSolutions.Services.Interfaces.IAuthenticationService, AuthenticationService>();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
