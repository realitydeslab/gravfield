using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class EffectParameter<T>
{
    public string address;
    public T value;
    public NetworkVariable<T> networkValue;
}

public class EffectRopeNetworkObject : EffectBaseNetworkObject
{
    public NetworkVariable<Vector3> thickness;
    public NetworkVariable<float> mass;
    public NetworkVariable<float> maxWidth = new NetworkVariable<float>(40);
    public NetworkVariable<float> ropeScaler = new NetworkVariable<float>(5);
    public NetworkVariable<float> ropeOffsetY = new NetworkVariable<float>(-0.3f);
    public NetworkVariable<Vector3> ropeOffset = new NetworkVariable<Vector3>(new Vector3(0,-0.2f,0.3f));

    //public NetworkVariable<EffectParameter_Float> thickness2 = new NetworkVariable<EffectParameter_Float>(
    //    new EffectParameter_Float("/rope-thickness", 1), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //public NetworkVariable<EffectParameter_Float> mass2 = new NetworkVariable<EffectParameter_Float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //List<NetworkVariableBase> AllParametersNeedToBeRegistered;



    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            BindOscAddress();

            RegisterOscReceiverFunction();
        }
    }

    protected override void BindOscAddress()
    {
        if (!IsServer) return;

        //parameterList.Add("/rope-mass", mass);
        parameterList.Add("/rope-maxWidth", maxWidth);
        parameterList.Add("/rope-scaler", ropeScaler);
        parameterList.Add("/thickness", thickness);
        parameterList.Add("/rope-offset-y", ropeOffsetY);
    }

    

    // bind address with parameter

    // register

    // initialize

    // receive command

    // send out parameter?

    // drive effect

    // return to default
}
