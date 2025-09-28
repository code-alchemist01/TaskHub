using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TaskMaster.Core.Models;
using TaskMaster.Core.Services;

namespace TaskMaster.Desktop.Windows
{
    public partial class AddEditTaskWindow : Window
    {
        private readonly ITaskService _taskService;
        private readonly TaskItem? _existingTask;
        private readonly bool _isEditMode;

        public AddEditTaskWindow() : this(null)
        {
        }

        public AddEditTaskWindow(TaskItem? existingTask)
        {
            InitializeComponent();
            _taskService = TaskService.Instance;
            _existingTask = existingTask;
            _isEditMode = existingTask != null;

            InitializeForm();
        }

        private void InitializeForm()
        {
            if (_isEditMode && _existingTask != null)
            {
                // Edit mode
                TxtHeader.Text = "Görevi Düzenle";
                Title = "Görevi Düzenle";
                BtnSave.Content = "Güncelle";

                // Show status field in edit mode
                LblStatus.Visibility = Visibility.Visible;
                CmbStatus.Visibility = Visibility.Visible;

                // Populate form with existing task data
                TxtTitle.Text = _existingTask.Title;
                TxtDescription.Text = _existingTask.Description;
                DpDueDate.SelectedDate = _existingTask.DueDate;
                TxtTags.Text = string.Join(", ", _existingTask.Tags);

                // Set priority
                foreach (ComboBoxItem item in CmbPriority.Items)
                {
                    if (item.Tag.ToString() == _existingTask.Priority.ToString())
                    {
                        CmbPriority.SelectedItem = item;
                        break;
                    }
                }

                // Set status
                foreach (ComboBoxItem item in CmbStatus.Items)
                {
                    if (item.Tag.ToString() == _existingTask.Status.ToString())
                    {
                        CmbStatus.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                // Add mode
                TxtHeader.Text = "Yeni Görev Ekle";
                Title = "Yeni Görev Ekle";
                BtnSave.Content = "Ekle";
            }

            // Focus on title field
            TxtTitle.Focus();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                BtnSave.IsEnabled = false;
                BtnCancel.IsEnabled = false;

                var task = CreateTaskFromForm();

                if (_isEditMode && _existingTask != null)
                {
                    // Update existing task
                    task.Id = _existingTask.Id;
                    task.CreatedAt = _existingTask.CreatedAt;
                    task.UpdatedAt = DateTime.Now;

                    await _taskService.UpdateTaskAsync(task);
                }
                else
                {
                    // Create new task
                    await _taskService.CreateTaskAsync(task);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Görev kaydedilirken hata oluştu: {ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnSave.IsEnabled = true;
                BtnCancel.IsEnabled = true;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateForm()
        {
            TxtValidation.Visibility = Visibility.Collapsed;

            // Title is required
            if (string.IsNullOrWhiteSpace(TxtTitle.Text))
            {
                ShowValidationError("Başlık alanı zorunludur.");
                TxtTitle.Focus();
                return false;
            }

            // Title length check
            if (TxtTitle.Text.Length > 200)
            {
                ShowValidationError("Başlık en fazla 200 karakter olabilir.");
                TxtTitle.Focus();
                return false;
            }

            // Due date check (cannot be in the past for new tasks)
            if (!_isEditMode && DpDueDate.SelectedDate.HasValue && 
                DpDueDate.SelectedDate.Value.Date < DateTime.Today)
            {
                ShowValidationError("Bitiş tarihi bugünden önce olamaz.");
                DpDueDate.Focus();
                return false;
            }

            return true;
        }

        private void ShowValidationError(string message)
        {
            TxtValidation.Text = message;
            TxtValidation.Visibility = Visibility.Visible;
        }

        private TaskItem CreateTaskFromForm()
        {
            var task = new TaskItem
            {
                Title = TxtTitle.Text.Trim(),
                Description = TxtDescription.Text.Trim(),
                DueDate = DpDueDate.SelectedDate,
                Priority = GetSelectedPriority(),
                Status = _isEditMode ? GetSelectedStatus() : TaskMaster.Core.Models.TaskStatus.Pending,
                Tags = ParseTags(TxtTags.Text),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return task;
        }

        private TaskPriority GetSelectedPriority()
        {
            if (CmbPriority.SelectedItem is ComboBoxItem selectedItem)
            {
                return Enum.Parse<TaskPriority>(selectedItem.Tag.ToString()!);
            }
            return TaskPriority.Medium;
        }

        private TaskMaster.Core.Models.TaskStatus GetSelectedStatus()
        {
            if (CmbStatus.SelectedItem is ComboBoxItem selectedItem)
            {
                return Enum.Parse<TaskMaster.Core.Models.TaskStatus>(selectedItem.Tag.ToString()!);
            }
            return TaskMaster.Core.Models.TaskStatus.Pending;
        }

        private List<string> ParseTags(string tagsText)
        {
            if (string.IsNullOrWhiteSpace(tagsText))
                return new List<string>();

            return tagsText
                .Split(',')
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct()
                .ToList();
        }
    }
}