using System.Collections;
using System.Collections.Generic;
using HoloKit.ImageTrackingRelocalization;
using UnityEngine;
using TMPro;

public class DisplayProgressText : MonoBehaviour
{
#if UNITY_IOS
    [SerializeField] ImageTrackingStablizer relocalizationStablizer;
    [SerializeField] TextMeshProUGUI progressText;
    void Update()
    {
        if (relocalizationStablizer.IsRelocalizing)
            progressText.text = Mathf.FloorToInt(relocalizationStablizer.Progress * 100f).ToString() + "%";
        else
            progressText.text = "Stopped";
    }
#endif
}
