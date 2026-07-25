
using TaskMangementAPI.DTOs;


namespace TaskMangementAPI.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectResponseDto>> GetAllAsync(ProjectQueryParams queryParams);
    Task<ProjectResponseDto> GetByIdAsync(int id);
    Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto);
    Task<ProjectResponseDto> UpdateAsync(int id, UpdateProjectDto dto);
    Task DeleteAsync(int id);
    Task<PagedResult<TaskResponseDto>> GetTasksByProjectIdAsync(int projectId, TaskQueryParams queryParams);
}