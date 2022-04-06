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
        transform.localScale = Vector3.one;
        if (currentMenu != null)
        {
            lastMenu = currentMenu;
            currentMenu.Close();
        }
        currentMenu = this;
    }
    
    public void Close()
    {
        transform.localScale = new Vector3(0.0001f,0.0001f,1);
    }

    public static void CloseCurrentMenu()
    {
        currentMenu.Close();
        lastMenu.Open();
    }

}