using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointMeshEffect : MonoBehaviour
{
    public Transform ropeTransformRoot;
    public GameObject meshBorder;

    PlayerManager roleManager;
    public Transform performerTransformRoot;
    List<Performer> performerList = new List<Performer>();

    List<Transform> borderMeshList = new List<Transform>();
    List<Transform> borderMeshDstList = new List<Transform>();

    void Awake()
    {
        roleManager = FindObjectOfType<PlayerManager>();


        for (int i = 0; i < performerTransformRoot.childCount; i++)
        {
            performerList.Add(performerTransformRoot.GetChild(i).GetComponent<Performer>());
   
        }
    }
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

                new_mesh.GetComponent<MeshRenderer>().enabled = false;
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

    void OnEnable()
    {
        roleManager.OnStartPerformingEvent.AddListener(OnStartPerforming);
        roleManager.OnStopPerformingEvent.AddListener(OnStopPerforming);
    }
    void OnDisable()
    {
        roleManager.OnStartPerformingEvent.RemoveListener(OnStartPerforming);
        roleManager.OnStopPerformingEvent.RemoveListener(OnStopPerforming);
    }

    void OnStartPerforming(int index, ulong client_index)
    {
        UpdateAllRopeState();
    }

    void OnStopPerforming(int index, ulong client_index)
    {
        UpdateAllRopeState();
    }

    void UpdateAllRopeState()
    {
        SetRopeState(0, performerList[0].isPerforming.Value == true && performerList[1].isPerforming.Value == true);
        SetRopeState(1, performerList[0].isPerforming.Value == true && performerList[2].isPerforming.Value == true);
        SetRopeState(2, performerList[1].isPerforming.Value == true && performerList[2].isPerforming.Value == true);
    }

    void SetRopeState(int index, bool state)
    {
        //SetPathVisible(index, state);
        SetSplineMeshVisible(index, state);
    }

    void SetSplineMeshVisible(int index, bool visible)
    {
        Transform joint_root = ropeTransformRoot.GetChild(index).Find("Joints");

        for (int k = 0; k < joint_root.childCount; k++)
        {
            Transform joint = joint_root.GetChild(k);
            Transform mesh = joint.GetChild(0);
            mesh.GetComponent<MeshRenderer>().enabled = visible;
        }

    }
}
