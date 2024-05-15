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


    public PerformerLocalData localData = new PerformerLocalData();
    

    public override void OnNetworkSpawn()
    {
        ResetLocalData();

        if(IsServer)
        {
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
        }
        // Just Stop Performing
        else if (isPerforming.Value == false && localData.isPerforming == true)
        {

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
    }

    void RegisterOscReceiverFunction()
    {
        if (!IsServer)
            return;

        
        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("mass"), new UnityAction<float>(OnReceive_Mass));
        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("drag"), new UnityAction<float>(OnReceive_Drag));
        ParameterReceiver.Instance.RegisterOscReceiverFunction(FormatedOscAddress("thickness"), new UnityAction<float>(OnReceive_Thickness));
    }

    string FormatedOscAddress(string param)
    {
        string performer_name = transform.GetSiblingIndex() == 1 ? "B" : (transform.GetSiblingIndex() == 2 ? "C" : "A");
        return "/" + performer_name + "-" + param;
    }

    void OnReceive_Mass(float v)
    {
        if (!IsServer)
            return;

        
        floatValue1.Value = SmoothValue(floatValue1.Value, v);
    }

    void OnReceive_Drag(float v)
    {
        if (!IsServer)
            return;

        floatValue2.Value = SmoothValue(floatValue2.Value, v);
    }

    void OnReceive_Thickness(float v)
    {
        if (!IsServer)
            return;

        floatValue3.Value = SmoothValue(floatValue3.Value, v);
    }

    float SmoothValue(float cur, float dst, float t = 0)
    {
        float cur_vel = 0;
        return Mathf.SmoothDamp(cur, dst, ref cur_vel, t);
    }
}
