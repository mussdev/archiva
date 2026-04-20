using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace anahged.Models;

public partial class Validationsfile
{
    public int IdValidation { get; set; }

    public int IdStatut { get; set; }
    [ForeignKey(nameof(AdpNavigation))]
    public int? IdAdp { get; set; }
    [ForeignKey(nameof(VplNavigation))]
    public int? IdVpl { get; set; }
    [ForeignKey(nameof(CarteNavigation))]
    public int? IdCarte { get; set; }

    public int UserId { get; set; }

    public DateTime? DateValidation { get; set; }

    public string? MotifRejet { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? TypeAction { get; set; }

    public string? Commentaire { get; set; }

    // Navigation properties
    public virtual Adp? AdpNavigation { get; set; }
    public virtual Carte? CarteNavigation { get; set; }
    public virtual Vpl? VplNavigation { get; set; }
    public virtual User? UserNavigation { get; set; } 
}
