using Box.Sdk.Gen;
using BoxWebhookShared.Application.DTOs;
using BoxWebhookShared.Application.Services;
using BoxWebhookShared.Domain.Entities;
using BoxWebhookShared.Domain.Interfaces;
using BoxWebhookShared.Infrastructure.Box;
using DotNetEnv;

// Load .env file if it exists
Env.TraversePath().Load();

try
{
    if (args.Length == 0)
    {
        PrintUsage();
        return 1;
    }

    var command = args[0].ToLower();

    if (command == "--help" || command == "-h" || command == "help")
    {
        PrintUsage();
        return 0;
    }

    var exitCode = command switch
    {
        "create" => await HandleCreateAsync(args.Skip(1).ToArray()),
        "list" => await HandleListAsync(args.Skip(1).ToArray()),
        "get" => await HandleGetAsync(args.Skip(1).ToArray()),
        "delete" => await HandleDeleteAsync(args.Skip(1).ToArray()),
        "list-folders" => await HandleListFoldersAsync(args.Skip(1).ToArray()),
        _ => throw new ArgumentException($"Unknown command: {command}")
    };

    return exitCode;
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Error: {ex.Message}");
    return 1;
}

void PrintUsage()
{
    Console.WriteLine(@"
Box Webhook CLI Tool

USAGE:
    box-webhook-tool <COMMAND> [OPTIONS]

COMMANDS:
    create          Create a new webhook on a Box folder
    list            List all webhooks
    get             Get webhook details
    delete          Delete a webhook
    list-folders    List items in a Box folder

OPTIONS (for create):
    --folder-id <ID>    The Box folder ID to monitor
    --url <URL>         Webhook endpoint URL (must be HTTPS)
    --trigger <TYPE>    Webhook trigger (e.g., FILE.UPLOADED)
    --auth <METHOD>     (Optional) Authentication method: developer-token, ccg, jwt, oauth

OPTIONS (for list):
    --auth <METHOD>     (Optional) Authentication method

OPTIONS (for get):
    --webhook-id <ID>   The webhook ID
    --auth <METHOD>     (Optional) Authentication method

OPTIONS (for delete):
    --webhook-id <ID>   The webhook ID to delete
    --auth <METHOD>     (Optional) Authentication method
    --force             Skip confirmation prompt

OPTIONS (for list-folders):
    --folder-id <ID>    Folder ID (default: 0 for root)
    --auth <METHOD>     (Optional) Authentication method

AUTHENTICATION (Auto-Detected):
    If --auth is not specified, the tool auto-detects based on these environment variables:
    1. BOX_DEVELOPER_TOKEN              → Uses developer-token auth
    2. BOX_CLIENT_ID + BOX_CLIENT_SECRET → Uses CCG auth
    3. BOX_JWT_CONFIG_PATH              → Uses JWT auth
    4. BOX_AUTHORIZATION_CODE           → Uses OAuth auth

QUICK START:
    # With developer token (auto-detected, no --auth needed)
    export BOX_DEVELOPER_TOKEN=""your-token""
    box-webhook-tool list

    # With client credentials (auto-detected)
    export BOX_CLIENT_ID=""your-id""
    export BOX_CLIENT_SECRET=""your-secret""
    box-webhook-tool list

EXAMPLES:
    box-webhook-tool create --folder-id 12345 --url https://example.com/webhook --trigger FILE.UPLOADED
    box-webhook-tool list
    box-webhook-tool delete --webhook-id webhook-id-here --force

ENVIRONMENT VARIABLES:
    BOX_CLIENT_ID           Box application Client ID
    BOX_CLIENT_SECRET       Box application Client Secret
    BOX_ENTERPRISE_ID       Box enterprise ID (optional for CCG)
    BOX_DEVELOPER_TOKEN     Box developer token (for developer-token auth)
    BOX_JWT_CONFIG_PATH     Path to JWT config file (for jwt auth)
    BOX_AUTHORIZATION_CODE  OAuth authorization code (for oauth auth)
");
}

async Task<int> HandleCreateAsync(string[] cmdArgs)
{
    var folderId = GetArgValue(cmdArgs, "--folder-id") ?? throw new ArgumentException("--folder-id is required");
    var url = GetArgValue(cmdArgs, "--url") ?? throw new ArgumentException("--url is required");
    var trigger = GetArgValue(cmdArgs, "--trigger") ?? throw new ArgumentException("--trigger is required");
    var authMethod = GetArgValue(cmdArgs, "--auth") ?? GetDefaultAuthMethod();

    var client = await AuthenticateAsync(authMethod);
    var webhookRepository = new BoxWebhookRepository(client);
    var webhookService = new WebhookService(webhookRepository);

    var triggerObj = WebhookTrigger.FromString(trigger);
    var triggerList = new[] { triggerObj };
    var request = new CreateWebhookRequest(folderId, url, triggerList);

    var result = await webhookService.CreateWebhookAsync(request);

    if (result.IsSuccess)
    {
        var webhook = result.Value!;
        Console.WriteLine($"✓ Webhook created successfully!");
        Console.WriteLine($"  ID: {webhook.Id}");
        Console.WriteLine($"  Target: {webhook.TargetType} (ID: {webhook.TargetId})");
        Console.WriteLine($"  Address: {webhook.Address}");
        Console.WriteLine($"  Triggers: {string.Join(", ", webhook.Triggers.Select(t => t.Value))}");
        return 0;
    }
    else
    {
        Console.WriteLine($"✗ Error: {result.Error}");
        return 1;
    }
}

async Task<int> HandleListAsync(string[] cmdArgs)
{
    var authMethod = GetArgValue(cmdArgs, "--auth") ?? GetDefaultAuthMethod();

    var client = await AuthenticateAsync(authMethod);
    var webhookRepository = new BoxWebhookRepository(client);
    var webhookService = new WebhookService(webhookRepository);

    var result = await webhookService.GetAllWebhooksAsync();

    if (!result.IsSuccess)
    {
        Console.WriteLine($"✗ Error: {result.Error}");
        return 1;
    }

    var webhooks = result.Value!;

    if (webhooks.Count == 0)
    {
        Console.WriteLine("No webhooks found.");
        return 0;
    }

    Console.WriteLine($"Found {webhooks.Count} webhook(s):\n");

    foreach (var webhook in webhooks)
    {
        Console.WriteLine($"  ID: {webhook.Id}");
        Console.WriteLine($"    Target: {webhook.TargetType} (ID: {webhook.TargetId})");
        Console.WriteLine($"    Address: {webhook.Address}");
        Console.WriteLine($"    Triggers: {string.Join(", ", webhook.Triggers.Select(t => t.Value))}");
        Console.WriteLine();
    }

    return 0;
}

async Task<int> HandleGetAsync(string[] cmdArgs)
{
    var webhookId = GetArgValue(cmdArgs, "--webhook-id") ?? throw new ArgumentException("--webhook-id is required");
    var authMethod = GetArgValue(cmdArgs, "--auth") ?? GetDefaultAuthMethod();

    var client = await AuthenticateAsync(authMethod);
    var webhookRepository = new BoxWebhookRepository(client);
    var webhookService = new WebhookService(webhookRepository);

    var result = await webhookService.GetWebhookByIdAsync(webhookId);

    if (!result.IsSuccess)
    {
        Console.WriteLine($"✗ Error: {result.Error}");
        return 1;
    }

    var webhook = result.Value!;
    Console.WriteLine($"Webhook Details:");
    Console.WriteLine($"  ID: {webhook.Id}");
    Console.WriteLine($"  Target Type: {webhook.TargetType}");
    Console.WriteLine($"  Target ID: {webhook.TargetId}");
    Console.WriteLine($"  Address: {webhook.Address}");
    Console.WriteLine($"  Triggers: {string.Join(", ", webhook.Triggers.Select(t => t.Value))}");
    Console.WriteLine($"  Created At: {webhook.CreatedAt}");
    Console.WriteLine($"  Created By: {webhook.CreatedByName} ({webhook.CreatedByEmail})");

    return 0;
}

async Task<int> HandleDeleteAsync(string[] cmdArgs)
{
    var webhookId = GetArgValue(cmdArgs, "--webhook-id") ?? throw new ArgumentException("--webhook-id is required");
    var authMethod = GetArgValue(cmdArgs, "--auth") ?? GetDefaultAuthMethod();
    var force = cmdArgs.Contains("--force");

    var client = await AuthenticateAsync(authMethod);
    var webhookRepository = new BoxWebhookRepository(client);
    var webhookService = new WebhookService(webhookRepository);

    if (!force)
    {
        Console.Write($"Delete webhook {webhookId}? (y/n): ");
        var response = Console.ReadLine();
        if (response?.ToLower() != "y")
        {
            Console.WriteLine("Deletion cancelled.");
            return 0;
        }
    }

    var result = await webhookService.DeleteWebhookAsync(webhookId);

    if (result.IsSuccess)
    {
        Console.WriteLine($"✓ Webhook deleted successfully!");
        return 0;
    }
    else
    {
        Console.WriteLine($"✗ Error: {result.Error}");
        return 1;
    }
}

async Task<int> HandleListFoldersAsync(string[] cmdArgs)
{
    var folderId = GetArgValue(cmdArgs, "--folder-id") ?? "0";
    var authMethod = GetArgValue(cmdArgs, "--auth") ?? GetDefaultAuthMethod();

    var client = await AuthenticateAsync(authMethod);
    var folderRepository = new BoxFolderRepository(client);
    var folderService = new FolderService(folderRepository);

    var result = await folderService.GetFolderItemsAsync(folderId);

    if (!result.IsSuccess)
    {
        Console.WriteLine($"✗ Error: {result.Error}");
        return 1;
    }

    var items = result.Value!;

    if (items.Count == 0)
    {
        Console.WriteLine($"No items found in folder {folderId}.");
        return 0;
    }

    Console.WriteLine($"Items in folder {folderId}:\n");

    foreach (var item in items)
    {
        var typeLabel = item.ItemType switch
        {
            FolderItemType.Folder => "[Folder]",
            FolderItemType.File => "[File]  ",
            FolderItemType.WebLink => "[Link]  ",
            _ => "[???]   "
        };

        Console.WriteLine($"  {typeLabel} {item.Id} - {item.Name}");
    }

    return 0;
}

// ============ Helpers ============

string GetDefaultAuthMethod()
{
    // If developer token is present, use it as default auth method
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BOX_DEVELOPER_TOKEN")))
        return "developer-token";
    
    // If client credentials are present, use CCG as default
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BOX_CLIENT_ID")) &&
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BOX_CLIENT_SECRET")))
        return "ccg";
    
    // If JWT config path is present, use JWT
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BOX_JWT_CONFIG_PATH")))
        return "jwt";
    
    // If OAuth auth code is present, use OAuth
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BOX_AUTHORIZATION_CODE")))
        return "oauth";
    
    throw new ArgumentException(
        "No authentication method could be determined. Set one of: " +
        "BOX_DEVELOPER_TOKEN, BOX_CLIENT_ID+BOX_CLIENT_SECRET, BOX_JWT_CONFIG_PATH, or BOX_AUTHORIZATION_CODE. " +
        "Or explicitly specify --auth method.");
}

string? GetArgValue(string[] cmdArgs, string argName)
{
    for (int i = 0; i < cmdArgs.Length - 1; i++)
    {
        if (cmdArgs[i] == argName)
            return cmdArgs[i + 1];
    }
    return null;
}

async Task<BoxClient> AuthenticateAsync(string authMethod)
{
    var clientFactory = new BoxClientFactory();

    return authMethod.ToLower() switch
    {
        "developer-token" => AuthenticateWithDeveloperToken(clientFactory),
        "ccg" => AuthenticateWithCCG(clientFactory),
        "jwt" => await AuthenticateWithJWTAsync(clientFactory),
        "oauth" => await AuthenticateWithOAuthAsync(clientFactory),
        _ => throw new ArgumentException($"Unknown authentication method: {authMethod}. Use: developer-token, ccg, jwt, or oauth")
    };
}

BoxClient AuthenticateWithDeveloperToken(IBoxClientFactory clientFactory)
{
    var token = Environment.GetEnvironmentVariable("BOX_DEVELOPER_TOKEN");
    if (string.IsNullOrEmpty(token))
        throw new ArgumentException("BOX_DEVELOPER_TOKEN environment variable not set");

    return clientFactory.CreateWithDeveloperToken(token);
}

BoxClient AuthenticateWithCCG(IBoxClientFactory clientFactory)
{
    var clientId = Environment.GetEnvironmentVariable("BOX_CLIENT_ID");
    var clientSecret = Environment.GetEnvironmentVariable("BOX_CLIENT_SECRET");
    var enterpriseId = Environment.GetEnvironmentVariable("BOX_ENTERPRISE_ID");

    if (string.IsNullOrEmpty(clientId))
        throw new ArgumentException("BOX_CLIENT_ID environment variable not set");
    if (string.IsNullOrEmpty(clientSecret))
        throw new ArgumentException("BOX_CLIENT_SECRET environment variable not set");

    return clientFactory.CreateWithCcg(clientId, clientSecret, enterpriseId);
}

async Task<BoxClient> AuthenticateWithJWTAsync(IBoxClientFactory clientFactory)
{
    var configPath = Environment.GetEnvironmentVariable("BOX_JWT_CONFIG_PATH");
    if (string.IsNullOrEmpty(configPath))
        throw new ArgumentException("BOX_JWT_CONFIG_PATH environment variable not set");

    return await clientFactory.CreateWithJwtAsync(configPath);
}

async Task<BoxClient> AuthenticateWithOAuthAsync(IBoxClientFactory clientFactory)
{
    var clientId = Environment.GetEnvironmentVariable("BOX_CLIENT_ID");
    var clientSecret = Environment.GetEnvironmentVariable("BOX_CLIENT_SECRET");
    var authorizationCode = Environment.GetEnvironmentVariable("BOX_AUTHORIZATION_CODE");

    if (string.IsNullOrEmpty(clientId))
        throw new ArgumentException("BOX_CLIENT_ID environment variable not set");
    if (string.IsNullOrEmpty(clientSecret))
        throw new ArgumentException("BOX_CLIENT_SECRET environment variable not set");
    if (string.IsNullOrEmpty(authorizationCode))
        throw new ArgumentException("BOX_AUTHORIZATION_CODE environment variable not set");

    return await clientFactory.CreateWithOAuthAsync(clientId, clientSecret, authorizationCode);
}
