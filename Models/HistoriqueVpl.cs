using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class HistoriqueVpl
{
    public int IdHistoriqueVpl { get; set; }

    public int IdVpl { get; set; }

    public int UserId { get; set; }

    public DateTime DateHisto { get; set; }

    public DateOnly DateVu { get; set; }

    public string? Commentaire { get; set; } = string.Empty;

    public string? TypeAction { get; set; } = string.Empty;

    public virtual Vpl IdVplNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
