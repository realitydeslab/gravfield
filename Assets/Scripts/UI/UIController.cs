using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UnityEngine.Events;
#if UNITY_IOS
using HoloKit;
#endif

public class UIController : MonoBehaviour
{
    [SerializeField]
    string performerPassword = "111";

    [SerializeField]
    string serverPassword = "222";

    [SerializeField]
    string commanderPassword = "333";

    #if UNITY_IOS
    HoloKitCameraManager holoKitManager;
#endif

    EffectManager effectManager;

    // Pages
    Transform pageHome;
    //Transform pageMore;
    Transform pageRelocalization;
    Transform pagePassword;
    Transform pageSettings;
    Transform pageGame;
    Transform pageWaiting;
    Transform pageExtraMenu;

    // Floating Panels
    Transform panelWarning;

    // Elements
    TextMeshProUGUI progressText;
    TMP_InputField inputPassword;
    

    Dictionary<string, Transform> registeredPages = new Dictionary<string, Transform>();

    public float LongPressedTimeThreshold = 3;
    float longPressedTime = 0;
    float lastInteractionTime = 0;

    Transform currentPage = null;

    private enum ApplyingRole
    {
        Undefined,
        Performer,
        Server,
        Commander,
        SinglePlayer
    }
    ApplyingRole applyingRole = ApplyingRole.Undefined;



    void Awake()
    {
        #if UNITY_IOS
        holoKitManager = FindFirstObjectByType<HoloKitCameraManager>();
        if (holoKitManager == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find HoloKitCameraManager.");
        }
#endif
        effectManager = FindFirstObjectByType<EffectManager>();
        if (effectManager == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find EffectManager.");
        }

        // Pages
        pageHome = FindTransformAndRegister("Page_Home");
        //pageMore = FindTransformAndRegister("Page_More");
        pagePassword = FindTransformAndRegister("Page_Password");
        pageSettings = FindTransformAndRegister("Page_Settings");
        pageWaiting = FindTransformAndRegister("Page_Waiting");
        pageRelocalization = FindTransformAndRegister("Page_Relocalization");
        pageGame = FindTransformAndRegister("Page_Game");
        pageExtraMenu = FindTransformAndRegister("Page_ExtraMenu");

        // Floating Panel
        panelWarning = FindTransformAndRegister("Panel_Warning", need_register: false);

        // Elements
        progressText = pageRelocalization.Find("Progress").GetComponent<TextMeshProUGUI>();
        inputPassword = pagePassword.Find("InputField_Password").GetComponent<TMP_InputField>();

    }

    void Start()
    {
        FindButtonAndBind("Page_Home/Button_Start", OnClickStart);
        FindButtonAndBind("Page_Home/Button_SinglePlayer", OnClickSinglePlayer);
        FindButtonAndBind("Page_Home/Button_Performer", OnClickPerformer);
        //FindButtonAndBind("Page_Home/Button_More", () => { GotoPage("Page_More"); });
        FindButtonAndBind("Page_Home/Button_Server", OnClickServer);
        FindButtonAndBind("Page_Home/Button_Settings", OnClickSettings);

        //FindButtonAndBind("Page_More/Button_Server", OnClickServer);
        //FindButtonAndBind("Page_More/Button_Commander", OnClickCommander);
        //FindButtonAndBind("Page_More/Button_Settings", OnClickSettings);
        //FindButtonAndBind("Page_More/Button_Return", GoBackToHomePage);


        FindButtonAndBind("Page_Password/Button_Enter", () => { OnConfirmPassword(transform.Find("Page_Password/InputField_Password")); });
        FindButtonAndBind("Page_Password/Button_Close", ()=> { GotoPage("Page_Home"); });

        FindButtonAndBind("Page_Settings/Button_Enter", () => { OnConfirmServerIP(transform.Find("Page_Settings/InputField_ServerIP")); });
        FindButtonAndBind("Page_Settings/Button_Close", () => { GotoPage("Page_Home"); });

        FindButtonAndBind("Page_Relocalization/Button_Close", () => { CloseRelocalizationPage(); });

        FindButtonAndBind("Page_Game/Button_Left", () => { OnClickLeftArrow(); });
        FindButtonAndBind("Page_Game/Button_Right", () => { OnClickRightArrow(); });

        FindButtonAndBind("Page_ExtraMenu/Button_Calibrate", () => { GotoRelocalizationPage(); });
        FindButtonAndBind("Page_ExtraMenu/Button_Exit", () => { RestartGame(); });
        FindButtonAndBind("Page_ExtraMenu/Button_Return", () => { HideExtraMenu(); });

        FindButtonAndBind("Page_ExtraMenu/Button_InfoPanel", ToggleInfoPanel);
        FindButtonAndBind("Page_ExtraMenu/Button_ReceiverPanel", ToggleReceiverPanel);
        FindButtonAndBind("Page_ExtraMenu/Button_SenderPanel", ToggleSenderPanel);

        GotoPage("Page_Home");

        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer
           || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            StartCoroutine(JoinAsServerAutomatically());
        }
    }

    Transform FindTransformAndRegister(string name, bool need_register = true)
    {
        Transform ui_element = transform.Find(name);

        if (ui_element == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find UI element: {name}");
        }
        else
        {
            if (need_register)
                registeredPages.Add(name, ui_element);
        }
        return ui_element;
    }

    Button FindButtonAndBind(string name, UnityAction action)
    {
        Transform ui_element = transform.Find(name);

        if (ui_element == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find UI element: {name}");
            return null;
        }

        Button button = ui_element.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find Button components on element: {name}");
            return null;
        }

        button.onClick.AddListener(action);

        return button;
    }    

    void Update()
    {
        // Long Pressed
        if (Input.GetMouseButton(0))
        {
            longPressedTime += Time.deltaTime;
            if (longPressedTime >= LongPressedTimeThreshold)
            {
                // If it's in game mode and screen mode is mono
#if UNITY_IOS
                if (currentPage == pageGame && (holoKitManager != null && holoKitManager.ScreenRenderMode == HoloKit.ScreenRenderMode.Mono))
#else
                if (currentPage == pageGame)
#endif
                {
                    ShowExtraMenu();
                    longPressedTime = 0;
                }
            }
        }
        else
        {
            longPressedTime = 0;
        }

        // Update relocalization progress
        if (currentPage == pageRelocalization)
            progressText.text = Mathf.FloorToInt(GameManager.Instance.GetRelocalizationProgress() * 100f).ToString() + "%";
    }


#region Click Listeners
    void OnClickStart()
    {
        JoinAsSpectator();
    }

    void OnClickSinglePlayer()
    {
        JoinAsSinglePlayer();
    }

    void OnClickPerformer()
    {
        if (GameManager.Instance.IsInDevelopment)
            inputPassword.text = performerPassword;

        GotoPasswordPage(ApplyingRole.Performer);
    }

    void OnClickServer()
    {
        if (GameManager.Instance.IsInDevelopment)
            inputPassword.text = serverPassword;

        GotoPasswordPage(ApplyingRole.Server);
    }

    void OnClickCommander()
    {
        if (GameManager.Instance.IsInDevelopment)
            inputPassword.text = commanderPassword;

        GotoPasswordPage(ApplyingRole.Commander);
    }

    void OnClickSettings()
    {
        GotoServerIpPage();
    }

    void OnConfirmPassword(Transform input_transform)
    {
        if (input_transform == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find input element for Password.");
            return;
        }

        TMP_InputField input_field = input_transform.GetComponent<TMP_InputField>();
        if (input_field == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find input component for Password.");
            return;
        }

        string pwd = input_field.text;
        if (applyingRole == ApplyingRole.Performer && pwd == performerPassword)
        {
            JoinAsPerformer();

            applyingRole = ApplyingRole.Undefined;
        }
        else if (applyingRole == ApplyingRole.Server && pwd == serverPassword)
        {
            JoinAsServer();

            applyingRole = ApplyingRole.Undefined;
        }
        else if (applyingRole == ApplyingRole.Commander && pwd == commanderPassword)
        {
            JoinAsCommander();

            applyingRole = ApplyingRole.Undefined;
        }
        else
        {
            GotoWaitingPage("Wrong Password.", auto_close_time: 2, () => {
                // if wrong, type again
                GotoPage("Page_Password");
            });
        }
    }   

    void OnConfirmServerIP(Transform input_transform)
    {
        if (input_transform == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find input element for ServerIP.");
            return;
        }

        TMP_InputField input_field = input_transform.GetComponent<TMP_InputField>();
        if (input_field == null)
        {
            Debug.LogError($"[{this.GetType()}] Can't find input component for ServerIP.");
            return;
        }

        string server_ip = input_field.text;

        if (GameManager.Instance.ConnectionManager.IsIPAddressValide(server_ip) == false)
        {
            GotoWaitingPage("Wrong format for ip address.", auto_close_time: 2, () => {
                // if wrong, type again
                GotoPage("Page_Settings");
            });
        }
        else
        {
            GameManager.Instance.ConnectionManager.ServerIP = server_ip;
            GotoPage("Page_Home");
        }
    }
#endregion


#region Join functions
    void JoinAsSpectator()
    {
        GotoWaitingPage("Connecting to server..");

        GameManager.Instance.JoinAsSpectator((result, msg) => {
            if (result == true)
            {
                RegisterCallback();

                GotoRelocalizationPage();
            }
            else
            {
                GotoWaitingPage(msg, 2, () =>
                {
                    GoBackToHomePage();
                }); // display for 2s
            }
        });
    }

    void JoinAsSinglePlayer()
    {
        GotoWaitingPage("Starting game..");

        GameManager.Instance.JoinAsHost((result, msg) => {
            if (result == true)
            {
                RegisterCallback();

                EnterGame();
            }
            else
            {
                GotoWaitingPage(msg, 2, () =>
                {
                    GoBackToHomePage();
                }); // display for 2s
            }
        });
    }

    void JoinAsPerformer()
    {
        GotoWaitingPage("Connecting to server..");

        GameManager.Instance.JoinAsPerformer((result, msg) => {
            if (result == true)
            {
                RegisterCallback();

                ApplyPerformer();
            }
            else
            {
                GotoWaitingPage(msg, 2, () =>
                {
                    GoBackToHomePage();
                }); // display for 2s
            }
        });
    }

    void ApplyPerformer()
    {
        GotoWaitingPage("Applying to be a performer.");

        GameManager.Instance.ApplyPerformer((result, msg) => {
            if (result == true)
            {
                GotoRelocalizationPage();
            }
            else
            {
                GotoWaitingPage(msg, 2, () =>
                {
                    GoBackToHomePage();
                }); // display for 2s
            }
        });
    }

    void JoinAsServer()
    {
        GameManager.Instance.JoinAsServer((result, msg) =>
        {
            if (result == true)
            {
                GotoRelocalizationPage();
            }
            else
            {
                GotoWaitingPage(msg, auto_close_time: 2, () => {
                    GoBackToHomePage();
                });
            }
        });
    }

    void JoinAsCommander()
    {
        GotoWaitingPage("Connecting to server..");

        GameManager.Instance.JoinAsCommander((result, msg) => {
            if (result == true)
            {
                RegisterCallback();

                GotoRelocalizationPage();
            }
            else
            {
                GotoWaitingPage(msg, 2, () =>
                {
                    GoBackToHomePage();
                }); // display for 2s
            }
        });
    }
#endregion


#region UI controlness
    void GotoPage(string page_name)
    {
        Debug.Log($"[{this.GetType()}] Goto Page: {page_name}");

        lastInteractionTime = Time.time;

        Transform target_page = null;

        if (registeredPages.ContainsKey(page_name))
        {
            target_page = registeredPages[page_name];
        }

        GotoPage(target_page);
    }

    void GotoPage(Transform target_page)
    {
        // Only show desired page
        ShowPageOnly(target_page);

        // update indicator
        currentPage = target_page;
    }

    void ShowPageOnly(Transform target_page)
    {
        foreach (var page in registeredPages.Values)
        {
            page.gameObject.SetActive(page == target_page);
        }
    }

    void ShowElementsOnly(Transform[] dst_elemetns)
    {
        foreach (Transform element in registeredPages.Values)
        {
            bool match = false;
            if (dst_elemetns != null)
            {
                foreach (Transform dst_element in dst_elemetns)
                {
                    if (element == dst_element)
                    {
                        match = true;
                        break;
                    }
                }
            }
            element.gameObject.SetActive(match);
        }
    }
#endregion

#region UI navigation
    void GotoPasswordPage(ApplyingRole role)
    {
        applyingRole = role;

        GotoPage("Page_Password");
    }

    void GotoServerIpPage()
    {
        GotoPage("Page_Settings");
    }

    public void GoBackToHomePage()
    {
        GotoPage("Page_Home");
    }

    void GotoWaitingPage(string msg, float auto_close_time = 0, System.Action action = null)
    {
        SetMessage_WaitingPage(msg);

        GotoPage("Page_Waiting");

        if (auto_close_time > 0)
        {
            StartCoroutine(HideWaitingPage(auto_close_time, action));
        }
    }

    IEnumerator HideWaitingPage(float delay_time, System.Action action)
    {
        yield return new WaitForSeconds(delay_time);

        SetMessage_WaitingPage("");

        action?.Invoke();
    }

    void SetMessage_WaitingPage(string msg)
    {
        pageWaiting.Find("Message").GetComponent<TextMeshProUGUI>().text = msg;
    }

    void GotoRelocalizationPage()
    {
        GotoPage("Page_Relocalization");

        GameManager.Instance.StartRelocalization(() => {
            EnterGame();
        });
    }

    void CloseRelocalizationPage()
    {
        EnterGame();

        GameManager.Instance.StopRelocalization();
    }

    public void ShowWarningText(string msg)
    {
        panelWarning.Find("Message").GetComponent<TextMeshProUGUI>().text = msg;
        panelWarning.gameObject.SetActive(true);

        StartCoroutine(HideWarning());
    }

    IEnumerator HideWarning()
    {
        yield return new WaitForSeconds(2);
        panelWarning.gameObject.SetActive(false);
    }    

    void EnterGame()
    {
        GotoPage(page_name: "Page_Game");

        pageGame.Find("Button_Left").gameObject.SetActive(GameManager.Instance.PlayerRole == PlayerRole.SinglePlayer);
        pageGame.Find("Button_Right").gameObject.SetActive(GameManager.Instance.PlayerRole == PlayerRole.SinglePlayer);
    }

    void RestartGame()
    {
        GameManager.Instance.RestartGame(() =>
        {
            UnregisterCallback();

            GotoPage("Page_Home");
        });
    }

    void ShowExtraMenu()
    {
        pageExtraMenu.gameObject.SetActive(true);

        pageExtraMenu.Find("Button_InfoPanel").gameObject.SetActive(GameManager.Instance.IsInDevelopment);
        pageExtraMenu.Find("Button_ReceiverPanel").gameObject.SetActive(GameManager.Instance.PlayerRole == PlayerRole.Server || GameManager.Instance.PlayerRole == PlayerRole.Commander);
        pageExtraMenu.Find("Button_SenderPanel").gameObject.SetActive(GameManager.Instance.PlayerRole == PlayerRole.Server);
    }

    void HideExtraMenu()
    {
        pageExtraMenu.gameObject.SetActive(false);
    }

    void ToggleInfoPanel()
    {
        Helper.Instance.ToggleInfoPanel();

        HideExtraMenu();
    }

    void ToggleReceiverPanel()
    {
        ParameterReceiver.Instance.ToggleControlPanel();

        HideExtraMenu();
    }

    void ToggleSenderPanel()
    {
        SenderForLive.Instance.ToggleControlPanel();

        HideExtraMenu();
    }
#endregion

    void OnClickLeftArrow()
    {
        effectManager.ChangeToPreviousEffect_UI();
    }

    void OnClickRightArrow()
    {
        effectManager.ChangeToNextEffect_UI();
    }

    #region Callbacks
    void RegisterCallback()
    {
        GameManager.Instance.ConnectionManager.OnServerLostEvent.AddListener(OnServerLostCallback);
    }

    void UnregisterCallback()
    {
        GameManager.Instance.ConnectionManager.OnServerLostEvent.RemoveListener(OnServerLostCallback);
    }

    void OnServerLostCallback()
    {
        RestartGame();
    }
#endregion

    IEnumerator JoinAsServerAutomatically()
    {
        float start_time = Time.time;
        bool enter_solo_mode = false;
        while (Time.time - start_time < 5)
        {
            if (Input.GetKey(KeyCode.S))
            {
                enter_solo_mode = true;
                break;
            }
            yield return null;
        }


        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer
            || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if(lastInteractionTime == 0)
            {
                GameManager.Instance.IsSoloMode = enter_solo_mode;
                JoinAsServer();
            }
        }
    }
}
