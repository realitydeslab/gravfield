using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PerformerSynchronizer : NetworkBehaviour
{
    private Transform transPerformer;

    private ARCameraManager m_ARCameraManager;

    private void Update()
    {
        if (transPerformer == null)
            return;

        if (IsSpawned)
        {
            if (m_ARCameraManager == null)
            {
                m_ARCameraManager = FindFirstObjectByType<ARCameraManager>();
            }

            if (m_ARCameraManager != null)
            {
                transPerformer.SetPositionAndRotation(m_ARCameraManager.transform.position, m_ARCameraManager.transform.rotation);
            }
        }
    }

    public void BindPerformerTransform(Transform trans)
    {
        transPerformer = trans;
    }

    public void UnbindPerformTransform()
    {
        transPerformer = null;
    }

}
