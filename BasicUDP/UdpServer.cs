using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class UdpServer
{
    static void Main(string[] args)
    {
        int port = 5000;

        if (args.Length >= 1)
            int.TryParse(args[0], out port);

        try
        {
            StartUdpServer(port);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"error: {ex.Message}");
        }
    }

    static void StartUdpServer(int port)
    {
        using (UdpClient udpServer = new UdpClient(port))
        {
            Console.WriteLine($"udp server started");
            Console.WriteLine($"port: {port}");
            Console.WriteLine($"waiting..(Ctrl+C to exit)\n");

            try
            {
                while (true)
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedData = udpServer.Receive(ref remoteEndPoint);

                    DataPackType dp_type=(DataPackType)receivedData[0];

                    //ExampleDataTarget target= Deserialize
                    Console.WriteLine($"responded");
                    Console.WriteLine($"   sender: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                    Console.WriteLine($"   time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

                    //respond
                    /*
                    if(respond!=null){
                        udpServer.Send(respond, respond.Length, remoteEndPoint);
                        Console.WriteLine($"respond: {respond}\n");
                    }
                    */
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"socket error: {ex.Message}");
            }
        }
    }
}

public enum DataPackType{msg=0}

public class ExampleDataTarget<T>
{
    public int id;
    public int sequenceID;
    public T value;
}