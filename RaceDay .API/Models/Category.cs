using System;
using System.Collections.Generic;

namespace RaceDay_.API.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public int EventId { get; set; }

    public string Name { get; set; } = null!;

    public decimal DistanceKm { get; set; }

    public decimal EntryFee { get; set; }

    public DateTime StartTime { get; set; }

    public virtual ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    public virtual Event Event { get; set; } = null!;
}
