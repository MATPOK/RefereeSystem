using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RefereeSystem.Helper; // Tu jest Twoj folder Helper
using RefereeSystem.Models; // Tu sa Twoje modele
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RefereeSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RefereeDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(RefereeDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // 1. Szukamy użytkownika po emailu
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return BadRequest("Nie znaleziono użytkownika.");
            }

            // 2. Weryfikujemy hasło (korzystamy z Twojego PasswordHelpera)
            if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                return BadRequest("Błędne hasło.");
            }

            // 3. Generujemy Token JWT
            var token = CreateToken(user);

            // Zwracamy token oraz rolę (przydatne dla frontendu)
            return Ok(new { token, role = user.Role, userId = user.Id });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Sprawdzamy czy email już istnieje
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Taki email już istnieje.");
            }

            // Hashujemy hasło
            user.PasswordHash = PasswordHelper.HashPassword(user.PasswordHash);

            // Ustawiamy domyślną rolę jeśli pusta (opcjonalnie)
            if (string.IsNullOrEmpty(user.Role)) user.Role = "Referee";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("Użytkownik dodany.");
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Pobieramy klucz z appsettings.json
            var secretKey = _configuration.GetSection("AppSettings:Token").Value;

            // Zabezpieczenie na wypadek braku klucza
            if (string.IsNullOrEmpty(secretKey))
                throw new Exception("Brak klucza JWT w appsettings.json!");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}