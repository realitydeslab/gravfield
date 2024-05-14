using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PerformerGroup : NetworkBehaviour
{
    public NetworkVariable<float> floatValue1;

    public NetworkVariable<float> floatValue2;

    public NetworkVariable<float> floatValue3;

    public NetworkVariable<float> floatValue4;

    public NetworkVariable<float> floatValue5;

    public NetworkVariable<int> intValue1;

    public NetworkVariable<int> intValue2;

    public NetworkVariable<int> intValue3;

    public NetworkVariable<int> intValue4;

    public NetworkVariable<int> intValue5;

    public NetworkVariable<Vector3> vectorValue1;

    public NetworkVariable<Vector3> vectorValue2;

    public NetworkVariable<Vector3> vectorValue3;
}
