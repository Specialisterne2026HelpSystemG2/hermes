namespace Hermes.Data;

public class TicketReply
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    public string Body { get; set; } = string.Empty;

    public string? AuthorId { get; set; }

    public ApplicationUser? Author { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
