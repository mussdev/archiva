using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Carte
{
    public int IdCarte { get; set; }

    public string Tube { get; set; } = null!;

    public string? Ville { get; set; }

    public string? Quartier { get; set; }

    public string Operation { get; set; } = null!;

    public string Legende { get; set; } = null!;

    public string? Originalite { get; set; }

    public string? Echelle { get; set; }

    public string DateCarte { get; set; } = null!;

    public string? Cote { get; set; }

    public string Lien { get; set; } = null!;

    public int? IdOpe { get; set; }

    public string? NumDossierCarte { get; set; }

    public virtual ICollection<HistoriqueCarte> HistoriqueCartes { get; set; } = new List<HistoriqueCarte>();

    public virtual Operation? IdOpeNavigation { get; set; }
}
