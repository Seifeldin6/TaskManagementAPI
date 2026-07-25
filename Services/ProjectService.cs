using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskMangementAPI.Data;
using TaskMangementAPI.DTOs;
using TaskMangementAPI.Models;
using TaskMangementAPI.Common.Exceptions;

namespace TaskMangementAPI.Services;


public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    public ProjectService(AppDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public async Task<PagedResult<ProjectResponseDto>> GetAllAsync(ProjectQueryParams queryParams)
    {
        var query = _context.Projects.AsQueryable();

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((queryParams.Page - 1) * queryParams.Limit)
            .Take(queryParams.Limit)
            .Select(p => ToDto(p))
            .ToListAsync();

        return new PagedResult<ProjectResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = queryParams.Page,
            Limit = queryParams.Limit
        };
    }

    public async Task<ProjectResponseDto> GetByIdAsync(int id)
    {
        var project = await _context.Projects
                          .Include(p => p.Tasks)
                          .FirstOrDefaultAsync(p => p.Id == id)
                      ?? throw new NotFoundException($"Project with id {id} was not found.");

        return ToDto(project);
    }

    public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto)
    {
        var nameExists = await _context.Projects.AnyAsync(p => p.Name == dto.Name);
        if (nameExists)
            throw new ConflictException($"A project with the name '{dto.Name}' already exists.");

        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return ToDto(project);
    }

    public async Task<ProjectResponseDto> UpdateAsync(int id, UpdateProjectDto dto)
    {
        var project = await _context.Projects.FindAsync(id)
                      ?? throw new NotFoundException($"Project with id {id} was not found.");

        var nameExists = await _context.Projects
            .AnyAsync(p => p.Name == dto.Name && p.Id != id);
        if (nameExists)
            throw new ConflictException($"A project with the name '{dto.Name}' already exists.");

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToDto(project);
    }

    public async Task DeleteAsync(int id)
    {
        var project = await _context.Projects.FindAsync(id)
                      ?? throw new NotFoundException($"Project with id {id} was not found.");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<TaskResponseDto>> GetTasksByProjectIdAsync(int projectId, TaskQueryParams queryParams)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
        if (!projectExists)
            throw new NotFoundException($"Project with id {projectId} was not found.");

        queryParams.ProjectId = projectId;
        var taskService = _serviceProvider.GetRequiredService<ITaskService>();
        return await taskService.GetAllAsync(queryParams);
    }

    private static ProjectResponseDto ToDto(Project p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        TaskCount = p.Tasks?.Count ?? 0,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
    
