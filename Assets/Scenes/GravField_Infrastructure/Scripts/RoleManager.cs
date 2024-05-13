using System;
using System.Collections;
using System.Collections.Generic;
using HoloKit.ImageTrackingRelocalization.iOS;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


public class RoleManager : NetworkBehaviour
{
    
    [SerializeField]
    private Transform performerTransformRoot;
    public Transform PerformerTransformRoot { get => performerTransformRoot; }

    [SerializeField]
    private float performerApplyTimeout = 10;

    private PerformerSynchronizer performerSynchronizer;

    private List<Performer> performerList = new List<Performer>();

    public enum PlayerRole
    {
        Undefined,
        Audience,
        Performer,
        Server
    }
    private PlayerRole playerRole = PlayerRole.Audience;
    private int audienceCount = 0;
    private int performerCount = 0;
    
    public UnityEvent<bool, string> OnReceiveRegistrationResultEvent;
    public Action<bool, string> OnReceiveRegistrationResultAction;
    public UnityEvent<bool> OnReceiveConnectionResultEvent;
    public UnityEvent<PlayerRole> OnSpecifyPlayerRoleEvent;

    public UnityEvent<int> OnAddPerformerEvent;
    public UnityEvent<int> OnRemovePerformerEvent;


    /// <summary>
    /// https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable/
    /// In-Scene Placed: Since the instantiation occurs via the scene loading mechanism(s), the Start method is invoked before OnNetworkSpawn.
    /// </summary>
    void Awake()
    {
        performerSynchronizer = transform.GetComponent<PerformerSynchronizer>();
        InitializePerformerList();
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

    public void JoinAsAudience()
    {
        SetPlayerRole(PlayerRole.Audience);
    }

    public void JoinAsPerformer()
    {
        SetPlayerRole(PlayerRole.Performer);
    }

    public void JoinAsServer()
    {
        SetPlayerRole(PlayerRole.Server);
    }

    public PlayerRole GetPlayerRole()
    {
        return playerRole;
    }

    public void ResetPlayerRole()
    {
        SetPlayerRole(PlayerRole.Undefined);
    }

    public void EnterSoloMode()
    {
        for(int i=0; i<performerList.Count; i++)
        {
            performerList[i].isPerforming.Value = true;
            performerList[i].clientID.Value = (ulong)i;
        }
    }
    #endregion


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
        }

        if (NetworkManager.Singleton.LocalClientId == client_id)
        {
            performerSynchronizer.UnbindPerformTransform();
            Debug.Log(string.Format("AddPerformerRpc | Unbind Performer Transform:{0}", index));
        }

        OnRemovePerformerEvent?.Invoke(index);
    }
    #endregion


    #region Connection Event Listener
    //public override void OnNetworkSpawn()
    //{
    //    NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
    //    NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

    //}
    //public override void OnNetworkDespawn()
    //{
    //    NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
    //    NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    //}

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

            RefreshPlayerCount();
        }
    }

    //private void OnClientConnectedCallback(ulong client_id)
    //{
    //    Debug.Log(string.Format("OnClientConnectedCallback | IsServer:{0}, ClientID:{1}", IsServer, client_id));

    //    if(IsServer)
    //    {
    //        RefreshPlayerCount();
    //    }
    //    else
    //    {
    //        OnReceiveConnectionResultEvent?.Invoke(true);
    //    }
    //}

    //private void OnClientDisconnectCallback(ulong client_id)
    //{
    //    Debug.Log(string.Format("OnClientDisconnectCallback | IsServer:{0}, ClientID:{1}", IsServer, client_id));
    //    if(IsServer)
    //    {
    //        // Unbind Performer if needed
    //        int performer_index = GetPerformerIndexByID(client_id);

    //        if (performer_index != -1)
    //        {
    //            RemovePerformerRpc(performer_index, client_id);
    //        }

    //        RefreshPlayerCount();
    //    }
    //    else
    //    {
    //        OnReceiveConnectionResultEvent?.Invoke(false);
    //    }
    //}
    #endregion


    #region Private Functions
    void RefreshPlayerCount()
    {
        if (!IsServer)
            return;

        int total_count = NetworkManager.Singleton.ConnectedClients.Count;
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

        Debug.Log(string.Format("Performer:{0}, Audience:{1}", performerCount, audienceCount));
    }


    


    void SetPlayerRole(PlayerRole role)
    {
        playerRole = role;

        OnSpecifyPlayerRoleEvent?.Invoke(role);

        Debug.Log("Send Event OnSpecifyPlayerRole: " + role.ToString());
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
