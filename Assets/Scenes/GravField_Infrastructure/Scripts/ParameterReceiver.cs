using System.Collections;
using System.Collections.Generic;
using OscJack;
using UnityEngine;
using UnityEngine.Events;

public class ParameterReceiver : MonoBehaviour
{
    [SerializeField] OscConnection receiverConnection = null;

    List<OscPropertyForReceiving> propertiesForReceiving = new List<OscPropertyForReceiving>();

    Transform transReceiver;

    public void RegisterOscReceiverFunction(string address, UnityAction<float> action)
    {
        propertiesForReceiving.Add(new OscPropertyForReceiving(address, action));
    }
    public void RegisterOscReceiverFunction(string address, UnityAction<Vector3> action)
    {
        propertiesForReceiving.Add(new OscPropertyForReceiving(address, action));
    }
    public void RegisterOscReceiverFunction(string address, UnityAction<int> action)
    {
        propertiesForReceiving.Add(new OscPropertyForReceiving(address, action));
    }
    public void RegisterOscReceiverFunction(string address, UnityAction<string> action)
    {
        propertiesForReceiving.Add(new OscPropertyForReceiving(address, action));
    }

    public void TurnOn()
    {
        Debug.Log("ParameterReceiver | TurnOn");

        InitializeAllOscReceiver();
    }

    public void TurnOff()
    {
        Debug.Log("ParameterReceiver | TurnOff");

        RemoveAllOscReceiver();
    }

    public string[] GetAllParameterAddresses()
    {
        if (propertiesForReceiving == null)
            return null;

        string[] addressed = new string[propertiesForReceiving.Count];

        for(int i=0; i< addressed.Length; i++)
        {
            addressed[i] = propertiesForReceiving[i].oscAddress;
        }

        return addressed;
    }

    void InitializeAllOscReceiver()
    {
        if(transReceiver == null)
        {
            transReceiver = transform.Find("Receiver");
        }

        Debug.Log("ParameterReceiver | InitializeAllOscReceiver");
        for (int i = 0; i < propertiesForReceiving.Count; i++)
        {
            OscPropertyForReceiving property = propertiesForReceiving[i];
            AddReceiverComponent(property);
        }
    }

    void RemoveAllOscReceiver()
    {
        if (transReceiver == null)
        {
            transReceiver = transform.Find("Receiver");
        }

        Debug.Log("ParameterReceiver | RemoveAllOscReceiver");
        foreach (OscEventReceiverModified receiver in transReceiver.GetComponents<OscEventReceiverModified>())
        {
            Destroy(receiver);
        }
    }

    void AddReceiverComponent(OscPropertyForReceiving property)
    {
        if (transReceiver == null)
        {
            transReceiver = transform.Find("Receiver");
        }

       
        OscEventReceiverModified receiver = transReceiver.gameObject.AddComponent<OscEventReceiverModified>();
        receiver._connection = receiverConnection;
        receiver._oscAddress = property.oscAddress;

        if (property.dataType == typeof(float))
        {
            receiver._dataType = OscEventReceiverModified.DataType.Float;
            receiver._floatEvent = new OscEventReceiverModified.FloatEvent();
            receiver._floatEvent.AddListener(property.floatAction);
            receiver.Initialize();
        }
        else if (property.dataType == typeof(Vector3))
        {
            receiver._dataType = OscEventReceiverModified.DataType.Vector3;
            receiver._vector3Event = new OscEventReceiverModified.Vector3Event();
            receiver._vector3Event.AddListener(property.vector3Action);
            receiver.Initialize();
        }
        else if (property.dataType == typeof(int))
        {
            receiver._dataType = OscEventReceiverModified.DataType.Int;
            receiver._intEvent = new OscEventReceiverModified.IntEvent();
            receiver._intEvent.AddListener(property.intAction);
            receiver.Initialize();
        }
        else if (property.dataType == typeof(string))
        {
            receiver._dataType = OscEventReceiverModified.DataType.String;
            receiver._stringEvent = new OscEventReceiverModified.StringEvent();
            receiver._stringEvent.AddListener(property.stringAction);
            receiver.Initialize();
        }
    }



    //[ContextMenu("SetReceiverConnection")]
    //public void SetReceiverConnection()
    //{
    //    if (transReceiver == null)
    //    {
    //        transReceiver = transform.Find("Receiver");
    //    }
    //    OscEventReceiverModified[] receivers = transReceiver.gameObject.GetComponents<OscEventReceiverModified>();
    //    foreach (OscEventReceiverModified receiver in receivers)
    //    {
    //        receiver._connection = receiverConnection;
    //    }
    //}

    #region Instance
    private static ParameterReceiver _Instance;

    public static ParameterReceiver Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<ParameterReceiver>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    _Instance = go.AddComponent<ParameterReceiver>();
                }
            }
            return _Instance;
        }
    }
    #endregion
}
