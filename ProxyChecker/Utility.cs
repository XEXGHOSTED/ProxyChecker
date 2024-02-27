namespace ProxyChecker
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public class Utility
    {
        public static async Task<bool> CheckProxyAsync(string IP, int Port)
        {
            try
            {
                IPAddress ipAddress;
                if (!IPAddress.TryParse(IP, out ipAddress))
                {
                    // Handle invalid IP address
                    return false;
                }

                IPEndPoint endPoint = new IPEndPoint(ipAddress, Port);
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(endPoint.Address, endPoint.Port);
                    if (await Task.WhenAny(connectTask, Task.Delay(5000)) == connectTask)
                    {
                        // Connection succeeded within the timeout
                        return true;
                    }
                    else
                    {
                        // Connection timed out
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static void Write(string Msg, int R, int G, int B)
        {
            Console.WriteLine("\u001b[38;2;84;84;84m" + DateTime.Now.ToString("T") + "\u001b[38;2;255;255;255m | " + Msg.Replace("[", $"\u001b[38;2;{R};{G};{B}m").Replace("]", "\u001b[38;2;255;255;255m"));
        }
    }
}
