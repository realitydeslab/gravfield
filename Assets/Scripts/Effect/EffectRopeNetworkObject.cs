using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;



public class EffectRopeNetworkObject : NetworkBehaviour
{
    public NetworkVariable<float> mass = new NetworkVariable<float>(42.8f);
    public NetworkVariable<float> maxWidth = new NetworkVariable<float>(40);
    public NetworkVariable<float> ropeScaler = new NetworkVariable<float>(5);
    public NetworkVariable<float> ropeOffsetY = new NetworkVariable<float>(-0.3f);
    public NetworkVariable<float> ropeOffsetZ = new NetworkVariable<float>(0.3f);

    //public NetworkVariable<EffectParameter_Float> thickness2 = new NetworkVariable<EffectParameter_Float>(
    //    new EffectParameter_Float("/rope-thickness", 1), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //public NetworkVariable<EffectParameter_Float> mass2 = new NetworkVariable<EffectParameter_Float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //List<NetworkVariableBase> AllParametersNeedToBeRegistered; 



    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            RegisterOscReceiverFunction();
        }
    }

    void RegisterOscReceiverFunction()
    {
        if (!IsServer) return;

        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-mass", mass, need_clamp: true, min_value: 10, max_value: 80);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-maxWidth", maxWidth, need_clamp: true, min_value: 1, max_value: 100);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-scaler", ropeScaler, need_clamp: true, min_value: 1, max_value: 20);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-offset-y", ropeOffsetY);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-offset-z", ropeOffsetZ);
    }

    

    // bind address with parameter

    // register

    // initialize

    // receive command

    // send out parameter?

    // drive effect

    // return to default
}
