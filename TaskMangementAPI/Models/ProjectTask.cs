using System;

namespace TaskMangementAPI.Models;

public enum ProjectTaskStatus { Todo, InProgress, Done }
public enum TaskPriority { Low, Medium, High }

public class ProjectTask
{
    public int Id { get; set; } 
    public int ProjectId { get; set; } 
    public string Title { get; set; } = string.Empty; 
    public string? Description { get; set; } 
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.Todo; 
    public TaskPriority Priority { get; set; } = TaskPriority.Medium; 
    public DateTime? DueDate { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; 

    public Project? Project { get; set; }
}