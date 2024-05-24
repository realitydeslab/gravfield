using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TestScenarioManager : MonoBehaviour
{
    [SerializeField]
    Transform[] synchronizedTransform;

    //[SerializeField]
    //Vector3 offset;

    Transform testEnvionmentTransformRoot;
    Transform testPerformerTransformRoot;

    PlayableDirector timeline;

    RoleManager roleManager;
    PerformerSynchronizer performerSynchronizer;

    bool needSynchronize = false;

    void Awake()
    {
        roleManager = FindObjectOfType<RoleManager>();
        performerSynchronizer = FindObjectOfType<PerformerSynchronizer>();

        testEnvionmentTransformRoot = transform.Find("Environment");
        testPerformerTransformRoot = transform.Find("Humanoids");

        timeline = transform.Find("Timeline").GetComponent<PlayableDirector>();
    }
    void Start()
    {
        if(IsValidEnvironment() == false)
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (needSynchronize == false || roleManager.PerformerTransformRoot.childCount != synchronizedTransform.Length)
            return;

        for (int i = 0; i < roleManager.PerformerTransformRoot.childCount; i++)
        {
            roleManager.PerformerTransformRoot.GetChild(i).localPosition = synchronizedTransform[i].position;//synchronizedTransform[i].TransformPoint(synchronizedTransform[i].localPosition/* + offset*/);
            roleManager.PerformerTransformRoot.GetChild(i).rotation = synchronizedTransform[i].rotation;
        }
    }

    void OnEnable()
    {
        roleManager?.OnSpecifyPlayerRoleEvent.AddListener(OnSpecifyPlayerRole);
    }

    void OnDisable()
    {
        roleManager?.OnSpecifyPlayerRoleEvent.RemoveListener(OnSpecifyPlayerRole);
    }

    void OnSpecifyPlayerRole(RoleManager.PlayerRole role)
    {
        if (IsValidEnvironment() && role == RoleManager.PlayerRole.Server)
        {
            testEnvionmentTransformRoot.gameObject.SetActive(true);
            testPerformerTransformRoot.gameObject.SetActive(true);
            timeline.gameObject.SetActive(true);
            timeline.Play();

            needSynchronize = true;

            Debug.Log("Reveal Test Scene");
        }
        else
        {
            testEnvionmentTransformRoot.gameObject.SetActive(false);
            testPerformerTransformRoot.gameObject.SetActive(false);
            timeline.gameObject.SetActive(false);
            timeline.Stop();

            needSynchronize = false;

            gameObject.SetActive(false);
        }
    }

    bool IsValidEnvironment()
    {
        return (GameManager.Instance.IsSoloMode == true && Application.platform != RuntimePlatform.IPhonePlayer);
    }
}
