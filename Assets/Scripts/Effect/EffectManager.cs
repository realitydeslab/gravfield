using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS
using HoloKit;
#endif
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EffectManager : NetworkBehaviour
{
    // Network Variables
    public NetworkVariable<float> effectMode;


    // Effects
    [Header("Effects")]
    [SerializeField]
    EffectRopeController effectRope;

    [SerializeField]
    EffectSpringController effectSpring;

    [SerializeField]
    EffectMagneticField effectMagneticField;

    [SerializeField]
    Volume volume;
    Bloom bloom;

#if UNITY_IOS
    HoloKitCameraManager holoKitCameraManager;
#endif

    void Awake()
    {
#if UNITY_IOS
        holoKitCameraManager = FindFirstObjectByType<HoloKitCameraManager>();
        if (holoKitCameraManager == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find HoloKitCameraManager.");
        }
#endif
    }

    void OnEnable()
    {
        GameManager.Instance.OnStartGame.AddListener(OnStartGame);
        GameManager.Instance.OnStopGame.AddListener(OnStopGame);
    }

    void OnDisable()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnStartGame.RemoveListener(OnStartGame);
            GameManager.Instance.OnStopGame.RemoveListener(OnStopGame);
        }        
    }

    void Update()
    {
        if (IsServer)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    effectMode.Value = i;
                }
            }
        }
    }

#region Start / Stop game 
    void OnStartGame(PlayerRole player_role)
    {
        // Register NetworkVariable functions
        RegisterNetworkVariableCallback();

        // Start
        ChangeEffectModeTo(effectMode.Value);
    }

    void OnStopGame(PlayerRole player_role)
    {
        // Unregister NetworkVariable functions
        UnregisterNetworkVariableCallback();
    }
#endregion

#region NetworkVariable 
    void RegisterNetworkVariableCallback()
    {
        effectMode.OnValueChanged += OnEffectModeChange;
#if UNITY_IOS
        holoKitCameraManager.OnScreenRenderModeChanged += OnScreenRenderModeChanged;
#endif
    }

    void UnregisterNetworkVariableCallback()
    {
        effectMode.OnValueChanged -= OnEffectModeChange;

#if UNITY_IOS
        holoKitCameraManager.OnScreenRenderModeChanged -= OnScreenRenderModeChanged;
#endif
    }
    void OnEffectModeChange(float prev, float cur)
    {
        ChangeEffectModeTo(cur);
    }
#endregion


    public void ChangeToPreviousEffect_UI()
    {
        if (IsServer == false)
            return;
        int index = (int)effectMode.Value - 1;
        if (index < 0) index = 2;
        ChangeEffect_UI(index);
    }

    public void ChangeToNextEffect_UI()
    {
        if (IsServer == false)
            return;
        int index = (int)effectMode.Value + 1;
        if (index > 2) index = 0;
        ChangeEffect_UI(index);
    }

    public void ChangeEffect_UI(int index)
    {
        if (IsServer == false)
            return;

        effectMode.Value = index;
    }

    void ChangeEffectModeTo(float effect_index)
    {
        Debug.Log("ChangeEffectModeTo: " + effect_index);
        int effect_mode = Mathf.RoundToInt(effect_index);

        effectRope.SetEffectState(effect_mode == 0);

        effectSpring.SetEffectState(effect_mode == 1);

        effectMagneticField.SetEffectState(effect_mode == 2);
    }
#if UNITY_IOS
    void OnScreenRenderModeChanged(HoloKit.ScreenRenderMode mode)
    {
        VolumeProfile profile = volume.sharedProfile;
        profile.TryGet<Bloom>(out bloom);
#if UNITY_EDITOR
        //bloom.active = true;
        //bloom.intensity.value = 1.93f;
#else
        if (mode == HoloKit.ScreenRenderMode.Mono)
        {
            //bloom.int = false;
            bloom.intensity.value = 0.93f;
        }
        else
        {
            //bloom.active = true;
            bloom.intensity.value = 1.93f;
        }
#endif
    }
#endif
}
