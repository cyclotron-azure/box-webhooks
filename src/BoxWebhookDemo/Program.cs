using BoxWebhookDemo.Application.Services;
using BoxWebhookDemo.Domain.Interfaces;
using BoxWebhookDemo.Infrastructure.Box;
using BoxWebhookDemo.Presentation.ConsoleUI;

namespace BoxWebhookDemo;

/// <summary>
/// Box Webhook Demo Application
/// 
/// Restructured using DDD and SOLID principles:
/// - Domain Layer: Entities, Value Objects, and Repository Interfaces
/// - Application Layer: Services that orchestrate domain logic
/// - Infrastructure Layer: Box SDK implementations
/// - Presentation Layer: Console UI components
/// 
/// SOLID Principles Applied:
/// - Single Responsibility: Each class has one reason to change
/// - Open/Closed: Extensible through interfaces without modification
/// - Liskov Substitution: Implementations are interchangeable
/// - Interface Segregation: Focused interfaces (IWebhookService, IFolderService)
/// - Dependency Inversion: High-level modules depend on abstractions
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Box Webhook Demo ===\n");

        try
        {
            // Create console I/O abstraction
            IConsoleIO console = new ConsoleIO();

            // Create authentication factory
            IBoxClientFactory clientFactory = new BoxClientFactory();

            // Authenticate and get Box client
            var authHandler = new AuthenticationHandler(clientFactory, console);
            var boxClient = await authHandler.AuthenticateAsync();

            // Quick validation of client/auth: fetch current user to ensure token is valid
            try
            {
                await boxClient.Folders.GetFolderItemsAsync(folderId: "0");
                console.WriteLine("✓ Authentication validated successfully.");
            }
            catch (Exception ex)
            {
                console.WriteLine($"Warning: failed to validate Box client after authentication. {ex.GetType().Name}: {ex.Message}");
                console.WriteLine("This often indicates an invalid/expired developer token or network issue.");
                console.WriteLine("Full error: " + ex);
            }

            // Create repositories (Infrastructure layer)
            IWebhookRepository webhookRepository = new BoxWebhookRepository(boxClient);
            IFolderRepository folderRepository = new BoxFolderRepository(boxClient);

            // Create services (Application layer)
            IWebhookService webhookService = new WebhookService(webhookRepository);
            IFolderService folderService = new FolderService(folderRepository);

            // Create and run menu handler (Presentation layer)
            var menuHandler = new MenuHandler(webhookService, folderService, console);
            await menuHandler.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}
