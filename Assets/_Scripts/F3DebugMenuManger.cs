using System;
using Unity.VisualScripting;
using UnityEngine;

public partial class F3MenuManger : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject timeMenu;
    
    private void Start()
    {
        mainMenu = shiftF3Container.GetChild(0).gameObject;
        timeMenu = shiftF3Container.Find("time").gameObject;
    }

    public bool CheckForDebugControls()
    {
        // Check which menu is open

        if (mainMenu.activeSelf)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyUp(KeyCode.F3))
                {
                    mainMenu.SetActive(false);
                    return true;
                }
            }
            
            if (Input.GetKey(KeyCode.F3))
            {
                // Open time menu
                if (Input.GetKeyDown(KeyCode.T))
                {
                    mainMenu.SetActive(false);
                    timeMenu.SetActive(true);
                    
                    return true;
                }
            }
        } 
        else if (timeMenu.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                World.Instance.skyLightMultiplier = Mathf.Clamp(World.Instance.skyLightMultiplier+0.1f,0.15F,1f);
                World.Instance.UpadteLightTexture();
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                World.Instance.skyLightMultiplier = Mathf.Clamp(World.Instance.skyLightMultiplier-0.1f,0.15F,1f);;
                World.Instance.UpadteLightTexture();
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                mainMenu.SetActive(true);
                timeMenu.SetActive(false);
                return true;
            }
        }
        
        
        return false;
    }
}