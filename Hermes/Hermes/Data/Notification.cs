namespace Hermes.Data;

/// <summary>RF04.3 — raised when a ticket receives a reply.</summary>
public class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
