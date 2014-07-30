using System;

namespace Common.Log
{
    public static class Log
    {
        public enum LogLevels
        {
            Debug,
            Info,
            Warning,
            Error,
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("TetriNET");

        public static void Initialize(string path, string file, string fileTarget = "logfile")
        {
            string logfile = System.IO.Path.Combine(path, file);
            NLog.Targets.FileTarget target = NLog.LogManager.Configuration.FindTargetByName(fileTarget) as NLog.Targets.FileTarget;
            if (target == null)
                throw new ApplicationException(String.Format("Couldn't find target {0} in NLog config", fileTarget));
            target.FileName = logfile;
        }

        public static void WriteLine(LogLevels level, string format, params object[] args)
        {
            switch (level)
            {
                case LogLevels.Debug:
                    Logger.Debug(format, args);
                    break;
                case LogLevels.Info:
                    Logger.Info(format, args);
                    break;
                case LogLevels.Warning:
                    Logger.Warn(format, args);
                    break;
                case LogLevels.Error:
                    Logger.Error(format, args);
                    break;
            }
        }
    }

}
