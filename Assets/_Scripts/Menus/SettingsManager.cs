using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    public float brightness;
    public int renderDistance;
    public int guiScale;
    public int fov;
    
    public readonly Dictionary<string,float> settings = new Dictionary<string, float>();
    
    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }
    
    public void Start()
    {
        this.ExecuteAfterFrames(1, () =>
        {
            LoadSettings();
            foreach (var slider in UISlider.sliders.Values)
            {
                slider.SetValue(settings[slider.name]);
            }

            foreach (var toggleButton in UIToggleButton.toggleButtons)
            {
                toggleButton.SetValue((int)settings[toggleButton.name]);
            }

            ApplySettings();
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (scene.name == "World")
                {
                    ApplySettings();
                }
            };
        });
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("brightness", brightness);
        PlayerPrefs.SetInt("renderDistance", renderDistance);
        PlayerPrefs.SetInt("guiScale", guiScale);
        PlayerPrefs.SetInt("fov", fov);
        
        PlayerPrefs.Save();
        
        SetDictionary();
    }
    
    public void LoadSettings()
    {
        brightness = PlayerPrefs.GetFloat("brightness", 0f);
        renderDistance = PlayerPrefs.GetInt("renderDistance", 4);
        guiScale = PlayerPrefs.GetInt("guiScale", 2);
        fov = PlayerPrefs.GetInt("fov", 90);
        
        SetDictionary();
    }

    public void SetDictionary()
    {
        settings["brightness"] = brightness;
        settings["renderDistance"] = renderDistance;
        settings["guiScale"] = guiScale;
        settings["fov"] = fov;
    }
    
    public void SetValue(string name, float value)
    {
        switch (name)
        {
            case "brightness":
                brightness = value;
                break;
            case "renderDistance":
                renderDistance = (int)value;
                break;
            case "guiScale":
                guiScale = (int)value;
                break;
            case "fov":
                fov = (int)value;
                break;
        }
    }
    
    public void ApplySettings()
    {
        SaveSettings();
        if (World.Instance != null) World.Instance.gamma = brightness/10f;
        if (World.Instance != null) World.Instance.renderDistance = renderDistance;
        foreach (var canvas in FindObjectsOfType<CanvasScaler>())
        {
            canvas.scaleFactor = guiScale;
        }
        if (Camera.main != null) Camera.main.fieldOfView = fov;
    }
    
}