using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TaskMaster.Core.Models;
using TaskMaster.Core.Services;
using TaskMaster.Desktop.Models;
using TaskMaster.Desktop.Windows;

namespace TaskMaster.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly ITaskService _taskService;
        private ObservableCollection<TaskItemViewModel> _tasks;
        private ObservableCollection<TaskItemViewModel> _filteredTasks;

        public MainWindow()
        {
            InitializeComponent();
            _taskService = TaskService.Instance;
            _tasks = new ObservableCollection<TaskItemViewModel>();
            _filteredTasks = new ObservableCollection<TaskItemViewModel>();
            
            DgTasks.ItemsSource = _filteredTasks;
            
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTasksAsync();
        }

        private async Task LoadTasksAsync()
        {
            try
            {
                TxtStatus.Text = "Görevler yükleniyor...";
                
                var tasks = await _taskService.GetAllTasksAsync();
                
                _tasks.Clear();
                foreach (var task in tasks)
                {
                    _tasks.Add(new TaskItemViewModel(task));
                }
                
                ApplyFilters();
                UpdateTaskCount();
                TxtStatus.Text = "Hazır";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Görevler yüklenirken hata oluştu: {ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Hata oluştu";
            }
        }

        private void ApplyFilters()
        {
            var filtered = _tasks.AsEnumerable();

            // Durum filtresi
            if (CmbStatusFilter.SelectedItem is ComboBoxItem statusItem && 
                statusItem.Content.ToString() != "Tümü")
            {
                var statusText = statusItem.Content.ToString();
                var status = statusText switch
                {
                    "Bekliyor" => TaskMaster.Core.Models.TaskStatus.Pending,
                    "Devam Ediyor" => TaskMaster.Core.Models.TaskStatus.InProgress,
                    "Tamamlandı" => TaskMaster.Core.Models.TaskStatus.Completed,
                    "İptal Edildi" => TaskMaster.Core.Models.TaskStatus.Cancelled,
                    _ => (TaskMaster.Core.Models.TaskStatus?)null
                };

                if (status.HasValue)
                {
                    filtered = filtered.Where(t => t.Status == status.Value);
                }
            }

            // Öncelik filtresi
            if (CmbPriorityFilter.SelectedItem is ComboBoxItem priorityItem && 
                priorityItem.Content.ToString() != "Tümü")
            {
                var priorityText = priorityItem.Content.ToString();
                var priority = priorityText switch
                {
                    "Düşük" => TaskPriority.Low,
                    "Orta" => TaskPriority.Medium,
                    "Yüksek" => TaskPriority.High,
                    "Kritik" => TaskPriority.Critical,
                    _ => (TaskPriority?)null
                };

                if (priority.HasValue)
                {
                    filtered = filtered.Where(t => t.Priority == priority.Value);
                }
            }

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(TxtSearch.Text))
            {
                var searchTerm = TxtSearch.Text.ToLower();
                filtered = filtered.Where(t => 
                    t.Title.ToLower().Contains(searchTerm) ||
                    t.Description.ToLower().Contains(searchTerm) ||
                    t.Task.Tags.Any(tag => tag.ToLower().Contains(searchTerm))
                );
            }

            _filteredTasks.Clear();
            foreach (var task in filtered.OrderBy(t => t.Id))
            {
                _filteredTasks.Add(task);
            }
        }

        private void UpdateTaskCount()
        {
            TxtTaskCount.Text = $"{_filteredTasks.Count} görev";
        }

        private async void BtnAddTask_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditTaskWindow();
            if (addWindow.ShowDialog() == true)
            {
                await LoadTasksAsync();
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadTasksAsync();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_tasks != null)
            {
                ApplyFilters();
                UpdateTaskCount();
            }
        }

        private void CmbPriorityFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_tasks != null)
            {
                ApplyFilters();
                UpdateTaskCount();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_tasks != null)
            {
                ApplyFilters();
                UpdateTaskCount();
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TaskItemViewModel taskViewModel)
            {
                var editWindow = new AddEditTaskWindow(taskViewModel.Task);
                if (editWindow.ShowDialog() == true)
                {
                    await LoadTasksAsync();
                }
            }
        }

        private async void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TaskItemViewModel taskViewModel)
            {
                try
                {
                    var task = taskViewModel.Task;
                    task.Status = TaskMaster.Core.Models.TaskStatus.Completed;
                    
                    await _taskService.UpdateTaskAsync(task);
                    await LoadTasksAsync();
                    
                    TxtStatus.Text = $"Görev #{task.Id} tamamlandı olarak işaretlendi";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Görev güncellenirken hata oluştu: {ex.Message}", 
                                  "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TaskItemViewModel taskViewModel)
            {
                var result = MessageBox.Show(
                    $"'{taskViewModel.Title}' görevini silmek istediğinizden emin misiniz?",
                    "Görevi Sil",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _taskService.DeleteTaskAsync(taskViewModel.Id);
                        await LoadTasksAsync();
                        
                        TxtStatus.Text = $"Görev #{taskViewModel.Id} silindi";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Görev silinirken hata oluştu: {ex.Message}", 
                                      "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}