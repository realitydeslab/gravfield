using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class PerformerLocalData
{
    public bool isPerforming;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
}

public class Performer : NetworkBehaviour
{
    public NetworkVariable<ulong> clientID;

    public NetworkVariable<bool> isPerforming;



    //public NetworkVariable<float> floatValue1;

    //public NetworkVariable<float> floatValue2;

    //public NetworkVariable<float> floatValue3;

    //public NetworkVariable<float> floatValue4;

    //public NetworkVariable<float> floatValue5;

    //public NetworkVariable<int> intValue1;

    //public NetworkVariable<int> intValue2;

    //public NetworkVariable<int> intValue3;

    //public NetworkVariable<int> intValue4;

    //public NetworkVariable<int> intValue5;

    //public NetworkVariable<Vector3> vectorValue1;

    //public NetworkVariable<Vector3> vectorValue2;

    //public NetworkVariable<Vector3> vectorValue3;

    public NetworkVariable<float> remoteThickness;
    AutoSwitchedParameter<float> localThickness = new AutoSwitchedParameter<float>(10);

    public NetworkVariable<float> remoteMass;
    public AutoSwitchedParameter<float> localMass = new AutoSwitchedParameter<float>(1);

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

        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("mass"), new UnityAction<float>(OnReceive_Mass));
        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("thickness"), new UnityAction<float>(OnReceive_Thickness));
    }

    string FormatedOscAddress(string param)
    {
        return "/" + performerName + "-" + param;
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
