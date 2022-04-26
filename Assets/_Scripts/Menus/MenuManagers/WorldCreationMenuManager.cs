using System;
using System.IO;
using System.Linq;
using Mirror;
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
        var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),".minecraftUnity/saves/");
        var countOfWorldsWithSameName = new DirectoryInfo(savePath).EnumerateDirectories().Count(d => d.Name == worldNameInputField.text);
        if (countOfWorldsWithSameName != 0)
        {
            worldNameInputField.text += $" ({countOfWorldsWithSameName})";
        }
        
        WorldSettingsManager.Instance.worldName = worldNameInputField.text;
        WorldSettingsManager.Instance.seedOffset = GenerateSeed(worldSeedInputField.text);
        
        if (!multiplayerMode)
        {
            NetworkManager.singleton.StartHost();
        }
        else
        {
            var lobbyNull = await SteamMatchmaking.CreateLobbyAsync();
            if (lobbyNull.HasValue)
            {
                var lobby = lobbyNull.Value;
                lobby.SetPublic();
                lobby.SetData("name", $"{SteamClient.Name} playing on {worldNameInputField.text} ({LobbyCreationMenu.lobbyName})");
                lobby.SetData("minecraft", "TRUE");
                NetworkManager.singleton.StartHost();
            }
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