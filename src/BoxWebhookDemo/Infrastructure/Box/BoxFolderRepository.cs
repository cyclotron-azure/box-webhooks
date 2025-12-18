using Box.Sdk.Gen;
using Box.Sdk.Gen.Managers;
using BoxWebhookDemo.Domain.Entities;
using BoxWebhookDemo.Domain.Interfaces;

namespace BoxWebhookDemo.Infrastructure.Box;

/// <summary>
/// Box SDK implementation of the folder repository.
/// Follows Liskov Substitution principle (LSP) - can replace interface anywhere.
/// </summary>
public class BoxFolderRepository : IFolderRepository
{
    private readonly BoxClient _client;

    public BoxFolderRepository(BoxClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<FolderEntity> CreateAsync(
        string name,
        string parentFolderId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Folder name cannot be empty", nameof(name));

        // Ensure parent id is set to root if not provided
        if (string.IsNullOrWhiteSpace(parentFolderId))
            parentFolderId = "0";

        var requestBody = new CreateFolderRequestBody(
            name: name,
            parent: new CreateFolderRequestBodyParentField(id: parentFolderId));

        try
        {
            var folder = await _client.Folders.CreateFolderAsync(requestBody: requestBody);

            return new FolderEntity(
                id: folder.Id ?? string.Empty,
                name: folder.Name ?? name,
                createdAt: folder.CreatedAt?.DateTime);
        }
        catch (HttpRequestException httpEx)
        {
            var status = httpEx.StatusCode.HasValue ? ((int)httpEx.StatusCode).ToString() : "unknown";
            throw new InvalidOperationException($"Box API folder creation HTTP error (status {status}): {httpEx.Message}", httpEx);
        }
        catch (System.Text.Json.JsonException jsonEx)
        {
            throw new InvalidOperationException(
                $"Failed to parse response from Box API when creating folder. Original error: {jsonEx.Message}. This often indicates an empty or non-JSON response (check auth token and network).",
                jsonEx);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Box API folder creation failed: {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<FolderItemEntity>> GetItemsAsync(
        string folderId,
        CancellationToken cancellationToken = default)
    {
        var items = await _client.Folders.GetFolderItemsAsync(folderId: folderId);

        if (items.Entries == null)
            return Array.Empty<FolderItemEntity>();

        var result = new List<FolderItemEntity>();

        foreach (var item in items.Entries)
        {
            if (item.FolderMini != null)
            {
                result.Add(new FolderItemEntity(
                    item.FolderMini.Id ?? string.Empty,
                    item.FolderMini.Name ?? string.Empty,
                    FolderItemType.Folder));
            }
            else if (item.FileFull != null)
            {
                result.Add(new FolderItemEntity(
                    item.FileFull.Id ?? string.Empty,
                    item.FileFull.Name ?? string.Empty,
                    FolderItemType.File));
            }
            else if (item.WebLink != null)
            {
                result.Add(new FolderItemEntity(
                    item.WebLink.Id ?? string.Empty,
                    item.WebLink.Name ?? string.Empty,
                    FolderItemType.WebLink));
            }
        }

        return result;
    }
}
