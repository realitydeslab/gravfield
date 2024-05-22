using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public EffectRopeController effectRope;

    public EffectSpringController effectSpring;

    public EffectMagneticField effectMagneticField;

    float effectMode = 0;

    void OnEnable()
    {
        GameManager.Instance.RoleManager.OnSpecifyPlayerRoleEvent.AddListener(OnSpecifyPlayerRole);
    }

    void OnDisable()
    {
        GameManager.Instance.RoleManager.OnSpecifyPlayerRoleEvent.RemoveListener(OnSpecifyPlayerRole);
    }

    void OnSpecifyPlayerRole(RoleManager.PlayerRole role)
    {
        ChangeEffectModeTo(GameManager.Instance.PerformerGroup.effectMode.Value);
    }


    #region NetworkVariable / Clients also should execute
    void RegisterNetworkVariableCallback_Client()
    {
        GameManager.Instance.PerformerGroup.effectMode.OnValueChanged += (float prev, float cur) => { ChangeEffectModeTo(cur); };
    }

    void ChangeEffectModeTo(float effect_index)
    {
        effectMode = Mathf.RoundToInt(effect_index);

        effectRope.SetEffectState(effectMode == 0);

        effectSpring.SetEffectState(effectMode == 1);

        effectMagneticField.SetEffectState(effectMode == 2);
    }
    #endregion

    

    #region Instance
    private static EffectManager _Instance;

    public static EffectManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<EffectManager>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject();
                    _Instance = go.AddComponent<EffectManager>();
                }
            }
            return _Instance;
        }
    }
    #endregion
}
