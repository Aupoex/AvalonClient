using System;
using System.IO;

namespace CefFlashBrowser.Infrastructure
{
    public static class ClientLogger
    {
        private static readonly object SyncRoot = new object();

        public static void Info(string message)
        {
            Write("INFO", message, null);
        }

        public static void Error(string message, Exception exception = null)
        {
            Write("ERROR", message, exception);
        }

        private static void Write(string level, string message, Exception exception)
        {
            try
            {
                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {level,-5} | {message}";
                if (exception != null)
                {
                    line += Environment.NewLine + exception;
                }

                lock (SyncRoot)
                {
                    File.AppendAllText(ClientPaths.AppLogPath, line + Environment.NewLine);
                }
            }
            catch
            {
            }
        }
    }
}
