using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EffectMagneticField : MonoBehaviour
{
    //private bool isPerformingA = false;
    //public bool IsPerformingA { get => isPerformingA; }

    //private bool isPerformingB = false;
    //public bool IsPerformingB { get => isPerformingB; }

    //private bool isPerformingC = false;
    //public bool IsPerformingC { get => isPerformingC; }

    //private bool magneticA = true;
    //public bool MagneticA { get => magneticA; }

    //private bool magneticB = false;
    //public bool MagneticB { get => magneticB; }

    //private bool magneticC = true;
    //public bool MagneticC { get => magneticC; }

    public Transform performerTransformRoot;

    List<Performer> performerList = new  List<Performer>();
    VisualEffect vfx;
    bool effectEnabled = false;

    void Awake()
    {
        vfx = transform.GetComponentInChildren<VisualEffect>();

        for(int i=0; i< performerTransformRoot.childCount; i++)
        {
            performerList.Add(performerTransformRoot.GetChild(i).GetComponent<Performer>());
        }
        
    }
    void Start()
    {
        
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
        vfx.SetBool("IsPerformingA", performerList[0].localData.isPerforming);
        vfx.SetBool("IsPerformingB", performerList[1].localData.isPerforming);
        vfx.SetBool("IsPerformingC", performerList[2].localData.isPerforming);

        vfx.SetBool("MagneticA", performerList[0].localData.positive);
        vfx.SetBool("MagneticB", performerList[1].localData.positive);
        vfx.SetBool("MagneticC", performerList[2].localData.positive);
    }
}
