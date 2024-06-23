using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LineTest : MonoBehaviour
{
    public Transform performerStart;
    public Transform performerEnd;

    public MeshRenderer springMesh;

    public Camera cam;

    public Vector3 ropeOffset = new Vector3(0, -0.3f, 0);
    void Start()
    {
        
    }


    void Update()
    {
        Vector3 start_pos = performerStart.transform.TransformPoint(ropeOffset);// + RandomOffset(performerStart.transform.position));
        Vector3 end_pos = performerEnd.transform.TransformPoint(ropeOffset);// + RandomOffset(performerEnd.transform.position));


        springMesh.transform.position = Vector3.Lerp(start_pos, end_pos, 0.5f);
        //float width = 2;
        float length = Vector3.Distance(start_pos, end_pos);
        //float new_width = Utilities.Remap(length, 1, maxDistance, maxSpringThickness, minSpringThickness, true);
        springMesh.transform.localScale = new Vector3(length / 5.0f, 1, 2);

        Vector3 dis = end_pos - start_pos;
        springMesh.transform.eulerAngles = new Vector3(0, Mathf.Rad2Deg * Mathf.Atan2(dis.x, dis.z) + 90, -Mathf.Rad2Deg * Mathf.Asin(dis.y/dis.magnitude));


        Vector3 center_pos = Vector3.Lerp(start_pos, end_pos, 0.5f);
        center_pos = cam.transform.InverseTransformPoint(center_pos);

        float angle = Vector3.Angle(cam.transform.forward, cam.transform.TransformPoint(new Vector3(0, center_pos.y, center_pos.z)) - cam.transform.position);
        Debug.Log(angle);

        //float rotation_y = Mathf.Atan2(dis.x, dis.z);
        //Vector3 center_pos = Vector3.Lerp(start_pos, end_pos, 0.5f);
        //Vector3 eye_up = GameManager.Instance.HolokitCameraManager.CenterEyePose.up;
        ////Debug.Log(eye_up);
        ////springMesh.transform.rotation = Quaternion.LookRotation(-GameManager.Instance.HolokitCameraManager.CenterEyePose.forward, );
        ////springMesh.transform.LookAt(GameManager.Instance.HolokitCameraManager.CenterEyePose);
        ////springMesh.transform.Rotate(Vector3.right, 90, Space.Self);
        ////springMesh.transform.Rotate(Vector3.up, Mathf.Atan2(dis.x, dis.z), Space.World);
    }
}
