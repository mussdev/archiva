using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Usersession
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public Guid SessionToken { get; set; }

    public DateTime DateConnexion { get; set; }

    public DateTime? DateDeconnexion { get; set; }

    public string? AdresseIp { get; set; }

    public bool? IsActive { get; set; }

    public virtual User User { get; set; } = null!;
}
