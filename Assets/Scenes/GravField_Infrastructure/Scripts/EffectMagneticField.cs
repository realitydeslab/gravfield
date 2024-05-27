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
    AutoSwitchedParameter<float> magabpos = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magabneg = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magacpos = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magacneg = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magbcpos = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magbcneg = new AutoSwitchedParameter<float>();

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

        vfx.SetBool("IsPositiveA", performerList[0].localData.positive);
        vfx.SetBool("IsPositiveB", performerList[1].localData.positive);
        vfx.SetBool("IsPositiveC", performerList[2].localData.positive);
    }


    #region Parameter sent to Live
    void RegisterPropertiesToLive_Server()
    {
        if (NetworkManager.Singleton.IsServer == false) return;

        SenderForLive.Instance.RegisterOscPropertyToSend("/magabpos", magabpos);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magabneg", magabneg);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magacpos", magacpos);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magacneg", magacneg);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magbcpos", magbcpos);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magbcneg", magbcneg);
    }
    void UpdateParamtersForLive()
    {
        bool is_positive = IsPositive(performerList[0], performerList[1]);
        if (is_positive)
            magabpos.OrginalValue = CalculateMag(performerList[0], performerList[1]);
        else
            magabneg.OrginalValue = CalculateMag(performerList[0], performerList[1]);

        is_positive = IsPositive(performerList[0], performerList[2]);
        if (is_positive)
            magacpos.OrginalValue = CalculateMag(performerList[0], performerList[2]);
        else
            magacneg.OrginalValue = CalculateMag(performerList[0], performerList[2]);


        is_positive = IsPositive(performerList[1], performerList[2]);
        if (is_positive)
            magbcpos.OrginalValue = CalculateMag(performerList[1], performerList[2]);
        else
            magbcneg.OrginalValue = CalculateMag(performerList[1], performerList[2]);


    }
    bool IsPositive(Performer start, Performer end)
    {
        return !(start.localData.positive != end.localData.positive);
    }

    float CalculateMag(Performer start, Performer end)
    {
        float mag = Vector3.Distance(start.localData.position, end.localData.position);
        //mag *= (start.localData.positive != end.localData.positive) ? -1f : 1f;

        return mag;
        
    }
    #endregion
}
