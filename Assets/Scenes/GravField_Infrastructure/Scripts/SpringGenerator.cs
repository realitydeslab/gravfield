using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SplineMesh;
using UnityEngine;

public class SpringGenerator : RopeGenerator
{


    protected override void AddBasicComponent(GameObject go, int start_index, int end_index)
    {
        EffectSpring spring_path = go.AddComponent<EffectSpring>();
        spring_path.performerStart = performerTransformRoot.GetChild(start_index).GetComponent<Performer>();
        spring_path.performerEnd = performerTransformRoot.GetChild(end_index).GetComponent<Performer>();
        spring_path.ropeOffset = ropeCornerOffset;
    }

}
