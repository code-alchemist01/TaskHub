using CommandLine;
using TaskMaster.Core.Models;
using TaskMaster.Core.Services;

namespace TaskMaster.CLI.Commands
{
    [Verb("list", HelpText = "Görevleri listele")]
    public class ListCommand : BaseCommand
    {
        [Option('s', "status", Required = false, HelpText = "Duruma göre filtrele (Pending, InProgress, Completed, Cancelled)")]
        public string? Status { get; set; }

        [Option('p', "priority", Required = false, HelpText = "Önceliğe göre filtrele (Low, Medium, High, Critical)")]
        public string? Priority { get; set; }

        [Option('t', "tag", Required = false, HelpText = "Etikete göre filtrele")]
        public string? Tag { get; set; }

        [Option("overdue", Required = false, HelpText = "Sadece gecikmiş görevleri göster")]
        public bool ShowOverdue { get; set; }

        private readonly ITaskService _taskService;

        public ListCommand()
        {
            _taskService = TaskService.Instance;
        }

        public override async Task<int> ExecuteAsync()
        {
            try
            {
                IEnumerable<TaskItem> tasks;

                if (ShowOverdue)
                {
                    tasks = await _taskService.GetOverdueTasksAsync();
                }
                else if (!string.IsNullOrEmpty(Status) && Enum.TryParse<TaskMaster.Core.Models.TaskStatus>(Status, true, out var status))
                {
                    tasks = await _taskService.GetTasksByStatusAsync(status);
                }
                else if (!string.IsNullOrEmpty(Priority) && Enum.TryParse<TaskPriority>(Priority, true, out var priority))
                {
                    tasks = await _taskService.GetTasksByPriorityAsync(priority);
                }
                else if (!string.IsNullOrEmpty(Tag))
                {
                    tasks = await _taskService.GetTasksByTagAsync(Tag);
                }
                else
                {
                    tasks = await _taskService.GetAllTasksAsync();
                }

                if (!tasks.Any())
                {
                    Console.WriteLine("Hiç görev bulunamadı.");
                    return 0;
                }

                Console.WriteLine($"{"ID",-5} {"Başlık",-30} {"Durum",-12} {"Öncelik",-10} {"Son Tarih",-12}");
                Console.WriteLine(new string('-', 75));

                foreach (var task in tasks.OrderBy(t => t.Id))
                {
                    var dueDate = task.DueDate?.ToString("dd.MM.yyyy") ?? "Yok";
                    var statusText = task.Status switch
                    {
                        TaskMaster.Core.Models.TaskStatus.Pending => "Bekliyor",
                        TaskMaster.Core.Models.TaskStatus.InProgress => "Devam Ediyor",
                        TaskMaster.Core.Models.TaskStatus.Completed => "Tamamlandı",
                        TaskMaster.Core.Models.TaskStatus.Cancelled => "İptal Edildi",
                        _ => "Bilinmiyor"
                    };
                    var priorityText = GetPriorityText(task.Priority);

                    Console.WriteLine($"{task.Id,-5} {TruncateString(task.Title, 30),-30} {statusText,-12} {priorityText,-10} {dueDate,-12}");
                }

                Console.WriteLine($"\nToplam: {tasks.Count()} görev");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return 1;
            }
        }

        private static string GetStatusText(TaskMaster.Core.Models.TaskStatus status)
        {
            return status switch
            {
                TaskMaster.Core.Models.TaskStatus.Pending => "Bekliyor",
                TaskMaster.Core.Models.TaskStatus.InProgress => "Devam Ediyor",
                TaskMaster.Core.Models.TaskStatus.Completed => "Tamamlandı",
                TaskMaster.Core.Models.TaskStatus.Cancelled => "İptal Edildi",
                _ => status.ToString()
            };
        }

        private static string GetPriorityText(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => "Düşük",
                TaskPriority.Medium => "Orta",
                TaskPriority.High => "Yüksek",
                TaskPriority.Critical => "Kritik",
                _ => priority.ToString()
            };
        }

        private static string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input ?? string.Empty;

            return input.Substring(0, maxLength - 3) + "...";
        }
    }
}