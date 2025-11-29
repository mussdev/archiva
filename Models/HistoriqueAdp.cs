using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class HistoriqueAdp
{
    public int IdHistoriqueAdp { get; set; }

    public int IdAdp { get; set; }

    public int UserId { get; set; }

    public DateTime DateHisto { get; set; }

    public DateOnly DateVu { get; set; } = DateOnly.MinValue;

    public virtual Adp IdAdpNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
