using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Validationsfile
{
    public int IdValidation { get; set; }

    public int IdStatut { get; set; }

    public int? IdAdp { get; set; }

    public int? IdVpl { get; set; }

    public int? IdCarte { get; set; }

    public int UserId { get; set; }

    public DateTime? DateValidation { get; set; }

    public string? MotifRejet { get; set; }

    public DateTime? CreatedAt { get; set; }
}
