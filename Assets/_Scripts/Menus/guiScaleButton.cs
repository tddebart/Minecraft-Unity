using UnityEngine;
using UnityEngine.UI;

public class guiScaleButton : MonoBehaviour
{

    public void ApplyGuiScale()
    {
        foreach (var canvas in FindObjectsOfType<CanvasScaler>())
        {
            var toggleButton = GetComponent<UIToggleButton>();
            canvas.scaleFactor = toggleButton.values[toggleButton.currentIndex];
        }
    }
}