using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace XiaoyaLogger
{
    public class RuntimeLogger
    {

        struct LogData
        {
            public string fileName;
            public string content;
            public bool doWriteToConsole;
        }

        private string mLogFileName;
        private bool mDoWriteToConsole;

        private static BlockingCollection<LogData> queue = new BlockingCollection<LogData>();
        private static CancellationTokenSource cancellationTokenSource = null;
        private static object readLock = new object();

        public static object ReadLock
        {
            get
            {
                return readLock;
            }
        }

        static RuntimeLogger()
        {
            WriteAsync();
        }

        public RuntimeLogger(string logFileName, bool doWriteToConsole = false)
        {
            if (!Directory.Exists(Path.GetDirectoryName(logFileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
            }
            mLogFileName = logFileName;
            mDoWriteToConsole = doWriteToConsole;
        }

        public void Log(string className, string message)
        {
            var content = string.Format(
                "{0}\r\n{1}\r\n{2}\r\n\r\n",
                DateTime.Now.ToString(),
                className,
                message
            );

            queue.Add(new LogData
            {
                fileName = mLogFileName,
                content = content,
                doWriteToConsole = mDoWriteToConsole,
            });
        }

        public void LogException(string className, string message, Exception e)
        {
            var exceptionMessage = e.GetType().Name + "\r\n" + e.Message + "\r\n" + e.StackTrace + "\r\n";
            while (e.InnerException != null)
            {
                e = e.InnerException;
                exceptionMessage += e.GetType().Name + "\r\n" + e.Message + "\r\n" + e.StackTrace + "\r\n";
            }

            var content = string.Format(
                "{0}\r\n{1}\r\n{2}\r\nException:\r\n{3}\r\n",
                DateTime.Now.ToString(),
                className,
                message,
                exceptionMessage
            );

            queue.Add(new LogData
            {
                fileName = mLogFileName,
                content = content,
                doWriteToConsole = mDoWriteToConsole,
            });
        }

        public static void StartWrite()
        {
            WriteAsync();
        }

        public static void StopWrite()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }
        }

        protected static async void WriteAsync()
        {
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            await Task.Run(() =>
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    // Wait and take one LogData
                    var data = queue.Take();

                    lock (ReadLock)
                    {
                        StreamWriter writer = null;
                        string fileName = "";
                        int flushCount = 0;

                        do
                        {
                            if (data.fileName != fileName)
                            {
                                // Different file
                                if (writer != null)
                                {
                                    // Dispose StreamWriter
                                    writer.Flush();
                                    writer.Dispose();
                                }

                                flushCount = 0;
                                fileName = data.fileName;

                                try
                                {
                                    if (File.Exists(fileName)
                                        && new FileInfo(fileName).Length >= 10 * 1024 * 1024)
                                    {
                                        File.Move(fileName,
                                            Path.Combine(Path.GetDirectoryName(fileName),
                                            Path.GetFileNameWithoutExtension(fileName) +
                                            "." + DateTime.Now.ToBinary().ToString() + Path.GetExtension(fileName)));
                                    }
                                    writer = File.AppendText(fileName);
                                }
                                catch (IOException)
                                {
                                    // Failed, add it back
                                    queue.Add(data);
                                    break;
                                }
                            }

                            writer.Write(data.content);
                            flushCount++;

                            if (data.doWriteToConsole)
                            {
                                Console.WriteLine(data.content);
                            }

                            if (flushCount % 50 == 0)
                            {
                                writer.Flush();
                                flushCount = 0;
                            }

                        } while (queue.TryTake(out data, TimeSpan.FromMilliseconds(100)));

                        if (writer != null)
                        {
                            writer.Flush();
                            writer.Dispose();
                            writer = null;
                        }
                    }
                }
            }, token);
        }
    }
}
