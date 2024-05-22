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

    RoleManager roleManager;

    ParameterReceiver parameterReceiver;

    PerfomerDataProcessor dataProcessor;

    SenderForCoda senderForCoda;

    SenderForLive senderForLive;

    void Awake()
    {
        roleManager = FindObjectOfType<RoleManager>();

        parameterReceiver = FindObjectOfType<ParameterReceiver>();

        dataProcessor = FindObjectOfType<PerfomerDataProcessor>();

        senderForCoda = FindObjectOfType<SenderForCoda>();

        senderForLive = FindObjectOfType<SenderForLive>();
    }

    void OnEnable()
    {
        roleManager?.OnSpecifyPlayerRoleEvent.AddListener(OnSpecifyPlayerRole);
    }

    void OnDisable()
    {
        roleManager?.OnSpecifyPlayerRoleEvent.RemoveListener(OnSpecifyPlayerRole);
    }

    void OnSpecifyPlayerRole(RoleManager.PlayerRole role)
    {        
        // A phone server can not send data to coda or live
        if (role == RoleManager.PlayerRole.Server && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            dataProcessor.TurnOn();

            senderForCoda.TurnOn();

            senderForLive.TurnOn();
        }
        else
        {
            dataProcessor.TurnOff();

            senderForCoda.TurnOff();

            senderForLive.TurnOff();
        }

        // A phone server might receive parameters from coda or others
        // ParameterReceiver has to be turned on at the end because it needs to add OscReceiver for parameters that were registed during previous TurnOn functions
        if (role == RoleManager.PlayerRole.Server)
        {
            parameterReceiver.TurnOn();
        }
        else
        {
            parameterReceiver.TurnOff();
        }
    }
}
