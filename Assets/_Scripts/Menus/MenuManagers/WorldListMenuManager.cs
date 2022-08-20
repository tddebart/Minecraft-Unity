using System;
using System.IO;
using System.Linq;
using Mirror;
using Steamworks;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class WorldListMenuManager : MonoBehaviour
{
    public static WorldListMenuManager instance;
    public GameObject worldPrefab;
    public Transform worldContainer;
    public Menu joiningMenu;
    public Menu worldCreationMenu;
    
    public bool multiplayerMode;

    private void Awake()
    {
        instance = this;
    }

    public async void GetWorlds()
    {
        for (var i = 0; i < worldContainer.childCount; i++)
        {
            Destroy(worldContainer.GetChild(i).gameObject);
        }
        
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),".minecraftUnity/saves/");
        
        if (!Directory.Exists(path))
        {
            return;
        }
        
        var worlds = Directory.GetDirectories(path).ToList();
        worlds.Sort((x, y) =>
        
            File.GetLastWriteTime(Path.Combine(y, "world.json")).CompareTo(File.GetLastWriteTime(Path.Combine(x, "world.json")))
        );

        foreach (var world in worlds)
        {
            var worldFolderName = Path.GetFileName(world);
            var worldFilePath = Path.Combine(world, "world.json");
            
            var worldFile = await File.ReadAllTextAsync(worldFilePath);
            var worldSaveData = JsonUtility.FromJson<WorldSaveData>(worldFile);
            

            var worldObject = Instantiate(worldPrefab, worldContainer);

            worldObject.GetComponentsInChildren<TextMeshProUGUI>()[0].text = worldSaveData.worldName;
            worldObject.GetComponentsInChildren<TextMeshProUGUI>()[1].text = worldFolderName + " (" + File.GetLastWriteTime(worldFilePath).ToString("dd/MM/yy HH:mm tt") + ")";
            
            worldObject.GetComponent<WorldButton>().onClick += async () =>
            {
                await WorldSettingsManager.Instance.LoadWorldSettings(worldSaveData);
                joiningMenu.Open();
                if (!multiplayerMode)
                {
                    NetworkManager.singleton.StartHost();
                }
                else
                {
                    WorldCreationMenuManager.CreateLobby(worldSaveData.worldName, worldSaveData.seedOffset);
                }
            };
        }

    }

    public void OpenCreationMenu()
    {
        worldCreationMenu.Open();
        worldCreationMenu.GetComponent<WorldCreationMenuManager>().multiplayerMode = multiplayerMode;
    }

    public void SetMultiplayerMode(bool value)
    {
        multiplayerMode = value;
    }

    public void JoinSelectedWorld()
    {
        WorldButton.currentWorldButton.onClick?.Invoke();
    }
    
    public void DeleteSelectedWorld()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),".minecraftUnity/saves/");
        var worldFolder = Path.Combine(path, WorldButton.currentWorldButton.GetComponentInChildren<TextMeshProUGUI>().text);
        Directory.Delete(worldFolder, true);

        GetWorlds();
    }

    public void EnableJoinButton()
    {
        transform.Find("Layout/Play").GetComponent<UIButton>().Enable();
        transform.Find("Layout/Delete").GetComponent<UIButton>().Enable();
    }

}