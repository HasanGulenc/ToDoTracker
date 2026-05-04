using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Common;
using TaskManager.Data;

namespace TaskManager.Tasks;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController(AppDbContext db) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<TaskListResponse>> GetAll(
        [FromQuery] TaskStatus? status,
        [FromQuery] TaskPriority? priority,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var query = db.Tasks.Where(t => t.UserId == CurrentUserId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        query = (sortBy?.ToLower(), sortDir?.ToLower() == "desc") switch
        {
            ("duedate", false) => query.OrderBy(t => t.DueDate),
            ("duedate", true) => query.OrderByDescending(t => t.DueDate),
            ("priority", false) => query.OrderBy(t => t.Priority),
            ("priority", true) => query.OrderByDescending(t => t.Priority),
            ("createdat", false) => query.OrderBy(t => t.CreatedAt),
            ("createdat", true) => query.OrderByDescending(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt),
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => TaskMapper.ToResponse(t))
            .ToListAsync();

        return Ok(new TaskListResponse(items, total, page, pageSize));
    }

    [HttpGet("due-today")]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetDueToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = await db.Tasks
            .Where(t => t.UserId == CurrentUserId && t.DueDate == today)
            .OrderBy(t => t.Priority)
            .Select(t => TaskMapper.ToResponse(t))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId)
            ?? throw new NotFoundException($"Task {id} not found.");

        return Ok(TaskMapper.ToResponse(task));
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest req)
    {
        var task = new TaskItem
        {
            Title = req.Title,
            Description = req.Description,
            DueDate = req.DueDate,
            Status = req.Status,
            Priority = req.Priority,
            UserId = CurrentUserId,
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, TaskMapper.ToResponse(task));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskResponse>> Update(int id, UpdateTaskRequest req)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId)
            ?? throw new NotFoundException($"Task {id} not found.");

        task.Title = req.Title;
        task.Description = req.Description;
        task.DueDate = req.DueDate;
        task.Status = req.Status;
        task.Priority = req.Priority;
        task.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Ok(TaskMapper.ToResponse(task));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId)
            ?? throw new NotFoundException($"Task {id} not found.");

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
