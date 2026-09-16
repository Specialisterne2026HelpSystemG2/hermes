using Hermes.Common;
using Hermes.Data;
using Hermes.Models;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Services;

public class TicketService(HermesContext context) : ITicketService
{
    public async Task<IReadOnlyList<Ticket>> GetAllAsync(TicketFilter filter, CancellationToken ct = default)
    {
        var query = context.Tickets
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.RestrictToUserId))
        {
            query = query.Where(t =>
                t.CreatedById == filter.RestrictToUserId ||
                t.AssignedToId == filter.RestrictToUserId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term));
        }

        if (filter.CategoryId is > 0)
        {
            query = query.Where(t => t.CategoryId == filter.CategoryId);
        }

        if (filter.Status is not null)
        {
            query = query.Where(t => t.Status == filter.Status);
        }

        if (!string.IsNullOrWhiteSpace(filter.CollaboratorId))
        {
            query = query.Where(t => t.CreatedById == filter.CollaboratorId);
        }

        query = (filter.Sort, filter.Descending) switch
        {
            (TicketSort.Priority, true) => query.OrderByDescending(t => t.Priority).ThenByDescending(t => t.CreatedAt),
            (TicketSort.Priority, false) => query.OrderBy(t => t.Priority).ThenByDescending(t => t.CreatedAt),
            (_, true) => query.OrderByDescending(t => t.CreatedAt),
            (_, false) => query.OrderBy(t => t.CreatedAt)
        };

        return await query.ToListAsync(ct);
    }

    public async Task<Ticket?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await context.Tickets
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Replies.OrderBy(r => r.CreatedAt))
                .ThenInclude(r => r.Author)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<OperationResult<Ticket>> CreateAsync(
        TicketFormModel model, string authorId, CancellationToken ct = default)
    {
        var categoryExists = await context.Categories.AnyAsync(c => c.Id == model.CategoryId, ct);
        if (!categoryExists)
        {
            return OperationResult<Ticket>.FieldFailure(
                nameof(TicketFormModel.CategoryId), "Choose a valid category.");
        }

        var ticket = new Ticket
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            CategoryId = model.CategoryId!.Value,
            Priority = model.Priority ?? TicketPriority.Normal,
            Status = TicketStatus.Open,
            CreatedById = authorId,
            AssignedToId = string.IsNullOrWhiteSpace(model.AssignedToId) ? null : model.AssignedToId,
            CreatedAt = DateTime.UtcNow
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync(ct);

        return OperationResult<Ticket>.Success(ticket);
    }

    public async Task<OperationResult<TicketReply>> AddReplyAsync(
        int ticketId, ReplyFormModel model, string authorId, CancellationToken ct = default)
    {
        var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId, ct);
        if (ticket is null)
        {
            return OperationResult<TicketReply>.Failure("Ticket not found.");
        }

        if (ticket.Status == TicketStatus.Closed)
        {
            return OperationResult<TicketReply>.Failure("This ticket is closed and cannot receive replies.");
        }

        var reply = new TicketReply
        {
            TicketId = ticketId,
            Body = model.Body.Trim(),
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow
        };

        context.TicketReplies.Add(reply);
        ticket.UpdatedAt = DateTime.UtcNow;

        // RF04.3 — notify whoever is on the other side of the conversation.
        var authorName = await context.Users
            .Where(u => u.Id == authorId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync(ct) ?? "Someone";

        foreach (var recipientId in Recipients(ticket, authorId))
        {
            context.Notifications.Add(new Notification
            {
                UserId = recipientId,
                TicketId = ticketId,
                Message = $"{authorName} replied to \"{ticket.Title}\".",
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(ct);

        return OperationResult<TicketReply>.Success(reply);
    }

    public async Task<OperationResult> ChangeStatusAsync(
        int ticketId, TicketStatus status, string requesterId, CancellationToken ct = default)
    {
        var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId, ct);
        if (ticket is null)
        {
            return OperationResult.Failure("Ticket not found.");
        }

        // RF07.2 — enforced here, not only by hiding the control in the page.
        if (ticket.CreatedById != requesterId)
        {
            return OperationResult.Failure(
                "Only the collaborator who raised this ticket can change its status.");
        }

        if (ticket.Status == status)
        {
            return OperationResult.Success();
        }

        ticket.Status = status;
        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.ResolvedAt = status is TicketStatus.Resolved or TicketStatus.Closed
            ? DateTime.UtcNow
            : null;

        await context.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (ticket is null)
        {
            return OperationResult.Failure("Ticket not found.");
        }

        context.Tickets.Remove(ticket);
        await context.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    private static IEnumerable<string> Recipients(Ticket ticket, string authorId)
    {
        if (!string.IsNullOrWhiteSpace(ticket.CreatedById) && ticket.CreatedById != authorId)
        {
            yield return ticket.CreatedById;
        }

        if (!string.IsNullOrWhiteSpace(ticket.AssignedToId) &&
            ticket.AssignedToId != authorId &&
            ticket.AssignedToId != ticket.CreatedById)
        {
            yield return ticket.AssignedToId;
        }
    }
}
