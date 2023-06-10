using BepInEx;
using BepInEx.Logging;
using InscryptionAPI.Helpers;
using InscryptionAPI.Totems;
using UnityEngine;

namespace PumpkinTotemTopMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
	    public const string PluginGuid = "jamesgames.inscryption.pumpkintotemtop";
	    public const string PluginName = "Pumpkin Totem Top Mod";
	    public const string PluginVersion = "1.0.3.0";

	    public static string PluginDirectory;
	    public static ManualLogSource Log;

        private void Awake()
        {
	        Log = Logger;
            Logger.LogInfo($"Loading {PluginName}...");
            PluginDirectory = this.Info.Location.Replace("PumpkinTotemTop.dll", "");
            
            // The resource bank has been cleared. refill it
            string path = Path.Combine(Plugin.PluginDirectory, "AssetBundles/pumpkintotemtop");
            if (AssetBundleHelper.TryGet(path, "PumpkinTotemTop", out GameObject prefab))
            {
	            TotemManager.SetDefaultTotemTop<CustomIconTotemTopPiece>(prefab);
            }
            
            Logger.LogInfo($"Loaded {PluginName}!");
        }
    }
}
