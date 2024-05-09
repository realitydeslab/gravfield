using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OscProperty
{
    public string oscAddress = "/unity";
    [HideInInspector]
    public float value;
    public float originalValue;
    [HideInInspector]
    public float defaultValue;
    public Vector2 srcRange = new Vector2(0, 1);
    public Vector2 dstRange = new Vector2(0, 1);
    public bool needClamp = false;
    public bool keepSending = false;
}
