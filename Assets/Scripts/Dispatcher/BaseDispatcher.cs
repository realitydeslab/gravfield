using UnityEngine;

public class BaseDispatcher : MonoBehaviour
{
    protected bool isDispatching = false;

    protected string oscBaseName = "";

    protected int oscIndex = -1;

    void OnEnable()
    {
        GameManager.Instance.OnStartGame.AddListener(OnStartGame);
        GameManager.Instance.OnStopGame.AddListener(OnStopGame);
    }

    void OnDisable()
    {
        GameManager.Instance.OnStartGame.RemoveListener(OnStartGame);
        GameManager.Instance.OnStopGame.RemoveListener(OnStopGame);
    }

    #region Start / Stop game 

    void OnStartGame(PlayerRole player_role)
    {
        if(player_role == PlayerRole.Server)
        {
            RegisterSender_ForLive();
            RegisterSender_ForCoda();
            RegisterReceiver();

            isDispatching = true;
        }
    }

    void OnStopGame(PlayerRole player_role)
    {
        if (player_role == PlayerRole.Server)
        { 
            //UnregisterSender_ForLive();
            //UnregisterSender_ForCoda();
            //UnregisterReceiver();

            isDispatching = false;
        }
    }
    #endregion

    protected virtual void RegisterSender_ForLive()
    {
        throw new System.NotImplementedException();
    }
    protected virtual void RegisterSender_ForCoda()
    {
        throw new System.NotImplementedException();
    }
    protected virtual void RegisterReceiver()
    {
        throw new System.NotImplementedException();
    }

    //protected virtual void UnregisterSender_ForLive()
    //{
    //    throw new System.NotImplementedException();
    //}
    //protected virtual void UnregisterSender_ForCoda()
    //{
    //    throw new System.NotImplementedException();
    //}
    //protected virtual void UnregisterReceiver()
    //{
    //    throw new System.NotImplementedException();
    //}


    //#region Receiver
    //protected override void RegisterReceiver()
    //{

    //}

    //protected override void UnregisterReceiver()
    //{

    //}
    //#endregion


    //#region Sender for live
    //protected override void RegisterSender_ForLive()
    //{

    //}
    //protected override void RegisterSender_ForCoda()
    //{

    //}
    //#endregion


    //#region Sender for Coda
    //protected override void UnregisterSender_ForLive()
    //{

    //}
    //protected override void UnregisterSender_ForCoda()
    //{

    //}
    //#endregion



    protected string FormateOscAddress_Sender(string base_name, int index, string param_name)
    {
        if (base_name.Length == 0)
            return "";

        if (base_name[0] == '/')
        {
            base_name = base_name.Substring(1);
        }
        return "/" + base_name + index.ToString() + param_name;
    }

    protected string FormateOscAddress_Sender(string base_name, string index_name, string param_name)
    {
        return base_name + index_name + param_name;
    }

    protected string FormateOscAddress(string param, int index)
    {
        if (param.Length == 0)
            return "";

        if (param[0] == '/')
        {
            param = param.Substring(1);
        }
        if (param.Contains("-"))
        {
            string[] split_str = param.Split("-");
            return "/" + split_str[0] + index.ToString() + "-" + split_str[1];
        }

        return "/" + param + index.ToString();
    }
}
