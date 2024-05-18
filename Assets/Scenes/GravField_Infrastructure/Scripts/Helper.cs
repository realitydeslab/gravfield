using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using HoloKit.ColocatedMultiplayerBoilerplate;
using OscJack;

public class Helper : MonoBehaviour
{
    [SerializeField] PingManager pingManager;

    [SerializeField] bool infoPanelEnabled = false;
    [SerializeField] Transform infoPanelRoot;
    [SerializeField] GameObject infoItemPrefab;

    Dictionary<string, TextMeshProUGUI> infoItemList = new Dictionary<string, TextMeshProUGUI>();

    int pingSpeed = 0;

    void Start()
    {

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
            SetInfo("Coda", SenderForCoda.Instance.ConnectedWithCoda ? "Yes" : "No");
            SetInfo("Live", SenderForLive.Instance.ConnectedWithLive ? "Yes" : "No");
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
