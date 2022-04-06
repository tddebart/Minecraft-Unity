using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIToggleButton : MonoBehaviour
{
    public string[] visualNames;
    public int[] values;
    public int currentIndex;
    
    public static List<UIToggleButton> toggleButtons = new List<UIToggleButton>();
    
    public void Start()
    {
        toggleButtons.Add(this);
    }
    
    public void SetValue(int value)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == value)
            {
                currentIndex = i;
                UpdateVisual();
                break;
            }
        }
    }

    public void NextValue()
    {
        currentIndex = (currentIndex + 1) % values.Length;
        UpdateVisual();
    }
    
    public void UpdateVisual()
    {
        GetComponentInChildren<TextMeshProUGUI>().text = visualNames[currentIndex];
        SettingsManager.instance.SetValue(this.name, values[currentIndex]);
    }
}