using BoxWebhookDemo.Domain.Entities;

namespace BoxWebhookDemo.Domain.Interfaces;

/// <summary>
/// Repository interface for folder operations.
/// Follows the Repository pattern from DDD.
/// </summary>
public interface IFolderRepository
{
    /// <summary>
    /// Creates a new folder.
    /// </summary>
    Task<FolderEntity> CreateAsync(
        string name,
        string parentFolderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets items within a folder.
    /// </summary>
    Task<IReadOnlyList<FolderItemEntity>> GetItemsAsync(
        string folderId,
        CancellationToken cancellationToken = default);
}
