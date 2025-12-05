using BoxWebhookDemo.Domain.Entities;

namespace BoxWebhookDemo.Application.DTOs;

/// <summary>
/// Data Transfer Object for webhook creation requests.
/// </summary>
public record CreateWebhookRequest(
    string FolderId,
    string WebhookUrl,
    IReadOnlyList<WebhookTrigger> Triggers);
