using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class DiscordLoggerProvider : ILoggerProvider
    {
        private readonly string _webhookUrl;
        
        public DiscordLoggerProvider(string webhookUrl)
        {
            _webhookUrl = webhookUrl;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DiscordLogger(categoryName, _webhookUrl);
        }

        public void Dispose() { }
    }

    public class DiscordLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _webhookUrl;
        private static readonly HttpClient _httpClient = new HttpClient();

        public DiscordLogger(string categoryName, string webhookUrl)
        {
            _categoryName = categoryName;
            _webhookUrl = webhookUrl;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            // Log exceptions
            if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical) return true;
            
            // Log job logs
            if (_categoryName.Contains("RentReminderService") && logLevel >= LogLevel.Information) return true;

            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            if (string.IsNullOrEmpty(_webhookUrl)) return;

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message) && exception == null) return;

            if (exception != null)
            {
                message += $"\n\nException:\n{exception.ToString()}";
            }

            // Limit message length to 1900 characters to fit Discord's 2000 char limit
            if (message.Length > 1900)
            {
                message = message.Substring(0, 1900) + "...";
            }

            var color = logLevel switch
            {
                LogLevel.Error or LogLevel.Critical => 16711680, // Red
                LogLevel.Warning => 16776960, // Yellow
                _ => 3447003 // Blue
            };

            var embed = new
            {
                title = $"[{logLevel}] {_categoryName}",
                description = $"```{message}```",
                color = color,
                timestamp = DateTime.UtcNow.ToString("O")
            };

            var payload = new
            {
                embeds = new[] { embed }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            
            // Fire and forget
            _ = _httpClient.PostAsync(_webhookUrl, content);
        }
    }
}
