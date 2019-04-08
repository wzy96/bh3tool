using System;
using System.IO;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static Timer Timer;

        static void Main(string[] args)
        {
            Console.WriteLine("bh3tool test start");
            try
            {
                //Get bh3tool.dll path.
                string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                path = Path.Combine(path, "Config.ini");
                Console.WriteLine("Read Config.ini");
                //default comfig
                string[] config = { "Port = 8877\n", "Bh3UrlOnly = false\n", "EnableIos = true\n", "EnableAndroid = true\n" };

                if (File.Exists(path))
                    config = File.ReadAllLines(path);
                else
                    File.WriteAllLines(path, config);

                var cfg = new bh3tool.config(config);
                foreach (var a in config)
                    Console.WriteLine(a);

                bh3tool.ToolManager manager = new bh3tool.ToolManager();
                //start
                manager.Start(cfg.port, cfg.Bh3Only, cfg.EnableAndroid, cfg.EnableIos);
                //start timer for checkupdate
                Timer = new Timer(new TimerCallback(CheckUpdate), manager, 10 * 1000, 60 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + ex.Message + "\n" + ex.StackTrace);
            }
            Console.WriteLine("bh3tool is running");
            while (true)
            {
                string s = Console.ReadLine();
                if (s == "exit")
                {
                    return;
                }

            }

        }

        static void CheckUpdate(object s)
        {
            var manager = (bh3tool.ToolManager)s;
            manager.UpdateFile();
        }



    }
}
