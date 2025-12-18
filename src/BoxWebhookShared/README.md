# Box Webhook Shared Library

A reusable .NET library containing shared domain logic, application services, and Box SDK implementations for webhook management.

## Overview

`BoxWebhookShared` is the core library that contains all the business logic for managing Box webhooks. It follows **Domain-Driven Design (DDD)** and **SOLID** principles and is used by both the interactive console app (`BoxWebhookDemo`) and the CLI tool (`BoxWebhookTool`).

## Architecture

The library is structured in three layers:

### 1. Domain Layer (`Domain/`)

Contains core business logic with no external dependencies:

```
Domain/
├── Entities/
│   ├── WebhookEntity.cs         # Webhook aggregate root
│   ├── WebhookSummary.cs        # Lightweight webhook DTO
│   ├── FolderEntity.cs          # Folder entity
│   └── FolderItemEntity.cs      # Folder item entity (with enum)
├── ValueObjects/
│   ├── WebhookTrigger.cs        # Immutable trigger type (FILE.UPLOADED, etc.)
│   └── WebhookTargetType.cs     # Immutable target type (folder, file)
└── Interfaces/
    ├── IWebhookRepository.cs    # Webhook persistence contract
    ├── IFolderRepository.cs     # Folder persistence contract
    └── IBoxClientFactory.cs     # Authentication factory contract
```

**Key Principles:**
- No external dependencies (except abstractions)
- Value objects ensure type safety
- Repository interfaces define contracts for data access

### 2. Application Layer (`Application/`)

Contains use cases and application services:

```
Application/
├── DTOs/
│   ├── CreateWebhookRequest.cs  # Webhook creation request
│   ├── CreateFolderRequest.cs   # Folder creation request
│   └── OperationResult.cs       # Generic result wrapper (no exceptions)
└── Services/
    ├── IWebhookService.cs       # Webhook service interface
    ├── WebhookService.cs        # Webhook service implementation
    ├── IFolderService.cs        # Folder service interface
    └── FolderService.cs         # Folder service implementation
```

**Features:**
- Orchestrates domain logic
- Returns `OperationResult<T>` for explicit error handling (no exceptions)
- Validates business rules before executing

### 3. Infrastructure Layer (`Infrastructure/`)

Contains Box SDK implementations:

```
Infrastructure/
└── Box/
    ├── BoxClientFactory.cs      # Creates authenticated Box clients
    ├── BoxWebhookRepository.cs  # Box SDK webhook implementation
    └── BoxFolderRepository.cs   # Box SDK folder implementation
```

**Supports Multiple Authentication Methods:**
- Developer Token (testing)
- Client Credentials Grant (CCG) - server-to-server
- JWT Authentication (enterprise)
- OAuth 2.0 (user context)

## SOLID Principles

| Principle | Implementation |
| --------- | --------------- |
| **S**ingle Responsibility | Each service has one reason to change |
| **O**pen/Closed | Extensible via interfaces without modification |
| **L**iskov Substitution | Implementations are interchangeable |
| **I**nterface Segregation | Focused interfaces (IWebhookService, IFolderService) |
| **D**ependency Inversion | High-level modules depend on abstractions |

## Usage Examples

### Creating a Webhook

```csharp
using BoxWebhookShared.Application.DTOs;
using BoxWebhookShared.Application.Services;
using BoxWebhookShared.Domain.Entities;
using BoxWebhookShared.Domain.Interfaces;
using BoxWebhookShared.Infrastructure.Box;

// Create authenticated Box client
var clientFactory = new BoxClientFactory();
var boxClient = clientFactory.CreateWithDeveloperToken("your_token");

// Create repositories and services
var webhookRepository = new BoxWebhookRepository(boxClient);
var webhookService = new WebhookService(webhookRepository);

// Create webhook
var request = new CreateWebhookRequest(
    FolderId: "12345",
    WebhookUrl: "https://example.com/webhook",
    Triggers: new[] { WebhookTrigger.FileUploaded }
);

var result = await webhookService.CreateWebhookAsync(request);

if (result.IsSuccess)
{
    var webhook = result.Value;
    Console.WriteLine($"Created webhook: {webhook.Id}");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}
```

### Listing Webhooks

```csharp
var result = await webhookService.GetAllWebhooksAsync();

if (result.IsSuccess)
{
    foreach (var webhook in result.Value)
    {
        Console.WriteLine($"Webhook: {webhook.Address}");
        Console.WriteLine($"  Triggers: {string.Join(", ", webhook.Triggers.Select(t => t.Value))}");
    }
}
```

### Listing Folder Contents

```csharp
var folderRepository = new BoxFolderRepository(boxClient);
var folderService = new FolderService(folderRepository);

var result = await folderService.GetFolderItemsAsync("0"); // 0 = root

if (result.IsSuccess)
{
    foreach (var item in result.Value)
    {
        Console.WriteLine($"{item.ItemType}: {item.Name}");
    }
}
```

## Authentication

The `BoxClientFactory` supports multiple authentication methods:

```csharp
// Developer Token (60-minute expiration)
var client = factory.CreateWithDeveloperToken("token");

// Client Credentials Grant (CCG)
var client = factory.CreateWithCcg(
    clientId: "id",
    clientSecret: "secret",
    enterpriseId: "enterprise_id" // optional
);

// JWT
var client = await factory.CreateWithJwtAsync("/path/to/config.json");

// JWT from Base64
var client = await factory.CreateWithJwtFromBase64Async(base64String);

// OAuth 2.0
var client = await factory.CreateWithOAuthAsync(
    clientId: "id",
    clientSecret: "secret",
    authorizationCode: "code"
);

// OAuth 2.0 Authorization URL
var url = factory.GetOAuthAuthorizeUrl(clientId, clientSecret, redirectUri);
```

## Error Handling

Services use `OperationResult<T>` pattern for explicit error handling:

```csharp
var result = await service.CreateWebhookAsync(request);

if (result.IsSuccess)
{
    var webhook = result.Value;  // Safe to use
    // Process webhook
}
else
{
    var errorMessage = result.Error;  // Always populated on failure
    // Handle error
}
```

This approach:
- Eliminates unexpected exceptions
- Makes error handling explicit
- Provides clear error messages
- Enables functional programming style

## Available Webhook Triggers

### File Events
- `FILE.UPLOADED` - File uploaded or moved to folder
- `FILE.PREVIEWED` - File previewed
- `FILE.DOWNLOADED` - File downloaded
- `FILE.TRASHED` - File moved to trash
- `FILE.DELETED` - File permanently deleted
- `FILE.RESTORED` - File restored from trash
- `FILE.COPIED` - File copied
- `FILE.MOVED` - File moved
- `FILE.LOCKED` - File locked
- `FILE.UNLOCKED` - File unlocked
- `FILE.RENAMED` - File renamed

### Folder Events
- `FOLDER.CREATED` - Folder created
- `FOLDER.RENAMED` - Folder renamed
- `FOLDER.DOWNLOADED` - Folder downloaded
- `FOLDER.RESTORED` - Folder restored from trash
- `FOLDER.DELETED` - Folder permanently deleted
- `FOLDER.COPIED` - Folder copied
- `FOLDER.MOVED` - Folder moved to different folder
- `FOLDER.TRASHED` - Folder moved to trash

### Predefined Trigger Sets
```csharp
// Use predefined sets for common scenarios
var triggers = WebhookTrigger.AllFileUploadEvents;         // FILE.UPLOADED
var triggers = WebhookTrigger.AllFileAccessEvents;         // UPLOADED, DOWNLOADED, PREVIEWED
var triggers = WebhookTrigger.AllFileLifecycleEvents;      // UPLOADED, TRASHED, DELETED, etc.
```

## Dependencies

- **Box.Sdk.Gen** (1.12.0) - Official Box API client
- **.NET Standard 2.0+** - Compatible with .NET 6.0, 10.0, etc.

## Project Structure

- Used by: `BoxWebhookDemo` (interactive console app)
- Used by: `BoxWebhookTool` (CLI tool for CI/CD)
- Target Framework: `net10.0`
- No external UI dependencies

## License

MIT
