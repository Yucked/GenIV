using System;
using System.Linq;
using System.Reflection;
using Colorful;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Color = System.Drawing.Color;
using Console = Colorful.Console;

namespace GenIVIV {
    public static class Extensions {
        public const string MESSAGE_FORMAT = "[{0}] [{1}] [{2}]\n{3}";

        public static LogLevel FromSeverityToLevel(this LogSeverity logSeverity) {
            return (LogLevel) Math.Abs((int) logSeverity - 5);
        }

        public static Color GetLogLevelColor(this LogLevel logLevel) {
            return logLevel switch {
                LogLevel.Trace       => Color.LightBlue,
                LogLevel.Debug       => Color.PaleVioletRed,
                LogLevel.Information => Color.FromArgb(84, 194, 231),
                LogLevel.Warning     => Color.Coral,
                LogLevel.Error       => Color.Crimson,
                LogLevel.Critical    => Color.Red,
                LogLevel.None        => Color.Coral,
                _                    => Color.White
            };
        }

        public static string GetShortLogLevel(this LogLevel logLevel) {
            return logLevel switch {
                LogLevel.Trace       => "TRCE",
                LogLevel.Debug       => "DBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning     => "WARN",
                LogLevel.Error       => "EROR",
                LogLevel.Critical    => "CRIT",
                LogLevel.None        => "NONE",
                _                    => "UKON"
            };
        }

        public static IServiceCollection AddServices<TAttribute>(this IServiceCollection serviceCollection)
            where TAttribute : Attribute {
            var services = typeof(Program).Assembly
                .GetTypes()
                .Where(x => x.GetCustomAttribute<TAttribute>() != null);

            foreach (var service in services) {
                serviceCollection.AddSingleton(service);
            }

            return serviceCollection;
        }

        public static string GetNameOfT<T>() {
            return $"{typeof(T).Name[..^9]}s";
        }

        public static bool TryTakeInput(string message, out string input) {
            var formatters = new[] {
                new Formatter('>', Color.Coral),
                new Formatter(message, Color.White),
            };

            Console.WriteLineFormatted("{0} {1}", Color.White, formatters);
            input = Console.ReadLine();
            return !string.IsNullOrWhiteSpace(input);
        }

        public static void PrintInformationalHeader() {
            const string LOGO =
                @"
  e88'Y88                  888 Y8b Y88888P 888 Y8b Y88888P 
 d888  'Y   ,e e,  888 8e  888  Y8b Y888P  888  Y8b Y888P  
C8888 eeee d88 88b 888 88b 888   Y8b Y8P   888   Y8b Y8P   
 Y888 888P 888   , 888 888 888    Y8b Y    888    Y8b Y    
  ""88 88""   ""YeeP"" 888 888 888     Y8P     888     Y8P     
                
";

            Console.WriteWithGradient(LOGO, Color.Crimson, Color.HotPink);
            Console.ReplaceAllColorsWithDefaults();
        }
    }
}