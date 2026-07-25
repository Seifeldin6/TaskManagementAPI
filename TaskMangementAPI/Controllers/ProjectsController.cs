using Microsoft.AspNetCore.Mvc;
using TaskMangementAPI.DTOs;
using TaskMangementAPI.Services;

namespace TaskMangementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _service;

    public ProjectsController(IProjectService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectResponseDto>>> GetAll([FromQuery] ProjectQueryParams queryParams)
        => Ok(await _service.GetAllAsync(queryParams));

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectResponseDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<ProjectResponseDto>> Create(CreateProjectDto dto)
    {
        var project = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectResponseDto>> Update(int id, UpdateProjectDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/tasks")]
    public async Task<ActionResult<PagedResult<TaskResponseDto>>> GetTasks(int id, [FromQuery] TaskQueryParams queryParams)
        => Ok(await _service.GetTasksByProjectIdAsync(id, queryParams));

    [HttpPost("{id}/tasks")]
    public async Task<ActionResult<TaskResponseDto>> CreateTask(int id, CreateTaskDto dto, [FromServices] ITaskService taskService)
    {
        dto.ProjectId = id;
        var task = await taskService.CreateAsync(dto);
        return CreatedAtAction("GetById", "Tasks", new { id = task.Id }, task);
    }
}