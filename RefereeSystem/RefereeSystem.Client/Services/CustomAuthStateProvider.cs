using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace RefereeSystem.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
        {
            _localStorage = localStorage;
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string token = await _localStorage.GetItemAsync<string>("authToken");

            // 1. Sprawdź czy token w ogóle istnieje
            if (string.IsNullOrEmpty(token))
            {
                return GenerateEmptyState();
            }

            // 2. NOWOŚĆ: Sprawdź czy token nie wygasł!
            if (IsTokenExpired(token))
            {
                // Jeśli wygasł, usuwamy go i wylogowujemy użytkownika
                await _localStorage.RemoveItemAsync("authToken");
                return GenerateEmptyState();
            }

            // 3. Jeśli wszystko OK, zaloguj
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        }

        // Metoda pomocnicza do zwracania stanu "wylogowany"
        private AuthenticationState GenerateEmptyState()
        {
            _http.DefaultRequestHeaders.Authorization = null;
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public void NotifyUserLogin(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            _http.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(authState);
        }

        // --- SPRAWDZANIE DATY WAŻNOŚCI (NOWA METODA) ---
        private bool IsTokenExpired(string token)
        {
            try
            {
                var claims = ParseClaimsFromJwt(token);
                var expClaim = claims.FirstOrDefault(c => c.Type == "exp");

                if (expClaim == null) return false; // Brak daty ważności = nieważny/podejrzany, albo uznajemy że wieczny

                // 'exp' w JWT to liczba sekund od 1970 roku (Unix Timestamp)
                var expSeconds = long.Parse(expClaim.Value);
                var expDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;

                // Jeśli data wygaśnięcia jest wcześniejsza niż "teraz", to token jest przeterminowany
                return expDate <= DateTime.UtcNow;
            }
            catch
            {
                return true; // Jeśli wystąpił błąd parsowania, uznajmy token za nieważny
            }
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

        // Nowa metoda publiczna do wywoływania ręcznego sprawdzenia
        public async Task CheckTokenExpiration()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrEmpty(token) && IsTokenExpired(token))
            {
                // Jeśli token wygasł -> wyloguj
                await Logout();
            }
        }

        // Metoda pomocnicza, żeby nie powielać kodu wylogowania
        private async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _http.DefaultRequestHeaders.Authorization = null;
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            NotifyAuthenticationStateChanged(authState);
        }

        // Nowa metoda pomocnicza do pobrania daty wygaśnięcia
        public async Task<DateTime> GetTokenExpirationAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrEmpty(token)) return DateTime.MinValue;

            try
            {
                var claims = ParseClaimsFromJwt(token);
                var exp = claims.FirstOrDefault(c => c.Type == "exp");
                if (exp == null) return DateTime.MinValue;

                var expSeconds = long.Parse(exp.Value);
                // Tokeny JWT używają czasu UTC
                return DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}