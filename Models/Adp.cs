using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Adp
{
    public int IdAdp { get; set; }

    public string Annee { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Boite { get; set; } = null!;

    public string Logement { get; set; } = null!;

    public string Document { get; set; } = null!;

    public string DateDocument { get; set; } = null!;

    public string Client { get; set; } = null!;

    public string Fonctions { get; set; } = string.Empty;

    public string Adresse { get; set; } = string.Empty;

    public string Contact { get; set; } = string.Empty;

    public string Ville { get; set; } = string.Empty;

    public string CommuneQuartier { get; set; } = string.Empty;

    public string Cote { get; set; } = string.Empty;

    public string Lien { get; set; } = null!;

    public virtual ICollection<HistoriqueAdp> HistoriqueAdps { get; set; } = new List<HistoriqueAdp>();
}
