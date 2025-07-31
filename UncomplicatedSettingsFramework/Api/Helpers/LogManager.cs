using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using UncomplicatedSettingsFramework.Api.Interfaces;
using Logger = LabApi.Features.Console.Logger;

namespace UncomplicatedSettingsFramework.Api.Features.Helper
{
    internal class LogManager
    {
        // We should store the data here
        public static readonly List<LogEntry> History = new();

        public static bool MessageSent { get; internal set; } = false;

        public static void Debug(string message)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Debug.ToString(), message));
            if (Plugin.Instance.Config.Debug)
                Logger.Raw($"[DEBUG] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.Green);
        }

        public static void Info(string message)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Info.ToString(), message));
            Logger.Info(message);
        }

        public static void Warn(string message, string error = "CS0000")
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn.ToString(), message, error));
            Logger.Warn(message);
        }

        public static void Error(string message, string error = "CS0000")
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn.ToString(), message, error));
            Logger.Error(message + "\n\nIf you're seeing this, please run the `ucilogs` command in the server console to generate a log code. \n Then, post both the error and the log code in the #bug-reports forum in the UCI category of our Discord so we can help you faster. Thank you for reporting! \n Discord => 'https://discord.gg/5StRGu8EJV'");
        }
        
        public static void Raw(string message, ConsoleColor color, string logLevel, string category)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), logLevel, message));
            Logger.Raw($"[{category}] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", color);
        }
        
        public static void Updater(string message)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "Updater", message));
            Logger.Raw($"[Updater] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.Blue);
        }
        
        public static void Silent(string message)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "SILENT", message));
            if (Plugin.Instance.Config.ShowSilentLogs)
                Logger.Raw($"[Silent] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.White);
        }

        public static void System(string message) => History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "SYSTEM", message));
        
        public static HttpStatusCode SendReport(out HttpContent content)
        {
            content = null;

            if (MessageSent)
            {
                return HttpStatusCode.Forbidden;
            }

            if (History.Count < 1)
            {
                return HttpStatusCode.Forbidden;
            }

            string formattedContent = FormatLogsForReport();
            
            HttpStatusCode Response = Plugin.HttpManager.ShareLogs(formattedContent, out content);

            if (Response is HttpStatusCode.OK)
            {
                MessageSent = true;
            }

            return Response;
        }
        
        private static string FormatLogsForReport()
        {
            StringBuilder sb = new();

            sb.AppendLine("╔═══════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                    UncomplicatedSettingsFramework                 ║");
            sb.AppendLine("║                           Log Report                              ║");
            sb.AppendLine("╚═══════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            sb.AppendLine($"Generated: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
            sb.AppendLine($"Total Entries: {History.Count}");

            var logSummary = History.GroupBy(h => h.Level).ToDictionary(g => g.Key, g => g.Count());

            sb.AppendLine("Log Level Summary:");
            foreach (var kvp in logSummary.OrderByDescending(x => x.Value))
                sb.AppendLine($"  • {kvp.Key}: {kvp.Value} entries");

            sb.AppendLine();

            if (History.Count > 0)
            {
                DateTimeOffset firstLog = History.First().DateTimeOffset;
                DateTimeOffset lastLog = History.Last().DateTimeOffset;
                sb.AppendLine($"Time Range: {firstLog:yyyy-MM-dd HH:mm:ss} to {lastLog:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Duration: {lastLog - firstLog}");
                sb.AppendLine();
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine("                              LOG ENTRIES");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine();

            var groupedLogs = History.GroupBy(h => h.Level).OrderBy(g => GetLogLevelPriority(g.Key));

            foreach (var group in groupedLogs)
            {
                sb.AppendLine($"┌─ {group.Key.ToUpper()} LOGS ({group.Count()} entries) ─");
                sb.AppendLine();

                foreach (LogEntry entry in group)
                {
                    sb.AppendLine(FormatLogEntry(entry));
                }

                sb.AppendLine();
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine("                           CUSTOM SETTINGS");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine();

            if (CustomSetting.List.Any())
            {
                foreach (ICustomSetting setting in CustomSetting.List)
                {
                    sb.AppendLine("┌─ SETTING ─");
                    sb.AppendLine(LabApi.Loader.Features.Yaml.YamlConfigParser.Serializer.Serialize(setting));
                    sb.AppendLine("└───────────");
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("No custom settings configured.");
                sb.AppendLine();
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════════════");
            sb.AppendLine("                           END OF REPORT");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════");

            return sb.ToString();
        }
        
        private static string FormatLogEntry(LogEntry entry)
        {
            string timestamp = entry.DateTimeOffset.ToString("HH:mm:ss.fff");
            string errorPart = !string.IsNullOrEmpty(entry.Error) ? $"[{entry.Error}] " : "";

            return $" {timestamp} │ {errorPart}{entry.Content}";
        }
        
        private static int GetLogLevelPriority(string level)
        {
            return level.ToUpper() switch
            {
                "ERROR" => 0,
                "WARN" => 1,
                "INFO" => 2,
                "DEBUG" => 3,
                "UPDATER" => 4,
                "SYSTEM" => 5,
                "SILENT" => 6,
                _ => 7
            };
        }
    }
}