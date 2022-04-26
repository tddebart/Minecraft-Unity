using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreationMenu : MonoBehaviour
{
    public TMP_InputField lobbyNameInput;
    public Button createButton;
    public static string lobbyName;
    public Menu worldListMenu;
    
    public void OnEnable()
    {
        // lobbyNameInput.text = "";
        // createButton.interactable = false;
        
        lobbyNameInput.onValueChanged.AddListener(delegate { OnLobbyNameChanged(); });
        OnLobbyNameChanged();
    }
    
    public void OnLobbyNameChanged()
    {
        createButton.interactable = lobbyNameInput.text.Length > 0;
        lobbyName = lobbyNameInput.text;
    }
    
    public async void OnCreateButtonClicked()
    {
        worldListMenu.Open();
        worldListMenu.GetComponent<WorldListMenuManager>().GetWorlds();
        worldListMenu.GetComponent<WorldListMenuManager>().multiplayerMode = true;

        // if (createButton.interactable)
        // {
        //     var lobbyNull = await SteamMatchmaking.CreateLobbyAsync();
        //     if (lobbyNull.HasValue)
        //     {
        //         var lobby = lobbyNull.Value;
        //         lobby.SetPublic();
        //         lobby.SetData("name", lobbyNameInput.text);
        //         lobby.SetData("minecraft", "TRUE");
        //         NetworkManager.singleton.StartHost();
        //     }
        // }
    }
}