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
        }

        private string mLogFileName;

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

        public RuntimeLogger(string logFileName)
        {
            if (!Directory.Exists(Path.GetDirectoryName(logFileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
            }
            mLogFileName = logFileName;
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
