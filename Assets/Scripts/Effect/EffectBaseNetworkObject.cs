using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;


public class EffectParameter
{
    public NetworkVariable<float> networkValue;

    public bool needClamp = false;
    public float minValue = 0;
    public float maxValue = 1;
    

    public EffectParameter()
    {

    }
    public EffectParameter(NetworkVariable<float> _network_value)
    {
        networkValue = _network_value;
    }
    public EffectParameter(NetworkVariable<float> _network_value, bool need_clamp, float min_value, float max_value)
    {
        networkValue = _network_value;
        needClamp = need_clamp;
        minValue = min_value;
        maxValue = max_value;
    }
}

public class EffectBaseNetworkObject : NetworkBehaviour
{
    [SerializeField]
    protected string effectName;
    public string EffectName { get => effectName; }

    protected Dictionary<string, NetworkVariableBase> parameterList = new Dictionary<string, NetworkVariableBase>();

    protected Dictionary<string, EffectParameter> parameterList2 = new Dictionary<string, EffectParameter>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            BindOscAddress();

            RegisterOscReceiverFunction();
        }
    }


    protected virtual void BindOscAddress()
    {
        if (!IsServer) return;

        if (parameterList == null)
            parameterList = new Dictionary<string, NetworkVariableBase>();
    }

    protected virtual void RegisterOscReceiverFunction()
    {
        if (!IsServer) return;

        if (parameterList == null)
            return;


        foreach (var param in parameterList)
        {
            var type = (param.Value).GetType().GenericTypeArguments[0];

            if (type == typeof(float))
                ParameterReceiver.Instance.RegisterOscReceiverFunction(param.Key, new UnityAction<float>((v) => { (param.Value as NetworkVariable<float>).Value = v; }));
            //else if (type == typeof(Vector3))
            //    ParameterReceiver.Instance.RegisterOscReceiverFunction(param.Key, new UnityAction<Vector3>((v) => { (param.Value as NetworkVariable<Vector3>).Value = v; }));
            //else if (type == typeof(string))
            //    ParameterReceiver.Instance.RegisterOscReceiverFunction(param.Key, new UnityAction<string>((v) => { (param.Value as NetworkVariable<string>).Value = v; }));
        }

        foreach (var param in parameterList2)
        {
            ParameterReceiver.Instance.RegisterOscReceiverFunction(param.Key, new UnityAction<float>((v) => { (param.Value.networkValue as NetworkVariable<float>).Value = v; }));
        }
    }

}
