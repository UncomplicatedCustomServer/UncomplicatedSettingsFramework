using Discord;
using System;
using System.Collections.Generic;
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
                Logger.Raw($"[DEBUG] [{Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.Green); // Make the text green again like Exiled 📣
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
            Logger.Raw($"[{category}] [{ Plugin.Instance.GetType().Assembly.GetName().Name}] {message}", color);
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
    }
}
