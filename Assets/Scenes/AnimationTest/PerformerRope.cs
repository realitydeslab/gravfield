using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformerRope : MonoBehaviour
{
    public Transform performerTransformRoot;

    public Transform ropeCorner1;
    public Transform ropeCorner2;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ropeCorner1.transform.localPosition = performerTransformRoot.GetChild(0).localPosition + Vector3.up * 1.8f;
        ropeCorner2.transform.localPosition = performerTransformRoot.GetChild(1).localPosition + Vector3.up * 1.8f;

    }
}
