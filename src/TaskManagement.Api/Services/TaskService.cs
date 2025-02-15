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

        // Manybe I need to pass int id ??? TODO
        public async Task<bool> UpdateTaskAsync(TaskModel task)
        {
            _context.Tasks.Update(task);
            return await _context.SaveChangesAsync() > 0;
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
