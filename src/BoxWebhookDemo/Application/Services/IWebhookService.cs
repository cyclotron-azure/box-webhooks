using BoxWebhookDemo.Application.DTOs;
using BoxWebhookDemo.Domain.Entities;

namespace BoxWebhookDemo.Application.Services;

/// <summary>
/// Application service interface for webhook operations.
/// Follows the Interface Segregation principle (ISP).
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Creates a new webhook on a folder.
    /// </summary>
    Task<OperationResult<WebhookEntity>> CreateWebhookAsync(
        CreateWebhookRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all webhooks with full details.
    /// </summary>
    Task<OperationResult<IReadOnlyList<WebhookEntity>>> GetAllWebhooksAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets webhook details by ID.
    /// </summary>
    Task<OperationResult<WebhookEntity>> GetWebhookByIdAsync(
        string webhookId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook.
    /// </summary>
    Task<OperationResult> DeleteWebhookAsync(
        string webhookId,
        CancellationToken cancellationToken = default);
}
