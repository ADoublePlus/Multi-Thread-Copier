using System;
using System.Threading;
using System.IO;
using System.Linq;

namespace copy_multithread
{
    class Program
    {
        // A lock mechanism to avoid race condition i.e. collisions of threads for shared resource
        static Semaphore readSemaphore = new Semaphore(1, 1),
                                           writeSemaphore = new Semaphore(1, 1);

        static int srcHead = 0; // Source file head location
        string sourceLocation, destLocation; // Location of source and destination file

        public Program(string srcLoc = "", string destLoc = "") // Constructor used for initializing threads
        {
            this.sourceLocation = srcLoc;
            this.destLocation = destLoc;
        }

        public static void Main(string[] args)
        {
            Program p = new Program();
            p.startFunc(); // Starts the function

            // At this point the programs job is finished
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        public void startFunc()
        {
            Console.Write("Enter the source file name: ");
            sourceLocation = "test.txt";
            Console.ReadLine();

            Console.Write("Enter the destination file name: ");
            destLocation = "output.txt";
            Console.ReadLine();

            Console.Write("Enter number of threads: ");

            bool correctInput = false;

            do
            {
                try
                {
                    int N = 10; // Number of threads
                    Convert.ToInt32(Console.ReadLine());
                    Thread[] t = new Thread[N];
                    Program[] pr = new Program[N];

                    for (int i = 0; i < N; i++)
                    {
                        pr[i] = new Program(sourceLocation, destLocation);
                        t[i] = new Thread(new ThreadStart(pr[i].readWriteFunc));
                        t[i].Name = (i + 1).ToString();
                    }

                    for (int i = 0; i < N; i++)
                    {
                        t[i].Start(); // Multithreading starts from here
                    }

                    correctInput = true;
                }

                catch (Exception ex)
                {
                    Console.WriteLine("Wrong input, enter integers!");
                }
            } while (!correctInput);
        }

        public void readWriteFunc()
        {
            while (true)
            {
                try
                {
                    string tmp = null;

                    Console.WriteLine("Thread {0} trying to open source file...", Thread.CurrentThread.Name);
                    readSemaphore.WaitOne(); // Get the lock

                    // Read srcHead'th line
                    tmp = File.ReadLines(sourceLocation).Skip(srcHead).FirstOrDefault();

                    // Increase srcHead for next read from source
                    srcHead += 1;

                    readSemaphore.Release(); // Release the lock

                    if (tmp == null) // Nothing to read i.e. end of file; no need to write
                    {
                        Console.WriteLine("Thread {0} reached end of file.", Thread.CurrentThread.Name);
                        break;
                    }

                    writeSemaphore.WaitOne();
                    Console.WriteLine("Thread {0} trying to open destination file...", Thread.CurrentThread.Name);

                    using (StreamWriter sw = new StreamWriter(destLocation, true))
                    {
                        sw.WriteLine(tmp);
                    }

                    writeSemaphore.Release();
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}