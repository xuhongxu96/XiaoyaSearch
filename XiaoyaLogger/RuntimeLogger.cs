using System;
using System.IO;
using System.Threading.Tasks;

namespace XiaoyaLogger
{
    public class RuntimeLogger
    {

        private string mLogFileName;
        private object mSyncLock = new object();

        public RuntimeLogger(string logFileName)
        {
            if (!Directory.Exists(Path.GetDirectoryName(logFileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
            }
            mLogFileName = logFileName;
        }

        public async void Log(string className, string message)
        {
            await Task.Run(() =>
            {
                lock (mSyncLock)
                {
                    using (var writer = File.AppendText(mLogFileName))
                    {
                        Console.WriteLine(DateTime.Now);
                        Console.WriteLine(className);
                        Console.WriteLine(message);

                        writer.WriteLine(DateTime.Now);
                        writer.WriteLine(className);
                        writer.WriteLine(message);
                        writer.Flush();
                    }
                }
            });
        }
    }
}
