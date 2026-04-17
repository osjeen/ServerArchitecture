using UnityEngine;
using System.Threading.Tasks;
using System.Buffers.Binary;
public class UnityTCPQuery
{

    public static async Task<int> AssignClient(string division_path)
    {
        ClientAssign_PacketData data = new ClientAssign_PacketData();
        data.path = division_path;

        // 2. 직렬화 및 TCP 요청 (UnityProcess는 비동기로 구현되어 있어야 함)
        byte[] response = await UnityProcess.SendTCPQuery(QueryProtocol.Serialize(data));

        // 3. 예외 처리: 데이터가 비어있거나 부족할 경우
        if (response == null || response.Length < 4)
        {
            UnityEngine.Debug.LogError("서버로부터 유효하지 않은 응답을 받았습니다.");
            return -1; 
        }

        return ResponseProtocol.Deserialize<int>(response);
    }

    public static async Task<ClientMap> GetDivisionClients(string path)
    {
        
        DivisionClients_PacketData data = new DivisionClients_PacketData();
        data.path = path;

        // 2. 직렬화 및 TCP 요청 (UnityProcess는 비동기로 구현되어 있어야 함)
        byte[] response = await UnityProcess.SendTCPQuery(QueryProtocol.Serialize(data));

        // 3. 예외 처리: 데이터가 비어있거나 부족할 경우
        if (response == null || response.Length < 4)
        {
            UnityEngine.Debug.LogError("서버로부터 유효하지 않은 응답을 받았습니다.");
            return null; 
        }

        return ResponseProtocol.Deserialize<ClientMap>(response);
    }
}