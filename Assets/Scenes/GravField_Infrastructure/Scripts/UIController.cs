using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HoloKit.ImageTrackingRelocalization;
using System;

public class UIController : MonoBehaviour
{
    [SerializeField]
    string performerPassword = "111";

    [SerializeField]
    string serverPassword = "222";

    [SerializeField]
    string commanderPassword = "333";

    Transform transButtonStart;
    Transform transButtonPerformer;
    //Transform transButtonSettings;
    Transform transButtonServer;
    Transform transButtonCommander;

    Transform transPanelCalibration;
    Transform transPanelPassword;
    Transform transPanelServerIP;
    Transform transPanelWarning;
    Transform transPanelWaiting;

    Transform transPanelExtraMenu;

    TMP_InputField inputPassword;
    TMP_InputField inputServerIP;
    TextMeshProUGUI waitingMessageText;
    TextMeshProUGUI progressText;

    Dictionary<string, Transform> registeredUIElements = new Dictionary<string, Transform>();

    public float LongPressedTimeThreshold = 3;
    float longPressedTime = 0;

    Transform pressedButtonBeforePassword;

    void Awake()
    {
        // UI Elements
        transButtonStart = FindTransformAndRegister("Button_Start");
        transButtonPerformer = FindTransformAndRegister("Button_Performer");
        //transButtonSettings = FindTransformAndRegister("Button_Settings");
        transButtonServer = FindTransformAndRegister("Button_Server");
        transButtonCommander = FindTransformAndRegister("Button_Commander");

        transPanelPassword = FindTransformAndRegister("Panel_Password");
        transPanelServerIP = FindTransformAndRegister("Panel_ServerIP");

        transPanelWaiting = FindTransformAndRegister("Panel_Waiting");

        transPanelCalibration = FindTransformAndRegister("Panel_Calibration");

        transPanelExtraMenu = FindTransformAndRegister("Panel_ExtraMenu");

        transPanelWarning = transform.Find("Panel_Warning"); // WarningPanel is different because it's independent

        // Text Field
        inputPassword = transPanelPassword.Find("InputField_Password").GetComponent<TMP_InputField>();
        inputServerIP = transPanelServerIP.Find("InputField_ServerIP").GetComponent<TMP_InputField>();
        waitingMessageText = transPanelWaiting.Find("Message").GetComponent<TextMeshProUGUI>();
        progressText = transPanelCalibration.Find("Progress").GetComponent<TextMeshProUGUI>();
    }

    Transform FindTransformAndRegister(string name)
    {
        Transform ui_element = transform.Find(name);

        if (ui_element == null)
        {
            Debug.LogError("Can't find UI elements " + name);
        }
        else
        {
            registeredUIElements.Add(name, ui_element);
        }
        return ui_element;
    }

    void Start()
    {
        // Bind Basic Listener
        transButtonStart.GetComponent<Button>().onClick.AddListener(OnClickStart);
        transButtonPerformer.GetComponent<Button>().onClick.AddListener(OnClickPerformer);
        //transButtonSettings.GetComponent<Button>().onClick.AddListener(OnClickSettings);
        transButtonServer.GetComponent<Button>().onClick.AddListener(OnClickServer);
        transButtonCommander.GetComponent<Button>().onClick.AddListener(OnClickCommander);

        transPanelPassword.Find("Button_Enter").GetComponent<Button>().onClick.AddListener(OnEnterPassword);
        transPanelServerIP.Find("Button_Enter").GetComponent<Button>().onClick.AddListener(OnEnterServerIp);

        transPanelPassword.Find("Button_Close").GetComponent<Button>().onClick.AddListener(GoBackToHomePage);
        transPanelServerIP.Find("Button_Close").GetComponent<Button>().onClick.AddListener(GoBackToHomePage);
        transPanelCalibration.Find("Button_Close").GetComponent<Button>().onClick.AddListener(GameManager.Instance.StopRelocalization);

        transPanelExtraMenu.Find("Button_Calibrate").GetComponent<Button>().onClick.AddListener(GameManager.Instance.StartRelocalization);
        transPanelExtraMenu.Find("Button_Exit").GetComponent<Button>().onClick.AddListener(GameManager.Instance.RestartGame);
        transPanelExtraMenu.Find("Button_Return").GetComponent<Button>().onClick.AddListener(HideExtraMenu);

        transPanelExtraMenu.Find("Button_InfoPanel").GetComponent<Button>().onClick.AddListener(ToggleInfoPanel);
        transPanelExtraMenu.Find("Button_ControllerPanel").GetComponent<Button>().onClick.AddListener(ToggleControlPanel);
        transPanelExtraMenu.Find("Button_OscSenderPanel").GetComponent<Button>().onClick.AddListener(ToggleOscSenderPanel);
    }

    void Update()
    {
        // Long Pressed
        if (Input.GetMouseButton(0))
        {
            longPressedTime += Time.deltaTime;
            if (longPressedTime >= LongPressedTimeThreshold)
            {
                if (GameManager.Instance.IsPlaying && GameManager.Instance.HolokitCameraManager.ScreenRenderMode == HoloKit.ScreenRenderMode.Mono)
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

        if (GameManager.Instance.RelocalizationStablizer.IsRelocalizing)
            progressText.text = Mathf.FloorToInt(GameManager.Instance.RelocalizationStablizer.Progress * 100f).ToString() + "%";
    }


    #region Responsive Function
    void OnClickStart()
    {
        GameManager.Instance.JoinAsAudience();
    }

    void OnClickPerformer()
    {
        pressedButtonBeforePassword = transButtonPerformer;

        if (GameManager.Instance.IsInDevelopment)
            inputPassword.text = performerPassword;

        GotoPasswordPage();
    }

    void OnClickSettings()
    {
        GotoServerIpPage();
    }

    void OnClickServer()
    {
        pressedButtonBeforePassword = transButtonServer;

        if (GameManager.Instance.IsInDevelopment)
            inputPassword.text = serverPassword;

        GotoPasswordPage();
    }

    void OnClickCommander()
    {
        pressedButtonBeforePassword = transButtonCommander;

        if (GameManager.Instance.IsInDevelopment)
            inputPassword.text = commanderPassword;

        GotoPasswordPage();
    }

    public void OnEnterPassword()
    {
        if (pressedButtonBeforePassword == transButtonPerformer)
        {
            if (inputPassword.text == performerPassword)
            {
                GameManager.Instance.JoinAsPerformer();
            }
            else
            {
                ShowWarningText("Wrong Password.");
            }
        }
        else if (pressedButtonBeforePassword == transButtonServer)
        {

            if (inputPassword.text == serverPassword)
            {
                GameManager.Instance.JoinAsServer();
            }
            else
            {
                ShowWarningText("Wrong Password.");
            }
        }
        else if (pressedButtonBeforePassword == transButtonCommander)
        {

            if (inputPassword.text == commanderPassword)
            {
                GameManager.Instance.JoinAsCommander();
            }
            else
            {
                ShowWarningText("Wrong Password.");
            }
        }
    }

    public void OnEnterServerIp()
    {
        //if(GameManager.Instance.ConnectionManager.IsIPAddressValide(inputServerIP.text))
        //{
        //    GameManager.Instance.ConnectionManager.SetServerIP(inputServerIP.text);
        //    GoBackToHomePage();
        //}
        //else
        //{
        //    ShowWarningText("Wrong ServerIp.");
        //}
    }
    #endregion

    #region UI Arrangement Function
    void GotoPasswordPage()
    {
        Transform[] element_list = new Transform[] {
            transPanelPassword
        };

        ShowElementsOnly(element_list);
        //GotoPage(1, 1);
    }

    void GotoServerIpPage()
    {
        Transform[] element_list = new Transform[] {
            transPanelServerIP
        };

        ShowElementsOnly(element_list);
        //GotoPage(1, 2);
    }

    public void GoBackToHomePage()
    {
        Transform[] element_list = new Transform[] {
            transButtonStart,
            transButtonPerformer,
            //transButtonSettings,
            transButtonServer,
            transButtonCommander
        };

        ShowElementsOnly(element_list);

        //GotoPage(0);
    }

    public void GotoWaitingPage(string msg, float delay = 0, Action callback = null)
    {
        waitingMessageText.text = msg;

        Transform[] element_list = new Transform[] {
            transPanelWaiting
        };

        ShowElementsOnly(element_list);
        if (delay != 0 && callback != null)
        {
            StartCoroutine(WaitToCallFunction(delay, callback));
        }
        //GotoPage(2);
    }

    IEnumerator WaitToCallFunction(float delay, Action callback = null)
    {
        float start_time = Time.fixedTime; ;
        while (Time.fixedTime - start_time < delay)
        {
            yield return new WaitForSeconds(0.1f);
        }

        callback?.Invoke();
    }

    public void GoToRelocalizationPage()
    {
        Transform[] element_list = new Transform[] {
            transPanelCalibration
        };

        ShowElementsOnly(element_list);
        //GotoPage(1, 0);
    }

    public void HideRelocalizationPage()
    {
        transPanelCalibration.gameObject.SetActive(false);
    }

    public void GoIntoGame()
    {
        ShowElementsOnly(null);
        //GotoPage(2);
    }

    public void ShowWarningText(string msg)
    {
        transPanelWarning.Find("Message").GetComponent<TextMeshProUGUI>().text = msg;
        transPanelWarning.gameObject.SetActive(true);

        StartCoroutine(HideWarning());
    }

    IEnumerator HideWarning()
    {
        yield return new WaitForSeconds(2);
        transPanelWarning.gameObject.SetActive(false);
    }

    void ShowElementsOnly(Transform[] dst_elemetns)
    {
        foreach (Transform element in registeredUIElements.Values)
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

    void ShowExtraMenu()
    {
        transPanelExtraMenu.gameObject.SetActive(true);

        transPanelExtraMenu.Find("Button_InfoPanel").gameObject.SetActive(GameManager.Instance.IsInDevelopment);
        transPanelExtraMenu.Find("Button_ControllerPanel").gameObject.SetActive(GameManager.Instance.RoleManager.Role == RoleManager.PlayerRole.Server || GameManager.Instance.RoleManager.Role == RoleManager.PlayerRole.Commander);
        transPanelExtraMenu.Find("Button_OscSenderPanel").gameObject.SetActive(GameManager.Instance.RoleManager.Role == RoleManager.PlayerRole.Server);
    }

    void HideExtraMenu()
    {
        transPanelExtraMenu.gameObject.SetActive(false);
    }

    void ToggleInfoPanel()
    {
        Helper.Instance.ToggleInfoPanel();

        HideExtraMenu();
    }

    void ToggleControlPanel()
    {
        ControlPanel.Instance.ToggleDisplayPanel();

        HideExtraMenu();
    }

    void ToggleOscSenderPanel()
    {
        HideExtraMenu();
    }
    #endregion

    /// <summary>
    /// 
    /// PAGE INDEX
    ///  0 - Home Page
    ///  1 - Settings Page
    ///     - Performer Page
    ///     - ServerIp Page
    ///  2 - Waiting Page
    ///  3 - Calibration Page
    ///  4 - Game
    ///  
    /// </summary>


    /// <summary>
    /// 
    /// Page 0: Home Page
    ///     Start       ->      Page 1: Panel 0
    ///     Performer   ->      Page 1: Panel 1
    ///     Settings    ->      Page 1: Panel 2
    ///
    /// Page 2: Game Page(Hide UI)
    /// 
    /// </summary>
    /// <param name="page_index"></param>
    /// <param name="panel_index"></param>
    //void GotoPage(int page_index, int panel_index = 0)
    //{
    //    Debug.Log("Go to Page " + page_index + "," + panel_index);
    //    // Home Page
    //    if (page_index == 0)
    //    {
    //        transButtonStart.gameObject.SetActive(true);
    //        transButtonPerformer.gameObject.SetActive(true);
    //        transButtonSettings.gameObject.SetActive(true);
    //        transButtonServer.gameObject.SetActive(GameManager.Instance.IsInDevelopment ? true : false);

    //        transPanelCalibration.gameObject.SetActive(false);
    //        transPanelPassword.gameObject.SetActive(false);
    //        transPanelServerIP.gameObject.SetActive(false);

    //        return;
    //    }


    //    // Calibration / Performer / ServerIp Page
    //    if (page_index == 1)
    //    {
    //        transButtonStart.gameObject.SetActive(false);
    //        transButtonPerformer.gameObject.SetActive(false);
    //        transButtonSettings.gameObject.SetActive(false);
    //        transButtonServer.gameObject.SetActive(false);
    //        switch (panel_index)
    //        {
    //            // Calibration
    //            case 0:
    //                transPanelCalibration.gameObject.SetActive(true);
    //                StartCalibration();
    //                break;
    //            //Performer
    //            case 1:
    //                transPanelPassword.gameObject.SetActive(true);
    //                inputPassword.text = GameManager.Instance.IsInDevelopment ? GameManager.Instance.PerformerPassword : "";
    //                break;
    //            // ServerIp Page
    //            case 2:
    //                transPanelServerIP.gameObject.SetActive(true);
    //                inputServerIP.text = "192.168.0.";
    //                break;
    //        }

    //        return;
    //    }


    //    // In Game
    //    if (page_index == 2)
    //    {
    //        transButtonStart.gameObject.SetActive(false);
    //        transButtonPerformer.gameObject.SetActive(false);
    //        transButtonSettings.gameObject.SetActive(false);
    //        transButtonServer.gameObject.SetActive(false);

    //        transPanelCalibration.gameObject.SetActive(false);
    //        transPanelPassword.gameObject.SetActive(false);
    //        transPanelServerIP.gameObject.SetActive(false);

    //        return;
    //    }
    //}


}
