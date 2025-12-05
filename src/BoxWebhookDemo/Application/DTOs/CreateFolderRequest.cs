namespace BoxWebhookDemo.Application.DTOs;

/// <summary>
/// Data Transfer Object for folder creation requests.
/// </summary>
public record CreateFolderRequest(
    string Name,
    string ParentFolderId);
