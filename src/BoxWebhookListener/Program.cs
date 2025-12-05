using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add configuration for Box webhook keys
builder.Services.Configure<BoxWebhookSettings>(
    builder.Configuration.GetSection("BoxWebhook"));

var app = builder.Build();

// Health check endpoint
app.MapGet("/", () => "Box Webhook Listener is running!");

// Main webhook endpoint
app.MapPost("/webhook", async (HttpContext context, IConfiguration config, ILogger<Program> logger) =>
{
    // Read the raw body for signature verification
    context.Request.EnableBuffering();
    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
    var body = await reader.ReadToEndAsync();
    context.Request.Body.Position = 0;

    // Log incoming webhook
    logger.LogInformation("Received webhook at {Time}", DateTime.UtcNow);

    // Get signature headers
    var primarySignature = context.Request.Headers["BOX-SIGNATURE-PRIMARY"].FirstOrDefault();
    var secondarySignature = context.Request.Headers["BOX-SIGNATURE-SECONDARY"].FirstOrDefault();
    var deliveryId = context.Request.Headers["BOX-DELIVERY-ID"].FirstOrDefault();
    var timestamp = context.Request.Headers["BOX-DELIVERY-TIMESTAMP"].FirstOrDefault();

    logger.LogInformation("Delivery ID: {DeliveryId}, Timestamp: {Timestamp}", deliveryId, timestamp);

    // Verify signature (optional but recommended for production)
    var primaryKey = config["BoxWebhook:PrimaryKey"];
    var secondaryKey = config["BoxWebhook:SecondaryKey"];

    if (!string.IsNullOrEmpty(primaryKey))
    {
        var isValid = VerifySignature(body, primarySignature, primaryKey) ||
                      (!string.IsNullOrEmpty(secondaryKey) && VerifySignature(body, secondarySignature, secondaryKey));

        if (!isValid)
        {
            logger.LogWarning("Invalid webhook signature!");
            return Results.Unauthorized();
        }
        logger.LogInformation("Signature verified successfully");
    }

    // Parse the webhook payload
    try
    {
        var payload = JsonSerializer.Deserialize<BoxWebhookPayload>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (payload != null)
        {
            logger.LogInformation("=== Box Webhook Event ===");
            logger.LogInformation("Type: {Type}", payload.Type);
            logger.LogInformation("Trigger: {Trigger}", payload.Trigger);
            logger.LogInformation("Webhook ID: {WebhookId}", payload.Webhook?.Id);
            
            if (payload.Source != null)
            {
                logger.LogInformation("Source Type: {SourceType}", payload.Source.Type);
                logger.LogInformation("Source ID: {SourceId}", payload.Source.Id);
                logger.LogInformation("Source Name: {SourceName}", payload.Source.Name);
                
                if (payload.Source.Parent != null)
                {
                    logger.LogInformation("Parent Folder: {ParentName} (ID: {ParentId})", 
                        payload.Source.Parent.Name, payload.Source.Parent.Id);
                }
            }

            if (payload.CreatedBy != null)
            {
                logger.LogInformation("Created By: {UserName} ({UserEmail})", 
                    payload.CreatedBy.Name, payload.CreatedBy.Login);
            }

            logger.LogInformation("=========================");

            // Handle specific triggers
            await HandleWebhookEvent(payload, logger);
        }
    }
    catch (JsonException ex)
    {
        logger.LogError(ex, "Failed to parse webhook payload");
        logger.LogDebug("Raw payload: {Body}", body);
    }

    // Always return 200 OK to acknowledge receipt
    return Results.Ok(new { message = "Webhook received", deliveryId });
});

app.Run();

// Signature verification helper
static bool VerifySignature(string payload, string? signature, string key)
{
    if (string.IsNullOrEmpty(signature)) return false;

    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    var computedSignature = Convert.ToBase64String(hash);

    return signature == computedSignature;
}

// Event handler - customize this for your needs
static Task HandleWebhookEvent(BoxWebhookPayload payload, ILogger logger)
{
    switch (payload.Trigger?.ToUpperInvariant())
    {
        case "FILE.UPLOADED":
            logger.LogInformation("📁 New file uploaded: {FileName}", payload.Source?.Name);
            // Add your custom logic here (e.g., process file, notify users, etc.)
            break;

        case "FILE.DOWNLOADED":
            logger.LogInformation("⬇️ File downloaded: {FileName}", payload.Source?.Name);
            break;

        case "FILE.PREVIEWED":
            logger.LogInformation("👁️ File previewed: {FileName}", payload.Source?.Name);
            break;

        case "FILE.TRASHED":
            logger.LogInformation("🗑️ File trashed: {FileName}", payload.Source?.Name);
            break;

        case "FILE.DELETED":
            logger.LogInformation("❌ File deleted: {FileName}", payload.Source?.Name);
            break;

        case "FOLDER.CREATED":
            logger.LogInformation("📂 New folder created: {FolderName}", payload.Source?.Name);
            break;

        default:
            logger.LogInformation("📨 Event received: {Trigger}", payload.Trigger);
            break;
    }

    return Task.CompletedTask;
}

// Configuration settings
public class BoxWebhookSettings
{
    public string? PrimaryKey { get; set; }
    public string? SecondaryKey { get; set; }
}

// Webhook payload models
public class BoxWebhookPayload
{
    public string? Type { get; set; }
    public string? Id { get; set; }
    public string? Trigger { get; set; }
    public DateTime? CreatedAt { get; set; }
    public BoxWebhookInfo? Webhook { get; set; }
    public BoxUser? CreatedBy { get; set; }
    public BoxItem? Source { get; set; }
}

public class BoxWebhookInfo
{
    public string? Id { get; set; }
    public string? Type { get; set; }
}

public class BoxUser
{
    public string? Type { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Login { get; set; }
}

public class BoxItem
{
    public string? Type { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public BoxFolder? Parent { get; set; }
}

public class BoxFolder
{
    public string? Type { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
}
