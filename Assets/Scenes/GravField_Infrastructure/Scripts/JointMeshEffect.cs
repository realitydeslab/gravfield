using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointMeshEffect : MonoBehaviour
{
    public Transform ropeTransformRoot;
    public GameObject meshBorder;

    List<Transform> borderMeshList = new List<Transform>();
    List<Transform> borderMeshDstList = new List<Transform>();
    void Start()
    {
        for(int i=0; i< ropeTransformRoot.childCount; i++)
        {
            Transform joint_root = ropeTransformRoot.GetChild(i).Find("Joints");
            for(int k=0; k<joint_root.childCount; k++)
            {
                Transform joint = joint_root.GetChild(k);
                GameObject new_mesh = Instantiate(meshBorder, joint);
                borderMeshList.Add(new_mesh.transform);
                borderMeshDstList.Add(joint);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i=0; i< borderMeshList.Count; i++)
        {
            //borderMeshList[i].position = borderMeshDstList[i].position;
            //borderMeshList[i].rotation = borderMeshDstList[i].rotation;

            //borderMeshList[i].forward = borderMeshDstList[i].forward;
            borderMeshList[i].localRotation = Quaternion.Euler(-borderMeshDstList[i].localRotation.x, 0,90);
        }
    }
}
