using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskManagement.Api.Data;
using TaskManagement.Api.Models;
using TaskManagement.Api.Services;
using TaskStatus = TaskManagement.Api.Models.TaskStatus;

namespace TaskManagement.Tests;

public class TaskServiceTests
{
    private readonly TaskService _taskService;
    private readonly ApplicationDbContext _context;

    public TaskServiceTests()
    {
        // Use In-Memory Database for Testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new ApplicationDbContext(options);
        _taskService = new TaskService(_context);
    }

    [Fact]
    public async Task CreateTask_ShouldAddTaskToDatabase()
    {
        // Arrange
        var task = new TaskModel
        {
            Title = "Test Task",
            Description = "This is a test task",
            DueDate = DateTime.UtcNow.AddDays(2),
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Normal
        };

        // Act
        var createdTask = await _taskService.CreateTaskAsync(task);
        var retrievedTask = await _taskService.GetTaskByIdAsync(createdTask.Id);

        // Assert
        Assert.NotNull(retrievedTask);
        Assert.Equal("Test Task", retrievedTask.Title);
        Assert.Equal(TaskStatus.Pending, retrievedTask.Status);
        Assert.Equal(TaskPriority.Normal, retrievedTask.Priority);
    }

    [Fact]
    public async Task GetTaskById_ShouldFindTaskByValidId()
    {
        // Arrange
        var task = new TaskModel
        {
            Title = "Do the laundry",
            Description = "I have to do the laundry!!!!",
            DueDate = new DateTime(2025, 2, 15, 12, 0, 0),
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.Normal
        };

        // Act
        var retrievedTask = await _taskService.GetTaskByIdAsync(task.Id);

        // Assert
        Assert.NotNull(retrievedTask);
        Assert.Equal(task.Id, retrievedTask.Id);
        Assert.Equal("Valid Task", retrievedTask.Title);
        Assert.Equal(TaskStatus.InProgress, retrievedTask.Status);
        Assert.Equal(TaskPriority.Normal, retrievedTask.Priority);
    }

    // Create Tests for Rules 
}