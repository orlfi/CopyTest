using System;
using System.IO;
using System.Linq;
using System.Diagnostics;


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
        public void OnProgress(ProgressInfo progress, bool done)
        {
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            Console.SetCursorPosition(x, 10);
            Console.Write($"Progress: { progress.Progress}% ");
            if (done)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\tdone!");
            }
            
            Console.SetCursorPosition(x, y);

            Console.ResetColor();
        }

        public void Test(string source, string destSync, string destAsync)
        {
            FileInfo fi = new FileInfo(source);
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
            Copy(source, destSync);
            sw.Stop();
            Console.WriteLine($"Elapsed sync: {sw.ElapsedMilliseconds}");
            Console.WriteLine($"Start async");
            sw.Restart();
            CopyAsync(source, destAsync);
            sw.Stop();
            Console.WriteLine($"Elapsed async: {sw.ElapsedMilliseconds}");

            Console.CursorVisible = true;

        }

        public async void CopyAsync(string source, string destination)
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
                        OnProgress(pi, false);
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
        }


        public void Copy(string source, string destination)
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
                        OnProgress(pi, false);
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
        }
    }
}