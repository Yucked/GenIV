using System;
using System.Drawing;
using Colorful;
using Microsoft.Extensions.Logging;

namespace GenIVIV.Logging {
    public readonly struct ColorfulLogger : ILogger {
        private readonly string _categoryName;
        private readonly LogLevel _categoryLevel;
        private readonly LoggerProvider _provider;

        public ColorfulLogger(string categoryName, LogLevel categoryLevel, LoggerProvider provider) {
            _categoryName = categoryName;
            _categoryLevel = categoryLevel;
            _provider = provider;
        }

        public IDisposable BeginScope<TState>(TState state) {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel) {
            var isEnabled = _categoryLevel <= logLevel;
            return isEnabled;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                                Func<TState, Exception, string> formatter) {
            if (!IsEnabled(logLevel)) {
                return;
            }

            var message = formatter(state, exception);
            if (string.IsNullOrWhiteSpace(message)) {
                return;
            }

            var formatters = new[] {
                new Formatter($"{DateTimeOffset.Now:MMM d - hh:mm:ss tt}", Color.FromArgb(92, 169, 114)),
                new Formatter(logLevel.GetShortLogLevel(), logLevel.GetLogLevelColor()),
                new Formatter(_categoryName, Color.FromArgb(255, 95, 133)),
                new Formatter(message, Color.FromArgb(171, 157, 242))
            };

            _provider.Enqueue(formatters);
        }
    }
}