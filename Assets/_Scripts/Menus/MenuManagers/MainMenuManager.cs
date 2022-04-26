using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // transform.Find("Layout/Singleplayer").GetComponent<Button>().onClick.AddListener(() =>
        // {
        //     NetworkManager.singleton.StartHost();
        // });
        transform.Find("Layout/Bottom/Quit").GetComponent<Button>().onClick.AddListener(Application.Quit);
    }
}