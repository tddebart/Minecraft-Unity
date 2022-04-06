using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISlider : MonoBehaviour
{
    public string prefix;
    public string suffix;
    public bool wholeNumbers;
    
    private Slider slider;
    private TextMeshProUGUI text;
    
    public static readonly Dictionary<string, UISlider> sliders = new();
     
    private void Awake()
    {
        slider = GetComponent<Slider>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        sliders.TryAdd(name, this);
    }
    
    public void SetValue(float value)
    {
        slider.value = value;
        if (wholeNumbers)
        {
            value = Mathf.Round(value);
        }
        
        if (value == Math.Floor(value))
        {
            text.text = prefix + value + suffix;
        }
        else
        {
            text.text = prefix + value.ToString("0.0") + suffix;
        }
    }
    
    public void OnValueChanged(float value)
    {
        if (wholeNumbers)
        {
            value = Mathf.Round(value);
        }
        if (value == Math.Floor(value))
        {
            text.text = prefix + value + suffix;
        }
        else
        {
            text.text = prefix + value.ToString("0.0") + suffix;
        }
        SettingsManager.instance.SetValue(name, value);
    }
}