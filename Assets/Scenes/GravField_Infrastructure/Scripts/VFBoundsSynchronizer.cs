using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VFBoundsSynchronizer : MonoBehaviour
{
    public Transform dstPerformer;
    [SerializeField]
    bool lockPositionY = true;
    [SerializeField]
    bool lockRotationXZ = true;

    public float lockedY = 1.6f;

    Transform headTransform;
    Transform boundsTransform;

    private const string VFXPositionPostfix = "_position";
    private const string VFXRotationPostfix = "_angles";
    private const string VFXScalePostfix = "_scale";

   
    void Update()
    {

        if (headTransform == null) headTransform = transform;
        if (boundsTransform == null) boundsTransform = transform.GetChild(0);

        if (lockRotationXZ)
            headTransform.rotation = Quaternion.Euler(0, dstPerformer.eulerAngles.y, 0);
        if (lockPositionY)
            headTransform.position = new Vector3(dstPerformer.position.x, lockedY, dstPerformer.position.z);


        //headTransform.position = dstPerformer.position;
        //headTransform.eulerAngles = dstPerformer.eulerAngles;
        ////headTransform.localScale = dstPerformer.localScale;


        //if (lockRotationXZ)
        //    boundsTransform.rotation = Quaternion.Euler(0, dstPerformer.eulerAngles.y, 0);
        //if(lockPositionY)
        //    boundsTransform.position = new Vector3(dstPerformer.position.x, lockedY, dstPerformer.position.z);


        //vfx.SetVector3("Performer" + VFXPositionPostfix, performerTransform.position);
        //vfx.SetVector3("Performer" + VFXRotationPostfix, performerTransform.eulerAngles);
        //vfx.SetVector3("Performer" + VFXScalePostfix, performerTransform.localScale);

        //vfx.SetMatrix4x4("PerformerInverseMatrix", performerTransform.worldToLocalMatrix);


        //vectorFieldTransform.rotation = Quaternion.Euler(0, performerTransform.eulerAngles.y, 0);



        //vfx.SetVector3("VFTransform" + VFXPositionPostfix, vectorFieldTransform.position);
        //vfx.SetVector3("VFTransform" + VFXRotationPostfix, vectorFieldTransform.eulerAngles);
        //vfx.SetVector3("VFTransform" + VFXScalePostfix, vectorFieldTransform.localScale);

        //vfx.SetMatrix4x4("VFInverseMatrix", vectorFieldTransform.worldToLocalMatrix);


    }
}
