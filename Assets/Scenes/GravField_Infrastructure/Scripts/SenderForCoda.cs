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
    public bool ConnectedWithCoda { get => connectedWithCoda; }

    Transform transSender;

    List<OscPropertySenderModified> sernderList;


    void Awake()
    {
        transSender = transform.Find("Sender");

        sernderList = new List<OscPropertySenderModified>(transSender.GetComponents<OscPropertySenderModified>());
    }


    public void TurnOn()
    {
        Debug.Log("SenderForCoda | TurnOn");

        if (transSender == null)
            transSender = transform.Find("Sender");

        transSender.gameObject.SetActive(true);
    }

    public void TurnOff()
    {
        Debug.Log("SenderForCoda | TurnOff");

        if (transSender == null)
            transSender = transform.Find("Sender");

        transSender.gameObject.SetActive(false);
    }


    void Update()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        bool successfully_send = true;
        foreach (OscPropertySenderModified sender in sernderList)
        {
            successfully_send = successfully_send & sender.successfullySend;
        }

        connectedWithCoda = successfully_send;
    }




    //void RegisterPropertyForReceiving()
    //{
    //    propertiesForReceiving.Clear();
    //    propertiesForReceiving.Add(new OscPropertyForReceiving("/floatvalue1", new UnityAction<float>(OnReceiveGroup_FloatValue1)));
    //    propertiesForReceiving.Add(new OscPropertyForReceiving("/floatvalue2", new UnityAction<float>(OnReceiveGroup_FloatValue2)));
    //    propertiesForReceiving.Add(new OscPropertyForReceiving("/floatvalue3", new UnityAction<float>(OnReceiveGroup_FloatValue3)));
    //    propertiesForReceiving.Add(new OscPropertyForReceiving("/floatvalue4", new UnityAction<float>(OnReceiveGroup_FloatValue4)));
    //    propertiesForReceiving.Add(new OscPropertyForReceiving("/floatvalue5", new UnityAction<float>(OnReceiveGroup_FloatValue5)));

    //    propertiesForReceiving.Add(new OscPropertyForReceiving("/vector3value1", new UnityAction<Vector3>(OnReceiveGroup_Vector3Value1)));
    //    propertiesForReceiving.Add(new OscPropertyForReceiving("/vector3value2", new UnityAction<Vector3>(OnReceiveGroup_Vector3Value2)));
    //}
    //public void OnReceiveGroup_FloatValue1(float v)
    //{
    //    Debug.Log("OnReceiveGroup_FloatValue1:" + v.ToString());
    //    if (NetworkManager.Singleton.IsServer)
    //    {
    //        // Change NetworkVariables
    //    }
    //}
    //public void OnReceiveGroup_FloatValue2(float v)
    //{

    //}
    //public void OnReceiveGroup_FloatValue3(float v)
    //{

    //}
    //public void OnReceiveGroup_FloatValue4(float v)
    //{

    //}
    //public void OnReceiveGroup_FloatValue5(float v)
    //{

    //}

    //public void OnReceiveGroup_Vector3Value1(Vector3 v)
    //{

    //}
    //public void OnReceiveGroup_Vector3Value2(Vector3 v)
    //{

    //}



    //[ContextMenu("SetSenderConnection")]
    //public void SetSenderConnection()
    //{
    //    if (transSender == null)
    //    {
    //        transSender = transform.Find("Sender");
    //    }
    //    OscPropertySenderModified[] senders = transSender.gameObject.GetComponents<OscPropertySenderModified>();
    //    foreach (OscPropertySenderModified sender in senders)
    //    {
    //        sender._connection = senderConnection;
    //    }
    //}
    #region Instance
    private static SenderForCoda _Instance;

    public static SenderForCoda Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<SenderForCoda>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    _Instance = go.AddComponent<SenderForCoda>();
                }
            }
            return _Instance;
        }
    }
    #endregion
}
