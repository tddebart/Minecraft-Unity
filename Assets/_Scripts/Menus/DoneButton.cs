using UnityEngine;
using UnityEngine.UI;

public class DoneButton : MonoBehaviour
{
    public void Start()
    {
        GetComponent<Button>().onClick.AddListener(SettingsManager.instance.ApplySettings);
    }
}