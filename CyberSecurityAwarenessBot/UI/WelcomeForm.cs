using CyberSecurityAwarenessBot.Models;

namespace CyberSecurityAwarenessBot.UI;

internal sealed class WelcomeForm : Form
{
    private readonly TextBox _nameInput;
    private readonly Label _errorLabel;

    public UserProfile? User { get; private set; }

    public WelcomeForm()
    {
        Text = "Welcome — Cybersecurity Awareness Bot";
        Size = new Size(540, 400);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = GuiTheme.Background;
        Font = new Font("Segoe UI", 10F);

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = GuiTheme.PanelDark,
            Padding = new Padding(20, 16, 20, 12)
        };

        headerPanel.Controls.Add(new Label
        {
            Text = "CYBERSECURITY AWARENESS BOT",
            ForeColor = GuiTheme.AccentMagenta,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 14)
        });

        headerPanel.Controls.Add(new Label
        {
            Text = "Stay alert. Stay secure. Stay informed.",
            ForeColor = GuiTheme.AccentYellow,
            AutoSize = true,
            Location = new Point(20, 44)
        });

        var introLabel = new Label
        {
            Text = "Hello! Welcome to the Cybersecurity Awareness Bot.\nI am here to help you stay safe online.\n\nWhat is your name?",
            ForeColor = GuiTheme.TextPrimary,
            AutoSize = false,
            Size = new Size(460, 90),
            Location = new Point(24, 24)
        };

        _nameInput = new TextBox
        {
            Location = new Point(24, 130),
            Size = new Size(460, 30),
            BackColor = GuiTheme.InputBackground,
            ForeColor = GuiTheme.TextPrimary,
            BorderStyle = BorderStyle.FixedSingle
        };
        _nameInput.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ContinueClicked();
            }
        };

        _errorLabel = new Label
        {
            ForeColor = GuiTheme.AccentRed,
            AutoSize = true,
            Location = new Point(24, 168),
            Visible = false
        };

        var continueButton = new Button
        {
            Text = "Continue",
            Location = new Point(344, 210),
            Size = new Size(140, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = GuiTheme.AccentCyan,
            ForeColor = Color.FromArgb(15, 23, 42),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        continueButton.Click += (_, _) => ContinueClicked();

        Controls.Add(introLabel);
        Controls.Add(_nameInput);
        Controls.Add(_errorLabel);
        Controls.Add(continueButton);
        Controls.Add(headerPanel);

        AcceptButton = continueButton;
        Shown += (_, _) => _nameInput.Focus();
    }

    private void ContinueClicked()
    {
        string name = _nameInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            _errorLabel.Text = "Name cannot be empty. Please enter your name.";
            _errorLabel.Visible = true;
            _nameInput.Focus();
            return;
        }

        User = new UserProfile { Name = name };
        DialogResult = DialogResult.OK;
        Close();
    }
}
