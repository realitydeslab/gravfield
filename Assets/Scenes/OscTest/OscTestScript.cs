using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OscTestScript : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI oscText;
    string oscStr;


    void Update()
    {
        oscText.text = oscStr;
    }

    public void OnReceiveMessage(float v)
    {
        oscStr = v.ToString();
    }
}
