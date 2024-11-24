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
    public Volume volume;
    Bloom bloom;

    public EffectRopeController effectRope;

    public EffectSpringController effectSpring;

    public EffectMagneticField effectMagneticField;

    //float effectMode = 0;

    HoloKitCameraManager holoKitCameraManager;

    // Mode
    public NetworkVariable<float> effectMode;


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

        //GameManager.Instance.RoleManager.OnSpecifyPlayerRoleEvent.AddListener(OnSpecifyPlayerRole);

        //GameManager.Instance.PerformerGroup.OnPerformerFinishSpawn.AddListener(OnPerformerFinishSpawn);

        holoKitCameraManager.OnScreenRenderModeChanged += OnScreenRenderModeChanged;
    }

    void OnDisable()
    {
        GameManager.Instance.OnStartGame.RemoveListener(OnStartGame);
        GameManager.Instance.OnStopGame.RemoveListener(OnStopGame);

        holoKitCameraManager.OnScreenRenderModeChanged -= OnScreenRenderModeChanged;

        //GameManager.Instance.RoleManager.OnSpecifyPlayerRoleEvent.RemoveListener(OnSpecifyPlayerRole);
    }

    void OnStartGame(PlayerRole player_role)
    {  
        //AssignLocalVariable();

        RegisterNetworkVariableCallback_Client();

        //ChangeEffectModeTo(GameManager.Instance.PerformerGroup.effectMode.Value);
        ChangeEffectModeTo(effectMode.Value);
    }

    void OnStopGame(PlayerRole player_role)
    {
        //ResetLocalVariable();

        UnregisterNetworkVariableCallback_Client();
    }

    //void Start()
    //{


    //    OnScreenRenderModeChanged(GameManager.Instance.HolokitCameraManager.ScreenRenderMode);
    //}

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



    //void OnSpecifyPlayerRole(PlayerRole role)
    //{
    //    ChangeEffectModeTo(GameManager.Instance.PerformerGroup.effectMode.Value);
    //}

    //void OnPerformerFinishSpawn()
    //{
    //    AssignLocalVariable();

    //    RegisterNetworkVariableCallback_Client();
    //}

    #region NetworkVariable / Clients also should execute
    //void AssignLocalVariable()
    //{
    //    effectMode = GameManager.Instance.PerformerGroup.effectMode.Value;
    //}

    //void ResetLocalVariable()
    //{
    //    effectMode = 0;
    //}

    void RegisterNetworkVariableCallback_Client()
    {
        //GameManager.Instance.PerformerGroup.effectMode.OnValueChanged += OnEffectModeChange;
        effectMode.OnValueChanged += OnEffectModeChange;
    }

    void UnregisterNetworkVariableCallback_Client()
    {
        //GameManager.Instance.PerformerGroup.effectMode.OnValueChanged -= OnEffectModeChange;
        effectMode.OnValueChanged -= OnEffectModeChange;
    }

    void OnEffectModeChange(float prev, float cur)
    {
        ChangeEffectModeTo(cur);
    }

    void ChangeEffectModeTo(float effect_index)
    {
        Debug.Log("ChangeEffectModeTo: " + effect_index);
        int effect_mode = Mathf.RoundToInt(effect_index);

        effectRope.SetEffectState(effect_mode == 0);

        effectSpring.SetEffectState(effect_mode == 1);

        effectMagneticField.SetEffectState(effect_mode == 2);
    }
    #endregion

}
