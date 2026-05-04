using System.ComponentModel.DataAnnotations;

namespace TaskManager.Tasks;

public record CreateTaskRequest(
    [Required, MinLength(1), MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description,
    DateOnly? DueDate,
    TaskStatus Status,
    TaskPriority Priority
);

public record UpdateTaskRequest(
    [Required, MinLength(1), MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description,
    DateOnly? DueDate,
    TaskStatus Status,
    TaskPriority Priority
);

public record TaskResponse(
    int Id,
    string Title,
    string? Description,
    DateOnly? DueDate,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record TaskListResponse(IEnumerable<TaskResponse> Items, int Total, int Page, int PageSize);

public static class TaskMapper
{
    public static TaskResponse ToResponse(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.DueDate,
        t.Status.ToString(), t.Priority.ToString(),
        t.CreatedAt, t.UpdatedAt
    );
}
