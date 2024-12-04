using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Collections;

public class EffectSpring_AlvaNoto : MonoBehaviour
{

    public Performer performerStart;
    public Performer performerEnd;

    int springIndex;
    public int SpringIndex { get => springIndex; }

    public bool springEnabled = false;

    LineRenderer lineRender;



    float N = 1;
    public Vector2 NRange;

    const int pointCount = 100;
    List<Vector3> pointList = new List<Vector3>();

    [System.Serializable]
    public class WaveLine
    {
        public float waveLengthScaler;
        public float sinOffset;
        public float shakeSinValue;
        public float shakeSpeedScaler;
        public float shakeYScaler;
    }

    List<WaveLine> wavelineList = new List<WaveLine>();

    List<LineRenderer> lineRendererList = new List<LineRenderer>();

    public Vector2 shakeSpeedRange;
    public float shakeSpeed;
    float shakeSinValue;
    public Vector2 shakeYRange;
    public float shakeY;


    float baseRotateAngle = 0;
    public Vector2 rotateSpeedRange;
    public float rotateSpeed = 10;

    public float sampleScaler = 1;

    public float randomScaler = 1;


    void Start()
    {
        if (pointList == null)
            pointList = new List<Vector3>();

        if (pointList.Count != pointCount)
        {
            pointList.Clear();
            for (int i = 0; i < pointCount; i++)
            {
                pointList.Add(Vector3.zero);
            }
        }

        if (lineRendererList.Count != transform.childCount)
        {
            lineRendererList.Clear();
            foreach (Transform child in transform)
            {
                lineRendererList.Add(child.GetComponent<LineRenderer>());
            }
        }

        if (wavelineList.Count != transform.childCount)
        {
            wavelineList.Clear();
            foreach (Transform child in transform)
            {
                WaveLine new_wave_line = new WaveLine();
                new_wave_line.waveLengthScaler = Random.Range(0.5f, 2f);
                new_wave_line.shakeSinValue = 0;
                new_wave_line.shakeSpeedScaler = Random.Range(0.7f, 1.4f);
                new_wave_line.shakeYScaler = Random.Range(0.7f, 1.4f);
                wavelineList.Add(new_wave_line);
            }
        }
    }

    
    void Update()
    {
        AudioProcessor audioProcessor = GameManager.Instance.AudioProcessor;
        ////////////////////////////
        // update points
        ////////////////////////////
        ///

        float dis = Vector3.Distance(performerStart.transform.position, performerEnd.transform.position);
        Vector3 direction = (performerEnd.transform.position - performerStart.transform.position).normalized;
        Vector3 normal = Vector3.Cross(direction, Vector3.up);

        

        for (int m =0; m<lineRendererList.Count; m++)
        {
            LineRenderer lineRenderer = lineRendererList[m];

            // wave length. we take wave length as 1, then we can use percentage as X
            float wave_length = 2 * 1/ N;// wavelineList[m].N;

            // shake
            shakeSpeed = Utilities.Remap(audioProcessor.AudioVolume, 0, 1, shakeSpeedRange.x, shakeSpeedRange.y) * wavelineList[m].shakeSpeedScaler;
            wavelineList[m].shakeSinValue += shakeSpeed * Time.deltaTime;

            // rotate whole line
            rotateSpeed = Utilities.Remap(audioProcessor.AudioVolume, 0, 1, rotateSpeedRange.x, rotateSpeedRange.y);
            baseRotateAngle += Time.deltaTime * rotateSpeed;

            //
            shakeY = Utilities.Remap(audioProcessor.AudioVolume, 0, 1, shakeYRange.x, shakeYRange.y) * wavelineList[m].shakeYScaler;

            //
            float N_scaler = Utilities.Remap(audioProcessor.AudioVolume, 0, 1, NRange.x, NRange.y) * wavelineList[m].waveLengthScaler;

            float radiate_angle = baseRotateAngle + (float)m / (float)lineRendererList.Count * 360;
            Quaternion rotation = Quaternion.Euler(radiate_angle, 0, 0);
            Vector3 line_normal = rotation * normal;

            for (int i=0; i<pointCount; i++)
            {
                float per = (float)i / (float)(pointCount - 1);
                Vector3 pos_in_line = Vector3.Lerp(performerStart.transform.position, performerEnd.transform.position, per);
                float disA = dis * per;
                float disB = dis * (1 - per);

                //Vector3 right = (pointEnd.position - pointStart.position).normalized;
                //Vector3 forward = (Camera.main.transform.position - pos_in_line).normalized;
                //Vector3 up = Vector3.Cross(right, forward);


                per = Utilities.Remap(per, 0f, 1f, 0.5f-N_scaler*0.5f, 0.5f+ N_scaler*0.5f);
                float amp = shakeY * Mathf.Sin(2 * Mathf.PI / wave_length * (per - wavelineList[m].shakeSinValue)) + shakeY * Mathf.Sin(2 * Mathf.PI / wave_length * (per + wavelineList[m].shakeSinValue));

                int sample_index = Mathf.FloorToInt((float)i / (float)pointCount * (float)audioProcessor.Samples.Length);
                amp += audioProcessor.Samples[sample_index] * sampleScaler;

                float random_scaler = Utilities.Remap(audioProcessor.AudioVolume, 0, 1, 0, randomScaler);
                amp += Random.Range(-1f, 1f) * random_scaler;

                Vector3 point_normal = line_normal;//Quaternion.Euler(per * 360f, 0, 0) * line_normal;

                pointList[i] = pos_in_line + point_normal * amp;
            }
            // draw with line renderer
            lineRenderer.SetPositions(pointList.ToArray());
        }

        
    }

    public void SetSpringState(bool state)
    {
        springEnabled = state;

        lineRender.enabled = state;
    }

    public void BindPerformer(Performer performer_start, Performer performer_end)
    {
        performerStart = performer_start;
        performerEnd = performer_end;
    }
}
