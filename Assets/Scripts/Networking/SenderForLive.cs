using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using OscJack;
using UnityEngine.UI;
using TMPro;

public class SenderForLive : MonoBehaviour
{
    [SerializeField] OscConnection senderConnection = null;

    [Header("Control Panel")]
    [SerializeField] Transform parameterRoot;
    [SerializeField] GameObject prefabPropertyItem;


    private bool connectedWithLive = false;
    public bool ConnectedWithLive { get => connectedWithLive; }

    List<OscPropertyForSending> propertiesForSending = new List<OscPropertyForSending>();

    OscClient _client = null;

    bool showControlPanel = false;

    public void RegisterOscPropertyToSend(string address, AutoSwitchedParameter<float> param)
    {
        if (NetworkManager.Singleton.IsServer == false) return;

        OscPropertyForSending property = new OscPropertyForSending(address, param);

        propertiesForSending.Add(property);

        AddProperyItemInControlPanel(property);
    }
    public void RegisterOscPropertyToSend(string address, AutoSwitchedParameter<Vector3> param)
    {
        if (NetworkManager.Singleton.IsServer == false) return;

        OscPropertyForSending property = new OscPropertyForSending(address, param);

        propertiesForSending.Add(property);

        AddProperyItemInControlPanel(property);
    }
    public void RegisterOscPropertyToSend(string address, AutoSwitchedParameter<int> param)
    {
        if (NetworkManager.Singleton.IsServer == false) return;

        OscPropertyForSending property = new OscPropertyForSending(address, param);

        propertiesForSending.Add(property);

        AddProperyItemInControlPanel(property);
    }
    public void RegisterOscPropertyToSend(string address, AutoSwitchedParameter<string> param)
    {
        if (NetworkManager.Singleton.IsServer == false) return;

        OscPropertyForSending property = new OscPropertyForSending(address, param);

        propertiesForSending.Add(property);

        AddProperyItemInControlPanel(property);
    }


    void Update()
    {
        if (_client == null || !NetworkManager.Singleton.IsServer) return;

        bool error_occured = false;
        foreach (OscPropertyForSending property in propertiesForSending)
        {
            try
            {
                if (property.enabled)
                    Send(property);
            }
            catch
            {
                error_occured = true;
            }
        }
        connectedWithLive = !error_occured;


        if(showControlPanel)
        {
            UpdateControlPanel();
        }


        if(Input.GetKeyDown(KeyCode.F6))
        {
            if (showControlPanel) HideControlPanel();
            else ShowControlPanel();
        }
    }


    void Send(OscPropertyForSending property)
    {
        if (property.dataType == typeof(float))
        {
            if (!property.keepSending && !property.floatParameter.Changed)
                return;

            float data = property.floatParameter.Value;
            if (property.needRemap)
                data = Remap(data, property.srcRange.x, property.srcRange.y, property.dstRange.x, property.dstRange.y, property.needClamp);

            property.mappedValue = data;

            //Debug.Log($"SenderForLive | Send Address:{property.oscAddress}, Value:{data}");

             _client.Send(property.oscAddress, data);
        }
        else if (property.dataType == typeof(Vector3))
        {
            if (!property.keepSending && !property.vector3Parameter.Changed)
                return;

            Vector3 data = property.vector3Parameter.Value;
            if (property.needRemap)
            {
                data.x = Remap(data.x, property.srcRange.x, property.srcRange.y, property.dstRange.x, property.dstRange.y, property.needClamp);
                data.y = Remap(data.x, property.srcRange.x, property.srcRange.y, property.dstRange.x, property.dstRange.y, property.needClamp);
                data.z = Remap(data.x, property.srcRange.x, property.srcRange.y, property.dstRange.x, property.dstRange.y, property.needClamp);
            }

            _client.Send(property.oscAddress, data.x, data.y, data.z);
        }
        else if (property.dataType == typeof(int))
        {
            if (!property.keepSending && !property.intParameter.Changed)
                return ;

            int data = property.intParameter.Value;
            if (property.needRemap)
                data = Mathf.FloorToInt(Remap(data, property.srcRange.x, property.srcRange.y, property.dstRange.x, property.dstRange.y, property.needClamp));

            _client.Send(property.oscAddress, data);
        }
        else if (property.dataType == typeof(string))
        {
            if (!property.keepSending && !property.stringParameter.Changed)
                return;

            string data = property.stringParameter.Value;
            _client.Send(property.oscAddress, data);
        }
    }


    public void TurnOn()
    {
        Debug.Log($"[{this.GetType()}] TurnOn");

        if (senderConnection != null)
            _client = OscMaster.GetSharedClient(senderConnection.host, senderConnection.port);
        else
            _client = null;

        //if(useControlPanel)
        //{
        //    //if (!controlPanelCreated)
        //    //{
        //    //    GenerateControlPanel();
        //    //}

        //    ShowControlPanel();
        //}
    }


    public void TurnOff()
    {
        Debug.Log($"[{this.GetType()}] TurnOff");

        _client?.Dispose();
        _client = null;

        RemoveAllSender();

        RemoveAllItemsInControlPanel();

        HideControlPanel();
    }

    void RemoveAllSender()
    {
        propertiesForSending.Clear();
    }

    void RemoveAllItemsInControlPanel()
    {
        foreach (Transform child in parameterRoot)
        {
            Destroy(child.gameObject);
        }
    }


    //void GenerateControlPanel()
    //{
    //    for (int i = 0; i < propertiesForSending.Count; i++)
    //    {
    //        OscPropertyForSending property = propertiesForSending[i];
    //        AddProperyItemInControlPanel(property);            
    //    }

    //    controlPanelCreated = true;
    //}

    void AddProperyItemInControlPanel(OscPropertyForSending property)
    {
        GameObject new_item = Instantiate(prefabPropertyItem, parameterRoot.transform);
        new_item.name = property.oscAddress;

        new_item.transform.Find("Toggle_Enabled").GetComponent<Toggle>().isOn = property.enabled;
        new_item.transform.Find("Address").GetComponent<TextMeshProUGUI>().text = property.oscAddress;
        new_item.transform.Find("OriginalValue").GetComponent<TextMeshProUGUI>().text = property.floatParameter.Value.ToString("0.00");
        new_item.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = property.mappedValue.ToString("0.00");
        new_item.transform.Find("Toggle_Clamp").GetComponent<Toggle>().isOn = property.needClamp;
        new_item.transform.Find("InputField_Min").GetComponent<TMP_InputField>().text = property.srcRange.x.ToString("0.00");
        new_item.transform.Find("InputField_Max").GetComponent<TMP_InputField>().text = property.srcRange.y.ToString("0.00");


        new_item.transform.Find("Toggle_Enabled").GetComponent<Toggle>().onValueChanged.AddListener((bool v) => {
            property.enabled = v;
        });
        new_item.transform.Find("Toggle_Remap").GetComponent<Toggle>().onValueChanged.AddListener((bool v) => {
            property.needRemap = v;
        });
        new_item.transform.Find("Toggle_Clamp").GetComponent<Toggle>().onValueChanged.AddListener((bool v) => {
            property.needClamp = v;
        });
        new_item.transform.Find("InputField_Min").GetComponent<TMP_InputField>().onEndEdit.AddListener((string str) => {
            Debug.Log("Edit InputField_Min:" + property.oscAddress);
            try
            {
                property.srcRange.x = float.Parse(str);
            }
            catch
            { }

        });

        new_item.transform.Find("InputField_Max").GetComponent<TMP_InputField>().onEndEdit.AddListener((string str) => {
            Debug.Log("Edit InputField_Max:" + property.oscAddress);
            try
            {
                property.srcRange.y = float.Parse(str);
            }
            catch
            { }
        });
    }

    void UpdateControlPanel()
    {
        for (int i = 0; i < propertiesForSending.Count; i++)
        {
            OscPropertyForSending property = propertiesForSending[i];
            Transform item = parameterRoot.transform.GetChild(i);
            if (item.name != property.oscAddress)
                continue;

            item.Find("OriginalValue").GetComponent<TextMeshProUGUI>().text = property.floatParameter.Value.ToString("0.00");
            item.Find("Value").GetComponent<TextMeshProUGUI>().text = property.mappedValue.ToString("0.00");
        }
    }

    void ShowControlPanel()
    {
        parameterRoot.gameObject.SetActive(true);
        showControlPanel = true;
    }
    void HideControlPanel()
    {
        parameterRoot.gameObject.SetActive(false);
        showControlPanel = false;
    }

    public void ToggleControlPanel()
    {
        if (showControlPanel) HideControlPanel();
        else ShowControlPanel();
    }


    #region Remap Function
    static float Remap(float v, float src_min, float src_max, float dst_min, float dst_max, bool clamp = true)
    {
        if (clamp)
            v = Mathf.Clamp(v, src_min, src_max);

        if (src_min == src_max)
            return 0;

        return (v - src_min) / (src_max - src_min) * (dst_max - dst_min) + dst_min;
    }
    #endregion


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
    private static SenderForLive _Instance;

    public static SenderForLive Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<SenderForLive>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    _Instance = go.AddComponent<SenderForLive>();
                }
            }
            return _Instance;
        }
    }
    #endregion
}
