namespace Hermes.Models;

public enum TicketSort
{
    CreatedAt = 0,
    Priority = 1
}

/// <summary>RF05.2 and RF05.3 — filtering and ordering options for the ticket list.</summary>
public class TicketFilter
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public TicketStatus? Status { get; set; }

    /// <summary>Filters by the collaborator who raised the ticket.</summary>
    public string? CollaboratorId { get; set; }

    public TicketSort Sort { get; set; } = TicketSort.CreatedAt;

    public bool Descending { get; set; } = true;

    /// <summary>Set for a non-admin, so they only ever see their own tickets.</summary>
    public string? RestrictToUserId { get; set; }
}
