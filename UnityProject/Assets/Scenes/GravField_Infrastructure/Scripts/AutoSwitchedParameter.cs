using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSwitchedParameter<T>
{
    public AutoSwitchedParameter()
    {

    }
    public AutoSwitchedParameter(T v)
    {
        originalValue = v;
    }

    private const float delayDuration = 10;
    private float lastTimeFromCoda = float.MinValue;

    private T originalValue;
    public T OrginalValue
    {
        get => originalValue;
        set
        {
            if (!originalValue.Equals(value))
                originalValueChanged = true;
            else
                originalValueChanged = false;
            originalValue = value;
        }
    }
    bool originalValueChanged = false;

    private T codaValue;
    public T CodaValue
    {
        get => codaValue;
        set
        {
            if (!codaValue.Equals(value))
            {
                codaValueChanged = true;
                lastTimeFromCoda = Time.time;
            }
            else
            {
                codaValueChanged = false;
            }  

            codaValue = value;
        }
    }
    bool codaValueChanged = false;

    public T Value
    {
        get
        {
            if (Time.time - lastTimeFromCoda > delayDuration) return originalValue;
            else return codaValue;
        }
    }

    public bool Changed
    {
        get
        {
            if (Time.time - lastTimeFromCoda > delayDuration) return originalValueChanged;
            else return codaValueChanged;
        }
    }

    //private T minValue;
    //public T MinValue { get => minValue; set => minValue = value; }

    //private T maxValue;
    //public T MaxValue { get => maxValue; set => maxValue = value; }

    //public bool NeedClamp { get; set; }
}
