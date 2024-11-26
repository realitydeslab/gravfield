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
using UnityEngine.Events;

public enum PlayerRole
{
    Undefined,

    Performer,
    Spectator,
    Server,
    SinglePlayer,
    Commander
}

[DefaultExecutionOrder(-20)]
public class GameManager : MonoBehaviour
{
    [SerializeField]
    bool isInDevelopment = false;
    public bool IsInDevelopment { get => isInDevelopment; set => isInDevelopment = value; }

    bool isSoloMode = false;
    public bool IsSoloMode { get => isSoloMode; set => isSoloMode = value; }

    bool showPerformerAxis = false;

    HoloKitCameraManager holoKitCameraManager;
    public HoloKitCameraManager HolokitCameraManager { get => holoKitCameraManager; }

    private AudioProcessor audioProcessor;
    public AudioProcessor AudioProcessor { get => audioProcessor; }

    private NetcodeConnectionManager connectionManager;
    public NetcodeConnectionManager ConnectionManager { get => connectionManager; }

    private ImageTrackingStablizer relocalizationStablizer;
    public ImageTrackingStablizer RelocalizationStablizer { get => relocalizationStablizer; }

    private MiddlewareManager middlewareManager;
    public MiddlewareManager MiddlewareManager { get => middlewareManager; }

    private PlayerManager playerManager;
    public PlayerManager PlayerManager { get => playerManager; }

    private Commander commander;
    public Commander Commander { get => commander; }

    private UIController uiController;
    public UIController UIController { get => uiController; }

    public List<Performer> PerformerList { get => playerManager.PerformerList; }

    private PlayerRole playerRole;
    public PlayerRole PlayerRole { get => playerRole; }

    public UnityEvent<PlayerRole> OnStartGame;
    public UnityEvent<PlayerRole> OnStopGame;

    bool isInitialzed = false;

    #region Join As Specific Role
    public void JoinAsSpectator(System.Action<bool, string> action)
    {
        Debug.Log("Join As Spectator.");

        connectionManager.StartClient(((connection_result, connection_msg) => {
            if (connection_result == true)
            {
                // Start Game as player
                StartGame(PlayerRole.Spectator);
            }

            // Update UI
            action?.Invoke(connection_result, connection_msg);

        }));
    }

    public void JoinAsPerformer(System.Action<bool, string> action)
    {
        Debug.Log("Join As Performer.");

        connectionManager.StartClient(((connection_result, connection_msg) => {

            // Update UI
            action?.Invoke(connection_result, connection_msg);

        }));
    }

    public void ApplyPerformer(System.Action<bool, string> action)
    {
        PlayerManager.ApplyPerformer((result, msg) =>
        {
            if (result == true)
            {
                StartGame(PlayerRole.Spectator);
            }
            else
            {
                connectionManager.ShutDown();
            }

            // Update UI
            action?.Invoke(result, msg);
        });
    }





    public void JoinAsServer(System.Action<bool, string> action)
    {
        Debug.Log("Join As Server.");

        connectionManager.StartServer(new System.Action<bool, string>((result, msg) => {

            if(result)
            {
                StartGame(PlayerRole.Server);
            }

            action?.Invoke(result, msg);
        }));
    }



    public void JoinAsCommander(System.Action<bool, string> action)
    {
        Debug.Log("Join As Commander.");

        connectionManager.StartClient(((connection_result, connection_msg) => {

            if (connection_result == true)
            {
                StartGame(PlayerRole.Commander);

                //        RoleManager.JoinAsCommander();
                //        Commander.InitializeCommander();
            }

            // Update UI
            action?.Invoke(connection_result, connection_msg);

        }));
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
        PlayerManager.OnClientJoined(client_id);
    }
    void OnClientLost(ulong client_id)
    {
        PlayerManager.OnClientLost(client_id);

    }
    void OnServerLost()
    {
        //RestartGame();
    }
    #endregion


    #region Relocalization
    public void StartRelocalization(System.Action action)
    {
#if !UNITY_EDITOR
        UnityAction<Vector3, Quaternion> handler = null;
        handler = (Vector3 position, Quaternion rotation) =>
        {
            relocalizationStablizer.OnTrackedImagePoseStablized.RemoveListener(handler);

            OnFinishRelocalization(position, rotation, action);            
        };
        relocalizationStablizer.OnTrackedImagePoseStablized.AddListener(handler);

        relocalizationStablizer.IsRelocalizing = true;
#else
        OnFinishRelocalization(Vector3.zero, Quaternion.identity, action);

        // Rotate body by 90 degrees so the fake people representing unity editor can turn around to face real player holding phones
        //OnFinishRelocalization(Vector3.zero, Quaternion.AngleAxis(90, Vector3.up), action);
#endif
    }

    public void StopRelocalization()
    {
#if !UNITY_EDITOR
        relocalizationStablizer.OnTrackedImagePoseStablized.RemoveAllListeners() ;

        relocalizationStablizer.IsRelocalizing = false;
#endif
    }

    void OnFinishRelocalization(Vector3 position, Quaternion rotation, System.Action action)
    {
#if !UNITY_EDITOR
        HoloKit.ColocatedMultiplayerBoilerplate.TrackedImagePoseTransformer trackedImagePoseTransformer;
        trackedImagePoseTransformer = FindFirstObjectByType<HoloKit.ColocatedMultiplayerBoilerplate.TrackedImagePoseTransformer>();

        if(trackedImagePoseTransformer == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find TrackedImagePoseTransformer.");
        }
        else
        {
            trackedImagePoseTransformer.OnTrackedImageStablized(position, rotation);
        }
#endif

        action?.Invoke();
    }

    public float GetRelocalizationProgress()
    {
        if (relocalizationStablizer.IsRelocalizing)
            return relocalizationStablizer.Progress;
        else
            return 0;
    }

    #endregion




    #region Game control
    public void StartGame(PlayerRole player_role)
    {
        StartCoroutine(WaitForNetworkObjectSpawnedCoroutine(3, (result, msg) => {

            if(result == true)
            {
                StartGameByPlayerRole(player_role);

                
            }
            else
            {
                //RestartGame();
            }
        }));
    }

    IEnumerator WaitForNetworkObjectSpawnedCoroutine(float time_out, System.Action<bool, string> action)
    {
        float start_time = Time.time;
        bool result = true;
        while (true)
        {
            if(NetworkManager.Singleton != null && NetworkManager.Singleton.SpawnManager != null && NetworkManager.Singleton.SpawnManager.SpawnedObjects != null)
            {
                bool all_spawned = true;
                foreach(var network_object in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
                {
                    if(network_object.Value.IsSpawned == false)
                    {
                        all_spawned = false;
                        break;
                    }
                }
                if(all_spawned)
                {
                    break;
                }
            }

            yield return new WaitForSeconds(0.1f);

            if (Time.time - start_time > time_out)
            {
                result = false;
                break;
            }
        }

        string msg = result ? "Game initialization completed." : "Game initialization times out.";

        Debug.Log($"[{this.GetType()}] {msg}");

        action?.Invoke(result, msg);
    }

    void StartGameByPlayerRole(PlayerRole player_role)
    {
        playerRole = player_role;

        //PlayerManager.SetRole();

        OnStartGame?.Invoke(player_role);


        //if (player_role == PlayerRole.Server)
        //{
        //    middlewareManager.TurnOn();            

        //    Effect.RegisterSenderAndReceiver();

        //    if(isSoloMode)
        //    {
        //        CameraMovement.StartCinemachineMode();

        //        TestScenarioManager.TurnOn();
        //    }
        //}

        //if(player_role == PlayerRole.Commander)
        //{
        //    commander.TurnOn();
        //}

        

    }

    public void RestartGame(System.Action action)
    {
        ConnectionManager.ShutDown();

        //if (PlayerManager.Role == PlayerRole.Server)
        //{
        //    ControlPanel.ClearAllPropertyInControlPanel();
        //    MiddlewareManager.TurnOff();
        //}
        //else if (PlayerManager.Role == PlayerRole.Commander)
        //{
        //    Commander.DeinitializeCommander();
        //}

        OnStopGame?.Invoke(playerRole);

        playerRole = PlayerRole.Undefined;

        UIController.GoBackToHomePage();
    }

    void ResetGame()
    {

    }
    #endregion

    #region UI Related Functions




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

        MeshRenderer[] mesh_renderers = playerManager.PerformerTransformRoot.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in mesh_renderers)
        {
            renderer.enabled = state;
        }
    }
    #endregion

    

    void InitializeReferences()
    {
        // Initialize References
        holoKitCameraManager = FindFirstObjectByType<HoloKitCameraManager>();
        if (holoKitCameraManager == null)
        {
            Debug.LogError("No HoloKitCameraManager Found.");
        }
        
        audioProcessor = FindFirstObjectByType<AudioProcessor>();
        if (audioProcessor == null)
        {
            Debug.LogError("No AudioProcessor Found.");
        }

        connectionManager = FindFirstObjectByType<NetcodeConnectionManager>();
        if (connectionManager == null)
        {
            Debug.LogError("No NetcodeConnectionManager Found.");
        }

        relocalizationStablizer = FindFirstObjectByType<ImageTrackingStablizer>();
        if (relocalizationStablizer == null)
        {
            Debug.LogError("No ImageTrackingStablizer Found.");
        }

        middlewareManager = FindFirstObjectByType<MiddlewareManager>();
        if (middlewareManager == null)
        {
            Debug.LogError("No MiddlewareManager Found.");
        }

        playerManager = FindFirstObjectByType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("No RoleManager Found.");
        }

        commander = FindFirstObjectByType<Commander>();
        if (commander == null)
        {
            Debug.LogError("No Commander Found.");
        }

        uiController = FindFirstObjectByType<UIController>();
        if (uiController == null)
        {
            Debug.LogError("No UIController Found.");
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
                _Instance = GameObject.FindFirstObjectByType<GameManager>();
                //if (_Instance == null)
                //{
                //    GameObject go = new GameObject();
                //    _Instance = go.AddComponent<GameManager>();
                //}

                if(_Instance != null)
                {
                    _Instance.InitializeReferences();
                    DontDestroyOnLoad(_Instance);
                }
            }
            return _Instance;
        }
    }
    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            InitializeReferences();
        }
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
