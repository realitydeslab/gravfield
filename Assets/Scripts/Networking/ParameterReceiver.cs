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

    [Header("Control Panel")]
    [SerializeField] Transform panelRoot;
    [SerializeField] Transform parameterRoot;
    [SerializeField] GameObject prefabParameterItem;

    bool showControlPanel = false;


    List<OscPropertyForReceiving> propertiesForReceiving = new List<OscPropertyForReceiving>();

    public List<OscPropertyForReceiving> PropertiesForReceiving { get => propertiesForReceiving; }

    Transform transReceiver;

    public void RegisterOscReceiverFunction(string address, UnityAction<float> action, bool need_clamp = false, float min_value = 0, float max_value = 1)
    {
        OscPropertyForReceiving property = new OscPropertyForReceiving(address, action, (min_value + max_value) * 0.5f, need_clamp, min_value, max_value);

        RegisterOscReceiverFunction(property);
    }

    public void RegisterOscReceiverFunction(string address, NetworkVariable<float> param, bool need_clamp = false, float min_value = 0, float max_value = 1)
    {
        OscPropertyForReceiving property = new OscPropertyForReceiving(address, new UnityAction<float>((v)=> { param.Value = v; }), param.Value, need_clamp, min_value, max_value);

        RegisterOscReceiverFunction(property);
    }

    void RegisterOscReceiverFunction(OscPropertyForReceiving property)
    {
        propertiesForReceiving.Add(property);

        AddReceiverComponent(property);

        AddProperyInControlPanel(property);
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

        RemoveAllPropertyInControlPanel();

        HideControlPanel();
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

                OnUpdateDisplay(property.oscAddress, v);
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


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) && NetworkManager.Singleton.IsServer)
        {
            if (showControlPanel) HideControlPanel();
            else ShowControlPanel();
        }
    }

    void AddProperyInControlPanel(OscPropertyForReceiving property)
    {
        GameObject new_item = Instantiate(prefabParameterItem, parameterRoot);
        new_item.name = property.oscAddress.Substring(1);
        new_item.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = property.oscAddress;

        TextMeshProUGUI value_text = new_item.transform.Find("Value").GetComponent<TextMeshProUGUI>();
        value_text.text = "";

        TMP_InputField min_value_input = new_item.transform.Find("InputField_Min").GetComponent<TMP_InputField>();
        TMP_InputField max_value_input = new_item.transform.Find("InputField_Max").GetComponent<TMP_InputField>();
        min_value_input.text = property.minValue.ToString("0.00");
        max_value_input.text = property.maxValue.ToString("0.00");

        Slider slider = new_item.transform.Find("Slider").GetComponent<Slider>();
        slider.value = property.defaultValue;
        slider.minValue = property.minValue;
        slider.maxValue = property.maxValue;

        slider.onValueChanged.AddListener((v) =>
        {
            property.floatAction?.Invoke(v);

            value_text.text = v.ToString("0.00");
        });

        min_value_input.onEndEdit.AddListener((string str) => {
            if (float.TryParse(str, out float result))
            {
                slider.minValue = result;
            }
        });

        max_value_input.onEndEdit.AddListener((string str) => {
            if (float.TryParse(str, out float result))
            {
                slider.maxValue = result;
            }
        });
    }

    void RemoveAllPropertyInControlPanel()
    {
        foreach(Transform child in parameterRoot)
        {
            Destroy(child.gameObject);
        }
    }

    void OnUpdateDisplay(string address, float v)
    {
        if (showControlPanel == false)
            return;

        Transform item = parameterRoot.Find(address.Substring(1));
        if (item != null)
        {
            item.Find("Value").GetComponent<TextMeshProUGUI>().text = v.ToString();
        }
    }

    public void ShowControlPanel()
    {
        panelRoot.gameObject.SetActive(true);
        showControlPanel = true;
    }
    public void HideControlPanel()
    {
        panelRoot.gameObject.SetActive(false);
        showControlPanel = false;
    }
    public void ToggleControlPanel()
    {
        if (showControlPanel) HideControlPanel();
        else ShowControlPanel();
    }

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
