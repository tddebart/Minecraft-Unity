using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInputField : TMP_InputField
{

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        foreach (Transform trs in transform.GetChild(1))
        {
            trs.GetComponent<Image>().color = Color.white;
        }

        if (transform.Find("Text Area/Caret") != null)
        {
            var rect = transform.Find("Text Area/Caret").GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(rect.offsetMin.x, 0.3699999f);
            rect.offsetMax = new Vector2(rect.offsetMax.x, -0.6300001f);
            var scale = rect.localScale;
            scale.y = 0.9f;
            rect.localScale = scale;
        }
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        foreach (Transform trs in transform.GetChild(1))
        {
            trs.GetComponent<Image>().color = Color.HSVToRGB(0, 0, 63f/100f);
        }
    }


}