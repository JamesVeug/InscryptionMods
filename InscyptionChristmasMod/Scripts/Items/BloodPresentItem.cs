using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Items;
using UnityEngine;

namespace MoreItemsMod.Scripts.Items;

public class BloodPresentItem : BasePresentItem
{
	public static void Initialize()
	{
		ConsumableItemResource consumableItemResource = new ConsumableItemResource();
		consumableItemResource.FromPrefab(Plugin.itemAssetBundle.LoadAsset<GameObject>("BloodDeck"));

		ConsumableItemManager.ModelType modelType = ConsumableItemManager.RegisterPrefab(Plugin.PluginGuid, "BloodDeck", consumableItemResource);

		ConsumableItemManager.New(Plugin.PluginGuid, "Blood Present",
			"A present in the shape of a card deck in blood wrapping paper. Maybe it has bone cards in it?",
			TextureHelper.GetImageAsTexture(Path.Combine(Plugin.Directory, "Artwork/Items/present.png"), FilterMode.Point),
			typeof(BloodPresentItem), modelType);
	}

	public override List<CardInfo> CardInfos()
	{
		List<CardInfo> cards = CardManager.AllCardsCopy.FindAll((a) =>
			a.BloodCost > 0 && 
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