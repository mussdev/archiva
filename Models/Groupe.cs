using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Groupe
{
    public int GroupeId { get; set; }

    public string NomGroupe { get; set; } = null!;

    public int VplAdp { get; set; }

    public int Carte { get; set; }

    public int VpladpCarte { get; set; }

    public int Superviseur { get; set; }

    public int Administrateur { get; set; }
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
