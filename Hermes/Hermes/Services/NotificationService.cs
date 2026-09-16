using Hermes.Data;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Services;

public class NotificationService(HermesContext context) : INotificationService
{
    public async Task<IReadOnlyList<Notification>> GetForUserAsync(
        string userId, bool unreadOnly = false, CancellationToken ct = default)
    {
        var query = context.Notifications
            .AsNoTracking()
            .Include(n => n.Ticket)
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync(ct);
    }

    public Task<int> CountUnreadAsync(string userId, CancellationToken ct = default) =>
        context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public async Task MarkAsReadAsync(int id, string userId, CancellationToken ct = default)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);

        if (notification is null || notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        await context.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken ct = default)
    {
        await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }
}
