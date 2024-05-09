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

    string serverIP = "192.168.0.135";
    public string ServerIP { get => serverIP; set => serverIP = value; }

    [SerializeField]
    bool isInDevelopment = true;
    public bool IsInDevelopment { get => isInDevelopment; set => isInDevelopment = value; }

    [SerializeField]
    bool isOffLineMode = true;
    public bool IsOffLineMode { get => isOffLineMode; set => isOffLineMode = value; }

    [SerializeField]
    bool showPerformerAxis = false;

    private ImageTrackingStablizer relocalizationStablizer;
    public ImageTrackingStablizer RelocalizationStablizer { get => relocalizationStablizer; }

    private RoleManager roleManager;
    public RoleManager RoleManager { get => roleManager; }

    private UIController uiController;
    public UIController UIController { get => uiController; }

    public RoleManager.PlayerRole PlayerRole { get => roleManager.GetPlayerRole(); }
    void InitializeReferences()
    {
        // Initialize References
        relocalizationStablizer = FindObjectOfType<ImageTrackingStablizer>();
        if (relocalizationStablizer == null)
        {
            Debug.LogError("No ImageTrackingStablizer Found.");
        }

        roleManager = FindObjectOfType<RoleManager>();
        if (roleManager == null)
        {
            Debug.LogError("No RoleManager Found.");
        }
        


        uiController = FindObjectOfType<UIController>();
        if (uiController == null)
        {
            Debug.LogError("No UIController Found.");
        }
    }

    void OnEnable()
    {
        roleManager.OnReceiveConnectionResultEvent.AddListener(OnReceiveConnectionResult);
        roleManager.OnReceiveRegistrationResultEvent.AddListener(OnReceiveRegistrationResult);
    }

    void OnDisable()
    {
        roleManager.OnReceiveConnectionResultEvent.RemoveListener(OnReceiveConnectionResult);
        roleManager.OnReceiveRegistrationResultEvent.RemoveListener(OnReceiveRegistrationResult);
    }

    void Start()
    {
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
        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
        {
            string local_ip = GetLocalIPAddress();
            Debug.Log("Local IP:" + local_ip);
            if (local_ip == ServerIP || isOffLineMode)
            {
                JoinAsServer();
                uiController.GoIntoGame();
            }
        }
    }


    public void JoinAsServer()
    {
        OnBeforeHostStarted();

        NetworkManager.Singleton.StartServer();

        RoleManager.JoinAsServer();

        Debug.Log("Server Started.");
    }

    public void JoinAsPerformer()
    {
        OnBeforeClientStarted();

        NetworkManager.Singleton.StartClient();

        RoleManager.JoinAsPerformer();

        Debug.Log("Join As Performer.");
    }

    public void JoinAsAudience()
    {
        OnBeforeClientStarted();

        NetworkManager.Singleton.StartClient();

        RoleManager.JoinAsAudience();

        Debug.Log("Join As Audience.");
    }

    public void DisplayMessageOnUI(string msg)
    {
        uiController.DisplayMessageOnUI(msg);
    }

    public void RestartGame()
    {
        NetworkManager.Singleton.Shutdown();

        uiController.GoBackToHomePage();

        roleManager.ResetPlayerRole();
    }
    public void Relocalize()
    {
        uiController.GoToRelocalizationPage();
    }

    public void TogglePerformerAxis()
    {
        SetPerformerAxisState(!showPerformerAxis);
    }

    public void SetPerformerAxisState(bool state)
    {
        showPerformerAxis = state;

        MeshRenderer[] mesh_renderers = roleManager.PerformerTransformRoot.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in mesh_renderers)
        {
            renderer.enabled = state;
        }
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
            unityTransport.SetConnectionData(ServerIP, (ushort)7777);
        }
    }

    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && ip.ToString().Contains("192.168"))
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    void OnReceiveConnectionResult(bool result)
    {
        if (result == false)
            RestartGame();
    }

    void OnReceiveRegistrationResult(bool result)
    {
        if (result == false)
            RestartGame();
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

        //if (_Instance == null)
        //{
        //    _Instance = this;
        //    DontDestroyOnLoad(this.gameObject);
        //}
        //else
        //{
        //    Destroy(this);
        //}

        InitializeReferences();
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
