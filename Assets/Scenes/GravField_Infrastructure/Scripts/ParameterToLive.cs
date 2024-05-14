using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParameterToLive<T>
{
    private const float delayDuration = 10;
    private float lastTimeFromCoda = float.MinValue;

    private T originalValue;
    public T OrginalValue
    {
        get => originalValue;
        set
        {
            if (!originalValue.Equals(value))
                changed = true;
            else
                changed = false;
            originalValue = value;
        }
    }

    private T codaValue;
    public T CodaValue
    {
        get => codaValue;
        set
        {
            if (!codaValue.Equals(value))
                changed = true;
            else
                changed = false;

            codaValue = value;
            lastTimeFromCoda = Time.time;
        }
    }

    public T Value
    {
        get
        {
            if (Time.time - lastTimeFromCoda > delayDuration) return originalValue;
            else return codaValue;
        }
    }

    private bool changed = false;
    public bool Changed { get => changed; }

    //private T minValue;
    //public T MinValue { get => minValue; set => minValue = value; }

    //private T maxValue;
    //public T MaxValue { get => maxValue; set => maxValue = value; }

    //public bool NeedClamp { get; set; }
}
