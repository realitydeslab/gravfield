using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Events;

public class NetcodeConnectionManager : MonoBehaviour
{
    [SerializeField]
    string serverIP = "192.168.0.135";
    public string ServerIP { get => serverIP; set => serverIP = value; }

    string localIP = "";
    public string LocalIP { get => localIP; }
 

    public UnityEvent<ulong> OnClientJoinedEvent;
    public UnityEvent<ulong> OnClientLostEvent;

    public UnityEvent OnServerLostEvent;

    Action<bool, string> OnReceiveConnectionResultAction;

    NetworkManager networkManager;



    void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("Serious Problem : Can't find Network Manager!");
        }

    }

    #region Event Listener
    void OnEnable()
    {
        Debug.Log("Add All NetCode Listener");
        networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        networkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
        networkManager.OnClientStarted += OnClientStarted;
        networkManager.OnClientStopped += OnClientStopped;
        networkManager.OnServerStarted += OnServerStarted;
        networkManager.OnServerStopped += OnServerStopped;
        networkManager.OnTransportFailure += OnTransportFailure;
        networkManager.OnConnectionEvent += OnConnectionEvent;
    }

    void OnDisable()
    {
        Debug.Log("Remove All NetCode Listener");
        networkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        networkManager.OnClientStarted -= OnClientStarted;
        networkManager.OnClientStopped -= OnClientStopped;
        networkManager.OnServerStarted -= OnServerStarted;
        networkManager.OnServerStopped -= OnServerStopped;
        networkManager.OnTransportFailure -= OnTransportFailure;
        networkManager.OnConnectionEvent -= OnConnectionEvent;
    }

    void OnClientStarted()
    {
        Debug.Log("Listener : OnClientStarted");
        localIP = GetLocalIPAddress();
    }

    void OnClientStopped(bool result)
    {
        Debug.Log("Listener : OnClientStopped " + result);
    }

    void OnServerStarted()
    {
        Debug.Log("Listener : OnServerStarted");
        localIP = GetLocalIPAddress();
    }

    void OnServerStopped(bool result)
    {
        Debug.Log("Listener : OnServerStopped " + result);
    }

    void OnTransportFailure()
    {
        Debug.Log("OnTransportFailure");
    }

    void OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        //Debug.Log("OnConnectionEvent | " + data.ClientId + "Remain Count " + manager.ConnectedClientsList.Count + "," + manager.ConnectedClients.Count + ", " + manager.ConnectedClientsIds.Count);
    }

    void OnClientConnectedCallback(ulong client_id)
    {
        Debug.Log(string.Format("OnClientConnectedCallback | IsServer:{0}, IsClient:{1}, ClientID:{2}", networkManager.IsServer, networkManager.IsClient, client_id));

        if(networkManager.IsServer)
        {
            OnClientJoinedEvent?.Invoke(client_id);
        }

        if(networkManager.IsClient)
        {
            string msg = "Connected.";
            OnReceiveConnectionResultAction?.Invoke(true, msg);
            OnReceiveConnectionResultAction = null;
        }
    }

    void OnClientDisconnectCallback(ulong client_id)
    {
        Debug.Log(string.Format("OnClientDisconnectCallback | IsServer:{0}, IsClient:{1}, ClientID:{2}", networkManager.IsServer, networkManager.IsClient, client_id));
        if (networkManager.IsServer)
        {
            OnClientLostEvent?.Invoke(client_id);
            
            // Unbind Performer if needed
            //int performer_index = GetPerformerIndexByID(client_id);

            //if (performer_index != -1)
            //{
            //    RemovePerformerRpc(performer_index, client_id);
            //}

            //    //RefreshPlayerCount();
        }


        if (networkManager.IsClient)
        {
            // When Client trys to connect with Server
            if (OnReceiveConnectionResultAction != null)
            {
                string msg = "Couldn't connect to server.";
                OnReceiveConnectionResultAction?.Invoke(false, msg);
                OnReceiveConnectionResultAction = null;
            }

            // When suddenly lost Server
            else
            {
                OnServerLostEvent?.Invoke();
            }
        }
    }
        #endregion

    public bool IsIPAddressValide(string ip)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");
    }
    public void SetServerIP(string ip)
    {
        serverIP = ip;
    }

    public void StartClient(Action<bool, string> callback)
    {
        OnBeforeClientStarted();

        bool result = NetworkManager.Singleton.StartClient();

        if (result == true)
        {
              OnReceiveConnectionResultAction = callback;
        }
        else
        {
            string msg = "Failed to start client.";
            callback?.Invoke(false, msg);
        }
    }

    public void StartServer(Action<bool, string> callback)
    {
        OnBeforeHostStarted();

        bool result = NetworkManager.Singleton.StartServer();

        string msg = result ? "Server started." : "Failed to start server";
        callback?.Invoke(result, msg);
    }

    public void ShutDown()
    {
        if(NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            NetworkManager.Singleton.Shutdown();
    }


    void OnBeforeHostStarted()
    {
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (unityTransport != null)
        {
            //string localIPAddress = GetLocalIPAddress();
            unityTransport.SetConnectionData("127.0.0.1", (ushort)7777);
            unityTransport.ConnectionData.ServerListenAddress = "0.0.0.0";
            //m_HostIPAddress.text = $"Host IP Address: {localIPAddress}";
        }
    }
    void OnBeforeClientStarted()
    {
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (unityTransport != null)
        {
            unityTransport.SetConnectionData(ServerIP, (ushort)7777);
        }
    }

    string GetLocalIPAddress()
    {

        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) // && ip.ToString().Contains("192.168"))
            {
                return ip.ToString();
            }
        }
        //throw new System.Exception("No network adapters with an IPv4 address in the system!");
        return "";
    }

}
