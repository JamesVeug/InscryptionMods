using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using JamesGames.ReadmeMaker;
using JLPlugin.V2.Data;
using UnityEngine;

namespace JSONExporter
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "_jamesgames.inscryption.jsonexporter";
        public const string PluginName = "JSON Exporter";
        public const string PluginVersion = "1.0.0.0";

        private static string ExportDirectory => Path.Combine(PluginDirectory, "Exported");
        public static string PluginDirectory;
        public static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"Loading {PluginName}...");
            PluginDirectory = this.Info.Location.Replace("JSONExporter.dll", "");
            new Harmony(PluginGuid).PatchAll();
            
            string directory = ExportDirectory;
            if (Directory.Exists(directory))
            {
                Logger.LogInfo($"Deleting exported folder {directory}...");
                Directory.Delete(directory, true);
            }
        }

        private void Start()
        {
            ExportCardsToJSON();
        }
        
        private void ExportCardsToJSON()
        {
            string directory = ExportDirectory;
            Directory.CreateDirectory(directory);
            Logger.LogInfo($"Exporting cards to {directory}");
            foreach (CardInfo cardInfo in CardManager.AllCardsCopy)
            {
                if (string.IsNullOrEmpty(cardInfo.GetModPrefix()))
                {
                    continue;
                }

                CardSerializeInfo info = FromCardInfo(cardInfo);
                string fileName = ReplaceFullStops(info.modPrefix + "_" + info.displayedName);
                string path = Path.Combine(directory, fileName + ".jldr2");
                info.WriteToFile(path, true);
            }
        }

        private string ReplaceFullStops(string text)
        {
            if (text.IndexOf('.') >= 0)
            {
                text = text.Replace('.', '_');
            }

            return text;
        }

        public static CardSerializeInfo FromCardInfo(CardInfo card)
        {
            CardSerializeInfo serializeInfo = new CardSerializeInfo();

            if (card.decals != null && card.decals.Count > 0)
                serializeInfo.decals = card.decals.Select((a)=>TextureToPath.GetTexturePath(a) ?? "TODO decal").ToArray();
            
            serializeInfo.modPrefix = card.GetModPrefix();

            if (!string.IsNullOrEmpty(card.displayedName))
                serializeInfo.displayedName = card.displayedName;

            serializeInfo.name = card.name.Substring(serializeInfo.modPrefix.Length + 1);

            if (card.baseAttack > 0)
                serializeInfo.baseAttack = card.baseAttack;

            serializeInfo.baseHealth = card.baseHealth;

            if (card.BloodCost > 0)
                serializeInfo.bloodCost = card.BloodCost;

            if (card.bonesCost > 0)
                serializeInfo.bonesCost = card.bonesCost;

            if (card.energyCost > 0)
                serializeInfo.energyCost = card.energyCost;

            if (card.gemsCost != null)
                serializeInfo.gemsCost = card.gemsCost.Select(s => s.ToString()).ToArray();

            if (card.abilities != null && card.abilities.Count > 0)
                serializeInfo.abilities = card.abilities.Select(ParseAbility).ToArray();

            if (card.specialAbilities != null && card.specialAbilities.Count > 0)
                serializeInfo.specialAbilities = card.specialAbilities.Select(ParseSpecialAbilities).ToArray();

            if (card.specialStatIcon != SpecialStatIcon.None)
                serializeInfo.specialStatIcon = ParseSpecialStatIcon(card.specialStatIcon);

            if (card.metaCategories != null && card.metaCategories.Count > 0)
                serializeInfo.metaCategories = card.metaCategories.Select(s => s.ToString()).ToArray();

            serializeInfo.cardComplexity = card.cardComplexity.ToString();

            if (card.onePerDeck)
                serializeInfo.onePerDeck = card.onePerDeck;

            serializeInfo.temple = card.temple.ToString();

            if (card.titleGraphic != null)
                serializeInfo.titleGraphic = TextureToPath.GetTexturePath(card.titleGraphic) ?? "TODO titleGraphic";

            if (!string.IsNullOrEmpty(card.description))
                serializeInfo.description = card.description;

            if (card.hideAttackAndHealth)
                serializeInfo.hideAttackAndHealth = card.hideAttackAndHealth;

            if (card.appearanceBehaviour != null && card.appearanceBehaviour.Count > 0)
                serializeInfo.appearanceBehaviour = card.appearanceBehaviour.Select(s => s.ToString()).ToArray();

            if (card.portraitTex != null)
            {
                serializeInfo.texture = TextureToPath.GetTexturePath(card.portraitTex.texture) ?? "TODO portrait";
                if (TextureHelper.emissionMap.TryGetValue(card.portraitTex, out Sprite emissionTexture))
                    serializeInfo.emissionTexture = TextureToPath.GetTexturePath(emissionTexture.texture) ?? "TODO portrait emission";
            }

            if (card.alternatePortrait != null)
            {
                serializeInfo.altTexture = TextureToPath.GetTexturePath(card.alternatePortrait.texture) ?? "TODO alt portrait";
                if (TextureHelper.emissionMap.TryGetValue(card.alternatePortrait, out Sprite alternatePortraitEmission))
                    serializeInfo.altEmissionTexture = TextureToPath.GetTexturePath(alternatePortraitEmission.texture) ?? "TODO alt portrait emission";
            }

            if (card.pixelPortrait != null)
            {
                serializeInfo.pixelTexture = TextureToPath.GetTexturePath(card.pixelPortrait.texture) ?? "TODO pixel texture";
            }

            if (card.animatedPortrait != null)
            {
                serializeInfo.animatedPortrait = "TODO animated portrait";
            }

            if (card.holoPortraitPrefab != null)
            {
                serializeInfo.holoPortraitPrefab = "TODO portait prefab";
            }

            if (card.tribes != null && card.tribes.Count > 0)
                serializeInfo.tribes = card.tribes.Select(ParseTribe).ToArray();

            if (card.traits != null && card.traits.Count > 0)
                serializeInfo.traits = card.traits.Select(ParseTrait).ToArray();

            if (card.evolveParams != null)
            {
                serializeInfo.evolveIntoName = card.evolveParams.evolution.name;
                serializeInfo.evolveTurns = card.evolveParams.turnsToEvolve;
            }

            if (!string.IsNullOrEmpty(card.defaultEvolutionName))
                serializeInfo.defaultEvolutionName = card.defaultEvolutionName;

            if (card.tailParams != null)
                serializeInfo.tailName = card.tailParams.tail.name;

            if (card.iceCubeParams != null)
                serializeInfo.iceCubeName = card.iceCubeParams.creatureWithin.name;

            if (card.flipPortraitForStrafe)
                serializeInfo.flipPortraitForStrafe = card.flipPortraitForStrafe;


            Dictionary<string,string> extensionTable = card.GetCardExtensionTable();
            if (extensionTable != null)
            {
                Dictionary<string,string> dictionary = new Dictionary<string, string>(extensionTable);
                dictionary.Remove("JSONFilePath");
                dictionary.Remove("ModPrefix");
                dictionary.Remove("CallStackModGUID");
                if (dictionary.Count > 0)
                {
                    serializeInfo.extensionProperties = dictionary;
                }
            }

            return serializeInfo;
        }

        private static string ParseTribe(Tribe tribe)
        {
            if (tribe >= Tribe.None && tribe <= Tribe.NUM_TRIBES)
            {
                return tribe.ToString();
            }

            Tuple<string, string> guidname = Helpers.GetGUIDAndNameFromEnum(((int)tribe).ToString());
            
            if (guidname != null)
                return guidname.Item1 + "_" + guidname.Item2;
            
            return tribe.ToString();
        }

        private static string ParseTrait(Trait trait)
        {
            if (trait >= Trait.None && trait <= Trait.NUM_TRAITS)
            {
                return trait.ToString();
            }

            Tuple<string, string> guidname = Helpers.GetGUIDAndNameFromEnum(((int)trait).ToString());
            if (guidname != null)
                return guidname.Item1 + "_" + guidname.Item2;
            
            return trait.ToString();
        }

        private static string ParseSpecialStatIcon(SpecialStatIcon cardSpecialStatIcon)
        {
            if (cardSpecialStatIcon >= SpecialStatIcon.None && cardSpecialStatIcon <= SpecialStatIcon.NUM_ICONS)
            {
                return cardSpecialStatIcon.ToString();
            }

            Tuple<string, string> guidname = Helpers.GetGUIDAndNameFromEnum(((int)cardSpecialStatIcon).ToString());
            if (guidname != null)
                return guidname.Item1 + "_" + guidname.Item2;
            
            return cardSpecialStatIcon.ToString();
        }

        private static string ParseSpecialAbilities(SpecialTriggeredAbility specialTriggeredAbility)
        {
            if (specialTriggeredAbility >= SpecialTriggeredAbility.None && specialTriggeredAbility <= SpecialTriggeredAbility.NUM_ABILITIES)
            {
                return specialTriggeredAbility.ToString();
            }

            Tuple<string, string> guidname = Helpers.GetGUIDAndNameFromEnum(((int)specialTriggeredAbility).ToString());
            if (guidname != null)
                return guidname.Item1 + "_" + guidname.Item2;
            
            return specialTriggeredAbility.ToString();
        }

        private static string ParseAbility(Ability ability)
        {
            if (ability >= Ability.None && ability <= Ability.NUM_ABILITIES)
            {
                return ability.ToString();
            }

            Tuple<string, string> guidname = Helpers.GetGUIDAndNameFromEnum(((int)ability).ToString());
            if (guidname != null)
                return guidname.Item1 + "_" + guidname.Item2;
            
            return ability.ToString();
        }
    }
}
