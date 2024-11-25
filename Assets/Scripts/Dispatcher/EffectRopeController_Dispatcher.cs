using UnityEngine;

public class EffectRopeController_Dispatcher : BaseDispatcher
{
    //*******************
    // BaseDispatcher has been made only for server
    //*******************


    // receiver
    EffectRopeController effectRopeController;


    // sender for Live


    // sender for Coda

    void Awake()
    {
        effectRopeController = transform.GetComponent<EffectRopeController>();
        if (effectRopeController == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find EffectRopeController.");
        }
    }


    //void Update()
    //{
    //    if (isDispatching == false)
    //        return;
    //}


    #region Receiver
    protected override void RegisterReceiver()
    {
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-mass", effectRopeController.mass, need_clamp: true, min_value: 10, max_value: 80);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-maxWidth", effectRopeController.maxWidth, need_clamp: true, min_value: 1, max_value: 100);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-scaler", effectRopeController.ropeScaler, need_clamp: true, min_value: 1, max_value: 20);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-offset-y", effectRopeController.ropeOffsetY);
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/rope-offset-z", effectRopeController.ropeOffsetZ);
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
