using BoxWebhookDemo.Application.DTOs;
using BoxWebhookDemo.Domain.Entities;
using BoxWebhookDemo.Domain.Interfaces;

namespace BoxWebhookDemo.Application.Services;

/// <summary>
/// Application service for folder operations.
/// Orchestrates domain logic and repository operations.
/// Follows Single Responsibility principle (SRP).
/// </summary>
public class FolderService : IFolderService
{
    private readonly IFolderRepository _folderRepository;

    public FolderService(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository ?? throw new ArgumentNullException(nameof(folderRepository));
    }

    public async Task<OperationResult<IReadOnlyList<FolderItemEntity>>> GetFolderItemsAsync(
        string folderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(folderId))
                folderId = "0"; // Default to root folder

            var items = await _folderRepository.GetItemsAsync(folderId, cancellationToken);
            return OperationResult<IReadOnlyList<FolderItemEntity>>.Success(items);
        }
        catch (Exception ex)
        {
            return OperationResult<IReadOnlyList<FolderItemEntity>>.Failure($"Failed to list folder items: {ex.Message}");
        }
    }

    public async Task<OperationResult<FolderEntity>> CreateFolderAsync(
        CreateFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var name = string.IsNullOrWhiteSpace(request.Name)
                ? $"WebhookTest_{DateTime.Now:yyyyMMdd_HHmmss}"
                : request.Name;

            var parentId = string.IsNullOrWhiteSpace(request.ParentFolderId)
                ? "0"
                : request.ParentFolderId;

            var folder = await _folderRepository.CreateAsync(name, parentId, cancellationToken);
            return OperationResult<FolderEntity>.Success(folder);
        }
        catch (Exception ex)
        {
            return OperationResult<FolderEntity>.Failure($"Failed to create folder: {ex.Message}");
        }
    }
}
