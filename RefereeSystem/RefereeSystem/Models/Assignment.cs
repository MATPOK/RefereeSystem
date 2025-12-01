using System;
using System.Collections.Generic;

namespace RefereeSystem.Models;

public partial class Assignment
{
    public int Id { get; set; }

    public int MatchId { get; set; }

    public int RefereeId { get; set; }

    public string Function { get; set; } = null!;

    public virtual Match Match { get; set; } = null!;

    public virtual User Referee { get; set; } = null!;
}
