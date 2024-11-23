using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;
using UnityEngine.Events;
using Unity.Netcode;

public class SenderForCoda : MonoBehaviour
{
    [SerializeField] OscConnection senderConnection = null;
    
    private bool connectedWithCoda = false;
    public bool ConnectedWithCoda {
        get
        {
            if (transSender == null || transSender.gameObject.activeSelf == false)
                return false;
            else
                return connectedWithCoda;
        }
    }

    Transform transSender;

    List<OscPropertySenderModified> sernderList;

    void Awake()
    {
        transSender = transform.Find("Sender");

        sernderList = new List<OscPropertySenderModified>(transSender.GetComponents<OscPropertySenderModified>());
    }

    void Start()
    {
        SetSenderState(false);
    }

    public void TurnOn()
    {
        Debug.Log($"[{this.GetType()}] TurnOn");

        SetSenderState(true);
    }

    public void TurnOff()
    {
        Debug.Log($"[{this.GetType()}] TurnOff");

        SetSenderState(false);
    }

    void SetSenderState(bool state)
    {
        if (transSender == null)
            transSender = transform.Find("Sender");

        transSender.gameObject.SetActive(state);
    }


    void Update()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (transSender == null || transSender.gameObject.activeSelf == false)
            return;

        bool successfully_send = true;
        foreach (OscPropertySenderModified sender in sernderList)
        {
            successfully_send = successfully_send & sender.successfullySend;
        }

        connectedWithCoda = successfully_send;
    }
}
