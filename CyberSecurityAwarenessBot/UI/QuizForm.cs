using CyberSecurityAwarenessBot.Models;
using CyberSecurityAwarenessBot.Services;

namespace CyberSecurityAwarenessBot.UI;

internal sealed class QuizForm : Form
{
    private readonly QuizService _quizService;
    private readonly ActivityLogService _activityLog;
    private readonly Label _questionLabel;
    private readonly Label _progressLabel;
    private readonly Label _feedbackLabel;
    private readonly FlowLayoutPanel _optionsPanel;
    private readonly Button _nextButton;

    private int _currentIndex;
    private int _score;
    private bool _answered;

    public QuizForm(QuizService quizService, ActivityLogService activityLog)
    {
        _quizService = quizService;
        _activityLog = activityLog;

        Text = "Cybersecurity Quiz";
        Size = new Size(720, 520);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = GuiTheme.Background;
        Font = new Font("Segoe UI", 10F);

        _progressLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 36,
            ForeColor = GuiTheme.AccentCyan,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 8, 16, 0)
        };

        _questionLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 90,
            ForeColor = GuiTheme.TextPrimary,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Padding = new Padding(16, 8, 16, 8)
        };

        _optionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(16),
            BackColor = GuiTheme.Background
        };

        _feedbackLabel = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            ForeColor = GuiTheme.AccentGreen,
            Padding = new Padding(16, 8, 16, 8)
        };

        _nextButton = new Button
        {
            Text = "Next Question",
            Dock = DockStyle.Bottom,
            Height = 44,
            FlatStyle = FlatStyle.Flat,
            BackColor = GuiTheme.AccentCyan,
            ForeColor = Color.FromArgb(15, 23, 42),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Enabled = false
        };
        _nextButton.Click += (_, _) => ShowNextQuestion();

        Controls.Add(_optionsPanel);
        Controls.Add(_feedbackLabel);
        Controls.Add(_nextButton);
        Controls.Add(_questionLabel);
        Controls.Add(_progressLabel);

        LoadQuestion();
    }

    private void LoadQuestion()
    {
        _answered = false;
        _nextButton.Enabled = false;
        _feedbackLabel.Text = string.Empty;
        _optionsPanel.Controls.Clear();

        if (_currentIndex >= _quizService.Questions.Count)
        {
            ShowFinalScore();
            return;
        }

        QuizQuestion question = _quizService.Questions[_currentIndex];
        _progressLabel.Text = $"Question {_currentIndex + 1} of {_quizService.Questions.Count}";
        _questionLabel.Text = question.Question;

        for (int i = 0; i < question.Options.Length; i++)
        {
            int optionIndex = i;
            var button = new Button
            {
                Text = $"{(char)('A' + i)}. {question.Options[i]}",
                Width = 640,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = GuiTheme.BotBubble,
                ForeColor = GuiTheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 8),
                Cursor = Cursors.Hand
            };
            button.Click += (_, _) => SelectAnswer(optionIndex, button);
            _optionsPanel.Controls.Add(button);
        }
    }

    private void SelectAnswer(int selectedIndex, Button selectedButton)
    {
        if (_answered)
            return;

        _answered = true;
        QuizQuestion question = _quizService.Questions[_currentIndex];
        bool isCorrect = selectedIndex == question.CorrectIndex;

        if (isCorrect)
        {
            _score++;
            _feedbackLabel.ForeColor = GuiTheme.AccentGreen;
            _feedbackLabel.Text = $"Correct! {question.Explanation}";
            selectedButton.BackColor = Color.FromArgb(22, 101, 52);
        }
        else
        {
            _feedbackLabel.ForeColor = GuiTheme.AccentRed;
            _feedbackLabel.Text = $"Incorrect. {question.Explanation}";
            selectedButton.BackColor = Color.FromArgb(127, 29, 29);
        }

        foreach (Control control in _optionsPanel.Controls)
        {
            if (control is Button button)
                button.Enabled = false;
        }

        _nextButton.Enabled = true;
        _nextButton.Text = _currentIndex + 1 >= _quizService.Questions.Count ? "See Results" : "Next Question";
    }

    private void ShowNextQuestion()
    {
        _currentIndex++;
        LoadQuestion();
    }

    private void ShowFinalScore()
    {
        int total = _quizService.Questions.Count;
        string message = _quizService.GetClosingMessage(_score, total);
        _activityLog.Log($"Quiz completed — score { _score}/{total}.");

        _progressLabel.Text = "Quiz Complete";
        _questionLabel.Text = $"Your score: {_score}/{total}";
        _feedbackLabel.ForeColor = GuiTheme.AccentYellow;
        _feedbackLabel.Text = message;
        _optionsPanel.Controls.Clear();
        _nextButton.Text = "Close";
        _nextButton.Enabled = true;
        _nextButton.Click -= (_, _) => ShowNextQuestion();
        _nextButton.Click += (_, _) => Close();
    }
}
