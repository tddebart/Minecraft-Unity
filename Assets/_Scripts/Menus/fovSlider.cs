using System;
using UnityEngine;
using UnityEngine.UI;

public class fovSlider : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Slider>().onValueChanged.AddListener(OnValueChanged);
    }
    
    public void OnValueChanged(float value)
    {
        if (World.Instance != null && Camera.main != null) Camera.main.fieldOfView = Mathf.Round(value);
    }
}