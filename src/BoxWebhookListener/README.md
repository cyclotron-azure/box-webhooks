# Box Webhook Listener

A minimal ASP.NET Core API to receive and process Box webhook events.

## Features

- ✅ Receives Box webhook events
- ✅ Signature verification (HMAC-SHA256)
- ✅ Structured logging of events
- ✅ Handles all common Box triggers (FILE.UPLOADED, etc.)

## Quick Start

```bash
# Navigate to the project
cd BoxWebhookListener

# Run the listener
dotnet run
```

The API will start on `http://localhost:5000` (or configured port).

## Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/` | GET | Health check |
| `/webhook` | POST | Receives Box webhook events |

## Making it Publicly Accessible

Box requires your webhook URL to be:
- **HTTPS** on **port 443**
- Publicly accessible from the internet

### VS Code Port Forwarding

VS Code can create a public HTTPS tunnel to your local port.

1. **Start the listener**:
   ```bash
   cd BoxWebhookListener
   dotnet run
   ```

2. **Forward the port**:
   - Open the **PORTS** panel in VS Code (View → Ports, or click "Ports" in the bottom panel)
   - Click **"Forward a Port"**
   - Enter `7979`

3. **⚠️ IMPORTANT: Set visibility to Public**:
   - Right-click the port **7979** in the PORTS panel
   - Select **Port Visibility** → **Public**
   
   > Without this step, Box cannot reach your endpoint (returns 302 redirect requiring authentication)

4. **Copy the forwarded URL**:
   - VS Code provides a URL like: `https://abc123-7979.use2.devtunnels.ms`
   - Use `https://abc123-7979.use2.devtunnels.ms/webhook` as your Box webhook URL

5. **Verify it's working**:
   ```bash
   curl https://your-tunnel-url.devtunnels.ms/
   # Should return: "Box Webhook Listener is running!"
   ```

## Signature Verification

Box signs webhooks with HMAC-SHA256. To verify:

1. Get your keys from [Box Developer Console](https://app.box.com/developers/console)
   - Go to your app → Configuration → Webhooks
   - Copy **Primary Key** and **Secondary Key**

2. Add to `appsettings.json`:
```json
{
  "BoxWebhook": {
    "PrimaryKey": "your_primary_key_here",
    "SecondaryKey": "your_secondary_key_here"
  }
}
```

Or use environment variables:
```bash
export BoxWebhook__PrimaryKey="your_primary_key"
export BoxWebhook__SecondaryKey="your_secondary_key"
```

## Example Webhook Payload

When a file is uploaded, Box sends:

```json
{
  "type": "webhook_event",
  "id": "event_id",
  "created_at": "2024-01-01T12:00:00-00:00",
  "trigger": "FILE.UPLOADED",
  "webhook": {
    "id": "webhook_id",
    "type": "webhook"
  },
  "created_by": {
    "type": "user",
    "id": "user_id",
    "name": "John Doe",
    "login": "john@example.com"
  },
  "source": {
    "type": "file",
    "id": "file_id",
    "name": "document.pdf",
    "parent": {
      "type": "folder",
      "id": "folder_id",
      "name": "My Folder"
    }
  }
}
```

## Example Console Output

```
info: Program[0]
      Received webhook at 11/28/2025 10:30:00 AM
info: Program[0]
      Delivery ID: abc123, Timestamp: 2025-11-28T10:30:00Z
info: Program[0]
      === Box Webhook Event ===
info: Program[0]
      Type: webhook_event
info: Program[0]
      Trigger: FILE.UPLOADED
info: Program[0]
      Source Type: file
info: Program[0]
      Source ID: 123456789
info: Program[0]
      Source Name: report.pdf
info: Program[0]
      Parent Folder: Documents (ID: 987654321)
info: Program[0]
      Created By: John Doe (john@example.com)
info: Program[0]
      =========================
info: Program[0]
      📁 New file uploaded: report.pdf
```

## Customizing Event Handling

Edit the `HandleWebhookEvent` method in `Program.cs`:

```csharp
static Task HandleWebhookEvent(BoxWebhookPayload payload, ILogger logger)
{
    switch (payload.Trigger?.ToUpperInvariant())
    {
        case "FILE.UPLOADED":
            // Your custom logic here
            // e.g., Send notification, process file, update database
            break;
    }
    return Task.CompletedTask;
}
```

## Complete Workflow

1. **Start the listener**:
   ```bash
   cd BoxWebhookListener
   dotnet run
   ```

2. **Forward port in VS Code**:
   - Open **PORTS** panel → Forward port `7979`
   - Set **Port Visibility** → **Public**
   - Copy the devtunnels URL

3. **Create webhook** (using BoxWebhookDemo):
   ```bash
   cd ../BoxWebhookDemo
   dotnet run
   # Select option 1 to create webhook
   # Use devtunnels URL + /webhook as the address
   ```

4. **Upload a file** to your Box folder

5. **Watch the event** appear in your listener console!
