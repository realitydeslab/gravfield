using UnityEngine;

public class EffectRopeDispatcher : BaseDispatcher
{
    //*******************
    // BaseDispatcher has been made only for server
    //*******************


    // receiver
    [SerializeField]
    EffectRopeController effectRope;


    // sender for Live


    // sender for Coda

    void Awake()
    {
        effectRope = transform.GetComponent<EffectRopeController>();
        if (effectRope == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find EffectRopeController.");
        }
    }


    void Update()
    {
        if (isDispatching == false)
            return;

       
    }

    #region Receiver
    protected override void RegisterReceiver()
    {
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-mass", effectRope.mass, need_clamp: true, min_value: 10, max_value: 80);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-maxWidth", effectRope.maxWidth, need_clamp: true, min_value: 1, max_value: 100);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-scaler", effectRope.ropeScaler, need_clamp: true, min_value: 1, max_value: 20);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-offset-y", effectRope.ropeOffsetY);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-offset-z", effectRope.ropeOffsetZ);
    }

    #endregion


    #region Sender for live
    protected override void RegisterSender_ForLive()
    {
        
    }
    #endregion


    #region Sender for Coda
    protected override void RegisterSender_ForCoda()
    {

    }
    #endregion
}
