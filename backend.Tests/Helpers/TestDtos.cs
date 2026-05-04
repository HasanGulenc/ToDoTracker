namespace TaskManager.Tests.Helpers;

public record TaskResponse(
    int Id,
    string Title,
    string? Description,
    string? DueDate,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TaskListResponse(IEnumerable<TaskResponse> Items, int Total, int Page, int PageSize);
