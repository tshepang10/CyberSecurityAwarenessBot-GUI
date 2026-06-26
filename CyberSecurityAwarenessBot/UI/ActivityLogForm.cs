using CyberSecurityAwarenessBot.Services;

namespace CyberSecurityAwarenessBot.UI;

internal sealed class ActivityLogForm : Form
{
    private readonly ActivityLogService _activityLog;
    private readonly RichTextBox _logView;
    private bool _showAll;

    public ActivityLogForm(ActivityLogService activityLog)
    {
        _activityLog = activityLog;

        Text = "Activity Log";
        Size = new Size(640, 480);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = GuiTheme.Background;
        Font = new Font("Segoe UI", 10F);

        _logView = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = GuiTheme.InputBackground,
            ForeColor = GuiTheme.TextPrimary,
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 10F)
        };

        var toggleButton = new Button
        {
            Text = "Show More",
            Dock = DockStyle.Bottom,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            BackColor = GuiTheme.AccentCyan,
            ForeColor = Color.FromArgb(15, 23, 42),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };
        toggleButton.Click += (_, _) =>
        {
            _showAll = !_showAll;
            toggleButton.Text = _showAll ? "Show Recent Only" : "Show More";
            RefreshLog();
        };

        Controls.Add(_logView);
        Controls.Add(toggleButton);

        Shown += (_, _) => RefreshLog();
    }

    private void RefreshLog()
    {
        _logView.Text = _activityLog.FormatSummary(_showAll);
    }
}
