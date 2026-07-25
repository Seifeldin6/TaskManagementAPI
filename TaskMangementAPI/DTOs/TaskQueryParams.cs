using TaskMangementAPI.Models;

namespace TaskMangementAPI.DTOs;

public class TaskQueryParams
{
    public int? ProjectId { get; set; }
    public string? Q { get; set; }
    public ProjectTaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string? SortBy { get; set; } // dueDate, priority, createdAt
    public string? SortOrder { get; set; } // asc, desc
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
}
