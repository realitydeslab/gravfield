using System.Collections.Generic;
using UnityEngine;

public class Performer_Dispatcher : BaseDispatcher
{
    //*******************
    // BaseDispatcher has been made only for server
    //*******************


    // receiver


    // sender for Live
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

    // sender for Coda


    

    void Update()
    {
        if (isDispatching == false)
            return;

        CalculateDataToLive();
    }

    void CalculateDataToLive()
    {
        int performer_count = GameManager.Instance.PlayerManager.PerformerCount;
        List<Performer> performerList = GameManager.Instance.PlayerManager.PerformerList;

        // Pos / Speed / Acclerate
        if (performer_count > 0)
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
        if (performer_count > 1)
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
        if (performer_count == 3)
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

            for (int k = i + 1; k < performerList.Count; k++)
            {
                if (performerList[k].isPerforming.Value == false)
                    continue;

                int index = i + k - 1;
                if (index == 0) disab.OrginalValue = Vector3.Distance(performerList[i].transform.position, performerList[k].transform.position);
                else if (index == 1) disac.OrginalValue = Vector3.Distance(performerList[i].transform.position, performerList[k].transform.position);
                else disbc.OrginalValue = Vector3.Distance(performerList[i].transform.position, performerList[k].transform.position);
            }
        }
    }

    #region Receiver
    protected override void RegisterReceiver()
    {
        
    }

    #endregion


    #region Sender for live
    protected override void RegisterSender_ForLive()
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

        SenderForLive.Instance.RegisterOscPropertyToSend("/disTotal", DisTotal);
        SenderForLive.Instance.RegisterOscPropertyToSend("/disMin", DisMin);
        SenderForLive.Instance.RegisterOscPropertyToSend("/disMax", DisMax);

        //SenderForLive.Instance.RegisterOscPropertyToSend("/angleMin", AngleMin);
        //SenderForLive.Instance.RegisterOscPropertyToSend("/angleMax", AngleMax);
        SenderForLive.Instance.RegisterOscPropertyToSend("/area", Area);

        SenderForLive.Instance.RegisterOscPropertyToSend("/ay", ay);
        SenderForLive.Instance.RegisterOscPropertyToSend("/by", by);
        SenderForLive.Instance.RegisterOscPropertyToSend("/cy", cy);

        SenderForLive.Instance.RegisterOscPropertyToSend("/disab", disab);
        SenderForLive.Instance.RegisterOscPropertyToSend("/disac", disac);
        SenderForLive.Instance.RegisterOscPropertyToSend("/disbc", disbc);
    }
    #endregion


    #region Sender for Coda
    protected override void RegisterSender_ForCoda()
    {

    }
    #endregion
}
