using System;
using TaskMaster.Core.Models;

namespace TaskMaster.Desktop.Models
{
    public class TaskItemViewModel
    {
        public TaskItem Task { get; set; }

        public TaskItemViewModel(TaskItem task)
        {
            Task = task;
        }

        public int Id => Task.Id;
        public string Title => Task.Title;
        public string Description => Task.Description;
        public DateTime CreatedAt => Task.CreatedAt;
        public DateTime? DueDate => Task.DueDate;
        public TaskMaster.Core.Models.TaskStatus Status => Task.Status;
        public TaskPriority Priority => Task.Priority;

        public string StatusText => Status switch
        {
            TaskMaster.Core.Models.TaskStatus.Pending => "Bekliyor",
            TaskMaster.Core.Models.TaskStatus.InProgress => "Devam Ediyor",
            TaskMaster.Core.Models.TaskStatus.Completed => "Tamamlandı",
            TaskMaster.Core.Models.TaskStatus.Cancelled => "İptal Edildi",
            _ => Status.ToString()
        };

        public string PriorityText => Priority switch
        {
            TaskPriority.Low => "Düşük",
            TaskPriority.Medium => "Orta",
            TaskPriority.High => "Yüksek",
            TaskPriority.Critical => "Kritik",
            _ => Priority.ToString()
        };

        public string DueDateText => DueDate?.ToString("dd.MM.yyyy") ?? "Yok";

        public string TagsText => Task.Tags.Count > 0 ? string.Join(", ", Task.Tags) : "Yok";
    }
}