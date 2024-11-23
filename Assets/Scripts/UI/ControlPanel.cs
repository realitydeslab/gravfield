using OscJack;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class ControlPanel : MonoBehaviour
{
    [SerializeField] Transform transformControlPanel;
    [SerializeField] Transform parameterRoot;
    [SerializeField] GameObject prefabParameterItem;

    bool showControlPanel = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6) && NetworkManager.Singleton.IsServer)
        {
            if (showControlPanel) HideDisplayPanel();
            else ShowDisplayPanel();
        }
    }

    public void AddProperyInControlPanel_ServerMode(OscPropertyForReceiving property)
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

    public void AddProperyInControlPanel_CommanderMode()
    {

    }

    public void ClearAllPropertyInControlPanel()
    {

    }

    public void OnUpdateDisplay(string address, float v)
    {
        if (showControlPanel == false)
            return;

        Transform item = parameterRoot.Find(address.Substring(1));
        if (item != null)
        {
            item.Find("Value").GetComponent<TextMeshProUGUI>().text = v.ToString();
        }
    }

    public void ShowDisplayPanel()
    {
        transformControlPanel.gameObject.SetActive(true);
        showControlPanel = true;
    }
    public void HideDisplayPanel()
    {
        transformControlPanel.gameObject.SetActive(false);
        showControlPanel = false;
    }
    public void ToggleDisplayPanel()
    {
        if (showControlPanel) HideDisplayPanel();
        else ShowDisplayPanel();
    }


    #region Instance
    private static ControlPanel _Instance;

    public static ControlPanel Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<ControlPanel>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    _Instance = go.AddComponent<ControlPanel>();
                }
            }
            return _Instance;
        }
    }
    #endregion
}
