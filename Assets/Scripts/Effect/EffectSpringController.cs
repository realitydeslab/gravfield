using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EffectSpringController : NetworkBehaviour
{
    [SerializeField]
    Transform performerTransformRoot;

    List<Performer> performerList = new List<Performer>();

    List<bool> springStateList = new List<bool>();

    List<EffectSpringGroup> springGroupList = new List<EffectSpringGroup>();

    bool effectEnabled = false;


    void Awake()
    {
        //Transform performerTransformRoot = GameManager.Instance.PlayerManager.PerformerTransformRoot;
        for (int i=0; i<performerTransformRoot.childCount; i++)
        {
            performerList.Add(performerTransformRoot.GetChild(i).GetComponent<Performer>());
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            springStateList.Add(false);

            EffectSpringGroup spring_group = transform.GetChild(i).GetComponent<EffectSpringGroup>();
            Vector2Int performer_index = GetPerformerIndexOfRope(i);
            spring_group.InitializeSpringGroup(performerList[performer_index.x], performerList[performer_index.y]);

            springGroupList.Add(spring_group);
        }
    }

    void Start()
    {

    }

    void OnEnable()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnStartGame.AddListener(OnStartGame);
        GameManager.Instance.OnStopGame.AddListener(OnStopGame);

        GameManager.Instance.PlayerManager.OnStartPerformingEvent.AddListener(OnStartPerforming);
        GameManager.Instance.PlayerManager.OnStopPerformingEvent.AddListener(OnStopPerforming);
    }
    void OnDisable()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnStartGame.RemoveListener(OnStartGame);
        GameManager.Instance.OnStopGame.RemoveListener(OnStopGame);

        GameManager.Instance.PlayerManager.OnStartPerformingEvent.RemoveListener(OnStartPerforming);
        GameManager.Instance.PlayerManager.OnStopPerformingEvent.RemoveListener(OnStopPerforming);
    }

    public void SetEffectState(bool state)
    {
        effectEnabled = state;
        UpdateAllRopeState();
    }



    #region Start / Stop game 
    void OnStartGame(PlayerRole player_role)
    {
        
    }

    void OnStopGame(PlayerRole player_role)
    {
        
    }
    #endregion

    #region Performer join / leave
    void OnStartPerforming(int index, ulong client_index)
    {
        UpdateAllRopeState();
    }

    void OnStopPerforming(int index, ulong client_index)
    {
        UpdateAllRopeState();
    }
    #endregion



    
    void UpdateAllRopeState()
    {
        for (int i = 0; i < springStateList.Count; i++)
        {
            bool cur_rope_state = GetRopeState(i);
            // Just active rope, generate a new one
            if (cur_rope_state == true && springStateList[i] == false)
            {

            }
            // Just deactive rope
            if (cur_rope_state == false && springStateList[i] == true)
            {

            }
            springStateList[i] = cur_rope_state;

            SetRopeState(i, cur_rope_state);
        }
    }

    void SetRopeState(int index, bool state)
    {

        springGroupList[index].SetSpringState(state);


        //transform.GetChild(index).gameObject.SetActive(state);
        //SetSplineMeshVisible(index, state);
    }

    bool GetRopeState(int rope_index)
    {
        if (effectEnabled == false)
            return false;

        Vector2Int performer_index = GetPerformerIndexOfRope(rope_index);
        return performerList[performer_index.x].isPerforming.Value == true && performerList[performer_index.y].isPerforming.Value == true;
    }

    Vector2Int GetPerformerIndexOfRope(int rope_index)
    {
        // Rope 0: Performer 0 , 1
        // Rope 1: Performer 0 , 2
        // Rope 2: Performer 1 , 2
        int start_index = rope_index == 2 ? 1 : 0;
        int end_index = rope_index == 0 ? 1 : 2;
        return new Vector2Int(start_index, end_index);
    }
}
