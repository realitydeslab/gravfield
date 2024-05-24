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
    public AutoSwitchedParameter<float> mode0 = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> mode1 = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> mode2 = new AutoSwitchedParameter<float>();


    public NetworkVariable<float> ropeMeshScale;

    public UnityEvent OnPerformerFinishSpawn;
    bool performerFinishSpawn = false;

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

            RegisterPropertiesToLive_Server();
        }

    }


    void Update()
    {
        if (IsSpawned == false)
            return;


        if(performerFinishSpawn == false)
        {
            bool cur_spawned_state = true;
            for (int i = 0; i < performerList.Count; i++)
            {
                if (performerList[i].IsSpawned == false)
                {
                    cur_spawned_state = false;
                    break;
                }
            }
            if (cur_spawned_state == true)
            {
                OnPerformerFinishSpawn?.Invoke();

                Debug.Log("OnPerformerFinishSpawn");
            }
            performerFinishSpawn = true;
        }


        if(IsServer)
        {
            meshsize.Value = SmoothValue(meshsize.Value, 0, 0.2f);

            mode0.OrginalValue = effectMode.Value == 0 ? 1 : 0;
            mode1.OrginalValue = effectMode.Value == 1 ? 1 : 0;
            mode2.OrginalValue = effectMode.Value == 2 ? 1 : 0;
        }

        if(IsServer)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    OnReceive_Mode(i);
                }
            }
        }
    }


    #region Parameters From Coda
    void RegisterOscReceiverFunction()
    {
        if (!IsServer) return;

        ParameterReceiver.Instance.RegisterOscReceiverFunction("/mode", new UnityAction<float>(OnReceive_Mode));

        ParameterReceiver.Instance.RegisterOscReceiverFunction("/meshy", new UnityAction<float>(OnReceive_MeshY));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/meshnoise", new UnityAction<float>(OnReceive_MeshNoise));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/meshsize", new UnityAction<float>(OnReceive_MeshSize));
    }

    void OnReceive_Mode(float v)
    {
        if (!IsServer)
            return;

        effectMode.Value = v;
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

    #region
    void RegisterPropertiesToLive_Server()
    {
        SenderForLive.Instance.RegisterOscPropertyToSend("/mode0", mode0);
        SenderForLive.Instance.RegisterOscPropertyToSend("/mode1", mode1);
        SenderForLive.Instance.RegisterOscPropertyToSend("/mode2", mode2);
    }
    #endregion

}


