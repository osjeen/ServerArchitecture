using UnityEngine;
using System.Net;
using System.Collections.Generic;

public class ClientStructure
{
    public IPEndPoint client_endpoint;
    public int uuid;
    public string path;
}

public class ClientMap
{
    public List<ClientStructure> client_structures =new List<ClientStructure>();
}