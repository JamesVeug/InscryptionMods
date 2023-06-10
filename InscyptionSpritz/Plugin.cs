using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpritzMod.Scripts;
using SpritzMod.Scripts.Data;

namespace SpritzMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
	    public const string PluginGuid = "jamesgames.inscryption.spritzmod";
	    public const string PluginName = "Spritz Mod";
	    public const string PluginVersion = "0.1.0.0";

        public static string Directory;
        public static ManualLogSource Log;

        private void Awake()
        {
	        Log = Logger;
            Logger.LogInfo($"Loading {PluginName}...");
            Directory = this.Info.Location.Replace("SpritzMod.dll", "");
            new Harmony(PluginGuid).PatchAll();

            AnglerNodeSequencer.Initialize();

            Logger.LogInfo($"Loaded {PluginName}!");
        }
    }
}
