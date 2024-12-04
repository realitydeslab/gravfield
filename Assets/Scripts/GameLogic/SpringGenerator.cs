using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SplineMesh;
using UnityEngine;

public class SpringGenerator : RopeGenerator
{

    const int ropeCountEachSpring = 5;

    #region Generate Spring
    [ContextMenu("GenerateSprings")]
    protected override void GenerateRopes()
    {
        // Generate Springs
        for (int i = 0; i < performerTransformRoot.childCount; i++)
        {
            for (int k = i + 1; k < performerTransformRoot.childCount; k++)
            {
                // Generate Spring
                GameObject spring_group_root = new GameObject("Spring" + i.ToString() + k.ToString());
                spring_group_root.transform.parent = transform;
                spring_group_root.AddComponent<EffectSpringGroup>();

                for (int m = 0;m < ropeCountEachSpring; m++)
                {
                    GameObject spring_root = new GameObject("Spring" + m.ToString());
                    spring_root.transform.parent = spring_group_root.transform;
                    GenerateRope(spring_root, i, k);

                    Debug.Log("Spring" + i.ToString() + k.ToString() + ": Spring" + m.ToString());
                }
            }
        }
    }
    protected override void AddBasicComponent(GameObject go, int start_index, int end_index)
    {
        EffectSpring spring_path = go.AddComponent<EffectSpring>();
        spring_path.performerStart = performerTransformRoot.GetChild(start_index).GetComponent<Performer>();
        spring_path.performerEnd = performerTransformRoot.GetChild(end_index).GetComponent<Performer>();
        spring_path.ropeOffset = ropeCornerOffset;
    }
    protected override void AddExtraComponent(GameObject go, int start_index, int end_index)
    {
        Spline spline = go.AddComponent<Spline>();

        SplineSmoother smoother = go.AddComponent<SplineSmoother>();
        smoother.curvature = 0.4f;

        SplineMeshTiling meshTilling = go.AddComponent<SplineMeshTiling>();
        meshTilling.mesh = ropeMesh;
        meshTilling.material = ropeMat;
        meshTilling.rotation = new Vector3(0, 0, 0);
        meshTilling.scale = Vector3.one * 0.1f;

        meshTilling.generateCollider = false;
        meshTilling.updateInPlayMode = true;
        meshTilling.curveSpace = false;


        //LineRenderer line_renderer = go.AddComponent<LineRenderer>();
        //line_renderer.material = ropeMat;
        //line_renderer.positionCount = 100;
        //line_renderer.startWidth = 0.01f;
        //line_renderer.endWidth = 0.01f;
        //line_renderer.enabled = false;
    }

    protected override void DoExtraSettings(GameObject go, int start_index, int end_index)
    {
        // Set exclude layers to avoid collision
        Transform joint_root = go.transform.Find("Joints");
        for (int i=0; i<joint_root.childCount; i++)
        {
            Transform joint = joint_root.GetChild(i);
            joint.gameObject.layer = LayerMask.NameToLayer("Rope");

            Collider collider = joint.GetComponent<Collider>();
            collider.excludeLayers = LayerMask.GetMask("Rope"); // GetMask() is different to NameToLayer()
        }

        //Physics.IgnoreLayerCollision(layer, layer, true);


        // Random Mass / Drag and Spring / Damper
        float random_joint_mass = RandomValue(jointMass);
        float random_joint_drag = RandomValue(jointDrag);
        float random_joint_angular_drag = RandomValue(jointAngularDrag);
        float random_joint_sprint = RandomValue(jointSprint);
        float random_joint_damper = RandomValue(jointDamper);

        float random_segment_mass = RandomValue(segmentMass);
        float random_segment_drag = RandomValue(segmentDrag);
        float random_segment_angular_drag = RandomValue(segmentAngularDrag);

        for (int i = 0; i < joint_root.childCount; i++)
        {
            Transform joint = joint_root.GetChild(i);

            Rigidbody rigid_body = joint.GetComponent<Rigidbody>();
            rigid_body.mass = random_joint_mass;
            rigid_body.linearDamping = random_joint_drag;
            rigid_body.angularDamping = random_joint_angular_drag;
            rigid_body.useGravity = false;

            HingeJoint hinge = joint.GetComponent<HingeJoint>();
            hinge.useSpring = true;
            JointSpring spring_settings = new JointSpring();
            spring_settings.spring = random_joint_sprint;
            spring_settings.damper = random_joint_damper;
            hinge.spring = spring_settings;
        }


        Transform segment_root = go.transform.Find("Segments");
        for(int i=0; i<segment_root.childCount; i++)
        {
            Transform segment = segment_root.GetChild(i);
            Rigidbody rigid_body = segment.GetComponent<Rigidbody>();
            rigid_body.mass = random_segment_mass;
            rigid_body.linearDamping = random_segment_drag;
            rigid_body.angularDamping = random_segment_angular_drag;
            rigid_body.useGravity = false;
        }

        

    }
    float RandomValue(float v, float range = 0.5f)
    {
        return v * Random.Range(1 - range, 1 + range);
    }
    #endregion

    #region Control Rope
    [ContextMenu("HideAllPath")]
    protected override void HideAllPath()
    {
        SetPathVisible(0, false);
        SetPathVisible(1, false);
        SetPathVisible(2, false);
    }

    [ContextMenu("ShowAllPath")]
    protected override void ShowAllPath()
    {
        SetPathVisible(0, true);
        SetPathVisible(1, true);
        SetPathVisible(2, true);
    }

    protected override void SetPathVisible(int index, bool visiable)
    {
        if (index < 0 || index >= transform.childCount) return;

        Transform group_root = transform.GetChild(index);
        for(int i=0; i< group_root.childCount; i++)
        {
            Transform root_transform = group_root.GetChild(i);

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

            Transform mesh_transform = root_transform.Find("generated by SplineMeshTiling");
            mesh_transform?.gameObject.SetActive(visiable);
        }

       
    }
    #endregion
}
