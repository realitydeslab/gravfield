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
        
    }

    #endregion


    #region Sender for live
    protected override void RegisterSender_ForLive()
    {
        string base_name = "rope";
        int rope_index = effectRope.ropeIndex;
        SenderForLive.Instance.RegisterOscPropertyToSend(GetFormatedOscAddress(base_name, rope_index, "vel"), ropevel);
        SenderForLive.Instance.RegisterOscPropertyToSend(GetFormatedOscAddress(base_name, rope_index, "acc"), ropeacc);
    }
    #endregion


    #region Sender for Coda
    protected override void RegisterSender_ForCoda()
    {

    }
    #endregion
}
