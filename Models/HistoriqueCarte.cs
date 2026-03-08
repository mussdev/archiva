using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class HistoriqueCarte
{
    public int IdHistoriqueCarte { get; set; }

    public int IdCarte { get; set; }

    public int UserId { get; set; }

    public DateTime DateHisto { get; set; }

    public DateOnly DateVu { get; set; }

    public string? Commentaire { get; set; } = string.Empty;

    public string? TypeAction { get; set; } = string.Empty;

    public virtual Carte IdCarteNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
