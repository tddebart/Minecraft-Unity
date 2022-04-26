using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldSettingsManager : MonoBehaviour
{
    public static WorldSettingsManager Instance;
    
    public Vector3Int seedOffset;
    public string worldName;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (World.Instance != null)
            {
                World.Instance.mapSeedOffset = seedOffset;
                World.Instance.worldName = worldName;
            }
        };
    }

    public Task LoadWorldSettings(WorldSaveData worldSaveData)
    {
        return Task.Run(() =>
        {
            worldName = worldSaveData.worldName;
            seedOffset = worldSaveData.seedOffset;
        });
    }
}