using System.Collections;
using System.Collections.Generic;
using HoloKit.ImageTrackingRelocalization;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using HoloKit;

[DefaultExecutionOrder(-20)]
public class GameManager : MonoBehaviour
{
    string performerPassword = "111";
    public string PerformerPassword { get => performerPassword; set => performerPassword = value; }

    [SerializeField]
    bool isInDevelopment = true;
    public bool IsInDevelopment { get => isInDevelopment; set => isInDevelopment = value; }

    [SerializeField]
    bool isSoloMode = true;
    public bool IsSoloMode { get => isSoloMode; set => isSoloMode = value; }

    [SerializeField]
    bool showPerformerAxis = false;

    HoloKitCameraManager holoKitCameraManager;
    public HoloKitCameraManager HolokitCameraManager { get => holoKitCameraManager; }

    private AudioProcessor audioProcessor;
    public AudioProcessor AudioProcessor { get => audioProcessor; }

    private NetcodeConnectionManager connectionManager;
    public NetcodeConnectionManager ConnectionManager { get => connectionManager; }

    private ImageTrackingStablizer relocalizationStablizer;
    public ImageTrackingStablizer RelocalizationStablizer { get => relocalizationStablizer; }

    private RoleManager roleManager;
    public RoleManager RoleManager { get => roleManager; }

    private UIController uiController;
    public UIController UIController { get => uiController; }

    private PerformerGroup performerGroup;
    public PerformerGroup PerformerGroup { get => performerGroup; }

    private List<Performer> performerList;
    public List<Performer> PerformerList { get => performerList; }

    public RoleManager.PlayerRole PlayerRole { get => roleManager.GetPlayerRole(); }

    private bool isPlaying = false;
    public bool IsPlaying { get => isPlaying; }

    bool isInitialzed = false;
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

        //// Specify Role When Testing
        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer
            || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            
        }
        else
        {
            isSoloMode = false;
        }

        StartCoroutine(CheckIfNeedJoinAsServer());
    }

    IEnumerator CheckIfNeedJoinAsServer()
    {
        float start_time = Time.time;
        bool enter_solo_mode = false;
        while(Time.time - start_time < 5)
        {
            if(Input.GetKey(KeyCode.S))
            {
                enter_solo_mode = true;
                break;
            }
            yield return null;
        }
        

        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer
            || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if(RoleManager.Role == RoleManager.PlayerRole.Undefined)
            {
                isSoloMode = enter_solo_mode;
                JoinAsServer();
            }           
        }
    }


    #region Join As Specific Role
    public void JoinAsAudience()
    {
        Debug.Log("Join As Audience.");

        ConnectionManager.StartClient(callback: OnReceiveResult_JoinAsAudience);

        UIController.GotoWaitingPage("Connecting to server.");
    }

    void OnReceiveResult_JoinAsAudience(bool result, string msg)
    {
        if (result == true)
        {
            RoleManager.JoinAsAudience();

            UIController.GotoWaitingPage("Connected.");

            isPlaying = true;

            StartRelocalization();
        }
        else
        {
            UIController.GotoWaitingPage(msg);

            RestartGame();
        }
    }

    public void JoinAsPerformer(string password="")
    {
        Debug.Log("Join As Performer.");

        if(password == PerformerPassword)
        {
            ConnectionManager.StartClient(callback: OnReceiveResult_JoinAsPerformer);

            UIController.GotoWaitingPage("Connecting to server.");
        }
        else
        {
            UIController.ShowWarningText("Wrong Password.");
        }
    }
    void OnReceiveResult_JoinAsPerformer(bool result, string msg)
    {
        if (result == true)
        {
            RoleManager.ApplyPerformer(callback: OnReceiveResult_ApplyPeroformer);

            UIController.GotoWaitingPage("Applying to be a performer.");
        }
        else
        {
            UIController.GotoWaitingPage(msg);

            RestartGame();
        }
    }

    void OnReceiveResult_ApplyPeroformer(bool result, string msg)
    {
        if (result == true)
        {
            RoleManager.JoinAsPerformer();

            UIController.GotoWaitingPage("Succeed.");

            isPlaying = true;

            StartRelocalization();
        }
        else
        {
            UIController.GotoWaitingPage(msg);

            RestartGame();
        }
    }

    public void JoinAsServer()
    {
        Debug.Log("Join As Server.");

        connectionManager.StartServer(callback: OnReceiveResult_JoinAsServer);
    }

    void OnReceiveResult_JoinAsServer(bool result, string msg)
    {
        if (result == true)
        {
            RoleManager.JoinAsServer();

            uiController.GotoWaitingPage("Connected.");

            isPlaying = true;

            //StartRelocalization();
            UIController.GoIntoGame(); // No need to relocalize as server

            if (isSoloMode)
            {
                EnterSoloMode();
            }

            // Register world origin callback
           
        }
        else
        {
            uiController.GotoWaitingPage(msg);

            RestartGame();
        }
    }

    void EnterSoloMode()
    {
        Debug.Log("Enter Solo Mode");
        RoleManager.EnterSoloMode();
    }
    #endregion


    #region Connection Event Listener
    void OnEnable()
    {
        ConnectionManager.OnClientJoinedEvent.AddListener(OnClientJoined);
        ConnectionManager.OnClientLostEvent.AddListener(OnClientLost);
        ConnectionManager.OnServerLostEvent.AddListener(OnServerLost);
    }

    void OnDisable()
    {
        ConnectionManager.OnClientJoinedEvent.RemoveListener(OnClientJoined);
        ConnectionManager.OnClientLostEvent.RemoveListener(OnClientLost);
        ConnectionManager.OnServerLostEvent.RemoveListener(OnServerLost);
    }

    void OnClientJoined(ulong client_id)
    {
        RoleManager.OnClientJoined(client_id);
    }
    void OnClientLost(ulong client_id)
    {
        RoleManager.OnClientLost(client_id);

    }
    void OnServerLost()
    {
        RestartGame();
    }
    #endregion


    #region UI Related Functions
    public void StartRelocalization()
    {
        UIController.GoToRelocalizationPage();

#if !UNITY_EDITOR
        RelocalizationStablizer.OnTrackedImagePoseStablized.AddListener(OnFinishRelocalization);

        RelocalizationStablizer.IsRelocalizing = true;
#else
        OnFinishRelocalization(Vector3.zero, Quaternion.identity);
#endif
    }

    public void StopRelocalization()
    {
        UIController.HideRelocalizationPage();

#if !UNITY_EDITOR
        GameManager.Instance.RelocalizationStablizer.OnTrackedImagePoseStablized.RemoveListener(OnFinishRelocalization);

        RelocalizationStablizer.IsRelocalizing = false;
#endif
    }

    void OnFinishRelocalization(Vector3 position, Quaternion rotation)
    {
        //DisplayMessageOnUI($"pos: {position}, angle: {rotation.eulerAngles}");

#if !UNITY_EDITOR
            GameManager.Instance.RelocalizationStablizer.OnTrackedImagePoseStablized.RemoveListener(OnFinishRelocalization);
#endif
        UIController.GoIntoGame();
    }

    public void RestartGame()
    {
        ConnectionManager.ShutDown();

        isPlaying = false;

        RoleManager.ResetPlayerRole();

        UIController.GoBackToHomePage();
    }

    public void DisplayMessageOnUI(string msg)
    {
        uiController.ShowWarningText(msg);
    }
    #endregion


    #region Helping Functions
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
    #endregion

    

    void InitializeReferences()
    {
        // Initialize References
        holoKitCameraManager = FindObjectOfType<HoloKitCameraManager>();
        if (holoKitCameraManager == null)
        {
            Debug.LogError("No HoloKitCameraManager Found.");
        }
        
        audioProcessor = FindObjectOfType<AudioProcessor>();
        if (audioProcessor == null)
        {
            Debug.LogError("No AudioProcessor Found.");
        }

        connectionManager = FindObjectOfType<NetcodeConnectionManager>();
        if (connectionManager == null)
        {
            Debug.LogError("No NetcodeConnectionManager Found.");
        }

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

        performerGroup = FindObjectOfType<PerformerGroup>();
        if (uiController == null)
        {
            Debug.LogError("No PerformerGroup Found.");
        }

        if(performerGroup!= null)
        {
            Transform performer_root = performerGroup.transform;
            performerList = new List<Performer>();
            for(int i=0; i< performer_root.childCount; i++)
            {
                performerList.Add(performer_root.GetChild(i).GetComponent<Performer>());
            }
        }
    }

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

        InitializeReferences();
    }

    private void OnDestroy()
    {
        //_Instance = null;
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
