using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using SplineMesh;
using UnityEngine.Events;

public class PerformerGroup : NetworkBehaviour
{
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

    public Transform ropeTransformRoot;

    private List<Performer> performerList = new List<Performer>();

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

    void RegisterOscReceiverFunction()
    {
        if (!IsServer)
            return;

        ParameterReceiver.Instance.RegisterOscReceiverFunction("/meshy", new UnityAction<float>(OnReceive_MeshY));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/meshnoise", new UnityAction<float>(OnReceive_MeshNoise));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/meshsize", new UnityAction<float>(OnReceive_MeshSize));
    }

    string FormatedOscAddress(string param)
    {
        string performer_name = transform.GetSiblingIndex() == 1 ? "B" : (transform.GetSiblingIndex() == 2 ? "C" : "A");
        return "/" + performer_name + "-" + param;
    }

    void OnReceive_MeshY(float v)
    {
        if (!IsServer)
            return;

        floatValue1.Value = SmoothValue(floatValue1.Value, v);
    }

    void OnReceive_MeshNoise(float v)
    {
        if (!IsServer)
            return;

        floatValue2.Value = SmoothValue(floatValue2.Value, v);
    }

    void OnReceive_MeshSize(float v)
    {
        if (!IsServer)
            return;

        floatValue3.Value = v;
    }

    float SmoothValue(float cur, float dst, float t = 0.2f)
    {
        if (t == 0)
            return dst;

        float cur_vel = 0;
        return Mathf.SmoothDamp(cur, dst, ref cur_vel, t);
    }

    void Update()
    {
        if (IsSpawned == false)
            return;

        if(IsServer)
        {
            floatValue3.Value = SmoothValue(floatValue3.Value, 0, 0.2f);
        }

        // Update effects
        UpdateRopeEffect();

        
    }

    void UpdateRopeEffect()
    {
        for(int i=0; i<performerList.Count; i++)
        {
            if (performerList[i].isPerforming.Value == false)
                continue;
            for(int k=i+1; k<performerList.Count; k++)
            {
                if (performerList[k].isPerforming.Value == false)
                    continue;

                Performer start = performerList[i];
                Performer end = performerList[k];

                float mass_start = Mathf.Max(0.1f, start.floatValue1.Value);
                float mass_end = Mathf.Max(0.1f, end.floatValue1.Value);

                float drag_start = Mathf.Max(0.1f, start.floatValue2.Value);
                float drag_end = Mathf.Max(0.1f, end.floatValue2.Value);

                float thickness_start = Mathf.Max(0.1f, start.floatValue3.Value);
                float thickness_end = Mathf.Max(0.1f, end.floatValue3.Value);

                Transform rope_transform = ropeTransformRoot.GetChild(i + k - 1);
                Transform segment_root = rope_transform.Find("Segments");
                for(int m=0; m< segment_root.childCount; m++)
                {
                    Rigidbody rigid = segment_root.GetChild(m).GetComponent<Rigidbody>();
                    rigid.mass = Mathf.Lerp(mass_start, mass_end, m / segment_root.childCount-1);
                    rigid.drag = Mathf.Lerp(drag_start, drag_end, m / segment_root.childCount - 1);
                }


                Spline spline = rope_transform.GetComponent<Spline>();
                float currentLength = 0;
                foreach (CubicBezierCurve curve in spline.GetCurves())
                {
                    float startRate = currentLength / spline.Length;
                    currentLength += curve.Length;
                    float endRate = currentLength / spline.Length;

                    curve.n1.Scale = Vector3.one * (thickness_start + (thickness_end - thickness_start) * startRate);
                    curve.n2.Scale = Vector3.one * (thickness_start + (thickness_end - thickness_start) * endRate);
                }
            }
        }
    }

    


}


