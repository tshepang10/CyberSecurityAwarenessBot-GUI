using System.Text.Json;
using CyberSecurityAwarenessBot.Models;
using MySqlConnector;

namespace CyberSecurityAwarenessBot.Services;

public sealed class TaskService
{
    private readonly string? _connectionString;
    private readonly string _fallbackPath;
    private readonly List<CyberTask> _fallbackTasks = [];
    private int _nextFallbackId = 1;

    public TaskService()
    {
        _connectionString = AppConfig.GetConnectionString();
        _fallbackPath = Path.Combine(AppContext.BaseDirectory, "tasks-fallback.json");
        LoadFallbackTasks();
        TryInitializeDatabase();
    }

    public bool UsesDatabase => _connectionString is not null && CanConnect();

    public IReadOnlyList<CyberTask> GetAll()
    {
        if (UsesDatabase)
            return GetAllFromDatabase();

        return _fallbackTasks.OrderByDescending(t => t.CreatedAt).ToList();
    }

    public CyberTask Add(string title, string description, DateTime? reminderDate = null)
    {
        var task = new CyberTask
        {
            Title = title,
            Description = description,
            ReminderDate = reminderDate,
            CreatedAt = DateTime.UtcNow
        };

        if (UsesDatabase)
        {
            task.Id = InsertIntoDatabase(task);
            return task;
        }

        task.Id = _nextFallbackId++;
        _fallbackTasks.Add(task);
        SaveFallbackTasks();
        return task;
    }

    public bool MarkCompleted(int id)
    {
        if (UsesDatabase)
            return UpdateCompletedInDatabase(id, true);

        var task = _fallbackTasks.FirstOrDefault(t => t.Id == id);
        if (task is null)
            return false;

        task.IsCompleted = true;
        SaveFallbackTasks();
        return true;
    }

    public bool Delete(int id)
    {
        if (UsesDatabase)
            return DeleteFromDatabase(id);

        var task = _fallbackTasks.FirstOrDefault(t => t.Id == id);
        if (task is null)
            return false;

        _fallbackTasks.Remove(task);
        SaveFallbackTasks();
        return true;
    }

    public string FormatTaskList()
    {
        var tasks = GetAll();
        if (tasks.Count == 0)
            return "You have no tasks yet. Try: Add task - Enable two-factor authentication";

        return string.Join(
            Environment.NewLine,
            tasks.Select(task =>
            {
                string status = task.IsCompleted ? "[Done]" : "[Pending]";
                string reminder = task.ReminderDate.HasValue
                    ? $" (Reminder: {task.ReminderDate:yyyy-MM-dd})"
                    : string.Empty;
                return $"{status} #{task.Id} {task.Title} — {task.Description}{reminder}";
            }));
    }

    private void TryInitializeDatabase()
    {
        if (_connectionString is null || !CanConnect())
            return;

        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS tasks (
                id INT AUTO_INCREMENT PRIMARY KEY,
                title VARCHAR(255) NOT NULL,
                description TEXT NOT NULL,
                reminder_date DATETIME NULL,
                is_completed TINYINT(1) NOT NULL DEFAULT 0,
                created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            """;
        command.ExecuteNonQuery();
    }

    private bool CanConnect()
    {
        if (_connectionString is null)
            return false;

        try
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private List<CyberTask> GetAllFromDatabase()
    {
        var tasks = new List<CyberTask>();
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, title, description, reminder_date, is_completed, created_at
            FROM tasks
            ORDER BY created_at DESC;
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tasks.Add(new CyberTask
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Description = reader.GetString("description"),
                ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date"))
                    ? null
                    : reader.GetDateTime("reminder_date"),
                IsCompleted = reader.GetBoolean("is_completed"),
                CreatedAt = reader.GetDateTime("created_at")
            });
        }

        return tasks;
    }

    private int InsertIntoDatabase(CyberTask task)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO tasks (title, description, reminder_date, is_completed, created_at)
            VALUES (@title, @description, @reminderDate, 0, @createdAt);
            SELECT LAST_INSERT_ID();
            """;
        command.Parameters.AddWithValue("@title", task.Title);
        command.Parameters.AddWithValue("@description", task.Description);
        command.Parameters.AddWithValue("@reminderDate", task.ReminderDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@createdAt", task.CreatedAt);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    private bool UpdateCompletedInDatabase(int id, bool completed)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE tasks SET is_completed = @completed WHERE id = @id;";
        command.Parameters.AddWithValue("@completed", completed);
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    private bool DeleteFromDatabase(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM tasks WHERE id = @id;";
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    private void LoadFallbackTasks()
    {
        if (!File.Exists(_fallbackPath))
            return;

        try
        {
            var tasks = JsonSerializer.Deserialize<List<CyberTask>>(File.ReadAllText(_fallbackPath));
            if (tasks is null)
                return;

            _fallbackTasks.Clear();
            _fallbackTasks.AddRange(tasks);
            _nextFallbackId = _fallbackTasks.Count == 0 ? 1 : _fallbackTasks.Max(t => t.Id) + 1;
        }
        catch
        {
            // Ignore corrupt fallback file and start fresh.
        }
    }

    private void SaveFallbackTasks()
    {
        File.WriteAllText(_fallbackPath, JsonSerializer.Serialize(_fallbackTasks, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}

public static class AppConfig
{
    public static string? GetConnectionString()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
            return null;

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) &&
                connectionStrings.TryGetProperty("Default", out var value))
            {
                string connectionString = value.GetString() ?? string.Empty;
                return string.IsNullOrWhiteSpace(connectionString) ? null : connectionString;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
