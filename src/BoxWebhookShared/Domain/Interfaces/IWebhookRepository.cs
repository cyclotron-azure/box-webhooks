using BoxWebhookShared.Domain.Entities;

namespace BoxWebhookShared.Domain.Interfaces;

/// <summary>
/// Repository interface for webhook operations.
/// Follows the Repository pattern from DDD.
/// </summary>
public interface IWebhookRepository
{
    /// <summary>
    /// Creates a new webhook.
    /// </summary>
    Task<WebhookEntity> CreateAsync(
        string targetId,
        WebhookTargetType targetType,
        string address,
        IReadOnlyList<WebhookTrigger> triggers,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all webhooks (summary only).
    /// </summary>
    Task<IReadOnlyList<WebhookSummary>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a webhook by ID with full details.
    /// </summary>
    Task<WebhookEntity?> GetByIdAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook by ID.
    /// </summary>
    Task<bool> DeleteAsync(string webhookId, CancellationToken cancellationToken = default);
}
