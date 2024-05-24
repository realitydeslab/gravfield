using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SplineMesh;
using UnityEngine;

public class RopeGenerator : MonoBehaviour
{
    public Transform performerTransformRoot;

    public enum RopeElement
    {
        Anchor = 0,
        Joint,
        Segment
    }

    [Header("Rope")]    
    public float generatorRopeLength = 1.1f;
    public float generatorRopeThickness = 0.01f;
    public float generatorSegmentLength = 0.03f;
    [Tooltip("It's better to be even so the amount of joints would be odd, which would make rope more balanced.")]
    public int generatorSegmentCount = 6;

    [Header("Joint Settings")]
    // Joint. 
    [Tooltip("As the pivot of rope machanism, joint should not have a big mass which would make rope hard to swing.")]
    public float jointMass = 1;
    [Tooltip("Joint Drag would make rope hard to swing")]
    public float jointDrag = 0;
    [Tooltip("If Angular Drag is 0, rope will keep jiggering even after people stop swinging.")]
    public float jointAngularDrag = 1;
    public float jointSprint = 80;
    public float jointDamper = 50;

    [Header("Segment Settings")]
    [Tooltip("Where magic happens. Larger number would make rope swing further and more like a real rope")]
    // As segments doesn't have any joint components, they are not responsible for starting the swing movement.
    // But, after they are thrown away in the air, their mass would determine how much effort it will take for joints to drag them back.
    // Segment Mass is more related to the rope behavior after rope starts swinging while JointDrag and SegmentDrag are more related to the behavior before rope starts swinging
    public float segmentMass = 42.8f;
    public float segmentDrag = 0;
    public float segmentAngularDrag = 1;

    [Header("Spline Mesh")]
    public Mesh ropeMesh;
    public Material ropeMat;
    public Vector3 ropeMeshRotation = Vector3.zero;
    public float ropeMeshScale = 0.1f;

    [Tooltip("Caution! The scale values of Performer GameObjects matters a lot. Make sure they are set to (1, 1, 1)")]
    public Vector3 ropeCornerOffset = new Vector3(0, -0.1f, 0.4f);


    #region Generate Rope
    [ContextMenu("GenerateRopes")]
    protected virtual void GenerateRopes()
    {
        //// Remove all ropes
        var childList = transform.Cast<Transform>().ToList();
        foreach (Transform childTransform in childList)
        {
            DestroyImmediate(childTransform.gameObject);
        }

        // Generate Ropes
        for (int i = 0; i < performerTransformRoot.childCount; i++)
        {
            for(int k=i+1; k< performerTransformRoot.childCount; k++)
            {
                // Generate Root
                GameObject rope_root = new GameObject("Rope" + i.ToString() + k.ToString());
                rope_root.transform.parent = transform;

                GenerateRope(rope_root, i,k);
            }
            
        }
    }

    protected GameObject GenerateRope(GameObject rope_root, int start_index, int end_index)
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Anchor <- Joint -> Segment <- Joint -> Segment <- Joint -> Segment <- Joint -> Anchor
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        
        Vector3 start_pos = Vector3.zero;
        Vector3 end_pos = new Vector3(generatorRopeLength, 0, 0);
        //Vector3 start_pos = rope_path.performerStart.position;
        //Vector3 end_pos = rope_path.performerEnd.position;


        // Generate Segments
        GameObject segment_root = new GameObject("Segments");
        segment_root.transform.parent = rope_root.transform;
        for (int m = 0; m < generatorSegmentCount; m++)
        {
            GenerateSegment(segment_root, m, Vector3.Lerp(start_pos, end_pos, (float)(m + 0.5f) / (float)generatorSegmentCount));
        }

        // Generate Anchors
        GameObject anchor_root = new GameObject("Anchors");
        anchor_root.transform.parent = rope_root.transform;
        GameObject anchor0 = GenerateAnchor(anchor_root, 0, start_pos, segment_root.transform.GetChild(0));
        GameObject anchor1 = GenerateAnchor(anchor_root, 1, end_pos, segment_root.transform.GetChild(segment_root.transform.childCount - 1));
        //rope_path.ropeStart = anchor0.transform;
        //rope_path.ropeEnd = anchor1.transform;


        // Generate Joints
        GameObject joint_root = new GameObject("Joints");
        joint_root.transform.parent = rope_root.transform;
        GameObject left_segment;
        GameObject right_segment;
        for (int m = 0; m < generatorSegmentCount + 1; m++)
        {
            left_segment = m == 0 ? anchor0 : segment_root.transform.GetChild(m - 1).gameObject;
            right_segment = m == generatorSegmentCount ? anchor1 : segment_root.transform.GetChild(m).gameObject;

            GenerateJoint(joint_root, m, Vector3.Lerp(start_pos, end_pos, (float)(m) / (float)generatorSegmentCount), left_segment, right_segment);
        }



        // Add Components
        AddBasicComponent(rope_root, start_index, end_index);

        AddExtraComponent(rope_root, start_index, end_index);


        // ExtraSettings
        DoExtraSettings(rope_root, start_index, end_index);

        return rope_root;

    }


    GameObject GenerateAnchor(GameObject anchor_root, int index, Vector3 pos, Transform connected_segment)
    {
        GameObject anchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        anchor.name = "Anchor" + index.ToString();
        anchor.transform.parent = anchor_root.transform;
        anchor.transform.position = pos;
        anchor.transform.localScale = Vector3.one * generatorRopeThickness;
        Rigidbody rigid_body = anchor.AddComponent<Rigidbody>();
        rigid_body.useGravity = false;

        ////////////////////////////////////////////////////////////
        // Caution!!!
        // RigidBody of anchros have to be Kinematic to avoid repelling connected joint while correctly influencing it        // 
        ////////////////////////////////////////////////////////////
        rigid_body.isKinematic = true;

        rigid_body.constraints = RigidbodyConstraints.FreezePosition;
        rigid_body.mass = 100;

        anchor.GetComponent<Collider>().enabled = false;
        anchor.GetComponent<MeshRenderer>().enabled = false;

        return anchor;
    }

    GameObject GenerateJoint(GameObject joint_root, int index, Vector3 pos, GameObject left, GameObject right)
    {
        GameObject joint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        joint.name = "Joint" + index.ToString();
        joint.transform.parent = joint_root.transform;
        joint.transform.position = pos;
        joint.transform.localScale = Vector3.one * generatorRopeThickness;
        Rigidbody rigid_body = joint.AddComponent<Rigidbody>();
        rigid_body.mass = jointMass;
        rigid_body.linearDamping = jointDrag;
        rigid_body.angularDamping = jointAngularDrag;

        HingeJoint hinge_left = joint.AddComponent<HingeJoint>();
        HingeJoint hinge_right = joint.AddComponent<HingeJoint>();
        SetHingeProperties(hinge_left, left.GetComponent<Rigidbody>());
        SetHingeProperties(hinge_right, right.GetComponent<Rigidbody>());

        ////////////////////////////////////////////////////////////
        // Caution!!!
        // Collider of Joint can not be disabled otherwist it will lose elastcity
        ////////////////////////////////////////////////////////////
        joint.GetComponent<Collider>().enabled = true;

        joint.GetComponent<MeshRenderer>().enabled = false;

        return joint;
    }

    void SetHingeProperties(HingeJoint hinge, Rigidbody rigid)
    {
        hinge.anchor = Vector3.zero;
        hinge.connectedBody = rigid;

        ////////////////////////////////////////////////////////////
        // Caution!!!
        // 
        // autoConfigureConnectedAnchor is a great pain here.
        // 1. Joints connected to anchors do not need auto configuration cause it should stick with the anchor
        // 2. Joints connected to segments need to assign resonable anchors' position to behavior as we want which is why
        //    we choose auto configuration. However, if  Joints' position got modified before HingeJoint's Start()
        //    execute, it will use the new position to configure anchors.
        //    So, The entire rope MUST remain still for several frames when game starts for Joints to initialize its HingeJoints 
        ////////////////////////////////////////////////////////////
        // When connected to anchor, there is no need to set calculate anchor position. just connect to (0,0,0)
        if (rigid.gameObject.name.Contains("Anchor"))
        {
            hinge.autoConfigureConnectedAnchor = false;
            hinge.connectedAnchor = Vector3.zero;
        }
        // When connected to another joint, the result highly depends on the initial position. so better generate rope on the groud
        else
        {
            //hinge.autoConfigureConnectedAnchor = true;
            hinge.autoConfigureConnectedAnchor = false;
            hinge.connectedAnchor = CalculateConnectedAnchor(hinge.transform, rigid.transform);
        }

        hinge.useSpring = true;
        JointSpring spring_settings = new JointSpring();
        spring_settings.spring = jointSprint;
        spring_settings.damper = jointDamper;
        hinge.spring = spring_settings;

        JointLimits limits_settings = new JointLimits();
        limits_settings.bounceMinVelocity = 0;
        hinge.useLimits = true;
        hinge.limits = limits_settings;
        hinge.useLimits = true;
        hinge.limits = limits_settings;
    }


    GameObject GenerateSegment(GameObject segment_root, int index, Vector3 pos)
    {
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        segment.name = "Segment" + index.ToString();
        segment.transform.parent = segment_root.transform;
        segment.transform.position = pos;
        segment.transform.localScale = new Vector3(generatorSegmentLength * 0.5f, generatorSegmentLength, generatorSegmentLength * 0.5f); // Capsule : The Y-axis length is twice as long as the x-axis and z-axis by default
        segment.transform.localEulerAngles = new Vector3(0, 0, 90);
        Rigidbody rigid_body = segment.AddComponent<Rigidbody>();
        rigid_body.mass = segmentMass;
        rigid_body.linearDamping = segmentDrag;
        rigid_body.angularDamping = segmentAngularDrag;

        segment.GetComponent<Collider>().enabled = false;
        segment.GetComponent<MeshRenderer>().enabled = false;

        return segment;
    }

    protected virtual void AddBasicComponent(GameObject go, int start_index, int end_index)
    {
        EffectRope rope_path = go.AddComponent<EffectRope>();
        rope_path.performerStart = performerTransformRoot.GetChild(start_index).GetComponent<Performer>();
        rope_path.performerEnd = performerTransformRoot.GetChild(end_index).GetComponent<Performer>();
        rope_path.ropeOffset = ropeCornerOffset;
    }

    protected virtual void AddExtraComponent(GameObject go, int start_index, int end_index)
    {
        AddSplineMeshComponent(go);
    }

    void AddSplineMeshComponent(GameObject go)
    {
        Spline spline = go.AddComponent<Spline>();

        SplineSmoother smoother = go.AddComponent<SplineSmoother>();
        smoother.curvature = 0.4f;

        SplineMeshTiling meshTilling = go.AddComponent<SplineMeshTiling>();
        meshTilling.mesh = ropeMesh;
        meshTilling.material = ropeMat;
        meshTilling.rotation = ropeMeshRotation;
        meshTilling.scale = Vector3.one * ropeMeshScale;

        meshTilling.generateCollider = false;
        meshTilling.updateInPlayMode = true;
        meshTilling.curveSpace = false;

    }

    protected virtual void DoExtraSettings(GameObject go, int start_index, int end_index)
    {
        
    }

    #endregion


    #region Control Rope
    [ContextMenu("HideAllPath")]
    protected virtual void HideAllPath()
    {
        SetPathVisible(0, false);
        SetPathVisible(1, false);
        SetPathVisible(2, false);
    }

    [ContextMenu("ShowAllPath")]
    protected virtual void ShowAllPath()
    {
        SetPathVisible(0, true);
        SetPathVisible(1, true);
        SetPathVisible(2, true);
    }

    protected virtual void SetPathVisible(int index, bool visiable)
    {
        if (index < 0 || index >= transform.childCount) return;

        Transform root_transform = transform.GetChild(index);

        Transform anchor_root = root_transform.Find("Anchors");
        MeshRenderer[] renderers = anchor_root.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = visiable;
        }

        Transform joint_root = root_transform.Find("Joints");
        renderers = joint_root.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = visiable;
        }

        Transform segment_root = root_transform.Find("Segments");
        renderers = segment_root.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = visiable;
        }
    }
    #endregion


    // Possible equation. Need to be testified.
    Vector3 CalculateConnectedAnchor(Transform joint, Transform connected_object)
    {
        return connected_object.InverseTransformPoint(joint.position);
    }

    #region SplineExtrusion Attempts
    ////////////////////////////////////////////////////////////
    // Attempt to use SplineExtrusion instead of SplineMeshTiling.
    // But SplineExtrusion has worse effect even it's more efficient.
    ////////////////////////////////////////////////////////////
    //[ContextMenu("GenerateExtrusionShape")]
    //void GenerateExtrusionShape()
    //{
    //    for (int k = 0; k < transform.childCount; k++)
    //    {
    //        SplineExtrusion splineExtrusion = transform.GetChild(k).GetComponent<SplineExtrusion>();
    //        if (splineExtrusion != null)
    //        {
    //            int desired_count = 20;
    //            float desired_radius = 0.01f;
    //            List<ExtrusionSegment.Vertex> vertex_list = new List<ExtrusionSegment.Vertex>();
    //            for (int i = 0; i < desired_count; i++)
    //            {
    //                float angle = (float)i / (float)desired_count * 360f;
    //                Vector2 pos = new Vector2(desired_radius * Mathf.Sin(angle), desired_radius * Mathf.Cos(angle));
    //                Vector2 nor = pos;

    //                vertex_list.Add(new ExtrusionSegment.Vertex(pos, nor, 0));
    //            }
    //            splineExtrusion.shapeVertices.Clear();
    //            splineExtrusion.shapeVertices.AddRange(vertex_list);
    //        }
    //    }
    //}
    //// Extrusion keeps add collider components which will make rope act wierd. Have to diable them manully every frame
    //void DisableExtrusionCollider()
    //{
    //    for (int k = 0; k < transform.childCount; k++)
    //    {
    //        SplineExtrusion splineExtrusion = transform.GetChild(k).GetComponent<SplineExtrusion>();
    //        if (splineExtrusion != null)
    //        {
    //            Transform extrusion_root = transform.GetChild(k).Find("generated by SplineExtrusion");
    //            MeshCollider[] mesh_colliders = extrusion_root.GetComponentsInChildren<MeshCollider>();
    //            foreach (MeshCollider collider in mesh_colliders)
    //            {
    //                collider.enabled = false;
    //            }
    //        }
    //    }
    //}
    #endregion

    //int CalculateDesiredSegmentCount(Vector3 start, Vector3 end)
    //{
    //    float distance = Vector3.Distance(start, end);
    //    float src_min = 1;
    //    float src_max = 5;
    //    float dst_min = 3;
    //    float dst_max = 10;

    //    distance = Mathf.Clamp(distance, src_min, src_max);

    //    return Mathf.FloorToInt((distance - src_min) / (src_max - src_min) * (dst_max - dst_min) + dst_min);
    //}

    //Vector3 CalculateGameObjectPosition(string type, int index)
    //{

    //    if (type == "anchor")
    //    {
    //        return index == 0 ? Vector3.zero : new Vector3(generatorSegmentCount * generatorSegmentLength + (generatorSegmentCount + 1) * generatorRopeThickness + generatorInterval * generatorSegmentCount * 2 + generatorRopeThickness, 0, 0);
    //    }
    //    else if (type == "segment")
    //    {
    //        return new Vector3(generatorRopeThickness * 0.5f + (index + 1) * generatorRopeThickness + index * generatorSegmentLength + generatorInterval * (index * 2 + 1) + 0.5f * generatorSegmentLength, 0, 0);
    //    }
    //    else if (type == "joint")
    //    {
    //        return new Vector3(generatorRopeThickness * 0.5f + index * generatorRopeThickness + index * generatorSegmentLength + generatorInterval * (index * 2) + 0.5f * generatorRopeThickness, 0, 0);
    //    }

    //    return Vector3.zero;
    //}
}
