using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Pelts;
using InscryptionAPI.Pelts.Extensions;
using InscryptionAPI.Resource;
using Sirenix.Utilities;
using UnityEngine;

namespace MorePeltsMod.Scripts;

public class Pelts
{
	public static CardInfo rottenCardInfo;
	
	private static CardInfo CreateCard(string displayName, string imagePath, int attack, int health)
    {
        CardInfo info = CardManager.New(Plugin.PluginGuid, displayName, displayName, attack, health);
        info.SetPortrait(TextureHelper.GetImageAsTexture(Path.Combine(Plugin.PluginDirectory, imagePath)));
        info.cardComplexity = CardComplexity.Simple;
        info.AddTraits(Trait.Pelt);
        info.temple = CardTemple.Nature;
        info.AddSpecialAbilities(SpecialTriggeredAbility.SpawnLice);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground, CardAppearanceBehaviour.Appearance.TerrainLayout);

        return info;
    }
    
    private static CardInfo CreateRareCard(string displayName, string imagePath, int attack, int health)
    {
        CardInfo info = CreateCard(displayName, imagePath, attack, health);
        info.AddAppearances(CardAppearanceBehaviour.Appearance.GoldEmission);

        return info;
    }

    public static void EvolvePelt()
    {
        CardInfo info = CreateCard("Gecko Pelt", "Art/portrait_pelt_gecko.png", 0, 2);

        PeltManager.New(Plugin.PluginGuid, info, 3, 0, 8,
	        () =>
	        {
		        return CardManager.AllCardsCopy.FindAll((a) =>
			        (a.HasAbility(Ability.Evolve) || a.HasAbility(Ability.Transformer)) &&
			        a.temple == CardTemple.Nature &&
			        !a.traits.Contains(Trait.Pelt) &&
			        !a.metaCategories.Contains(CardMetaCategory.Rare));
	        }
        );
    }

    public static void BearPelt()
    {
        CardInfo info = CreateCard("Bear Pelt", "Art/portrait_skin_damage.png", 0, 3);

        PeltManager.New(Plugin.PluginGuid, info, 6, 0, 8,
	        () =>
	        {
		        return CardManager.AllCardsCopy.FindAll((a) =>
			        (a.BloodCost > 2 || a.bonesCost > 4 || a.EnergyCost > 3 || a.GemsCost.Count > 1) &&
			        a.temple == CardTemple.Nature &&
			        !a.traits.Contains(Trait.Pelt) &&
			        !a.metaCategories.Contains(CardMetaCategory.Rare) &&
			        a.metaCategories.Contains(CardMetaCategory.TraderOffer));
	        }
        );
    }

    public static void BuffaloPelt()
    {
	    CardInfo info = CreateCard("Buffalo Pelt", "Art/portrait_pelt_buffalo.png", 0, 3);

	    PeltManager.New(Plugin.PluginGuid, info, 6, 0, 8,
		    () =>
		    {
			    return CardManager.AllCardsCopy.FindAll((a) =>
				    a.HasAnyOfAbilities(Ability.Sacrificial, Ability.TripleBlood, Ability.BoneDigger, 
					    Ability.QuadrupleBones, Ability.GainBattery, Ability.Morsel, Ability.RandomConsumable,
					    Ability.GainGemBlue, Ability.GainGemGreen, Ability.GainGemOrange, Ability.GainGemTriple) &&
				    a.temple == CardTemple.Nature &&
				    !a.traits.Contains(Trait.Pelt) &&
				    !a.metaCategories.Contains(CardMetaCategory.Rare) &&
				    a.metaCategories.Contains(CardMetaCategory.TraderOffer));
		    }
	    );
    }

    public static void SubmergePelt()
    {
        CardInfo info = CreateCard("Fish Pelt", "Art/portrait_pelt_fish.png", 0, 2);

        PeltManager.New(Plugin.PluginGuid, info, 4, 0, 8,
	        () =>
	        {
		        return CardManager.AllCardsCopy.FindAll((a) =>
			        (a.HasAbility(Ability.Submerge) || a.HasAbility(Ability.SubmergeSquid)) &&
                     a.temple == CardTemple.Nature &&
                     !a.traits.Contains(Trait.Pelt) &&
                     !a.metaCategories.Contains(CardMetaCategory.Rare));
	        }
        );
    }

    public static void TerrainPelt()
    {
        CardInfo info = CreateCard("Beaver Pelt", "Art/portrait_skin_terrain.png", 0, 2);

        PeltManager.New(Plugin.PluginGuid, info, 4, 0, 8,
	        () =>
	        {
		        return CardManager.AllCardsCopy.FindAll((a) =>
			        a.traits.Contains(Trait.Terrain) &&
			        a.temple == CardTemple.Nature &&
			        !a.traits.Contains(Trait.Pelt) &&
			        !a.metaCategories.Contains(CardMetaCategory.Rare));
	        }
        );
    }

    private static bool GainsGems(CardInfo cardInfo)
    {
        return cardInfo.HasAnyOfAbilities(Ability.GainGemBlue, Ability.GainGemGreen,
	               Ability.GainGemOrange, Ability.GainGemTriple);
    }

    private static bool IsWizard(CardInfo cardInfo)
    {
        return GainsGems(cardInfo) || cardInfo.gemsCost.Count > 0 || cardInfo.HasTrait(Trait.Gem) ||
               cardInfo.HasAnyOfAbilities(Ability.GemDependant, Ability.GemsDraw, Ability.BuffGems,
	               Ability.ExplodeGems, Ability.ShieldGems, Ability.ConduitSpawnGems);
    }
    
    public static void MoxPelt()
    {
        CardInfo info = CreateCard("Mox Scroll", "Art/portrait_pelt_mox.png", 0, 1);

        PeltManager.New(Plugin.PluginGuid, info, 3, 0, 8,
	        () =>
	        {
		        return CardManager.AllCardsCopy.FindAll((a) =>
			        GainsGems(a)  &&
			        a.temple == CardTemple.Nature &&
			        !a.traits.Contains(Trait.Pelt) &&
			        !a.metaCategories.Contains(CardMetaCategory.Rare));
	        }
        );
    }

    public static void WizardPelt()
    {
        CardInfo info = CreateCard("Wizard Pelt", "Art/portrait_skin_mage.png", 0, 2);

        PeltManager.New(Plugin.PluginGuid, info, 3, 0, 8,
	        () =>
	        {
		        return CardManager.AllCardsCopy.FindAll((a) =>
			        IsWizard(a) &&
			        a.temple == CardTemple.Nature &&
			        !a.traits.Contains(Trait.Pelt) &&
			        !a.metaCategories.Contains(CardMetaCategory.Rare));
	        }
        );
    }

    public static void BonePelt()
    {
        CardInfo info = CreateCard("Bone Pelt", "Art/portrait_skin_bone.png", 0, 2);

        PeltManager.New(Plugin.PluginGuid, info, 3, 0, 8,
	        () =>
	        {
		        return CardManager.AllCardsCopy.FindAll((a) =>
			        a.BonesCost > 0 &&
			        a.temple == CardTemple.Nature &&
			        !a.traits.Contains(Trait.Pelt) &&
			        !a.metaCategories.Contains(CardMetaCategory.Rare));
	        }
        );
    }

    public static void LightPelt()
    {
        CardInfo info = CreateCard("Light Pelt", "Art/portrait_skin_shed.png", 0, 1);

        PeltManager.New(Plugin.PluginGuid, info, 5, 0, 8, 
	        () => 
	        {
		        return CardManager.AllCardsCopy.FindAll((a) =>
			        a.bonesCost == 0 && a.BloodCost == 0 && a.gemsCost.IsNullOrEmpty() && a.energyCost == 0 &&
			        !a.traits.Contains(Trait.Pelt) &&
			        a.temple == CardTemple.Nature &&
			        !a.metaCategories.Contains(CardMetaCategory.Rare));
			});
    }

    public static void AirPelt()
    {
        CardInfo info = CreateCard("Air Pelt", "Art/portrait_skin_airborn_alt1.png", 0, 2);

        PeltManager.New(Plugin.PluginGuid, info, 4, 0, 8,
	        () =>
	        {
		        return CardManager.AllCardsCopy.FindAll((a) =>
			        a.HasAnyOfAbilities(Ability.Reach, Ability.Flying) &&
			        !a.traits.Contains(Trait.Pelt) &&
			        a.temple == CardTemple.Nature &&
			        !a.metaCategories.Contains(CardMetaCategory.Rare));
	        }
        );
    }

    private static bool IsEnergy(CardInfo a)
    {
	    return a.EnergyCost > 0 || a.HasAnyOfAbilities(Ability.ConduitEnergy, Ability.ActivatedRandomPowerEnergy, Ability.ActivatedEnergyToBones, Ability.ActivatedStatsUpEnergy);
    }

    public static void EnergyPelt()
    {
	    CardInfo info = CreateCard("Battery", "Art/portrait_pelt_energy.png", 0, 2);

	    PeltManager.New(Plugin.PluginGuid, info, 4, 0, 8,
		    () =>
		    {
			    return CardManager.AllCardsCopy.FindAll((a) =>
				    IsEnergy(a) && 
				    !a.traits.Contains(Trait.Pelt) &&
				    a.temple == CardTemple.Nature &&
				    !a.metaCategories.Contains(CardMetaCategory.Rare));
		    }
	    );
    }

    public static void RareEnergyPelt()
    {
	    CardInfo info = CreateRareCard("Super Battery", "Art/portrait_pelt_energy_rare.png", 0, 3);
	    Sprite sprite = TextureHelper.GetImageAsSprite(Path.Combine(Plugin.PluginDirectory, "Art/portrait_pelt_hare_emit.png"), TextureHelper.SpriteType.CardPortrait);
	    info.SetEmissivePortrait(sprite);

	    PeltManager.New(Plugin.PluginGuid, info, 7, 1, 4,
		    () =>
		    {
			    return CardManager.AllCardsCopy.FindAll((a) =>
				    IsEnergy(a) && 
				    !a.traits.Contains(Trait.Pelt) &&
				    a.temple == CardTemple.Nature &&
				    a.metaCategories.Contains(CardMetaCategory.Rare));
		    }
	    );
    }

    public static void SuperPelt()
    {
        CardInfo info = CreateRareCard("Super Pelt", "Art/portrait_skin.png", 0, 5);
        Sprite sprite = TextureHelper.GetImageAsSprite(Path.Combine(Plugin.PluginDirectory, "Art/portrait_skin_super_alpha.png"), TextureHelper.SpriteType.CardPortrait);
        info.SetEmissivePortrait(sprite);

        PeltManager.New(Plugin.PluginGuid, info, 12, 2, 4,
	        () =>
	        {
		        return CardManager.AllCardsCopy.FindAll((a) =>
			        a.metaCategories.Contains(CardMetaCategory.Rare) &&
			        a.temple == CardTemple.Nature);
	        }
        );
    }

    public static void RottenPelts()
    {
	    CardAppearanceBehaviour.Appearance peltID = RottenPeltBackground.Initialize();
	    
	    ResourceBank.Resource resource = new ResourceBank.Resource();
	    resource.path = "Art/Cards/Decals/RottenPeltDecal";
	    resource.asset = TextureHelper.GetImageAsTexture(Path.Combine(Plugin.PluginDirectory, "Art/decal_mold.png"));
	    ResourceBankManager.Add(Plugin.PluginGuid, resource);

	    RottenHarePelt(resource, peltID);
	    RottenWolfPelt(resource, peltID);
	    RottenGoldenPelt(resource, peltID);
    }
    
    public static void RottenHarePelt(ResourceBank.Resource resource, CardAppearanceBehaviour.Appearance peltID)
    {
	    rottenCardInfo = CreateCard("Rotten Pelt", "Art/portrait_pelt_rotten.png", 0, 1);
        rottenCardInfo.SetPixelPortrait(TextureHelper.GetImageAsTexture(Path.Combine(Plugin.PluginDirectory, "Art/pixel_rotten_pelt.png")));
        rottenCardInfo.AddAppearances(peltID);
        rottenCardInfo.AddDecal((Texture)resource.asset);


        PeltManager.New(Plugin.PluginGuid, rottenCardInfo, 3, 0, 8,
	        () =>
	        {
		        return CardLoader.GetUnlockedCards(CardMetaCategory.TraderOffer, CardTemple.Nature);
	        }
        ).SetModifyCardChoiceAtTrader(ModifyCardChoiceAtTrader);
    }

    public static void RottenWolfPelt(ResourceBank.Resource resource, CardAppearanceBehaviour.Appearance peltID)
    {
	    CardInfo cardInfo = CreateCard("Rotten Wolf Pelt", "Art/portrait_pelt_rotten_wolf.png", 0, 2);
	    cardInfo.AddAppearances(peltID);
	    cardInfo.AddDecal((Texture)resource.asset);


	    PeltManager.New(Plugin.PluginGuid, cardInfo, 4, 1, 8,
		    () =>
		    {
			    return CardLoader.GetUnlockedCards(CardMetaCategory.TraderOffer, CardTemple.Nature);
		    }
	    ).SetModifyCardChoiceAtTrader(ModifyCardChoiceAtTrader);
    }

    public static void RottenGoldenPelt(ResourceBank.Resource resource, CardAppearanceBehaviour.Appearance peltID)
    {
	    CardInfo cardInfo = CreateRareCard("Rotten Golden Pelt", "Art/portrait_pelt_golden_rotten.png", 0, 3);
	    Sprite sprite = TextureHelper.GetImageAsSprite(Path.Combine(Plugin.PluginDirectory, "Art/portrait_pelt_golden_rotten_emit.png"), TextureHelper.SpriteType.CardPortrait);
	    cardInfo.SetEmissivePortrait(sprite);
	    
	    cardInfo.AddAppearances(peltID);
	    cardInfo.AddDecal((Texture)resource.asset);


	    PeltManager.New(Plugin.PluginGuid, cardInfo, 6, 1, 4,
		    () =>
		    {
			    return CardLoader.GetUnlockedCards(CardMetaCategory.Rare, CardTemple.Nature);
		    }
	    ).SetModifyCardChoiceAtTrader(ModifyCardChoiceAtTrader);
    }

    private static void ModifyCardChoiceAtTrader(CardInfo traderCardInfo)
    {
        // Show decal
        CardModificationInfo mod = new CardModificationInfo();
        mod.decalIds = new List<string>();
        mod.decalIds.Add("RottenPeltDecal");

        // Reduce stats
        if (traderCardInfo.Attack > 0)
        {
	        if (traderCardInfo.Attack == 1)
	        {
		        mod.attackAdjustment = -1;
	        }
	        else
	        {
		        int maxAdjustment = Mathf.FloorToInt(traderCardInfo.Attack / 2.0f);
		        mod.attackAdjustment = -maxAdjustment;
	        }
        }

        if (traderCardInfo.Health > 1)
        {
	        int maxAdjustment = Mathf.FloorToInt(traderCardInfo.Health / 2.0f);
	        mod.healthAdjustment = -maxAdjustment;
        }

        if (traderCardInfo.bonesCost > 1)
        {
	        int maxBoneChange = Mathf.FloorToInt(traderCardInfo.bonesCost / 2.0f);
	        mod.bonesCostAdjustment = -maxBoneChange;
        }
        else if (traderCardInfo.bonesCost > 1)
        {
	        mod.bonesCostAdjustment = -traderCardInfo.bonesCost;
        }

        if (traderCardInfo.BloodCost > 0)
        {
	        if (traderCardInfo.BloodCost == 1)
	        {
		        mod.bonesCostAdjustment = 2;
	        }

	        mod.bloodCostAdjustment = -1;
        }

        if (traderCardInfo.EnergyCost > 1)
        {
	        int maxAdjustment = Mathf.FloorToInt(traderCardInfo.EnergyCost / 2.0f);
	        mod.energyCostAdjustment = -maxAdjustment;
        }

        traderCardInfo.Mods.Add(mod);
    }
}