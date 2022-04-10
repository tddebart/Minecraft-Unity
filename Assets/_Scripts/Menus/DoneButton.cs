using UnityEngine;
using UnityEngine.UI;

public class DoneButton : MonoBehaviour
{
    public void Start()
    {
        if (SettingsManager.instance != null)
            GetComponent<Button>().onClick.AddListener(SettingsManager.instance.ApplySettings);
    }
}