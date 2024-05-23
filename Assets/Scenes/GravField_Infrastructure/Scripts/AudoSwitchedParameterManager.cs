using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class LocalRemoteParameterPair
{
    public LocalRemoteParameterPair() { }
    public LocalRemoteParameterPair(AutoSwitchedParameter<float> local_param, NetworkVariable<float> remote_param)
    {
        localParam = local_param;
        remoteParam = remote_param;
    }
    public NetworkVariable<float> remoteParam;
    public AutoSwitchedParameter<float> localParam;
}

public class AudoSwitchedParameterManager : MonoBehaviour
{
    //public NetworkVariable<float> f1;

    //public NetworkVariable<float> f2;

    //public NetworkVariable<float> f3;

    //public NetworkVariable<float> f4;

    //public NetworkVariable<float> f5;

    //public NetworkVariable<float> thickness;

    //int groupParameterCount = 0;




    Dictionary<string, LocalRemoteParameterPair> parameterList = new Dictionary<string, LocalRemoteParameterPair>();


    void Update()
    {
        foreach (LocalRemoteParameterPair pair in parameterList.Values)
        {
            if(pair.localParam.Changed)
            {
                pair.remoteParam.Value = pair.localParam.Value;
            }
        }
    }


    //void MapRemoteParameterWithLocalParameter(string name, NetworkVariable<float> remote_variable)
    //{
    //    if (parameterList.ContainsKey("name"))
    //        return;
    //    AutoSwitchedParameter<float> param = new AutoSwitchedParameter<float>();
    //    localParameterList.Add(name, param);
    //    remoteParameterList.Add(name, remote_variable);
    //}

    public bool RegisterParameterPair(string name, AutoSwitchedParameter<float> local_param, NetworkVariable<float> remote_param)
    {
        if (parameterList.ContainsKey("name"))
            return false;

        parameterList.Add(name, new LocalRemoteParameterPair(local_param, remote_param));
        return true;
    }

    //public void SetLocalParameter(string name, float v)
    //{
    //    if (localParameterList.ContainsKey(name) == false)
    //        return;

    //    localParameterList[name].CodaValue = v;
    //}

    //public void SetRemoteParameter(string name, float v)
    //{
    //    if (remoteParameterList.ContainsKey(name) == false)
    //        return;

    //    remoteParameterList[name].Value = v;
    //}

    #region Instance
    private static AudoSwitchedParameterManager _Instance;

    public static AudoSwitchedParameterManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<AudoSwitchedParameterManager>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    _Instance = go.AddComponent<AudoSwitchedParameterManager>();
                }
            }
            return _Instance;
        }
    }
    #endregion
}
