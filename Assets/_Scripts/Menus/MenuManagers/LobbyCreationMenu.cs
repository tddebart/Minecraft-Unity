using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreationMenu : MonoBehaviour
{
    public TMP_InputField lobbyNameInput;
    public Button createButton;
    
    public void OnEnable()
    {
        // lobbyNameInput.text = "";
        // createButton.interactable = false;
        
        lobbyNameInput.onValueChanged.AddListener(delegate { OnLobbyNameChanged(); });
    }
    
    public void OnLobbyNameChanged()
    {
        createButton.interactable = lobbyNameInput.text.Length > 0;
    }
    
    public async void OnCreateButtonClicked()
    {
        if (createButton.interactable)
        {
            var lobbyNull = await SteamMatchmaking.CreateLobbyAsync();
            if (lobbyNull.HasValue)
            {
                var lobby = lobbyNull.Value;
                lobby.SetPublic();
                lobby.SetData("name", lobbyNameInput.text);
                lobby.SetData("minecraft", "TRUE");
                NetworkManager.singleton.StartHost();
            }
        }
    }
}