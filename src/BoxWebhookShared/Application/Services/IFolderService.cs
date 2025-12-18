using BoxWebhookShared.Application.DTOs;
using BoxWebhookShared.Domain.Entities;

namespace BoxWebhookShared.Application.Services;

/// <summary>
/// Application service interface for folder operations.
/// Follows the Interface Segregation principle (ISP).
/// </summary>
public interface IFolderService
{
    /// <summary>
    /// Lists items in a folder.
    /// </summary>
    Task<OperationResult<IReadOnlyList<FolderItemEntity>>> GetFolderItemsAsync(
        string folderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new folder.
    /// </summary>
    Task<OperationResult<FolderEntity>> CreateFolderAsync(
        CreateFolderRequest request,
        CancellationToken cancellationToken = default);
}
