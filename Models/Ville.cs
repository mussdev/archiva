using System;
using System.Collections.Generic;

namespace anahged.Models;

public partial class Ville
{
    public int IdVille { get; set; }

    public string CodeVille { get; set; } = null!;

    public string DescriptionVille { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Operation> Operations { get; set; } = new List<Operation>();
}
