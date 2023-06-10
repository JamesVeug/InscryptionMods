using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
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
            CustomTotemBottoms();


            Logger.LogInfo($"Loaded {PluginName}!");	        
        }

        private void CustomTotemBottoms()
        {
	       // Texture2D boneTexture = TextureHelper.GetImageAsTexture(Path.Combine(PluginDirectory, "Art/bones_bottom.png"));
	       // TotemManager.NewBottomPiece<GainBonesTrigger>(PluginGuid, "boneybottom", "Gives you 4 bones when a card is drawn", boneTexture);
	       //
	       // Texture2D duplicateTexture = TextureHelper.GetImageAsTexture(Path.Combine(PluginDirectory, "Art/drawcopy_bottom.png"));
	       // TotemManager.NewBottomPiece<DuplicateTrigger>(PluginGuid, "copybottom", "Gives you a copy of the card when drawn", duplicateTexture);
	       //
	       // Texture2D powerTexture = TextureHelper.GetImageAsTexture(Path.Combine(PluginDirectory, "Art/power_bottom.png"));
	       // TotemManager.NewBottomPiece<PowerBuffStatEffect>(PluginGuid, "powerbottom", "Adds 1 power to the drawn card", powerTexture);
		      //  
	       // Texture2D healthTexture = TextureHelper.GetImageAsTexture(Path.Combine(PluginDirectory, "Art/health_bottom.png"));
	       // TotemManager.NewBottomPiece<HealthBuffStatEffect>(PluginGuid, "healthbottom", "Adds 2 health to the drawn card", healthTexture);
		       
	       byte[] resourceBytes = TextureHelper.GetResourceBytes("totembottom_doublesigil", typeof(Plugin).Assembly);
	       if (!AssetBundleHelper.TryGet(resourceBytes, "TotemBottomDoubleSigil", out GameObject go))
	       {
		       Logger.LogInfo($"Could not load asset bundle!");
		       return;
	       }
	       
	       DoubleSigilEffect.ID = TotemManager.NewBottomPiece<DoubleSigilTrigger, DoubleSigilEffect>(PluginGuid, "healthbottom",
			       "Adds 2 health to the drawn card", go)
		       .SetCompositeTotemPieceType(typeof(DoubleSigilComponentPiece)).EffectID;
        }
    }
    
    /*[HarmonyPatch(typeof(CardExtensions), nameof(CardExtensions.BloodCost))]
    internal static class CardExtensions_BloodCost
    {
	    public static void Postfix(PlayableCard card, ref int __result)
	    {
		    int enumerable = BoardManager.Instance.playerSlots.Count((a) => a.Card != null);
		    if (enumerable > 0)
		    {
			    __result = 4;
		    }
	    }
    }
    
    [HarmonyPatch(typeof(CardExtensions), nameof(CardExtensions.BonesCost))]
    internal static class CardExtensions_BonesCost
    {
	    public static void Postfix(PlayableCard card, ref int __result)
	    {
		    int enumerable = BoardManager.Instance.playerSlots.Count((a) => a.Card != null);
		    if (enumerable > 0)
		    {
			    __result = 8;
		    }
	    }
    }
    
    [HarmonyPatch(typeof(CardExtensions), nameof(CardExtensions.GemsCost))]
    internal static class CardExtensions_GemsCost
    {
	    public static void Postfix(PlayableCard card, ref List<GemType> __result)
	    {
		    int enumerable = BoardManager.Instance.playerSlots.Count((a) => a.Card != null);
		    if (enumerable > 0)
		    {
			    __result = new List<GemType>()
			    {
				    GemType.Blue,
				    GemType.Orange
			    };
		    }
	    }
    }
    
    [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.EnergyCost), MethodType.Getter)]
    internal static class CardExtensions_EnergyCost
    {
	    public static void Postfix(ref int __result)
	    {
		    int enumerable = BoardManager.Instance.playerSlots.Count((a) => a.Card != null);
		    if (enumerable > 0)
		    {
			    __result = 2;
		    }
	    }
    }*/
}
