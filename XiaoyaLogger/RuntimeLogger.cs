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

        private static BlockingCollection<LogData> mQueue = new BlockingCollection<LogData>();
        private static CancellationTokenSource mCancellationTokenSource = null;
        private static object mReadLock = new object();

        public static object ReadLock
        {
            get
            {
                return mReadLock;
            }
        }

        static RuntimeLogger()
        {
            WriteAsync();
        }

        public RuntimeLogger(string logFileName = null, bool doWriteToConsole = false)
        {
            if (logFileName != null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(logFileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
                }
            }
            mLogFileName = logFileName;
            mDoWriteToConsole = doWriteToConsole;
        }

        public void Log(string className, string message, bool? doWriteToConsole = null)
        {
            var content = string.Format(
                "{0}\t{1}\r\n{2}\r\n\r\n",
                DateTime.Now.ToString(),
                className,
                message
            );

            mQueue.Add(new LogData
            {
                fileName = mLogFileName,
                content = content,
                doWriteToConsole = doWriteToConsole ?? mDoWriteToConsole,
            });
        }

        public void LogException(string className, string message, Exception e, bool? doWriteToConsole = null)
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

            mQueue.Add(new LogData
            {
                fileName = mLogFileName,
                content = content,
                doWriteToConsole = doWriteToConsole ?? mDoWriteToConsole,
            });
        }

        public static void StartWrite()
        {
            WriteAsync();
        }

        public static void StopWrite()
        {
            if (mCancellationTokenSource != null)
            {
                mCancellationTokenSource.Cancel();
                mCancellationTokenSource = null;
            }
        }

        protected static async void WriteAsync()
        {
            mCancellationTokenSource = new CancellationTokenSource();
            var token = mCancellationTokenSource.Token;

            await Task.Run(() =>
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    // Wait and take one LogData
                    var data = mQueue.Take();

                    lock (ReadLock)
                    {
                        StreamWriter writer = null;
                        string fileName = "";
                        int flushCount = 0;

                        do
                        {
                            if (data.fileName != fileName && data.fileName != null)
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
                                    mQueue.Add(data);
                                    break;
                                }
                            }

                            if (data.doWriteToConsole)
                            {
                                Console.WriteLine(data.content);
                            }

                            if (data.fileName != null)
                            {
                                writer.Write(data.content);
                                flushCount++;

                                if (flushCount % 50 == 0)
                                {
                                    writer.Flush();
                                    flushCount = 0;
                                }
                            }

                        } while (mQueue.TryTake(out data, TimeSpan.FromMilliseconds(100)));

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
