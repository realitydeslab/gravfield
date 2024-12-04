using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
#if UNITY_IOS
using HoloKit;
#endif

public class CameraMovement : MonoBehaviour
{
#if UNITY_IOS
    public HoloKitCameraManager holokitCameraManager;
#endif
    public CinemachineBrain cinemachineCameraManager;

    public CinemachineMixingCamera mixingCamera;
    public CinemachineDollyCart dollyCart;

    bool initialized = false;

    //List<float> srcWeight = new List<float>();
    List<float> dstWeight = new List<float>();


    void OnEnable()
    {
        GameManager.Instance.OnStartGame.AddListener(OnStartGame);
        GameManager.Instance.OnStopGame.AddListener(OnStopGame);
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStartGame.RemoveListener(OnStartGame);
            GameManager.Instance.OnStopGame.RemoveListener(OnStopGame);
        }
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

        
        // KeyCode.Alpha0 to use aerial view
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ChangeFocusTo(0);
        }
        // KeyCode.Alpha4 - 9 to change other views
        for (int i=0; i<6; i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha4 + i))
            {
                ChangeFocusTo(i+1);
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

    void StartCinemachineMode()
    {
        #if UNITY_IOS
        holokitCameraManager.enabled = false;
#endif
        cinemachineCameraManager.enabled = true;

        mixingCamera.enabled = true;
        dollyCart.enabled = true;

        dollyCart.m_Speed = 1;

        initialized = true;
    }

    void StopCinemachineMode()
    {
#if UNITY_IOS
        holokitCameraManager.enabled = true;
#endif
        cinemachineCameraManager.enabled = false;

        mixingCamera.enabled = false;
        dollyCart.enabled = false;

        initialized = false;
    }

    void OnStartGame(PlayerRole role)
    {
        if (IsValidEnvironment() && role == PlayerRole.Server)
        {
            StartCinemachineMode();
        }
    }

    void OnStopGame(PlayerRole role)
    {
        if (IsValidEnvironment() && role == PlayerRole.Server)
        {
            StopCinemachineMode();
        }
    }

    bool IsValidEnvironment()
    {
        return (GameManager.Instance.IsSoloMode == true && Application.platform != RuntimePlatform.IPhonePlayer);
    }
}
