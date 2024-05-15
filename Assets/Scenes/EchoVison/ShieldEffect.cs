using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShieldEffect : MonoBehaviour
{
    public PerformerGroup performerGroup;
    public Material shieldMat;

    // Update is called once per frame
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

        float mesh_y = performerGroup.floatValue1.Value;
        float mesh_noise = performerGroup.floatValue2.Value;
        float mesh_size = performerGroup.floatValue3.Value;


        transform.localPosition = new Vector3(0, 4 + mesh_y, 0);

        shieldMat.SetFloat("_DisplacementNoiseScale", 4 + mesh_noise);

        shieldMat.SetFloat("_DisplaceStrength", 0.28f + mesh_size);

        
    }
}
