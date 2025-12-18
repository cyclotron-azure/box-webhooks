using BoxWebhookShared.Domain.Entities;

namespace BoxWebhookShared.Application.DTOs;

/// <summary>
/// Data Transfer Object for webhook creation requests.
/// </summary>
public record CreateWebhookRequest(
    string FolderId,
    string WebhookUrl,
    IReadOnlyList<WebhookTrigger> Triggers);
