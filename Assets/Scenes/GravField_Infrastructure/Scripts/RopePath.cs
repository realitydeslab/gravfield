using System.Collections;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;
using SplineMesh;

public class RopePath : MonoBehaviour
{
    public Transform performerStart;
    public Transform performerEnd;

    public Transform ropeStart;
    public Transform ropeEnd;

    public Vector3 ropeOffset;

    public bool useSplineMesh = true;

    Spline spline;

    List<GameObject> wayPoints = new List<GameObject>();

    void Start()
    {
        spline = GetComponent<Spline>();

        AssignWayPoints();

        AssignSplineNodes();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 nor = (performerEnd.position - performerStart.position).normalized;

        

        ropeStart.localPosition = performerStart.TransformPoint(ropeOffset);// + nor* 0.3f;
        ropeEnd.localPosition = performerEnd.TransformPoint(ropeOffset);// -nor*0.3f;

        //ropeStart.localRotation = performerStart.localRotation;
        //ropeEnd.localRotation = performerEnd.localRotation;

        if (useSplineMesh)
            UpdateNodes();

    }

    void UpdateNodes()
    {
        int i = 0;
        foreach (GameObject wayPoint in wayPoints)
        {
            var node = spline.nodes[i++];
            //if (Vector3.Distance(node.Position, transform.InverseTransformPoint(wayPoint.transform.position)) > 0.001f)
            //{
            //    node.Position = transform.InverseTransformPoint(wayPoint.transform.position);
            //    //node.Up = wayPoint.transform.up;
            //}

                node.Position =wayPoint.transform.position;
                //node.Up = wayPoint.transform.up;
        }
    }

    void AssignWayPoints()
    {
        wayPoints.Clear();
        //Transform anchor_root = transform.Find("Anchors");
        //wayPoints.Add(anchor_root.GetChild(0).gameObject);

        Transform joint_root = transform.Find("Joints");
        for (int i = 0; i < joint_root.childCount; i++)
        {
            wayPoints.Add(joint_root.GetChild(i).gameObject);
        }

        //wayPoints.Add(anchor_root.GetChild(1).gameObject);
    }

    void AssignSplineNodes()
    {
        foreach (var penisNode in wayPoints.ToList())
        {
            if (penisNode == null) wayPoints.Remove(penisNode);
        }
        int nodeCount = wayPoints.Count;
        // adjust the number of nodes in the spline.
        while (spline.nodes.Count < nodeCount)
        {
            spline.AddNode(new SplineNode(Vector3.zero, Vector3.zero));
        }
        while (spline.nodes.Count > nodeCount && spline.nodes.Count > 2)
        {
            spline.RemoveNode(spline.nodes.Last());
        }
    }
}
