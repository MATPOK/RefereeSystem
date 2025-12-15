using Microsoft.EntityFrameworkCore;
using RefereeSystem.Models;

namespace RefereeSystem.Services
{
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
            _logger.LogInformation("Serwis automatycznej aktualizacji meczów uruchomiony.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<RefereeDbContext>();

                        // Wywołanie procedury, która robi całą robotę w bazie
                        await context.Database.ExecuteSqlRawAsync("CALL auto_complete_old_matches()");

                        // Opcjonalnie: Logowanie tylko dla pewności (można usunąć, żeby nie śmiecić)
                        // _logger.LogInformation("Wywołano procedurę auto_complete_old_matches.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas wywoływania procedury auto_complete_old_matches.");
                }

                // Sprawdzaj co 5 minut (częstsze sprawdzanie nie jest konieczne)
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}