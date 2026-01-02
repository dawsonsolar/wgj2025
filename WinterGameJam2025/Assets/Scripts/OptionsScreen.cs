using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Taken mostly from https://www.youtube.com/watch?v=yeaELkoxD9w
public class OptionsScreen : MonoBehaviour
{
    public Toggle fullScreenToggle, vsyncToggle;
    public List<ResItem> resolutions = new List<ResItem>();
    private int selectedResolution;

    public TMP_Text resolutionLabel;
    void Start()
    {
        fullScreenToggle.isOn = Screen.fullScreen;

        if (QualitySettings.vSyncCount == 0 )
        {
            vsyncToggle.isOn = false;
        }
        else
        {
            vsyncToggle.isOn = true;
        }
    }

    void Update()
    {
        
    }

    public void ResLeft()
    {
        selectedResolution--;
        if (selectedResolution < 0 ) 
        {
            selectedResolution = 0;
        }

        UpdateResLabel();
    }

    public void ResRight()
    {
        selectedResolution++;
        if (selectedResolution > resolutions.Count - 1)
        {
            selectedResolution = resolutions.Count - 1;
        }

        UpdateResLabel();
    }

    public void UpdateResLabel()
    {
        resolutionLabel.text = resolutions[selectedResolution].horizontal.ToString() + "x" + resolutions[selectedResolution].vertical.ToString();
    }

    public void ApplyGraphics()
    {
       // Screen.fullScreen = fullScreenToggle.isOn;

        if (vsyncToggle.isOn) 
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
        }

        Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, fullScreenToggle.isOn);
    }
}

[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}
