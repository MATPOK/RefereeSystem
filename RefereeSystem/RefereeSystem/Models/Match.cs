using System;
using System.Collections.Generic;

namespace RefereeSystem.Models;

public partial class Match
{
    public int Id { get; set; }

    public DateTime MatchDate { get; set; }

    public string HomeTeam { get; set; } = null!;

    public string AwayTeam { get; set; } = null!;

    public string Location { get; set; } = null!;

    public string? Status { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
