using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class EffectMagneticField : MonoBehaviour
{
    public Transform performerTransformRoot;

    List<Performer> performerList = new  List<Performer>();
    VisualEffect vfx;
    bool effectEnabled = false;

    // Output to LIVE
    AutoSwitchedParameter<float> magab = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magac = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magbc = new AutoSwitchedParameter<float>();

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
    void OnEnable()
    {
        GameManager.Instance.PerformerGroup.OnPerformerFinishSpawn.AddListener(OnPerformerFinishSpawn);
    }

    void OnPerformerFinishSpawn()
    {
        RegisterPropertiesToLive_Server();
    }


    void Update()
    {
        if (effectEnabled == false) return;

        UpdateVFX();

        UpdateParamtersForLive();
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


    #region Parameter sent to Live
    void RegisterPropertiesToLive_Server()
    {
        if (NetworkManager.Singleton.IsServer == false) return;

        SenderForLive.Instance.RegisterOscPropertyToSend("/magab", magab);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magac", magac);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magbc", magbc);
    }
    void UpdateParamtersForLive()
    {
        magab.OrginalValue = CalculateMag(performerList[0], performerList[1]);
        magac.OrginalValue = CalculateMag(performerList[0], performerList[2]);
        magbc.OrginalValue = CalculateMag(performerList[1], performerList[2]);

    }
    float CalculateMag(Performer start, Performer end)
    {
        float mag = Vector3.Distance(start.localData.position, end.localData.position);
        mag *= (start.localData.positive != end.localData.positive) ? -1f : 1f;

        return mag;
        
    }
    #endregion
}
