namespace BoxWebhookShared.Domain.Entities;

/// <summary>
/// Domain entity representing a Box webhook.
/// </summary>
public class WebhookEntity
{
    public string Id { get; }
    public string TargetId { get; }
    public WebhookTargetType TargetType { get; }
    public string Address { get; }
    public IReadOnlyList<WebhookTrigger> Triggers { get; }
    public DateTime? CreatedAt { get; }
    public string? CreatedByName { get; }
    public string? CreatedByEmail { get; }

    public WebhookEntity(
        string id,
        string targetId,
        WebhookTargetType targetType,
        string address,
        IReadOnlyList<WebhookTrigger> triggers,
        DateTime? createdAt = null,
        string? createdByName = null,
        string? createdByEmail = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Webhook ID cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(targetId))
            throw new ArgumentException("Target ID cannot be empty", nameof(targetId));
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty", nameof(address));
        if (triggers == null || triggers.Count == 0)
            throw new ArgumentException("At least one trigger is required", nameof(triggers));

        Id = id;
        TargetId = targetId;
        TargetType = targetType;
        Address = address;
        Triggers = triggers;
        CreatedAt = createdAt;
        CreatedByName = createdByName;
        CreatedByEmail = createdByEmail;
    }
}

/// <summary>
/// Minimal webhook information (from list operations).
/// </summary>
public class WebhookSummary
{
    public string Id { get; }
    public string? TargetId { get; }
    public string? TargetType { get; }

    public WebhookSummary(string id, string? targetId, string? targetType)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        TargetId = targetId;
        TargetType = targetType;
    }
}
