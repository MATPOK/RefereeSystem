using System.ComponentModel.DataAnnotations;

namespace RefereeSystem.Client.Models
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Imię jest wymagane")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // NOWE POLA
        [Required(ErrorMessage = "Miejscowość jest wymagana")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon jest wymagany")]
        [Phone(ErrorMessage = "Niepoprawny numer telefonu")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data urodzenia jest wymagana")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18); // Domyślnie ustawione na pełnoletność
    }
}