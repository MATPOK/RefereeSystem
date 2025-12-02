// 1. KLUCZOWE: Alias, żeby system wiedział, że chodzi o Usera z Bazy Danych
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
    [Authorize(Roles = "Admin")] // Tylko Admin tu wejdzie!
    public class UsersController : ControllerBase
    {
        private readonly RefereeDbContext _context;

        public UsersController(RefereeDbContext context)
        {
            _context = context;
        }

        // Pobierz wszystkich użytkowników
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.OrderBy(u => u.LastName).ToListAsync();
        }

        // Zmień rolę użytkownika
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] string newRole)
        {
            // 1. Zabezpieczenie: Admin nie może zmienić roli sobie samemu
            // Pobieramy ID aktualnie zalogowanego admina z tokena
            var currentUserIdString = base.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(currentUserIdString, out int currentUserId))
            {
                if (currentUserId == id)
                {
                    return BadRequest("Nie możesz odebrać uprawnień samemu sobie! Poproś innego administratora.");
                }
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Walidacja ról
            if (newRole != "Admin" && newRole != "Scheduler" && newRole != "Referee" && newRole != "Guest")
            {
                return BadRequest("Nieprawidłowa rola.");
            }

            user.Role = newRole;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Zmieniono rolę na {newRole}" });
        }
    }
}