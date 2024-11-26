using System.Collections;
using System.Collections.Generic;
using OscJack;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ParameterReceiver : MonoBehaviour
{
    [SerializeField] OscConnection receiverConnection = null;


    List<OscPropertyForReceiving> propertiesForReceiving = new List<OscPropertyForReceiving>();

    public List<OscPropertyForReceiving> PropertiesForReceiving { get => propertiesForReceiving; }

    Transform transReceiver;

    public void RegisterOscReceiverFunction(string address, UnityAction<float> action, bool need_clamp = false, float min_value = 0, float max_value = 1)
    {
        OscPropertyForReceiving property = new OscPropertyForReceiving(address, action, need_clamp, min_value, max_value);

        RegisterOscReceiverFunction(property);
    }

    public void RegisterOscReceiverFunction(string address, NetworkVariable<float> param, bool need_clamp = false, float min_value = 0, float max_value = 1)
    {
        OscPropertyForReceiving property = new OscPropertyForReceiving(address, new UnityAction<float>((v)=> { param.Value = v; }), need_clamp, min_value, max_value);

        RegisterOscReceiverFunction(property);
    }

    public void RegisterOscReceiverFunction(OscPropertyForReceiving property)
    {
        propertiesForReceiving.Add(property);

        AddReceiverComponent(property);

        ControlPanel.Instance.AddProperyInControlPanel_ServerMode(property);
    }
    //public void RegisterOscReceiverFunction(string address, UnityAction<Vector3> action)
    //{
    //    OscPropertyForReceiving property = new OscPropertyForReceiving(address, action);

    //    propertiesForReceiving.Add(property);

    //    AddReceiverComponent(property);
    //}
    //public void RegisterOscReceiverFunction(string address, UnityAction<int> action)
    //{
    //    OscPropertyForReceiving property = new OscPropertyForReceiving(address, action);

    //    propertiesForReceiving.Add(property);

    //    AddReceiverComponent(property);
    //}
    //public void RegisterOscReceiverFunction(string address, UnityAction<string> action)
    //{
    //    OscPropertyForReceiving property = new OscPropertyForReceiving(address, action);

    //    propertiesForReceiving.Add(property);

    //    AddReceiverComponent(property);
    //}


    public void TurnOn()
    {
        Debug.Log($"[{this.GetType()}] TurnOn");

    }

    public void TurnOff()
    {
        Debug.Log($"[{this.GetType()}] TurnOff");

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

    //void InitializeAllOscReceiver()
    //{
    //    if(transReceiver == null)
    //    {
    //        transReceiver = transform.Find("Receiver");
    //    }

    //    Debug.Log("ParameterReceiver | InitializeAllOscReceiver");
    //    for (int i = 0; i < propertiesForReceiving.Count; i++)
    //    {
    //        OscPropertyForReceiving property = propertiesForReceiving[i];
    //        AddReceiverComponent(property);
    //    }
    //}

    void RemoveAllOscReceiver()
    {
        if (transReceiver == null)
        {
            transReceiver = transform.Find("Receiver");
        }

        OscEventReceiverModified[] receivers = transReceiver.GetComponents<OscEventReceiverModified>();
        foreach (OscEventReceiverModified receiver in receivers)
        {
            Destroy(receiver);
        }
    }

    void ActiveAllOscReceiver()
    {
        if (transReceiver == null)
        {
            transReceiver = transform.Find("Receiver");
        }

        OscEventReceiverModified[] receivers = transReceiver.GetComponents<OscEventReceiverModified>();
        foreach(OscEventReceiverModified receiver in receivers)
        {
            receiver.enabled = true;
        }
    }

    void DeactiveAllOscReceiver()
    {
        if (transReceiver == null)
        {
            transReceiver = transform.Find("Receiver");
        }

        OscEventReceiverModified[] receivers = transReceiver.GetComponents<OscEventReceiverModified>();
        foreach (OscEventReceiverModified receiver in receivers)
        {
            receiver.enabled = false;
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
            receiver._floatEvent.AddListener((v)=> {
                if (property.needClamp)
                    v = Mathf.Clamp(v, property.minValue, property.maxValue);

                property.floatAction?.Invoke(v);

                ControlPanel.Instance.OnUpdateDisplay(property.oscAddress, v);
            });            
            receiver.Initialize();
        }
        //else if (property.dataType == typeof(Vector3))
        //{
        //    receiver._dataType = OscEventReceiverModified.DataType.Vector3;
        //    receiver._vector3Event = new OscEventReceiverModified.Vector3Event();
        //    receiver._vector3Event.AddListener(property.vector3Action);
        //    receiver._vector3Event.AddListener((v) => {
        //        OnUpdateDisplay(property.oscAddress, v);
        //    });

        //    receiver.Initialize();
        //}
        //else if (property.dataType == typeof(int))
        //{
        //    receiver._dataType = OscEventReceiverModified.DataType.Int;
        //    receiver._intEvent = new OscEventReceiverModified.IntEvent();
        //    receiver._intEvent.AddListener(property.intAction);

        //    receiver.Initialize();
        //}
        //else if (property.dataType == typeof(string))
        //{
        //    receiver._dataType = OscEventReceiverModified.DataType.String;
        //    receiver._stringEvent = new OscEventReceiverModified.StringEvent();
        //    receiver._stringEvent.AddListener(property.stringAction);

        //    receiver.Initialize();
        //}
    }


    //void OnUpdateDisplay(string address, Vector3 v)
    //{
    //    if (displayPanelShown == false)
    //        return;

    //    //Transform item = transformDisplayPanel.Find(address.Substring(1));
    //    Transform item = parameterRoot.Find(address.Substring(1));
    //    if (item != null)
    //    {
    //        item.Find("Value").GetComponent<TextMeshProUGUI>().text = v.ToString();
    //    }
    //}

    

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
