using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ServerInfo
{
    public string ip;
    public long time;
}

public class ServerIPSynchronizer : MonoBehaviour
{
    
    private string serverIp;
    public string ServerIP
    {
        get => serverIp;
    }

    private ServerInfo serverInfo;
    public ServerInfo ServerInfo
    {
        get => serverInfo;
    }

    void Awake()
    {

    }

    void Start()
    {

    }

    void OnRoleSpecified()
    {
        //if(role == server)
        //{
        //    Start broadcasting self ip
        //}
        //else
        //{
        //    Start receiving server ip
        //}
    }
}
