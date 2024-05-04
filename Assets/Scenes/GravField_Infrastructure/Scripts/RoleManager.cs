using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


public class RoleManager : NetworkBehaviour
{
    public GameObject goServer;
    public GameObject goPerformer;
    public GameObject goAudience;
    const int PERFORMER_COUNT = 3;
    Performer[] performerList = new Performer[PERFORMER_COUNT];
    UnityEvent onReceiveApplicationResult;


    /// <summary>
    /// https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable/
    /// In-Scene Placed: Since the instantiation occurs via the scene loading mechanism(s), the Start method is invoked before OnNetworkSpawn.
    /// </summary>
    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        StorePerformerToList();
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
        // Apply to be a performer
        RegisterPerformerRPC();
    }
    public void JoinAsAudience()
    {
        // do nothing
    }

    void StorePerformerToList()
    {
        NetworkObject player_object = NetworkManager.LocalClient.PlayerObject;
        Debug.Log("StorePerformerToList: PlayerCount:" + player_object.transform.childCount);
        for (int i = 0; i < player_object.transform.childCount; i++)
        {
            performerList[i] = player_object.transform.GetChild(i).GetComponent<Performer>();
        }
    }

    [Rpc(SendTo.Server)]
    public void RegisterPerformerRPC(RpcParams rpcParams = default)
    {
        if (!IsServer)
            return;

        int available_index = GetAvailableIndex();
        OnGetRegistrationResultRPC(available_index, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        Debug.Log(string.Format("RegisterPerformerRPC | ClientID:{0}, Index:{1}", rpcParams.Receive.SenderClientId, available_index));
        if (available_index != -1)
        {
            AddNewPerformerRPC(available_index);
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void OnGetRegistrationResultRPC(int available_index, RpcParams rpcParams = default)
    {
        Debug.Log(string.Format("OnGetRegistrationResultRPC | ClientID:{0}, Index:{1}", NetworkManager.Singleton.LocalClientId, available_index));
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void AddNewPerformerRPC(int index)
    {
        Debug.Log("AddNewPerformerRPC | index:" + index);
        NetworkObject player_object = NetworkManager.LocalClient.PlayerObject;
        player_object.transform.GetChild(index).GetComponent<Performer>().isPerforming.Value = true;
    }

    int GetAvailableIndex()
    {
        NetworkObject player_object = NetworkManager.LocalClient.PlayerObject;
        for (int i = 0; i < player_object.transform.childCount; i++)
        {
            if (player_object.transform.GetChild(i).GetComponent<Performer>().isPerforming.Value == false)
            {
                return i;
            }
        }
        return -1;
    }
}
