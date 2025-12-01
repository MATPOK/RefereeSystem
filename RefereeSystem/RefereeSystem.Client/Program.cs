using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RefereeSystem.Client;
using RefereeSystem.Client.Services; // Tu jest nasz AuthProvider

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 1. Konfiguracja HttpClient - Adres Twojego API
// UWAGA: Musi wskazywaæ na adres, pod którym uruchamia siê Twój Backend (zwykle localhost:7xxx)
// Pobieramy go dynamicznie z adresu, z którego za³adowano stronê.
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 2. Dodajemy LocalStorage (do zapisu tokena)
builder.Services.AddBlazoredLocalStorage();

// 3. Dodajemy autoryzacjê
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

await builder.Build().RunAsync();