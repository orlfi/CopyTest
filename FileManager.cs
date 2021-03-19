using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


namespace TestCopy
{
    public delegate void OnProgressHandler(ProgressInfo progressInfo, bool done);

    public class FileManager
    {
        //public event OnProgressHandler ProgressEvent;
        public bool Progress { get; set;} = false;
        public FileManager()
        {
            //ProgressEvent += OnProgress;
        }
        public void OnProgress(ProgressInfo progress, bool done, ConsoleColor color)
        {
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            Console.SetCursorPosition(x, 10);
            Console.ForegroundColor = color;
            Console.Write($"Progress: { progress.Progress}% ");
            if (done)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\tdone!");
            }
            
            Console.SetCursorPosition(x, y);

            Console.ResetColor();
        }

        public async void Test(string source, string destSync, string destAsync)
        {
            FileInfo fi = new FileInfo(source);
            Console.WriteLine($"Current thread: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"Копирование файла {fi.Name} {fi.Length/1024}kB");
            //ProgressEvent += OnProgress;
            if (File.Exists(destSync))
                File.Delete(destSync);

            if (File.Exists(destAsync))
                File.Delete(destAsync);

            Console.CursorVisible = false;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            Console.WriteLine($"Start sync");
            sw.Start();
            var t1 = Copy(source, destSync);
            //await Copy(source, destSync);
            sw.Stop();
            Console.WriteLine($"Elapsed sync: {sw.ElapsedMilliseconds}");

            Console.WriteLine($"Start async");
            sw.Restart();
            //await CopyAsync(source, destAsync);
            var t2 =  CopyAsync(source, destAsync);
            sw.Stop();
            Console.WriteLine($"Elapsed async: {sw.ElapsedMilliseconds}");
            Task.WaitAll(t1,t2);
            Console.CursorVisible = true;

        }

        public async Task  CopyAsync(string source, string destination)
        {
            FileStream writeStream = null;
            FileStream readStream = null;
            try
            {
                FileInfo fileInfo = new FileInfo(source);
                long fileSize = fileInfo.Length;
                long total = 0;
                int bytesRead = -1;
                int buffLength = 1024 * 1024;
                byte[] buff = new byte[buffLength];
                int invoked = 0;

                writeStream = new FileStream(destination, FileMode.CreateNew, FileAccess.Write);
                readStream = new FileStream(source, FileMode.Open, FileAccess.Read);
                do
                {

                    bytesRead = await readStream.ReadAsync(buff, 0, buffLength);
                    await writeStream.WriteAsync(buff, 0, bytesRead);
                    total += bytesRead;
                    ProgressInfo pi = new ProgressInfo(total, fileSize, fileInfo.Name);
                    if (Progress && ((int)pi.Progress)/10 > invoked)
                    {
                        OnProgress(pi, false, ConsoleColor.Red);
                        //ProgressEvent?.Invoke(pi, false);
                        invoked++;
                    }
                } while (bytesRead > 0);
                writeStream.Flush();
            } 
            finally
            {
                writeStream?.Close();
                readStream?.Close();
            }
            Console.WriteLine($"Finished CopyAsync: {Thread.CurrentThread.ManagedThreadId}");
        }


        public async Task Copy(string source, string destination)
        {
            await Task.Run(()=> {Work(source, destination);});
        }

        public void Work(string source, string destination)
        {
            FileStream writeStream = null;
            FileStream readStream = null;
            try
            {
                FileInfo fileInfo = new FileInfo(source);
                long fileSize = fileInfo.Length;
                long total = 0;
                int bytesRead = -1;
                int buffLength = 1024 * 1024;
                byte[] buff = new byte[buffLength];
                int invoked = 0;

                writeStream = new FileStream(destination, FileMode.CreateNew, FileAccess.Write);
                readStream = new FileStream(source, FileMode.Open, FileAccess.Read);
                do
                {
                    bytesRead = readStream.Read(buff, 0, buffLength);
                    writeStream.Write(buff, 0, bytesRead);
                    total += bytesRead;
                    ProgressInfo pi = new ProgressInfo(total, fileSize, fileInfo.Name);
                    if (Progress && ((int)pi.Progress)/10 > invoked)
                    {
                        OnProgress(pi, false, ConsoleColor.Blue);
                        //ProgressEvent?.Invoke(pi, false);
                        invoked++;
                    }                } while (bytesRead > 0);
                writeStream.Flush();
            } 
            finally
            {
                writeStream?.Close();
                readStream?.Close();
            }
            Console.WriteLine($"Finished Copy: {Thread.CurrentThread.ManagedThreadId}");
        }

    }
}