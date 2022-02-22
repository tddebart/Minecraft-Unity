
using UnityEditor;
using UnityEngine;

namespace EckTechGames
{
	[InitializeOnLoad]
	public class AutoSaveExtension
	{
		// Static constructor that gets called when unity fires up.
		static AutoSaveExtension()
		{
#pragma warning disable CS0618
			EditorApplication.playmodeStateChanged += AutoSaveWhenPlaymodeStarts;
#pragma warning restore CS0618
		}

		private static void AutoSaveWhenPlaymodeStarts()
		{
			// If we're about to run the scene...
			if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
			{
				// Save the scene and the assets.
#pragma warning disable CS0618
				EditorApplication.SaveScene();
#pragma warning restore CS0618
				AssetDatabase.SaveAssets();
			}
		}
	}
}