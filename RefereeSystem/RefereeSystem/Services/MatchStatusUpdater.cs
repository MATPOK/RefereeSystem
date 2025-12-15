using Microsoft.EntityFrameworkCore;
using RefereeSystem.Models;

namespace RefereeSystem.Services
{
    // BackgroundService to wbudowana w .NET klasa do zadań cyklicznych
    public class MatchStatusUpdater : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MatchStatusUpdater> _logger;

        public MatchStatusUpdater(IServiceScopeFactory scopeFactory, ILogger<MatchStatusUpdater> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serwis aktualizacji statusów meczów uruchomiony.");

            // Pętla działająca dopóki aplikacja jest włączona
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateMatchesStatus();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji statusów meczów.");
                }

                // Czekaj 5 minut przed kolejnym sprawdzeniem (zmień wg uznania)
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task UpdateMatchesStatus()
        {
            // BackgroundService jest Singletonem (działa stale), a DbContext jest Scoped (na żądanie).
            // Musimy ręcznie stworzyć "zakres" (scope), żeby pobrać bazę danych.
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RefereeDbContext>();

                // 1. Obliczamy czas graniczny: Teraz minus 3 godziny
                var cutOffTime = DateTime.Now.AddHours(-3);

                // 2. Szukamy meczów, które:
                // - Data rozpoczęcia jest starsza niż 3h temu
                // - Status NIE JEST jeszcze "ODBYL_SIE"
                // - Status NIE JEST "ODWOLANY" ani "PRZERWANY" (żeby ich nie nadpisać!)
                var matchesToUpdate = await context.Matches
                    .Where(m => m.MatchDate < cutOffTime
                                && m.Status != "ODBYL_SIE"
                                && m.Status != "ODWOLANY"
                                && m.Status != "PRZERWANY")
                    .ToListAsync();

                if (matchesToUpdate.Any())
                {
                    foreach (var match in matchesToUpdate)
                    {
                        match.Status = "ODBYL_SIE";
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Zaktualizowano {matchesToUpdate.Count} meczów na status ODBYL_SIE.");
                }
            }
        }
    }
}