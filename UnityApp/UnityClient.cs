using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace LLNetCode
{

public class UnityNetClient
{
    private const string mainServerIP = "127.0.0.1";
    private const int tcp_port = 5001,udp_send_port=5004,udp_receive_port=5005;

    /*TCP SOCKET*/

    public static async Task<byte[]> SendTCPQuery(byte[] data)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(mainServerIP, tcp_port);
                
                using (NetworkStream stream = client.GetStream())
                {
                    await stream.WriteAsync(data, 0, data.Length);

                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        byte[] response = new byte[bytesRead];
                        Array.Copy(buffer, 0, response, 0, bytesRead);
                        return response;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"TCP 통신 에러: {e.Message}");
        }
        return null; 
    }

    //division path->client id
    public static async Task<int> AssignClient(string division_path)
    {
        ClientAssign_PacketData data = new ClientAssign_PacketData();
        data.path = division_path;

        byte[] response = await SendTCPQuery(QueryProtocol.Serialize(data));

        if (response == null || response.Length < 4)
        {
            Debug.LogError("서버로부터 유효하지 않은 응답을 받았습니다.");
            return -1; 
        }

        return ResponseProtocol.Deserialize<int>(response);
    }

    //division path->client datas
    public static async Task<ClientMap> GetDivisionClients(string path)
    {
        
        DivisionClients_PacketData data = new DivisionClients_PacketData();
        data.path = path;

        // 2. 직렬화 및 TCP 요청 (UnityProcess는 비동기로 구현되어 있어야 함)
        byte[] response = await SendTCPQuery(QueryProtocol.Serialize(data));

        // 3. 예외 처리: 데이터가 비어있거나 부족할 경우
        if (response == null || response.Length < 4)
        {
            UnityEngine.Debug.LogError("서버로부터 유효하지 않은 응답을 받았습니다.");
            return null; 
        }

        return ResponseProtocol.Deserialize<ClientMap>(response);
    }
    /*UDP SOCKET*/
    public static async void SendUDPData(byte[] data)
    {
        using (UdpClient udpClient = new UdpClient())
        {
            try
            {
                await udpClient.SendAsync(data, data.Length, mainServerIP, udp_send_port);
            }
            catch (Exception e)
            {
                Console.WriteLine($"오류 발생: {e.Message}");
            }
        }
    }

    public static void SendMsg(string msg,string path="/")
    {
        SendMsg_PacketPata data = new SendMsg_PacketPata();
        data.msg=msg;
        data.path=path;
        SendUDPData(QueryProtocol.Serialize(data));
    }
    void UDPReceiveLoop()
    {
        Debug.Log("Start UdpReceive");
        Task.Run(async () =>
        {
        using (UdpClient udpListener = new UdpClient(udp_receive_port))
        {
            Debug.Log($"[UDP 수신 시작] 포트 {udp_receive_port}에서 대기 중...");
            try
            {
                while (true)
                {
                    UdpReceiveResult result = await udpListener.ReceiveAsync();

                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                    if(result.RemoteEndPoint.Address.ToString()==mainServerIP)
                    Debug.Log($"[수신] {result.RemoteEndPoint}: {receivedMessage}");
                    else Debug.Log("Hack Access");
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[수신 에러] {ex.Message}");
            }
        }
        });
    }
    public UnityNetClient()
    {
        UDPReceiveLoop();
    }
}

}