using MedicalDoctorRecommender.Data;
using MedicalDoctorRecommender.Services;
using MedicalDoctorRecommender.Services.Doctors;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =====================
// LOAD .env
// =====================
Env.Load();

// =====================
// SERVICES
// =====================

// MVC
builder.Services.AddControllersWithViews();

// DbContext (from .env)
var connectionString =
    Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? throw new Exception("DB_CONNECTION not found in .env");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// LLM + Doctors
builder.Services.AddHttpClient<GroqLLMService>();
builder.Services.AddScoped<DoctorRecommendationService>();

var app = builder.Build();

// =====================
// MIDDLEWARE
// =====================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session MUST come after routing
app.UseSession();

app.UseAuthorization();

// =====================
// ROUTING
// =====================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
