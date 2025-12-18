using BoxWebhookShared.Application.DTOs;
using BoxWebhookShared.Application.Services;
using BoxWebhookShared.Domain.Entities;

namespace BoxWebhookDemo.Presentation.ConsoleUI;

/// <summary>
/// Handles the console menu and user interactions.
/// Follows Single Responsibility principle (SRP).
/// </summary>
public class MenuHandler
{
    private readonly IWebhookService _webhookService;
    private readonly IFolderService _folderService;
    private readonly IConsoleIO _console;

    public MenuHandler(
        IWebhookService webhookService,
        IFolderService folderService,
        IConsoleIO console)
    {
        _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        _folderService = folderService ?? throw new ArgumentNullException(nameof(folderService));
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public async Task RunAsync()
    {
        while (true)
        {
            DisplayMenu();
            var choice = _console.ReadLine();

            switch (choice)
            {
                case "1":
                    await CreateWebhookAsync();
                    break;
                case "2":
                    await ListWebhooksAsync();
                    break;
                case "3":
                    await GetWebhookDetailsAsync();
                    break;
                case "4":
                    await DeleteWebhookAsync();
                    break;
                case "5":
                    await ListFoldersAsync();
                    break;
                case "6":
                    await CreateTestFolderAsync();
                    break;
                case "0":
                    _console.WriteLine("Goodbye!");
                    return;
                default:
                    _console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    private void DisplayMenu()
    {
        _console.WriteLine("\nSelect an operation:");
        _console.WriteLine("1. Create webhook on a folder");
        _console.WriteLine("2. List all webhooks");
        _console.WriteLine("3. Get webhook details");
        _console.WriteLine("4. Delete a webhook");
        _console.WriteLine("5. List folders (to find folder ID)");
        _console.WriteLine("6. Create a test folder");
        _console.WriteLine("0. Exit");
        _console.Write("\nChoice: ");
    }

    private async Task CreateWebhookAsync()
    {
        _console.Write("\nEnter Folder ID (use '0' for root folder): ");
        var folderId = _console.ReadLine();

        if (string.IsNullOrWhiteSpace(folderId))
        {
            _console.WriteLine("Folder ID cannot be empty");
            return;
        }

        _console.Write("Enter Webhook URL (must be HTTPS, port 443): ");
        var webhookUrl = _console.ReadLine();

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _console.WriteLine("Webhook URL cannot be empty");
            return;
        }

        DisplayTriggerOptions();
        _console.Write("\nSelect trigger (1-9): ");
        var triggerChoice = _console.ReadLine();

        var triggers = GetTriggersForChoice(triggerChoice);

        var request = new CreateWebhookRequest(folderId, webhookUrl, triggers);
        var result = await _webhookService.CreateWebhookAsync(request);

        if (result.IsSuccess)
        {
            var webhook = result.Value!;
            _console.WriteLine("\n✓ Webhook created successfully!");
            _console.WriteLine($"  Webhook ID: {webhook.Id}");
            _console.WriteLine($"  Target: {webhook.TargetType} (ID: {webhook.TargetId})");
            _console.WriteLine($"  Address: {webhook.Address}");
            _console.WriteLine($"  Triggers: {string.Join(", ", webhook.Triggers.Select(t => t.Value))}");
            _console.WriteLine($"  Created At: {webhook.CreatedAt}");
        }
        else
        {
            _console.WriteLine($"\n✗ {result.Error}");
        }
    }

    private void DisplayTriggerOptions()
    {
        _console.WriteLine("\nAvailable triggers for folders:");
        _console.WriteLine("1. FILE.UPLOADED - New file uploaded");
        _console.WriteLine("2. FILE.DOWNLOADED - File downloaded");
        _console.WriteLine("3. FILE.PREVIEWED - File previewed");
        _console.WriteLine("4. FILE.TRASHED - File moved to trash");
        _console.WriteLine("5. FILE.DELETED - File permanently deleted");
        _console.WriteLine("6. FILE.COPIED - File copied");
        _console.WriteLine("7. FILE.MOVED - File moved");
        _console.WriteLine("8. FOLDER.CREATED - Subfolder created");
        _console.WriteLine("9. All file events (UPLOADED, DOWNLOADED, PREVIEWED)");
    }

    private static IReadOnlyList<WebhookTrigger> GetTriggersForChoice(string? choice)
    {
        return choice switch
        {
            "1" => new[] { WebhookTrigger.FileUploaded },
            "2" => new[] { WebhookTrigger.FileDownloaded },
            "3" => new[] { WebhookTrigger.FilePreviewed },
            "4" => new[] { WebhookTrigger.FileTrashed },
            "5" => new[] { WebhookTrigger.FileDeleted },
            "6" => new[] { WebhookTrigger.FileCopied },
            "7" => new[] { WebhookTrigger.FileMoved },
            "8" => new[] { WebhookTrigger.FolderCreated },
            "9" => WebhookTrigger.AllFileAccessEvents.ToArray(),
            _ => new[] { WebhookTrigger.FileUploaded }
        };
    }

    private async Task ListWebhooksAsync()
    {
        var result = await _webhookService.GetAllWebhooksAsync();

        if (!result.IsSuccess)
        {
            _console.WriteLine($"\n✗ {result.Error}");
            return;
        }

        var webhooks = result.Value!;

        if (webhooks.Count == 0)
        {
            _console.WriteLine("\nNo webhooks found.");
            return;
        }

        _console.WriteLine($"\n=== Found {webhooks.Count} webhook(s) ===\n");

        foreach (var webhook in webhooks)
        {
            _console.WriteLine($"Webhook ID: {webhook.Id}");
            _console.WriteLine($"  Target: {webhook.TargetType} (ID: {webhook.TargetId})");
            _console.WriteLine($"  Address: {webhook.Address}");
            _console.WriteLine($"  Triggers: {string.Join(", ", webhook.Triggers.Select(t => t.Value))}");
            _console.WriteLine(string.Empty);
        }
    }

    private async Task GetWebhookDetailsAsync()
    {
        _console.Write("\nEnter Webhook ID: ");
        var webhookId = _console.ReadLine();

        if (string.IsNullOrWhiteSpace(webhookId))
        {
            _console.WriteLine("Webhook ID cannot be empty");
            return;
        }

        var result = await _webhookService.GetWebhookByIdAsync(webhookId);

        if (!result.IsSuccess)
        {
            _console.WriteLine($"\n✗ {result.Error}");
            return;
        }

        var webhook = result.Value!;
        _console.WriteLine($"\n=== Webhook Details ===");
        _console.WriteLine($"ID: {webhook.Id}");
        _console.WriteLine($"Target Type: {webhook.TargetType}");
        _console.WriteLine($"Target ID: {webhook.TargetId}");
        _console.WriteLine($"Address: {webhook.Address}");
        _console.WriteLine($"Triggers: {string.Join(", ", webhook.Triggers.Select(t => t.Value))}");
        _console.WriteLine($"Created At: {webhook.CreatedAt}");
        _console.WriteLine($"Created By: {webhook.CreatedByName} ({webhook.CreatedByEmail})");
    }

    private async Task DeleteWebhookAsync()
    {
        _console.Write("\nEnter Webhook ID to delete: ");
        var webhookId = _console.ReadLine();

        if (string.IsNullOrWhiteSpace(webhookId))
        {
            _console.WriteLine("Webhook ID cannot be empty");
            return;
        }

        _console.Write("Are you sure you want to delete this webhook? (y/n): ");
        var confirm = _console.ReadLine();

        if (confirm?.ToLower() != "y")
        {
            _console.WriteLine("Deletion cancelled.");
            return;
        }

        var result = await _webhookService.DeleteWebhookAsync(webhookId);

        if (result.IsSuccess)
        {
            _console.WriteLine("\n✓ Webhook deleted successfully!");
        }
        else
        {
            _console.WriteLine($"\n✗ {result.Error}");
        }
    }

    private async Task ListFoldersAsync()
    {
        _console.Write("\nEnter parent Folder ID (use '0' for root): ");
        var folderId = _console.ReadLine() ?? "0";

        var result = await _folderService.GetFolderItemsAsync(folderId);

        if (!result.IsSuccess)
        {
            _console.WriteLine($"\n✗ {result.Error}");
            return;
        }

        var items = result.Value!;

        if (items.Count == 0)
        {
            _console.WriteLine("\nNo items found in this folder.");
            return;
        }

        _console.WriteLine($"\n=== Items in folder {folderId} ===\n");

        foreach (var item in items)
        {
            var typeLabel = item.ItemType switch
            {
                FolderItemType.Folder => "[Folder]",
                FolderItemType.File => "[File]  ",
                FolderItemType.WebLink => "[Link]  ",
                _ => "[???]   "
            };

            _console.WriteLine($"{typeLabel} ID: {item.Id} - Name: {item.Name}");
        }
    }

    private async Task CreateTestFolderAsync()
    {
        _console.Write("\nEnter folder name: ");
        var folderName = _console.ReadLine();

        _console.Write("Enter parent Folder ID (use '0' for root): ");
        var parentFolderId = _console.ReadLine() ?? "0";

        var request = new CreateFolderRequest(folderName ?? string.Empty, parentFolderId);
        var result = await _folderService.CreateFolderAsync(request);

        if (result.IsSuccess)
        {
            var folder = result.Value!;
            _console.WriteLine($"\n✓ Folder created successfully!");
            _console.WriteLine($"  Folder ID: {folder.Id}");
            _console.WriteLine($"  Name: {folder.Name}");
            _console.WriteLine($"  Created At: {folder.CreatedAt}");
            _console.WriteLine("\nYou can now create a webhook on this folder using its ID.");
        }
        else
        {
            _console.WriteLine($"\n✗ {result.Error}");
        }
    }
}
