using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Userconnexionlog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime DateEvenement { get; set; }

    public string? AdresseIp { get; set; }

    public string TypeEvenement { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
