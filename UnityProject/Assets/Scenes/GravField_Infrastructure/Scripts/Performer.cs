using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class PerformerLocalData
{
    public bool isPerforming = false;
    public Vector3 position = Vector3.zero;
    public Vector3 velocity = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
    public bool positive = false;
}

public class Performer : NetworkBehaviour
{
    public NetworkVariable<ulong> clientID;

    public NetworkVariable<bool> isPerforming;


    public NetworkVariable<float> remoteThickness;
    AutoSwitchedParameter<float> localThickness = new AutoSwitchedParameter<float>(10);

    public NetworkVariable<float> remoteMass;
    public AutoSwitchedParameter<float> localMass = new AutoSwitchedParameter<float>(1);

    public NetworkVariable<float> effectRope_mass;
    public NetworkVariable<float> effectRope_thickness;

    public NetworkVariable<float> magnetic;

    // Rope Effect
    public NetworkVariable<float> ropeMass = new NetworkVariable<float>(42.8f);
    public NetworkVariable<float> ropeMaxWidth = new NetworkVariable<float>(40);
    public NetworkVariable<float> ropeScaler = new NetworkVariable<float>(5);
    public NetworkVariable<float> ropeOffsetY = new NetworkVariable<float>(-0.3f);

    // Spring Effect
    public NetworkVariable<float> springFreq = new NetworkVariable<float>(30);
    public NetworkVariable<float> springWidth = new NetworkVariable<float>(1);

    // Magnetic Effect

    int performerIndex = 0;

    string performerName = "A";

    public PerformerLocalData localData = new PerformerLocalData();

    public UnityEvent<int, ulong> OnStartPerforming;

    public UnityEvent<int, ulong> OnStopPerforming;

    void Awake()
    {
        performerIndex = transform.GetSiblingIndex();
        performerName = performerIndex == 1 ? "B" : performerIndex == 2 ? "C" : "A";
    }
    
    public override void OnNetworkSpawn()
    {
        ResetLocalData();

        if(IsServer)
        {
            InitialLocalParameter();

            RegisterOscReceiverFunction();
        }
    }

    public void ResetLocalData()
    {
        localData.isPerforming = isPerforming.Value;
        localData.position = transform.localPosition;
        localData.velocity = Vector3.zero;
        localData.acceleration = Vector3.zero;
        localData.positive = magnetic.Value > 0.5 ? true : false;
    }

    void Update()
    {
        if (IsSpawned == false)
            return;

        // Just Start Performing
        if (isPerforming.Value == true && localData.isPerforming == false)
        {
            localData.position = transform.localPosition;
            localData.velocity = Vector3.zero;
            localData.acceleration = Vector3.zero;

            OnStartPerforming?.Invoke(performerIndex, clientID.Value);
        }
        // Just Stop Performing
        else if (isPerforming.Value == false && localData.isPerforming == true)
        {
            OnStopPerforming?.Invoke(performerIndex, clientID.Value);
        }
        localData.isPerforming = isPerforming.Value;
        if (localData.isPerforming == false)
            return;


        // Update local data
        Vector3 new_pos = transform.localPosition;
        Vector3 new_vel = (new_pos - localData.position) / Time.deltaTime;

        localData.acceleration = (new_vel - localData.velocity) / Time.deltaTime;
        localData.velocity = new_vel;
        localData.position = new_pos;
        localData.positive = magnetic.Value > 0.5 ? true : false;

        // 
        UpdateRemoteParameterWithLocal();
    }


    #region Paramters received from Coda / Example to use local/remote parameter pair
    void InitialLocalParameter()
    {
        if (!IsServer) return;

        //localThickness.OrginalValue = remoteThickness.Value;
        //localMass.OrginalValue = remoteMass.Value;
    }

    void UpdateRemoteParameterWithLocal()
    {
        if (!IsServer) return;

        //remoteMass.Value = localMass.Value;
        //remoteThickness.Value = localThickness.Value;
    }

    void RegisterOscReceiverFunction()
    {
        if (!IsServer) return;

        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("/rope-mass"), new UnityAction<float>((v) => { ropeMass.Value = v; }));
        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("/rope-maxwidth"), new UnityAction<float>((v) => { ropeMaxWidth.Value = v; }));
        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("/rope-scaler"), new UnityAction<float>((v) => { ropeScaler.Value = v; }));
        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("/rope-offset"), new UnityAction<float>((v) => { ropeOffsetY.Value = v; }));

        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("/spring-freq"), new UnityAction<float>((v) => { springFreq.Value = v; }));
        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("/spring-width"), new UnityAction<float>((v) => { springWidth.Value = v; }));

        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("/mag"), new UnityAction<float>((v) => { magnetic.Value = v; }));
    }

    string FormatedOscAddress(string param)
    {
        if (param[0] == '/')
        {
            param = param.Substring(1);
        }
        if(param.Contains("-"))
        {
            string[] split_str = param.Split("-");
            return "/" + split_str[0] + performerIndex.ToString() + "-" + split_str[1];
        }

        return "/" + param + performerIndex.ToString();
    }

    void OnReceive_Mass(float v)
    {
        v = Mathf.Clamp(v, 0.1f, 100);
        //localMass.CodaValue = SmoothValue(localMass.Value, v);
        remoteMass.Value = SmoothValue(remoteMass.Value, v);
    }

    void OnReceive_Thickness(float v)
    {
        v = Mathf.Clamp(v, 1f, 50);
        //localThickness.CodaValue = SmoothValue(localThickness.Value, v);
        remoteThickness.Value = SmoothValue(remoteThickness.Value, v);
    }

    float SmoothValue(float cur, float dst, float t = 0)
    {
        if (t == 0)
            return dst;
        float cur_vel = 0;
        return Mathf.SmoothDamp(cur, dst, ref cur_vel, t);
    }
    #endregion

    
}
