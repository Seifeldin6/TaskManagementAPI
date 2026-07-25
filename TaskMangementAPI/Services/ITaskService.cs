using TaskMangementAPI.DTOs;

namespace TaskMangementAPI.Services;

public interface ITaskService
{
    Task<PagedResult<TaskResponseDto>> GetAllAsync(TaskQueryParams queryParams);
    Task<TaskResponseDto> GetByIdAsync(int id);
    Task<TaskResponseDto> CreateAsync(CreateTaskDto dto);
    Task<TaskResponseDto> UpdateAsync(int id, UpdateTaskDto dto);
    Task DeleteAsync(int id);
}