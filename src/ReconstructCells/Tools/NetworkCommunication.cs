using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ReconstructCells.Tools
{
    public class NetworkCommunication
    {
        public static  bool KeepRunning = true;
        static UdpClient udpClient;
        public static int StartNetworkListener(Queue<string> messages, int port)
        {
            if (udpClient == null)
            {
                //check if this port is already being used
                while (udpClient == null)
                {
                    try
                    {
                        udpClient = new UdpClient(port);
                    }
                    catch
                    {
                        port++;
                    }
                }
                Thread MonitorTreads = new Thread(delegate(object Vars)
                {

                    while (KeepRunning == true )
                    {
                        try
                        {
                            //IPEndPoint object will allow us to read datagrams sent from any source.
                            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);

                            // Blocks until a message returns on this socket from a remote host.
                            Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                            string returnData = Encoding.ASCII.GetString(receiveBytes);

                            messages.Enqueue(returnData);
                            try
                            {
                                Console.WriteLine(returnData.ToString());
                            }
                            catch { }
                        }
                        catch { }
                    }
                    udpClient.Close();
                });

                MonitorTreads.Start();
            }
            return port;
        }

        static string DictName;
        static IPEndPoint RemoteIpEndPoint;
        static System.Net.Sockets.UdpClient udpClientB;

        public static void StartNetworkWriter(string dictName, int Port)
        {
            try
            {
                DictName = dictName;
                udpClientB = new System.Net.Sockets.UdpClient();
                RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);
                // Sends a message to the host to which you have connected.
               // Byte[] sendBytes = Encoding.ASCII.GetBytes(DictName + "Is anybody there?");

                // Sends a message to a different host using optional hostname and port parameters.
                //udpClientB.Send(sendBytes, sendBytes.Length, RemoteIpEndPoint);
            }
            catch { }
        }

        private static object CriticalSectionLock = new object();
        public static void SendNetworkPacket(string Key, object Value)
        {
            lock (CriticalSectionLock)
            {
                bool NewValue = true;
                if (udpClientB != null && NewValue == true)
                {
                    System.Diagnostics.Debug.Print(DictName + "|" + Key + " = " + Value.ToString());
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(DictName + "|" + Key + " = " + Value.ToString());
                    // Sends a message to a different host using optional hostname and port parameters.
                    udpClientB.Send(sendBytes, sendBytes.Length, RemoteIpEndPoint);
                }
                Console.WriteLine(Key + " = " + Value);
            }
        }

        public static void SendMessage(string message)
        {
            lock (CriticalSectionLock)
            {
                bool NewValue = true;
                if (udpClientB != null && NewValue == true)
                {
                    System.Diagnostics.Debug.Print(message);
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(message);
                    // Sends a message to a different host using optional hostname and port parameters.
                    udpClientB.Send(sendBytes, sendBytes.Length, RemoteIpEndPoint);
                }
                Console.WriteLine(message);
            }
        }



    }

    /// <summary>
    /// a dictionary that allows an key to be added with the same name as a previous key.
    /// The old key will be replaced
    /// </summary>

    public class ReplaceChatStringDictionary : Dictionary<string, object>
    {
        string DictName;
        IPEndPoint RemoteIpEndPoint;
        System.Net.Sockets.UdpClient udpClientB;

        public ReplaceChatStringDictionary(string DictName, int Port)
        {
            this.DictName = DictName;
            udpClientB = new System.Net.Sockets.UdpClient();
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);
            // Sends a message to the host to which you have connected.
            Byte[] sendBytes = Encoding.ASCII.GetBytes(DictName + "Is anybody there?");

            // Sends a message to a different host using optional hostname and port parameters.
            udpClientB.Send(sendBytes, sendBytes.Length, RemoteIpEndPoint);
        }
        private object CriticalSectionLock = new object();
        public new void Add(string Key, object Value)
        {
            lock (CriticalSectionLock)
            {
                bool NewValue = true;
                if (base.ContainsKey(Key) == false)
                    base.Add(Key, Value);
                else
                {
                    if (this[Key] != Value)
                    {
                        base.Remove(Key);
                        base.Add(Key, Value);
                    }
                    else
                        NewValue = false;
                }

                if (udpClientB != null && NewValue == true)
                {
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(DictName + ":" + Key + " = " + Value.ToString());

                    // Sends a message to a different host using optional hostname and port parameters.
                    udpClientB.Send(sendBytes, sendBytes.Length, RemoteIpEndPoint);
                }
                if (NewValue)
                    Console.WriteLine(Key + " = " + Value);
            }

        }

        public void AddSafe(string Key, object Value)
        {
            lock (CriticalSectionLock)
            {
                bool NewValue = true;
                if (base.ContainsKey(Key) == false)
                    base.Add(Key, Value);
                else
                {
                    if (this[Key] != Value)
                    {
                        base.Remove(Key);
                        base.Add(Key, Value);
                    }
                    else
                        NewValue = false;
                }

                if (udpClientB != null && NewValue == true)
                {
                    try
                    {
                        Byte[] sendBytes = Encoding.ASCII.GetBytes(DictName + ":" + Key + " = " + Value.ToString());

                        // Sends a message to a different host using optional hostname and port parameters.
                        udpClientB.Send(sendBytes, sendBytes.Length, RemoteIpEndPoint);
                    }
                    catch { }
                }
                try
                {
                    if (NewValue)
                        Console.WriteLine(Key + " = " + Value);
                }
                catch { }
            }
        }

    }
}
