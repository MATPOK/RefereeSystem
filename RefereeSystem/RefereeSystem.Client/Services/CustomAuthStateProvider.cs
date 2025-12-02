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

        // Ta metoda uruchamia się przy odświeżeniu strony (F5)
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Pobieramy token z pamięci przeglądarki
            string token = await _localStorage.GetItemAsync<string>("authToken");

            // Jeśli brak tokena -> Ustawiamy HttpClienta na "anonimowy" i zwracamy pusty stan
            if (string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = null;
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Jeśli jest token -> Doklejamy go do nagłówka i zwracamy stan "zalogowany"
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        }

        // ==========================================
        // METODY DO POWIADAMIANIA O ZMIANACH (LOGOWANIE / WYLOGOWANIE)
        // ==========================================

        public void NotifyUserLogin(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            // <<< NAPRAWA: To jest ta linijka, której brakowało! >>>
            // Natychmiast po zalogowaniu "uzbrajamy" HttpClienta w token.
            // Bez tego, pierwsze zapytanie po zalogowaniu poleci bez tokena (błąd 401).
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            // Powiadamiamy widoki (NavMenu, AuthorizeView), że stan się zmienił
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            // <<< NAPRAWA: Tutaj czyścimy nagłówek >>>
            // Żeby po wylogowaniu nikt nie mógł wysłać zapytania ze starym tokenem
            _http.DefaultRequestHeaders.Authorization = null;

            NotifyAuthenticationStateChanged(authState);
        }

        // ==========================================
        // POMOCNICZE (Parsowanie Tokena JWT)
        // ==========================================
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            // To proste parsowanie wyciąga role i inne dane z tokena
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
    }
}