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

    PlayerManager roleManager;
    PerformerSynchronizer performerSynchronizer;

    bool needSynchronize = false;

    void Awake()
    {
        roleManager = FindObjectOfType<PlayerManager>();
        performerSynchronizer = FindObjectOfType<PerformerSynchronizer>();

        testEnvionmentTransformRoot = transform.Find("Environment");
        testPerformerTransformRoot = transform.Find("Humanoids");

        timeline = transform.Find("Timeline").GetComponent<PlayableDirector>();
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
        GameManager.Instance.OnStartGame.AddListener(OnStartGame);
        GameManager.Instance.OnStopGame.AddListener(OnStopGame);
    }

    void OnDisable()
    {
        GameManager.Instance.OnStartGame.RemoveListener(OnStartGame);
        GameManager.Instance.OnStopGame.RemoveListener(OnStopGame);
    }

    public void TurnOn()
    {
        testEnvionmentTransformRoot.gameObject.SetActive(true);
        testPerformerTransformRoot.gameObject.SetActive(true);
        timeline.gameObject.SetActive(true);
        timeline.Play();

        needSynchronize = true;
    }

    public void TurnOff()
    {
        testEnvionmentTransformRoot.gameObject.SetActive(false);
        testPerformerTransformRoot.gameObject.SetActive(false);
        timeline.gameObject.SetActive(false);
        timeline.Stop();

        needSynchronize = false;
    }

    void OnStartGame(PlayerRole role)
    {
        if (IsValidEnvironment() && role == PlayerRole.Server)
        {
            TurnOn();
        }
    }

    void OnStopGame(PlayerRole role)
    {
        if (IsValidEnvironment() && role == PlayerRole.Server)
        {
            TurnOff();
        }
    }

    bool IsValidEnvironment()
    {
        return (GameManager.Instance.IsSoloMode == true && Application.platform != RuntimePlatform.IPhonePlayer);
    }
}
