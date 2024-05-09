using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;
using UnityEngine.UI;
using TMPro;

public class PerformerData
{
    public PerformerData(bool _is_performing, Vector3 _pos)
    {
        isPerforming = _is_performing;
        position = _pos;
    }

    public bool isPerforming;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
}


public class Middleware : MonoBehaviour
{
    public Transform transformOscToLive;

    public bool controlPanelEnabled = false;
    public Transform transformControlPanel;
    public GameObject prefabPropertyItem;

    public float PerformerCount { get; set; }

    public float PosXMin { get; set; }
    public float PosXMax { get; set; }
    public float PosYMin { get; set; }
    public float PosYMax { get; set; }
    public float PosZMin { get; set; }
    public float PosZMax { get; set; }


    public float DisTotal { get; set; }
    public float DisMin { get; set; }
    public float DisMax { get; set; }
    public float VelMin { get; set; }
    public float VelMax { get; set; }
    public float AccMin { get; set; }
    public float AccMax { get; set; }

    public float AngleMin { get; set; }
    public float AngleMax { get; set; }

    public float Area { get; set; }


    bool middlewareEnabled = false;
    RoleManager roleManager;
    List<Performer> performerList = new List<Performer>();
    List<PerformerData> performerDataList = new List<PerformerData>();


    List<OscPropertySenderRange> livePropertyList = new List<OscPropertySenderRange>();


    void Start()
    {
        

        
    }

    void Update()
    {
        if (middlewareEnabled == false)
            return;

        UpdatePerformerData();

        CalculateDataToLive();

        UpdateControlPanel();
    }



    void UpdatePerformerData()
    {
        int performer_count = 0;
        for(int i=0; i<performerDataList.Count; i++)
        {
            Performer performer = performerList[i];
            PerformerData performer_data = performerDataList[i];

            // Just Start Performing
            if (performer.isPerforming.Value == true && performer_data.isPerforming == false)
            {
                performer_data.position = performer.transform.localPosition;
                performer_data.velocity = Vector3.zero;
                performer_data.acceleration = Vector3.zero;
            }
            // Just Stop Performing
            else if (performer.isPerforming.Value == false && performer_data.isPerforming == true)
            {
                
            }
            performer_data.isPerforming = performer.isPerforming.Value;


            if (performerDataList[i].isPerforming == false)
                continue;

            Vector3 new_pos = performer.transform.localPosition;
            Vector3 new_vel = (new_pos - performer_data.position) / Time.deltaTime;

            performer_data.acceleration = (new_vel - performer_data.velocity) / Time.deltaTime;
            performer_data.velocity = new_vel;
            performer_data.position = new_pos;

            performer_count++;
        }
        PerformerCount = performer_count;
    }



    void CalculateDataToLive()
    {
        // Pos / Speed / Acclerate
        if (PerformerCount > 0)
        {
            PosXMin = float.MaxValue;
            PosXMax = float.MinValue;
            PosYMin = float.MaxValue;
            PosYMax = float.MinValue;
            PosZMin = float.MaxValue;
            PosZMax = float.MinValue;

            VelMin = float.MaxValue;
            VelMax = float.MinValue;
            AccMin = float.MaxValue;
            AccMax = float.MinValue;
            for (int i = 0; i < performerDataList.Count; i++)
            {
                PerformerData performer = performerDataList[i];
                if (performer.isPerforming == false)
                    continue;

                PosXMin = Mathf.Min(PosXMin, performer.position.x);
                PosXMax = Mathf.Max(PosXMax, performer.position.x);
                PosYMin = Mathf.Min(PosYMin, performer.position.y);
                PosYMax = Mathf.Max(PosYMax, performer.position.y);
                PosZMin = Mathf.Min(PosZMin, performer.position.z);
                PosZMax = Mathf.Max(PosZMax, performer.position.z);

                VelMin = Mathf.Min(VelMin, performer.velocity.magnitude);
                VelMax = Mathf.Max(VelMax, performer.velocity.magnitude);
                AccMin = Mathf.Min(AccMin, performer.acceleration.magnitude);
                AccMax = Mathf.Max(AccMax, performer.acceleration.magnitude);
            }
        }

        // Distance 
        if(PerformerCount > 1)
        {
            DisTotal = 0;
            DisMin = float.MaxValue;
            DisMax = float.MinValue;

            for (int i = 0; i < performerDataList.Count; i++)
            {
                PerformerData performer = performerDataList[i];

                if (performer.isPerforming == false)
                    continue;

                Vector3 p1 = performer.position;
                for (int k = i + 1; k < performerList.Count; k++)
                {
                    PerformerData performer2 = performerDataList[k];
                    if (performer2.isPerforming == false)
                        continue;

                    Vector3 p2 = performer2.position;

                    float distance = Vector3.Distance(p1, p2);
                    DisTotal += distance;
                    DisMin = Mathf.Min(DisMin, distance);
                    DisMax = Mathf.Max(DisMax, distance);
                }
            }
        }




        // Area / Angle
        if (PerformerCount == 3)
        {
            Vector3 p1 = performerDataList[0].position;
            Vector3 p2 = performerDataList[1].position;
            Vector3 p3 = performerDataList[2].position;

            Vector3 norm = Vector3.Cross(p1 - p2, p1 - p3);
            Area = norm.magnitude * 0.5f;


            float angle1 = Vector3.Angle(p1 - p2, p1 - p3);
            float angle2 = Vector3.Angle(p2 - p1, p2 - p3);
            float angle3 = Vector3.Angle(p3 - p1, p3 - p2);

            AngleMin = Mathf.Min(angle1, Mathf.Min(angle2, angle3));
            AngleMax = Mathf.Max(angle1, Mathf.Max(angle2, angle3));
        }
    }

    void UpdateControlPanel()
    {
        if (controlPanelEnabled == false)
            return;

        for (int i = 0; i < livePropertyList.Count; i++)
        {
            OscPropertySenderRange property = livePropertyList[i];
            Transform item = transformControlPanel.transform.GetChild(i);
            if (item.name != property._oscAddress)
                continue;

            item.Find("OriginalValue").GetComponent<TextMeshProUGUI>().text = PropertyValueToString(property._originalFloatValue);
            item.Find("Value").GetComponent<TextMeshProUGUI>().text = PropertyValueToString(property._floatValue);
        }
    }

    

    void Awake()
    {
        roleManager = FindObjectOfType<RoleManager>();

        DisableMiddleWare();
    }
    
    void OnEnable()
    {
        roleManager?.OnSpecifyPlayerRoleEvent.AddListener(OnSpecifyPlayerRole);
    }

    void OnDisable()
    {
        roleManager?.OnSpecifyPlayerRoleEvent.RemoveListener(OnSpecifyPlayerRole);
    }

    void OnSpecifyPlayerRole(RoleManager.PlayerRole role)
    {
        if (role == RoleManager.PlayerRole.Server && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            EnableMiddleWare();
        }
    }

    void EnableMiddleWare()
    {
        SetMiddlewareState(true);
    }

    void DisableMiddleWare()
    {
        SetMiddlewareState(false);
    }

    void SetMiddlewareState(bool enabled)
    {
        middlewareEnabled = enabled;

        for(int i=0; i<transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(enabled);
        }

        if(enabled)
        {
            // 
            performerList.Clear();
            performerDataList.Clear();
            Transform transform_root = roleManager.PerformerTransformRoot;
            for(int i=0; i<transform_root.childCount; i++)
            {
                Performer performer = transform_root.GetChild(i).GetComponent<Performer>();
                performerList.Add(performer);
                performerDataList.Add(new PerformerData(performer.isPerforming.Value, performer.transform.localPosition));
            }


            //
            livePropertyList.Clear();
            transformOscToLive.GetComponents<OscPropertySenderRange>(livePropertyList);



            //
            for (int i = 0; i < livePropertyList.Count; i++)
            {
                OscPropertySenderRange property = livePropertyList[i];
                GameObject new_item = Instantiate(prefabPropertyItem, transformControlPanel.transform);
                new_item.name = property._oscAddress;

                new_item.transform.Find("Toggle_Enabled").GetComponent<Toggle>().isOn = property.enabled;
                new_item.transform.Find("Address").GetComponent<TextMeshProUGUI>().text = property._oscAddress;
                new_item.transform.Find("OriginalValue").GetComponent<TextMeshProUGUI>().text = PropertyValueToString(property._originalFloatValue);
                new_item.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = PropertyValueToString(property._floatValue);
                new_item.transform.Find("Toggle_Clamp").GetComponent<Toggle>().isOn = property._needClamp;
                new_item.transform.Find("InputField_Min").GetComponent<TMP_InputField>().text = property._srcRange.x.ToString("0.00");
                new_item.transform.Find("InputField_Max").GetComponent<TMP_InputField>().text = property._srcRange.y.ToString("0.00");


                new_item.transform.Find("Toggle_Enabled").GetComponent<Toggle>().onValueChanged.AddListener((bool v) =>{
                    property.enabled = v;
                });
                new_item.transform.Find("Toggle_Clamp").GetComponent<Toggle>().onValueChanged.AddListener((bool v) => {
                    property._needClamp = v;
                });
                new_item.transform.Find("InputField_Min").GetComponent<TMP_InputField>().onEndEdit.AddListener((string str) => {
                    Debug.Log("Edit InputField_Min:" + property._oscAddress);
                    try
                    {
                        property._srcRange.x = float.Parse(str);
                    }
                    catch
                    {}

                });

                new_item.transform.Find("InputField_Max").GetComponent<TMP_InputField>().onEndEdit.AddListener((string str) => {
                    Debug.Log("Edit InputField_Max:" + property._oscAddress);
                    try
                    {
                        property._srcRange.y = float.Parse(str);
                    }
                    catch
                    {}
                });
            }

            transformControlPanel.gameObject.SetActive(controlPanelEnabled);
        }
    }

    string PropertyValueToString(float v)
    {
        return Mathf.Abs(v) > 10000 ? "NaN" : v.ToString("0.00");
    }
}
