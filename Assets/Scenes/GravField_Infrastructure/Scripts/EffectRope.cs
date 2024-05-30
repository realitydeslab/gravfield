using System.Collections;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;
using SplineMesh;
using Unity.Netcode;
using UnityEngine.VFX;

public class EffectRope : MonoBehaviour
{
    // Basic
    public Performer performerStart;
    public Performer performerEnd;
    Transform ropeStart;
    Transform ropeEnd; 
    public Vector3 ropeOffset;
    int ropeIndex;
    bool ropeEnabled = false;

    // Path
    Spline spline;
    List<GameObject> wayPoints = new List<GameObject>();

    // VFX
    VisualEffect vfx;
    List<Vector3> ropePointList = new List<Vector3>();
    private const int BUFFER_STRIDE = 12; // 12 Bytes for a vector3
    private const int bufferInitialCapacity = 100;
    private bool dynamicallyResizeBuffer = false;
    private GraphicsBuffer bufferRopePoints;

    // Output to LIVE
    Transform centroidTransform;
    private Vector3 centroidPos;
    public Vector3 CentroidPos { get => centroidPos; }
    private Vector3 centroidVel;
    public Vector3 CentroidVel { get => centroidVel; }
    private Vector3 centroidAcc;
    public Vector3 CentroidAcc { get => centroidAcc; }
    AutoSwitchedParameter<float> ropevel = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> ropeacc = new AutoSwitchedParameter<float>();

    // Parameters    
    //float ropeMeshScale;
    //float startThickness;
    //float endThickness;
    //float startMass;
    //float endMass;
    float ropeMass = 42.8f;

    float cornerThickness = 2;
    float centerThickness = 40;
    float offsetMultiplier = 5;

    bool isInitialized = false;

    void Awake()
    {
        ropeIndex = transform.GetSiblingIndex();

        ropeStart = transform.Find("Anchors").GetChild(0);
        ropeEnd = transform.Find("Anchors").GetChild(1);

        vfx = GetComponent<VisualEffect>();
        EnsureBufferCapacity(ref bufferRopePoints, bufferInitialCapacity, BUFFER_STRIDE);
        int VertexBufferPropertyID = Shader.PropertyToID("RopePointBuffer");
        vfx.SetGraphicsBuffer(VertexBufferPropertyID, bufferRopePoints);
    }

    void OnPerformerFinishSpawn()
    {
        InitializeLocalVariable();

        RegisterNetworkVariableCallback_Client();

        RegisterPropertiesToLive_Server();
    }

    void Start()
    {
        spline = GetComponent<Spline>();

        AssignWayPoints();

        AssignSplineNodes();
    }

    void Update()
    {
        if(isInitialized == false && GameManager.Instance.PerformerGroup.PerformerFinishSpawn == true)
        {
            OnPerformerFinishSpawn();
            isInitialized = true;
        }


        if (ropeEnabled == false) return;

        UpdateRopeAnchors();

        UpdateNodes();

        

        UpdateRopeEffect();

        UpdateVFX();


        UpdateParamtersForLive();
    }

    void UpdateRopeAnchors()
    {
        //Vector3 nor = (performerEnd.position - performerStart.position).normalized;
        //ropeStart.localPosition = performerStart.position + nor* 0.3f;
        //ropeEnd.localPosition = performerEnd.position -nor*0.3f;


        ropeStart.localPosition = ApplyOffset(performerStart.transform);
        ropeEnd.localPosition = ApplyOffset(performerEnd.transform);


        //ropeStart.localRotation = performerStart.transform.localRotation;
        //ropeEnd.localRotation = performerEnd.transform.localRotation;
    }

    Vector3 ApplyOffset(Transform trans)
    {
        float angle_x = Mathf.Abs(trans.localRotation.eulerAngles.x);
        angle_x = angle_x > 180 ? 360 - angle_x : angle_x;

        float offset_multipler = Utilities.Remap(angle_x, 0, 90, 1f, Mathf.Max(1, offsetMultiplier), true);
        return trans.TransformPoint(Vector3.Scale(ropeOffset, Vector3.forward * offset_multipler));
    }

    void UpdateNodes()
    {
        int i = 0;
        foreach (GameObject wayPoint in wayPoints)
        {
            var node = spline.nodes[i++];
            //if (Vector3.Distance(node.Position, transform.InverseTransformPoint(wayPoint.transform.position)) > 0.001f)
            //{
            //    node.Position = transform.InverseTransformPoint(wayPoint.transform.position);
            //    //node.Up = wayPoint.transform.up;
            //}
            node.Position = wayPoint.transform.position;
            //node.Up = wayPoint.transform.up;
        }
    }

    void UpdateParamtersForLive()
    {
        Vector3 last_pos = centroidPos;
        centroidPos = centroidTransform.localPosition;
        Vector3 last_vel = centroidVel;
        centroidVel = (centroidPos - last_pos) / Time.deltaTime;
        centroidAcc = (centroidVel- last_vel) / Time.deltaTime;

        float vel =0;
        ropevel.OrginalValue = Mathf.SmoothDamp(ropevel.Value, centroidVel.magnitude, ref vel, 0.1f);
        ropeacc.OrginalValue = Mathf.SmoothDamp(ropeacc.Value, centroidAcc.magnitude, ref vel, 0.1f);
    }

    void UpdateVFX()
    {
        //int sample_count = bufferInitialCapacity;
        //if(ropePointList.Count < sample_count)
        //{
        //    ropePointList = new List<Vector3>(sample_count);
        //}

        ropePointList.Clear();
        float start_percentage = 0.1f;
        float end_percentage = 0.9f;
        for(int i=0; i< bufferInitialCapacity; i++)
        {
            try
            {
                CurveSample point = spline.GetSampleAtDistance(spline.Length * Utilities.Remap(i,  0, (bufferInitialCapacity - 1), start_percentage, end_percentage));
                ropePointList.Add(point.location);
            }
            catch
            {

            }
        }
        

        // Set Buffer data, but before that ensure there is enough capacity
        // We use Audio Raw Data instead of FFT
        EnsureBufferCapacity(ref bufferRopePoints, ropePointList.Count, BUFFER_STRIDE);
        bufferRopePoints.SetData(ropePointList);

        

        vfx.SetVector3("RopeCenterPos", centroidPos);
        vfx.SetVector3("RopeCenterVel", centroidVel);
        vfx.SetVector3("RopeCenterRight", centroidTransform.right);
    }

    void UpdateRopeEffect()
    {
        float min_thickness = Utilities.Remap(spline.Length, 0, 10, cornerThickness, 1, true);
        float max_thickness = Utilities.Remap(ropevel.Value, 0, 20, min_thickness, centerThickness);

        float currentLength = 0;
        foreach (CubicBezierCurve curve in spline.GetCurves())
        {
            float start_percentage = 1 - Mathf.Abs((currentLength / spline.Length - 0.5f) * 2);
            currentLength += curve.Length;
            float end_percentage = 1 - Mathf.Abs((currentLength / spline.Length - 0.5f) * 2);

            curve.n1.Scale = Vector3.one * (min_thickness + (max_thickness - min_thickness) * start_percentage);
            curve.n2.Scale = Vector3.one * (min_thickness + (max_thickness - min_thickness) * end_percentage);
        }
    }

    public void SetRopeState(bool state)
    {
        ropeEnabled = state;
        Transform mesh_transform = transform.Find("generated by SplineMeshTiling");
        mesh_transform?.gameObject.SetActive(state);

        vfx.enabled = state;
    }



    #region NetworkVariable
    void InitializeLocalVariable()
    {
        //ropeMeshScale = GameManager.Instance.PerformerGroup.ropeMeshScale.Value;
        //startThickness = performerStart.remoteThickness.Value;
        //endThickness = performerEnd.remoteThickness.Value;
        //startMass = performerStart.remoteMass.Value;
        //endMass = performerEnd.remoteMass.Value;

        ropeMass =  GameManager.Instance.PerformerList[ropeIndex].ropeMass.Value;
        centerThickness = GameManager.Instance.PerformerList[ropeIndex].ropeMaxWidth.Value;
        offsetMultiplier = GameManager.Instance.PerformerList[ropeIndex].ropeScaler.Value;
    }

    void RegisterNetworkVariableCallback_Client()
    {
        //performerStart.remoteThickness.OnValueChanged += (float prev, float cur) => { startThickness = cur; UpdateRopeThickness(); };
        //performerEnd.remoteThickness.OnValueChanged += (float prev, float cur) => { endThickness = cur; UpdateRopeThickness(); };

        //performerStart.remoteMass.OnValueChanged += (float prev, float cur) => { startMass = cur; UpdateRopeMass(); };
        //performerEnd.remoteMass.OnValueChanged += (float prev, float cur) => { endMass = cur; UpdateRopeMass(); };

        //GameManager.Instance.PerformerGroup.ropeMeshScale.OnValueChanged += (float prev, float cur) => { ropeMeshScale = cur; UpdateRopeMeshScale(); };

        GameManager.Instance.PerformerList[ropeIndex].ropeMass.OnValueChanged += (float prev, float cur) => { ropeMass = Mathf.Clamp(cur, 10, 80); UpdateRopeMass();  };
        GameManager.Instance.PerformerList[ropeIndex].ropeMaxWidth.OnValueChanged += (float prev, float cur) => { centerThickness = Mathf.Clamp(cur, 1, 100);  };
        GameManager.Instance.PerformerList[ropeIndex].ropeScaler.OnValueChanged += (float prev, float cur) => { offsetMultiplier = Mathf.Clamp(cur, 1, 20); };
    }    

    void UpdateRopeMass()
    {
        Transform segment_root = transform.Find("Segments");
        for (int m = 0; m < segment_root.childCount; m++)
        {
            Rigidbody rigid = segment_root.GetChild(m).GetComponent<Rigidbody>();
            rigid.mass = ropeMass; //Mathf.Lerp(startMass, endMass, m / segment_root.childCount - 1);
        }
    }

    //void UpdateRopeThickness()
    //{
    //    float currentLength = 0;
    //    foreach (CubicBezierCurve curve in spline.GetCurves())
    //    {
    //        float startRate = currentLength / spline.Length;
    //        currentLength += curve.Length;
    //        float endRate = currentLength / spline.Length;

    //        curve.n1.Scale = Vector3.one * (startThickness + (endThickness - startThickness) * startRate);
    //        curve.n2.Scale = Vector3.one * (startThickness + (endThickness - startThickness) * endRate);
    //    }
    //}

    //void UpdateRopeMeshScale()
    //{
    //    SplineMeshTiling meshTiling = GetComponent<SplineMeshTiling>();
    //    meshTiling.scale = Mathf.Max(0.05f, ropeMeshScale) * Vector3.one;
    //}
    #endregion

    #region Parameter sent to Live
    void RegisterPropertiesToLive_Server()
    {
        if (NetworkManager.Singleton.IsServer == false) return;

        SenderForLive.Instance.RegisterOscPropertyToSend(FormatedOscAddressToLive("vel"), ropevel);
        SenderForLive.Instance.RegisterOscPropertyToSend(FormatedOscAddressToLive("acc"), ropeacc);
    }

    string FormatedOscAddressToLive(string param)
    {
        return "/rope" + ropeIndex.ToString() + param;
    }
    #endregion

    #region Paramters received from Coda

    #endregion


    private void EnsureBufferCapacity(ref GraphicsBuffer buffer, int capacity, int stride)
    {
        // Reallocate new buffer only when null or capacity is not sufficient
        if (buffer == null || (dynamicallyResizeBuffer && buffer.count < capacity)) // remove dynamic allocating function
        {
            Debug.Log("Graphic Buffer reallocated!");
            // Buffer memory must be released
            buffer?.Release();
            // Vfx Graph uses structured buffer
            buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, stride);
        }
    }
    private void ReleaseBuffer(ref GraphicsBuffer buffer)
    {
        // Buffer memory must be released
        buffer?.Release();
        buffer = null;
    }
    void OnDestroy()
    {
        ReleaseBuffer(ref bufferRopePoints);
    }

    public void BindPerformer(Performer performer_start, Performer performer_end)
    {
        performerStart = performer_start;
        performerEnd = performer_end;
    }

    public void SetPerformerOffset(Vector3 offset)
    {
        ropeOffset = offset;
    }

    //public void BindRopeAnchors(Transform performer_start, Transform performer_end)
    //{
    //    ropeStart = performer_start;
    //}

    //public void SetThickness(float start_thickness, float end_thickness)
    //{
    //    startThickness = start_thickness;
    //    endThickness = end_thickness;
    //}






    void AssignWayPoints()
    {
        wayPoints.Clear();
        //Transform anchor_root = transform.Find("Anchors");
        //wayPoints.Add(anchor_root.GetChild(0).gameObject);

        Transform joint_root = transform.Find("Joints");
        for (int i = 0; i < joint_root.childCount; i++)
        {
            wayPoints.Add(joint_root.GetChild(i).gameObject);
        }

        //wayPoints.Add(anchor_root.GetChild(1).gameObject);

        centroidTransform = joint_root.GetChild(Mathf.FloorToInt(joint_root.childCount / 2f));
    }

    void AssignSplineNodes()
    {
        foreach (var penisNode in wayPoints.ToList())
        {
            if (penisNode == null) wayPoints.Remove(penisNode);
        }
        int nodeCount = wayPoints.Count;
        // adjust the number of nodes in the spline.
        while (spline.nodes.Count < nodeCount)
        {
            spline.AddNode(new SplineNode(Vector3.zero, Vector3.zero));
        }
        while (spline.nodes.Count > nodeCount && spline.nodes.Count > 2)
        {
            spline.RemoveNode(spline.nodes.Last());
        }
    }

    float CalculateRopeLength()
    {
        Vector3 last_pos = Vector3.zero;
        float dis_total = 0;

        foreach (GameObject wayPoint in wayPoints)
        {
            if (last_pos == Vector3.zero)
            {
                last_pos = wayPoint.transform.position;
            }
            else
            {
                dis_total += Vector3.Distance(wayPoint.transform.position, last_pos);
                last_pos = wayPoint.transform.position;
            }
        }
        //Debug.Log($"Rope{transform.GetSiblingIndex()} Length:{dis_total}");
        return dis_total;
    }
}
