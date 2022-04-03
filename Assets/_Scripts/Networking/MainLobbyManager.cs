using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.FizzySteam;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainLobbyManager : MonoBehaviour
{
    public NetworkManager networkManager;
    public TextMeshProUGUI lobbyName;
    public Transform content;
    public static Lobby currentLobby;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        networkManager = GetComponent<NetworkManager>();
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        // StartCoroutine(lobbyTimer());
        // SceneManager.sceneLoaded += (scene, mode) =>
        // {
        //     if (scene.name != "Lobby")
        //     {
        //         StopAllCoroutines();
        //     }
        // };
    }

    public async void HostLobby()
    {
        var lobbyNull = await SteamMatchmaking.CreateLobbyAsync();
        if (lobbyNull.HasValue)
        {
            var lobby = lobbyNull.Value;
            lobby.SetPublic();
            lobby.SetData("name", lobbyName.text);
            lobby.SetData("minecraft", "TRUE");
            networkManager.StartHost();
        }
    }
    
    public async void GetLobbys()
    {
        for (var i = 1; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        
        var lobbys = await SteamMatchmaking.LobbyList.WithKeyValue("minecraft", "TRUE").RequestAsync();
        if (lobbys != null)
        {
            foreach (var lobby in lobbys)
            {
                var obj = Instantiate(content.GetChild(0).gameObject, content);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = lobby.GetData("name") + " " + lobby.MemberCount + "/" + lobby.MaxMembers;
                obj.SetActive(true);
                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    lobby.Join();
                    lobbyName.text = lobby.GetData("name");
                });
            }
        }
    }
    
    // IEnumerator lobbyTimer()
    // {
    //     GetLobbys();
    //     yield return new WaitForSeconds(5);
    //     StartCoroutine(lobbyTimer());
    // }

    public void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        lobby.Join();
    }
    
    public void OnLobbyEntered(Lobby lobby)
    {
        currentLobby = lobby;
        if (NetworkServer.active)
        {
            return;
        }

        if(networkManager.transport is FizzyFacepunch)
        {
            networkManager.networkAddress = lobby.Owner.Id.ToString();
        }
        else
        {
            networkManager.networkAddress = "localhost";
        }
        
        networkManager.StartClient();
    }

    // private void OnDisable()
    // {
    //     StopAllCoroutines();
    // }
}
