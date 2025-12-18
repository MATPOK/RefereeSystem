using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // <--- 1. WAŻNE: Dodaj to

namespace RefereeSystem.Models
{
    public partial class Match
    {
        public int Id { get; set; }
        public DateTime MatchDate { get; set; }
        public int HomeTeamId { get; set; }
        [ForeignKey("HomeTeamId")]
        public Team HomeTeam { get; set; }
        public int AwayTeamId { get; set; }
        [ForeignKey("AwayTeamId")]
        public Team AwayTeam { get; set; }
        public string Location { get; set; } = null!;
        public string? Status { get; set; }

        // Relacja do Assignments (Obsad)
        // [JsonIgnore] sprawia, że API nie wymaga tego pola przy dodawaniu meczu
        // oraz nie próbuje go wysyłać w nieskończoność przy pobieraniu.
        [JsonIgnore] // <--- 2. WAŻNE: Dodaj to
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}