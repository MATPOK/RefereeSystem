using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefereeSystem.Models;

namespace RefereeSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchesController : ControllerBase
    {
        private readonly RefereeDbContext _context;

        public MatchesController(RefereeDbContext context)
        {
            _context = context;
        }

        // GET: api/matches (Pobierz listę)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatches()
        {
            // 1. Pobieramy mecze WRAZ z danymi drużyn (JOIN)
            var matches = await _context.Matches
                .Include(m => m.HomeTeam) // Pobierz dane z tabeli Teams
                .Include(m => m.AwayTeam) // Pobierz dane z tabeli Teams
                .OrderBy(m => m.MatchDate)
                .ToListAsync();

            // 2. Zamieniamy (mapujemy) obiekty bazy danych na proste DTO dla Frontendu
            var matchDtos = matches.Select(m => new MatchDto
            {
                Id = m.Id,
                MatchDate = m.MatchDate,
                Location = m.Location,
                Status = m.Status,
                // Tutaj wyciągamy nazwę z obiektu Team. 
                // Używamy "?.", żeby nie wywaliło błędu jak drużyna będzie nullem (choć nie powinna)
                HomeTeam = m.HomeTeam?.Name ?? "Nieznana",
                AwayTeam = m.AwayTeam?.Name ?? "Nieznana"
            }).ToList();

            return Ok(matchDtos);
        }

        // POST: api/matches (Dodaj mecz)
        [HttpPost]
        [Authorize(Roles = "Admin,Scheduler")]
        public async Task<ActionResult<MatchDto>> PostMatch(MatchDto matchDto)
        {
            // 1. Logika "Znajdź lub Stwórz" dla Gospodarza
            var homeTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Name == matchDto.HomeTeam);
            if (homeTeam == null)
            {
                homeTeam = new Team { Name = matchDto.HomeTeam}; 
                _context.Teams.Add(homeTeam);
            }

            // 2. Logika "Znajdź lub Stwórz" dla Gościa
            var awayTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Name == matchDto.AwayTeam);
            if (awayTeam == null)
            {
                awayTeam = new Team { Name = matchDto.AwayTeam};
                _context.Teams.Add(awayTeam);
            }

            // Zapisujemy nowe drużyny, żeby dostały swoje ID
            await _context.SaveChangesAsync();

            // 3. Tworzymy obiekt Mecz (Entity) używając ID drużyn
            var match = new Match
            {
                MatchDate = matchDto.MatchDate,
                Location = matchDto.Location,
                Status = string.IsNullOrEmpty(matchDto.Status) ? "ZAPLANOWANY" : matchDto.Status,
                HomeTeamId = homeTeam.Id, // Kluczowe: wiążemy po ID
                AwayTeamId = awayTeam.Id  // Kluczowe: wiążemy po ID
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            // Aktualizujemy ID w DTO, żeby zwrócić poprawny obiekt
            matchDto.Id = match.Id;

            return CreatedAtAction("GetMatches", new { id = match.Id }, matchDto);
        }

        // DELETE: api/matches/5 (Usuń mecz)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Scheduler")]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound();

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}