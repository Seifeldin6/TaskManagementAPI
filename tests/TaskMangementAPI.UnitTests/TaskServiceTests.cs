using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskMangementAPI.Common.Exceptions;
using TaskMangementAPI.Data;
using TaskMangementAPI.DTOs;
using TaskMangementAPI.Models;
using TaskMangementAPI.Services;
using Xunit;

namespace TaskMangementAPI.UnitTests;

public class TaskServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<TaskService>> _loggerMock;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _loggerMock = new Mock<ILogger<TaskService>>();
        _service = new TaskService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithPastDueDate_ThrowsValidationException()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var dto = new CreateTaskDto
        {
            ProjectId = project.Id,
            Title = "Task 1",
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WithTodayOrFutureDueDate_Succeeds()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var dtoToday = new CreateTaskDto
        {
            ProjectId = project.Id,
            Title = "Task Today",
            DueDate = DateTime.UtcNow
        };

        var dtoFuture = new CreateTaskDto
        {
            ProjectId = project.Id,
            Title = "Task Future",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var resultToday = await _service.CreateAsync(dtoToday);
        var resultFuture = await _service.CreateAsync(dtoFuture);

        // Assert
        Assert.NotNull(resultToday);
        Assert.NotNull(resultFuture);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidProjectId_ThrowsNotFoundException()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            ProjectId = 999,
            Title = "Task 1"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_DoneToTodoTransition_IsLogged()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var task = new ProjectTask
        {
            ProjectId = project.Id,
            Title = "Task 1",
            Status = ProjectTaskStatus.Done
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var dto = new UpdateTaskDto
        {
            Title = "Task 1 Updated",
            Status = ProjectTaskStatus.Todo
        };

        // Act
        await _service.UpdateAsync(task.Id, dto);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("transitioning from Done to Todo")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
