using Control_De_Tareas.Data;
using Control_De_Tareas.Data.Entitys;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Configuración de la base de datos
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar el DbSeeder
builder.Services.AddTransient<DbSeeder>();

// ========== CONFIGURACIÓN DE AUTENTICACIÓN Y AUTORIZACIÓN ==========
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Error/Error403";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("Profesor", policy => policy.RequireRole("Profesor"));
    options.AddPolicy("Estudiante", policy => policy.RequireRole("Estudiante"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ¡IMPORTANTE! Authentication ANTES de Authorization
app.UseAuthentication();
app.UseAuthorization();

// Manejar errores 403/404
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Ejecutar el seeder al iniciar la aplicación (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync();
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();