using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;



public class AudioWaveTest : MonoBehaviour
{
    [Header("Auido Source")]
    public AudioSource microphoneAudioSource;
    public AudioSource masterAudioSource;

    [Header("Auido Mixer")]
    public AudioMixerGroup masterAudioGroup;
    public AudioMixerGroup microphoneAudioGroup;

    bool isInitialized = false;

    public AudioSource audioSource;


    [SerializeField]
    Material meshMat;

    const int LINE_COUNT = 5;
    public float[] soundData;
    float[] lineShakeY;
    float[] lineSinStartPos;
    float[] lineSinMoveSpeed;
    float[] lineSinFrequency;
    float[] lineSmoothness;
    float[] lineVibePosY;
    float[] lineVibePosYSpeed;

    float frequency = 30;
    float frequencyRange = 30;

    void Awake()
    {
        soundData = new float[LINE_COUNT];

        lineShakeY = new float[LINE_COUNT];
        lineSinStartPos = new float[LINE_COUNT];
        lineSinMoveSpeed = new float[LINE_COUNT];
        lineSinFrequency = new float[LINE_COUNT];
        lineSmoothness = new float[LINE_COUNT];
        lineVibePosY = new float[LINE_COUNT];
        lineVibePosYSpeed = new float[LINE_COUNT];

        for (int i = 0; i < LINE_COUNT; i++)
        {
            lineSinStartPos[i] = Random.Range(0f, 1000f);
            lineSinMoveSpeed[i] = Random.Range(2f, 4f);
            lineSinFrequency[i] = frequency + Random.Range( -0.5f , 0.5f) * frequencyRange;
            lineSmoothness[i] = 0;// Random.Range(0f, 1f);
            lineVibePosY[i] = Random.Range(-1f, 1f);
            if (lineVibePosY[i] > 0) lineVibePosYSpeed[i] = -1;
            else lineVibePosYSpeed[i] = 1;
        }

        meshMat.SetFloatArray("lineSinStartPos", lineSinStartPos);
        meshMat.SetFloatArray("lineSinFrequency", lineSinFrequency);
        meshMat.SetFloatArray("lineSmoothness", lineSmoothness);
    }


    void Start()
    {
        _samples = new float[QSamples];
        _spectrum = new float[QSamples];
        _fSample = AudioSettings.outputSampleRate;

        
    }

    void Update()
    {
        AnalyzeSound();
        audioVolume = Utilities.Remap(DbValue, -10, 10, 0, 1, true);
        audioPitch = Utilities.Remap(PitchValue, 0, 600, 0, 1, true);


        int step = Mathf.FloorToInt(listFFT.Count / LINE_COUNT);
        for (int i = 0; i < LINE_COUNT; i++)
        {
            float current_vel = 0;

            lineVibePosY[i] += lineVibePosYSpeed[i] * Time.deltaTime;
            if(lineVibePosY[i] > 1)
            {
                lineVibePosYSpeed[i] = -lineVibePosYSpeed[i];
            }
            else if(lineVibePosY[i] < -1)
            {
                lineVibePosYSpeed[i] = -lineVibePosYSpeed[i];
            }


            soundData[i] -= Time.deltaTime * 2f;
            if (soundData[i] < 0)
                soundData[i] = 0;
            

            float new_sound_data = 0;
            if (listFFT.Count == 0)
                new_sound_data = 0;
            else
                new_sound_data = Utilities.Remap(Mathf.Abs(listFFT[i * step]), 0, 0.3f, 0, 1,true); // Random.Range(-1f, 1f);// 

            if(Time.frameCount % 2 == 0)
                soundData[i] = new_sound_data > soundData[i] ? new_sound_data : soundData[i];

            //soundData[i] = lineVibePosY[i] * new_sound_data;
            //soundData[i] = new_sound_data * Mathf.Sin(lineSinStartPos[i]);

            float shake_y = Mathf.Sin(lineSinStartPos[i]);
            shake_y = Mathf.Sign(shake_y) * EasingFunctions.InCubic(Mathf.Abs(shake_y));
            lineShakeY[i] = soundData[i] * shake_y;

            float new_sin_start_pos = Random.Range(-5f, 5f);// * lineSinMoveSpeed[i];//Mathf.PerlinNoise(i, Time.time * 10) *
            lineSinStartPos[i] += 8f * Time.deltaTime;
            //lineSinStartPos[i] = Random.Range(-2f, 2f);//Mathf.SmoothDamp(lineSinStartPos[i], new_sin_start_pos, ref current_vel, 1.5f);
            //lineSinStartPos[i] = 10 *Time.deltaTime * Mathf.PerlinNoise(i, Time.time * 10) * lineSinMoveSpeed[i];//Random.Range(10f, -10f) * 
            //lineSinStartPos[i] = Mathf.PerlinNoise(i*5, Time.time * 10); //Random.Range(-1f, 1f) * 10f; 
            //lineSinFrequency[i] = frequency * Utilities.Remap(soundData[i], 0, 1, 1f, 3f);
            lineSmoothness[i] = Utilities.Remap(soundData[i], 0.5f, 1, 0, 0.2f, true);
        }


        meshMat.SetFloatArray("soundData", lineShakeY);
        //meshMat.SetFloatArray("lineSinStartPos", lineSinStartPos);
        meshMat.SetFloatArray("lineSinFrequency", lineSinFrequency);
        meshMat.SetFloatArray("lineSmoothness", lineSmoothness);
    }

    public void OnChangeFrequency(float v)
    {
        frequency = v;
        for (int i = 0; i < LINE_COUNT; i++)
        {
            lineSinFrequency[i] = frequency + Random.Range(-0.5f, 0.5f) * frequencyRange;
        }
        meshMat.SetFloatArray("lineSinFrequency", lineSinFrequency);
    }
    public void OnChangeFrequencyRange(float v)
    {
        frequencyRange = v;
        for (int i = 0; i < LINE_COUNT; i++)
        {
            lineSinFrequency[i] = frequency + Random.Range(-0.5f, 0.5f) * frequencyRange;
        }
        meshMat.SetFloatArray("lineSinFrequency", lineSinFrequency);
    }
    //public void OnChangeFrequencyRange(float v)
    //{

    //}


    #region Method Two
    float RmsValue;
    float DbValue;
    float PitchValue;

    private const int QSamples = 64;
    private float RefValue = 0.1f;
    private float Threshold = 0.02f;

    float[] _samples;
    float[] _spectrum;
    float _fSample;

    List<float> listFFT = new List<float>();
    const int bufferInitialCapacity = 10;

    AudioClip clipRecord;
    string deviceName;

    float audioVolume;
    public float AudioVolume { get { return audioVolume; } }

    float audioPitch;
    public float AudioPitch { get { return audioPitch; } }
    void AnalyzeSound()
    {
        microphoneAudioSource.GetOutputData(_samples, 0); // fill array with samples
        int i;
        float sum = 0;
        for (i = 0; i < QSamples; i++)
        {
            sum += _samples[i] * _samples[i]; // sum squared samples
        }
        RmsValue = Mathf.Sqrt(sum / QSamples); // rms = square root of average
        DbValue = 20 * Mathf.Log10(RmsValue / RefValue); // calculate dB
        if (DbValue < -160) DbValue = -160; // clamp it to -160dB min
                                            // get sound spectrum
        microphoneAudioSource.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
        float maxV = 0;
        var maxN = 0;
        for (i = 0; i < QSamples; i++)
        {
            // find max 
            if (!(_spectrum[i] > maxV) || !(_spectrum[i] > Threshold))
                continue;

            maxV = _spectrum[i];
            maxN = i; // maxN is the index of max
        }
        float freqN = maxN; // pass the index to a float variable
        if (maxN > 0 && maxN < QSamples - 1)
        { // interpolate index using neighbours
            var dL = _spectrum[maxN - 1] / _spectrum[maxN];
            var dR = _spectrum[maxN + 1] / _spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }
        PitchValue = freqN * (_fSample / 2) / QSamples; // convert index to frequency


        listFFT.Clear();
        int index_step = Mathf.FloorToInt(_samples.Length / bufferInitialCapacity);
        int sample_index = 0;
        for (int k = 0; k < bufferInitialCapacity; k++)
        {
            listFFT.Add(_samples[sample_index]);
            sample_index += index_step;
        }
    }
    #endregion
    
    #region Micorphone 
    void StartMicrophone()
    {
        if (isInitialized)
            return;

        // List devices
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphone device found!");
            return;
        }
        //for (int i = 0; i < Microphone.devices.Length; i++)
        //{
        //    Debug.Log(string.Format("Mic{0}:{1}", i, Microphone.devices[i]));
        //}

        if (deviceName == null)
            deviceName = Microphone.devices[0];

        // Start microphone
        clipRecord = Microphone.Start(deviceName, true, 1, 44100);
        if (clipRecord == null)
        {
            Debug.Log("Failed to start microphone");
            return;
        }

        // Send clip to audio source
        //microphoneAudioSource = GetComponent<AudioSource>();
        microphoneAudioSource.clip = clipRecord;
        microphoneAudioSource.loop = true;
        microphoneAudioSource.volume = 1;

        masterAudioSource.clip = clipRecord;
        masterAudioSource.loop = true;
        masterAudioSource.volume = 0.01f; // Can not be set to zero cause that will cut off the data stream


        microphoneAudioSource.outputAudioMixerGroup = microphoneAudioGroup;
        masterAudioSource.outputAudioMixerGroup = masterAudioGroup;

        // loop to clear buffer
        while (!(Microphone.GetPosition(null) > 0))
        {
        }

        microphoneAudioSource.Play();
        //masterAudioSource.Play();

        isInitialized = Microphone.IsRecording(null);
        //Debug.Log("Started. IsRecording:" + Microphone.IsRecording(null));
    }

    void StopMicrophone()
    {
        Microphone.End(deviceName);

        isInitialized = Microphone.IsRecording(null);
        //Debug.Log("Stopped. IsRecording:" + Microphone.IsRecording(null));
    }


    void OnEnable()
    {
        //Debug.Log("OnEnable:Start");
        StartMicrophone();

    }

    void OnDisable()
    {
        //Debug.Log("OnDisable:End");
        StopMicrophone();
    }

    void OnDestroy()
    {
        //Debug.Log("OnDestroy:End");
        StopMicrophone();

    }
    /// <summary>
    /// Caution!
    /// Will execute even the script is disabled.
    /// </summary>
    void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            //Debug.Log("OnFocus:Start");
            StartMicrophone();
        }

        else
        {
            //Debug.Log("OnLoseFocus:End");
            StopMicrophone();
        }
    }

    #endregion
    
}
