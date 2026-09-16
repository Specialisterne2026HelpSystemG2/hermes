using Hermes.Common;
using Hermes.Data;
using Hermes.Models;

namespace Hermes.Services;

public interface ITicketService
{
    /// <summary>RF05 — listing with filters and ordering.</summary>
    Task<IReadOnlyList<Ticket>> GetAllAsync(TicketFilter filter, CancellationToken ct = default);

    /// <summary>Includes the category, both users and the replies with their authors.</summary>
    Task<Ticket?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<OperationResult<Ticket>> CreateAsync(TicketFormModel model, string authorId, CancellationToken ct = default);

    /// <summary>RF04.2 — saves a reply against the ticket and notifies the other party.</summary>
    Task<OperationResult<TicketReply>> AddReplyAsync(
        int ticketId, ReplyFormModel model, string authorId, CancellationToken ct = default);

    /// <summary>
    /// RF07 — only the collaborator who raised the ticket may change its status,
    /// so the caller's id is required rather than taken on trust from the page.
    /// </summary>
    Task<OperationResult> ChangeStatusAsync(
        int ticketId, TicketStatus status, string requesterId, CancellationToken ct = default);

    Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default);
}
