using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefereeSystem.Models;

namespace RefereeSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Opcjonalnie: Odkomentuj, jeśli chcesz, żeby tylko zalogowani widzieli mecze
    public class MatchesController : ControllerBase
    {
        private readonly RefereeDbContext _context;

        public MatchesController(RefereeDbContext context)
        {
            _context = context;
        }

        // GET: api/matches (Pobierz listę)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Match>>> GetMatches()
        {
            return await _context.Matches.OrderBy(m => m.MatchDate).ToListAsync();
        }

        // POST: api/matches (Dodaj mecz)
        [HttpPost]
        [Authorize(Roles = "Admin,Scheduler")] // Tylko Admin i Obsadowca mogą dodawać
        public async Task<ActionResult<Match>> PostMatch(Match match)
        {
            // Prosta walidacja statusu
            if (string.IsNullOrEmpty(match.Status)) match.Status = "ZAPLANOWANY";

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMatches", new { id = match.Id }, match);
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