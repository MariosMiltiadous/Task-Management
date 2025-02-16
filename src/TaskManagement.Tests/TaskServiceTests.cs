using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagement.Api.Data;
using TaskManagement.Api.Models;
using TaskManagement.Api.Services;
using TaskStatus = TaskManagement.Api.Models.TaskStatus;

namespace TaskManagement.Tests;

public class TaskServiceTests
{
    private readonly TaskService _taskService;
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICacheService> _cacheServiceMock; // Mock the cache service

    public TaskServiceTests()
    {
        // Use In-Memory Database for Testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new ApplicationDbContext(options);
        _cacheServiceMock = new Mock<ICacheService>(); // Create mock instance

        _taskService = new TaskService(_context, _cacheServiceMock.Object);
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

        // Add the task to the in-memory database and save changes
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var retrievedTask = await _taskService.GetTaskByIdAsync(task.Id);

        // Assert
        Assert.NotNull(retrievedTask);
        Assert.Equal(task.Id, retrievedTask.Id);
        Assert.Equal("Do the laundry", retrievedTask.Title);
        Assert.Equal(TaskStatus.InProgress, retrievedTask.Status);
        Assert.Equal(TaskPriority.Normal, retrievedTask.Priority);
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        // Act
        var retrievedTask = await _taskService.GetTaskByIdAsync(151515); // Non-existing ID

        // Assert
        Assert.Null(retrievedTask);
    }

    [Fact]
    public async Task UpdateTask_ShouldModifyTaskDetails()
    {
        // Arrange
        var task = new TaskModel
        {
            Title = "Initial Title",
            Description = "Initial Description",
            DueDate = DateTime.UtcNow.AddDays(3),
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Normal
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Modify the task
        task.Title = "Updated Title";
        task.Description = "Updated Description";
        task.Status = TaskStatus.InProgress;

        // Act
        var (success, message) = await _taskService.UpdateTaskAsync(task);
        var updatedTask = await _taskService.GetTaskByIdAsync(task.Id);

        // Assert
        Assert.True(success);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal("Updated Description", updatedTask.Description);
        Assert.Equal(TaskStatus.InProgress, updatedTask.Status);
    }

    [Fact]
    public async Task UpdateTask_ShouldPreventCompletion_IfDueMoreThan3DaysAhead()
    {
        // Arrange
        var task = new TaskModel
        {
            Title = "Test Rule 1",
            Description = "Bulk updated. This is a description for Task 5",
            DueDate = DateTime.UtcNow.AddDays(5), // More than 3 days ahead
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Normal
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        task.Status = TaskStatus.Completed;
        var (success, message) = await _taskService.UpdateTaskAsync(task);

        // Assert
        Assert.False(success);
        Assert.Contains("cannot be marked as 'Completed'", message);
    }

    [Fact]
    public async Task UpdateTask_ShouldNotUpdate_IfCompleted()
    {
        // Arrange
        var task = new TaskModel
        {
            Title = "Completed Task",
            Description = "This task is already completed",
            DueDate = DateTime.UtcNow.AddDays(-1), // Past due date
            Status = TaskStatus.Completed,
            Priority = TaskPriority.Normal
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Attempt to update Priority
        task.Priority =TaskPriority.Low;

        // Act
        var (success, message) = await _taskService.UpdateTaskAsync(task);

        // Assert
        Assert.False(success);
        Assert.Contains("already marked as 'Completed'", message);
    }

    [Fact]
    public async Task DeleteTask_ShouldRemoveTaskFromDatabase()
    {
        // Arrange
        var task = new TaskModel
        {
            Title = "Task to be deleted",
            Description = "This task will be removed",
            DueDate = DateTime.UtcNow,
            Status = TaskStatus.Pending,
            Priority = TaskPriority.Normal
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.DeleteTaskAsync(task.Id);
        var retrievedTask = await _taskService.GetTaskByIdAsync(task.Id);

        // Assert
        Assert.True(result);
        Assert.Null(retrievedTask);
    }
}