using Mirror.FizzySteam;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenuManager : MonoBehaviour
{
    public static MultiplayerMenuManager Instance;

    public GameObject lobbyPrefab;
    public GameObject textPrefab;
    public Transform lobbyContainer;
    public LobbyButton selectedLobby;
    
    public void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Instance = this;
    }

    public void PressSelectedLobby()
    {
        if (selectedLobby != null)
        {
            selectedLobby.onClick.Invoke();
        }
    }
    
    public async void GetLobbys()
    {
        
        for (var i = 0; i < lobbyContainer.childCount; i++)
        {
            Destroy(lobbyContainer.GetChild(i).gameObject);
        }
        var lobbys = await SteamMatchmaking.LobbyList.WithKeyValue("minecraft", "TRUE").RequestAsync();
        if (lobbys != null)
        {
            foreach (var lobby in lobbys)
            {
                var obj = Instantiate(lobbyPrefab, lobbyContainer);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = lobby.GetData("name");
                obj.GetComponentsInChildren<TextMeshProUGUI>()[1].text = lobby.MemberCount + "/" + lobby.MaxMembers;
                obj.SetActive(true);
                obj.GetComponent<LobbyButton>().onClick += () =>
                {
                    lobby.Join();
                };
            }
        }
        else
        {
            var obj = Instantiate(textPrefab, lobbyContainer);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = "No lobbys found :(";
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(305, 130);
        }
    }
}