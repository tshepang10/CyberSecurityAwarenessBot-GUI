using CyberSecurityAwarenessBot.Models;
using CyberSecurityAwarenessBot.Services;

namespace CyberSecurityAwarenessBot.UI;

internal sealed class TaskManagerForm : Form
{
    private readonly TaskService _taskService;
    private readonly ActivityLogService _activityLog;
    private readonly ListView _taskList;
    private readonly TextBox _titleInput;
    private readonly TextBox _descriptionInput;
    private readonly NumericUpDown _reminderDays;

    public TaskManagerForm(TaskService taskService, ActivityLogService activityLog)
    {
        _taskService = taskService;
        _activityLog = activityLog;

        Text = "Cybersecurity Task Assistant";
        Size = new Size(760, 560);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = GuiTheme.Background;
        Font = new Font("Segoe UI", 10F);

        var header = new Label
        {
            Dock = DockStyle.Top,
            Height = 48,
            Text = "Manage your cybersecurity tasks and reminders",
            ForeColor = GuiTheme.AccentCyan,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Padding = new Padding(16, 12, 16, 0)
        };

        _taskList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BackColor = GuiTheme.InputBackground,
            ForeColor = GuiTheme.TextPrimary,
            BorderStyle = BorderStyle.None
        };
        _taskList.Columns.Add("ID", 50);
        _taskList.Columns.Add("Title", 180);
        _taskList.Columns.Add("Description", 260);
        _taskList.Columns.Add("Reminder", 120);
        _taskList.Columns.Add("Status", 80);

        var formPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 180,
            BackColor = GuiTheme.PanelDark,
            Padding = new Padding(16)
        };

        _titleInput = CreateInput("Task title", new Point(16, 16), new Size(320, 28));
        _descriptionInput = CreateInput("Description", new Point(16, 56), new Size(480, 28));
        _reminderDays = new NumericUpDown
        {
            Location = new Point(16, 96),
            Size = new Size(80, 28),
            Minimum = 0,
            Maximum = 365,
            BackColor = GuiTheme.InputBackground,
            ForeColor = GuiTheme.TextPrimary
        };

        formPanel.Controls.Add(new Label
        {
            Text = "Title",
            ForeColor = GuiTheme.TextPrimary,
            Location = new Point(16, 0),
            AutoSize = true
        });
        formPanel.Controls.Add(new Label
        {
            Text = "Description",
            ForeColor = GuiTheme.TextPrimary,
            Location = new Point(16, 40),
            AutoSize = true
        });
        formPanel.Controls.Add(new Label
        {
            Text = "Reminder (days from now, 0 = none)",
            ForeColor = GuiTheme.TextPrimary,
            Location = new Point(16, 80),
            AutoSize = true
        });
        formPanel.Controls.Add(_titleInput);
        formPanel.Controls.Add(_descriptionInput);
        formPanel.Controls.Add(_reminderDays);

        var addButton = CreateActionButton("Add Task", new Point(520, 16), GuiTheme.AccentGreen);
        var completeButton = CreateActionButton("Mark Complete", new Point(520, 56), GuiTheme.AccentCyan);
        var deleteButton = CreateActionButton("Delete", new Point(520, 96), GuiTheme.AccentRed);
        var refreshButton = CreateActionButton("Refresh", new Point(620, 16), GuiTheme.BotBubble);

        addButton.Click += (_, _) => AddTask();
        completeButton.Click += (_, _) => CompleteSelected();
        deleteButton.Click += (_, _) => DeleteSelected();
        refreshButton.Click += (_, _) => RefreshTasks();

        formPanel.Controls.Add(addButton);
        formPanel.Controls.Add(completeButton);
        formPanel.Controls.Add(deleteButton);
        formPanel.Controls.Add(refreshButton);

        Controls.Add(_taskList);
        Controls.Add(formPanel);
        Controls.Add(header);

        Shown += (_, _) => RefreshTasks();
    }

    private void AddTask()
    {
        string title = _titleInput.Text.Trim();
        string description = _descriptionInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Please enter a task title.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(description))
            description = $"Complete the cybersecurity task: {title}";

        DateTime? reminder = _reminderDays.Value > 0
            ? DateTime.Today.AddDays((double)_reminderDays.Value)
            : null;

        var task = _taskService.Add(title, description, reminder);
        _activityLog.Log(reminder.HasValue
            ? $"Task added: '{task.Title}' (Reminder set for {reminder:yyyy-MM-dd})."
            : $"Task added: '{task.Title}'.");

        _titleInput.Clear();
        _descriptionInput.Clear();
        _reminderDays.Value = 0;
        RefreshTasks();
    }

    private void CompleteSelected()
    {
        if (_taskList.SelectedItems.Count == 0)
            return;

        int id = int.Parse(_taskList.SelectedItems[0].Text);
        if (_taskService.MarkCompleted(id))
        {
            _activityLog.Log($"Task #{id} marked as completed.");
            RefreshTasks();
        }
    }

    private void DeleteSelected()
    {
        if (_taskList.SelectedItems.Count == 0)
            return;

        int id = int.Parse(_taskList.SelectedItems[0].Text);
        if (_taskService.Delete(id))
        {
            _activityLog.Log($"Task #{id} deleted.");
            RefreshTasks();
        }
    }

    private void RefreshTasks()
    {
        _taskList.Items.Clear();
        foreach (CyberTask task in _taskService.GetAll())
        {
            var item = new ListViewItem(task.Id.ToString());
            item.SubItems.Add(task.Title);
            item.SubItems.Add(task.Description);
            item.SubItems.Add(task.ReminderDate?.ToString("yyyy-MM-dd") ?? "-");
            item.SubItems.Add(task.IsCompleted ? "Done" : "Pending");
            _taskList.Items.Add(item);
        }
    }

    private static TextBox CreateInput(string _, Point location, Size size)
    {
        return new TextBox
        {
            Location = location,
            Size = size,
            BackColor = GuiTheme.InputBackground,
            ForeColor = GuiTheme.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private static Button CreateActionButton(string text, Point location, Color backColor)
    {
        return new Button
        {
            Text = text,
            Location = location,
            Size = new Size(120, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
    }
}
