using CyberSecurityAwarenessBot.Models;
using CyberSecurityAwarenessBot.Services;

namespace CyberSecurityAwarenessBot.UI;

internal sealed class MainForm : Form
{
    private readonly UserProfile _user;
    private readonly MemoryService _memory = new();
    private readonly SentimentService _sentiment = new();
    private readonly ActivityLogService _activityLog = new();
    private readonly TaskService _taskService = new();
    private readonly QuizService _quizService = new();
    private readonly NlpService _nlpService = new();
    private readonly ResponseService _responseService;
    private readonly ChatOrchestrator _orchestrator;

    private readonly RichTextBox _chatHistory;
    private readonly TextBox _messageInput;
    private bool _isClosing;

    private static readonly string[] Suggestions =
    [
        "How are you?",
        "Tell me about password safety",
        "Give me a phishing tip",
        "I'm interested in privacy",
        "Add task - Enable two-factor authentication",
        "Start quiz",
        "What have you done for me?"
    ];

    public MainForm(UserProfile user)
    {
        _user = user;
        _responseService = new ResponseService(_memory, _sentiment);
        _orchestrator = new ChatOrchestrator(_user, _responseService, _nlpService, _taskService, _activityLog);
        _orchestrator.IntentDetected += HandleIntent;

        Text = "Cybersecurity Awareness Bot";
        Size = new Size(980, 720);
        MinimumSize = new Size(760, 560);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = GuiTheme.Background;
        Font = new Font("Segoe UI", 10F);

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(16),
            BackColor = GuiTheme.Background
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));

        _chatHistory = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = GuiTheme.InputBackground,
            ForeColor = GuiTheme.TextPrimary,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            Font = new Font("Segoe UI", 10F)
        };

        _messageInput = new TextBox
        {
            Dock = DockStyle.Fill,
            BackColor = GuiTheme.InputBackground,
            ForeColor = GuiTheme.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle
        };

        var sendButton = new Button
        {
            Text = "Send",
            Dock = DockStyle.Right,
            Width = 96,
            FlatStyle = FlatStyle.Flat,
            BackColor = GuiTheme.AccentCyan,
            ForeColor = Color.FromArgb(15, 23, 42),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        var inputContainer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        inputContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        inputContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
        inputContainer.Controls.Add(_messageInput, 0, 0);
        inputContainer.Controls.Add(sendButton, 1, 0);

        mainLayout.Controls.Add(BuildHeaderPanel(), 0, 0);
        mainLayout.Controls.Add(_chatHistory, 0, 1);
        mainLayout.Controls.Add(BuildToolbarPanel(), 0, 2);
        mainLayout.Controls.Add(inputContainer, 0, 3);
        Controls.Add(mainLayout);

        sendButton.Click += (_, _) => SendMessage();
        _messageInput.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SendMessage();
            }
        };

        Load += OnFormLoad;
        FormClosing += OnFormClosing;
    }

    private void OnFormLoad(object? sender, EventArgs e)
    {
        AudioPlayer.PlayGreeting("Assets/greetings.wav");
        AppendBotMessage($"Nice to meet you, {_user.Name}! I am your Cybersecurity Awareness Assistant.");
        AppendBotMessage("You can chat about passwords, phishing, privacy, scams, manage tasks, take a quiz, or view your activity log.");
        foreach (string suggestion in Suggestions)
            AppendBotMessage($"- {suggestion}");

        string storage = _taskService.UsesDatabase ? "MySQL database" : "local fallback storage";
        AppendBotMessage($"Task storage: {storage}. Configure appsettings.json for MySQL.");
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isClosing)
            return;

        e.Cancel = true;
        _isClosing = true;
        AppendBotMessage(_responseService.GetResponse("exit", _user));
        var timer = new System.Windows.Forms.Timer { Interval = 900 };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            timer.Dispose();
            FormClosing -= OnFormClosing;
            Close();
        };
        timer.Start();
    }

    private void SendMessage()
    {
        string input = _messageInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            AppendError("Invalid input. Please type a valid question.");
            return;
        }

        AppendUserMessage(input);
        _messageInput.Clear();

        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            _isClosing = true;
            AppendBotMessage(_responseService.GetResponse("exit", _user));
            var timer = new System.Windows.Forms.Timer { Interval = 900 };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                timer.Dispose();
                FormClosing -= OnFormClosing;
                Close();
            };
            timer.Start();
            return;
        }

        bool shouldClose = _orchestrator.HandleInput(input, out string response);
        AppendBotMessage(response);

        if (shouldClose)
        {
            _isClosing = true;
            var timer = new System.Windows.Forms.Timer { Interval = 900 };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                timer.Dispose();
                FormClosing -= OnFormClosing;
                Close();
            };
            timer.Start();
        }
    }

    private void HandleIntent(UserIntent intent)
    {
        switch (intent)
        {
            case UserIntent.StartQuiz:
                using (var quizForm = new QuizForm(_quizService, _activityLog))
                    quizForm.ShowDialog(this);
                break;
        }
    }

    private Panel BuildHeaderPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = GuiTheme.PanelDark,
            Padding = new Padding(16, 12, 16, 8)
        };

        panel.Controls.Add(new Label
        {
            Text = "CYBERSECURITY AWARENESS BOT",
            ForeColor = GuiTheme.AccentMagenta,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(16, 10)
        });

        panel.Controls.Add(new Label
        {
            Text = $"Signed in as: {_user.Name}",
            ForeColor = GuiTheme.AccentGreen,
            AutoSize = true,
            Location = new Point(16, 42)
        });

        panel.Controls.Add(new Label
        {
            Text = "[ PROTECT • DETECT • PREVENT ]  |  ASCII: 🛡 CYBER SHIELD",
            ForeColor = GuiTheme.AccentCyan,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(16, 68)
        });

        return panel;
    }

    private FlowLayoutPanel BuildToolbarPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            BackColor = GuiTheme.Background
        };

        AddToolbarButton(panel, "Quiz", () =>
        {
            _activityLog.Log("Quiz started from toolbar.");
            using var form = new QuizForm(_quizService, _activityLog);
            form.ShowDialog(this);
        });

        AddToolbarButton(panel, "Tasks", () =>
        {
            using var form = new TaskManagerForm(_taskService, _activityLog);
            form.ShowDialog(this);
        });

        AddToolbarButton(panel, "Activity Log", () =>
        {
            using var form = new ActivityLogForm(_activityLog);
            form.ShowDialog(this);
        });

        foreach (string suggestion in Suggestions.Take(4))
        {
            string text = suggestion;
            AddToolbarButton(panel, text.Length > 22 ? text[..22] + "…" : text, () =>
            {
                _messageInput.Text = text;
                SendMessage();
            });
        }

        return panel;
    }

    private static void AddToolbarButton(FlowLayoutPanel panel, string text, Action onClick)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            BackColor = GuiTheme.BotBubble,
            ForeColor = GuiTheme.AccentCyan,
            Margin = new Padding(0, 0, 8, 0),
            Padding = new Padding(10, 6, 10, 6),
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9F)
        };
        button.FlatAppearance.BorderColor = GuiTheme.AccentCyan;
        button.Click += (_, _) => onClick();
        panel.Controls.Add(button);
    }

    private void AppendUserMessage(string message) => AppendLine($"You ({_user.Name})", message, GuiTheme.AccentGreen);
    private void AppendBotMessage(string message) => AppendLine("Bot", message, GuiTheme.AccentCyan);
    private void AppendError(string message) => AppendLine("Error", message, GuiTheme.AccentRed);

    private void AppendLine(string speaker, string message, Color speakerColor)
    {
        _chatHistory.SelectionStart = _chatHistory.TextLength;
        _chatHistory.SelectionLength = 0;
        _chatHistory.SelectionColor = speakerColor;
        _chatHistory.SelectionFont = new Font(_chatHistory.Font, FontStyle.Bold);
        _chatHistory.AppendText($"{speaker}: ");
        _chatHistory.SelectionColor = GuiTheme.TextPrimary;
        _chatHistory.SelectionFont = _chatHistory.Font;
        _chatHistory.AppendText($"{message}{Environment.NewLine}{Environment.NewLine}");
        _chatHistory.ScrollToCaret();
    }
}
