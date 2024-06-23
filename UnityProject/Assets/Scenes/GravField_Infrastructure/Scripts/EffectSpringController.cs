using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpringController : MonoBehaviour
{
    public Transform performerTransformRoot;

    RoleManager roleManager;

    List<Performer> performerList = new List<Performer>();
    List<bool> springStateList = new List<bool>();
    List<List<EffectSpring>> springList = new List<List<EffectSpring>>();
    bool effectEnabled = false;


    void Awake()
    {
        roleManager = FindObjectOfType<RoleManager>();


        for (int i=0; i<performerTransformRoot.childCount; i++)
        {
            performerList.Add(performerTransformRoot.GetChild(i).GetComponent<Performer>());
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            springStateList.Add(false);
            Transform spring_group_root = transform.GetChild(i);
            List<EffectSpring> spring_group = new List<EffectSpring>();
            springList.Add(spring_group);
            for (int k = 0; k < spring_group_root.childCount; k++)
            {
                EffectSpring spring = spring_group_root.GetChild(k).GetComponent<EffectSpring>();
                Vector2Int performer_index = GetPerformerIndexOfRope(i);
                spring.BindPerformer(performerList[performer_index.x], performerList[performer_index.y]);
                spring_group.Add(spring);
            }
        }
    }

    void Start()
    {
        
    }
    void OnEnable()
    {
        roleManager.OnStartPerformingEvent.AddListener(OnStartPerforming);
        roleManager.OnStopPerformingEvent.AddListener(OnStopPerforming);
    }
    void OnDisable()
    {
        roleManager.OnStartPerformingEvent.RemoveListener(OnStartPerforming);
        roleManager.OnStopPerformingEvent.RemoveListener(OnStopPerforming);
    }

    void OnStartPerforming(int index, ulong client_index)
    {
        UpdateAllRopeState();
    }

    void OnStopPerforming(int index, ulong client_index)
    {
        UpdateAllRopeState();
    }


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

    void SetRopeState(int index, bool state)
    {
        for (int i = 0; i < springList[index].Count; i++)
        {
            springList[index][i].SetSpringState(state);
        }


        //transform.GetChild(index).gameObject.SetActive(state);
        //SetSplineMeshVisible(index, state);
    }

    public void SetEffectState(bool state)
    {
        effectEnabled = state;
        UpdateAllRopeState();
    }
}
