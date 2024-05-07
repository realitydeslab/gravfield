using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformerRandomAnimation : MonoBehaviour
{
    [SerializeField]
    bool needSynchronize = false;
    [SerializeField]
    Transform performerTransformRoot;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (needSynchronize == false || performerTransformRoot == null || performerTransformRoot.childCount != transform.childCount)
            return;

        for(int i=0; i< performerTransformRoot.childCount; i++)
        {
            performerTransformRoot.GetChild(i).localPosition = transform.GetChild(i).localPosition;
            performerTransformRoot.GetChild(i).localRotation = transform.GetChild(i).localRotation;
        }
    }
}
