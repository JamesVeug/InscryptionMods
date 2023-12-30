using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Saves;
using UnityEngine;

[HarmonyPatch]
public static class TemporaryCardInfo
{
    public static bool IsTemporaryCardInfo(this CardInfo cardInfo)
    {
        return cardInfo.Mods != null && 
               cardInfo.Mods.Any(a => !string.IsNullOrEmpty(a.singletonId) && a.singletonId.StartsWith("temporary_card"));
    }

    public static CardInfo CloneAsTemporaryCard(this CardInfo templateCardInfo)
    {
        int value = ModdedSaveManager.RunState.GetValueAsInt(Plugin.PluginGuid, "TotalTempCards") + 1;
        ModdedSaveManager.RunState.SetValue(Plugin.PluginGuid, "TotalTempCards", value);

        CardInfo clone = ScriptableObject.CreateInstance<CardInfo>();
        clone.name = $"{templateCardInfo.name}_temp_{value}";
        clone.abilities = new List<Ability>(templateCardInfo.abilities);
        clone.specialAbilities = new List<SpecialTriggeredAbility>(templateCardInfo.specialAbilities);
        clone.tribes = new List<Tribe>(templateCardInfo.tribes);
        clone.gemsCost = new List<GemType>(templateCardInfo.gemsCost);
        clone.appearanceBehaviour = new List<CardAppearanceBehaviour.Appearance>(templateCardInfo.appearanceBehaviour);
        clone.description = templateCardInfo.description;
        clone.baseAttack = templateCardInfo.baseAttack; 
        clone.baseHealth = templateCardInfo.baseHealth;
        clone.cost = templateCardInfo.cost;
        clone.bonesCost = templateCardInfo.bonesCost;
        clone.energyCost = templateCardInfo.energyCost;
        clone.boon = templateCardInfo.boon;
        clone.onePerDeck = templateCardInfo.onePerDeck;
        clone.temple = templateCardInfo.temple;
        clone.displayedName = templateCardInfo.displayedName;
        clone.displayedNameLocId = templateCardInfo.displayedNameLocId;
        clone.hideAttackAndHealth = templateCardInfo.hideAttackAndHealth;
        clone.specialStatIcon = templateCardInfo.specialStatIcon;
        clone.titleGraphic = templateCardInfo.titleGraphic;
        clone.portraitTex = templateCardInfo.portraitTex;
        clone.alternatePortrait = templateCardInfo.alternatePortrait;
        clone.holoPortraitPrefab = templateCardInfo.holoPortraitPrefab;
        clone.animatedPortrait = templateCardInfo.animatedPortrait;
        clone.pixelPortrait = templateCardInfo.pixelPortrait;
        clone.defaultEvolutionName = templateCardInfo.defaultEvolutionName;
        clone.flipPortraitForStrafe = templateCardInfo.flipPortraitForStrafe;
        clone.cardComplexity = templateCardInfo.cardComplexity;
        clone.decals = new List<Texture>(templateCardInfo.decals);
        clone.metaCategories = new List<CardMetaCategory>(templateCardInfo.metaCategories);
        clone.traits = new List<Trait>(templateCardInfo.traits);
        clone.ascensionAbilities = new List<Ability>(templateCardInfo.ascensionAbilities);
        clone.temporaryDecals = new List<Texture>(templateCardInfo.temporaryDecals);
        clone.get_decals = new List<Texture>(templateCardInfo.get_decals);
        foreach (KeyValuePair<string, string> pair in templateCardInfo.GetCardExtensionTable())
        {
            clone.SetExtendedProperty(pair.Key, pair.Value);
        }

        if (clone.evolveParams != null)
        {
            clone.evolveParams = new EvolveParams();
            clone.evolveParams.turnsToEvolve = templateCardInfo.evolveParams.turnsToEvolve;
            clone.evolveParams.evolution = templateCardInfo.evolveParams.evolution;
        }
        
        if (clone.tailParams != null)
        {
            clone.tailParams = new TailParams();
            clone.tailParams.tail = templateCardInfo.tailParams.tail;
            clone.tailParams.tailLostPortrait = templateCardInfo.tailParams.tailLostPortrait;
        }
        
        if (clone.iceCubeParams != null)
        {
            clone.iceCubeParams = new IceCubeParams();
            clone.iceCubeParams.creatureWithin = templateCardInfo.iceCubeParams.creatureWithin;
        }
        
        clone.Mods = new List<CardModificationInfo>(templateCardInfo.Mods.Select(a=>a.Clone()).Cast<CardModificationInfo>());
        if (!templateCardInfo.IsTemporaryCardInfo())
        {
            clone.Mods.Add(new CardModificationInfo()
            {
                singletonId = "temporary_card|" + templateCardInfo.name
            });
        }
        
        return clone;
    }
    
    public static void SaveTemporaryCard(CardInfo cardInfo)
    {
        string json = JsonUtility.ToJson(cardInfo);
        ModdedSaveManager.RunState.SetValue(Plugin.PluginGuid, cardInfo.name, json);
    }
    
    public static CardInfo GetTemporaryCard(string cardInfoName)
    {
        string s = ModdedSaveManager.RunState.GetValue(Plugin.PluginGuid, cardInfoName);
        if (!string.IsNullOrEmpty(s))
        {
            CardInfo cardInfo = ScriptableObject.CreateInstance<CardInfo>();
            JsonUtility.FromJsonOverwrite(s, cardInfo);

            cardInfo.name = cardInfoName; // name doesn't translate over
            return cardInfo;
        }

        return null;
    }

    [HarmonyPatch(typeof(CardManager), "LogCardInfo", new Type[] { typeof(CardInfo), typeof(string) })]
    [HarmonyPrefix]
    public static bool GetTemporaryCard(ref CardInfo info, ref string cardInfoName)
    {
        if (info == null)
        {
            info = GetTemporaryCard(cardInfoName);
        }

        return true;
    }

    // [HarmonyPatch(typeof(DeckInfo), "LoadCards", new Type[] { })]
    // [HarmonyPostfix]
    // public static void DeckInfo_LoadCards(DeckInfo __instance)
    // {
    //     Plugin.Log.LogInfo($"[DeckInfo_LoadCards]");
    //     List<CardInfo> moddedCards = new List<CardInfo>();
    //     foreach (KeyValuePair<string, List<CardModificationInfo>> cardIdModInfo in __instance.cardIdModInfos)
    //     {
    //         string entryName = cardIdModInfo.Key;
    //         Plugin.Log.LogInfo($"[DeckInfo_LoadCards] " + entryName);
    //         if (entryName.Contains("#"))
    //         {
    //             entryName = entryName.Remove(cardIdModInfo.Key.IndexOf("#"[0]));
    //         }
    //         
    //         CardInfo cardInfo = __instance.Cards.Find((CardInfo x) => !moddedCards.Contains(x) && entryName == x.name);
    //         if (cardInfo != null)
    //         {
    //             Plugin.Log.LogInfo($"[DeckInfo_LoadCards] Found " + entryName);
    //             foreach (CardModificationInfo info in cardIdModInfo.Value)
    //             {
    //                 Plugin.Log.LogInfo($"\t[DeckInfo_LoadCards] Mod " + info.attackAdjustment + " " + info.healthAdjustment + " " + info.abilities.Count);
    //             }
    //             //cardInfo.Mods.AddRange(cardIdModInfo.Value);
    //             moddedCards.Add(cardInfo);
    //         }
    //         else
    //         {
    //             Plugin.Log.LogError($"[DeckInfo_LoadCards] Could not find card " + entryName);
    //         }
    //     }
    // }
}