using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskMangementAPI.Data;
using TaskMangementAPI.DTOs;
using TaskMangementAPI.Models;
using TaskMangementAPI.Common.Exceptions;

namespace TaskMangementAPI.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(AppDbContext context, ILogger<TaskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<TaskResponseDto>> GetAllAsync(TaskQueryParams queryParams)
    {
        var query = _context.Tasks
            .Include(t => t.Project)
            .AsQueryable();

        // Filtering
        if (queryParams.ProjectId.HasValue)
            query = query.Where(t => t.ProjectId == queryParams.ProjectId.Value);

        if (queryParams.Status.HasValue)
            query = query.Where(t => t.Status == queryParams.Status.Value);

        if (queryParams.Priority.HasValue)
            query = query.Where(t => t.Priority == queryParams.Priority.Value);

        if (queryParams.DueDateFrom.HasValue)
            query = query.Where(t => t.DueDate >= queryParams.DueDateFrom.Value);

        if (queryParams.DueDateTo.HasValue)
            query = query.Where(t => t.DueDate <= queryParams.DueDateTo.Value);

        if (!string.IsNullOrWhiteSpace(queryParams.Q))
        {
            var searchTerm = queryParams.Q.ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(searchTerm) || 
                                     (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
        }

        // Sorting
        query = queryParams.SortBy?.ToLower() switch
        {
            "duedate" => queryParams.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "priority" => queryParams.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            _ => queryParams.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((queryParams.Page - 1) * queryParams.Limit)
            .Take(queryParams.Limit)
            .Select(t => ToDto(t))
            .ToListAsync();

        return new PagedResult<TaskResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = queryParams.Page,
            Limit = queryParams.Limit
        };
    }

    public async Task<TaskResponseDto> GetByIdAsync(int id)
    {
        var task = await _context.Tasks
                       .Include(t => t.Project)
                       .FirstOrDefaultAsync(t => t.Id == id)
                   ?? throw new NotFoundException($"Task with id {id} was not found.");
        return ToDto(task);
    }

    public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId);
        if (!projectExists)
            throw new NotFoundException($"Project with id {dto.ProjectId} was not found.");

        if (dto.DueDate.HasValue && dto.DueDate.Value.Date < DateTime.UtcNow.Date)
            throw new ValidationException("Due date cannot be in the past.");

        var task = new ProjectTask
        {
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        
        // Reload to get Project Name
        await _context.Entry(task).Reference(t => t.Project).LoadAsync();
        
        return ToDto(task);
    }

    public async Task<TaskResponseDto> UpdateAsync(int id, UpdateTaskDto dto)
    {
        var task = await _context.Tasks
                       .Include(t => t.Project)
                       .FirstOrDefaultAsync(t => t.Id == id)
                   ?? throw new NotFoundException($"Task with id {id} was not found.");

        if (dto.DueDate.HasValue && dto.DueDate.Value.Date < DateTime.UtcNow.Date)
            throw new ValidationException("Due date cannot be in the past.");

        if (task.Status == ProjectTaskStatus.Done && dto.Status == ProjectTaskStatus.Todo)
        {
            _logger.LogInformation("Task {TaskId} transitioning from Done to Todo.", id);
        }

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToDto(task);
    }

    public async Task DeleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id)
                   ?? throw new NotFoundException($"Task with id {id} was not found.");

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    private static TaskResponseDto ToDto(ProjectTask t) => new()
    {
        Id = t.Id,
        ProjectId = t.ProjectId,
        ProjectName = t.Project?.Name ?? string.Empty,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status.ToString(),
        Priority = t.Priority.ToString(),
        DueDate = t.DueDate,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}