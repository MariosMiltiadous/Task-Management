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
            return await _context.Tasks
                        .OrderBy(t =>
                            t.DueDate <= DateTime.UtcNow.AddDays(1) ? 0 :  // Urgent (0)
                            t.DueDate <= DateTime.UtcNow.AddDays(3) ? 1 :  // Normal (1)
                            2 // Low Priority (2)
                        )
                        .ThenBy(t => t.DueDate) // Sort by DueDate within each category
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
