using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using OscJack;

public interface IOscPropertyForSending
{
    
}

[System.Serializable]
public class OscPropertyForSending
{
    public OscPropertyForSending()
    {

    }
    public OscPropertyForSending(string _adress)
    {
        oscAddress = _adress;
    }
    public OscPropertyForSending(string _address, AutoSwitchedParameter<float> _param) : this(_address)
    {
        floatParameter = _param;
        dataType = typeof(float);
    }

    public OscPropertyForSending(string _address, AutoSwitchedParameter<Vector3> _param) : this(_address)
    {
        vector3Parameter = _param;
        dataType = typeof(Vector3);
    }
    public OscPropertyForSending(string _address, AutoSwitchedParameter<int> _param) : this(_address)
    {
        intParameter = _param;
        dataType = typeof(int);
    }
    public OscPropertyForSending(string _address, AutoSwitchedParameter<string> _param) : this(_address)
    {
        stringParameter = _param;
        dataType = typeof(string);
    }

    public string oscAddress = "/unity";
    public Type dataType = typeof(float);
    public bool keepSending = false;
    public bool enabled = true;

    public bool needRemap = true;
    public bool needClamp = false;
    public Vector2 srcRange = new Vector2(0, 1);
    public Vector2 dstRange = new Vector2(0, 1);
    public float mappedValue = 0;

    public AutoSwitchedParameter<float> floatParameter;
    public AutoSwitchedParameter<Vector3> vector3Parameter;
    public AutoSwitchedParameter<int> intParameter;
    public AutoSwitchedParameter<string> stringParameter;
}



[System.Serializable]
public class OscPropertyForReceiving
{
    public OscPropertyForReceiving()
    {

    }
    public OscPropertyForReceiving(string _adress)
    {
        oscAddress = _adress;
    }
    public OscPropertyForReceiving(string _address, UnityAction<float> _action):this(_address)
    {
        floatAction = _action;
        dataType = typeof(float);
    }

    public OscPropertyForReceiving(string _address, UnityAction<float> _action, float default_value, bool need_clamp, float min_value, float max_value) : this(_address)
    {
        floatAction = _action;
        dataType = typeof(float);

        defaultValue = default_value;
        needClamp = need_clamp;
        minValue = min_value;
        maxValue = max_value;
    }

    public OscPropertyForReceiving(string _address, UnityAction<Vector3> _action) : this(_address)
    {
        vector3Action = _action;
        dataType = typeof(Vector3);
    }
    public OscPropertyForReceiving(string _address, UnityAction<int> _action) : this(_address)
    {
        intAction = _action;
        dataType = typeof(int);
    }
    public OscPropertyForReceiving(string _address, UnityAction<string> _action) : this(_address)
    {
        stringAction = _action;
        dataType = typeof(string);
    }
    public string oscAddress = "/unity";
    public Type dataType;
       
    public UnityAction<float> floatAction;
    public UnityAction<Vector3> vector3Action;
    public UnityAction<int> intAction;
    public UnityAction<string> stringAction;

    public float defaultValue;
    public float minValue;
    public float maxValue;
    public bool needClamp = false;

}
