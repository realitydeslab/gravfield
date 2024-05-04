using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using HoloKit.ImageTrackingRelocalization;

public class UIController : MonoBehaviour
{

    Transform transButtonStart;
    Transform transButtonPerformer;
    Transform transButtonSettings;
    Transform transPanelCalibration;
    Transform transPanelPassword;
    Transform transPanelServerIP;
    Transform transPanelWarning;

    TMP_InputField inputPassword;
    TMP_InputField inputServerIP;

    void Start()
    {
        // Initialize
        transButtonStart = transform.Find("Button_Start");
        transButtonPerformer = transform.Find("Button_Performer");
        transButtonSettings = transform.Find("Button_Settings");
        transPanelCalibration = transform.Find("Panel_Calibration");
        transPanelPassword = transform.Find("Panel_Password");
        transPanelServerIP = transform.Find("Panel_ServerIP");
        transPanelWarning = transform.Find("Panel_Warning");


        if (transButtonStart == null || transButtonPerformer == null || transButtonSettings == null
            || transPanelCalibration == null || transPanelPassword == null || transPanelServerIP == null
            || transPanelWarning == null)
        {
            Debug.LogError("Can't find UI elements properly.");
            return;
        }

        inputPassword = transPanelPassword.Find("InputField_Password").GetComponent<TMP_InputField>();
        inputServerIP = transPanelServerIP.Find("InputField_ServerIP").GetComponent<TMP_InputField>();


        // Bind Basic Listener
        transButtonStart.GetComponent<Button>().onClick.AddListener(() => GotoPage(1, 0));
        transButtonPerformer.GetComponent<Button>().onClick.AddListener(() => GotoPage(1, 1));
        transButtonSettings.GetComponent<Button>().onClick.AddListener(() => GotoPage(1, 2));

        transPanelCalibration.Find("Button_Close").GetComponent<Button>().onClick.AddListener(() => GotoPage(0));
        transPanelPassword.Find("Button_Close").GetComponent<Button>().onClick.AddListener(() => GotoPage(0));
        transPanelServerIP.Find("Button_Close").GetComponent<Button>().onClick.AddListener(() => GotoPage(0));

        transPanelPassword.Find("Button_Enter").GetComponent<Button>().onClick.AddListener(OnEnterPassword);
        transPanelServerIP.Find("Button_Enter").GetComponent<Button>().onClick.AddListener(OnEnterServerIp);

        // Enter Home Page
        GotoPage(0);
    }

    public void OnFinishRelocalization(Vector3 position, Quaternion rotation)
    {
        GameManager.Instance.RelocalizationStablizer.OnTrackedImagePoseStablized.RemoveListener(OnFinishRelocalization);

        // Join As Audience
        GameManager.Instance.JoinAsAudience();
        GotoPage(2);
    }

    public void OnEnterPassword()
    {
        string password = inputPassword.text;


        if (password == GameManager.Instance.PerformerPassword)
        {
            // Join As Performer
            GameManager.Instance.JoinAsPerformer();
            GotoPage(2);
        }
        else
        {
            // Show Error Info
            DisplayMessageOnUI("Wrong Password.");
        }

    }

    public void OnEnterServerIp()
    {
        string server_ip = inputServerIP.text;

        if (Regex.IsMatch(server_ip, @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$"))
        {
            // Change Server Ip
            GameManager.Instance.ServerIP = server_ip;
            GotoPage(0);
        }
        else
        {
            // Show Error Info
            DisplayMessageOnUI("Wrong ServerIp.");
        }
    }

    public void DisplayMessageOnUI(string msg)
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


    void GotoPage(int page_index, int panel_index = 0)
    {
        // Home Page
        if (page_index == 0)
        {
            transButtonStart.gameObject.SetActive(true);
            transButtonPerformer.gameObject.SetActive(true);
            transButtonSettings.gameObject.SetActive(true);

            transPanelCalibration.gameObject.SetActive(false);
            transPanelPassword.gameObject.SetActive(false);
            transPanelServerIP.gameObject.SetActive(false);

            return;
        }


        // Calibration / Performer / ServerIp Page
        if (page_index == 1)
        {
            transButtonStart.gameObject.SetActive(false);
            transButtonPerformer.gameObject.SetActive(false);
            transButtonSettings.gameObject.SetActive(false);
            switch (panel_index)
            {
                // Calibration
                case 0:
                    transPanelCalibration.gameObject.SetActive(true);
                    StartCalibration();
                    break;
                //Performer
                case 1:
                    transPanelPassword.gameObject.SetActive(true);
                    inputPassword.text = GameManager.Instance.IsInDevelopment ? GameManager.Instance.PerformerPassword : "";
                    break;
                // ServerIp Page
                case 2:
                    transPanelServerIP.gameObject.SetActive(true);
                    inputServerIP.text = "192.168.0.";
                    break;
            }

            return;
        }


        // In Game
        if (page_index == 2)
        {
            transButtonStart.gameObject.SetActive(false);
            transButtonPerformer.gameObject.SetActive(false);
            transButtonSettings.gameObject.SetActive(false);

            transPanelCalibration.gameObject.SetActive(false);
            transPanelPassword.gameObject.SetActive(false);
            transPanelServerIP.gameObject.SetActive(false);

            return;
        }
    }


    void StartCalibration()
    {
#if !UNITY_EDITOR
        ImageTrackingStablizer stablizer = GameManager.Instance.RelocalizationStablizer;
        stablizer.OnTrackedImagePoseStablized.AddListener(OnFinishRelocalization);
        stablizer.IsRelocalizing = true;
#else
        OnFinishRelocalization(Vector3.zero, Quaternion.identity);
#endif
    }
}
