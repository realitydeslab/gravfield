using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class OscillatingNode
{
    public OscillatingNode(float _shakeSpeed, float _shakeSinOffset, float _shakeSinFreq)
    {
        shakeSpeed = 10;
        shakeSinValue = 0;
        shakeSinOffset = 0;
        shakeSinFreq = 0.4f;
        shakeY = 0.01f;
    }
    public float shakeSpeed;
    public float shakeSinValue;
    public float shakeSinOffset;
    public float shakeSinFreq;
    public float shakeY;
}


public class EffectSpringGroup_Old : MonoBehaviour
{
    Performer performerStart;
    Performer performerEnd;

    int springIndex;
    public int SpringIndex { get => springIndex; }

    bool springEnabled = false;

    [SerializeField]
    OscillatingNode startNode = new OscillatingNode(10, 0, 0.1f);
    [SerializeField]
    OscillatingNode endNode = new OscillatingNode(20, 0.1f, 0.1f);

    // Effect Parameters
    [SerializeField] float ropeMass = 42.8f;
    [SerializeField] float jointMass = 1.17f;
    [SerializeField] float anchorMass = 100f;
    [SerializeField] float thickness = 2;
    [SerializeField] float hingeSpring = 80;
    [SerializeField] float hingeDamper = 50;

    List<EffectSpring> springList = new List<EffectSpring>();

    void Awake()
    {
        springIndex = transform.GetSiblingIndex();
    }

    public void InitializeSpringGroup(Performer start, Performer end)
    {
        performerStart = start;
        performerEnd = end;

        for (int i = 0; i < transform.childCount; i++)
        {
            EffectSpring spring = transform.GetChild(i).GetComponent<EffectSpring>();
            spring.BindPerformer(start, end);
            springList.Add(spring);
        }
    }

    public void SetSpringState(bool state)
    {
        springEnabled = state;
        for (int i = 0; i < springList.Count; i++)
        {
            springList[i].SetSpringState(state);
        }
    }

    void Update()
    {
        if (springEnabled == false) return;

        // update parameters
        UpdateParameter();

    }

    void UpdateParameter()
    {
        // Shake
        startNode.shakeSinValue += Time.deltaTime * startNode.shakeSpeed;
        float sin_value = Mathf.Sin(startNode.shakeSinValue * startNode.shakeSinFreq + startNode.shakeSinOffset);
        float shake_y_start = startNode.shakeY * sin_value;
        float shake_angle_start = sin_value * 90;

        endNode.shakeSinValue += Time.deltaTime * endNode.shakeSpeed;
        sin_value = Mathf.Sin(endNode.shakeSinValue * endNode.shakeSinFreq + endNode.shakeSinOffset);
        float shake_y_end = endNode.shakeY * sin_value;
        float shake_angle_end = sin_value * 90;

        // 
        for (int i=0; i<springList.Count; i++)
        {
            springList[i].ropeMass = ropeMass;
            springList[i].jointMass = jointMass;
            springList[i].anchorMass = anchorMass;
            springList[i].thickness = thickness;
            springList[i].hingeSpring = hingeSpring;
            springList[i].hingeDamper = hingeDamper;

            springList[i].shakeYStart = shake_y_start;
            springList[i].shakeYEnd = shake_y_end;

            springList[i].shakeAngleStart = shake_angle_start;
            springList[i].shakeAngleEnd = shake_angle_end;
        }
    }


}
