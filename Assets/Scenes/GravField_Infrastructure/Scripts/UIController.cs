using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HoloKit.ImageTrackingRelocalization;
using System;

public class UIController : MonoBehaviour
{
    Transform transButtonStart;
    Transform transButtonPerformer;
    Transform transButtonSettings;
    Transform transButtonServer;

    Transform transPanelCalibration;
    Transform transPanelPassword;
    Transform transPanelServerIP;
    Transform transPanelWarning;
    Transform transPanelWaiting;

    TMP_InputField inputPassword;
    TMP_InputField inputServerIP;
    TextMeshProUGUI waitingMessageText;

    Dictionary<string, Transform> registeredUIElements = new Dictionary<string, Transform>();

    void Awake()
    {
        // UI Elements
        transButtonStart = FindTransformAndRegister("Button_Start"); 
        transButtonPerformer = FindTransformAndRegister("Button_Performer");
        transButtonSettings = FindTransformAndRegister("Button_Settings");
        transButtonServer = FindTransformAndRegister("Button_Server"); transButtonServer.gameObject.SetActive(GameManager.Instance.IsInDevelopment ? true : false);

        transPanelPassword = FindTransformAndRegister("Panel_Password");
        transPanelServerIP = FindTransformAndRegister("Panel_ServerIP");

        transPanelWaiting = FindTransformAndRegister("Panel_Waiting");

        transPanelCalibration = FindTransformAndRegister("Panel_Calibration");
        
        transPanelWarning = transform.Find("Panel_Warning"); // WarningPanel is different because it's independent

        // Text Field
        inputPassword = transPanelPassword.Find("InputField_Password").GetComponent<TMP_InputField>();
        inputServerIP = transPanelServerIP.Find("InputField_ServerIP").GetComponent<TMP_InputField>();
        waitingMessageText = transPanelWaiting.Find("Message").GetComponent<TextMeshProUGUI>();
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
        transButtonStart.GetComponent<Button>().onClick.AddListener(() => OnClickStart());
        transButtonServer.GetComponent<Button>().onClick.AddListener(() => OnClickServer());
        transButtonPerformer.GetComponent<Button>().onClick.AddListener(() => OnClickPerformer());
        transButtonSettings.GetComponent<Button>().onClick.AddListener(() => OnClickSettings());

        transPanelPassword.Find("Button_Enter").GetComponent<Button>().onClick.AddListener(OnEnterPassword);
        transPanelServerIP.Find("Button_Enter").GetComponent<Button>().onClick.AddListener(OnEnterServerIp);

        transPanelPassword.Find("Button_Close").GetComponent<Button>().onClick.AddListener(() => GoBackToHomePage());
        transPanelServerIP.Find("Button_Close").GetComponent<Button>().onClick.AddListener(() => GoBackToHomePage());
        transPanelCalibration.Find("Button_Close").GetComponent<Button>().onClick.AddListener(() => HideRelocalizationPage());        
    }


    #region Responsive Function
    void OnClickStart()
    {
        GameManager.Instance.JoinAsAudience();
    }

    void OnClickPerformer()
    {
        GotoPasswordPage();
    }

    void OnClickSettings()
    {
        GotoServerIpPage();
    }

    void OnClickServer()
    {
        GameManager.Instance.JoinAsServer();
    }

    public void OnEnterPassword()
    {
        GameManager.Instance.JoinAsPerformer(inputPassword.text);
    }

    public void OnEnterServerIp()
    {
        if(GameManager.Instance.ConnectionManager.IsIPAddressValide(inputServerIP.text))
        {
            GameManager.Instance.ConnectionManager.SetServerIP(inputServerIP.text);
            GoBackToHomePage();
        }
        else
        {
            ShowWarningText("Wrong ServerIp.");
        }
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
            transButtonSettings,
            transButtonServer
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
        if(delay != 0 && callback != null)
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

    void ShowElementsOnly(Transform []dst_elemetns)
    {
        foreach(Transform element in registeredUIElements.Values)
        {
            bool match = false;
            if(dst_elemetns != null)
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
