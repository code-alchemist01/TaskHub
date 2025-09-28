using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskMaster.Core.Models;

namespace TaskMaster.Core.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> GetAllTasksAsync();
        Task<TaskItem?> GetTaskByIdAsync(int id);
        Task<TaskItem> CreateTaskAsync(TaskItem task);
        Task<TaskItem> UpdateTaskAsync(TaskItem task);
        Task<bool> DeleteTaskAsync(int id);
        Task<IEnumerable<TaskItem>> SearchTasksAsync(string searchTerm);
        Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(Models.TaskStatus status);
        Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(TaskPriority priority);
        Task<IEnumerable<TaskItem>> GetTasksByTagAsync(string tag);
        Task<IEnumerable<TaskItem>> GetOverdueTasksAsync();
    }
}