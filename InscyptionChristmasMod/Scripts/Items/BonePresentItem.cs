using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreItemsMod.Scripts.Items;

public class BonePresentItem : BasePresentItem
{
	public static void Initialize()
	{
		ConsumableItemResource consumableItemResource = new ConsumableItemResource();
		consumableItemResource.FromPrefab(Plugin.itemAssetBundle.LoadAsset<GameObject>("BoneDeck"));

		ConsumableItemManager.ModelType modelType = ConsumableItemManager.RegisterPrefab(Plugin.PluginGuid, "BoneDeck", consumableItemResource);

		ConsumableItemManager.New(Plugin.PluginGuid, "Bone Present",
			"A present in the shape of a card deck in bone wrapping paper. Maybe it has bone cards in it?",
			TextureHelper.GetImageAsTexture(Path.Combine(Plugin.Directory, "Artwork/Items/present.png"), FilterMode.Point),
			typeof(BonePresentItem), modelType);
	}

	public override List<CardInfo> CardInfos()
	{
		List<CardInfo> cards = CardManager.AllCardsCopy.FindAll((a) =>
			a.BonesCost > 0 && 
			a.temple == CardTemple.Nature && 
			!a.traits.Contains(Trait.Pelt) &&
			(a.metaCategories.Contains(CardMetaCategory.ChoiceNode) || a.metaCategories.Contains(CardMetaCategory.TraderOffer)) &&
			!a.metaCategories.Contains(CardMetaCategory.Rare));
		if (cards.Count == 0)
		{
			cards.Add(CardManager.AllCardsCopy.Find((a) => a.name == "Amalgam"));
		}

		return cards;
	}
}