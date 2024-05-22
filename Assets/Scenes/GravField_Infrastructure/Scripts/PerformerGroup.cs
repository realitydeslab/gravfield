using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using SplineMesh;
using UnityEngine.Events;
using Xiaobo.Parameter;

public class PerformerGroup : NetworkBehaviour
{
    public NetworkVariable<float> meshy;

    public NetworkVariable<float> meshnoise;

    public NetworkVariable<float> meshsize;

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

    public Transform ropeTransformRoot;

    private List<Performer> performerList = new List<Performer>();

    public NetworkVariable<float> effectMode;

    public NetworkVariable<float> ropeMeshScale;

    void Awake()
    {
        for(int i=0; i<transform.childCount; i++)
        {
            performerList.Add(transform.GetChild(i).GetComponent<Performer>());
        }        
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            RegisterOscReceiverFunction();
        }
    }


    void Update()
    {
        if (IsSpawned == false)
            return;

        if(IsServer)
        {
            meshsize.Value = SmoothValue(meshsize.Value, 0, 0.2f);
        }
    }


    #region Parameters From Coda
    void RegisterOscReceiverFunction()
    {
        if (!IsServer) return;

        ParameterReceiver.Instance.RegisterOscReceiverFunction("/meshy", new UnityAction<float>(OnReceive_MeshY));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/meshnoise", new UnityAction<float>(OnReceive_MeshNoise));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/meshsize", new UnityAction<float>(OnReceive_MeshSize));
    }

    void OnReceive_MeshY(float v)
    {
        if (!IsServer)
            return;

        meshy.Value = SmoothValue(meshy.Value, v);
    }

    void OnReceive_MeshNoise(float v)
    {
        if (!IsServer)
            return;

        meshnoise.Value = SmoothValue(meshnoise.Value, v);
    }

    void OnReceive_MeshSize(float v)
    {
        if (!IsServer)
            return;

        meshsize.Value = v;
    }

    float SmoothValue(float cur, float dst, float t = 0.2f)
    {
        if (t == 0)
            return dst;

        float cur_vel = 0;
        return Mathf.SmoothDamp(cur, dst, ref cur_vel, t);
    }
    #endregion


}


