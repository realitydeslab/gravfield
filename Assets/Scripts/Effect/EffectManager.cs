using System;
using System.Collections;
using System.Collections.Generic;
using HoloKit;
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
    
    HoloKitCameraManager holoKitCameraManager;

    void Awake()
    {
        holoKitCameraManager = FindFirstObjectByType<HoloKitCameraManager>();
        if (holoKitCameraManager == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find HoloKitCameraManager.");
        }
    }

    void OnEnable()
    {
        GameManager.Instance.OnStartGame.AddListener(OnStartGame);
        GameManager.Instance.OnStopGame.AddListener(OnStopGame);
    }

    void OnDisable()
    {
        GameManager.Instance.OnStartGame.RemoveListener(OnStartGame);
        GameManager.Instance.OnStopGame.RemoveListener(OnStopGame);
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

        holoKitCameraManager.OnScreenRenderModeChanged += OnScreenRenderModeChanged;
    }

    void UnregisterNetworkVariableCallback()
    {
        effectMode.OnValueChanged -= OnEffectModeChange;

        holoKitCameraManager.OnScreenRenderModeChanged -= OnScreenRenderModeChanged;
    }
    void OnEffectModeChange(float prev, float cur)
    {
        ChangeEffectModeTo(cur);
    }
    #endregion


    void ChangeEffectModeTo(float effect_index)
    {
        Debug.Log("ChangeEffectModeTo: " + effect_index);
        int effect_mode = Mathf.RoundToInt(effect_index);

        effectRope.SetEffectState(effect_mode == 0);

        effectSpring.SetEffectState(effect_mode == 1);

        effectMagneticField.SetEffectState(effect_mode == 2);
    }

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
}
