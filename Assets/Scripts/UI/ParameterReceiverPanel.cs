using OscJack;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class ParameterReceiverPanel : MonoBehaviour
{
    [SerializeField] Transform panelRoot;
    [SerializeField] Transform parameterRoot;
    [SerializeField] GameObject prefabParameterItem;

    bool showControlPanel = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) && NetworkManager.Singleton.IsServer)
        {
            if (showControlPanel) HideControlPanel();
            else ShowControlPanel();
        }
    }

    public void AddProperyInControlPanel(OscPropertyForReceiving property)
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

    public void RemoveAllPropertyInControlPanel()
    {
        foreach (Transform child in parameterRoot)
        {
            Destroy(child.gameObject);
        }
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
    private static ParameterReceiverPanel _Instance;

    public static ParameterReceiverPanel Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<ParameterReceiverPanel>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    _Instance = go.AddComponent<ParameterReceiverPanel>();
                }
            }
            return _Instance;
        }
    }
    #endregion
}
