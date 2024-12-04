using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


public class PlayerManager : NetworkBehaviour
{
    
    [SerializeField]
    private Transform performerTransformRoot;
    public Transform PerformerTransformRoot { get => performerTransformRoot; }

    private PlayerRole playerRole = PlayerRole.Undefined;
    public PlayerRole Role { get => playerRole; }

    private int audienceCount = 0;
    public int AudienceCount { get => audienceCount; }

    private int performerCount = 0;
    public int PerformerCount { get => performerCount; }

    public UnityEvent<PlayerRole> OnSpecifyPlayerRoleEvent;


    public UnityEvent<int, ulong> OnStartPerformingEvent;
    public UnityEvent<int, ulong> OnStopPerformingEvent;

    // When OnAddPerformerEvent triggers, each clients' NetworkVariable may has not been updated
    // So these two events can not be used as an indicator to trigger effects, they are more like server events
    private UnityEvent<int> OnAddPerformerEvent;
    private UnityEvent<int> OnRemovePerformerEvent;
    

    [SerializeField]
    private float performerApplyTimeout = 10;
    private Action<bool, string> OnReceiveRegistrationResultAction;

    private PerformerSynchronizer performerSynchronizer;
    private List<Performer> performerList = new List<Performer>();
    public List<Performer> PerformerList { get => performerList; }

    /// <summary>
    /// https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable/
    /// In-Scene Placed: Since the instantiation occurs via the scene loading mechanism(s), the Start method is invoked before OnNetworkSpawn.
    /// </summary>
    void Awake()
    {
        performerSynchronizer = FindObjectOfType<PerformerSynchronizer>();
        InitializePerformerList();
    }

    void OnEnable()
    {
        GameManager.Instance.OnStartGame.AddListener(OnStartGame);
        GameManager.Instance.OnStopGame.AddListener(OnStopGame);

        for (int i = 0; i < performerList.Count; i++)
        {
            performerList[i].OnStartPerforming.AddListener(OnStartPerforming);
            performerList[i].OnStopPerforming.AddListener(OnStopPerforming);
        }
    }
    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStartGame.RemoveListener(OnStartGame);
            GameManager.Instance.OnStopGame.RemoveListener(OnStopGame);
        }

        for (int i = 0; i < performerList.Count; i++)
        {
            performerList[i].OnStartPerforming.RemoveListener(OnStartPerforming);
            performerList[i].OnStopPerforming.RemoveListener(OnStopPerforming);
        }
    }

    void OnStartPerforming(int index, ulong cliend_id)
    {
        OnStartPerformingEvent?.Invoke(index, cliend_id);
    }

    void OnStopPerforming(int index, ulong cliend_id)
    {
        OnStopPerformingEvent?.Invoke(index, cliend_id);
    }


    #region Joining As Specific Role
    public void ApplyPerformer(Action<bool, string> callback)
    {
        OnReceiveRegistrationResultAction = callback;

        // As Network Object needs serveral frames to be intialized after the connection has been built,
        // we need to wait to call Rpcs until it's been spawned.
        StartCoroutine(WaitToApplyPerformer());
    }

    IEnumerator WaitToApplyPerformer()
    {
        float start_time = Time.fixedTime; ;
        while(Time.fixedTime - start_time < performerApplyTimeout && IsSpawned == false)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if(IsSpawned)
        {
            RegisterPerformerRpc();
        }
        else
        {
            OnReceiveRegistrationResult(false, "Performer Registration Times Out");
        }
    }    

    void EnterSoloMode()
    {
        for(int i=0; i<performerList.Count; i++)
        {
            performerList[i].isPerforming.Value = true;
            performerList[i].clientID.Value = (ulong)i;

            OnStartPerformingEvent?.Invoke(i, (ulong)i);
        }


        RefreshPlayerCount();
    }

    void StopSoloMode()
    {
        for (int i = 0; i < performerList.Count; i++)
        {
            performerList[i].isPerforming.Value = false;
            performerList[i].clientID.Value = 0;

            OnStopPerformingEvent?.Invoke(i, (ulong)i);
        }


        RefreshPlayerCount();
    }

    void EnterSinglePlayerMode()
    {
        ApplyPerformer((result, msg)=> {
            if(result)
            {
                performerList[1].isPerforming.Value = true;
                performerList[1].clientID.Value = (ulong)1;

                RefreshPlayerCount();
            }
        });
    }

    void ExitSinglePlayerMode()
    {
        for(int i = 0; i < performerList.Count; i++)
        {
            performerList[i].isPerforming.Value = false;
            performerList[i].clientID.Value = 0;

            OnStopPerformingEvent?.Invoke(i, (ulong)i);
        }
    }
    #endregion

    void OnStartGame(PlayerRole player_role)
    {
        playerRole = player_role;

        if(player_role == PlayerRole.Server && GameManager.Instance.IsSoloMode)
        {
            EnterSoloMode();
        }

        if(player_role == PlayerRole.SinglePlayer)
        {
            EnterSinglePlayerMode();
        }
    }

    void OnStopGame(PlayerRole player_role)
    {
        playerRole = PlayerRole.Undefined;

        if (player_role == PlayerRole.Server && GameManager.Instance.IsSoloMode)
        {
            StopSoloMode();
        }

        if (player_role == PlayerRole.SinglePlayer)
        {
            ExitSinglePlayerMode();
        }
    }


    #region RPCs
    [Rpc(SendTo.Server)]
    void RegisterPerformerRpc(RpcParams rpcParams = default)
    {
        if (!IsServer)
            return;

        int available_index = GetAvailablePerformerIndex();

        Debug.Log(string.Format("RegisterPerformerRpc | PerformerIndex:{0}, ClientID:{1}", available_index, rpcParams.Receive.SenderClientId));

        ReplyRegistrationResultRpc(available_index != -1 ? true:false, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        
        if (available_index != -1)
        {
            AddPerformerRpc(available_index, rpcParams.Receive.SenderClientId);

            RefreshPlayerCount();
        }
    }


    [Rpc(SendTo.SpecifiedInParams)]
    void ReplyRegistrationResultRpc(bool result, RpcParams rpcParams = default)
    {
        Debug.Log(string.Format("ReplyRegistrationResultRpc | ClientID:{0}, Result:{1}", NetworkManager.Singleton.LocalClientId, result));

        OnReceiveRegistrationResult(result, result ? "Succeed" : "Performers are full.");
    }

    void OnReceiveRegistrationResult(bool result, string msg = "")
    {
        Debug.Log(string.Format("OnReceiveRegistrationResult | ClientID:{0}, Result:{1}, Msg:{2}", NetworkManager.Singleton.LocalClientId, result, msg));

        OnReceiveRegistrationResultAction?.Invoke(result, msg);
        OnReceiveRegistrationResultAction = null;
    }

    [Rpc(SendTo.Everyone)]
    void AddPerformerRpc(int index, ulong client_id)
    {
        Debug.Log(string.Format("AddPerformerRpc | PerformerIndex:{0}, ClientID:{1}", index, client_id));
        if(IsServer)
        {
            performerList[index].isPerforming.Value = true;
            performerList[index].clientID.Value = client_id;
            performerList[index].GetComponent<NetworkObject>().ChangeOwnership(client_id);
        }

        if (NetworkManager.Singleton.LocalClientId == client_id)
        {
            performerSynchronizer.BindPerformerTransform(performerList[index].gameObject.transform);
            Debug.Log(string.Format("AddPerformerRpc | Bind Performer Transform:{0}", index));
        }

        OnAddPerformerEvent?.Invoke(index);
    }


    [Rpc(SendTo.Everyone)]
    void RemovePerformerRpc(int index, ulong client_id)
    {
        Debug.Log(string.Format("RemovePerformerRpc | PerformerIndex:{0}, ClientID:{1}", index, client_id));
        if (IsServer)
        {
            performerList[index].isPerforming.Value = false;
            performerList[index].clientID.Value = 0;
            performerList[index].GetComponent<NetworkObject>().RemoveOwnership();

            RefreshPlayerCount(client_id);
        }

        if (NetworkManager.Singleton.LocalClientId == client_id)
        {
            performerSynchronizer.UnbindPerformTransform(performerList[index].gameObject.transform);
            Debug.Log(string.Format("AddPerformerRpc | Unbind Performer Transform:{0}", index));
        }

        OnRemovePerformerEvent?.Invoke(index);
    }
    #endregion


    #region Connection Event Listener
    public void OnClientJoined(ulong client_id)
    {
        if (IsServer)
        {
            RefreshPlayerCount();
        }
    }

    public void OnClientLost(ulong client_id)
    {
        if (IsServer)
        {
            int performer_index = GetPerformerIndexByID(client_id);

            if (performer_index != -1)
            {
                RemovePerformerRpc(performer_index, client_id);
            }

            RefreshPlayerCount(client_id);
        }
    }
    #endregion


    #region Private Functions
    void RefreshPlayerCount(ulong just_deleted_id = ulong.MaxValue)
    {
        if (!IsServer)
            return;


        // Total Count
        // Has to deal with ConnectedClientsIds this way because the following issue 
        // https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/2927
        int total_count = 0;
        if (just_deleted_id == ulong.MaxValue)
        {
            total_count = NetworkManager.Singleton.ConnectedClients.Count;
        }
        else
        {
            for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
            {
                if (just_deleted_id != NetworkManager.Singleton.ConnectedClientsIds[i])
                    total_count++;
            }
        }


        // Performer Count
        int performer_count = 0;
        for(int i=0; i<performerList.Count; i++)
        {
            if(performerList[i].isPerforming.Value == true)
            {
                performer_count++;
            }
        }        

        performerCount = performer_count;
        audienceCount = total_count - performer_count;

        Debug.Log(string.Format("RefreshPlayerCount | Performer:{0}, Audience:{1}", performerCount, audienceCount));
    }


    void SetPlayerRole(PlayerRole role)
    {
        playerRole = role;
    }


    void InitializePerformerList()
    {
        for (int i = 0; i < performerTransformRoot.childCount; i++)
        {
            performerList.Add(performerTransformRoot.GetChild(i).GetComponent<Performer>());
        }
    }

    int GetAvailablePerformerIndex()
    {
        for (int i = 0; i < performerList.Count; i++)
        {
            if (performerList[i].isPerforming.Value == false)
            {
                return i;
            }
        }
        return -1;
    }

    int GetPerformerIndexByID(ulong client_id)
    {
        for (int i = 0; i < performerList.Count; i++)
        {
            if (performerList[i].isPerforming.Value == true && performerList[i].clientID.Value == client_id)
            {
                return i;
            }
        }
        return -1;
    }
    #endregion
}
