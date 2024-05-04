using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


public class RoleManager : NetworkBehaviour
{
    public Transform transPerformerRoot;
    public GameObject goServer;
    public GameObject goPerformer;
    public GameObject goAudience;
    const int PERFORMER_COUNT = 3;
    Performer[] performerList = new Performer[PERFORMER_COUNT];
    UnityEvent<bool> onReceiveApplicationResult;
    public bool isPerformer = false;


    /// <summary>
    /// https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable/
    /// In-Scene Placed: Since the instantiation occurs via the scene loading mechanism(s), the Start method is invoked before OnNetworkSpawn.
    /// </summary>
    void Start()
    {
        StorePerformerToList();
    }

    public override void OnNetworkSpawn()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Check connection. If performer losts, delete it
        }
    }

    public void JoinAsServer()
    {

    }

    public void JoinAsPerformer()
    {
        StartCoroutine(WaitToApplyPerformer());
    }
    IEnumerator WaitToApplyPerformer()
    {
        float elapsed_time = 0;
        while(elapsed_time < 5 && IsSpawned == false)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if(IsSpawned)
        {
            RegisterPerformerServerRpc();
        }
        else
        {
            Debug.Log("Apply Performer: Time Out. Be an audience instead");
            onReceiveApplicationResult?.Invoke(false);
            JoinAsAudience();
        }
    }

    public void JoinAsAudience()
    {
        // do nothing
    }

    void StorePerformerToList()
    {
        //NetworkObject player_object = NetworkManager.LocalClient.PlayerObject;
        //Debug.Log("StorePerformerToList: PlayerCount:" + player_object.transform.childCount);
        //for (int i = 0; i < player_object.transform.childCount; i++)
        //{
        //    performerList[i] = player_object.transform.GetChild(i).GetComponent<Performer>();
        //}

        Debug.Log("StorePerformerToList: PlayerCount:" + transPerformerRoot.childCount);
        for (int i = 0; i < transPerformerRoot.childCount; i++)
        {
            performerList[i] = transPerformerRoot.GetChild(i).GetComponent<Performer>();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TestServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("TestServerRpc | " + IsSpawned);
        if (!IsServer)
            return;

        int available_index = GetAvailableIndex();
        Debug.Log(string.Format("TestServerRpc | ClientID:{0}, Index:{1}", rpcParams.Receive.SenderClientId, available_index));
        
    }

    [Rpc(SendTo.Server)]
    void RegisterPerformerServerRpc(RpcParams rpcParams = default)
    {
        
        if (!IsServer)
            return;

        int available_index = GetAvailableIndex();
        OnGetRegistrationResultClientRpc(available_index, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        Debug.Log(string.Format("RegisterPerformerServerRpc | ClientID:{0}, Index:{1}", rpcParams.Receive.SenderClientId, available_index));
        if (available_index != -1)
        {
            AddNewPerformerRpc(available_index);
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void OnGetRegistrationResultClientRpc(int available_index, RpcParams rpcParams = default)
    {
        Debug.Log(string.Format("OnGetRegistrationResultClientRpc | ClientID:{0}, Index:{1}", NetworkManager.Singleton.LocalClientId, available_index));
    }

    [Rpc(SendTo.ClientsAndHost)]
    void AddNewPerformerRpc(int index)
    {
        Debug.Log("AddNewPerformerRpc | index:" + index);
        NetworkObject player_object = NetworkManager.LocalClient.PlayerObject;
        player_object.transform.GetChild(index).GetComponent<Performer>().isPerforming.Value = true;
    }

    int GetAvailableIndex()
    {
        //NetworkObject player_object = NetworkManager.LocalClient.PlayerObject;
        //for (int i = 0; i < player_object.transform.childCount; i++)
        //{
        //    if (player_object.transform.GetChild(i).GetComponent<Performer>().isPerforming.Value == false)
        //    {
        //        return i;
        //    }
        //}
        //return -1;

        // NetworkObject player_object = NetworkManager.LocalClient.PlayerObject;
        Debug.Log("PerformList:" + performerList);
        Debug.Log("PerformListCount:" + performerList.Length);
        Debug.Log("PerformList[0]:" + performerList[0].isPerforming);
        for (int i = 0; i < performerList.Length; i++)
        {
            if (performerList[i].isPerforming.Value == false)
            {
                return i;
            }
        }
        return -1;
    }
}
