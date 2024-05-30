using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SplineMesh;
using Unity.Netcode;
using UnityEngine;


public class SoundWaveLine
{
    public float soundData = 0;
    public float shakeY;
    public float sinShift;
    public float sinSpeed;
    public float curSinSpeed;
    public float sinFrequency;
    public float chaos;
    public float thickness;

    public float dstSoundData = 0;
    public float falloffSpeed = 0.5f;
    public float direction = 1;
    public float elapsedTime = 0;
    public float duration = 0.5f;
}


public class EffectSpring : MonoBehaviour
{
    public Camera cam;
    // Basic
    public Performer performerStart;
    public Performer performerEnd;
    //Transform ropeStart;
    //Transform ropeEnd;
    public Vector3 ropeOffset;
    int springIndex;
    public bool springEnabled = false;
    bool isInitialized = false;


    // Path    
    //Spline spline;
    //List<GameObject> wayPoints = new List<GameObject>();
    //LineRenderer lineRenderer;
    //Material lineMat;


    //Mesh
    MeshRenderer springMesh;

    // Output to LIVE
    Transform centroidTransform;
    private Vector3 centroidPos;
    public Vector3 CentroidPos { get => centroidPos; }
    private Vector3 centroidVel;
    public Vector3 CentroidVel { get => centroidVel; }
    AutoSwitchedParameter<float> ropevel = new AutoSwitchedParameter<float>();

    // Parameters    
    //float ropeMeshScale;
    //float startThickness;
    //float endThickness;
    //float startMass;
    //float endMass;


    float sinBaseValue;


    [SerializeField]
    Material meshMat;
    const int LINE_COUNT = 5;
    float soundwaveFrequency = 30;
    float frequencyRange = 30;
    public float soundAmplifier = 6;
    List<SoundWaveLine> soundWaveList = new List<SoundWaveLine>();

    float maxDistance = 8;
    public float maxSpringThickness = 1;
    public float minSpringThickness = 0.2f;
    public Vector2 switchDirectionTime = new Vector2(0, 1f);
    public float attackSpeed = 0.01f;
    public float decaySpeed = 0.001f;
    public float sinChangeSpeed = 0.001f;
    public Vector2 sinSpeedRange = new Vector2(0.4f, 1f);

    public float amplifier = 1;
    public float period = 1;

    void Awake()
    {
        springIndex = transform.GetSiblingIndex();

        //ropeStart = transform.Find("Anchors").GetChild(0);
        //ropeEnd = transform.Find("Anchors").GetChild(1);

        springMesh = GetComponent<MeshRenderer>();

        sinBaseValue = Random.Range(0, 1000f);

        for(int i=0; i< LINE_COUNT; i++)
        {
            SoundWaveLine soundwave = new SoundWaveLine();
            soundWaveList.Add(soundwave);

            soundwave.soundData = 0;
            soundwave.chaos = 0;
            soundwave.sinShift = Random.Range(0f, 1000f);
            soundwave.sinSpeed = Random.Range(5f, 11f); 
            soundwave.sinFrequency = soundwaveFrequency + Random.Range(-0.5f, 0.5f) * frequencyRange;

            soundwave.direction = Random.Range(0f, 1f) > 0.5f ? 1 : -1;
            soundwave.duration = Random.Range(0f, 1f);
        }
    }

    //void OnEnable()
    //{
    //    GameManager.Instance.PerformerGroup.OnPerformerFinishSpawn.AddListener(OnPerformerFinishSpawn);
    //}

    void OnPerformerFinishSpawn()
    {
        InitializeLocalVariable();

        RegisterNetworkVariableCallback_Client();

        RegisterPropertiesToLive_Server();
    }

    void Start()
    {
        //spline = GetComponent<Spline>();
        //lineRenderer = GetComponent<LineRenderer>();
        //lineMat = lineRenderer.material;

        //AssignWayPoints();

        //AssignSplineNodes(); 
    }

    

    void Update()
    {
        if (isInitialized == false && GameManager.Instance.PerformerGroup.PerformerFinishSpawn == true)
        {
            OnPerformerFinishSpawn();
            isInitialized = true;
        }

        if (springEnabled == false) return;

        UpdateSpringAnchors();

        //UpdateNodes();

        //UpdateSpringThickness();

        UpdateSpringMaterial();

        //UpdateLineRenderer();

        //UpdateParamtersForLive();
    }

    void UpdateSpringAnchors()
    {
        Vector3 start_pos = performerStart.transform.TransformPoint(ropeOffset);// + RandomOffset(performerStart.transform.position));
        Vector3 end_pos = performerEnd.transform.TransformPoint(ropeOffset);// + RandomOffset(performerEnd.transform.position));

        springMesh.transform.position = Vector3.Lerp(start_pos, end_pos, 0.5f);
        float width = 2;
        float length = Vector3.Distance(start_pos, end_pos);
        float new_width = Utilities.Remap(length, 1, maxDistance, maxSpringThickness, minSpringThickness, true);
        springMesh.transform.localScale = new Vector3(length / 5.0f, 1, new_width);

        Vector3 dis = end_pos - start_pos;
        float rotation_y = Mathf.Atan2(dis.x, dis.z);
        Vector3 eye_up = GameManager.Instance.HolokitCameraManager.CenterEyePose.up;
        //Debug.Log(eye_up);
        springMesh.transform.rotation = Quaternion.LookRotation(Vector3.Cross(dis.normalized, Vector3.up), Vector3.up);

        





        //ropeStart.localRotation = performerStart.transform.localRotation;
        //ropeEnd.localRotation = performerEnd.transform.localRotation;

        //float mass = GameManager.Instance.AudioProcessor.AudioVolume;
        //mass = Utilities.Remap(mass, 0, 1f, 20f, 60f) * Random.Range(0.9f, 1.1f);
        //SetSegmentMass(mass);
    }

    Vector3 RandomOffset(Vector3 pos)
    {
        //List<float> audio_data = GameManager.Instance.AudioProcessor.ListFFT;
        //float random_angle = Mathf.PerlinNoise(Time.time + pos.x, pos.y);
        //random_angle = Utilities.Remap(random_angle, 0, 1, 0, 2 * Mathf.PI);
        //Vector3 random_offset = new Vector3(Mathf.Cos(random_angle), Mathf.Sin(random_angle), 0);
        //if (audio_data != null && audio_data.Count > springIndex)
        //{
        //    float sound_factor = Utilities.Remap(Mathf.Abs(audio_data[springIndex]), 0, 0.25f, 0, 0.01f);
        //    random_offset = random_offset * sound_factor;
        //}

        float offset_y = Mathf.Sin(sinBaseValue) * GameManager.Instance.AudioProcessor.AudioVolume * 0.2f;
        sinBaseValue += Time.deltaTime*4;
        Vector3 random_offset = new Vector3(0, offset_y, 0);


        return random_offset;
    }

    //void UpdateSpringThickness()
    //{
    //    //float length = spline.Length;
    //    float length = Vector3.Distance(performerStart.transform.position, performerEnd.transform.position);
    //    float new_thickness = Utilities.Remap(length, 1, maxDistance, maxSpringThickness, minSpringThickness, true);


    //    SetSpringThickness(new_thickness, GetAngleX(performerStart.transform), GetAngleX(performerEnd.transform));
    //}

    float GetAngleX(Transform trans)
    {
        float angle_x = Mathf.Abs(trans.localRotation.eulerAngles.x);
        angle_x = angle_x > 180 ? 360 - angle_x : angle_x;

        return angle_x;
    }

    void UpdateSpringMaterial()
    {
        List<float> audio_data = GameManager.Instance.AudioProcessor.ListFFT;
        int step = Mathf.FloorToInt(audio_data.Count / LINE_COUNT * 0.5f) ;
        for (int i = 0; i < LINE_COUNT; i++)
        {
            SoundWaveLine soundwave = soundWaveList[i];

            float new_sound_data = 0;

            if (audio_data.Count > 0)
                new_sound_data = Mathf.Abs(audio_data[i * step]) * soundAmplifier; // Utilities.Remap(Mathf.Abs(audio_data[i * step] * soundAmplifier), 0, 1, 0, 1, true);




            //if (Mathf.Abs(new_sound_data) > Mathf.Abs(soundwave.dstSoundData))
            //{
            //    soundwave.direction = -soundwave.direction;
            //}

            //soundwave.elapsedTime += Time.deltaTime;
            //if (soundwave.elapsedTime > soundwave.duration)
            //{
            //    soundwave.direction = -soundwave.direction;
            //    soundwave.elapsedTime = 0;
            //    soundwave.duration = Random.Range(switchDirectionTime.x, switchDirectionTime.y);
            //}

            soundwave.dstSoundData = new_sound_data * soundwave.direction;

            if (Mathf.Sign(soundwave.dstSoundData) != Mathf.Sign(soundwave.soundData))
            {
                soundwave.soundData = Mathf.Lerp(soundwave.soundData, soundwave.dstSoundData, attackSpeed);
            }
            else if (Mathf.Abs(soundwave.dstSoundData) > Mathf.Abs(soundwave.soundData))
            {
                soundwave.soundData = Mathf.Lerp(soundwave.soundData, soundwave.dstSoundData, attackSpeed);
            }
            else
            {
                soundwave.soundData = Mathf.Lerp(soundwave.soundData, soundwave.dstSoundData, decaySpeed);
            }

            //if (i == 0)
            //{
            //    Debug.Log($"shake:{soundwave.soundData}, dir:{soundwave.direction}, dst:{soundwave.dstSoundData}");
            //}

            soundwave.soundData = Oscillator(0, Mathf.Abs(soundwave.soundData) * amplifier, period);



            //float shake_y = Mathf.Sin(soundwave.sinShift);
            //shake_y = Mathf.Sign(shake_y) * EasingFunctions.InCubic(Mathf.Abs(shake_y));
            //soundwave.shakeY = soundwave.soundData * shake_y;
            soundwave.shakeY = soundwave.soundData;

            float new_sin_speed = soundwave.sinSpeed * Utilities.Remap(GameManager.Instance.AudioProcessor.AudioVolume, 0, 1f, sinSpeedRange.x, sinSpeedRange.y);

            soundwave.curSinSpeed = Mathf.Lerp(soundwave.curSinSpeed, new_sin_speed, sinChangeSpeed);
            soundwave.sinShift += soundwave.curSinSpeed * Time.deltaTime;

            soundwave.thickness = Utilities.Remap(soundwave.soundData, 0f, 1, 0.02f, 0.2f, true);
            soundwave.chaos = Utilities.Remap(soundwave.soundData, 0.5f, 1, 0, 0.02f, true);

            //soundwave.sinFrequency = 30 + Mathf.PerlinNoise1D(performerStart.localData.position.x) * soundwaveFrequency;

            soundwave.sinFrequency = 30 + Mathf.PerlinNoise(Time.time, springIndex) * soundwaveFrequency;
        }


        PushChangeToMaterial();
    }

    void PushChangeToMaterial()
    {
        //meshMat = transform.Find("generated by SplineMeshTiling").GetComponentInChildren<MeshRenderer>().material;

        float[] shake_y = new float[LINE_COUNT];
        float[] sin_shift = new float[LINE_COUNT];
        float[] sin_freq = new float[LINE_COUNT];
        float[] wave_thickness = new float[LINE_COUNT];
        float[] chaos = new float[LINE_COUNT];
        for (int i=0; i< LINE_COUNT; i++)
        {
            shake_y[i] = soundWaveList[i].shakeY;
            sin_shift[i] = soundWaveList[i].sinShift;
            sin_freq[i] = soundWaveList[i].sinFrequency;
            wave_thickness[i] = soundWaveList[i].thickness;
            chaos[i] = soundWaveList[i].chaos;
        }
        meshMat.SetFloatArray("waveShakeY", shake_y);
        meshMat.SetFloatArray("waveSinShift", sin_shift);
        meshMat.SetFloatArray("waveSinFrequncy", sin_freq);
        meshMat.SetFloatArray("waveThickness", wave_thickness);
        meshMat.SetFloatArray("waveChaos", chaos);


        float distance = Vector3.Distance(performerStart.transform.position, performerEnd.transform.position);
        meshMat.SetFloat("_SpringLength", Utilities.Remap(distance, 0, maxDistance, 0, 1, true));
        
    }

    float Oscillator(float _origin, float _ampitude, float _period)
    {
        var pos = _origin + _ampitude * Mathf.Sin(2.0f * Mathf.PI * Time.time / _period);
        return pos;
    }

    float Oscillator2(float time, float speed, float scale)
    {
        return Mathf.Cos(time * speed / Mathf.PI) * scale;
    }

    //void UpdateNodes()
    //{
    //    int i = 0;
    //    foreach (GameObject wayPoint in wayPoints)
    //    {
    //        var node = spline.nodes[i++];
    //        //if (Vector3.Distance(node.Position, transform.InverseTransformPoint(wayPoint.transform.position)) > 0.001f)
    //        //{
    //        //    node.Position = transform.InverseTransformPoint(wayPoint.transform.position);
    //        //    //node.Up = wayPoint.transform.up;
    //        //}
    //        node.Position = wayPoint.transform.position;
    //        //node.Up = wayPoint.transform.up;
    //    }
    //}

    //void UpdateLineRenderer()
    //{
    //    int sampleCount = lineRenderer.positionCount;

    //    List<float> audio_data = GameManager.Instance.AudioProcessor.ListFFT;
    //    if (audio_data == null) audio_data = new List<float>();

    //    int index_step = Mathf.FloorToInt(audio_data.Count / sampleCount);
    //    float min = float.MaxValue;
    //    float max = float.MinValue;

    //    //Vector3 axis_y = Vector3.up;
    //    //Vector3 axis_z = (ropeEnd.position - ropeStart.position).normalized;
    //    //Vector3 axis_x = Vector3.Cross(axis_y, axis_z);
    //    //axis_y = Vector3.Cross(axis_x, axis_z);
    //    //float random_angle = Random.Range(0, 2 * Mathf.PI);
    //    //Vector3 random_dir = Mathf.Cos(random_angle) * axis_x + Mathf.Sin(random_angle) * axis_y;

    //    for (int i=0; i< sampleCount; i++)
    //    {
    //        // Normally, audio value ranges from -0.25 to 0.25
    //        float audio_value = audio_data[i * index_step];
    //        audio_value = Utilities.Remap(audio_value, -0.25f, 0.25f, -0.05f, 0.05f);
    //        min = Mathf.Min(min, audio_value);
    //        max = Mathf.Max(max, audio_value);

    //        try
    //        {
    //            CurveSample point = spline.GetSampleAtDistance(spline.Length * (float)i / (float)(sampleCount - 1));

    //            //Vector3 axis_y = point.up;
    //            //Vector3 axis_z = point.tangent;
    //            //Vector3 axis_x = Vector3.Cross(axis_y, axis_z);
    //            //float random_angle = Random.Range(0, 2 * Mathf.PI);
    //            //Vector3 random_dir = Mathf.Cos(random_angle) * axis_x + Mathf.Sin(random_angle) * axis_y;

    //            //Vector3 random_pos = audio_value * random_dir;
    //            //lineRenderer.SetPosition(i, point.location + random_pos);// point.up * audio_value);

    //            lineRenderer.SetPosition(i, point.location);
    //        }
    //        catch
    //        {

    //        }
    //    }

    //    float distance_param = Utilities.Remap(spline.Length, 0, 10, 0, 1);
    //    lineMat.SetFloat("_Distance", distance_param);

    //    distance_param = Utilities.Remap(spline.Length, 0, 10, 0.02f, 0.0001f);
    //    lineRenderer.startWidth = distance_param;
    //    lineRenderer.endWidth = distance_param;

    //    //Debug.Log($"Min:{min}, Max{max}");
    //}

    //void UpdateParamtersForLive()
    //{
    //    Vector3 last_pos = centroidPos;
    //    centroidPos = centroidTransform.localPosition;
    //    centroidVel = (centroidPos - last_pos) / Time.deltaTime;

    //    ropevel.OrginalValue = centroidVel.magnitude;
    //}

    public void SetSpringState(bool state)
    {
        springEnabled = state;

        //lineRenderer.enabled = state;

        //Transform mesh_transform = transform.Find("generated by SplineMeshTiling");
        //mesh_transform?.gameObject.SetActive(state);

        springMesh.enabled = state;
    }

    #region NetworkVariable
    void InitializeLocalVariable()
    {
        //ropeMeshScale = GameManager.Instance.PerformerGroup.ropeMeshScale.Value;
        //startThickness = performerStart.remoteThickness.Value;
        //endThickness = performerEnd.remoteThickness.Value;
        //startMass = performerStart.remoteMass.Value;
        //endMass = performerEnd.remoteMass.Value;
    }

    void RegisterNetworkVariableCallback_Client()
    {
        //performerStart.remoteThickness.OnValueChanged += (float prev, float cur) => { startThickness = cur; SetSpringThickness(cur); };
        //performerEnd.remoteThickness.OnValueChanged += (float prev, float cur) => { endThickness = cur; SetSpringThickness(cur); };

        //performerStart.remoteMass.OnValueChanged += (float prev, float cur) => { startMass = cur; UpdateRopeMass(); };
        //performerEnd.remoteMass.OnValueChanged += (float prev, float cur) => { endMass = cur; UpdateRopeMass(); };

        //GameManager.Instance.PerformerGroup.ropeMeshScale.OnValueChanged += (float prev, float cur) => { ropeMeshScale = cur; UpdateRopeMeshScale(); };

        GameManager.Instance.PerformerList[springIndex].springFreq.OnValueChanged += (float prev, float cur) => { soundwaveFrequency = Mathf.Clamp(cur, 0, 200); };
        GameManager.Instance.PerformerList[springIndex].springWidth.OnValueChanged += (float prev, float cur) => { maxSpringThickness = Mathf.Clamp(cur, minSpringThickness, 40); };


    }
    //void SetSpringThickness(float new_thickness, float start_angle_x = 0, float end_angle_x = 0)
    //{
        //float currentLength = 0;
        //int curve_index = 0;
        //foreach (CubicBezierCurve curve in spline.GetCurves())
        //{            
        //    if (curve_index == 0 || curve_index == spline.curves.Count - 1)
        //    {
        //        float startRate = currentLength / spline.Length;
        //        currentLength += curve.Length;
        //        float endRate = currentLength / spline.Length;

        //        float start_thickness = curve_index == 0 ? Utilities.Remap(start_angle_x, 0, 90, new_thickness, new_thickness * 0.5f) : new_thickness;
        //        float end_thickness = curve_index == 0 ? new_thickness : Utilities.Remap(end_angle_x, 0, 90, new_thickness, new_thickness * 0.5f);

        //        curve.n1.Scale = Vector3.one * (start_thickness + (end_thickness - start_thickness) * startRate);
        //        curve.n2.Scale = Vector3.one * (start_thickness + (end_thickness - start_thickness) * endRate);
        //    }
        //    else
        //    {
        //        curve.n1.Scale = Vector3.one * new_thickness;
        //        curve.n2.Scale = Vector3.one * new_thickness;
        //    }
        //}
    //}

    //void UpdateRopeMass()
    //{
    //    Transform segment_root = transform.Find("Segments");
    //    for (int m = 0; m < segment_root.childCount; m++)
    //    {
    //        Rigidbody rigid = segment_root.GetChild(m).GetComponent<Rigidbody>();
    //        rigid.mass = Mathf.Lerp(startMass, endMass, m / segment_root.childCount - 1);
    //    }
    //    Debug.Log($"UpdateRopeMass:{startMass}, {endMass}");
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
        //if (NetworkManager.Singleton.IsServer == false) return;

        //SenderForLive.Instance.RegisterOscPropertyToSend(FormatedOscAddress("vel"), ropevel);
    }

    string FormatedOscAddressToLive(string param)
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

    void SetSegmentMass(float v)
    {
        Transform segment_root = transform.Find("Segments");
        for(int i=0; i<segment_root.childCount; i++)
        {
            Rigidbody rigid = segment_root.GetChild(i).GetComponent<Rigidbody>();
            rigid.mass = v;
        }
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






    //void AssignWayPoints()
    //{
    //    wayPoints.Clear();
    //    //Transform anchor_root = transform.Find("Anchors");
    //    //wayPoints.Add(anchor_root.GetChild(0).gameObject);

    //    Transform joint_root = transform.Find("Joints");
    //    for (int i = 0; i < joint_root.childCount; i++)
    //    {
    //        wayPoints.Add(joint_root.GetChild(i).gameObject);
    //    }

    //    //wayPoints.Add(anchor_root.GetChild(1).gameObject);

    //    centroidTransform = joint_root.GetChild(Mathf.FloorToInt(joint_root.childCount / 2f));
    //}

    //void AssignSplineNodes()
    //{
    //    foreach (var penisNode in wayPoints.ToList())
    //    {
    //        if (penisNode == null) wayPoints.Remove(penisNode);
    //    }
    //    int nodeCount = wayPoints.Count;
    //    // adjust the number of nodes in the spline.
    //    while (spline.nodes.Count < nodeCount)
    //    {
    //        spline.AddNode(new SplineNode(Vector3.zero, Vector3.zero));
    //    }
    //    while (spline.nodes.Count > nodeCount && spline.nodes.Count > 2)
    //    {
    //        spline.RemoveNode(spline.nodes.Last());
    //    }
    //}

    //float CalculateRopeLength()
    //{
    //    Vector3 last_pos = Vector3.zero;
    //    float dis_total = 0;

    //    foreach (GameObject wayPoint in wayPoints)
    //    {
    //        if (last_pos == Vector3.zero)
    //        {
    //            last_pos = wayPoint.transform.position;
    //        }
    //        else
    //        {
    //            dis_total += Vector3.Distance(wayPoint.transform.position, last_pos);
    //            last_pos = wayPoint.transform.position;
    //        }
    //    }
    //    //Debug.Log($"Rope{transform.GetSiblingIndex()} Length:{dis_total}");
    //    return dis_total;
    //}
}
