using CommandLine;
using TaskMaster.Core.Models;
using TaskMaster.Core.Services;

namespace TaskMaster.CLI.Commands
{
    [Verb("search", HelpText = "Görevlerde arama yap")]
    public class SearchCommand : BaseCommand
    {
        [Value(0, Required = true, HelpText = "Arama terimi")]
        public string SearchTerm { get; set; } = string.Empty;

        private readonly ITaskService _taskService;

        public SearchCommand()
        {
            _taskService = TaskService.Instance;
        }

        public override async Task<int> ExecuteAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchTerm))
                {
                    Console.WriteLine("Arama terimi boş olamaz.");
                    return 1;
                }

                var tasks = await _taskService.SearchTasksAsync(SearchTerm);

                if (!tasks.Any())
                {
                    Console.WriteLine($"'{SearchTerm}' için hiç sonuç bulunamadı.");
                    return 0;
                }

                Console.WriteLine($"'{SearchTerm}' için {tasks.Count()} sonuç bulundu:\n");

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
                    
                    // Açıklama varsa ve arama terimini içeriyorsa göster
                    if (!string.IsNullOrEmpty(task.Description) && 
                        task.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"      Açıklama: {TruncateString(task.Description, 60)}");
                    }

                    // Etiketler varsa ve arama terimini içeriyorsa göster
                    var matchingTags = task.Tags.Where(tag => 
                        tag.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    if (matchingTags.Any())
                    {
                        Console.WriteLine($"      Etiketler: {string.Join(", ", matchingTags)}");
                    }
                }

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