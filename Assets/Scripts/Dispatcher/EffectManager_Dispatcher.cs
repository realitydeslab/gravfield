using UnityEngine;
using UnityEngine.Events;

public class EffectManager_Dispatcher : BaseDispatcher
{
    //*******************
    // BaseDispatcher has been made only for server
    //*******************


    // receiver
    [SerializeField]
    EffectManager effectManager;


    // sender for Live
    public AutoSwitchedParameter<float> mode0 = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> mode1 = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> mode2 = new AutoSwitchedParameter<float>();

    // sender for Coda



    void Update()
    {
        if (isDispatching == false)
            return;

        mode0.OrginalValue = effectManager.effectMode.Value == 0 ? 1 : 0;
        mode1.OrginalValue = effectManager.effectMode.Value == 1 ? 1 : 0;
        mode2.OrginalValue = effectManager.effectMode.Value == 2 ? 1 : 0;
    }

    #region Receiver
    protected override void RegisterReceiver()
    {
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/mode", (v)=> { effectManager.effectMode.Value = v; });
    }

    #endregion


    #region Sender for live
    protected override void RegisterSender_ForLive()
    {
        SenderForLive.Instance.RegisterOscPropertyToSend("/mode0", mode0);
        SenderForLive.Instance.RegisterOscPropertyToSend("/mode1", mode1);
        SenderForLive.Instance.RegisterOscPropertyToSend("/mode2", mode2);
    }
    #endregion


    #region Sender for Coda
    protected override void RegisterSender_ForCoda()
    {

    }
    #endregion

}
