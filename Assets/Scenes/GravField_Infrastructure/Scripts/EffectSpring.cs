using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SplineMesh;
using Unity.Netcode;
using UnityEngine;

public class EffectSpring : MonoBehaviour
{
    // Basic
    public Performer performerStart;
    public Performer performerEnd;
    Transform ropeStart;
    Transform ropeEnd;
    public Vector3 ropeOffset;
    int springIndex;
    bool springEnabled = false;


    // Path
    Spline spline;
    List<GameObject> wayPoints = new List<GameObject>();
    LineRenderer lineRenderer;
    Material lineMat;

    // Output to LIVE
    Transform centroidTransform;
    private Vector3 centroidPos;
    public Vector3 CentroidPos { get => centroidPos; }
    private Vector3 centroidVel;
    public Vector3 CentroidVel { get => centroidVel; }
    AutoSwitchedParameter<float> ropevel = new AutoSwitchedParameter<float>();

    // Parameters    
    float ropeMeshScale;
    float startThickness;
    float endThickness;
    float startMass;
    float endMass;

    bool jointAnchorInitialzed = false;

    void Awake()
    {
        springIndex = transform.GetSiblingIndex();

        ropeStart = transform.Find("Anchors").GetChild(0);
        ropeEnd = transform.Find("Anchors").GetChild(1);
    }

    void Start()
    {
        spline = GetComponent<Spline>();
        lineRenderer = GetComponent<LineRenderer>();
        lineMat = lineRenderer.material;

        AssignWayPoints();

        AssignSplineNodes();

        RegisterNetworkVariableCallback_Client();

        RegisterPropertiesToLive_Server();
    }

    

    void Update()
    {
        if (springEnabled == false) return;

        UpdateRopeAnchors();

        UpdateNodes();

        UpdateLineRenderer();

        UpdateParamtersForLive();
    }

    void UpdateRopeAnchors()
    {
        //Vector3 nor = (performerEnd.position - performerStart.position).normalized;
        //ropeStart.localPosition = performerStart.position + nor* 0.3f;
        //ropeEnd.localPosition = performerEnd.position -nor*0.3f;

        ropeStart.localPosition = performerStart.transform.TransformPoint(ropeOffset);
        ropeEnd.localPosition = performerEnd.transform.TransformPoint(ropeOffset);

        ropeStart.localRotation = performerStart.transform.localRotation;
        ropeEnd.localRotation = performerEnd.transform.localRotation;
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

    void UpdateLineRenderer()
    {
        int sampleCount = lineRenderer.positionCount;

        List<float> audio_data = GameManager.Instance.AudioProcessor.ListFFT;
        if (audio_data == null) audio_data = new List<float>();

        int index_step = Mathf.FloorToInt(audio_data.Count / sampleCount);
        float min = float.MaxValue;
        float max = float.MinValue;

        Vector3 axis_y = Vector3.up;
        Vector3 axis_z = (ropeEnd.position - ropeStart.position).normalized;
        Vector3 axis_x = Vector3.Cross(axis_y, axis_z);
        axis_y = Vector3.Cross(axis_x, axis_z);
        float random_angle = Random.Range(0, 2 * Mathf.PI);
        Vector3 random_dir = Mathf.Cos(random_angle) * axis_x + Mathf.Sin(random_angle) * axis_y;

        for (int i=0; i< sampleCount; i++)
        {
            // Normally, audio value ranges from -0.25 to 0.25
            float audio_value = audio_data[i * index_step];
            audio_value = Utilities.Remap(audio_value, -0.25f, 0.25f, -0.05f, 0.05f);
            min = Mathf.Min(min, audio_value);
            max = Mathf.Max(max, audio_value);

            //try
            //{
                CurveSample point = spline.GetSampleAtDistance(spline.Length * (float)i / (float)(sampleCount - 1));

                //Vector3 axis_y = point.up;
                //Vector3 axis_z = point.tangent;
                //Vector3 axis_x = Vector3.Cross(axis_y, axis_z);
                //float random_angle = Random.Range(0, 2 * Mathf.PI);
                //Vector3 random_dir = Mathf.Cos(random_angle) * axis_x + Mathf.Sin(random_angle) * axis_y;

                Vector3 random_pos = audio_value * random_dir;

                lineRenderer.SetPosition(i, point.location + random_pos);// point.up * audio_value); 
            //}
            //catch
            //{

            //}
        }

        float distance_param = Utilities.Remap(spline.Length, 0, 10, 0, 1);
        lineMat.SetFloat("_Distance", distance_param);

        distance_param = Utilities.Remap(spline.Length, 0, 10, 0.05f, 0.001f);
        lineRenderer.startWidth = distance_param;
        lineRenderer.endWidth = distance_param;

        //Debug.Log($"Min:{min}, Max{max}");
    }

    void UpdateParamtersForLive()
    {
        Vector3 last_pos = centroidPos;
        centroidPos = centroidTransform.localPosition;
        centroidVel = (centroidPos - last_pos) / Time.deltaTime;

        ropevel.OrginalValue = centroidVel.magnitude;
    }

    public void SetSpringState(bool state)
    {
        springEnabled = state;


    }

    #region NetworkVariable
    void RegisterNetworkVariableCallback_Client()
    {
        performerStart.remoteThickness.OnValueChanged += (float prev, float cur) => { startThickness = cur; UpdateRopeThickness(); };
        performerEnd.remoteThickness.OnValueChanged += (float prev, float cur) => { endThickness = cur; UpdateRopeThickness(); };

        performerStart.remoteMass.OnValueChanged += (float prev, float cur) => { startMass = cur; UpdateRopeMass(); };
        performerEnd.remoteMass.OnValueChanged += (float prev, float cur) => { endMass = cur; UpdateRopeMass(); };

        GameManager.Instance.PerformerGroup.ropeMeshScale.OnValueChanged += (float prev, float cur) => { ropeMeshScale = cur; UpdateRopeMeshScale(); };

    }
    void UpdateRopeThickness()
    {
        float currentLength = 0;
        foreach (CubicBezierCurve curve in spline.GetCurves())
        {
            float startRate = currentLength / spline.Length;
            currentLength += curve.Length;
            float endRate = currentLength / spline.Length;

            curve.n1.Scale = Vector3.one * (startThickness + (endThickness - startThickness) * startRate);
            curve.n2.Scale = Vector3.one * (startThickness + (endThickness - startThickness) * endRate);
        }
    }

    void UpdateRopeMass()
    {
        Transform segment_root = transform.Find("Segments");
        for (int m = 0; m < segment_root.childCount; m++)
        {
            Rigidbody rigid = segment_root.GetChild(m).GetComponent<Rigidbody>();
            rigid.mass = Mathf.Lerp(startMass, endMass, m / segment_root.childCount - 1);
        }
    }

    void UpdateRopeMeshScale()
    {
        SplineMeshTiling meshTiling = GetComponent<SplineMeshTiling>();
        meshTiling.scale = Mathf.Max(0.05f, ropeMeshScale) * Vector3.one;
    }
    #endregion

    #region Parameter sent to Live
    void RegisterPropertiesToLive_Server()
    {
        //if (NetworkManager.Singleton.IsServer == false) return;

        //SenderForLive.Instance.RegisterOscPropertyToSend(FormatedOscAddress("vel"), ropevel);
    }

    string FormatedOscAddress(string param)
    {
        return "/spring" + springIndex.ToString() + param;
    }
    #endregion

    #region Paramters received from Coda

    #endregion




    public void BindPerformer(Performer performer_start, Performer performer_end)
    {
        performerStart = performer_start;
        performerEnd = performer_end;
    }

    //public void SetPerformerOffset(Vector3 offset)
    //{
    //    ropeOffset = offset;
    //}

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
