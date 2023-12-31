using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
	public const string PluginGuid = "jamesgames.inscryption.goobertextended";
	public const string PluginName = "Goobert Extended";
	public const string PluginVersion = "1.3.0";

	public static Plugin Instance;

	public static ManualLogSource Log;

	private void Awake()
	{
		Logger.LogInfo($"Loading {PluginName}...");
		Instance = this;
		Log = Logger;

		new Harmony(PluginGuid).PatchAll();
		
		Logger.LogInfo($"Loaded {PluginName}!");
	}
}
