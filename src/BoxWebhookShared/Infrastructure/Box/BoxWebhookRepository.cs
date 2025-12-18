using global::Box.Sdk.Gen;
using global::Box.Sdk.Gen.Managers;
using global::Box.Sdk.Gen.Schemas;
using BoxWebhookShared.Domain.Entities;
using BoxWebhookShared.Domain.Interfaces;

namespace BoxWebhookShared.Infrastructure.Box;

/// <summary>
/// Box SDK implementation of the webhook repository.
/// Follows Liskov Substitution principle (LSP) - can replace interface anywhere.
/// </summary>
public class BoxWebhookRepository : IWebhookRepository
{
    private readonly BoxClient _client;

    public BoxWebhookRepository(BoxClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<WebhookEntity> CreateAsync(
        string targetId,
        WebhookTargetType targetType,
        string address,
        IReadOnlyList<WebhookTrigger> triggers,
        CancellationToken cancellationToken = default)
    {
        var target = new CreateWebhookRequestBodyTargetField
        {
            Id = targetId,
            Type = targetType == WebhookTargetType.Folder
                ? CreateWebhookRequestBodyTargetTypeField.Folder
                : CreateWebhookRequestBodyTargetTypeField.File
        };

        var sdkTriggers = triggers
            .Select(t => MapToSdkTrigger(t))
            .Select(t => new StringEnum<CreateWebhookRequestBodyTriggersField>(t))
            .ToArray();

        var requestBody = new CreateWebhookRequestBody(
            target: target,
            address: address,
            triggers: Array.AsReadOnly(sdkTriggers));

        var webhook = await _client.Webhooks.CreateWebhookAsync(requestBody: requestBody);

        return MapToEntity(webhook);
    }

    public async Task<IReadOnlyList<WebhookSummary>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var webhooks = await _client.Webhooks.GetWebhooksAsync();

        if (webhooks.Entries == null)
            return Array.Empty<WebhookSummary>();

        return webhooks.Entries
            .Select(w => new WebhookSummary(
                w.Id ?? string.Empty,
                w.Target?.Id,
                w.Target?.Type?.Value.ToString()))
            .ToList();
    }

    public async Task<WebhookEntity?> GetByIdAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        try
        {
            var webhook = await _client.Webhooks.GetWebhookByIdAsync(webhookId: webhookId);
            return MapToEntity(webhook);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.Webhooks.DeleteWebhookByIdAsync(webhookId: webhookId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static WebhookEntity MapToEntity(Webhook webhook)
    {
        var triggers = webhook.Triggers?
            .Select(t => WebhookTrigger.FromString(t.Value.ToString()!))
            .ToList() ?? new List<WebhookTrigger>();

        return new WebhookEntity(
            id: webhook.Id ?? string.Empty,
            targetId: webhook.Target?.Id ?? string.Empty,
            targetType: WebhookTargetType.FromString(webhook.Target?.Type?.Value.ToString() ?? "folder"),
            address: webhook.Address ?? string.Empty,
            triggers: triggers,
            createdAt: webhook.CreatedAt?.DateTime,
            createdByName: webhook.CreatedBy?.Name,
            createdByEmail: webhook.CreatedBy?.Login);
    }

    private static CreateWebhookRequestBodyTriggersField MapToSdkTrigger(WebhookTrigger trigger)
    {
        return trigger.Value switch
        {
            "FILE.UPLOADED" => CreateWebhookRequestBodyTriggersField.FileUploaded,
            "FILE.DOWNLOADED" => CreateWebhookRequestBodyTriggersField.FileDownloaded,
            "FILE.PREVIEWED" => CreateWebhookRequestBodyTriggersField.FilePreviewed,
            "FILE.TRASHED" => CreateWebhookRequestBodyTriggersField.FileTrashed,
            "FILE.DELETED" => CreateWebhookRequestBodyTriggersField.FileDeleted,
            "FILE.RESTORED" => CreateWebhookRequestBodyTriggersField.FileRestored,
            "FILE.COPIED" => CreateWebhookRequestBodyTriggersField.FileCopied,
            "FILE.MOVED" => CreateWebhookRequestBodyTriggersField.FileMoved,
            "FILE.LOCKED" => CreateWebhookRequestBodyTriggersField.FileLocked,
            "FILE.UNLOCKED" => CreateWebhookRequestBodyTriggersField.FileUnlocked,
            "FILE.RENAMED" => CreateWebhookRequestBodyTriggersField.FileRenamed,
            "FOLDER.CREATED" => CreateWebhookRequestBodyTriggersField.FolderCreated,
            "FOLDER.DOWNLOADED" => CreateWebhookRequestBodyTriggersField.FolderDownloaded,
            "FOLDER.TRASHED" => CreateWebhookRequestBodyTriggersField.FolderTrashed,
            "FOLDER.DELETED" => CreateWebhookRequestBodyTriggersField.FolderDeleted,
            "FOLDER.RESTORED" => CreateWebhookRequestBodyTriggersField.FolderRestored,
            "FOLDER.COPIED" => CreateWebhookRequestBodyTriggersField.FolderCopied,
            "FOLDER.MOVED" => CreateWebhookRequestBodyTriggersField.FolderMoved,
            "FOLDER.RENAMED" => CreateWebhookRequestBodyTriggersField.FolderRenamed,
            _ => CreateWebhookRequestBodyTriggersField.FileUploaded
        };
    }
}
