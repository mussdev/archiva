using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Operation
{
    public int IdOpe { get; set; }

    public string CodeOpe { get; set; } = null!;

    public string DescriptionOpe { get; set; } = null!;

    public int? IdVille { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Adp> Adps { get; set; } = new List<Adp>();

    public virtual ICollection<Carte> Cartes { get; set; } = new List<Carte>();

    public virtual Ville? IdVilleNavigation { get; set; }

    public virtual ICollection<Vpl> Vpls { get; set; } = new List<Vpl>();
}
