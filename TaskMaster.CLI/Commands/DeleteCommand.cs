using CommandLine;
using TaskMaster.Core.Services;

namespace TaskMaster.CLI.Commands
{
    [Verb("delete", HelpText = "Görevi sil")]
    public class DeleteCommand : BaseCommand
    {
        [Value(0, Required = true, HelpText = "Silinecek görevin ID'si")]
        public int Id { get; set; }

        [Option('f', "force", Required = false, HelpText = "Onay istemeden sil")]
        public bool Force { get; set; }

        private readonly ITaskService _taskService;

        public DeleteCommand()
        {
            _taskService = TaskService.Instance;
        }

        public override async Task<int> ExecuteAsync()
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(Id);
                if (task == null)
                {
                    Console.WriteLine($"ID {Id} ile görev bulunamadı.");
                    return 1;
                }

                if (!Force)
                {
                    Console.WriteLine($"Aşağıdaki görevi silmek istediğinizden emin misiniz?");
                    Console.WriteLine($"  ID: {task.Id}");
                    Console.WriteLine($"  Başlık: {task.Title}");
                    Console.WriteLine($"  Durum: {GetStatusText(task.Status)}");
                    Console.Write("\nDevam etmek için 'y' veya 'yes' yazın: ");
                    
                    var confirmation = Console.ReadLine()?.Trim().ToLower();
                    if (confirmation != "y" && confirmation != "yes")
                    {
                        Console.WriteLine("İşlem iptal edildi.");
                        return 0;
                    }
                }

                var deleted = await _taskService.DeleteTaskAsync(Id);
                if (deleted)
                {
                    Console.WriteLine($"✓ Görev #{Id} başarıyla silindi.");
                    return 0;
                }
                else
                {
                    Console.WriteLine($"Görev #{Id} silinemedi.");
                    return 1;
                }
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
    }
}