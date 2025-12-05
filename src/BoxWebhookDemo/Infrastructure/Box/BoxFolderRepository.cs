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
        var requestBody = new CreateFolderRequestBody(
            name: name,
            parent: new CreateFolderRequestBodyParentField(id: parentFolderId));

        var folder = await _client.Folders.CreateFolderAsync(requestBody: requestBody);

        return new FolderEntity(
            id: folder.Id ?? string.Empty,
            name: folder.Name ?? name,
            createdAt: folder.CreatedAt?.DateTime);
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
