using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Localizing;
using InscryptionAPI.Totems;
using UnityEngine;

namespace MoreTotemBottoms
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
	    public const string PluginGuid = "jamesgames.inscryption.moretotembottoms";
	    public const string PluginName = "More Totem Bottoms";
	    public const string PluginVersion = "0.1.0.0";

	    public static string PluginDirectory;
	    public static ManualLogSource Log;

        private void Awake()
        {
	        Log = Logger;
	        Log.LogInfo($"Loading {PluginName}...");
            PluginDirectory = this.Info.Location.Replace("MoreTotemBottoms.dll", "");

            new Harmony(PluginGuid).PatchAll();
            //CustomTotemBottoms();

            var font = LocalizationManager.GetFontReplacementForFont(LocalizationManager.FontReplacementType.DaggerSquare);

            List<FontReplacement> fonts = new List<FontReplacement>();
            foreach (LocalizationManager.FontReplacementType fontReplacementType in Enum.GetValues(typeof(LocalizationManager.FontReplacementType)))
            {
	            fonts.Add(LocalizationManager.GetFontReplacementForFont(fontReplacementType, font.replacementFont, font.replacementTMPFont));
            }
            
            string stringtablepath = Path.Combine(PluginDirectory, "stringtable.csv");
            LocalizationManager.NewLanguage(PluginGuid, "Derk", "Derk", "Reset With Polish", stringtablepath, fonts);


            Logger.LogInfo($"Loaded {PluginName}!");	        
        }

        // private void CustomTotemBottoms()
        // {
	       // byte[] resourceBytes = TextureHelper.GetResourceBytes("totembottom_doublesigil", typeof(Plugin).Assembly);
	       // if (!AssetBundleHelper.TryGet(resourceBytes, "TotemBottomDoubleSigil", out GameObject go))
	       // {
		      //  Logger.LogInfo($"Could not load asset bundle!");
		      //  return;
	       // }
	       //
	       // DoubleSigilEffect.ID = TotemManager.NewBottomPiece<DoubleSigilTrigger, DoubleSigilEffect>(PluginGuid, "healthbottom",
			     //   "Adds 2 health to the drawn card", go)
		      //  .SetCompositeTotemPieceType(typeof(DoubleSigilComponentPiece)).EffectID;
        // }
    }
}
