using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using HoloKit;

public class CameraMovement : MonoBehaviour
{
    public HoloKitCameraManager holokitCameraManager;
    public CinemachineBrain cinemachineCameraManager;

    public CinemachineMixingCamera mixingCamera;
    public CinemachineDollyCart dollyCart;

    bool initialized = false;

    //List<float> srcWeight = new List<float>();
    List<float> dstWeight = new List<float>();

    RoleManager roleManager;

    private void Awake()
    {
        roleManager = FindObjectOfType<RoleManager>();
    }
    void OnEnable()
    {
        roleManager?.OnSpecifyPlayerRoleEvent.AddListener(OnSpecifyPlayerRole);
    }
    void OnDisable()
    {
        roleManager?.OnSpecifyPlayerRoleEvent.RemoveListener(OnSpecifyPlayerRole);
    }

    void Update()
    {
        if (!initialized || mixingCamera.ChildCameras.Length != dstWeight.Count)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ChangeFocusTo(0);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeFocusTo(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeFocusTo(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeFocusTo(3);
        }

        for (int i=0; i<dstWeight.Count; i++)
        {
            float src_weight = mixingCamera.GetWeight(i);
            float dst_weight = dstWeight[i];
            if (src_weight != dst_weight)
            {
                float current_speed = 0;
                mixingCamera.SetWeight(i, Mathf.SmoothDamp(src_weight, dst_weight, ref current_speed, 0.1f));
            }
        }
    }

    void OnSpecifyPlayerRole(RoleManager.PlayerRole role)
    {
        if (role == RoleManager.PlayerRole.Server && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            holokitCameraManager.enabled = false;
            cinemachineCameraManager.enabled = true;

            mixingCamera.enabled = true;
            dollyCart.enabled = true;

            dollyCart.m_Speed = 1;

            //srcWeight.Clear();
            dstWeight.Clear();

            mixingCamera.SetWeight(0, 1);     dstWeight.Add(1);
            mixingCamera.SetWeight(1, 0);     dstWeight.Add(0);
            mixingCamera.SetWeight(2, 0);     dstWeight.Add(0);
            mixingCamera.SetWeight(3, 0);     dstWeight.Add(0);

            initialized = true;
            Debug.Log("Change Camera to Cinemachine Mode");
        }
    }

    public void ChangeFocusTo(int index)
    {
        for(int i=0; i<dstWeight.Count; i++)
        {
            if(i == index)
            {
                //srcWeight[i] = mixingCamera.GetWeight(i);
                dstWeight[i] = 1;
            }
            else
            {
                //srcWeight[i] = mixingCamera.GetWeight(i);
                dstWeight[i] = 0;
            }
        }
        Debug.Log("Focus Camera to " + (index == 0 ? "Group" : ("Performer"+index.ToString())));
    }
}
