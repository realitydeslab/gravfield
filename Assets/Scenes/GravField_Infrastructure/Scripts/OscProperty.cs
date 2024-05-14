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
    public OscPropertyForSending(string _address, ParameterToLive<float> _param) : this(_address)
    {
        floatParameter = _param;
        dataType = typeof(float);
    }

    public OscPropertyForSending(string _address, ParameterToLive<Vector3> _param) : this(_address)
    {
        vector3Parameter = _param;
        dataType = typeof(Vector3);
    }
    public OscPropertyForSending(string _address, ParameterToLive<int> _param) : this(_address)
    {
        intParameter = _param;
        dataType = typeof(int);
    }
    public OscPropertyForSending(string _address, ParameterToLive<string> _param) : this(_address)
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

    public ParameterToLive<float> floatParameter;
    public ParameterToLive<Vector3> vector3Parameter;
    public ParameterToLive<int> intParameter;
    public ParameterToLive<string> stringParameter;
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
}
