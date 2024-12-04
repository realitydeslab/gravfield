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
}

public class Performer : NetworkBehaviour
{
    public NetworkVariable<ulong> clientID;

    public NetworkVariable<bool> isPerforming;

    public NetworkVariable<float> soundVolume = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<float> soundPitch = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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

        // Use this way to find out when Performer is ready to perform on each client
        // Then, can use OnStartPerforming to trigger effects
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


        if(IsOwner)
        {
            soundVolume.Value = GameManager.Instance.AudioProcessor.SmoothedAudioVolume;
            soundPitch.Value = GameManager.Instance.AudioProcessor.AudioPitch;
        }
    }


    #region Paramters received from Coda / Example to use local/remote parameter pair


    float SmoothValue(float cur, float dst, float t = 0)
    {
        if (t == 0)
            return dst;
        float cur_vel = 0;
        return Mathf.SmoothDamp(cur, dst, ref cur_vel, t);
    }
    #endregion

    
}
