using System.Collections;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;
using SplineMesh;

public class PerformerRope : MonoBehaviour
{
    public Transform performer1;
    public Transform performer2;

    public Transform ropeCorner1;
    public Transform ropeCorner2;

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
        if (ropeCorner1 == null) Debug.Log("1");
        if(performer1 == null) Debug.Log("2");
        if (ropeCorner2 == null) Debug.Log("11");
        if (performer2 == null) Debug.Log("22");
        ropeCorner1.transform.localPosition = performer1.localPosition + Vector3.up * 0.2f;
        ropeCorner2.transform.localPosition = performer2.localPosition + Vector3.up * 0.2f;

        if(useSplineMesh)
            UpdateNodes();
    }

    void UpdateNodes()
    {
        if (wayPoints == null) Debug.Log("3");
        if (spline == null) Debug.Log("4");
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
                node.Up = wayPoint.transform.up;
        }
    }

    void AssignWayPoints()
    {
        wayPoints.Clear();
        Transform anchor_root = transform.Find("Anchors");
        wayPoints.Add(anchor_root.GetChild(0).gameObject);

        Transform joint_root = transform.Find("Joints");
        for (int i = 0; i < joint_root.childCount; i++)
        {
            wayPoints.Add(joint_root.GetChild(i).gameObject);
        }

        wayPoints.Add(anchor_root.GetChild(1).gameObject);
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
