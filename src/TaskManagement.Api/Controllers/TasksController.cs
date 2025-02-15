﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.Models;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // GET /tasks → List tasks, sorted by urgency.
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        // GET /tasks/{id} → Retrieve a specific task
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            return task == null ? NotFound() : Ok(task);
        }

        // POST /tasks → Create a new task
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskModel task)
        {
            // Check if task id existst within database and error show message
            var existingTask = await _taskService.GetTaskByIdAsync(task.Id);
            if (existingTask != null)
            {
                return Conflict(new { message = $"Task with ID {task.Id} already exists." });
            }

            // Check if status and priority are between enums (0, 1, 2)
            if (!Enum.IsDefined(typeof(Models.TaskStatus), task.Status) || !Enum.IsDefined(typeof(TaskPriority), task.Priority))
            {
                return BadRequest(new { message = "Invalid status or priority." });
            }
            var createdTask = await _taskService.CreateTaskAsync(task);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }
        // PUT /tasks/{id} → Update a task.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskModel updatedTask)
        {
            if (id != updatedTask.Id) return BadRequest();

            // Validates if the status and priority are valid values
            if (!Enum.IsDefined(typeof(Models.TaskStatus), updatedTask.Status) || !Enum.IsDefined(typeof(TaskPriority), updatedTask.Priority))
            {
                return BadRequest(new { message = "Invalid status or priority." });
            }

            var result = await _taskService.UpdateTaskAsync(updatedTask);
            return result ? NoContent() : NotFound();
        }

        // PUT /tasks → Update multiple tasks to a predefined status in a single bulk operation
        [HttpPut]
        public async Task<IActionResult> UpdateMultipleTasks([FromBody] List<TaskModel> tasks)
        {
            if (tasks == null || !tasks.Any())
            {
                return BadRequest(new { message = "Task list cannot be empty." });
            }

            // Validate status and priority for each task
            foreach (var updatedTask in tasks)
            {
                // Check if the status is a valid enum value
                if (!Enum.IsDefined(typeof(Models.TaskStatus), updatedTask.Status))
                {
                    return BadRequest(new { message = $"Invalid status for task with ID {updatedTask.Id}." });
                }

                // Check if the priority is a valid enum value
                if (!Enum.IsDefined(typeof(TaskPriority), updatedTask.Priority))
                {
                    return BadRequest(new { message = $"Invalid priority for task with ID {updatedTask.Id}." });
                }
            }

            var updateSuccess = await _taskService.UpdateMultipleTasksAsync(tasks);

            if (!updateSuccess)
            {
                return NotFound(new { message = "No valid tasks found to update." });
            }

            return NoContent(); // 204 No Content on success
        }

        // DELETE /tasks/{id} → Delete a task.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
