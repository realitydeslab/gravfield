using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using System;

public class DataProcessor : MonoBehaviour
{
    public float PerformerCount { get; set; }

    public AutoSwitchedParameter<float> PosXMin = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> PosXMax = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> PosYMin = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> PosYMax = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> PosZMin = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> PosZMax = new AutoSwitchedParameter<float>();

    public AutoSwitchedParameter<float> VelMin = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> VelMax = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> AccMin = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> AccMax = new AutoSwitchedParameter<float>();


    public AutoSwitchedParameter<float> DisTotal = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> DisMin = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> DisMax = new AutoSwitchedParameter<float>();


    public AutoSwitchedParameter<float> AngleMin = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> AngleMax = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> Area = new AutoSwitchedParameter<float>();

    public AutoSwitchedParameter<float> ay = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> by = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> cy = new AutoSwitchedParameter<float>();


    public AutoSwitchedParameter<float> disab = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> disac = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> disbc = new AutoSwitchedParameter<float>();

    public AutoSwitchedParameter<float> valuef1 = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> valuef2 = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> valuef3 = new AutoSwitchedParameter<float>();

    public AutoSwitchedParameter<float> valuef4 = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> valuef5 = new AutoSwitchedParameter<float>();
    public AutoSwitchedParameter<float> valuef6 = new AutoSwitchedParameter<float>();


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
    }

    void RegisterProperties()
    {
        if (propertiesRegistered || !NetworkManager.Singleton.IsServer)
            return;

        Debug.Log("DataProcessor | RegisterProperties");

        RegisterPropertiesFromCoda_Server();

        RegisterPropertiesToLive_Server();

        propertiesRegistered = true;
    }

    void RegisterPropertiesToLive_Server()
    {
        //SenderForLive.Instance.RegisterOscPropertyToSend("/posXMin", PosXMin);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/posXMax", PosXMax);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/posYMin", PosYMin);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/posYMax", PosYMax);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/posZMin", PosZMin);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/posZMax", PosZMax);

        //SenderForLive.Instance.RegisterOscPropertyToSend("/velMin", VelMin);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/velMax", VelMax);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/accMin", AccMin);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/accMax", AccMax);

        //SenderForLive.Instance.RegisterOscPropertyToSend("/disTotal", DisTotal);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/disMin", DisMin);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/disMax", DisMax);

        //SenderForLive.Instance.RegisterOscPropertyToSend("/angleMin", AngleMin);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/angleMax", AngleMax);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/area", Area);

        SenderForLive.Instance.RegisterOscPropertyToSend("/ay", ay);
        SenderForLive.Instance.RegisterOscPropertyToSend("/by", by);
        SenderForLive.Instance.RegisterOscPropertyToSend("/cy", cy);

        SenderForLive.Instance.RegisterOscPropertyToSend("/disab", disab);
        SenderForLive.Instance.RegisterOscPropertyToSend("/disac", disac);
        SenderForLive.Instance.RegisterOscPropertyToSend("/disbc", disbc);

        SenderForLive.Instance.RegisterOscPropertyToSend("/valuef1", valuef1);
        SenderForLive.Instance.RegisterOscPropertyToSend("/valuef2", valuef2);
        SenderForLive.Instance.RegisterOscPropertyToSend("/valuef3", valuef3);

        SenderForLive.Instance.RegisterOscPropertyToSend("/valuef4", valuef4);
        SenderForLive.Instance.RegisterOscPropertyToSend("/valuef5", valuef5);
        SenderForLive.Instance.RegisterOscPropertyToSend("/valuef6", valuef6);
    }

    void RegisterPropertiesFromCoda_Server()
    {
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/posXMin", new UnityAction<float>(OnReceive_posXMin));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/posXMax", new UnityAction<float>(OnReceive_posXMax));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/posYMin", new UnityAction<float>(OnReceive_posYMin));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/posYMax", new UnityAction<float>(OnReceive_posYMax));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/posZMin", new UnityAction<float>(OnReceive_posZMin));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/posZMax", new UnityAction<float>(OnReceive_posZMax));

        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/velMin", new UnityAction<float>(OnReceive_velMin));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/velMax", new UnityAction<float>(OnReceive_velMax));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/accMin", new UnityAction<float>(OnReceive_accMin));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/accMax", new UnityAction<float>(OnReceive_accMax));

        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/disTotal", new UnityAction<float>(OnReceive_disTotal));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/disMin", new UnityAction<float>(OnReceive_disMin));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/disMax", new UnityAction<float>(OnReceive_disMax));

        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/angleMin", new UnityAction<float>(OnReceive_angleMin));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/angleMax", new UnityAction<float>(OnReceive_angleMax));
        //ParameterReceiver.Instance.RegisterOscReceiverFunction("/area", new UnityAction<float>(OnReceive_area));

        ParameterReceiver.Instance.RegisterOscReceiverFunction("/valuef1", new UnityAction<float>(OnReceive_valuef1));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/valuef2", new UnityAction<float>(OnReceive_valuef2));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/valuef3", new UnityAction<float>(OnReceive_valuef3));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/valuef4", new UnityAction<float>(OnReceive_valuef4));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/valuef5", new UnityAction<float>(OnReceive_valuef5));
        ParameterReceiver.Instance.RegisterOscReceiverFunction("/valuef6", new UnityAction<float>(OnReceive_valuef6));
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


        // ay/by/cy, disab,disac,disbc
        for (int i = 0; i < performerList.Count; i++)
        {
            if (performerList[i].isPerforming.Value == false)
                continue;
            
            if (i == 0) ay.OrginalValue = performerList[i].transform.position.y;
            else if (i == 1) by.OrginalValue = performerList[i].transform.position.y;
            else cy.OrginalValue = performerList[i].transform.position.y;
            
            for (int k = i+1; k < performerList.Count; k++)
            {
                if (performerList[k].isPerforming.Value == false)
                    continue;

                int index = i + k - 1;
                if (index == 0) disab.OrginalValue = Vector3.Distance(performerList[i].transform.position, performerList[k].transform.position);
                else if(index == 1) disac.OrginalValue = Vector3.Distance(performerList[i].transform.position, performerList[k].transform.position);
                else disbc.OrginalValue = Vector3.Distance(performerList[i].transform.position, performerList[k].transform.position);
            }
        }
    }

    #region Callbacks
    //private void OnReceive_posXMin(float v)
    //{
    //    PosXMin.CodaValue = v;
    //}

    //private void OnReceive_posXMax(float v)
    //{
    //    PosXMax.CodaValue = v;
    //}

    //private void OnReceive_posYMin(float v)
    //{
    //    PosYMin.CodaValue = v;
    //}

    //private void OnReceive_posYMax(float v)
    //{
    //    PosYMax.CodaValue = v;
    //}

    //private void OnReceive_posZMin(float v)
    //{
    //    PosZMin.CodaValue = v;
    //}

    //private void OnReceive_posZMax(float v)
    //{
    //    PosZMax.CodaValue = v;
    //}

    //private void OnReceive_velMin(float v)
    //{
    //    VelMin.CodaValue = v;
    //}

    //private void OnReceive_velMax(float v)
    //{
    //    VelMax.CodaValue = v;
    //}

    //private void OnReceive_accMin(float v)
    //{
    //    AccMin.CodaValue = v;
    //}

    //private void OnReceive_accMax(float v)
    //{
    //    AccMax.CodaValue = v;
    //}

    //private void OnReceive_disTotal(float v)
    //{
    //    DisTotal.CodaValue = v;
    //}

    //private void OnReceive_disMin(float v)
    //{
    //    DisMin.CodaValue = v;
    //}

    //private void OnReceive_disMax(float v)
    //{
    //    DisMax.CodaValue = v;
    //}

    //private void OnReceive_angleMin(float v)
    //{
    //    AngleMin.CodaValue = v;
    //}

    //private void OnReceive_angleMax(float v)
    //{
    //    AngleMax.CodaValue = v;
    //}

    //private void OnReceive_area(float v)
    //{
    //    Area.CodaValue = v;
    //}

    private void OnReceive_valuef1(float v)
    {
        valuef1.CodaValue = v;
    }

    private void OnReceive_valuef2(float v)
    {
        valuef2.CodaValue = v;
    }

    private void OnReceive_valuef3(float v)
    {
        valuef3.CodaValue = v;
    }

    private void OnReceive_valuef4(float v)
    {
        valuef4.CodaValue = v;
    }

    private void OnReceive_valuef5(float v)
    {
        valuef5.CodaValue = v;
    }

    private void OnReceive_valuef6(float v)
    {
        valuef6.CodaValue = v;
    }

    #endregion
}
