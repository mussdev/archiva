using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Carte
{
    public int IdCarte { get; set; }

    public string Tube { get; set; } = null!;

    public string Ville { get; set; } = string.Empty;

    public string Quartier { get; set; } = string.Empty;

    public string Operation { get; set; } = null!;

    public string Legende { get; set; } = null!;

    public string Originalite { get; set; } = string.Empty;

    public string Echelle { get; set; } = string.Empty;

    public string DateCarte { get; set; } = null!;

    public string Cote { get; set; } = string.Empty;

    public string Lien { get; set; } = null!;

    public virtual ICollection<HistoriqueCarte> HistoriqueCartes { get; set; } = new List<HistoriqueCarte>();
}
