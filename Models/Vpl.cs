using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Vpl
{
    public int IdVpl { get; set; }

    public string Annee { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Boite { get; set; } = null!;

    public string Logement { get; set; } = null!;

    public string Document { get; set; } = null!;

    public string? NumDossierVpl { get; set; } = null!;
    public string DateDocument { get; set; } = null!;

    public string Client { get; set; } = null!;

    public string? Fonctions { get; set; }

    public string? Adresse { get; set; }

    public string? Contact { get; set; }

    public string? Ville { get; set; }

    public string? CommuneQuartier { get; set; }

    public string? Cote { get; set; }

    public string Lien { get; set; } = null!;

    public int? IdOpe { get; set; }

    public virtual ICollection<HistoriqueVpl> HistoriqueVpls { get; set; } = new List<HistoriqueVpl>();

    public virtual Operation? IdOpeNavigation { get; set; }
}
