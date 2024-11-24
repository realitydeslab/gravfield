using UnityEngine;

public class BaseDispatcher : MonoBehaviour
{
    protected bool isDispatching = false;

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

}
