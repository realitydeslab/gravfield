using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShieldEffect : MonoBehaviour
{
    public PerformerGroup performerGroup;
    public Material shieldMat;

    AutoSwitchedParameter<float> meshy = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> meshnoise = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> meshsize = new AutoSwitchedParameter<float>();


    void Start()
    {
        meshy.OrginalValue = 4;
        meshnoise.OrginalValue = 4;
        meshsize.OrginalValue = 0.28f;
    }
    void Update()
    {
        if ((NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient)
            || (NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening))
        {
            UpdateShieldEffect();
        }
    }

    void UpdateShieldEffect()
    {
        if (performerGroup == null || performerGroup.IsSpawned == false)
            return;

        meshy.CodaValue = performerGroup.floatValue1.Value;
        meshnoise.CodaValue = performerGroup.floatValue2.Value;
        meshsize.CodaValue = performerGroup.floatValue3.Value;


        transform.localPosition = new Vector3(0, meshy.Value, 0);

        shieldMat.SetFloat("_DisplacementNoiseScale", 4 + meshnoise.Value);

        shieldMat.SetFloat("_DisplaceStrength", 0.28f + meshsize.Value);

        
    }
}
