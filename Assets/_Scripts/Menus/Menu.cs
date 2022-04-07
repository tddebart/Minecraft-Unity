using UnityEngine;

public class Menu : MonoBehaviour
{
    public bool opened;

    public static Menu lastMenu;
    public static Menu currentMenu;
    
    private void Start()
    {
        if (opened)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    public void Open()
    {
        if (currentMenu != null)
        {
            lastMenu = currentMenu;
            currentMenu.Close();
        }
        currentMenu = this;
        transform.localScale = Vector3.one;
        opened = true;
    }
    
    public void Close()
    {
        transform.localScale = new Vector3(0.0001f,0.0001f,1);
        opened = false;
    }
    
    public void Toggle()
    {
        if (opened)
        {
            Close();
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Open();
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public static void CloseCurrentMenu()
    {
        currentMenu.Close();
        lastMenu.Open();
    }

}