using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class User
{
    public int UserId { get; set; }

    public string NomUser { get; set; } = null!;

    public string PrenomUser { get; set; } = null!;

    public string Contact { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Pwd { get; set; } = null!;

    public int Actif { get; set; }

    public int GroupeId { get; set; }

    public virtual Groupe Groupe { get; set; } = null!;

    public virtual ICollection<Userstatut> Userstatuts { get; set; } = new List<Userstatut>();

    public virtual ICollection<HistoriqueAdp> HistoriqueAdps { get; set; } = new List<HistoriqueAdp>();

    public virtual ICollection<HistoriqueCarte> HistoriqueCartes { get; set; } = new List<HistoriqueCarte>();

    public virtual ICollection<HistoriqueVpl> HistoriqueVpls { get; set; } = new List<HistoriqueVpl>();
}
