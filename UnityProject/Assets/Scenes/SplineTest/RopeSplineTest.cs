using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class RopeSplineTest : MonoBehaviour
{
    Spline spline;
    SplineSmoother smoother;
    void Start()
    {
        spline = GetComponent<Spline>();

        for(int i=0; i<transform.childCount; i++)
        {
            spline.AddNode(new SplineNode(transform.GetChild(i).localPosition, Vector3.up));
        }

        smoother = GetComponent<SplineSmoother>();
        smoother.curvature = 0.4f;

        spline.RefreshCurves();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
