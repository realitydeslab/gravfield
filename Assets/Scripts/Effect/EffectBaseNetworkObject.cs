using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class EffectBaseNetworkObject : NetworkBehaviour
{
    [SerializeField]
    protected string effectName;
    public string EffectName { get => effectName; }

    protected Dictionary<string, NetworkVariableBase> parameterList = new Dictionary<string, NetworkVariableBase>();

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
            else if (type == typeof(Vector3))
                ParameterReceiver.Instance.RegisterOscReceiverFunction(param.Key, new UnityAction<Vector3>((v) => { (param.Value as NetworkVariable<Vector3>).Value = v; }));
            else if (type == typeof(string))
                ParameterReceiver.Instance.RegisterOscReceiverFunction(param.Key, new UnityAction<string>((v) => { (param.Value as NetworkVariable<string>).Value = v; }));
        }
    }

}
