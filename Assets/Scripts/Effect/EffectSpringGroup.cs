using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class EffectSpringGroup : MonoBehaviour
{
    Performer performerStart;
    Performer performerEnd;

    int springIndex;
    public int SpringIndex { get => springIndex; }

    bool springEnabled = false;

    [System.Serializable]
    public class WaveLine
    {
        public float shakeSinValue;

        public float waveLengthScaler;        
        public float shakeSpeedScaler;
        public float shakeStrengthScaler;
        public float waveWidthScaler;

        //public LineRenderer lineRenderer;
        public MeshRenderer meshRenderer;
        public Material mat;
    }
    List<WaveLine> wavelineList = new List<WaveLine>();

    const int pointCount = 100;
    List<Vector3> pointList = new List<Vector3>();

    public Vector2 wavelengthScalerRange;
    public Vector2 shakeSpeedRange;
    public Vector2 shakeStrengthRange;
    public Vector2 waveWidthRange;

    float baseRotateAngle = 0;
    public Vector2 rotateSpeedRange;

    [HideInInspector]
    public Vector3 springOffset;

    void Awake()
    {
        springIndex = transform.GetSiblingIndex();

    }

    void Start()
    {
        pointList.Clear();
        for (int i = 0; i < pointCount; i++)
        {
            pointList.Add(Vector3.zero);
        }

        wavelineList.Clear();
        foreach (Transform child in transform)
        {
            wavelineList.Add(CreateNewWaveLine(child));
        }
    }

    WaveLine CreateNewWaveLine(Transform trans)
    {
        WaveLine waveline = new WaveLine();

        waveline.waveLengthScaler = Random.Range(0.5f, 2f);
        waveline.shakeSinValue = 0;
        waveline.shakeSpeedScaler = Random.Range(0.7f, 1.4f);
        waveline.shakeStrengthScaler = Random.Range(0.7f, 1.4f);
        waveline.waveWidthScaler = Random.Range(0.7f, 1.4f);

        //waveline.lineRenderer = trans.GetComponent<LineRenderer>();
        waveline.meshRenderer = trans.GetComponent<MeshRenderer>();
        waveline.mat = waveline.meshRenderer.material;

        return waveline;
    }

    public void InitializeSpringGroup(Performer start, Performer end, Vector3 offset)
    {
        performerStart = start;
        performerEnd = end;

        springOffset = offset;

        //for (int i = 0; i < transform.childCount; i++)
        //{
        //    EffectSpring_AlvaNoto spring = transform.GetChild(i).GetComponent<EffectSpring_AlvaNoto>();
        //    spring.BindPerformer(start, end);
        //    springList.Add(spring);
        //}
    }

    public void SetSpringState(bool state)
    {
        springEnabled = state;

        //for (int i = 0; i < springList.Count; i++)
        //{
        //    springList[i].SetSpringState(state);
        //}
        foreach(var wave in wavelineList)
        {
            //wave.lineRenderer.enabled = state;
            wave.meshRenderer.enabled = state;
        }
    }

    void Update()
    {
        if (springEnabled == false) return;

        // update parameters
        UpdateParameter();

    }

    void UpdateParameter()
    {
        float audioVolume = (performerStart.soundVolume.Value + performerEnd.soundVolume.Value) * 0.5f;
        float audioPitch = (performerStart.soundPitch.Value + performerEnd.soundPitch.Value) * 0.5f;

        Vector3 startPos = performerStart.transform.TransformPoint(springOffset);
        Vector3 endPos = performerEnd.transform.TransformPoint(springOffset);

        float dis = Vector3.Distance(startPos, endPos);
        Vector3 direction = (endPos - startPos).normalized;
        Vector3 normal = Vector3.Cross(direction, Vector3.up);
        

        Quaternion base_rotation = Quaternion.LookRotation(direction, Vector3.up);
        Vector3 base_angles = base_rotation.eulerAngles;

        // rotate all lines
        float rotateSpeed = Utilities.Remap(audioVolume, 0, 1, rotateSpeedRange.x, rotateSpeedRange.y);
        baseRotateAngle += Time.deltaTime * rotateSpeed;

        // wave length. we take wave length as 1, then we can use percentage as X
        float L = 1;
        float N = 1;
        float wavelength = 2 * L / N;

        for (int m = 0; m < wavelineList.Count; m++)
        {
            WaveLine waveline = wavelineList[m];

            // shake sin value
            float shake_speed = Utilities.Remap(audioVolume, 0, 1, shakeSpeedRange.x, shakeSpeedRange.y) * waveline.shakeSpeedScaler;
            waveline.shakeSinValue += shake_speed * Time.deltaTime;

            // shake strength
            float shake_strength = Utilities.Remap(audioVolume, 0, 1, shakeStrengthRange.x, shakeStrengthRange.y) *
                Utilities.Remap(dis, 0, 5, 1, 0.1f, true) * waveline.shakeStrengthScaler;

            // wave length
            float wavelength_scaler = Utilities.Remap(audioVolume, 0, 1, wavelengthScalerRange.x, wavelengthScalerRange.y) * waveline.waveLengthScaler;

            // wave line width
            float width = Utilities.Remap(dis, 5, 0, waveWidthRange.x, waveWidthRange.y) * 
                Utilities.Remap(audioVolume, 0, 1, 0.8f, 1.5f) * waveline.waveWidthScaler;

            // line rotation
            float radiate_angle = baseRotateAngle + (float)m / (float)wavelineList.Count * 360;
            Quaternion rotation = Quaternion.Euler(base_angles.x, base_angles.y, base_angles.z + radiate_angle); 
            Vector3 line_normal = rotation * normal;
            Vector3 forward = Vector3.Cross(direction, line_normal);

            // the logic behind Shader Graph
            //for (int i = 0; i < pointCount; i++)
            //{
            //    float per = (float)i / (float)(pointCount - 1);
            //    Vector3 pos_in_line = Vector3.Lerp(startPos, endPos, per);
            //    float disA = dis * per;
            //    float disB = dis * (1 - per);

            //    per = Utilities.Remap(per, 0f, 1f, 0.5f - wavelength_scaler * 0.5f, 0.5f + wavelength_scaler * 0.5f);
            //    float amp = shake_strength * Mathf.Sin(2 * Mathf.PI / wavelength * (per - wavelineList[m].shakeSinValue))
            //        + shake_strength * Mathf.Sin(2 * Mathf.PI / wavelength * (per + wavelineList[m].shakeSinValue));

            //    Vector3 point_normal = line_normal;//Quaternion.Euler(per * 360f, 0, 0) * line_normal;

            //    pointList[i] = pos_in_line + point_normal * amp;
            //}


            // draw with line renderer
            //waveline.lineRenderer.SetPositions(pointList.ToArray());

            waveline.mat.SetVector("_StartPos", startPos);
            waveline.mat.SetVector("_EndPos", endPos);
            waveline.mat.SetFloat("_WaveLengthScaler", wavelength_scaler);
            waveline.mat.SetFloat("_ShakeStrength", shake_strength);
            waveline.mat.SetFloat("_ShakeSinValue", waveline.shakeSinValue);
            waveline.mat.SetVector("_Normal", line_normal);
            waveline.mat.SetVector("_Forward", forward);
            waveline.mat.SetFloat("_Width", width);
            waveline.mat.SetFloat("_Volume", audioVolume);
        }
    }


}
