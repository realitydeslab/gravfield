using System.Collections;
using System.Collections.Generic;
using OscJack;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ParameterReceiver : MonoBehaviour
{
    [SerializeField] OscConnection receiverConnection = null;

    [Header("Control Panel")]
    //[SerializeField] bool useDisplayPanel = false;
    [SerializeField] Transform transformDisplayPanel;
    [SerializeField] GameObject prefabPropertyItem;

    bool displayPanelShown = false;

    List<OscPropertyForReceiving> propertiesForReceiving = new List<OscPropertyForReceiving>();

    Transform transReceiver;

    public void RegisterOscReceiverFunction(string address, UnityAction<float> action)
    {
        OscPropertyForReceiving property = new OscPropertyForReceiving(address, action);

        propertiesForReceiving.Add(property);

        AddReceiverComponent(property);

        AddProperyInDisplayPanel(property);
    }
    public void RegisterOscReceiverFunction(string address, UnityAction<Vector3> action)
    {
        OscPropertyForReceiving property = new OscPropertyForReceiving(address, action);

        propertiesForReceiving.Add(property);

        AddReceiverComponent(property);
    }
    public void RegisterOscReceiverFunction(string address, UnityAction<int> action)
    {
        OscPropertyForReceiving property = new OscPropertyForReceiving(address, action);

        propertiesForReceiving.Add(property);

        AddReceiverComponent(property);
    }
    public void RegisterOscReceiverFunction(string address, UnityAction<string> action)
    {
        OscPropertyForReceiving property = new OscPropertyForReceiving(address, action);

        propertiesForReceiving.Add(property);

        AddReceiverComponent(property);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F6) && NetworkManager.Singleton.IsServer)
        {
            if (displayPanelShown) HideDisplayPanel();
            else ShowDisplayPanel();
        }
    }

    public void TurnOn()
    {
        Debug.Log("ParameterReceiver | TurnOn");

        //InitializeAllOscReceiver();
        ActiveAllOscReceiver();
    }

    public void TurnOff()
    {
        Debug.Log("ParameterReceiver | TurnOff");

        //RemoveAllOscReceiver();
        DeactiveAllOscReceiver();
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

    //void RemoveAllOscReceiver()
    //{
    //    if (transReceiver == null)
    //    {
    //        transReceiver = transform.Find("Receiver");
    //    }

    //    Debug.Log("ParameterReceiver | RemoveAllOscReceiver");
    //    foreach (OscEventReceiverModified receiver in transReceiver.GetComponents<OscEventReceiverModified>())
    //    {
    //        Destroy(receiver);
    //    }
    //}
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
            receiver._floatEvent.AddListener(property.floatAction);
            receiver._floatEvent.AddListener((v)=>{
                OnUpdateDisplay(property.oscAddress, v);
            });
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
        receiver.enabled = false;
    }

    void AddProperyInDisplayPanel(OscPropertyForReceiving property)
    {
        GameObject new_item = Instantiate(prefabPropertyItem, transformDisplayPanel.transform);
        new_item.name = property.oscAddress.Substring(1);
        new_item.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = property.oscAddress;
        new_item.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = "";
    }

    void OnUpdateDisplay(string address, float v)
    {
        if (displayPanelShown == false)
            return;

        Transform item = transformDisplayPanel.Find(address.Substring(1));
        if(item != null)
        {
            item.Find("Value").GetComponent<TextMeshProUGUI>().text = v.ToString();
        }
    }

    void ShowDisplayPanel()
    {
        transformDisplayPanel.gameObject.SetActive(true);
        displayPanelShown = true;
    }
    void HideDisplayPanel()
    {
        transformDisplayPanel.gameObject.SetActive(false);
        displayPanelShown = false;
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
