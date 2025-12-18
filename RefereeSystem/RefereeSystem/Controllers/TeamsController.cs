using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefereeSystem.Models;


namespace RefereeSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly RefereeDbContext _context;

        public TeamsController(RefereeDbContext context)
        {
            _context = context;
        }

        // GET: api/teams
        // Zwraca prostą listę nazw zespołów, żeby wypełnić dropdown
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
        {
            return await _context.Teams.OrderBy(t => t.Name).ToListAsync();
        }

        [HttpPost]
        // [Authorize(Roles = "Admin,Scheduler")] // Odkomentuj jeśli chcesz zabezpieczyć
        public async Task<ActionResult<Team>> PostTeam(Team team)
        {
            if (string.IsNullOrWhiteSpace(team.Name))
                return BadRequest("Nazwa drużyny jest wymagana.");

            // Sprawdź czy już taka nie istnieje
            if (await _context.Teams.AnyAsync(t => t.Name == team.Name))
                return Conflict("Taka drużyna już istnieje.");

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTeams", new { id = team.Id }, team);
        }
    }
}