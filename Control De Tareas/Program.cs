using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Microsoft.EntityFrameworkCore;
using Control_De_Tareas.Services;
using Control_De_Tareas;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();

MapsterConfig.Configure();

builder.Services.AddDbContext<ContextDB>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // ← CORREGIDO
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Administrador"));
    options.AddPolicy("Profesor", policy =>
        policy.RequireRole("Profesor"));
    options.AddPolicy("Estudiante", policy =>
        policy.RequireRole("Estudiante"));
    options.AddPolicy("ProfesorOAdmin", policy =>
        policy.RequireRole("Profesor", "Administrador"));
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();