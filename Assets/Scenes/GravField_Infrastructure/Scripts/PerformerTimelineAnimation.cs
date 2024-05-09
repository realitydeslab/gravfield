using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformerTimelineAnimation : MonoBehaviour
{
    
    [SerializeField]
    bool needSynchronize = false;
    [SerializeField]
    Transform performerTransformRoot;
    [SerializeField]
    Transform[] dummyHeadTransform;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (needSynchronize == false || performerTransformRoot == null || performerTransformRoot.childCount != dummyHeadTransform.Length)
            return;

        for (int i = 0; i < performerTransformRoot.childCount; i++)
        {
            performerTransformRoot.GetChild(i).localPosition = dummyHeadTransform[i].position;
            performerTransformRoot.GetChild(i).localRotation = dummyHeadTransform[i].rotation;
        }
    }
}
