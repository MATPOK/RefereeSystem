using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; // <--- 1. WAŻNE: Dodaj to

namespace RefereeSystem.Models
{
    public partial class Match
    {
        public int Id { get; set; }
        public DateTime MatchDate { get; set; }
        public string HomeTeam { get; set; } = null!;
        public string AwayTeam { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string? Status { get; set; }

        // Relacja do Assignments (Obsad)
        // [JsonIgnore] sprawia, że API nie wymaga tego pola przy dodawaniu meczu
        // oraz nie próbuje go wysyłać w nieskończoność przy pobieraniu.
        [JsonIgnore] // <--- 2. WAŻNE: Dodaj to
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}