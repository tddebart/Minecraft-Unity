using System;
using System.IO;
using System.Linq;
using Mirror;
using Newtonsoft.Json;
using Steamworks;
using TMPro;
using UnityEngine;

public class WorldCreationMenuManager : MonoBehaviour
{
    public TMP_InputField worldNameInputField;
    public TMP_InputField worldSeedInputField;
    
    public bool multiplayerMode;
    
    public void OnWorldNameInputChanged(string newName)
    {
        if (newName.Length <= 0)
        {
            transform.Find("Done").GetComponent<UIButton>().Disable();
        } else
        {
            transform.Find("Done").GetComponent<UIButton>().Enable();
        }
    }
    
    public async void CreateWorld()
    {
        CheckIfNameIsUsed();
        
        WorldSettingsManager.Instance.worldName = worldNameInputField.text;
        WorldSettingsManager.Instance.seedOffset = GenerateSeed(worldSeedInputField.text);
        
        if (!multiplayerMode)
        {
            NetworkManager.singleton.StartHost();
        }
        else
        {
            CreateLobby(worldNameInputField.text, WorldSettingsManager.Instance.seedOffset);
        }
    }

    public static async void CreateLobby(string worldName, Vector3Int seedOffset)
    {
        var lobbyNull = await SteamMatchmaking.CreateLobbyAsync();
        if (lobbyNull.HasValue)
        {
            var lobby = lobbyNull.Value;
            lobby.SetPublic();
            lobby.SetData("name", $"{LobbyCreationMenu.lobbyName}\n<color=#808080>{SteamClient.Name} playing on {worldName}</color>");
            lobby.SetData("seed", JsonConvert.SerializeObject(seedOffset));
            lobby.SetData("minecraft", "TRUE");
            NetworkManager.singleton.StartHost();
        }
    }

    private void CheckIfNameIsUsed()
    {
        var worldName = worldNameInputField.text;
        var count = 1;
        while (true)
        {
            var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraftUnity/saves/");
            var countOfWorldsWithSameName = new DirectoryInfo(savePath).EnumerateDirectories().Count(d => d.Name == worldNameInputField.text);
            if (countOfWorldsWithSameName != 0)
            {
                worldNameInputField.text = $"{worldName} ({count})";
                count++;
                continue; // Check if the new name is used again
            }

            break;
        }
    }

    public static Vector3Int GenerateSeed(string seed = "")
    {
        System.Random random;
        if (seed.Length == 0)
        {
            random = new System.Random();
        } else
        {
            random = new System.Random(seed.GetHashCode());
        }
        //TODO: Replace 500 with int max and min when noise generator is fixed
        return new Vector3Int(random.Next(-500, 500), random.Next(-500, 500), random.Next(-500, 500));
    }
}