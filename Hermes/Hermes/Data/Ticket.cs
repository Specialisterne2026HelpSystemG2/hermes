using Hermes.Models;

namespace Hermes.Data;

public class Ticket
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public TicketPriority Priority { get; set; } = TicketPriority.Normal;

    /// <summary>The collaborator who raised the ticket. RF07.2 keys off this.</summary>
    public string? CreatedById { get; set; }

    public ApplicationUser? CreatedBy { get; set; }

    public string? AssignedToId { get; set; }

    public ApplicationUser? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public ICollection<TicketReply> Replies { get; set; } = new List<TicketReply>();
}
