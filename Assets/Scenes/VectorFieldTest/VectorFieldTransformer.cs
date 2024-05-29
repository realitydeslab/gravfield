using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VectorFieldTransformer : MonoBehaviour
{
    public VisualEffect vfx;
    public Transform performerTransform;
    public Transform vectorFieldTransform;

    private const string VFXPositionPostfix = "_position";
    private const string VFXRotationPostfix = "_angles";
    private const string VFXScalePostfix = "_scale";
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        vfx.SetVector3("Performer" + VFXPositionPostfix, performerTransform.position);
        vfx.SetVector3("Performer" + VFXRotationPostfix, performerTransform.eulerAngles);
        vfx.SetVector3("Performer" + VFXScalePostfix, performerTransform.localScale);

        vfx.SetMatrix4x4("PerformerInverseMatrix", performerTransform.worldToLocalMatrix);


        vectorFieldTransform.rotation = Quaternion.Euler(0, performerTransform.eulerAngles.y, 0);



        vfx.SetVector3("VFTransform" + VFXPositionPostfix, vectorFieldTransform.position);
        vfx.SetVector3("VFTransform" + VFXRotationPostfix, vectorFieldTransform.eulerAngles);
        vfx.SetVector3("VFTransform" + VFXScalePostfix, vectorFieldTransform.localScale);

        vfx.SetMatrix4x4("VFInverseMatrix", vectorFieldTransform.worldToLocalMatrix);

        
    }
}
