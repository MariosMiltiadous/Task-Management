using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskModel>> GetAllTasksAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.Tasks
                .Select(t => new
                {
                    Task = t,
                    Priority = t.DueDate <= now.AddDays(1) ? TaskPriority.Urgent :  // Due within 24h
                               (t.DueDate > now.AddDays(1) && t.DueDate <= now.AddDays(3)) ? TaskPriority.Normal :  // Due in 2-3 days
                               TaskPriority.Low // Due after 3 days
                })
                .OrderByDescending(t => t.Priority)  // Urgent (2) → Normal (1) → Low (0)
                .ThenBy(t => t.Task.DueDate) // Sort by DueDate within each priority
                .Select(t => t.Task) // Return only TasModel objects
                .ToListAsync();

        }

        public async Task<TaskModel?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<TaskModel> CreateTaskAsync(TaskModel task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        // update ths method to return meaningful messages
        public async Task<(bool Success, string Message)> UpdateTaskAsync(TaskModel task)
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                var existingTask = await _context.Tasks.FindAsync(task.Id);
                if (existingTask == null)
                {
                    return (false, $"Task with ID {task.Id} not found.");
                }

                // --- Rules ---

                // Rule 3: If the task is already marked as "Completed", prevent updating its status
                if (existingTask.Status == Models.TaskStatus.Completed)
                {
                    return (false, $"Task with ID {task.Id} is already marked as 'Completed'. Status cannot be changed.");
                }
                // Rule 1: If setting to "Completed" but due more than 3 days ahead, prevent update
                if (task.Status == Models.TaskStatus.Completed && task.DueDate > currentDate.AddDays(3))
                {
                    return (false, $"Task with ID {task.Id} cannot be marked as 'Completed' because it is due more than 3 days ahead.");
                }

                // Apply updates to allowed fields
                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.DueDate = task.DueDate;
                existingTask.Priority = task.Priority;
                existingTask.Status = task.Status;
                existingTask.UpdatedAt = currentDate;
                // Rule 2: If overdue (past due date) and not completed, set to "Urgent"
                if (existingTask.DueDate < currentDate && existingTask.Status != Models.TaskStatus.Completed)
                {
                    existingTask.Priority = TaskPriority.Urgent;
                }
               

                await _context.SaveChangesAsync();
                return (true, $"Task with ID {task.Id} updated successfully.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating tasks.", ex);
            }
        }

        public async Task<bool> UpdateMultipleTasksAsync(List<TaskModel> tasksToUpdate)
        {
            // Beggin a transaction and in case something goes bad then rollback
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (tasksToUpdate == null || !tasksToUpdate.Any()) return false;

                var taskIds = tasksToUpdate.Select(t => t.Id).ToList();

                // Fetch only existing tasks to prevent errors
                var existingTasks = await _context.Tasks.Where(t => taskIds.Contains(t.Id)).ToListAsync();
                if (!existingTasks.Any()) return false; // No valid tasks to update

                // Map new status updates while keeping other data unchanged
                foreach (var existingTask in existingTasks)
                {
                    var updatedTask = tasksToUpdate.FirstOrDefault(t => t.Id == existingTask.Id);
                    if (updatedTask != null)
                    {
                        existingTask.Title = updatedTask.Title;
                        existingTask.Description = updatedTask.Description;
                        existingTask.DueDate = updatedTask.DueDate;
                        existingTask.Status = updatedTask.Status;
                        existingTask.Priority = updatedTask.Priority;
                        existingTask.UpdatedAt = DateTime.UtcNow;

                        // Mark entity as modified
                        _context.Entry(existingTask).State = EntityState.Modified;
                    }
                }

                // Use Bulk Update for High-Performance Transactions (100K+ records)
                await _context.BulkUpdateAsync(existingTasks);
                // Commit the transaction to ensure changes are persisted
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log the error or handle accordingly
                throw new Exception("An error occurred while updating tasks.", ex);
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
