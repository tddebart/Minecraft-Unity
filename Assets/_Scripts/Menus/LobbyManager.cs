using Steamworks.Data;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    public Lobby? currentLobby;

    private void Awake()
    {
        instance = this;
    }
}