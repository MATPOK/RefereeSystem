namespace RefereeSystem.Models
{
    public class AssignmentDto
    {
        public int MatchId { get; set; }
        public int RefereeId { get; set; }
        public string Function { get; set; } = string.Empty;
    }
}