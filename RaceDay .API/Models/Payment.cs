using System;
using System.Collections.Generic;

namespace RaceDay_.API.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int EnrolmentId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public virtual Enrolment Enrolment { get; set; } = null!;
}
