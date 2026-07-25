using Microsoft.EntityFrameworkCore;
using TaskMangementAPI.Common.Exceptions;
using TaskMangementAPI.Data;
using TaskMangementAPI.DTOs;
using TaskMangementAPI.Models;
using TaskMangementAPI.Services;
using Xunit;

namespace TaskMangementAPI.UnitTests;

public class ProjectServiceTests
{
    private readonly AppDbContext _context;
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        // We can pass null for serviceProvider if we don't call GetTasksByProjectIdAsync in these tests
        _service = new ProjectService(_context, null!);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsConflictException()
    {
        // Arrange
        var existingProject = new Project { Name = "Existing Project" };
        _context.Projects.Add(existingProject);
        await _context.SaveChangesAsync();

        var dto = new CreateProjectDto { Name = "Existing Project" };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ThrowsConflictException()
    {
        // Arrange
        var project1 = new Project { Name = "Project 1" };
        var project2 = new Project { Name = "Project 2" };
        _context.Projects.AddRange(project1, project2);
        await _context.SaveChangesAsync();

        var dto = new UpdateProjectDto { Name = "Project 2" };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(project1.Id, dto));
    }
}
