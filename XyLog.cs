using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace xySoft.log
{
    public static class XyLog
    {
        private static string _logDir = "log";
        private static string _logFileName = "log";

        static XyLog()
        {
        }

        public static void init(string logDir, string logFileName)
        {
            _logDir = logDir;
            _logFileName = logFileName;
        }

        private static Task? runningTask;
        private static bool running = false;
        public static void log(string logInfo)
        {
            lock (LogStringsDic)
            {
                LogStringsDic.Add(DateTime.Now, logInfo);
            }

            _ = Task.Run(async () => {
                if(!running)
                {
                    running = true;

                    if (runningTask != null)
                    {
                        await runningTask;
                        runningTask = null;
                    }
                    runningTask = Task.Run(() => { 
                        runLog();
                        running = false;
                    });
                }
            });
        }
        public static void log(Exception e)
        {
            log(e.Message + " - " + e.StackTrace);
        }

        private static Dictionary<DateTime, string> LogStringsDic = 
            new Dictionary<DateTime, string>();
        private static void runLog()
        {
            while (LogStringsDic.Count > 0)
            {
                DateTime logKey = LogStringsDic.Keys.First();
                string logString = LogStringsDic[logKey];
                lock (LogStringsDic)
                {
                    LogStringsDic.Remove(logKey);
                }
                StreamWriter logWriter = getLogWriter(logKey);

                logWriter.WriteLine(logKey + " -- " + logString);
                logWriter.Flush();
            }
        }
        private static StreamWriter? logWriter = null;
        private static string currentDateString = "";
        private static StreamWriter getLogWriter(DateTime lotDt)
        {
            string cateString = lotDt.ToString("yyyyMMdd");
            if(currentDateString != cateString)
            {
                currentDateString = cateString;
                string fileName =Path.Combine(
                            _logDir,
                            _logFileName.Split('.')[0] +
                            currentDateString + "." +
                            (
                                (_logFileName.Split('.').Length > 1) ?
                                _logFileName.Split('.')[1] : "log"
                            )
                            );
                if (logWriter != null)
                {
                    logWriter.Flush();
                    logWriter.Close();
                }
                if(!Directory.Exists(_logDir))
                {
                    Directory.CreateDirectory(_logDir);
                }
                if (!File.Exists(fileName))
                {
                    logWriter = new StreamWriter(fileName);
                }
                else
                {
                    logWriter = File.AppendText(fileName);
                    logWriter.WriteLine("");
                    logWriter.Flush();
                }
            }

            return logWriter!;
        }
    }

}
