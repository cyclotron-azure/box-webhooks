# Box Webhooks

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/cyclotron-azure/box-webhooks/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Box SDK](https://img.shields.io/badge/Box%20SDK-Gen-0061D5)](https://github.com/box/box-dotnet-sdk-gen)

A .NET solution for creating and managing Box webhooks programmatically, with a listener to receive webhook events.

## Overview

This solution contains three projects that work together to manage Box webhooks:

| Project | Description |
|---------|-------------|
| [BoxWebhookShared](src/BoxWebhookShared/) | Shared library with domain logic, application services, and Box SDK implementations |
| [BoxWebhookDemo](src/BoxWebhookDemo/README.md) | Interactive console app to create, list, and manage Box webhooks |
| [BoxWebhookTool](src/BoxWebhookTool/README.md) | CLI tool for managing webhooks in CI/CD pipelines and automation scripts |
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

## CLI Tool for CI/CD

For automation and CI/CD pipelines, use **BoxWebhookTool** - a command-line interface that doesn't require user interaction.

### Quick Start with CLI Tool

```bash
# Set environment variables for authentication
export BOX_CLIENT_ID="your_client_id"
export BOX_CLIENT_SECRET="your_client_secret"

# Create a webhook
dotnet src/BoxWebhookTool/bin/Debug/net10.0/BoxWebhookTool.dll create \
  --folder-id 12345 \
  --url https://example.com/webhook \
  --trigger FILE.UPLOADED

# List all webhooks
dotnet src/BoxWebhookTool/bin/Debug/net10.0/BoxWebhookTool.dll list

# Delete a webhook
dotnet src/BoxWebhookTool/bin/Debug/net10.0/BoxWebhookTool.dll delete \
  --webhook-id webhook-id-here --force
```

See [BoxWebhookTool README](src/BoxWebhookTool/README.md) for detailed CLI documentation and CI/CD examples.

## Project Structure

```
box-webhooks/
├── README.md                    # This file
├── box-webhooks.sln             # Solution file
├── .gitignore                   # Git ignore rules
└── src/
    ├── BoxWebhookShared/        # Shared library (new!)
    │   ├── Domain/              # Core business logic
    │   ├── Application/         # Services and DTOs
    │   └── Infrastructure/      # Box SDK implementations
    │
    ├── BoxWebhookDemo/          # Webhook management console app
    │   ├── Presentation/        # Console UI
    │   ├── Infrastructure/      # OAuth handler
    │   └── README.md            # Detailed documentation
    │
    ├── BoxWebhookTool/          # CLI tool for CI/CD
    │   ├── Program.cs           # CLI entry point
    │   └── README.md            # CLI documentation
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

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
