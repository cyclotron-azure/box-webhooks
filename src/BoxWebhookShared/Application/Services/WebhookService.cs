using BoxWebhookShared.Application.DTOs;
using BoxWebhookShared.Domain.Entities;
using BoxWebhookShared.Domain.Interfaces;

namespace BoxWebhookShared.Application.Services;

/// <summary>
/// Application service for webhook operations.
/// Orchestrates domain logic and repository operations.
/// Follows Single Responsibility principle (SRP).
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly IWebhookRepository _webhookRepository;

    public WebhookService(IWebhookRepository webhookRepository)
    {
        _webhookRepository = webhookRepository ?? throw new ArgumentNullException(nameof(webhookRepository));
    }

    public async Task<OperationResult<WebhookEntity>> CreateWebhookAsync(
        CreateWebhookRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.FolderId))
                return OperationResult<WebhookEntity>.Failure("Folder ID cannot be empty");

            if (string.IsNullOrWhiteSpace(request.WebhookUrl))
                return OperationResult<WebhookEntity>.Failure("Webhook URL cannot be empty");

            if (!request.WebhookUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return OperationResult<WebhookEntity>.Failure("Webhook URL must use HTTPS");

            if (request.Triggers == null || request.Triggers.Count == 0)
                return OperationResult<WebhookEntity>.Failure("At least one trigger is required");

            var webhook = await _webhookRepository.CreateAsync(
                request.FolderId,
                WebhookTargetType.Folder,
                request.WebhookUrl,
                request.Triggers,
                cancellationToken);

            return OperationResult<WebhookEntity>.Success(webhook);
        }
        catch (Exception ex)
        {
            return OperationResult<WebhookEntity>.Failure($"Failed to create webhook: {ex.Message}");
        }
    }

    public async Task<OperationResult<IReadOnlyList<WebhookEntity>>> GetAllWebhooksAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var summaries = await _webhookRepository.GetAllAsync(cancellationToken);
            var webhooks = new List<WebhookEntity>();

            foreach (var summary in summaries)
            {
                var fullWebhook = await _webhookRepository.GetByIdAsync(summary.Id, cancellationToken);
                if (fullWebhook != null)
                {
                    webhooks.Add(fullWebhook);
                }
            }

            return OperationResult<IReadOnlyList<WebhookEntity>>.Success(webhooks);
        }
        catch (Exception ex)
        {
            return OperationResult<IReadOnlyList<WebhookEntity>>.Failure($"Failed to list webhooks: {ex.Message}");
        }
    }

    public async Task<OperationResult<WebhookEntity>> GetWebhookByIdAsync(
        string webhookId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(webhookId))
                return OperationResult<WebhookEntity>.Failure("Webhook ID cannot be empty");

            var webhook = await _webhookRepository.GetByIdAsync(webhookId, cancellationToken);

            if (webhook == null)
                return OperationResult<WebhookEntity>.Failure($"Webhook with ID '{webhookId}' not found");

            return OperationResult<WebhookEntity>.Success(webhook);
        }
        catch (Exception ex)
        {
            return OperationResult<WebhookEntity>.Failure($"Failed to get webhook: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteWebhookAsync(
        string webhookId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(webhookId))
                return OperationResult.Failure("Webhook ID cannot be empty");

            var deleted = await _webhookRepository.DeleteAsync(webhookId, cancellationToken);

            if (!deleted)
                return OperationResult.Failure($"Failed to delete webhook with ID '{webhookId}'");

            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Failed to delete webhook: {ex.Message}");
        }
    }
}
