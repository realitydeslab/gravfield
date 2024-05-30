using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererTest : MonoBehaviour
{

    public Transform performerStart;
    public Transform performerEnd;


    List<LineRenderer> lineRendererList = new List<LineRenderer>();
    int pointCount = 100;

    int springIndex = 1;
    public float lienRendererWidth = 0.001f;

    [SerializeField]
    Material meshMat;
    const int LINE_COUNT = 5;
    float soundwaveFrequency = 30;
    float frequencyRange = 30;
    public float soundAmplifier = 6;
    List<SoundWaveLine> soundWaveList = new List<SoundWaveLine>();

    float maxDistance = 8;
    public float maxSpringThickness = 20;
    public float minSpringThickness = 0.2f;
    public Vector2 switchDirectionTime = new Vector2(0, 1f);
    public float attackSpeed = 0.01f;
    public float decaySpeed = 0.001f;
    public float sinChangeSpeed = 0.001f;
    public Vector2 sinSpeedRange = new Vector2(0.4f, 1f);

    public float amplifier = 1;
    public float period = 1;

    void Start()
    {

        for (int i=0; i<transform.childCount; i++)
        {
            lineRendererList.Add(transform.GetChild(i).GetComponent<LineRenderer>());
            lineRendererList[i].positionCount = pointCount;
            SoundWaveLine soundwave = new SoundWaveLine();
            soundWaveList.Add(soundwave);
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpringMaterial();
    }

    void UpdateSpringMaterial()
    {
        int LINE_COUNT = lineRendererList.Count;
        //List<float> audio_data = GameManager.Instance.AudioProcessor.ListFFT;
        //int step = Mathf.FloorToInt(audio_data.Count / LINE_COUNT * 0.5f);
        for (int i = 0; i < LINE_COUNT; i++)
        {
            SoundWaveLine soundwave = soundWaveList[i];

            float new_sound_data = 0;

            new_sound_data = Random.Range(0, soundAmplifier);// Mathf.Abs(audio_data[i * step]) * soundAmplifier; // Utilities.Remap(Mathf.Abs(audio_data[i * step] * soundAmplifier), 0, 1, 0, 1, true);




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

            float new_sin_speed = 2;////soundwave.sinSpeed * Utilities.Remap(GameManager.Instance.AudioProcessor.AudioVolume, 0, 1f, sinSpeedRange.x, sinSpeedRange.y);

            soundwave.curSinSpeed = Mathf.Lerp(soundwave.curSinSpeed, new_sin_speed, sinChangeSpeed);
            soundwave.sinShift += soundwave.curSinSpeed * Time.deltaTime;

            soundwave.thickness = Utilities.Remap(soundwave.soundData, 0f, 1, 0.02f, 0.2f, true);
            soundwave.chaos = Utilities.Remap(soundwave.soundData, 0.5f, 1, 0, 0.02f, true);

            //soundwave.sinFrequency = 30 + Mathf.PerlinNoise1D(performerStart.localData.position.x) * soundwaveFrequency;

            soundwave.sinFrequency = 30 + Mathf.PerlinNoise(Time.time, springIndex) * soundwaveFrequency;


            CalculateLinePos(performerStart.position, performerEnd.position, (float)i / (float)LINE_COUNT * 360f, soundwave, lineRendererList[i]);
        }

        
    }

    void CalculateLinePos(Vector3 start_pos, Vector3 end_pos, float angle,SoundWaveLine soundwave, LineRenderer line_renderer)
    {
        Vector3 dis = end_pos - start_pos;
        Vector3 up_direction;
        if (dis.magnitude < 0.001f)
            up_direction = Vector3.up;
        else
            up_direction = Vector3.Cross(Vector3.Cross(dis.normalized, Vector3.up).normalized, dis.normalized);


        for (int i=0; i<pointCount; i++)
        {
            float percentage = (float)i / (float)(pointCount - 1);
            Vector3 base_pos = Vector3.Lerp(start_pos, end_pos, percentage);
            Vector3 up_dir = Quaternion.AngleAxis(angle, dis.normalized) * up_direction;

            float offset_y = Mathf.Sin(percentage * soundwave.sinFrequency + soundwave.sinShift) * soundwave.shakeY;
            if (percentage < 0.1)
            {
                offset_y = offset_y * Utilities.Remap(percentage, 0, 0.1f, 0, 1);
            }
            else if (percentage > 0.9)
            {
                offset_y = offset_y * Utilities.Remap(percentage, 0.9f, 1, 1, 0);
            }

            line_renderer.SetPosition(i, base_pos + up_dir * offset_y);
        }
        line_renderer.startWidth = lienRendererWidth;
        line_renderer.endWidth = lienRendererWidth;
    }

    float Oscillator(float _origin, float _ampitude, float _period)
    {
        var pos = _origin + _ampitude * Mathf.Sin(2.0f * Mathf.PI * Time.time / _period);
        return pos;
    }
}
