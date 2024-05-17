using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using System;

public class DataProcessor : MonoBehaviour
{
    public float PerformerCount { get; set; }

    public ParameterToLive<float> PosXMin = new ParameterToLive<float>();
    public ParameterToLive<float> PosXMax = new ParameterToLive<float>();
    public ParameterToLive<float> PosYMin = new ParameterToLive<float>();
    public ParameterToLive<float> PosYMax = new ParameterToLive<float>();
    public ParameterToLive<float> PosZMin = new ParameterToLive<float>();
    public ParameterToLive<float> PosZMax = new ParameterToLive<float>();

    public ParameterToLive<float> VelMin = new ParameterToLive<float>();
    public ParameterToLive<float> VelMax = new ParameterToLive<float>();
    public ParameterToLive<float> AccMin = new ParameterToLive<float>();
    public ParameterToLive<float> AccMax = new ParameterToLive<float>();


    public ParameterToLive<float> DisTotal = new ParameterToLive<float>();
    public ParameterToLive<float> DisMin = new ParameterToLive<float>();
    public ParameterToLive<float> DisMax = new ParameterToLive<float>();


    public ParameterToLive<float> AngleMin = new ParameterToLive<float>();
    public ParameterToLive<float> AngleMax = new ParameterToLive<float>();
    public ParameterToLive<float> Area = new ParameterToLive<float>();


    List<Performer> performerList = new List<Performer>();

    RoleManager roleManager;

    bool running = false;

    bool propertiesRegistered = false;

    void Awake()
    {
        roleManager = FindObjectOfType<RoleManager>();
    }

    void Start()
    {
        Transform performer_root = roleManager.PerformerTransformRoot;
        for (int i = 0; i < performer_root.childCount; i++)
        {
            performerList.Add(performer_root.GetChild(i).GetComponent<Performer>());
        }

        RegisterProperties();
    }

    void RegisterProperties()
    {
        if (propertiesRegistered)
            return;

        Debug.Log("DataProcessor | RegisterProperties");

        RegisterPropertiesFromCoda();

        RegisterPropertiesToLive();

        propertiesRegistered = true;
    }

    void RegisterPropertiesToLive()
    {
        SenderForLive.Instance.RegisterOscPropertyToSend("/posXMin", PosXMin);
        SenderForLive.Instance.RegisterOscPropertyToSend("/posXMax", PosXMax);
        SenderForLive.Instance.RegisterOscPropertyToSend("/posYMin", PosYMin);
        SenderForLive.Instance.RegisterOscPropertyToSend("/posYMax", PosYMax);
        SenderForLive.Instance.RegisterOscPropertyToSend("/posZMin", PosZMin);
        SenderForLive.Instance.RegisterOscPropertyToSend("/posZMax", PosZMax);

        SenderForLive.Instance.RegisterOscPropertyToSend("/velMin", VelMin);
        SenderForLive.Instance.RegisterOscPropertyToSend("/velMax", VelMax);
        SenderForLive.Instance.RegisterOscPropertyToSend("/accMin", AccMin);
        SenderForLive.Instance.RegisterOscPropertyToSend("/accMax", AccMax);

        SenderForLive.Instance.RegisterOscPropertyToSend("/disTotal", DisTotal);
        SenderForLive.Instance.RegisterOscPropertyToSend("/disMin", DisMin);
        SenderForLive.Instance.RegisterOscPropertyToSend("/disMax", DisMax);

        SenderForLive.Instance.RegisterOscPropertyToSend("/angleMin", AngleMin);
        SenderForLive.Instance.RegisterOscPropertyToSend("/angleMax", AngleMax);
        SenderForLive.Instance.RegisterOscPropertyToSend("/area", Area);
    }

    void RegisterPropertiesFromCoda()
    {
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/posXMin", new UnityAction<float>(OnReceive_posXMin));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/posXMax", new UnityAction<float>(OnReceive_posXMax));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/posYMin", new UnityAction<float>(OnReceive_posYMin));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/posYMax", new UnityAction<float>(OnReceive_posYMax));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/posZMin", new UnityAction<float>(OnReceive_posZMin));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/posZMax", new UnityAction<float>(OnReceive_posZMax));

        ParameterReceiver.Instance.RegisterOscReceiverFunction("/velMin", new UnityAction<float>(OnReceive_velMin));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/velMax", new UnityAction<float>(OnReceive_velMax));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/accMin", new UnityAction<float>(OnReceive_accMin));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/accMax", new UnityAction<float>(OnReceive_accMax));

        ParameterReceiver.Instance.RegisterOscReceiverFunction("/disTotal", new UnityAction<float>(OnReceive_disTotal));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/disMin", new UnityAction<float>(OnReceive_disMin));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/disMax", new UnityAction<float>(OnReceive_disMax));

        ParameterReceiver.Instance.RegisterOscReceiverFunction("/angleMin", new UnityAction<float>(OnReceive_angleMin));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/angleMax", new UnityAction<float>(OnReceive_angleMax));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/area", new UnityAction<float>(OnReceive_area));
    }


    public void TurnOn()
    {
        Debug.Log("DataProcessor | TurnOn");

        RegisterProperties();

        running = true;
    }

    public void TurnOff()
    {
        Debug.Log("DataProcessor | TurnOff");

        running = false;
    }

    void Update()
    {
        if (running == false || !NetworkManager.Singleton.IsServer)
            return;

        CalculateDataToLive();
    }

    void CalculateDataToLive()
    {
        PerformerCount = GameManager.Instance.RoleManager.PerformerCount;

        // Pos / Speed / Acclerate
        if (PerformerCount > 0)
        {
            Vector3 pos_min = Vector3.one * float.MaxValue;
            Vector3 pos_max = Vector3.one * float.MinValue;

            float vel_min = float.MaxValue;
            float vel_max = float.MinValue;
            float acc_min = float.MaxValue;
            float acc_max = float.MinValue;
            for (int i = 0; i < performerList.Count; i++)
            {
                Performer performer = performerList[i];
                if (performer.localData.isPerforming == false)
                    continue;
                pos_min = Vector3.Min(pos_min, performer.localData.position);
                pos_max = Vector3.Max(pos_max, performer.localData.position);

                vel_min = Mathf.Min(vel_min, performer.localData.velocity.magnitude);
                vel_max = Mathf.Max(vel_max, performer.localData.velocity.magnitude);

                acc_min = Mathf.Min(acc_min, performer.localData.acceleration.magnitude);
                acc_max = Mathf.Max(acc_max, performer.localData.acceleration.magnitude);
            }

            PosXMin.OrginalValue = pos_min.x; PosYMin.OrginalValue = pos_min.y; PosZMin.OrginalValue = pos_min.z;
            PosXMax.OrginalValue = pos_max.x; PosYMax.OrginalValue = pos_max.y; PosZMax.OrginalValue = pos_max.z;

            VelMin.OrginalValue = vel_min; VelMax.OrginalValue = vel_max;
            AccMin.OrginalValue = acc_min; AccMax.OrginalValue = acc_max;
        }

        // Distance 
        if (PerformerCount > 1)
        {
            float dis_total = 0;
            float dis_min = float.MaxValue;
            float dis_max = float.MinValue;

            for (int i = 0; i < performerList.Count; i++)
            {
                Performer performer = performerList[i];

                if (performer.localData.isPerforming == false)
                    continue;

                Vector3 p1 = performer.localData.position;
                for (int k = i + 1; k < performerList.Count; k++)
                {
                    Performer performer2 = performerList[k];
                    if (performer2.localData.isPerforming == false)
                        continue;

                    Vector3 p2 = performer2.localData.position;

                    float distance = Vector3.Distance(p1, p2);
                    dis_total += distance;
                    dis_min = Mathf.Min(dis_min, distance);
                    dis_max = Mathf.Max(dis_max, distance);
                }
            }

            DisTotal.OrginalValue = dis_total;
            DisMin.OrginalValue = dis_min; DisMax.OrginalValue = dis_max;
        }

        // Area / Angle
        if (PerformerCount == 3)
        {
            Vector3 p1 = performerList[0].localData.position;
            Vector3 p2 = performerList[1].localData.position;
            Vector3 p3 = performerList[2].localData.position;

            Vector3 norm = Vector3.Cross(p1 - p2, p1 - p3);
            Area.OrginalValue = norm.magnitude * 0.5f;


            float angle1 = Vector3.Angle(p1 - p2, p1 - p3);
            float angle2 = Vector3.Angle(p2 - p1, p2 - p3);
            float angle3 = Vector3.Angle(p3 - p1, p3 - p2);

            AngleMin.OrginalValue = Mathf.Min(angle1, Mathf.Min(angle2, angle3));
            AngleMax.OrginalValue = Mathf.Max(angle1, Mathf.Max(angle2, angle3));
        }
    }

    #region Callbacks
    private void OnReceive_posXMin(float v)
    {
        PosXMin.CodaValue = v;
    }

    private void OnReceive_posXMax(float v)
    {
        PosXMax.CodaValue = v;
    }

    private void OnReceive_posYMin(float v)
    {
        PosYMin.CodaValue = v;
    }

    private void OnReceive_posYMax(float v)
    {
        PosYMax.CodaValue = v;
    }

    private void OnReceive_posZMin(float v)
    {
        PosZMin.CodaValue = v;
    }

    private void OnReceive_posZMax(float v)
    {
        PosZMax.CodaValue = v;
    }

    private void OnReceive_velMin(float v)
    {
        VelMin.CodaValue = v;
    }

    private void OnReceive_velMax(float v)
    {
        VelMax.CodaValue = v;
    }

    private void OnReceive_accMin(float v)
    {
        AccMin.CodaValue = v;
    }

    private void OnReceive_accMax(float v)
    {
        AccMax.CodaValue = v;
    }

    private void OnReceive_disTotal(float v)
    {
        DisTotal.CodaValue = v;
    }

    private void OnReceive_disMin(float v)
    {
        DisMin.CodaValue = v;
    }

    private void OnReceive_disMax(float v)
    {
        DisMax.CodaValue = v;
    }

    private void OnReceive_angleMin(float v)
    {
        AngleMin.CodaValue = v;
    }

    private void OnReceive_angleMax(float v)
    {
        AngleMax.CodaValue = v;
    }

    private void OnReceive_area(float v)
    {
        Area.CodaValue = v;
    }
    #endregion
}
