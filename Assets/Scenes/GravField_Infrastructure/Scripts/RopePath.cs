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

    Transform centroidTransform;
    private Vector3 centroidPos;
    public Vector3 CentroidPos { get => centroidPos; }
    private Vector3 centroidVel;
    public Vector3 CentroidVel { get => centroidVel; }

    Spline spline;

    List<GameObject> wayPoints = new List<GameObject>();

    void Start()
    {
        spline = GetComponent<Spline>();

        AssignWayPoints();

        AssignSplineNodes();
    }

    public void BindPerformer(Transform performer_start, Transform performer_end)
    {
        performerStart = performer_start;
        performerEnd = performer_end;
    }

    public void SetPerformerOffset(Vector3 offset)
    {
        ropeOffset = offset;
    }

    public void BindRopeAnchors(Transform performer_start, Transform performer_end)
    {
        ropeStart = performer_start;

    }


    void Update()
    {
        //Vector3 nor = (performerEnd.position - performerStart.position).normalized;
        //ropeStart.localPosition = performerStart.position + nor* 0.3f;
        //ropeEnd.localPosition = performerEnd.position -nor*0.3f;

        ropeStart.localPosition = performerStart.TransformPoint(ropeOffset);
        ropeEnd.localPosition = performerEnd.TransformPoint(ropeOffset);

        ropeStart.localRotation = performerStart.localRotation;
        ropeEnd.localRotation = performerEnd.localRotation;

        Vector3 last_pos = centroidPos;
        centroidPos = centroidTransform.localPosition;
        centroidVel = (centroidPos - last_pos) / Time.deltaTime;

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

        centroidTransform = joint_root.GetChild(Mathf.FloorToInt(joint_root.childCount / 2f));
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

    float CalculateRopeLength()
    {
        Vector3 last_pos = Vector3.zero;
        float dis_total = 0;

        foreach (GameObject wayPoint in wayPoints)
        {
            if (last_pos == Vector3.zero)
            {
                last_pos = wayPoint.transform.position;
            }
            else
            {
                dis_total += Vector3.Distance(wayPoint.transform.position, last_pos);
                last_pos = wayPoint.transform.position;
            }
        }
        //Debug.Log($"Rope{transform.GetSiblingIndex()} Length:{dis_total}");

        return dis_total;
    }
}
