using UnityEngine;

[RequireComponent(typeof(EffectRope))]
public class EffectRope_Dispatcher : BaseDispatcher
{
    //*******************
    // BaseDispatcher has been made only for server
    //*******************


    // receiver
    EffectRope effectRope;


    // sender for Live
    AutoSwitchedParameter<float> ropevel = new AutoSwitchedParameter<float>();
    AutoSwitchedParameter<float> ropeacc = new AutoSwitchedParameter<float>();


    // sender for Coda

    void Awake()
    {
        effectRope = transform.GetComponent<EffectRope>();
        if (effectRope == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find EffectRope.");
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
        ropevel.OrginalValue = effectRope.centroidVel.magnitude;
        ropeacc.OrginalValue = effectRope.centroidAcc.magnitude;
    }

    #region Receiver
    protected override void RegisterReceiver()
    {
        string base_name = "/rope";
        string rope_index = effectRope.RopeIndex.ToString();

        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + rope_index + "-mass", effectRope.NV_Mass, need_clamp: false, min_value: 10, max_value: 80);
        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + rope_index + "-maxWidth", effectRope.NV_MaxWidth, need_clamp: false, min_value: 1, max_value: 100);
        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + rope_index + "-scaler", effectRope.NV_RopeScaler, need_clamp: false, min_value: 1, max_value: 20);
        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + rope_index + "-offset-y", effectRope.NV_RopeOffsetY, need_clamp: false, min_value: -1, max_value: 1);
        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + rope_index + "-offset-z", effectRope.NV_RopeOffsetZ, need_clamp: false, min_value: -1, max_value: 1);
    }

    #endregion


    #region Sender for live
    protected override void RegisterSender_ForLive()
    {
        string base_name = "/rope";
        string rope_index = effectRope.RopeIndex.ToString();
        SenderForLive.Instance.RegisterOscPropertyToSend(base_name + rope_index + "vel", ropevel);
        SenderForLive.Instance.RegisterOscPropertyToSend(base_name + rope_index + "acc", ropeacc);
    }
    #endregion


    #region Sender for Coda
    protected override void RegisterSender_ForCoda()
    {

    }
    #endregion
}
