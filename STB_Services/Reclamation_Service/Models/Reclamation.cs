using System;
using System.Collections.Generic;

namespace Reclamation_Service.Models;

public partial class Reclamation
{
    public int ReclamationId { get; set; }

    public int AccountId { get; set; }

    public string Subject { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime? ReclamationDate { get; set; }

    public string Status { get; set; } = null!;

    public int? AssignedAdminId { get; set; }

    public DateTime? AssignmentDate { get; set; }
}
