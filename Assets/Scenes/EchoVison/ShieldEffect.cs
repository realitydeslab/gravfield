using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShieldEffect : MonoBehaviour
{
    public PerformerGroup performerGroup;
    public Material shieldMat;

    float meshy;
    float meshnoise;
    float meshsize;


    void Start()
    {
        RegisterNetworkVariableCallback();
    }


    #region NetworkVariable
    void RegisterNetworkVariableCallback()
    {
        performerGroup.meshy.OnValueChanged += (float prev, float cur) => { meshy = cur; UpdateShieldEffect(); };
        performerGroup.meshnoise.OnValueChanged += (float prev, float cur) => { meshnoise = cur; UpdateShieldEffect(); };
        performerGroup.meshsize.OnValueChanged += (float prev, float cur) => { meshsize = cur; UpdateShieldEffect(); };
    }

    void UpdateShieldEffect()
    {
        if (performerGroup == null || performerGroup.IsSpawned == false)
            return;

        transform.localPosition = new Vector3(0, meshy, 0);

        shieldMat.SetFloat("_DisplacementNoiseScale", Mathf.Min(4, meshnoise));

        shieldMat.SetFloat("_DisplaceStrength", Mathf.Min(0.28f, meshsize));
    }
    #endregion
}
