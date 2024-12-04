using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EffectMagneticField))]
public class EffectMagneticField_Dispatcher : BaseDispatcher
{
    //*******************
    // BaseDispatcher has been made only for server
    //*******************


    // receiver
    EffectMagneticField effectMagneticField;


    // sender for Live
    AutoSwitchedParameter<float> magabpos = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magabneg = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magacpos = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magacneg = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magbcpos = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> magbcneg = new AutoSwitchedParameter<float>();


    // sender for Coda

    void Awake()
    {
        effectMagneticField = transform.GetComponent<EffectMagneticField>();
        if (effectMagneticField == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find EffectMagneticField.");
        }
    }


    void Update()
    {
        if (isDispatching == false)
            return;

        // update data sent to live
        UpdateParamtersForLive();
    }

    void UpdateParamtersForLive()
    {
        List<Performer> performerList = GameManager.Instance.PlayerManager.PerformerList;

        bool is_positive = IsRepelling(effectMagneticField.NV_Magnetic0.Value, effectMagneticField.NV_Magnetic1.Value);
        if (is_positive)
            magabpos.OrginalValue = CalculateMag(performerList[0], performerList[1]);
        else
            magabneg.OrginalValue = CalculateMag(performerList[0], performerList[1]);

        is_positive = IsRepelling(effectMagneticField.NV_Magnetic0.Value, effectMagneticField.NV_Magnetic2.Value);
        if (is_positive)
            magacpos.OrginalValue = CalculateMag(performerList[0], performerList[2]);
        else
            magacneg.OrginalValue = CalculateMag(performerList[0], performerList[2]);


        is_positive = IsRepelling(effectMagneticField.NV_Magnetic1.Value, effectMagneticField.NV_Magnetic2.Value);
        if (is_positive)
            magbcpos.OrginalValue = CalculateMag(performerList[1], performerList[2]);
        else
            magbcneg.OrginalValue = CalculateMag(performerList[1], performerList[2]);
    }

    bool IsRepelling(float start, float end)
    {
        bool is_start_positive = start > 0.5f ? true : false;
        bool is_end_positive = end > 0.5f ? true : false;

        return !(is_start_positive != is_end_positive);
    }

    float CalculateMag(Performer start, Performer end)
    {
        float mag = Vector3.Distance(start.localData.position, end.localData.position);
        //mag *= (start.localData.positive != end.localData.positive) ? -1f : 1f;

        return mag;
    }

    #region Receiver
    protected override void RegisterReceiver()
    {
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/mag0", effectMagneticField.NV_Magnetic0);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/mag1", effectMagneticField.NV_Magnetic1);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/mag2", effectMagneticField.NV_Magnetic2);
    }

    #endregion


    #region Sender for live
    protected override void RegisterSender_ForLive()
    {
        SenderForLive.Instance.RegisterOscPropertyToSend("/magabpos", magabpos);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magabneg", magabneg);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magacpos", magacpos);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magacneg", magacneg);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magbcpos", magbcpos);
        SenderForLive.Instance.RegisterOscPropertyToSend("/magbcneg", magbcneg);
    }
    #endregion


    #region Sender for Coda
    protected override void RegisterSender_ForCoda()
    {

    }
    #endregion
}
