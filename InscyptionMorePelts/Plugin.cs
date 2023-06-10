using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Ascension;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace MorePeltsMod.Scripts
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
	    public const string PluginGuid = "jamesgames.inscryption.morepelts";
	    public const string PluginName = "More Pelts";
	    public const string PluginVersion = "0.3.0.0";

	    public static string PluginDirectory;
	    public static ManualLogSource Log;

	    public static ChallengeManager.FullChallenge PeltsOnlyChallengeID;

        private void Awake()
        {
	        Log = Logger;
	        Log.LogInfo($"Loading {PluginName}...");
	        PluginDirectory = this.Info.Location.Replace("MorePelts.dll", "");
	        
	        new Harmony(PluginGuid).PatchAll();
	        
	        Pelts.BonePelt();
	        Pelts.LightPelt();
	        Pelts.AirPelt();
	        Pelts.SuperPelt();
	        Pelts.EnergyPelt();
	        Pelts.RareEnergyPelt();
	        Pelts.EvolvePelt();
	        Pelts.BearPelt();
	        Pelts.BuffaloPelt();
	        Pelts.SubmergePelt();
	        Pelts.TerrainPelt();
	        Pelts.MoxPelt();
	        Pelts.WizardPelt();
	        Pelts.RottenPelts();

	        RottenPeltStarterDeck();
	        PeltsOnlyChallenge();
            
            Logger.LogInfo($"Loaded {PluginName}!");	        
        }

        private void RottenPeltStarterDeck()
        {
	        string path = Path.Combine(PluginDirectory, "Art/RottenPeltDeck.png");
	        StarterDeckManager.New(PluginGuid, "Rotten Deck", path, new string[]
	        {
		        Pelts.rottenCardInfo.name,
		        Pelts.rottenCardInfo.name,
		        Pelts.rottenCardInfo.name,
		        Pelts.rottenCardInfo.name,
	        });
        }

        private void PeltsOnlyChallenge()
        {
	        AscensionChallengeInfo ascensionChallengeInfo = ScriptableObject.CreateInstance<AscensionChallengeInfo>();
	        ascensionChallengeInfo.title = "Pelts Only";
	        ascensionChallengeInfo.description = "Only way to gain new cards for your deck is through the trader or defeating bosses.";
	        ascensionChallengeInfo.pointValue = 20;
	        ascensionChallengeInfo.iconSprite = TextureHelper.GetImageAsSprite(Path.Combine(Plugin.PluginDirectory, "Art/peltsonly.png"), TextureHelper.SpriteType.ChallengeIcon);
	        ascensionChallengeInfo.activatedSprite = TextureHelper.GetImageAsSprite(Path.Combine(Plugin.PluginDirectory, "Art/peltsonly_activated.png"), TextureHelper.SpriteType.ChallengeIcon);

	        PeltsOnlyChallengeID = ChallengeManager.Add(PluginGuid, ascensionChallengeInfo);
	        Plugin.Log.LogInfo("PeltsOnlyChallengeID " + (int)PeltsOnlyChallengeID.Challenge.challengeType);
        }
    }
}
