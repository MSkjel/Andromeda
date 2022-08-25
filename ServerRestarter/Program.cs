using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerRestarter
{
    internal class Program
    {
        static int MaxSeconds = 1000;
        static int PID = 0;
        static Stopwatch watch = new Stopwatch();

        static void Main(string[] args)
        {
            WatchDog();
            StartListening();
        }

        // Incoming data from the client.  
        public static string? data = null;

        public static void StartListening()
        {
            byte[] bytes = new byte[1024];

            IPAddress addr = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(addr, 8585);
            Socket listener = new Socket(addr.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(endPoint);
                listener.Listen(1);

                while (true)
                {
                    Socket handler = listener.Accept();
                    data = null;

                    int bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);


                    Console.WriteLine(DateTime.Now.ToString() + "::: " + "Text received : {0}", data);
                    CheckInput(data);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void CheckInput(string msg)
        {
            if(msg.StartsWith("ImHere"))
            {
                PID = int.Parse(msg.Split(" ")[1]);

                if (watch.IsRunning)
                    watch.Restart();
                else
                    watch.Start();
            }
        }

        public static void WatchDog()
        {
            Task.Factory.StartNew(() =>
            {
                Console.WriteLine(DateTime.Now.ToString() + "::: " + "Watchdog started");
                while(true)
                {
                    if(watch.IsRunning)
                    {
                        if((watch.ElapsedMilliseconds / 1000) > MaxSeconds && PID != 0)
                        {
                            Process.GetProcessById(PID).Kill();
                            Console.WriteLine(DateTime.Now.ToString() + "::: " + "Killed Server");
                            watch.Reset();
                            watch.Stop();
                        }
                    }

                    Thread.Sleep(10000);
                }
            });
        }
    }
}