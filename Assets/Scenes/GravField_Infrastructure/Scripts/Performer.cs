using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Performer : NetworkBehaviour
{
    public Transform transform;
    public int performerID;
    public NetworkVariable<ulong> clientID;

    public NetworkVariable<bool> isPerforming;

    void Awake()
    {
        //isPerforming = new NetworkVariable<bool>();
        //isPerforming.Value = false;
    }

}
