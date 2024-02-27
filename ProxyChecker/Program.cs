using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProxyChecker
{
    internal class Program
    {
        private static List<ProxyStruct> proxyList;
        public static List<ProxyStruct> GoodProxyList { get; set; }

        static async Task Main(string[] args)
        {
            proxyList = new List<ProxyStruct>();

            if (File.Exists(args[0]))
            {
                string[] lines = File.ReadAllLines(args[0]);
                foreach (string line in lines)
                {
                    string[] proxy = line.Split(':');
                    proxyList.Add(new ProxyStruct { IP = proxy[0], Port = int.Parse(proxy[1]) });
                }
            }

            GoodProxyList = new List<ProxyStruct>();

            var tasks = new List<Task>();

            int maxConcurrentTasks = 100; // Adjust this number as needed
            var semaphore = new SemaphoreSlim(maxConcurrentTasks);

            foreach (ProxyStruct proxy in proxyList)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        var result = await Utility.CheckProxyAsync(proxy.IP, proxy.Port);
                        if (result)
                        {
                            lock (GoodProxyList)
                            {
                                GoodProxyList.Add(proxy);
                            }
                            Utility.Write(string.Format("Proxy [{0}:{1}] Online", proxy.IP, proxy.Port), 6, 214, 160);
                        }
                        else
                        {
                            Utility.Write(string.Format("Proxy [{0}:{1}] Offline", proxy.IP, proxy.Port), 193, 18, 31);
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
            Utility.Write(string.Format("Finished checking {0}/{1}", proxyList.Count, GoodProxyList.Count), 17, 138, 178);

            File.WriteAllText("goodproxies.txt", string.Join("\n", GoodProxyList.ConvertAll(x => x.IP + ":" + x.Port)));
        }
    }

    public class ProxyStruct
    {
        public string IP { get; set; }
        public int Port { get; set; }
    }
}
