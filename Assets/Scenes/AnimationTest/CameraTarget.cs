using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public Transform performerTransformRoot;


    
    void Start()
    {
        
    }

    
    void Update()
    {
        Vector3 sum = Vector3.zero;

        for(int i=0; i< performerTransformRoot.childCount; i++)
        {
            sum += performerTransformRoot.GetChild(i).localPosition;
        }

        transform.localPosition = sum / 3.0f;
    }
}
