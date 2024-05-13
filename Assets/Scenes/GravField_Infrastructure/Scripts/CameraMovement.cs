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

    void Start()
    {
        dstWeight.Clear();
        for (int i = 0; i < mixingCamera.ChildCameras.Length; i++)
        {
            if (i == 0)
                mixingCamera.SetWeight(i, 1);
            else
                mixingCamera.SetWeight(i, 0);

            dstWeight.Add(mixingCamera.GetWeight(i));
        }
    }

    void Update()
    {
        if (!initialized || mixingCamera.ChildCameras.Length != dstWeight.Count)
            return;

        for(int i=0; i<7; i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                ChangeFocusTo(i);
            }
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

    void ChangeFocusTo(int index)
    {
        for (int i = 0; i < dstWeight.Count; i++)
        {
            if (i == index)
            {
                dstWeight[i] = 1;
            }
            else
            {
                dstWeight[i] = 0;
            }
        }
        Debug.Log("Focus Camera to " + (index == 0 ? "Group" : (index < 4 ? "Performer" + (index-1).ToString() : "Performer-FPV" + (index-4).ToString())));
    }

    public void StartCinemachineMode()
    {
        holokitCameraManager.enabled = false;
        cinemachineCameraManager.enabled = true;

        mixingCamera.enabled = true;
        dollyCart.enabled = true;

        dollyCart.m_Speed = 1;

        initialized = true;
    }

    public void StopCinemachineMode()
    {
        holokitCameraManager.enabled = true;
        cinemachineCameraManager.enabled = false;

        mixingCamera.enabled = false;
        dollyCart.enabled = false;

        initialized = false;
    }

    void OnSpecifyPlayerRole(RoleManager.PlayerRole role)
    {
        if (IsValidEnvironment() && role == RoleManager.PlayerRole.Server)
        {
            StartCinemachineMode();
            Debug.Log("Change Camera to Cinemachine Mode");
        }
        else
        {
            StopCinemachineMode();
        }
    }

    bool IsValidEnvironment()
    {
        return Application.platform != RuntimePlatform.IPhonePlayer;
    }
}
