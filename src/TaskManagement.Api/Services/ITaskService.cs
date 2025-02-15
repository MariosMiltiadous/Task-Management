using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services
{
    public interface ITaskService
    {
        Task<List<TaskModel>> GetAllTasksAsync();
        Task<TaskModel?> GetTaskByIdAsync(int id);
        Task<TaskModel> CreateTaskAsync(TaskModel task);
        Task<bool> UpdateTaskAsync(TaskModel task);
        Task<bool> UpdateMultipleTasksAsync(List<TaskModel> tasksToUpdate);
        Task<bool> DeleteTaskAsync(int id);
    }
}
