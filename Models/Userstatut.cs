using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Userstatut
{
    public int IdUserStatut { get; set; }

    public int? IdStatut { get; set; }

    public int? UserId { get; set; }

    public DateTime? CreatedAt { get; set; }
}
