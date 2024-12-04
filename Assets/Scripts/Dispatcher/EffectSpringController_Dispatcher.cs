using UnityEngine;

public class EffectSpringController_Dispatcher : BaseDispatcher
{
    //*******************
    // BaseDispatcher has been made only for server
    //*******************


    // receiver
    EffectSpringController effectSpringController;


    // sender for Live



    // sender for Coda

    void Awake()
    {
        effectSpringController = transform.GetComponent<EffectSpringController>();
        if (effectSpringController == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find EffectSpringController.");
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
        string base_name = "/spring";



        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + "-wavelengthMax", effectSpringController.NV_WavelengthScalerMax, need_clamp: false, min_value: 1, max_value: 10);
        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + "-shakeSpeedMax", effectSpringController.NV_ShakeSpeedMax, need_clamp: false, min_value: 1, max_value: 20);
        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + "-shakeStrengthMax", effectSpringController.NV_ShakeStrengthMax, need_clamp: false, min_value: 0.01f, max_value: 1);
        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + "-waveWidthMax", effectSpringController.NV_WaveWidthMax, need_clamp: false, min_value: 0.03f, max_value: 0.5f);
        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + "-rotateSpeedMax", effectSpringController.NV_RotateSpeedMax, need_clamp: false, min_value: 10, max_value: 200);
        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + "-offsetY", effectSpringController.NV_SpringOffsetY, need_clamp: false, min_value: -1, max_value: 1);
        ParameterReceiver.Instance.RegisterOscReceiverFunction(base_name + "-offsetZ", effectSpringController.NV_SpringOffsetZ, need_clamp: false, min_value: 0, max_value: 1);
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
