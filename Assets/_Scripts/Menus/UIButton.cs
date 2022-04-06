using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Sprite image;
    private Sprite hoverImage;
    
    private void Awake()
    {
        image = GetComponent<Image>().sprite;
        hoverImage = Resources.LoadAll<Sprite>("Sprites/UI/widgets").First(s => s.name == "button_selected");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Image>().sprite = hoverImage;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<Image>().sprite = image;
    }

    private void OnDisable()
    {
        OnPointerExit(null);
    }
}