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


    #region Instance
    private static Commander _Instance;

    public static Commander Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<Commander>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    _Instance = go.AddComponent<Commander>();
                }
            }
            return _Instance;
        }
    }
    #endregion
}
