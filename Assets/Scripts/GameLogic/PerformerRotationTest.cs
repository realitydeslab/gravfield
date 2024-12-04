using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformerRotationTest : MonoBehaviour
{

    Vector3 target;
    Vector3 defalutAngle;
    GameObject targetGO;
    void OnEnable()
    {
        defalutAngle = transform.localEulerAngles;
        targetGO = new GameObject("target");
        targetGO.transform.parent = null;

        targetGO.transform.localPosition = transform.localPosition;
        targetGO.transform.localRotation = transform.localRotation;
        targetGO.transform.localScale = Vector3.one;
    }


    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            //Vector2 mouse_pos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            //mouse_pos = (mouse_pos - Vector2.one * 0.5f) * 6;

            float angle = Input.mousePosition.x / Screen.width * 360;

            target = targetGO.transform.TransformPoint(new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 2));
            transform.LookAt(target, Vector3.up);
        }
        else
        {
            transform.eulerAngles = defalutAngle;
        }
    }

    void OnDrawGizmos()
    {
        if (Input.GetMouseButton(0))
        {
            Gizmos.DrawLine(transform.position, target);
        }

    }
}
