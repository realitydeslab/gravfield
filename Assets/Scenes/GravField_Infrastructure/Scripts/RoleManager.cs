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

    private PerformerSynchronizer performerSynchronizer;

    private List<Performer> performerList = new List<Performer>();

    public enum PlayerRole
    {
        Audience,
        Performer,
        Server
    }
    private PlayerRole playerRole = PlayerRole.Audience;
    private int audienceCount = 0;
    private int performerCount = 0;
    
    public UnityEvent<bool> OnReceiveRegistrationResultEvent;
    
    

    
    /// <summary>
    /// https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable/
    /// In-Scene Placed: Since the instantiation occurs via the scene loading mechanism(s), the Start method is invoked before OnNetworkSpawn.
    /// </summary>
    void Start()
    {
        performerSynchronizer = transform.GetComponent<PerformerSynchronizer>();
    }

    public override void OnNetworkSpawn()
    {
        InitializePerformerList();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

    }

    void OnDestroy()
    {
        NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        base.OnDestroy();
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log("OnClientConnectedCallback | ClientID:" + clientId);
        if (!IsServer)
            return;
        
        RefreshPlayerCount();

        Debug.Log("OnClientConnectedCallback | ClientID:" + clientId);
    }
    private void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log("OnClientDisconnectCallback | ClientID:" + clientId);
        if (!IsServer)
            return;

        for(int i=0; i<performerList.Count; i++)
        {
            if(performerList[i].clientID.Value == clientId)
            {
                RemovePerformerRpc(i, clientId);
            }
        }

        RefreshPlayerCount();

        Debug.Log("OnClientDisconnectCallback | ClientID:" + clientId);
    }

    void Update()
    {
        
    }

    #region Joining Functions
    public void JoinAsServer()
    {

    }

    public void JoinAsPerformer()
    {
        // As Network Object needs serveral frames to be intialized after the connection has been built,
        // we need to wait to call Rpcs until it's been spawned.
        StartCoroutine(WaitToApplyPerformer());
    }

    IEnumerator WaitToApplyPerformer()
    {
        float start_time = Time.fixedTime; ;
        while(Time.fixedTime - start_time < 5 && IsSpawned == false)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if(IsSpawned)
        {
            RegisterPerformerRpc();
        }
        else
        {
            GameManager.Instance.DisplayMessageOnUI("Performer Registration Time Out. Switch to audience instead.");
            OnReceiveRegistrationResult(false);
            
        }
    }

    public void JoinAsAudience()
    {
        // do nothing
    }
    #endregion


    #region RPCs
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rpcParams"></param>
    [Rpc(SendTo.Server)]
    void RegisterPerformerRpc(RpcParams rpcParams = default)
    {
        Debug.Log(string.Format("RegisterPerformerRpc In"));
        if (!IsServer)
            return;

        int available_index = GetAvailablePerformerIndex();

        ReplyRegistrationResultRpc(available_index != -1 ? true:false, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        
        if (available_index != -1)
        {
            AddPerformerRpc(available_index, rpcParams.Receive.SenderClientId);
        }

        Debug.Log(string.Format("RegisterPerformerRpc | Index:{0}, ClientID:{1}", available_index, rpcParams.Receive.SenderClientId));
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="result"></param>
    /// <param name="rpcParams"></param>
    [Rpc(SendTo.SpecifiedInParams)]
    void ReplyRegistrationResultRpc(bool result, RpcParams rpcParams = default)
    {
        OnReceiveRegistrationResult(result);
        Debug.Log(string.Format("OnGetRegistrationResultClientRpc | ClientID:{0}, result:{1}", NetworkManager.Singleton.LocalClientId, result));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="client_id"></param>
    [Rpc(SendTo.Everyone)]
    void AddPerformerRpc(int index, ulong client_id)
    {
        Debug.Log("AddPerformerRpc | index:" + index);
        if(IsServer)
        {
            performerList[index].isPerforming.Value = true;
            performerList[index].clientID.Value = client_id;
        }

        if (NetworkManager.Singleton.LocalClientId == client_id)
        {
            performerSynchronizer.BindPerformerTransform(performerList[index].transform);
            Debug.Log("AddPerformerRpc | Bind me");
        }

        RefreshPlayerCount();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="client_id"></param>
    [Rpc(SendTo.Everyone)]
    void RemovePerformerRpc(int index, ulong client_id)
    {
        Debug.Log("RemovePerformerRpc | index:" + index);
        if (IsServer)
        {
            performerList[index].isPerforming.Value = false;
            performerList[index].clientID.Value = 0;
        }

        if (NetworkManager.Singleton.LocalClientId == client_id)
        {
            performerSynchronizer.UnbindPerformTransform();
            Debug.Log("RemovePerformerRpc | Unbind me");
        }

        RefreshPlayerCount();
    }
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


    void OnReceiveRegistrationResult(bool result)
    {
        if (result == true)
        {
            SetPlayerRole(PlayerRole.Performer);
        }
        else
        {
            SetPlayerRole(PlayerRole.Audience);
            GameManager.Instance.DisplayMessageOnUI("Performers Are Full . Switch to audience instead.");
        }
        OnReceiveRegistrationResultEvent?.Invoke(result);
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
    #endregion
}
