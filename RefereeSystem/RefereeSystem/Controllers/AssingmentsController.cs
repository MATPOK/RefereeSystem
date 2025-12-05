using User = RefereeSystem.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefereeSystem.Models;
using System.Security.Claims;


namespace RefereeSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssignmentsController : ControllerBase
    {
        private readonly RefereeDbContext _context;

        public AssignmentsController(RefereeDbContext context)
        {
            _context = context;
        }

        // 1. Pobierz listę tylko Sędziów (do dropdowna)
        [HttpGet("referees")]
        public async Task<ActionResult<IEnumerable<User>>> GetReferees()
        {
            return await _context.Users
                .Where(u => u.Role == "Referee") // Pobieramy tylko sędziów
                .Select(u => new User
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    City = u.City
                }) // Pobieramy tylko potrzebne pola
                .ToListAsync();
        }

        // 2. Przypisz sędziego (Wywołanie PROCEDURY SKŁADOWANEJ)
        [HttpPost]
        [Authorize(Roles = "Admin,Scheduler")]
        public async Task<IActionResult> AssignReferee(int matchId, int refereeId, string function)
        {
            try
            {
                // To jest kluczowy moment projektu!
                // Wywołujemy procedurę SQL, która sprawdzi konflikty terminów.
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL assign_referee_safe({0}, {1}, {2})",
                    matchId, refereeId, function);

                return Ok(new { message = "Sędzia przypisany pomyślnie." });
            }
            catch (Exception ex)
            {
                // Jeśli procedura SQL rzuci błąd (np. "Konflikt terminów"), 
                // łapiemy go tutaj i wysyłamy do Frontendu.

                // Postgres często pakuje błąd głębiej w InnerException
                var msg = ex.InnerException?.Message ?? ex.Message;

                // Usuwamy techniczne prefixy Postgresa, żeby komunikat był ładny
                if (msg.Contains("P0001")) // Kod błędu własnego w Postgres
                {
                    msg = msg.Split(':')[1].Trim(); // Bierzemy tylko treść błędu
                }

                return BadRequest(new { error = msg });
            }
        }

        // 1. Pobierz obsadę dla konkretnego meczu
        [HttpGet("by-match/{matchId}")]
        public async Task<ActionResult<IEnumerable<AssignmentDto>>> GetAssignmentsForMatch(int matchId)
        {
            var assignments = await _context.Assignments
                .Where(a => a.MatchId == matchId)
                .Include(a => a.Referee) // Dołączamy dane sędziego
                .Select(a => new AssignmentDto
                {
                    MatchId = a.MatchId,
                    RefereeId = a.RefereeId,
                    Function = a.Function
                })
                .ToListAsync();

            return Ok(assignments);
        }

        // 2. Metoda do USUWANIA starej obsady (potrzebne przy zmianie sędziego)
        [HttpDelete("{matchId}/{function}")]
        [Authorize(Roles = "Admin,Scheduler")]
        public async Task<IActionResult> RemoveAssignment(int matchId, string function)
        {
            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.MatchId == matchId && a.Function == function);

            if (assignment != null)
            {
                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpGet("my-matches")]
        [Authorize(Roles = "Referee,Admin,Scheduler")]
        public async Task<ActionResult<IEnumerable<RefereeMatchDto>>> GetMyMatches()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            // UŻYCIE PROCEDURY SKŁADOWANEJ (Funkcji SQL)
            // EF Core automatycznie zmapuje kolumny z SQL na właściwości klasy RefereeMatchDto
            var myMatches = await _context.Database
                .SqlQuery<RefereeMatchDto>($"SELECT * FROM get_my_matches({userId})")
                .ToListAsync();

            return Ok(myMatches);
        }
    }
}