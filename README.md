# Box Webhooks

A .NET solution for creating and managing Box webhooks programmatically, with a listener to receive webhook events.

## Overview

This solution contains two projects that work together to demonstrate the complete Box webhook workflow:

| Project | Description |
|---------|-------------|
| [BoxWebhookDemo](src/BoxWebhookDemo/README.md) | Console app to create, list, and manage Box webhooks |
| [BoxWebhookListener](src/BoxWebhookListener/README.md) | Minimal API to receive and process webhook events |

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- [Box Developer Account](https://developer.box.com)
- Box application with **Manage webhooks** scope enabled

### 1. Clone and Build

```bash
git clone https://github.com/cyclotron-azure/box-webhooks
cd box-webhooks
dotnet build
```

### 2. Configure Credentials

Copy the sample environment file and add your Box credentials:

```bash
cd src/BoxWebhookDemo
cp .env.sample .env
```

Edit `.env` with your Box app credentials:

```env
BOX_CLIENT_ID=your_client_id
BOX_CLIENT_SECRET=your_client_secret
```

### 3. Start the Webhook Listener

In a terminal, start the listener to receive webhook events:

```bash
cd src/BoxWebhookListener
dotnet run
```

The listener will start on `http://localhost:7979`.

### 4. Expose the Listener (for Box to reach it)

Use VS Code Port Forwarding to expose your local listener:

1. Open VS Code **Ports** panel (View → Terminal → Ports tab)
2. Click **Forward a Port** and enter `7979`
3. **Important**: Set visibility to **Public** (right-click → Change Port Visibility → Public)
4. Copy the forwarded URL (e.g., `https://xxx-7979.use2.devtunnels.ms`)

### 5. Create a Webhook

In another terminal, run the demo app:

```bash
cd src/BoxWebhookDemo
dotnet run
```

Choose your authentication method:

| Method | Account Type | Recommended For |
|--------|--------------|-----------------|
| 1. Developer Token | Personal & Enterprise | Quick testing (expires in 60 min) |
| 2. CCG | Enterprise only | Server-to-server automation |
| 3. JWT | Enterprise only | Enterprise applications |
| 4. OAuth 2.0 | Personal & Enterprise | Personal accounts, user context |

Then:
1. Select **"Create webhook on a folder"**
2. Enter a folder ID (use option 5 to list folders)
3. Enter your public webhook URL (from step 4)
4. Select triggers (e.g., `FILE.UPLOADED`)

### 6. Test It

Upload a file to your monitored folder in Box. You should see the webhook event in the listener console!

## Project Structure

```
box-webhooks/
├── README.md                    # This file
├── box-webhooks.sln             # Solution file
├── .gitignore                   # Git ignore rules
└── src/
    ├── BoxWebhookDemo/          # Webhook management console app
    │   ├── Domain/              # Core business logic
    │   ├── Application/         # Use cases and services
    │   ├── Infrastructure/      # Box SDK implementations
    │   ├── Presentation/        # Console UI
    │   └── README.md            # Detailed documentation
    │
    └── BoxWebhookListener/      # Webhook receiver API
        ├── Program.cs           # Minimal API entry point
        ├── rest/                # HTTP test files
        └── README.md            # Detailed documentation
```

## Authentication Options

### Personal Box Accounts

Use **Developer Token** or **OAuth 2.0**:
- Developer Token: Quick setup, expires in 60 minutes
- OAuth 2.0: Browser-based login, automatic token refresh

### Enterprise Box Accounts

All methods available:
- Developer Token: Testing only
- CCG: Server-to-server (requires admin authorization)
- JWT: Enterprise apps (requires key pair setup)
- OAuth 2.0: User-context operations

## Common Use Cases

### Monitor File Uploads

```
Trigger: FILE.UPLOADED
Use case: Process new files automatically, trigger workflows
```

### Track Document Changes

```
Triggers: FILE.RENAMED, FILE.MOVED, FILE.TRASHED
Use case: Audit trail, sync with external systems
```

### Collaboration Events

```
Triggers: COLLABORATION.CREATED, COLLABORATION.ACCEPTED
Use case: Notify team members, update permissions
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| `invalid_client` error | Wrong app type - CCG apps can't use OAuth, create correct app type |
| Webhook not receiving events | Ensure listener URL is **Public** in VS Code port forwarding |
| `403 Forbidden` | Enable "Manage webhooks" scope in Box Developer Console |
| CCG authentication fails | Requires Enterprise account + admin authorization |

## Resources

- [Box Developer Documentation](https://developer.box.com)
- [Box Webhook Guides](https://developer.box.com/guides/webhooks/)
- [Box .NET SDK](https://github.com/box/box-dotnet-sdk-gen)

## License

MIT License
