using UnityEngine;

public class Commander : MonoBehaviour
{
    public void InitializeCommander()
    {
        // add item to ui
        //ControlPanel.Instance.AddProperyInControlPanel_CommanderMode("/rope-mass");

        // add sender to send osc when adjusting parameter

    }

    public void DeinitializeCommander()
    {
        // delete all ui;

        // delete all sender;
    }

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

    void OnStartGame(PlayerRole role)
    {

    }

    void OnStopGame(PlayerRole role)
    {

    }

}
