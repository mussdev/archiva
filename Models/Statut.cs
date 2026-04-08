using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Statut
{
    public int IdStatut { get; set; }

    public string CodeStatut { get; set; } = null!;

    public string DescriptionStatut { get; set; } = null!;

    public string? NoteStatut { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Adp> Adps { get; set; } = new List<Adp>();

    public virtual ICollection<Carte> Cartes { get; set; } = new List<Carte>();

    public virtual ICollection<Vpl> Vpls { get; set; } = new List<Vpl>();
}
