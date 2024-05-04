using System.Collections;
using System.Collections.Generic;
using HoloKit.ImageTrackingRelocalization;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class GameManager : MonoBehaviour
{
    string performerPassword = "111";
    public string PerformerPassword { get => performerPassword; set => performerPassword = value; }

    string serverIp = "192.168.0.135";
    public string ServerIp { get => serverIp; set => serverIp = value; }

    private ImageTrackingStablizer relocalizationStablizer;
    public ImageTrackingStablizer RelocalizationStablizer { get => relocalizationStablizer; }

    private RoleManager roleManager;
    public RoleManager RoleManager { get => roleManager; }

    void Start()
    {
        // Initialize References
        relocalizationStablizer = FindObjectOfType<ImageTrackingStablizer>();
        if (relocalizationStablizer == null)
        {
            Debug.LogError("No ImageTrackingStablizer Found.");
        }

        //roleManager = FindObjectOfType<RoleManager>();
        //if (roleManager == null)
        //{
        //    Debug.LogError("No RoleManager Found.");
        //}


        // Read Command Line
        string mode_from_command = GetModeFromCommandLine();
        switch (mode_from_command)
        {
            case "server":
                JoinAsServer();
                break;
            case "performer":
                JoinAsPerformer();
                break;
            case "client":
                JoinAsAudience();
                break;
            default:
                // Do nothing
                break;
        }

        // Specify Role When Testing
        string local_ip = GetLocalIPAddress();
        if (local_ip == ServerIp)
            JoinAsServer();
        else
            JoinAsAudience();
    }

    public void JoinAsServer()
    {
        OnBeforeHostStarted();

        NetworkManager.Singleton.StartServer();

        //RoleManager.JoinAsServer();

        Debug.Log("Join As Server.");
    }

    public void JoinAsPerformer()
    {
        OnBeforeClientStarted();

        NetworkManager.Singleton.StartClient();

        //RoleManager.JoinAsPerformer();

        // Try to register as a performer
        Debug.Log("Join As Performer.");
    }

    public void JoinAsAudience()
    {
        OnBeforeClientStarted();

        NetworkManager.Singleton.StartClient();

        //RoleManager.JoinAsAudience();

        Debug.Log("Join As Audience.");
    }




    #region Network
    void OnBeforeHostStarted()
    {
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (unityTransport != null)
        {
            string localIPAddress = GetLocalIPAddress();
            unityTransport.SetConnectionData(localIPAddress, (ushort)7777);
            unityTransport.ConnectionData.ServerListenAddress = "0.0.0.0";
            //m_HostIPAddress.text = $"Host IP Address: {localIPAddress}";
        }
    }
    void OnBeforeClientStarted()
    {
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (unityTransport != null)
        {
            unityTransport.SetConnectionData(ServerIp, (ushort)7777);
        }
    }

    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
    #endregion

    #region Instance
    private static GameManager _Instance;

    public static GameManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<GameManager>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    _Instance = go.AddComponent<GameManager>();
                }
            }
            return _Instance;
        }
    }
    private void Awake()
    {
        if (_Instance != null)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        _Instance = null;
    }
    #endregion

    #region ARGS

    private string GetModeFromCommandLine()
    {
        var args = GetCommandlineArgs();

        if (args.TryGetValue("-mode", out string mode))
        {
            return mode;
        }
        return "";
    }
    private Dictionary<string, string> GetCommandlineArgs()
    {
        Dictionary<string, string> argDictionary = new Dictionary<string, string>();

        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; ++i)
        {
            var arg = args[i].ToLower();
            if (arg.StartsWith("-"))
            {
                var value = i < args.Length - 1 ? args[i + 1].ToLower() : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;

                argDictionary.Add(arg, value);
            }
        }
        return argDictionary;
    }
    #endregion
}
