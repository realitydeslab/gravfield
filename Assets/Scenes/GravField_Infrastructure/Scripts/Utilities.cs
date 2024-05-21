using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities
{
    public static float SmoothValue(float cur, float dst, float t = 0.2f)
    {
        if (t == 0)
            return dst;

        float cur_vel = 0;
        return Mathf.SmoothDamp(cur, dst, ref cur_vel, t);
    }
}
