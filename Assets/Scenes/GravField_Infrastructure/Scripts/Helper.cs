using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using HoloKit.ColocatedMultiplayerBoilerplate;
using OscJack;
using UnityEngine.UI;
using System;

public class Helper : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] PingManager pingManager;

    [SerializeField] bool infoPanelEnabled = false;
    [SerializeField] Transform infoPanelRoot;
    [SerializeField] GameObject infoItemPrefab;

    Dictionary<string, TextMeshProUGUI> infoItemList = new Dictionary<string, TextMeshProUGUI>();

    [Header("Parameters")]
    public RectTransform controlPanelRoot;
    public bool controlPanelEnabled = false;
    Dictionary<string, Action<float>> sliderActionList;

    int pingSpeed = 0;

    void Start()
    {
        if (controlPanelEnabled)
        {
            controlPanelRoot.gameObject.SetActive(true);

            sliderActionList = new Dictionary<string, Action<float>>();
            for (int i = 0; i < controlPanelRoot.childCount; i++)
            {
                Transform item = controlPanelRoot.GetChild(i);
                if (item.gameObject.activeSelf == false)
                    continue;

                string param_name = item.Find("Label").GetComponent<TextMeshProUGUI>().text;
                Slider slider = item.Find("Slider").GetComponent<Slider>();
                TextMeshProUGUI display_value = item.Find("Value").GetComponent<TextMeshProUGUI>();                


                // register slider
                slider.onValueChanged.AddListener((float v) =>
                {
                    display_value.text = v.ToString("0.00");

                    //SliderCallbackFunction(item.name, param_name, v);
                });
            }
        }
    }

    void OnEnable()
    {
        if (pingManager != null)
            pingManager.OnReceivedRtt.AddListener(OnReceiveRtt);
    }
    void OnDisable()
    {
        if (pingManager != null)
            pingManager.OnReceivedRtt.RemoveListener(OnReceiveRtt);
    }

    void OnReceiveRtt(int rtt)
    {
        pingSpeed = rtt;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (infoPanelEnabled) HideInfoPanel();
            else ShowInfoPanel();
        }


        if (infoPanelEnabled == false)
            return;

        // Client & Server
        SetInfo("FPS", (1.0f / Time.smoothDeltaTime).ToString("0.0"));
        SetInfo("IP", GameManager.Instance.ConnectionManager.LocalIP);
        SetInfo("IsServer", NetworkManager.Singleton.IsServer ? "Yes" : "No");
        SetInfo("Connected", (NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening) || (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient) ? "Yes": "No");
        SetInfo("Ping", pingSpeed.ToString());

        // Only Server
        if(NetworkManager.Singleton.IsServer)
        {
            SetInfo("PerformerCount", GameManager.Instance.RoleManager.PerformerCount.ToString());
            SetInfo("AudienceCount", GameManager.Instance.RoleManager.AudienceCount.ToString());
            //SetInfo("Coda", SenderForCoda.Instance.ConnectedWithCoda ? "Yes" : "No");
            //SetInfo("Live", SenderForLive.Instance.ConnectedWithLive ? "Yes" : "No");
        }
    }


    public void SetInfo(string name, string text)
    {
        if(infoItemList.ContainsKey(name) == false)
        {
            GameObject item = Instantiate(infoItemPrefab, infoPanelRoot);
            item.name = name;
            item.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = name;
            infoItemList.Add(name, item.transform.Find("Value").GetComponent<TextMeshProUGUI>());
        }

        infoItemList[name].text = text;
    }

    public void ToggleInfoPanel()
    {
        if (infoPanelEnabled) HideInfoPanel();
        else ShowInfoPanel();
    }

    void ShowInfoPanel()
    {
        infoPanelRoot.gameObject.SetActive(true);
        infoPanelEnabled = true;
    }

    void HideInfoPanel()
    {
        infoPanelRoot.gameObject.SetActive(false);
        infoPanelEnabled = false;
    }

    void SliderCallbackFunction(string item_name, string param_name, float v)
    {
           if(sliderActionList.ContainsKey(param_name))
                sliderActionList[param_name]?.Invoke(v);
    }


    public void AddSliderAction(string name, Action<float> action)
    {
        sliderActionList.Add(name, action);
    }


    #region Instance
    private static Helper _Instance;

    public static Helper Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<Helper>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    _Instance = go.AddComponent<Helper>();
                }
            }
            return _Instance;
        }
    }
    private void Awake()
    {
        if (_Instance != null)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        _Instance = null;
    }
    #endregion
}
