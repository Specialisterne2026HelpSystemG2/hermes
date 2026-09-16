using Hermes.Data;

namespace Hermes.Services;

/// <summary>RF04.3 — delivery side of the reply notifications.</summary>
public interface INotificationService
{
    Task<IReadOnlyList<Notification>> GetForUserAsync(
        string userId, bool unreadOnly = false, CancellationToken ct = default);

    Task<int> CountUnreadAsync(string userId, CancellationToken ct = default);

    Task MarkAsReadAsync(int id, string userId, CancellationToken ct = default);

    Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);
}
