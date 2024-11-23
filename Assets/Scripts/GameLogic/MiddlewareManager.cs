using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class PerformerData
{
    public PerformerData(bool _is_performing, Vector3 _pos)
    {
        isPerforming = _is_performing;
        position = _pos;
    }

    public bool isPerforming;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
}


public class MiddlewareManager : MonoBehaviour
{
    [SerializeField]
    ParameterReceiver parameterReceiver;

    [SerializeField]
    PerfomerDataProcessor dataProcessor;

    [SerializeField]
    SenderForCoda senderForCoda;

    [SerializeField]
    SenderForLive senderForLive;

    //void Awake()
    //{
    //    parameterReceiver = FindObjectOfType<ParameterReceiver>();

    //    dataProcessor = FindObjectOfType<PerfomerDataProcessor>();

    //    senderForCoda = FindObjectOfType<SenderForCoda>();

    //    senderForLive = FindObjectOfType<SenderForLive>();
    //}

    //void OnEnable()
    //{
    //    roleManager?.OnSpecifyPlayerRoleEvent.AddListener(OnSpecifyPlayerRole);
    //}

    //void OnDisable()
    //{
    //    roleManager?.OnSpecifyPlayerRoleEvent.RemoveListener(OnSpecifyPlayerRole);
    //}

    void OnEnable()
    {
        GameManager.Instance.OnStartGame.AddListener(OnStartGame);
        GameManager.Instance.OnStopGame.AddListener(OnStopGame);
    }

    void OnDisable()
    {
        GameManager.Instance.OnStartGame.RemoveListener(OnStartGame);
        GameManager.Instance.OnStopGame.RemoveListener(OnStopGame);
    }

    void OnStartGame(PlayerRole role)
    {
        if (role == PlayerRole.Server)// && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            dataProcessor.TurnOn();

            senderForCoda.TurnOn();

            senderForLive.TurnOn();

            parameterReceiver.TurnOn();
        }
    }

    void OnStopGame(PlayerRole role)
    {
        if (role == PlayerRole.Server)
        {
            dataProcessor.TurnOff();

            senderForCoda.TurnOff();

            senderForLive.TurnOff();

            parameterReceiver.TurnOff();
        }
    }

    //void OnSpecifyPlayerRole(PlayerRole role)
    //{        
    //    if (role == PlayerRole.Server)// && Application.platform != RuntimePlatform.IPhonePlayer)
    //    {
    //        dataProcessor.TurnOn();

    //        senderForCoda.TurnOn();

    //        senderForLive.TurnOn();
    //    }
    //    else
    //    {
    //        dataProcessor.TurnOff();

    //        senderForCoda.TurnOff();

    //        senderForLive.TurnOff();
    //    }

    //    // A phone server might receive parameters from coda or others
    //    // ParameterReceiver has to be turned on at the end because it needs to add OscReceiver for parameters that were registed during previous TurnOn functions
    //    if (role == PlayerRole.Server)
    //    {
    //        parameterReceiver.TurnOn();
    //    }
    //    else
    //    {
    //        parameterReceiver.TurnOff();
    //    }
    //}



    public void TurnOn()
    {
        dataProcessor.TurnOn();

        senderForCoda.TurnOn();

        senderForLive.TurnOn();
    }

    public void TurnOff()
    {
        //if(roleManager.Role == PlayerRole.Server && Application.platform != RuntimePlatform.IPhonePlayer)
        //{
            dataProcessor.TurnOff();

            senderForCoda.TurnOff();

            senderForLive.TurnOff();

            parameterReceiver.TurnOff();
        //}
    }
}
