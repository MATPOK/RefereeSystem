using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage; // To naprawia Twój b³¹d
using Microsoft.AspNetCore.Components.Authorization;
using RefereeSystem.Client;
using RefereeSystem.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 1. Konfiguracja HttpClient - Adres Twojego API
// To pozwala Clientowi "rozmawiaæ" z Serverem
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 2. Dodajemy LocalStorage (TO JEST TA BRAKUJ¥CA CZÊŒÆ)
builder.Services.AddBlazoredLocalStorage();

// 3. Dodajemy autoryzacjê i nasz CustomAuthStateProvider
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

await builder.Build().RunAsync();