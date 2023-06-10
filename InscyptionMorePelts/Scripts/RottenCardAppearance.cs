using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace MorePeltsMod.Scripts
{
	public class RottenCardBackground : CardAppearanceBehaviour
	{
		public static Appearance CustomAppearance;
		
		private static Texture2D decal;

		public static Appearance Initialize()
		{
			decal = TextureHelper.GetImageAsTexture(Path.Combine(Plugin.PluginDirectory, "Art/decal_mold.png"));
			decal.name = "molddecal";
			
			var newBackgroundBehaviour =
				CardAppearanceBehaviourManager.Add(Plugin.PluginGuid, "RottenBackground", typeof(RottenCardBackground));
			CustomAppearance = newBackgroundBehaviour.Id;
			return CustomAppearance;
		}

		public override void ApplyAppearance()
		{
			if (!Card.Info.TempDecals.Find((a) => a.name == "molddecal"))
			{
				Card.Info.TempDecals.Add(decal);
			}
		}
	}
}