using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldButton : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    public static WorldButton currentWorldButton;
    
    public Action onClick;
    
    private void Start()
    {
        onClick += () =>
        {
            GameObject.Find("Canvas/JoiningMenu").GetComponent<Menu>().Open();
        };
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentWorldButton != null)
        {
            currentWorldButton.SetBorder(false);
        }
        else
        {
            WorldListMenuManager.instance.EnableJoinButton();
        }
        currentWorldButton = this;
        SetBorder(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.clickCount == 2)
        {
            SetBorder(false);
            onClick?.Invoke();
        }
    }

    public void SetBorder(bool value)
    {
        if (value)
        {
            transform.GetChild(0).gameObject.SetActive(true);
        } 
        else
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}