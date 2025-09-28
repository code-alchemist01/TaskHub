using CommandLine;
using TaskMaster.Core.Models;
using TaskMaster.Core.Services;

namespace TaskMaster.CLI.Commands
{
    [Verb("add", HelpText = "Yeni görev ekle")]
    public class AddCommand : BaseCommand
    {
        [Option('t', "title", Required = true, HelpText = "Görev başlığı")]
        public string Title { get; set; } = string.Empty;

        [Option('d', "description", Required = false, HelpText = "Görev açıklaması")]
        public string? Description { get; set; }

        [Option("due", Required = false, HelpText = "Son tarih (dd.MM.yyyy formatında)")]
        public string? DueDate { get; set; }

        [Option('p', "priority", Required = false, Default = TaskPriority.Medium, HelpText = "Öncelik (Low, Medium, High, Critical)")]
        public TaskPriority Priority { get; set; }

        [Option("tags", Required = false, HelpText = "Etiketler (virgülle ayrılmış)")]
        public string? Tags { get; set; }

        private readonly ITaskService _taskService;

        public AddCommand()
        {
            _taskService = TaskService.Instance;
        }

        public override async Task<int> ExecuteAsync()
        {
            try
            {
                var task = new TaskItem
                {
                    Title = Title,
                    Description = Description ?? string.Empty,
                    Priority = Priority,
                    Status = TaskMaster.Core.Models.TaskStatus.Pending
                };

                // Son tarihi parse et
                if (!string.IsNullOrEmpty(DueDate))
                {
                    if (DateTime.TryParseExact(DueDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                    {
                        task.DueDate = parsedDate;
                    }
                    else
                    {
                        Console.WriteLine("Geçersiz tarih formatı. dd.MM.yyyy formatını kullanın (örn: 25.12.2024)");
                        return 1;
                    }
                }

                // Etiketleri parse et
                if (!string.IsNullOrEmpty(Tags))
                {
                    task.Tags = Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(tag => tag.Trim())
                                   .Where(tag => !string.IsNullOrEmpty(tag))
                                   .ToList();
                }

                var createdTask = await _taskService.CreateTaskAsync(task);

                Console.WriteLine($"✓ Görev başarıyla oluşturuldu!");
                Console.WriteLine($"  ID: {createdTask.Id}");
                Console.WriteLine($"  Başlık: {createdTask.Title}");
                Console.WriteLine($"  Öncelik: {GetPriorityText(createdTask.Priority)}");
                
                if (createdTask.DueDate.HasValue)
                {
                    Console.WriteLine($"  Son Tarih: {createdTask.DueDate.Value:dd.MM.yyyy}");
                }

                if (createdTask.Tags.Any())
                {
                    Console.WriteLine($"  Etiketler: {string.Join(", ", createdTask.Tags)}");
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return 1;
            }
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
    }
}