namespace RefereeSystem.Client.Models
{
    public class RefereeMatchDto
    {
        public int MatchId { get; set; }
        public DateTime MatchDate { get; set; }
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Function { get; set; } = string.Empty;
    }
}