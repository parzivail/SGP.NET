using System;
using System.Diagnostics;

namespace PFX
{
    public class Lumberjack
    {
        public static OutputLevel TraceLevel = OutputLevel.Info;

        public static void Debug(string message)
        {
            Log(message, ConsoleColor.Gray, OutputLevel.Debug, "DEBUG");
        }

        public static void Log(string message)
        {
            Log(message, ConsoleColor.Gray, OutputLevel.Log, "LOG");
        }

        public static void Info(string message)
        {
            Log(message, ConsoleColor.Green, OutputLevel.Info, "INFO");
        }

        public static void Warn(string message)
        {
            Log(message, ConsoleColor.Yellow, OutputLevel.Warn, "WARN");
        }

        public static void Error(string message)
        {
            Log(message, ConsoleColor.Red, OutputLevel.Error, "ERROR");
        }

        public static void Log(string message, ConsoleColor color, OutputLevel level, string header = "")
        {
            if (TraceLevel > level)
                return;

            if (Console.ForegroundColor == color)
                Console.WriteLine(Resources.Log_Format, DateTime.Now, header.Length > 0 ? " " + header : header, message);
            else
            {
                Console.ForegroundColor = color;
                Console.WriteLine(Resources.Log_Format, DateTime.Now, header.Length > 0 ? " " + header : header, message);
            }
        }
    }

    public enum OutputLevel
    {
        Debug,
        Log,
        Info,
        Warn,
        Error,
        Off
    }
}