namespace RefereeSystem.Client.Models
{
    public class Match
    {
        public int Id { get; set; }
        public DateTime MatchDate { get; set; } = DateTime.Now; // Domyślnie dzisiaj
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = "ZAPLANOWANY";
    }
}