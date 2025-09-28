using CommandLine;
using TaskMaster.Core.Models;
using TaskMaster.Core.Services;

namespace TaskMaster.CLI.Commands
{
    [Verb("update", HelpText = "Mevcut görevi güncelle")]
    public class UpdateCommand : BaseCommand
    {
        [Value(0, Required = true, HelpText = "Güncellenecek görevin ID'si")]
        public int Id { get; set; }

        [Option('s', "status", Required = false, HelpText = "Yeni durum (Pending, InProgress, Completed, Cancelled)")]
        public string? Status { get; set; }

        [Option('p', "priority", Required = false, HelpText = "Yeni öncelik (Low, Medium, High, Critical)")]
        public string? Priority { get; set; }

        [Option('t', "title", Required = false, HelpText = "Yeni başlık")]
        public string? Title { get; set; }

        [Option('d', "description", Required = false, HelpText = "Yeni açıklama")]
        public string? Description { get; set; }

        [Option("due", Required = false, HelpText = "Yeni son tarih (dd.MM.yyyy formatında, 'clear' yazarak silebilirsiniz)")]
        public string? DueDate { get; set; }

        private readonly ITaskService _taskService;

        public UpdateCommand()
        {
            _taskService = TaskService.Instance;
        }

        public override async Task<int> ExecuteAsync()
        {
            try
            {
                var existingTask = await _taskService.GetTaskByIdAsync(Id);
                if (existingTask == null)
                {
                    Console.WriteLine($"ID {Id} ile görev bulunamadı.");
                    return 1;
                }

                // Mevcut değerleri kopyala
                var updatedTask = new TaskItem
                {
                    Id = existingTask.Id,
                    Title = existingTask.Title,
                    Description = existingTask.Description,
                    CreatedAt = existingTask.CreatedAt,
                    DueDate = existingTask.DueDate,
                    Status = existingTask.Status,
                    Priority = existingTask.Priority,
                    Tags = new List<string>(existingTask.Tags)
                };

                bool hasChanges = false;

                // Durumu güncelle
                if (!string.IsNullOrEmpty(Status))
                {
                    if (Enum.TryParse<TaskMaster.Core.Models.TaskStatus>(Status, true, out var newStatus))
                    {
                        updatedTask.Status = newStatus;
                        hasChanges = true;
                    }
                    else
                    {
                        Console.WriteLine($"Geçersiz durum: {Status}. Geçerli değerler: Pending, InProgress, Completed, Cancelled");
                        return 1;
                    }
                }

                // Önceliği güncelle
                if (!string.IsNullOrEmpty(Priority))
                {
                    if (Enum.TryParse<TaskPriority>(Priority, true, out var newPriority))
                    {
                        updatedTask.Priority = newPriority;
                        hasChanges = true;
                    }
                    else
                    {
                        Console.WriteLine($"Geçersiz öncelik: {Priority}. Geçerli değerler: Low, Medium, High, Critical");
                        return 1;
                    }
                }

                // Başlığı güncelle
                if (!string.IsNullOrEmpty(Title))
                {
                    updatedTask.Title = Title;
                    hasChanges = true;
                }

                // Açıklamayı güncelle
                if (Description != null)
                {
                    updatedTask.Description = Description;
                    hasChanges = true;
                }

                // Son tarihi güncelle
                if (!string.IsNullOrEmpty(DueDate))
                {
                    if (DueDate.Equals("clear", StringComparison.OrdinalIgnoreCase))
                    {
                        updatedTask.DueDate = null;
                        hasChanges = true;
                    }
                    else if (DateTime.TryParseExact(DueDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                    {
                        updatedTask.DueDate = parsedDate;
                        hasChanges = true;
                    }
                    else
                    {
                        Console.WriteLine("Geçersiz tarih formatı. dd.MM.yyyy formatını kullanın (örn: 25.12.2024) veya 'clear' yazın");
                        return 1;
                    }
                }

                if (!hasChanges)
                {
                    Console.WriteLine("Hiçbir değişiklik belirtilmedi.");
                    return 1;
                }

                await _taskService.UpdateTaskAsync(updatedTask);

                Console.WriteLine($"✓ Görev #{Id} başarıyla güncellendi!");
                Console.WriteLine($"  Başlık: {updatedTask.Title}");
                Console.WriteLine($"  Durum: {GetStatusText(updatedTask.Status)}");
                Console.WriteLine($"  Öncelik: {GetPriorityText(updatedTask.Priority)}");
                
                if (updatedTask.DueDate.HasValue)
                {
                    Console.WriteLine($"  Son Tarih: {updatedTask.DueDate.Value:dd.MM.yyyy}");
                }
                else
                {
                    Console.WriteLine("  Son Tarih: Yok");
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
    }
}