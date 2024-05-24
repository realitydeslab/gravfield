using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PerformerSynchronizer : NetworkBehaviour
{

    Transform performerTransform = null;

    private ARCameraManager arCameraManager;

    void Awake()
    {
        arCameraManager = FindFirstObjectByType<ARCameraManager>();
    }

    void Update()
    {
        if (IsSpawned == false || performerTransform == null || arCameraManager == null)
            return;

        performerTransform.SetPositionAndRotation(arCameraManager.transform.position, arCameraManager.transform.rotation);
    }

    public void BindPerformerTransform(Transform performer)
    {
        performerTransform = performer;
    }

    public void UnbindPerformTransform(Transform performer)
    {
        performerTransform = null;
    }


    /*
    List<(Transform, Transform)> bondedTransformList = new List<(Transform, Transform)>();

    private ARCameraManager arCameraManager;

    void Awake()
    {
        arCameraManager = FindFirstObjectByType<ARCameraManager>();
    }
    void Update()
    {
        if (bondedTransformList.Count == 0 || IsSpawned == false)
            return;

        foreach ((Transform, Transform) bond in bondedTransformList)
        {
            bond.Item1.SetPositionAndRotation(bond.Item2.position, bond.Item2.rotation);
        }
    }

    public void BindPerformerTransform(Transform performer, Transform source = null)
    {
        if (source == null)
        {
            source = arCameraManager.transform;
            bondedTransformList.Clear();
        }

        bondedTransformList.Add((performer, source));
    }

    public void UnbindPerformTransform(Transform performer)
    {
        foreach((Transform, Transform) bond in bondedTransformList)
        {
            if(bond.Item1 == performer)
            {
                bondedTransformList.Remove(bond);
                return;
            }
        }
    }
    */
}
