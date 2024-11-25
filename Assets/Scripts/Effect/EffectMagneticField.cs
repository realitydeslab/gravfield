using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;


public class EffectMagneticField : MonoBehaviour
{
    public NetworkVariable<float> NV_Magnetic0;
    public NetworkVariable<float> NV_Magnetic1;
    public NetworkVariable<float> NV_Magnetic2;


    List<Performer> performerList = new  List<Performer>();
    [SerializeField] float headOffsetY;
    VisualEffect vfx;
    bool effectEnabled = false;


    void Awake()
    {
        vfx = transform.GetComponent<VisualEffect>();
        //transform.GetComponentInChildren<VFXPropertyBinder>().enabled = false;

        Transform performerTransformRoot = GameManager.Instance.PlayerManager.PerformerTransformRoot;
        for (int i=0; i< performerTransformRoot.childCount; i++)
        {
            performerList.Add(performerTransformRoot.GetChild(i).GetComponent<Performer>());
        }        
    }
    

    void Update()
    {
        if (effectEnabled == false) return;

        UpdateVFX();
    }

    public void SetEffectState(bool state)
    {
        effectEnabled = state;
        vfx.enabled = state;
    }

    void UpdateVFX()
    {
        //vfx.SetVector3("PerformerA" + "_position", performerList[0].localData.position + new Vector3(0, headOffsetY, 0));
        //vfx.SetVector3("PerformerB" + "_position", performerList[1].localData.position + new Vector3(0, headOffsetY, 0));
        //vfx.SetVector3("PerformerC" + "_position", performerList[2].localData.position + new Vector3(0, headOffsetY, 0));


        vfx.SetBool("IsPerformingA", performerList[0].localData.isPerforming);
        vfx.SetBool("IsPerformingB", performerList[1].localData.isPerforming);
        vfx.SetBool("IsPerformingC", performerList[2].localData.isPerforming);

        vfx.SetBool("MagneticA", IsPositive(NV_Magnetic0.Value));
        vfx.SetBool("MagneticB", IsPositive(NV_Magnetic1.Value));
        vfx.SetBool("MagneticC", IsPositive(NV_Magnetic2.Value));
    }

    bool IsPositive(float magnetic_value)
    {
        return magnetic_value > 0.5 ? true : false;
    }
}
