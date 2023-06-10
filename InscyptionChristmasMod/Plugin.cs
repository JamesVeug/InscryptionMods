using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Items;
using InscryptionAPI.Items.Extensions;
using InscryptionAPI.Pelts;
using InscryptionAPI.Regions;
using InscryptionAPI.Sound;
using InscryptionAPI.Totems;
using MoreItemsMod.Scripts.Items;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Diagnostics;
using Random = UnityEngine.Random;

namespace MoreItemsMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
	    public const string PluginGuid = "zzzjamesgames.inscryption.christmasmod";
	    public const string PluginName = "ChristmasMod";
	    public const string PluginVersion = "0.1.0.0";

	    public static string Directory;
	    public static ManualLogSource Log;

	    List<string> scenaryPrefabNames = new List<string>();
	    
	    public static List<string> assetBundlePrefabNames = new List<string>();
	    public static AssetBundle presentDecoratioAssetBundle = null;
	    public static AssetBundle itemAssetBundle = null;


	    private void Awake()
        {
	        Log = Logger;
	        Log.LogInfo($"Loading {PluginName}...");
	        Directory = this.Info.Location.Replace("ChristmasMod.dll", "");

	        PresentItems();
	        PresentDecorations();

	        new Harmony(PluginGuid).PatchAll();
	            
            Logger.LogInfo($"Loaded {PluginName}!");	        
        }

	    private void PresentItems()
	    {
		    string combine = Path.Combine(Directory, "AssetBundles/christmasitems");
		    itemAssetBundle = AssetBundle.LoadFromFile(combine);
		    if (itemAssetBundle == null)
		    {
			    Log.LogError("Could not load christmasitems assetbundle from path " + combine + "!");
		    }
		    
		    BonePresentItem.Initialize();
		    BloodPresentItem.Initialize();
	    }

	    private void PresentDecorations()
	    {
		    string combine = Path.Combine(Directory, "AssetBundles/presentdecorations");
		    presentDecoratioAssetBundle = AssetBundle.LoadFromFile(combine);
		    if (presentDecoratioAssetBundle == null)
		    {
			    Log.LogError("Could not load presentdecorations assetbundle from path " + combine + "!");
		    }
		    
		    for (int i = 1; i <= 8; i++)
		    {
			    assetBundlePrefabNames.Add($"Present{i}");
			    scenaryPrefabNames.Add($"Christmas/Present{i}");
		    }

		    foreach (RegionData regionData in RegionManager.AllRegionsCopy)
		    {
			    SceneryData data = ScriptableObject.CreateInstance<SceneryData>();
			    data.name = "Presents";
			    data.radius = 0.05f;
			    data.minScale = new Vector2(0.55f, 0.55f);
			    data.maxScale = new Vector2(0.85f, 0.85f);
			    data.prefabNames = scenaryPrefabNames;
			    regionData.fillerScenery ??= new List<FillerSceneryEntry>();


			    FillerSceneryEntry fillerData = new FillerSceneryEntry();
			    fillerData.data = data;
			    regionData.fillerScenery.Add(fillerData);
		    }
	    }

	    private static void LoadPresentsIntoResourceBank()
        {
	        foreach (string prefabName in assetBundlePrefabNames)
	        {
		        GameObject prefab = presentDecoratioAssetBundle.LoadAsset<GameObject>(prefabName);
		        if (prefab == null)
		        {
			        Log.LogError("Could not load asset " + prefabName);
			        continue;
		        }
                
		        if (prefab.GetComponent<MapElement>() == null)
		        {
			        prefab.AddComponent<MapElement>();
		        }

		        ResourceBank.instance.resources.Add(new ResourceBank.Resource()
		        {
			        path = $"Prefabs/Map/MapScenery/Christmas/{prefabName}",  // Prefabs/Map/MapScenery/Char/TreeA
			        asset = prefab,
		        });
	        }
        }
        
        [HarmonyPatch(typeof (ResourceBank), "Awake", new System.Type[] {})]
        public class ResourceBank_Awake
        {
	        public static void Postfix()
	        {
		        if (presentDecoratioAssetBundle != null)
		        {
			        LoadPresentsIntoResourceBank();
		        }
	        }
        }
    }
}
