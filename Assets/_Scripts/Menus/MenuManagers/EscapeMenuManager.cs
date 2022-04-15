using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class EscapeMenuManager : MonoBehaviour
{
    private void Start()
    {
        transform.Find("Layout/Return").GetComponent<Button>().onClick.AddListener(Return);
        transform.Find("Layout/Quit").GetComponent<Button>().onClick.AddListener(Quit);
    }
    
    private void Quit()
    {
        if(LobbyManager.currentLobby.HasValue)  LobbyManager.currentLobby.Value.Leave();
        NetworkManager.singleton.StopHost();
        NetworkManager.singleton.StopClient();
    }
    
    private void Return()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GameManager.Instance.localPlayer.escapeMenuOpen = false;
    }
}