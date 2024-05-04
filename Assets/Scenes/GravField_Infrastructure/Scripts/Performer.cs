using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Performer : NetworkBehaviour
{
    public NetworkVariable<ulong> clientID;

    public NetworkVariable<bool> isPerforming;

}
