using Microsoft.AspNetCore.Authentication.JwtBearer; // Do obs³ugi tokenów
using Microsoft.EntityFrameworkCore; // Do obs³ugi bazy danych
using Microsoft.IdentityModel.Tokens; // Do szyfrowania
using RefereeSystem.Components; // Twój g³ówny komponent Blazor
using RefereeSystem.Models; // Tutaj jest Twój RefereeDbContext
using System.Text;
using Blazored.LocalStorage;
using RefereeSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ====================================================
// 1. KONFIGURACJA US£UG (SERVICES)
// ====================================================

// A. Dodajemy obs³ugê kontrolerów API (To jest niezbêdne dla Backend-u!)
builder.Services.AddControllers();

// B. Konfiguracja Bazy Danych (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Nie znaleziono ConnectionString 'DefaultConnection' w appsettings.json");
}

builder.Services.AddDbContext<RefereeDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7292/")
});

// C. Konfiguracja Logowania (JWT)
var jwtKey = builder.Configuration.GetSection("AppSettings:Token").Value;
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("Nie znaleziono klucza JWT 'AppSettings:Token' w appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,   // Uproszczenie na potrzeby projektu studenckiego
            ValidateAudience = false, // Uproszczenie na potrzeby projektu studenckiego
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(); // Dodajemy autoryzacjê

// D. Konfiguracja Blazora (To ju¿ mia³eœ, zostawiamy bez zmian)
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// E. Dodajemy nasz serwis cykliczny do aktualizacji statusów meczów
builder.Services.AddHostedService<MatchStatusUpdater>();

var app = builder.Build();

// ====================================================
// 2. KONFIGURACJA PIPELINE (MIDDLEWARE)
// ====================================================

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Wa¿ne: Obs³uga plików statycznych (CSS, JS)
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();

// E. Uruchamiamy Logowanie i Autoryzacjê (Kolejnoœæ jest wa¿na!)
app.UseAuthentication(); // Kto to jest?
app.UseAuthorization();  // Co mo¿e robiæ?

// F. Mapowanie (Routing)

// Najpierw API (¿eby dzia³a³y kontrolery AuthController i AssignmentsController)
app.MapControllers();

// Potem Blazor (UI)
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(RefereeSystem.Client._Imports).Assembly);

app.Run();