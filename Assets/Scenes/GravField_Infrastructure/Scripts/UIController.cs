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
    Transform transPanelServerIp;
    Transform transPanelWarning;

    void Start()
    {
        // Initialize
        transButtonStart = transform.Find("Button_Start");
        transButtonPerformer = transform.Find("Button_Performer");
        transButtonSettings = transform.Find("Button_Settings");
        transPanelCalibration = transform.Find("Panel_Calibration");
        transPanelPassword = transform.Find("Panel_Password");
        transPanelServerIp = transform.Find("Panel_ServerIp");
        transPanelWarning = transform.Find("Panel_Warning");

        if (transButtonStart == null || transButtonPerformer == null || transButtonSettings == null
            || transPanelCalibration == null || transPanelPassword == null || transPanelServerIp == null
            || transPanelWarning == null)
        {
            Debug.LogError("Can't find UI elements properly.");
            return;
        }

        // Bind Basic Listener
        transButtonStart.GetComponent<Button>().onClick.AddListener(() => GotoPage(1, 0));
        transButtonPerformer.GetComponent<Button>().onClick.AddListener(() => GotoPage(1, 1));
        transButtonSettings.GetComponent<Button>().onClick.AddListener(() => GotoPage(1, 2));

        transPanelCalibration.Find("Button_Close").GetComponent<Button>().onClick.AddListener(() => GotoPage(0));
        transPanelPassword.Find("Button_Close").GetComponent<Button>().onClick.AddListener(() => GotoPage(0));
        transPanelServerIp.Find("Button_Close").GetComponent<Button>().onClick.AddListener(() => GotoPage(0));

        transPanelPassword.Find("Button_Enter").GetComponent<Button>().onClick.AddListener(OnEnterPassword);
        transPanelServerIp.Find("Button_Enter").GetComponent<Button>().onClick.AddListener(OnEnterServerIp);

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
        string password = transPanelPassword.Find("InputField_Password").GetComponent<TMP_InputField>().text;


        if (password == GameManager.Instance.PerformerPassword)
        {
            // Join As Performer
            GameManager.Instance.JoinAsPerformer();
            GotoPage(2);
        }
        else
        {
            // Show Error Info
            ShowWarning("Wrong Password.");
        }

    }

    public void OnEnterServerIp()
    {
        string server_ip = transPanelServerIp.Find("InputField_ServerIp").GetComponent<TMP_InputField>().text;

        if (Regex.IsMatch(server_ip, @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$"))
        {
            // Change Server Ip
            GameManager.Instance.ServerIp = server_ip;
            GotoPage(0);
        }
        else
        {
            // Show Error Info
            ShowWarning("Wrong ServerIp.");
        }
    }

    public void ShowWarning(string msg)
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
            transPanelServerIp.gameObject.SetActive(false);

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
                case 0:
                    transPanelCalibration.gameObject.SetActive(true);
                    StartCalibration();
                    break;
                case 1:
                    transPanelPassword.gameObject.SetActive(true);
                    break;
                case 2:
                    transPanelServerIp.gameObject.SetActive(true);
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
            transPanelServerIp.gameObject.SetActive(false);

            return;
        }
    }


    void StartCalibration()
    {
#if UNITY_IOS
        ImageTrackingStablizer stablizer = GameManager.Instance.RelocalizationStablizer;
        stablizer.OnTrackedImagePoseStablized.AddListener(OnFinishRelocalization);
        stablizer.IsRelocalizing = true;
#else
        OnFinishRelocalization(Vector3.zero, Quaternion.identity);
#endif
    }
}
