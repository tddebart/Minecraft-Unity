using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Sprite image;
    private Sprite hoverImage;
    private Sprite disabledImage;
    public bool isDisabled;
    
    private void Awake()
    {
        image = GetComponent<Image>().sprite;
        hoverImage = Resources.LoadAll<Sprite>("Sprites/UI/widgets").First(s => s.name == "button_selected");
        disabledImage = Resources.LoadAll<Sprite>("Sprites/UI/widgets").First(s => s.name == "slider_background");

        if (isDisabled)
        {
            Disable();
        } else
        {
            Enable();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(isDisabled) return;
        
        GetComponent<Image>().sprite = hoverImage;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(isDisabled) return;
        
        GetComponent<Image>().sprite = image;
    }
    
    public void Disable()
    {
        GetComponent<Button>().interactable = false;
        GetComponent<Image>().sprite = disabledImage;
        GetComponentInChildren<TMP_Text>().color = new Color(0.63f, 0.63f, 0.63f);
        isDisabled = true;
    }
    
    public void Enable()
    {
        GetComponent<Button>().interactable = true;
        GetComponent<Image>().sprite = image;
        GetComponentInChildren<TMP_Text>().color = Color.white;
        isDisabled = false;
    }

    private void OnDisable()
    {
        OnPointerExit(null);
    }
}