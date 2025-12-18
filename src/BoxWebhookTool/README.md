# Box Webhook CLI Tool

A command-line tool for managing Box webhooks in CI/CD pipelines and automation scripts.

This tool is distributed as a `dotnet tool` and can be installed globally or as a local tool in your repository.

## Overview

BoxWebhookTool is a lightweight CLI that reuses the domain and infrastructure layers from the shared BoxWebhookShared library. It provides a simple command-line interface for creating, listing, and managing Box webhooks without requiring a graphical interface.

## Installation

### Option 1: Global Installation (from GitHub releases or local build)

```bash
dotnet tool install --global BoxWebhookTool --version 1.0.0
```

### Option 2: Local Tool Installation (from repository)

Create or update `.config/dotnet-tools.json`:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "boxwebhooktool": {
      "version": "1.0.0",
      "commands": [
        "box-webhook-tool"
      ]
    }
  }
}
```

Then install:

```bash
dotnet tool restore
```

### Option 3: Build and Install Locally

```bash
cd src/BoxWebhookTool
dotnet pack
dotnet tool install --global --add-source ./nupkg BoxWebhookTool
```

## Quick Start (Without Installation)

If you want to run the tool directly without installing it:

```bash
cd src/BoxWebhookTool
dotnet run -- [command] [options]
```

**Examples:**

```bash
# List all webhooks
dotnet run -- list --auth developer-token

# Create a webhook
dotnet run -- create \
  --folder-id "12345" \
  --url "https://your-webhook-endpoint.example.com/webhook" \
  --trigger "FILE.UPLOADED"

# Get webhook details
dotnet run -- get --webhook-id "webhook-id-here"

# List folders
dotnet run -- list-folders --folder-id "0"
```

**Note:** Environment variables for authentication (like `BOX_DEVELOPER_TOKEN`) work the same way as with the installed tool.

## Usage

### Create a Webhook

```bash
box-webhook-tool create \
  --folder-id "12345" \
  --url "https://your-webhook-endpoint.example.com/webhook" \
  --trigger "FILE.UPLOADED"
```

**Options:**

- `--folder-id, -f` (required): The Box folder ID to monitor
- `--url, -u` (required): Webhook endpoint URL (must be HTTPS, port 443)
- `--trigger, -t` (required): Webhook trigger (e.g., `FILE.UPLOADED`, `FILE.DELETED`, `FOLDER.CREATED`)
- `--auth, -a` (optional): Authentication method (auto-detected from env vars if not specified)

### List All Webhooks

```bash
box-webhook-tool list
```

### Get Webhook Details

```bash
box-webhook-tool get \
  --webhook-id "webhook-id-here"
```

### Delete a Webhook

```bash
box-webhook-tool delete \
  --webhook-id "webhook-id-here" \
  --force  # Skip confirmation prompt
```

### List Folders

```bash
box-webhook-tool list-folders \
  --folder-id "0"
```

## Authentication

**Authentication is automatic!** If `BOX_DEVELOPER_TOKEN` is present, no `--auth` flag is needed.

### Auto-Detection Priority

The tool detects available authentication in this order:

1. **Developer Token** - If `BOX_DEVELOPER_TOKEN` is set
2. **Client Credentials Grant (CCG)** - If `BOX_CLIENT_ID` + `BOX_CLIENT_SECRET` are set
3. **JWT** - If `BOX_JWT_CONFIG_PATH` is set
4. **OAuth** - If `BOX_AUTHORIZATION_CODE` is set

### Quick Start (Developer Token)

```bash
export BOX_DEVELOPER_TOKEN="your_token_here"
box-webhook-tool list  # No --auth needed!
```

### Developer Token

```bash
export BOX_DEVELOPER_TOKEN="your_token_here"
box-webhook-tool list --auth developer-token
```

### Client Credentials Grant (CCG)

Recommended for server-to-server automation.

```bash
export BOX_CLIENT_ID="your_client_id"
export BOX_CLIENT_SECRET="your_client_secret"
export BOX_ENTERPRISE_ID="your_enterprise_id"  # Optional
box-webhook-tool list --auth ccg
```

### JWT

```bash
export BOX_JWT_CONFIG_PATH="/path/to/jwt_config.json"
box-webhook-tool list --auth jwt
```

### OAuth 2.0

```bash
export BOX_CLIENT_ID="your_client_id"
export BOX_CLIENT_SECRET="your_client_secret"
export BOX_AUTHORIZATION_CODE="auth_code_from_oauth_flow"
box-webhook-tool list --auth oauth
```

## CI/CD Pipeline Examples

### Azure DevOps

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

variables:
  boxFolderId: '12345'
  webhookUrl: 'https://example.com/webhook'

steps:
  - task: UseDotNet@2
    inputs:
      version: '10.0.x'

  - task: DotNetCoreCLI@2
    inputs:
      command: 'custom'
      custom: 'tool'
      arguments: 'restore'

  - script: |
      box-webhook-tool create \
        --folder-id "$(boxFolderId)" \
        --url "$(webhookUrl)" \
        --trigger "FILE.UPLOADED" \
        --auth "ccg" \
        --force
    displayName: 'Create Box Webhook'
    env:
      BOX_CLIENT_ID: $(boxClientId)
      BOX_CLIENT_SECRET: $(boxClientSecret)
      BOX_ENTERPRISE_ID: $(boxEnterpriseId)
```

### GitHub Actions

```yaml
name: Create Box Webhook

on:
  workflow_dispatch

jobs:
  create-webhook:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore local tools
        run: dotnet tool restore

      - name: Create webhook
        env:
          BOX_CLIENT_ID: ${{ secrets.BOX_CLIENT_ID }}
          BOX_CLIENT_SECRET: ${{ secrets.BOX_CLIENT_SECRET }}
          BOX_ENTERPRISE_ID: ${{ secrets.BOX_ENTERPRISE_ID }}
        run: |
          box-webhook-tool create \
            --folder-id "12345" \
            --url "https://example.com/webhook" \
            --trigger "FILE.UPLOADED" \
            --auth "ccg"
```

### GitHub Actions with Matrix

```yaml
name: Create Multiple Webhooks

on:
  workflow_dispatch

jobs:
  create-webhooks:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        environment:
          - { folderId: '12345', trigger: 'FILE.UPLOADED' }
          - { folderId: '67890', trigger: 'FOLDER.CREATED' }
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore local tools
        run: dotnet tool restore

      - name: Create webhook for ${{ matrix.environment.trigger }}
        env:
          BOX_CLIENT_ID: ${{ secrets.BOX_CLIENT_ID }}
          BOX_CLIENT_SECRET: ${{ secrets.BOX_CLIENT_SECRET }}
          BOX_ENTERPRISE_ID: ${{ secrets.BOX_ENTERPRISE_ID }}
        run: |
          box-webhook-tool create \
            --folder-id "${{ matrix.environment.folderId }}" \
            --url "https://example.com/webhook" \
            --trigger "${{ matrix.environment.trigger }}" \
            --auth "ccg"
```

## Exit Codes

- `0`: Success
- `1`: Error (detailed message printed to stderr)

## Development

To build and test locally:

```bash
cd src/BoxWebhookTool
dotnet build
dotnet run -- list --auth developer-token
```

To pack as a tool:

```bash
dotnet pack
```

This creates a NuGet package in `./nupkg/` that can be distributed and installed.

## Manual Deployment of NuGet Package

### Building the Package

```bash
cd src/BoxWebhookTool
dotnet pack --configuration Release --output ./nupkg
```

This creates a NuGet package file (e.g., `BoxWebhookTool.1.0.0.nupkg`) in the `nupkg` directory.

### Local Installation from Package

**Install globally from local package:**

```bash
dotnet tool install --global --add-source ./nupkg BoxWebhookTool --version 1.0.0
```

**Install as a local tool in your repository:**

1. Create `.config/dotnet-tools.json`:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "boxwebhooktool": {
      "version": "1.0.0",
      "commands": ["box-webhook-tool"]
    }
  }
}
```

2. Install local tools:

```bash
dotnet tool restore
```

### Publishing to NuGet (Optional)

To publish the package to nuget.org or a private NuGet feed:

**Push to NuGet.org (requires account and API key):**

```bash
dotnet nuget push ./nupkg/BoxWebhookTool.1.0.0.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

**Push to private NuGet feed:**

```bash
dotnet nuget push ./nupkg/BoxWebhookTool.1.0.0.nupkg \
  --api-key YOUR_FEED_API_KEY \
  --source https://your-private-feed.com/nuget
```

### Verifying Installation

After installation, verify the tool is available:

```bash
box-webhook-tool --version
```

## License

MIT

## Support

For issues or feature requests, visit the [repository](https://github.com/cyclotron-azure/box-webhooks).
