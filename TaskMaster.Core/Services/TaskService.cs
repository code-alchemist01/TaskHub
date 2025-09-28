using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TaskMaster.Core.Models;

namespace TaskMaster.Core.Services
{
    public class TaskService : ITaskService
    {
        private static readonly Lazy<TaskService> _instance = new Lazy<TaskService>(() => new TaskService());
        public static TaskService Instance => _instance.Value;

        private readonly List<TaskItem> _tasks;
        private int _nextId = 1;
        private readonly string _dataFilePath;

        private TaskService()
        {
            _tasks = new List<TaskItem>();
            _dataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TaskMaster", "tasks.json");
            LoadData();
        }

        public Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            return Task.FromResult(_tasks.AsEnumerable());
        }

        public Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(task);
        }

        public Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            // Debug: Log the task creation attempt
            System.Diagnostics.Debug.WriteLine($"Creating task: {task.Title}");
            
            task.Id = _nextId++;
            task.CreatedAt = DateTime.Now;
            task.UpdatedAt = DateTime.Now;
            _tasks.Add(task);
            
            // Debug: Log the task list count before saving
            System.Diagnostics.Debug.WriteLine($"Task added to list. Total tasks: {_tasks.Count}");
            
            SaveData();
            
            // Debug: Confirm task creation
            System.Diagnostics.Debug.WriteLine($"Task created successfully with ID: {task.Id}");
            
            return Task.FromResult(task);
        }

        public Task<TaskItem> UpdateTaskAsync(TaskItem task)
        {
            var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existingTask != null)
            {
                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.DueDate = task.DueDate;
                existingTask.Status = task.Status;
                existingTask.Priority = task.Priority;
                existingTask.Tags = task.Tags;
                existingTask.UpdatedAt = DateTime.Now;
                SaveData();
                return Task.FromResult(existingTask);
            }
            throw new ArgumentException($"Task with ID {task.Id} not found.");
        }

        public Task<bool> DeleteTaskAsync(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                _tasks.Remove(task);
                SaveData();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<IEnumerable<TaskItem>> SearchTasksAsync(string searchTerm)
        {
            var results = _tasks.Where(t => 
                t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Tags.Any(tag => tag.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            );
            return Task.FromResult(results);
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(Models.TaskStatus status)
        {
            var results = _tasks.Where(t => t.Status == status);
            return await Task.FromResult(results);
        }

        public Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(TaskPriority priority)
        {
            var results = _tasks.Where(t => t.Priority == priority);
            return Task.FromResult(results);
        }

        public Task<IEnumerable<TaskItem>> GetTasksByTagAsync(string tag)
        {
            var results = _tasks.Where(t => t.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
            return Task.FromResult(results);
        }

        public Task<IEnumerable<TaskItem>> GetOverdueTasksAsync()
        {
            var now = DateTime.Now;
            var results = _tasks.Where(t => 
                t.DueDate.HasValue && 
                t.DueDate.Value < now && 
                t.Status != Models.TaskStatus.Completed
            );
            return Task.FromResult(results);
        }

        private void SeedData()
        {
            var sampleTasks = new[]
            {
                new TaskItem
                {
                    Id = _nextId++,
                    Title = "Proje planlaması yap",
                    Description = "Yeni proje için detaylı planlama ve zaman çizelgesi oluştur",
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now.AddDays(-5),
                    DueDate = DateTime.Now.AddDays(3),
                    Status = Models.TaskStatus.InProgress,
                    Priority = TaskPriority.High,
                    Tags = new List<string> { "planlama", "proje", "önemli" }
                },
                new TaskItem
                {
                    Id = _nextId++,
                    Title = "Kod review yap",
                    Description = "Takım arkadaşlarının kodlarını gözden geçir",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    UpdatedAt = DateTime.Now.AddDays(-2),
                    DueDate = DateTime.Now.AddDays(1),
                    Status = Models.TaskStatus.Pending,
                    Priority = TaskPriority.Medium,
                    Tags = new List<string> { "kod", "review", "takım" }
                },
                new TaskItem
                {
                    Id = _nextId++,
                    Title = "Dokümantasyon güncelle",
                    Description = "API dokümantasyonunu son değişikliklere göre güncelle",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now.AddDays(-1),
                    Status = Models.TaskStatus.Completed,
                    Priority = TaskPriority.Low,
                    Tags = new List<string> { "dokümantasyon", "api" }
                }
            };

            _tasks.AddRange(sampleTasks);
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var json = File.ReadAllText(_dataFilePath);
                    var data = JsonSerializer.Deserialize<TaskData>(json);
                    if (data != null)
                    {
                        _tasks.Clear();
                        _tasks.AddRange(data.Tasks);
                        _nextId = data.NextId;
                        return;
                    }
                }
            }
            catch (Exception)
            {
                // If loading fails, fall back to seed data
            }
            
            // If no file exists or loading failed, create seed data
            SeedData();
        }

        private void SaveData()
        {
            try
            {
                var directory = Path.GetDirectoryName(_dataFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                var data = new TaskData
                {
                    Tasks = _tasks,
                    NextId = _nextId
                };

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_dataFilePath, json);
                
                // Debug: Verify the file was written
                System.Diagnostics.Debug.WriteLine($"Data saved successfully. Tasks count: {_tasks.Count}, File: {_dataFilePath}");
            }
            catch (Exception ex)
            {
                // Log the error and rethrow to make it visible
                System.Diagnostics.Debug.WriteLine($"SaveData failed: {ex.Message}");
                throw new InvalidOperationException($"Veri kaydedilemedi: {ex.Message}", ex);
            }
        }

        private class TaskData
        {
            public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
            public int NextId { get; set; } = 1;
        }
    }
}