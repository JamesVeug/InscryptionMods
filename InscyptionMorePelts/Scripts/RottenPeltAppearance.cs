using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace MorePeltsMod.Scripts
{
	public class RottenPeltBackground : CardAppearanceBehaviour
	{
		public static Appearance CustomAppearance;
		
		private static Texture2D backgroundImage;

		public static Appearance Initialize()
		{
			backgroundImage = TextureHelper.GetImageAsTexture(Path.Combine(Plugin.PluginDirectory, "Art/rotten_appearance.png"));
			
			var newBackgroundBehaviour =
				CardAppearanceBehaviourManager.Add(Plugin.PluginGuid, "RottenPeltBackground", typeof(RottenPeltBackground));
			CustomAppearance = newBackgroundBehaviour.Id;
			return CustomAppearance;
		}

		public override void ApplyAppearance()
		{
			base.Card.RenderInfo.baseTextureOverride = backgroundImage;
		}
	}
}