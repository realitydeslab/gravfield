using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformerTestAnimation : MonoBehaviour
{
    [SerializeField]
    bool needSynchronize = false;
    [SerializeField]
    Transform performerTransformRoot;
    [SerializeField]
    Transform[] dummyHeadTransform;

    [SerializeField]
    Transform testEnvionmentTransformRoot;

    [SerializeField]
    Transform testPerformerTransformRoot;

    RoleManager roleManager;

    bool initialized = false;

    void Awake()
    {
        roleManager = FindObjectOfType<RoleManager>(); 
    }
    void OnDisable()
    {
        roleManager?.OnSpecifyPlayerRoleEvent.RemoveListener(OnSpecifyPlayerRole);
    }
    void OnEnable()
    {
        roleManager?.OnSpecifyPlayerRoleEvent.AddListener(OnSpecifyPlayerRole);
    }

    // Update is called once per frame
    void Update()
    {
        if (initialized == false || needSynchronize == false || performerTransformRoot == null || performerTransformRoot.childCount != dummyHeadTransform.Length)
            return;

        for (int i = 0; i < performerTransformRoot.childCount; i++)
        {
            performerTransformRoot.GetChild(i).localPosition = dummyHeadTransform[i].position;
            performerTransformRoot.GetChild(i).localRotation = dummyHeadTransform[i].rotation;
        }
    }

    void OnSpecifyPlayerRole(RoleManager.PlayerRole role)
    {
        if (role == RoleManager.PlayerRole.Server && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            initialized = true;

            testEnvionmentTransformRoot.gameObject.SetActive(true);
            testPerformerTransformRoot.gameObject.SetActive(true);

            Debug.Log("Reveal Test Scene");
        }
        else
        {
            needSynchronize = false;

            testEnvionmentTransformRoot.gameObject.SetActive(false);
            testPerformerTransformRoot.gameObject.SetActive(false);
        }
    }
}
