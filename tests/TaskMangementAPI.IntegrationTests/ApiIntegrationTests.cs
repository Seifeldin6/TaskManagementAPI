using System.Net.Http.Json;
using TaskMangementAPI.DTOs;
using TaskMangementAPI.Models;
using Xunit;

namespace TaskMangementAPI.IntegrationTests;

public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FullLifecycle_Works()
    {
        // 1. Create project
        var createProjectDto = new CreateProjectDto { Name = "Lifecycle Project", Description = "Desc" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", createProjectDto);
        projectResponse.EnsureSuccessStatusCode();
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();
        Assert.NotNull(project);

        // 2. Add task
        var createTaskDto = new CreateTaskDto { ProjectId = project.Id, Title = "Lifecycle Task" };
        var taskResponse = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", createTaskDto);
        taskResponse.EnsureSuccessStatusCode();
        var task = await taskResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.NotNull(task);
        Assert.Equal(project.Name, task.ProjectName);

        // 3. Mark task as done
        var updateTaskDto = new UpdateTaskDto { Title = task.Title, Status = ProjectTaskStatus.Done };
        var updateTaskResponse = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", updateTaskDto);
        updateTaskResponse.EnsureSuccessStatusCode();
        var updatedTask = await updateTaskResponse.Content.ReadFromJsonAsync<TaskResponseDto>();
        Assert.Equal("Done", updatedTask!.Status);

        // 4. Delete project
        var deleteResponse = await _client.DeleteAsync($"/api/projects/{project.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        // 5. Verify task is gone
        var getTaskResponse = await _client.GetAsync($"/api/tasks/{task.Id}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, getTaskResponse.StatusCode);
    }

    [Fact]
    public async Task FilterTasks_Works()
    {
        // Arrange
        var projectDto = new CreateProjectDto { Name = "Filter Project" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();

        await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/tasks", new CreateTaskDto { ProjectId = project.Id, Title = "Task 1", Status = ProjectTaskStatus.InProgress, Priority = TaskPriority.High });
        await _client.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", new CreateTaskDto { ProjectId = project.Id, Title = "Task 2", Status = ProjectTaskStatus.Todo, Priority = TaskPriority.Low });

        // Act
        var filteredResponse = await _client.GetAsync("/api/tasks?status=InProgress&priority=High");
        filteredResponse.EnsureSuccessStatusCode();
        var result = await filteredResponse.Content.ReadFromJsonAsync<PagedResult<TaskResponseDto>>();

        // Assert
        Assert.Single(result!.Items);
        Assert.Equal("Task 1", result.Items[0].Title);
    }

    [Fact]
    public async Task SearchAndPagination_Works()
    {
        // Arrange
        var projectDto = new CreateProjectDto { Name = "Search Project" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectDto);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponseDto>();

        for (int i = 1; i <= 15; i++)
        {
            await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/tasks", new CreateTaskDto { ProjectId = project.Id, Title = $"Task {i} keyword" });
        }
        await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/tasks", new CreateTaskDto { ProjectId = project.Id, Title = "Other Task" });

        // Act
        var pagedResponse = await _client.GetAsync("/api/tasks?q=keyword&page=2&limit=10");
        pagedResponse.EnsureSuccessStatusCode();
        var result = await pagedResponse.Content.ReadFromJsonAsync<PagedResult<TaskResponseDto>>();

        // Assert
        Assert.Equal(15, result!.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(5, result.Items.Count);
    }
}
