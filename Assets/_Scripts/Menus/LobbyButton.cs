using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LobbyButton : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    public static readonly List<LobbyButton> lobbyButtons = new List<LobbyButton>();
    
    public static LobbyButton currentLobbyButton;
    
    public Action onClick;

    private void Start()
    {
        onClick += () =>
        {
            GameObject.Find("Canvas/JoiningMenu").GetComponent<Menu>().Open();
        };
    }

    private void OnEnable()
    {
        lobbyButtons.Add(this);
    }
    
    private void OnDisable()
    {
        lobbyButtons.Remove(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentLobbyButton != null)
        {
            currentLobbyButton.SetBorder(false);
        }
        currentLobbyButton = this;
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
            transform.GetChild(2).gameObject.SetActive(true);
            MultiplayerMenuManager.Instance.selectedLobby = this;
        } 
        else
        {
            transform.GetChild(2).gameObject.SetActive(false);
            MultiplayerMenuManager.Instance.selectedLobby = null;
        }
    }
}