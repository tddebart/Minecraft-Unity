using Mirror;
using Mirror.FizzySteam;
using Newtonsoft.Json;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    
    private void Start()
    {
        Instance = this;
        Cursor.lockState = CursorLockMode.None;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
    }

    public void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        lobby.Join();
    }
    
    public void OnLobbyEntered(Lobby lobby)
    {
        LobbyManager.currentLobby = lobby;
        if (NetworkServer.active)
        {
            return;
        }

        WorldSettingsManager.Instance.seedOffset = JsonConvert.DeserializeObject<Vector3Int>(lobby.GetData("seed"));

        if(NetworkManager.singleton.transport is FizzyFacepunch)
        {
            NetworkManager.singleton.networkAddress = lobby.Owner.Id.ToString();
        }
        else
        {
            NetworkManager.singleton.networkAddress = "localhost";
        }
        
        NetworkManager.singleton.StartClient();
    }
    
    public void CancelLobby()
    {
        LobbyManager.currentLobby?.Leave();
        NetworkManager.singleton.StopHost();
        NetworkManager.singleton.StopClient();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Menu.CloseCurrentMenu();
        }
    }
}