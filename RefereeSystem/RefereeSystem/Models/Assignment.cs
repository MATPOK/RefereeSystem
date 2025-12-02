using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; // <--- 1. Import

namespace RefereeSystem.Models
{
    public partial class Assignment
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int RefereeId { get; set; }
        public string Function { get; set; } = null!;

        [JsonIgnore] // <--- 2. Ignoruj powrót do Meczu
        public virtual Match Match { get; set; } = null!;

        [JsonIgnore] // <--- 3. Ignoruj powrót do Sędziego
        public virtual User Referee { get; set; } = null!;
    }
}